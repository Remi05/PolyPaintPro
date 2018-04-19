using PolyPaint.Extensions;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Cache;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using Unity.Interception.Utilities;

namespace PolyPaint.Services.Drawing
{
    using StrokeList = List<Stroke>;
    using StrokeUndoStack = Stack<StrokeModel>;

    public class Drawing
    {
        private IDatabaseService DatabaseService { get; }
        private IAuthenticationService AuthService { get; }
        private ICacheService Cache { get; }
        private ILogger Logger { get; }
        private ISubscription AttributesSubscription { get; set; }

        public DrawingModel DrawingModel { get; set; } = new DrawingModel();
        public StrokeCollection Strokes { get; set; } = new StrokeCollection();
        public InkCanvasEditingMode? EditingMode { get; set; }

        public string Id { get; }
        private string UserId => AuthService?.CurrentUser?.Id ?? AuthService.OfflineClientId;

        private StrokeUndoStack UndoneStrokes { get; set; } = new StrokeUndoStack();
        private HashSet<string> SelectedStrokesIds { get; set; } = new HashSet<string>();
        private StrokeList SelectedStrokes => new List<Stroke>(StrokeCollection.GetStrokes(SelectedStrokesIds));
        private StrokeList Clipboard { get; set; }

        private bool IsRefreshingStrokes { get; set; } = false;
        private bool IsOffline => AuthService?.CurrentUser == null;

        public event Action<int, int> SizeChanged;
        public event Action<List<Stroke>> SelectionChanged;
        public event Action KickedOut;
        public event Action PasswordRequested;
        private object StrokesLock { get; set; } = new object();

        private SyncedStrokeCollection StrokeCollection { get; set; }

        public Drawing(string id, DrawingModel drawingModel, IAuthenticationService authService, ICacheService cache, IDatabaseService databaseService, ILogger logger)
        {
            if (!(drawingModel?.IsPublic ?? true) && drawingModel?.Owner != authService?.CurrentUser?.Id)
            {
                KickedOut?.Invoke();
                return;
            }

            Id = id;
            Cache = cache;
            Logger = logger;
            DrawingModel = drawingModel;
            AuthService = authService;
            DatabaseService = databaseService;
            StrokeCollection = new SyncedStrokeCollection(Id, AuthService, databaseService, cache, logger);
            StrokeCollection.StrokesUpdated += () =>
            {
                UpdateStrokes();
                HighlightSelectedStrokes();
            };

            ToggleSync();
            AuthService.CurrentUserChanged += (_) => ToggleSync();
            AuthService.BeforeLogout += async (_) => await RemoveSelection();
            AppDomain.CurrentDomain.ProcessExit += (_, __) => OnQuit().Wait();
            (Strokes as INotifyCollectionChanged).CollectionChanged += OnStrokesChanged;
        }

        public async Task Stop()
        {
            if (AttributesSubscription != null)
            {
                AttributesSubscription.Stop();
            }

            StrokeCollection?.StopSync();

            await OnQuit();
        }

        ~Drawing()
        {
            if (AttributesSubscription != null)
            {
                AttributesSubscription.Stop();
            }
        }

        private async Task OnQuit()
        {
            await RemoveSelection();
        }

        private async void ToggleSync()
        {
            if (IsOffline)
            {
                AttributesSubscription?.Stop();
            }
            else
            {
                await DatabaseService.Ref(DatabasePaths.Drawings).Child(Id).Set(DrawingModel);
                AttributesSubscription = DatabaseService.Ref(DatabasePaths.Drawings)
                                                           .Child(Id)
                                                           .OnValue<DrawingModel>(SyncRemoteAttributesToLocal);

                StrokeCollection.StartOnlineSync();
            }
        }

        public void OnStrokesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsRefreshingStrokes)
                return;

