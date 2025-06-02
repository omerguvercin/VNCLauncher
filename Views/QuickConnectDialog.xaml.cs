using System.Windows;
using VNCLauncher.Helpers; // IpAddressHelper için

namespace VNCLauncher.Views
{
    public partial class QuickConnectDialog : Window
    {
        public string IpAddress { get; private set; } = string.Empty;

        public QuickConnectDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            IpAddressTextBox.Focus();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpAddressTextBox.Text.Trim();
            if (IpAddressHelper.IsValidIpAddress(ip)) // IpAddressHelper.cs'deki metodu kullan
            {
                IpAddress = ip;
                DialogResult = true;
            }
            else
            {
                ErrorTextBlock.Text = "Geçerli bir IPv4 adresi girin (örn: 192.168.1.10).";
            }
        }
    }
} 