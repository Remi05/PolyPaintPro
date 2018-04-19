using System;
using System.Security;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using PolyPaint.Extensions;
using PolyPaint.Models;
using PolyPaint.Services.Toasts;
using PolyPaint.ViewInterfaces;
using PolyPaint.Views.Drawing;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IDrawingConfigurationWizardViewModel : IViewModel
    {
        bool IsNewDrawing { get; set; }
        bool IsPublic { get; set; }
        bool IsProtected { get; set; }
        SecureString Password { get; set; }

        RelayCommand<IHasPasswords> SaveCommand { get; }
        RelayCommand CloseCommand { get; }
        RelayCommand MakePublicCommand { get; }
        RelayCommand MakePrivateCommand { get; }

        void SetDrawing(DrawingModel drawing, bool isNewDrawing);
    }

    public class DrawingConfigurationWizardViewModel : ViewModel, IDrawingConfigurationWizardViewModel
    {
        public IToastsService ToastsService { get; }

        private bool isNewDrawing;
        public bool IsNewDrawing
        {
            get => isNewDrawing;
            set { isNewDrawing = value; RaisePropertyChanged(); }
        }

        private bool isPublic;
        public bool IsPublic
        {
            get => isPublic;
            set
            {
                isPublic = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPrivate));
            }
        }

        public bool IsPrivate => !IsPublic;

        private bool isProtected;
        public bool IsProtected
        {
            get => isProtected;
            set { isProtected = value; RaisePropertyChanged(); }
        }

        public SecureString Password { get; set; }

        public RelayCommand<IHasPasswords> SaveCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand MakePublicCommand { get; }
        public RelayCommand MakePrivateCommand { get; }

        public DrawingConfigurationWizardViewModel(IToastsService toastsService)
        {
            ToastsService = toastsService;

            IsPublic = true;

            SaveCommand = new RelayCommand<IHasPasswords>((passwords) => Save(passwords));
            CloseCommand = new RelayCommand(() => Close(false));
            MakePublicCommand = new RelayCommand(() => IsPublic = true);
            MakePrivateCommand = new RelayCommand(() => IsPublic = false);
        }

        public DrawingConfigurationWizard GetCurrentWindow()
        {
            // TODO : User something better or refactor in a util class
            WindowCollection wCollection = Application.Current.Windows;

            foreach (Window win in wCollection)
            {
                if (win is DrawingConfigurationWizard)
                {
                    return win as DrawingConfigurationWizard;
                }
            }

            return null;
        }

        private void Save(IHasPasswords passwords)
        {
            if (!AreFieldsFilled(passwords))
            {
                ToastsService.Pop("Not yet!", "You must enter a password or make your image unprotected.", Constants.YouShallNotSave);
                return;
            }

            if (!ArePasswordsEqual(passwords))
            {
                ToastsService.Pop("Not yet!", "The passwords you entered do not match.", Constants.YouShallNotSave);
                return;
            }

            Password = passwords.Passwords[0];
            Close(true);
        }

        private void Close(bool save)
        {
            var window = GetCurrentWindow();
            window.DialogResult = save;
            window?.Close();
        }

        private bool AreFieldsFilled(IHasPasswords securedPasswords)
        {
            return !IsProtected || !string.IsNullOrWhiteSpace(securedPasswords.Passwords[0].ToUnsecureString());
        }

        private bool ArePasswordsEqual(IHasPasswords securedPasswords)
        {
            var passwordA = securedPasswords.Passwords[0].ToUnsecureString();
            var passwordB = securedPasswords.Passwords[1].ToUnsecureString();
            return passwordA == passwordB;
        }

        public void SetDrawing(DrawingModel drawing, bool isNewDrawing)
        {
            IsPublic = drawing.IsPublic;
            IsProtected = drawing.IsProtected;
            IsNewDrawing = isNewDrawing;
        }

        private static class Constants
        {
            public readonly static string YouShallNotSave = "pack://application:,,,/Resources/youshallnotsave.png";
        }
    }
}