            lock (StrokesLock)
            {
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (Stroke s in e.OldItems)
                    {
                        var stroke = s as StrokeModel;
                        if (stroke == null) { continue; }

                        StrokeCollection.UpdateStroke(stroke.Id, null);
                    }
                }

                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach (Stroke s in e.NewItems)
                    {
                        var stroke = s as StrokeModel;
                        if (stroke == null)
                        {
                            stroke = new StrokeModel(s, UserId);
                        }
                        else
                        {
                            stroke.Id = Guid.NewGuid().ToString();
                        }

                        StrokeCollection.UpdateStroke(stroke.Id, stroke);
                    }
                }

                // Erasing the last stroke is considered like a Reset action.
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (StrokeModel stroke in StrokeCollection.GetMergedStrokeMaps())
                    {
                        StrokeCollection.UpdateStroke(stroke.Id, null);
                    }
                }
            }
        }

        public void OnStrokeErased(InkCanvasStrokeErasingEventArgs e)
        {
            var stroke = e.Stroke as StrokeModel;
            e.Cancel = stroke != null && IsSelectedByOtherUser(stroke);
        }

        public void Reset()
        {
            lock (StrokesLock)
            {
                foreach (StrokeModel stroke in StrokeCollection.GetMergedStrokeMaps())
                {
                    StrokeCollection.UpdateStroke(stroke.Id, null);
                }
            }
        }

        public void Undo()
        {
            lock (StrokesLock)
            {
                var currentUserLatestStroke = StrokeCollection.GetMergedStrokeMaps()?.FindLast(x => x.AuthorId == UserId);
                if (currentUserLatestStroke == null)
                    return;

                Strokes.Remove(currentUserLatestStroke);
                StrokeCollection.UpdateStroke(currentUserLatestStroke.Id, null);
                UndoneStrokes.Push(currentUserLatestStroke);
            }
        }

        public void Redo()
        {
            if (UndoneStrokes.Count == 0)
                return;

            lock (StrokesLock)
            {
                var strokeToRedo = UndoneStrokes.Pop();
                Strokes.Add(strokeToRedo);
                StrokeCollection.UpdateStroke(strokeToRedo.Id, strokeToRedo);
            }
        }

        public void Cut()
        {
            if (SelectedStrokesIds.Count == 0)
                return;
            Clipboard = new StrokeList(SelectedStrokes);
            Clipboard.ForEach(x => Strokes.Remove(x));
            SelectedStrokesIds.Clear();
            EditingMode = InkCanvasEditingMode.Select;
        }

        public void Duplicate()
        {
            if (SelectedStrokesIds.Count > 0)
            {
                Clipboard = new StrokeList(SelectedStrokes);
            }

            if (Clipboard == null)
                return;

            SelectionChanged?.Invoke(new StrokeList());

            var copiedStrokes = new StrokeList();
            var translationMatrix = Constants.DuplicationTransform;
            var offset = 0;
            foreach (var s in Clipboard.OrderBy(x => (x as StrokeModel)?.CreatedDate))
            {
                var strokeModel = new StrokeModel(s.Clone(), UserId);
                strokeModel.CreatedDate = strokeModel.CreatedDate.AddMilliseconds(offset++);
                strokeModel.Transform(translationMatrix, false);
                copiedStrokes.Add(strokeModel);
            }

            Strokes.Add(new StrokeCollection(copiedStrokes));
            SelectionChanged?.Invoke(copiedStrokes);
            EditingMode = InkCanvasEditingMode.Select;
        }

        public async void OnSelectionChanged(InkCanvasSelectionChangingEventArgs args)
        {
            if (IsSelectionDifferent(args.GetSelectedStrokes()))
            {
                // We make the previously selected strokes unselected
                var strokesToUpdate = new Dictionary<string, string>();
                DrawingModel?.SelectedStrokes?.Where(x => x.Value == UserId).ForEach(x => strokesToUpdate[x.Key] = null);

                // We make the currently selected strokes selected
                var selectedStrokes = new List<StrokeModel>();
                foreach (var s in args.GetSelectedStrokes())
                {
                    var stroke = s as StrokeModel;
                    if (stroke == null || IsSelectedByOtherUser(stroke)) { continue; }
                    selectedStrokes.Add(stroke as StrokeModel);
                }

                SelectedStrokesIds = new HashSet<string>(selectedStrokes.Select(x => x.Id));
                SelectedStrokesIds.ForEach(x => strokesToUpdate[x] = UserId);

                SelectionChanged?.Invoke(SelectedStrokes);
                args.SetSelectedStrokes(new StrokeCollection(SelectedStrokes));

                if (!IsOffline)
                {
                    await DatabaseService.Ref(DatabasePaths.Drawings)
                                         .Child(Id)
                                         .Child(DatabasePaths.SelectedStrokes)
                                         .Update(strokesToUpdate);
                }
            }
        }

        public void OnSelectionTransformed()
        {
            foreach (string id in SelectedStrokesIds)
            {
                var stroke = StrokeCollection.GetStroke(id);
                if (stroke != null)
                {
                    StrokeCollection.UpdateStroke(id, stroke);
                }
            }
        }

        public async void SaveDimensions()
        {
            if (IsOffline)
            {
                Cache.Write(DatabasePaths.Drawings, Id, DrawingModel);
            }
            else
            {
                await DatabaseService.Ref(DatabasePaths.Drawings).Child(Id).Set(DrawingModel);
            }
        }

        private async void SyncRemoteAttributesToLocal(DrawingModel drawingModel)
        {
            if (drawingModel.IsProtected != DrawingModel.IsProtected
            || drawingModel.IsPublic != DrawingModel.IsPublic)
            {
                if (!IsOffline && drawingModel.Owner != AuthService.CurrentUser.Id)
                {
                    if (!drawingModel.IsPublic)
                    {
                        await RemoveSelection();
                        KickedOut?.Invoke();
                    }
                    else if (drawingModel.IsProtected)
                    {
                        await RemoveSelection();
                        PasswordRequested?.Invoke();
                    }
                }
            }

            DrawingModel = drawingModel;
            SizeChanged?.Invoke(DrawingModel.Width, DrawingModel.Height);
            HighlightSelectedStrokes();
        }

        private void UpdateStrokes()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // Do we need a lock here?
                IsRefreshingStrokes = true;
                var newStrokes = new StrokeCollection(StrokeCollection.GetMergedStrokeMaps());

                Strokes.Clear();
                Strokes.Add(newStrokes);

                if (EditingMode == InkCanvasEditingMode.Select)
                {
                    SelectionChanged?.Invoke(SelectedStrokes);
                }
                IsRefreshingStrokes = false;
            });
        }

        private void HighlightSelectedStrokes()
        {
            if (DrawingModel?.SelectedStrokes == null)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                lock (StrokesLock)
                {
                    foreach (var s in Strokes)
                    {
                        var stroke = s as StrokeModel;
                        if (stroke == null || !DrawingModel.SelectedStrokes.ContainsKey(stroke.Id))
                        {
                            stroke.DrawingAttributes.IsHighlighter = false;
                        }
                        else
                        {
                            stroke.DrawingAttributes.IsHighlighter = IsSelectedByOtherUser(stroke);
                        }
                    }
                }
            });
        }

        private bool IsSelectedByOtherUser(StrokeModel stroke)
        {
            string strokeOwnerId = null;
            DrawingModel?.SelectedStrokes?.TryGetValue(stroke.Id, out strokeOwnerId);

            return strokeOwnerId != null && strokeOwnerId != UserId;
        }

        private bool IsSelectionDifferent(StrokeCollection newSelection)
        {
            if (newSelection.Count != SelectedStrokesIds.Count) return true;

            foreach (var s in newSelection)
            {
                var stroke = s as StrokeModel;
                if (stroke == null || !SelectedStrokesIds.Contains(stroke.Id)) return true;
            }

            return false;
        }

        private async Task RemoveSelection()
        {
            var strokesToUpdate = new Dictionary<string, string>();
            SelectedStrokesIds.ForEach(x => strokesToUpdate.Add(x, null));

            if (!IsOffline)
            {
                await DatabaseService.Ref(DatabasePaths.Drawings)
                                       .Child(Id)
                                       .Child(DatabasePaths.SelectedStrokes)
                                       .Update(strokesToUpdate);
            }
        }

        public async Task UpdateSettings(bool isPublic, bool isProtected, SecureString password)
        {
            DrawingModel.IsPublic = isPublic;
            DrawingModel.IsProtected = isProtected;
            DrawingModel.Password = isProtected ? password.ToUnsecureString() : null;

            await DatabaseService.Ref(DatabasePaths.Drawings).Child(Id).Update(DrawingModel);
        }

        private static class Constants
        {
            public const int DuplicationOffset = 3;
            public const int CanvasMinWidth = 32;
            public const int CanvasMinHeight = 32;
            public static readonly Matrix DuplicationTransform = (new TranslateTransform(DuplicationOffset, DuplicationOffset)).Value;
        }
    }
}
