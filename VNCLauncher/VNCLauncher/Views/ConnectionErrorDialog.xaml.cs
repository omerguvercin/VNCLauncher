using System.Windows;

namespace VNCLauncher.Views
{
    public partial class ConnectionErrorDialog : Window
    {
        public ConnectionErrorDialog(string connectionName, string ipAddress)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            txtErrorMessage.Text = $"{connectionName} ({ipAddress}) adlı bağlantıya şu anda ulaşılamıyor.\nLütfen bağlantının durumunu kontrol edin.";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
} 