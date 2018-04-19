using PolyPaint.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Unity;

namespace PolyPaint.Services
{
    public interface IViewsManager
    {
        T Get<T>() where T : Window;
        T GetUserControl<T>() where T : UserControl;
        T GetViewModel<T>() where T : IViewModel;
        Window Show<T>() where T : Window;
        bool ShowDialog<T>() where T : Window;
    }

    public class ViewsManager : IViewsManager
    {
        public ViewsManager(IUnityContainer container)
        {
            Container = container;
        }

        private IUnityContainer Container { get; }

        public T Get<T>() where T : Window
        {
            return Container.Resolve<T>();
        }

        public T GetUserControl<T>() where T : UserControl
        {
            return Container.Resolve<T>();
        }

        public T GetViewModel<T>() where T : IViewModel
        {
            return Container.Resolve<T>();
        }

        public Window Show<T>() where T : Window
        {
            var window = Get<T>();
            window?.Show();
            return window;
        }

        public bool ShowDialog<T>() where T : Window
        {
            return Get<T>()?.ShowDialog() ?? false;
        }
    }
}