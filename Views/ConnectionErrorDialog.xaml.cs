using System.Windows;

namespace VNCLauncher.Views
{
    public partial class ConnectionErrorDialog : Window
    {
        public ConnectionErrorDialog(string title, string message)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            if (!string.IsNullOrEmpty(message))
                txtErrorMessage.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
} 