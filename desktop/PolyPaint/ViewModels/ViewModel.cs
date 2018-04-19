using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PolyPaint.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged { }

    public interface IRefreshableViewModel : IViewModel
    {
        Task Refresh();
    }

    public class ViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}