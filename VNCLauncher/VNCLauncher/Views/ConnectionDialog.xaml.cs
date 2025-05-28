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
<<<<<<< HEAD
            
            // IP adresi değiştiğinde kontrolü için event handler'lar XAML'de tanımlı
            // IpAddressTextBox.TextChanged += IpAddressTextBox_TextChanged; // XAML'de yoksa eklenebilir veya XAML'den kaldırılabilir
            // IpAddressTextBox.PreviewTextInput += IpAddressTextBox_PreviewTextInput; // XAML'de tanımlı
            // DataObject.AddPastingHandler(IpAddressTextBox, IpAddressTextBox_Pasting); // XAML'de tanımlı
            
=======
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
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
                if (caretIndex == 0 || currentText.Count(c => c == '.') >= 3 || (caretIndex > 0 && currentText[caretIndex - 1] == '.'))
                {
                    e.Handled = true;
                    return;
                }
<<<<<<< HEAD
                string textBeforeCaret = currentText.Substring(0, caretIndex);
                string[] parts = textBeforeCaret.Split('.');
                if (string.IsNullOrEmpty(parts.LastOrDefault()))
=======
                // Nokta girildikten sonra caret bir sağa geçsin
                e.Handled = false;
                Dispatcher.BeginInvoke(new Action(() =>
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
                {
                    textBox.CaretIndex = caretIndex + 1;
                }), System.Windows.Threading.DispatcherPriority.Background);
                return;
            }
            else if (char.IsDigit(newChar[0]))
            {
                string segment = "";
                int lastDotIndex = currentText.LastIndexOf('.', caretIndex - 1);
                segment = currentText.Substring(lastDotIndex + 1, caretIndex - (lastDotIndex + 1));
                segment = new string(segment.Where(char.IsDigit).ToArray());
                if (segment.Length >= 3)
                {
                    e.Handled = true;
                    return;
                }
                if (currentText.Count(char.IsDigit) >= 12)
                {
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Handled = true;
                return;
            }
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
<<<<<<< HEAD
            int newCaretPosition = caretPosition;
=======
            int lastDotCaret = -1;

>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
            for (int i = 0; i < currentText.Length; i++)
            {
                char c = currentText[i];
                if (char.IsDigit(c))
                {
                    if (segmentDigitCount < 3)
                    {
                        formattedIp.Append(c);
                        segmentDigitCount++;
                        if (segmentDigitCount == 3 && dotCount < 3)
                        {
                            formattedIp.Append('.');
                            dotCount++;
                            segmentDigitCount = 0;
                            if (i + 1 == caretPosition)
                            {
                                newCaretPosition = formattedIp.Length;
                            }
                        }
                    }
                }
                else if (c == '.')
                {
                    if (dotCount < 3 && segmentDigitCount > 0)
                    {
                        formattedIp.Append('.');
                        dotCount++;
                        segmentDigitCount = 0;
<<<<<<< HEAD
                        if (i + 1 == caretPosition)
                        {
                            newCaretPosition = formattedIp.Length;
                        }
=======
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
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
                    }
                }
            }
            string newText = formattedIp.ToString();
            textBox.Text = newText;
            try
            {
<<<<<<< HEAD
                textBox.CaretIndex = newCaretPosition;
=======
                // Eğer son eklenen karakter nokta ise caret'i sağa al
                if (e.Changes.Any(c => c.AddedLength == 1 && textBox.Text.Length > 0 && textBox.Text[textBox.CaretIndex - 1] == '.'))
                {
                    textBox.CaretIndex = Math.Min(textBox.CaretIndex + 1, newText.Length);
                }
                else
                {
                    textBox.CaretIndex = Math.Min(caretPosition, newText.Length);
                }
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
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