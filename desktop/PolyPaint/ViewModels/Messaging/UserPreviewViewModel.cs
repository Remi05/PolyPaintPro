using PolyPaint.Models;
using PolyPaint.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IUserPreviewViewModel : IViewModel
    {
        event Action OnClick;

        ProfileModel Profile { get; set; }
    }

    public class UserPreviewViewModel : ViewModel, IUserPreviewViewModel
    {
        public event Action OnClick;

        private ProfileModel profile;
        public ProfileModel Profile
        {
            get => profile;
            set { profile = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> ClickCommand { get; private set; }

        public UserPreviewViewModel()
        {
            ClickCommand = new RelayCommand<object>((_) => Click());
        }

        private void Click()
        {
            OnClick?.Invoke();
        }
    }
}
