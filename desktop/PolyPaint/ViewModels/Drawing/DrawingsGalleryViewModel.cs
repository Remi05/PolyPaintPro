using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyPaint.Services;
using PolyPaint.Utils;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IDrawingsGalleryViewModel : IRefreshableViewModel
    {
        event Action<IDrawingViewModel> OnDrawingSelected;
        bool HasDrawings { get; }
        RelayCommand<IDrawingViewModel> SelectDrawingCommand { get; }

        Task SetDrawingsIds(ObservableCollection<string> drawingsIds);
    }

    public class DrawingsGalleryViewModel : ViewModel, IDrawingsGalleryViewModel
    {
        public event Action<IDrawingViewModel> OnDrawingSelected;

        private IViewsManager ViewsManager { get; }

        private object DrawingsViewModelsLock { get; set; } = new object();

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set
            {
                isLoading = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasDrawings));
            }
        }

        public bool HasDrawings => IsLoading || DrawingsViewModels?.Count > 0;

        private ObservableCollection<string> DrawingsIds { get; set; }

        private ObservableCollection<IDrawingViewModel> drawingsViewModels;
        public ObservableCollection<IDrawingViewModel> DrawingsViewModels
        {
            get => drawingsViewModels;
            private set
            {
                drawingsViewModels = value;
                if (drawingsViewModels != null)
                {
                    drawingsViewModels.CollectionChanged += (_, __) => RaisePropertyChanged(nameof(HasDrawings));
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasDrawings));
            }
        }

        public RelayCommand<IDrawingViewModel> SelectDrawingCommand { get; }

        public DrawingsGalleryViewModel(IViewsManager viewsManager)
        {
            ViewsManager = viewsManager;
            DrawingsViewModels = new ObservableCollection<IDrawingViewModel>();
            SelectDrawingCommand = new RelayCommand<IDrawingViewModel>(SelectDrawing);
        }

        public async Task Refresh()
        {
            IsLoading = true;
            await UpdateDrawingsViewModels(DrawingsIds);
            IsLoading = false;
        }

        public async Task SetDrawingsIds(ObservableCollection<string> drawingsIds)
        {
            DrawingsIds = drawingsIds;
            await Refresh();
        }

        private async Task UpdateDrawingsViewModels(IEnumerable drawingsIds)
        {
            if (drawingsIds == null)
                return;

            var tasks = new List<Task>();
            var newDrawingViewModels = new Collection<IDrawingViewModel>();

            foreach (string drawingId in drawingsIds)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var drawingViewModel = ViewsManager.GetViewModel<IDrawingViewModel>();
                    await drawingViewModel.SetDrawing(drawingId);
                    try
                    {
                        newDrawingViewModels.Add(drawingViewModel);
                    }
                    catch (Exception) { }
                }));

                if (tasks.Count % Constants.NumberOfDrawingsBeforeUpdate == 0)
                {
                    await Task.WhenAll(tasks.ToArray());

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        lock (DrawingsViewModelsLock)
                        {
                            DrawingsViewModels.Update(newDrawingViewModels);
                        }
                    });
                    
                    tasks.Clear();
                }
            }

            await Task.WhenAll(tasks.ToArray());

            App.Current.Dispatcher.Invoke(() =>
            {
                lock (DrawingsViewModelsLock)
                {
                    DrawingsViewModels.Update(newDrawingViewModels);
                }
            });
        }

        private async Task CreateDrawingViewModel(string drawingId, Collection<IDrawingViewModel> container)
        {
            var drawingViewModel = ViewsManager.GetViewModel<IDrawingViewModel>();
            await drawingViewModel.SetDrawing(drawingId);
            container.Add(drawingViewModel);
        }

        private void SelectDrawing(IDrawingViewModel drawingViewModel)
        {
            OnDrawingSelected?.Invoke(drawingViewModel);
        }

        private static class Constants
        {
            public static readonly int NumberOfDrawingsBeforeUpdate = 24;
        }
    }
}
