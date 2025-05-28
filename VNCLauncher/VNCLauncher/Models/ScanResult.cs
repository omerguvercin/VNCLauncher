using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VNCLauncher.Models
{
    public class ScanResult : INotifyPropertyChanged
    {
        private string _ipAddress = string.Empty;
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        private string? _hostname;
        public string? Hostname
        {
            get => _hostname;
            set { _hostname = value; OnPropertyChanged(nameof(Hostname)); }
        }

        private bool _isVncPortOpen;
        public bool IsVncPortOpen
        {
            get => _isVncPortOpen;
<<<<<<< HEAD
            set { _isVncPortOpen = value; OnPropertyChanged(nameof(IsVncPortOpen)); }
=======
            set
            {
                if (_isVncPortOpen != value)
                {
                    _isVncPortOpen = value;
                    OnPropertyChanged();
                }
            }
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
<<<<<<< HEAD
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
=======
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    // Seçim değiştiğinde seçili öğe sayısını güncelle
                    if (Application.Current?.Dispatcher != null)
                    {
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow?.UpdateSelectedCount();
                    }
                }
            }
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
<<<<<<< HEAD
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
=======
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
    }
}