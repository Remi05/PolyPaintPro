using GalaSoft.MvvmLight.Command;
using PolyPaint.ViewInterfaces;
using System;
using System.Security;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IProtectedDrawingPasswordPromptViewModel : IViewModel
    {
        event Action<SecureString> PasswordEntered;
        RelayCommand<IHasPassword> ClickCommand { get; }
    }

    public class ProtectedDrawingPasswordPromptViewModel : ViewModel, IProtectedDrawingPasswordPromptViewModel
    {
        public event Action<SecureString> PasswordEntered;
        public RelayCommand<IHasPassword> ClickCommand { get; }

        public ProtectedDrawingPasswordPromptViewModel()
        {
            ClickCommand = new RelayCommand<IHasPassword>(EmitPasswordEntered);
        }

        private void EmitPasswordEntered(IHasPassword password)
        {
            PasswordEntered?.Invoke(password.Password);
        }
    }
}
