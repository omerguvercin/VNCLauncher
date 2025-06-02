using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace VNCLauncher.Helpers
{
    public static class NotificationHelper
    {
        // Toast bildirimi göster
        public static void ShowToast(TaskbarIcon taskbarIcon, string title, string message, int durationInSeconds = 2)
        {
            if (taskbarIcon == null)
            {
                return;
            }
            
            // Toast bildirimi oluştur
            var toastNotification = new Grid
            {
                Width = 250,
                Height = 70,
                Background = new SolidColorBrush(Colors.WhiteSmoke),
                Margin = new Thickness(10)
            };
            
            // Kenarlık ekle
            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5)
            };
            
            // İçerik paneli
            StackPanel contentPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };
            
            // Başlık
            TextBlock titleText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            // Mesaj
            TextBlock messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            
            // Panele ekle
            contentPanel.Children.Add(titleText);
            contentPanel.Children.Add(messageText);
            
            // Border'a içeriği ekle
            border.Child = contentPanel;
            
            // Grid'e border'ı ekle
            toastNotification.Children.Add(border);
            
            // Toast bildirimi göster
            taskbarIcon.ShowCustomBalloon(toastNotification, System.Windows.Controls.Primitives.PopupAnimation.Fade, durationInSeconds * 1000);
            
            // Timer ile bildirimi kapat
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationInSeconds)
            };
            
            timer.Tick += (s, e) =>
            {
                taskbarIcon.CloseBalloon();
                timer.Stop();
            };
            
            timer.Start();
        }
    }
} 