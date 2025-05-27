using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VNCLauncher.Models
{
    public class ScanResult : INotifyPropertyChanged
    {
        private string _ipAddress = string.Empty;
        private string _hostname = string.Empty;
        private bool _isVncPortOpen;
        private bool _isSelected;

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public string Hostname
        {
            get => _hostname;
            set
            {
                _hostname = value;
                OnPropertyChanged(nameof(Hostname));
            }
        }

        public bool IsVncPortOpen
        {
            get => _isVncPortOpen;
            set
            {
                if (_isVncPortOpen != value)
                {
                    _isVncPortOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
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
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}