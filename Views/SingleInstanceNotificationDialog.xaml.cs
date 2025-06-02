using System.Windows;

namespace VNCLauncher.Views
{
    /// <summary>
    /// SingleInstanceNotificationDialog.xaml etkileşim mantığı
    /// </summary>
    public partial class SingleInstanceNotificationDialog : Window
    {
        public SingleInstanceNotificationDialog(string message)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            txtNotificationMessage.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            // Close(); // DialogResult = true pencereyi kapatacaktır
        }
    }
} 