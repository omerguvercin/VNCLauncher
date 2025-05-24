using System.ComponentModel;

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
                _isVncPortOpen = value;
                OnPropertyChanged(nameof(IsVncPortOpen));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 