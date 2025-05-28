using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace VNCLauncher.Views
{
    public partial class DeleteSuccessDialog : Window
    {
        private readonly DispatcherTimer? _closeTimer;
        
        // Varsayılan constructor
        public DeleteSuccessDialog()
        {
            InitializeComponent();
        }
        
        // Bağlantı adını mesaja ekleme
        public DeleteSuccessDialog(string title, string message) : this()
        {
            Title = title;
            MessageTextBlock.Text = message;
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _closeTimer.Tick += CloseTimer_Tick;
            _closeTimer.Start();

            Loaded += (s, e) => 
            {
                var fadeIn = FindResource("FadeIn") as Storyboard;
                fadeIn?.Begin(this);
            };
        }
        
        // Zamanlayıcı olayı
        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            _closeTimer?.Stop();
            var fadeOut = FindResource("FadeOut") as Storyboard;
            if (fadeOut != null)
            {
                fadeOut.Completed += (s, args) => Close();
                fadeOut.Begin(this);
            }
            else
            {
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = FindResource("FadeOut") as Storyboard;
            if (fadeOut != null)
            {
                fadeOut.Completed += (s, args) => Close();
                fadeOut.Begin(this);
            }
            else
            {
                Close();
            }
        }
    }
} 