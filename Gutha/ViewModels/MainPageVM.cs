using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gutha.ViewModels
{
    public class MainPageVM : INotifyPropertyChanged
    {
        private int _count;

        public MainPageVM()
        {
            _count = 0;
        }

        public int Count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
