using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyPaint.Services.Auth;
using PolyPaint.Utils;
using PolyPaint.ViewModels.Social;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IPostViewModel : IRefreshableViewModel
    {
        IProfileViewModel AuthorProfileViewModel { get; set; }
        IDrawingViewModel DrawingViewModel { get; set; }
        bool IsCurrentLoggedInUser { get; }
        bool IsReporting { get; }
        bool IsReasonSelected { get; }
        bool IsLoading { get; }

        RelayCommand<object> ContinueReportingCommand { get; }
        RelayCommand<object> ToggleIsReportedCommand { get; }
        RelayCommand<string> UpdateConfirmButtonCommand { get; }
    }

    public class PostViewModel : ViewModel, IPostViewModel
    {
        public static string[] Reasons => new[] {
            "It is racist",
            "It is offensive and not marked NSFW",
            "Not approved by the Supreme Leader."
        };

        private IAuthenticationService AuthService { get; }

        private IProfileViewModel authorProfileViewModel;
        public IProfileViewModel AuthorProfileViewModel
        {
            get => authorProfileViewModel;
            set
            {
                authorProfileViewModel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCurrentLoggedInUser));
            }
        }

        private IDrawingViewModel drawingViewModel;
        public IDrawingViewModel DrawingViewModel
        {
            get => drawingViewModel;
            set { drawingViewModel = value; RaisePropertyChanged(); }
        }

        public bool IsCurrentLoggedInUser => AuthorProfileViewModel != null
                                          && AuthService.CurrentUser != null
                                          && AuthorProfileViewModel.UserId == AuthService.CurrentUser.Id;

        private bool isReporting;
        public bool IsReporting
        {
            get => isReporting;
            private set { isReporting = value; RaisePropertyChanged(); }
        }

        public bool IsReasonSelected => RadioButtonValues.Contains(true);

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set { isLoading = value; RaisePropertyChanged(); }
        }

        private List<bool> radioButtonValues;
        private List<bool> RadioButtonValues
        {
            get => radioButtonValues;
            set
            {
                radioButtonValues = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsReasonSelected));
            }
        }

        public RelayCommand<object> ContinueReportingCommand { get; }
        public RelayCommand<object> ToggleIsReportedCommand { get; }
        public RelayCommand<string> UpdateConfirmButtonCommand { get; }

        public PostViewModel(IAuthenticationService authService)
        {
            AuthService = authService;
            AuthService.CurrentUserChanged += (_) => RaisePropertyChanged(nameof(IsCurrentLoggedInUser));

            RadioButtonValues = new List<bool>(new bool[] { false, false, false });

            ContinueReportingCommand = new RelayCommand<object>(async (_) => await ReportDrawing());
            ToggleIsReportedCommand = new RelayCommand<object>(async (_) => await ToggleIsReported());
            UpdateConfirmButtonCommand = new RelayCommand<string>(UpdateConfirmButton);
        }

        public async Task Refresh()
        {
            IsLoading = true;
            await AuthorProfileViewModel?.Refresh();
            DrawingViewModel.UserId = AuthorProfileViewModel.UserId;
            await DrawingViewModel?.Refresh();
            IsLoading = false;
        }

        private async Task ToggleIsReported()
        {
            if (DrawingViewModel.IsReportedByCurrentUser)
            {
                await DrawingViewModel.UndoReport();
            }
            else
            {
                IsReporting = !IsReporting;
            }
        }

        private async Task ReportDrawing()
        {
            string reason = Reasons[RadioButtonValues.FindIndex((val) => { return val; })];
            await DrawingViewModel.Report(reason);
            IsReporting = false;
        }

        private void UpdateConfirmButton(string indexString)
        {
            int index = int.Parse(indexString);
            for (int i = 0; i < RadioButtonValues.Count; ++i)
            {
                RadioButtonValues[i] = i == index;
            }
            RaisePropertyChanged(nameof(RadioButtonValues));
            RaisePropertyChanged(nameof(IsReasonSelected));
        }

        
    }

    public class PostComparer : IComparer<IPostViewModel>
    {
        public int Compare(IPostViewModel post1, IPostViewModel post2)
        {
            int followedCompare = post1.AuthorProfileViewModel.IsFollowedByCurrentUser.CompareTo(post2.AuthorProfileViewModel.IsFollowedByCurrentUser);
            int lastModifiedCompare = post1.DrawingViewModel.LastModifiedOn.CompareTo(post2.DrawingViewModel.LastModifiedOn);
            return followedCompare != 0 ? followedCompare : lastModifiedCompare;
        }
    }
}
