using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VNCLauncher.Helpers;
using VNCLauncher.Models;
using System.Text;
using System.Linq;

namespace VNCLauncher.Views
{
    public partial class ConnectionDialog : Window
    {
        public VncConnection Connection { get; private set; }
        private readonly List<VncConnection> _existingConnections;
        private readonly bool _isEditMode;
        private readonly DispatcherTimer _closeTimer;
        private string OriginalIpAddress = string.Empty;
        private bool _isUpdatingIpText = false;
        
        public ConnectionDialog(List<VncConnection> existingConnections, VncConnection? connectionToEdit = null)
        {
            Owner = Application.Current.MainWindow;
            _existingConnections = existingConnections;
            if (connectionToEdit == null)
            {
                Connection = new VncConnection();
                _isEditMode = false;
                Title = "Yeni Bağlantı";
            }
            else
            {
                Connection = connectionToEdit;
                _isEditMode = true;
                Title = "Bağlantıyı Düzenle";
            }
            DataContext = Connection;
            if (_isEditMode && connectionToEdit != null)
            {
                OriginalIpAddress = connectionToEdit.IpAddress;
            }
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _closeTimer.Tick += CloseTimer_Tick;
        }
        
        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            _closeTimer.Stop();
            DialogResult = true;
        }
        
        private void IpAddressTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                // Allow only digits when pasting. Formatting will be handled by TextChanged.
                if (!Regex.IsMatch(text, @"^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        
        private void IpAddressTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string currentText = textBox.Text;
            int caretIndex = textBox.CaretIndex;
            string newChar = e.Text;

            if (newChar == ".")
            {
                if (caretIndex == 0 || // Nokta başta olamaz
                    currentText.Count(c => c == '.') >= 3 || // Zaten 3 nokta var
                    (caretIndex > 0 && currentText[caretIndex - 1] == '.')) // Bir önceki karakter zaten nokta
                {
                    e.Handled = true;
                    return;
                }
                // Nokta girildikten sonra caret bir sağa geçsin
                e.Handled = false;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.CaretIndex = caretIndex + 1;
                }), System.Windows.Threading.DispatcherPriority.Background);
                return;
            }
            else if (char.IsDigit(newChar[0]))
            {
                // Rakam giriliyorsa, mevcut segmentin uzunluğunu kontrol et
                string segment = "";
                int lastDotIndex = currentText.LastIndexOf('.', caretIndex - 1);
                segment = currentText.Substring(lastDotIndex + 1, caretIndex - (lastDotIndex + 1));
                segment = new string(segment.Where(char.IsDigit).ToArray()); // Sadece segmentteki rakamlar

                if (segment.Length >= 3)
                {
                    e.Handled = true; // Segment zaten 3 rakam içeriyor
                    return;
                }
                // Toplam rakam sayısı kontrolü
                if (currentText.Count(char.IsDigit) >= 12)
                {
                    e.Handled = true;
                    return;
                }
            }
            else // Rakam veya nokta değilse
            {
                e.Handled = true;
                return;
            }

            // Genel uzunluk kontrolü (örn: xxx.xxx.xxx.xxx -> 15 karakter)
            // Önizleme metninin uzunluğunu kontrol et
            string prospectiveText = currentText.Insert(caretIndex, newChar);
            if (prospectiveText.Length > 15)
            {
                e.Handled = true;
            }
        }
        
        private void IpAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingIpText)
                return;

            _isUpdatingIpText = true;
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                _isUpdatingIpText = false;
                return;
            }

            string currentText = textBox.Text;
            int caretPosition = textBox.CaretIndex;

            // Segmentlerdeki baştaki sıfırları sil
            var segments = currentText.Split('.');
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Length > 1 && segments[i].All(char.IsDigit))
                {
                    segments[i] = int.Parse(segments[i]).ToString();
                }
            }
            currentText = string.Join(".", segments);

            StringBuilder formattedIp = new StringBuilder();
            int segmentDigitCount = 0;
            int dotCount = 0;
            int lastDotCaret = -1;

            for (int i = 0; i < currentText.Length; i++)
            {
                char c = currentText[i];

                if (char.IsDigit(c))
                {
                    if (segmentDigitCount < 3)
                    {
                        formattedIp.Append(c);
                        segmentDigitCount++;
                    }
                }
                else if (c == '.')
                {
                    if (dotCount < 3 && segmentDigitCount > 0)
                    {
                        formattedIp.Append('.');
                        dotCount++;
                        segmentDigitCount = 0;
                        lastDotCaret = formattedIp.Length; // caret için
                    }
                }
                if (segmentDigitCount == 3 && dotCount < 3)
                {
                    bool addDot = true;
                    if (i + 1 < currentText.Length && currentText[i + 1] == '.')
                        addDot = false;
                    if (addDot)
                    {
                        formattedIp.Append('.');
                        dotCount++;
                        segmentDigitCount = 0;
                        lastDotCaret = formattedIp.Length; // caret için
                    }
                }
            }
            string newText = formattedIp.ToString();
            if (newText.EndsWith(".") && newText.Count(x => x == '.') > newText.Count(char.IsDigit) / 3.00 && newText.Count(char.IsDigit) % 3 != 0 && dotCount >= 3)
            {
                // Sonda gereksiz nokta varsa kaldır
            }
            textBox.Text = newText;
            try
            {
                // Eğer son eklenen karakter nokta ise caret'i sağa al
                if (e.Changes.Any(c => c.AddedLength == 1 && textBox.Text.Length > 0 && textBox.Text[textBox.CaretIndex - 1] == '.'))
                {
                    textBox.CaretIndex = Math.Min(textBox.CaretIndex + 1, newText.Length);
                }
                else
                {
                    textBox.CaretIndex = Math.Min(caretPosition, newText.Length);
                }
            }
            catch { textBox.CaretIndex = newText.Length; }
            _isUpdatingIpText = false;
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // İsim ve IP boş olamaz kontrolü
            if (string.IsNullOrWhiteSpace(Connection.Name) || string.IsNullOrWhiteSpace(Connection.IpAddress))
            {
                // Hata mesajı gösterilemiyor, sadece return
                return;
            }
            string currentIp = Connection.IpAddress.Trim();
            if (!IpAddressHelper.IsValidIpAddress(currentIp))
            {
                // Hata mesajı gösterilemiyor, sadece return
                return;
            }
            // IP Çakışma Kontrolü
            string? idToExclude = _isEditMode ? Connection.Id : null;
            bool isDuplicate = _existingConnections.Any(c => c.IpAddress.Equals(currentIp, StringComparison.OrdinalIgnoreCase) && c.Id != idToExclude);
            if (isDuplicate)
            {
                // Hata mesajı gösterilemiyor, sadece return
                return;
            }
            Connection.Name = Connection.Name.Trim(); // Zaten DataContext ile bağlı ama trim için kalsın.
            Connection.IpAddress = currentIp; // Zaten DataContext ile bağlı ama trim için kalsın.
            // Connection.IsFavorite zaten güncelleniyor
            // Hata/success mesajı gösterilemiyor
            _closeTimer.Start();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {   
            if (e.Key == Key.Enter)
            {
                // IsDefault=true olan buton (Kaydet) zaten Enter ile tetiklenir.
                // Explicit bir şey yapmaya gerek yok, ancak focus durumuna göre işlem yapılmak istenirse:
                // if (FocusManager.GetFocusedElement(this) is Button focusedButton && focusedButton.IsDefault)
                // {
                //    SaveButton_Click(sender, e); 
                // }
            }
        }
    }
}