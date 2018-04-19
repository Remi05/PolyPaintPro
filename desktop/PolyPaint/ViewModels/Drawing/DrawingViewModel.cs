using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Stories;
using PolyPaint.Utils;
using Unity.Attributes;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IDrawingViewModel : IViewModel
    {
        string DrawingId { get; }
        string UserId { get; set; }
        string PreviewUrl { get; }
        string ThumbnailUrl { get; }
        string Owner { get; }
        DateTime LastModifiedOn { get; }
        int NumberOfLikes { get; }
        bool IsLikedByCurrentUser { get; }
        bool IsNsfw { get; }
        bool IsHidden { get; }
        bool IsReportedByCurrentUser { get; }
        bool IsPartOfStory { get; }
        bool IsLoading { get; }
        RelayCommand<object> ToggleIsLikedCommand { get; }
        RelayCommand<object> ToggleIsPartOfStoryCommand { get; }

        Task Refresh();
        Task SetDrawing(string drawingId);
        Task Report(string reason);
        Task UndoReport();

        void Hide();
#if DEBUG
        RelayCommand<object> DeleteCommand { get; }
#endif
    }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class DrawingViewModel : ViewModel, IDrawingViewModel
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        private IAuthenticationService AuthService { get; }
        private IDrawingService DrawingService { get; }
        private IStoriesService StoriesService { get; }

        private string drawingId;
        public string DrawingId
        {
            get => drawingId;
            private set
            {
                drawingId = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPartOfStory));
            }
        }

        private string userId;
        public string UserId
        {
            get => userId;
            set
            {
                userId = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPartOfStory));
            }
        }

        private DrawingInfo drawingInfo;
        private DrawingInfo DrawingInfo
        {
            get => drawingInfo;
            set
            {
                drawingInfo = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PreviewUrl));
                RaisePropertyChanged(nameof(Owner));
                RaisePropertyChanged(nameof(IsLikedByCurrentUser));
                RaisePropertyChanged(nameof(IsNsfw));
                RaisePropertyChanged(nameof(IsReportedByCurrentUser));
                RaisePropertyChanged(nameof(IsPartOfStory));
                RaisePropertyChanged(nameof(LastModifiedOn));
                RaisePropertyChanged(nameof(NumberOfLikes));
            }
        }

        private StoryModel currentUserStory;
        private StoryModel CurrentUserStory
        {
            get => currentUserStory;
            set
            {
                currentUserStory = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPartOfStory));
            }
        }

        public string PreviewUrl => DrawingInfo?.PreviewUrl;
        public string ThumbnailUrl => DrawingInfo?.ThumbnailUrl ?? DrawingInfo?.PreviewUrl;

        public string Owner => DrawingInfo?.Owner;

        public DateTime LastModifiedOn => DrawingInfo?.LastModifiedOn ?? default(DateTime);

        public int NumberOfLikes => DrawingInfo?.Likes?.Count ?? 0;

        public bool IsLikedByCurrentUser => AuthService.CurrentUser != null
                                        && (DrawingInfo?.Likes?.ContainsKey(AuthService.CurrentUser.Id) ?? false);

        public bool IsNsfw => DrawingInfo?.IsNsfw ?? false;

        public bool IsReportedByCurrentUser => AuthService.CurrentUser != null
                                            && (DrawingInfo?.Reports?.ContainsKey(AuthService.CurrentUser.Id) ?? false);

        public bool IsPartOfStory => CurrentUserStory != null
                                 && !CurrentUserStory.IsExpired
                                 && (CurrentUserStory.Drawings?.ContainsKey(DrawingId) ?? false);

        private bool isHidden = false;
        public bool IsHidden
        {
            get => isHidden;
            set { isHidden = value; RaisePropertyChanged(); }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set { isLoading = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> ToggleIsLikedCommand { get; }
        public RelayCommand<object> ToggleIsPartOfStoryCommand { get; set; }

#if DEBUG
        public RelayCommand<object> DeleteCommand { get; }
#endif

        public DrawingViewModel(IAuthenticationService authService, IDrawingService drawingService, IStoriesService storiesService)
        {
            AuthService = authService;
            AuthService.CurrentUserChanged += (user) => OnCurrentUserChanged();
            DrawingService = drawingService;
            StoriesService = storiesService;

            ToggleIsLikedCommand = new RelayCommand<object>(async (_) => await ToggleIsLiked());
            ToggleIsPartOfStoryCommand = new RelayCommand<object>(async (_) => await ToggleIsPartOfStory());

#if DEBUG
            DeleteCommand = new RelayCommand<object>((_) => DrawingService.DeleteDrawing(DrawingId));
#endif
        }

        public async Task Refresh()
        {
            IsLoading = true;
            if (AuthService?.CurrentUser?.Id == UserId)
            {
                CurrentUserStory = null;
            }
            else
            {
                CurrentUserStory = await StoriesService.GetStory(AuthService.CurrentUser.Id);
            }

            DrawingInfo = DrawingId != null ? await DrawingService.GetDrawingInfo(DrawingId) : null;
            IsLoading = false;
        }

        public async Task SetDrawing(string drawingId)
        {
            DrawingId = drawingId;
            await Refresh();
        }

        private void OnCurrentUserChanged()
        {
            RaisePropertyChanged(nameof(IsLikedByCurrentUser));
            RaisePropertyChanged(nameof(IsReportedByCurrentUser));
        }

        public async Task Report(string reason)
        {
            MockReport(reason);
            await DrawingService.Report(DrawingId, reason);
            await Refresh();
        }

        private void MockReport(string reason)
        {
            if (DrawingInfo.Reports == null)
            {
                DrawingInfo.Reports = new Dictionary<string, string>();
            }
            DrawingInfo.Reports.Add(AuthService.CurrentUser?.Id, reason);
            RaisePropertyChanged(nameof(IsReportedByCurrentUser));
        }

        public async Task UndoReport()
        {
            MockUndoReport();
            await DrawingService.UndoReport(DrawingId);
            await Refresh();
        }

        private void MockUndoReport()
        {
            DrawingInfo?.Reports?.Remove(DrawingId);
            RaisePropertyChanged(nameof(IsReportedByCurrentUser));
        }

        private async Task ToggleIsLiked()
        {
            bool isLiked = IsLikedByCurrentUser;
            MockToggleIsLiked();
            await DrawingService.SetIsDrawingLiked(DrawingId, !isLiked);
            await Refresh();
        }

        private void MockToggleIsLiked()
        {
            if (IsLikedByCurrentUser)
            {
                DrawingInfo?.Likes?.Remove(AuthService.CurrentUser?.Id);
            }
            else
            {
                if (DrawingInfo.Likes == null)
                {
                    DrawingInfo.Likes = new Dictionary<string, bool>();
                }
                DrawingInfo.Likes.Add(AuthService.CurrentUser?.Id, true);
            }
            RaisePropertyChanged(nameof(IsLikedByCurrentUser));
            RaisePropertyChanged(nameof(NumberOfLikes));
        }

        private async Task ToggleIsPartOfStory()
        {
            bool isPartOfStory = IsPartOfStory;
            MockToggleIsPartOfStory();
            if (isPartOfStory)
            {
                await StoriesService.RemoveFromStory(DrawingId);
            }
            else
            {
                await StoriesService.AppendToStory(DrawingId);
            }
            await Refresh();
        }

        private void MockToggleIsPartOfStory()
        {
            if (IsPartOfStory)
            {
                CurrentUserStory?.Drawings?.Remove(DrawingId);
            }
            else
            {
                if (currentUserStory == null)
                {
                    CurrentUserStory = new StoryModel();
                }
                if (CurrentUserStory.Drawings == null)
                {
                    CurrentUserStory.Drawings = new Dictionary<string, int>();
                }
                if (CurrentUserStory.IsExpired)
                {
                    CurrentUserStory.Drawings.Clear();
                    CurrentUserStory.ExpirationDate = DateTime.Now + TimeSpan.FromDays(1);
                }
                CurrentUserStory.Drawings.Add(DrawingId, 0);
            }
            RaisePropertyChanged(nameof(IsPartOfStory));
        }

        public void Hide()
        {
            IsHidden = true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DrawingViewModel;
            return other != null
                && (other.DrawingInfo != null
                && other.DrawingInfo.Equals(DrawingInfo)
                || DrawingInfo == null
                && other.DrawingInfo == null);
        }
    }
}
