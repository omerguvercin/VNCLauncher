using System.ComponentModel;

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
            set { _isVncPortOpen = value; OnPropertyChanged(nameof(IsVncPortOpen)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 