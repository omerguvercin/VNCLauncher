using System.Windows;
using System.Windows.Threading;
using System;

namespace VNCLauncher.Views
{
    public partial class DeleteSuccessDialog : Window
    {
        private readonly DispatcherTimer _closeTimer;
        
        // Varsayılan constructor
        public DeleteSuccessDialog()
        {
            InitializeComponent();
            
            // Otomatik kapatma zamanlayıcısı
            _closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _closeTimer.Tick += CloseTimer_Tick;
            _closeTimer.Start();
        }
        
        // Bağlantı adını mesaja ekleme
        public DeleteSuccessDialog(string title, string message) : this()
        {
            if (!string.IsNullOrEmpty(title))
                txtTitle.Text = title;
            if (!string.IsNullOrEmpty(message))
                txtMessage.Text = message;
        }
        
        // Tamam butonu tıklandığında
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            _closeTimer.Stop();
            DialogResult = true;
            Close();
        }
        
        // Zamanlayıcı olayı
        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            _closeTimer.Stop();
            DialogResult = true;
            Close();
        }
    }
} 