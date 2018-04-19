using PolyPaint.Models;
using PolyPaint.Utils;
using System;
using System.Reactive.Linq;

namespace PolyPaint.ViewModels.Social
{
    public interface IStoryViewModel : IViewModel
    {
        DetailedStoryModel Story { get; set; }
        TimeSpan StoryDuration { get; }
        RelayCommand<object> Close { get; set; }
        event Action OnClose;
    }

    public class StoryViewModel : ViewModel, IStoryViewModel
    {
        private int CurrentImageIndex { get; set; }

        private IDisposable ImageCyclingSubscription { get; set; }

        public TimeSpan StoryDuration => TimeSpan.FromSeconds(Story?.DrawingPreviewUrls?.Count * Constants.TimeDurationInSeconds ?? 0);

        private string previousImage;
        public string PreviousImage
        {
            get => previousImage;
            set { previousImage = value; RaisePropertyChanged(); }
        }

        private string currentImage;
        public string CurrentImage
        {
            get => currentImage;
            set { currentImage = value; RaisePropertyChanged(); }
        }

        private DetailedStoryModel storyModel;
        public DetailedStoryModel Story
        {
            get => storyModel;
            set
            {
                storyModel = value;
                StartCyclingImages();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(StoryDuration));
            }
        }

        public event Action OnClose;
        public RelayCommand<object> Close { get; set; }

        public StoryViewModel()
        {
            Close = new RelayCommand<object>((_) => OnClose?.Invoke());
        }

        private void StartCyclingImages()
        {
            CurrentImageIndex = 0;
            CurrentImage = Story?.DrawingPreviewUrls?[CurrentImageIndex];

            ImageCyclingSubscription = Observable.Interval(Constants.ImageDuration).Subscribe((_) =>
            {
                if (++CurrentImageIndex >= Story.DrawingPreviewUrls.Count)
                {
                    ImageCyclingSubscription?.Dispose();
                    return;
                }

                if (CurrentImage != null) { PreviousImage = CurrentImage; }

                CurrentImage = Story.DrawingPreviewUrls[CurrentImageIndex];
            });
        }

        private static class Constants
        {
            public static readonly int TimeDurationInSeconds = 7;
            public static readonly TimeSpan ImageDuration = TimeSpan.FromSeconds(TimeDurationInSeconds);
        }
    }
}
