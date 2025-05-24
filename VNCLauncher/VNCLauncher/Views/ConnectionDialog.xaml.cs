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

namespace VNCLauncher.Views
{
    public partial class ConnectionDialog : Window
    {
        public VncConnection Connection { get; private set; }
        private readonly List<VncConnection> _existingConnections;
        private readonly bool _isEditMode;
        private readonly DispatcherTimer _closeTimer;
        private string OriginalIpAddress = string.Empty; // Nullable warning için başlangıç değeri atandı
        private bool _isUpdatingIpText = false; // TextChanged reentrancy guard for IP formatting
        
        public ConnectionDialog(List<VncConnection> existingConnections, VncConnection? connectionToEdit = null) // connectionToEdit nullable yapıldı
        {
            InitializeComponent();
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
                NameTextBox.Text = Connection.Name;
                IpAddressTextBox.Text = Connection.IpAddress;
                OriginalIpAddress = Connection.IpAddress;
            }
            
            // IP adresi değiştiğinde kontrolü için event handler'lar XAML'de tanımlı
            // IpAddressTextBox.TextChanged += IpAddressTextBox_TextChanged; // XAML'de yoksa eklenebilir veya XAML'den kaldırılabilir
            // IpAddressTextBox.PreviewTextInput += IpAddressTextBox_PreviewTextInput; // XAML'de tanımlı
            // DataObject.AddPastingHandler(IpAddressTextBox, IpAddressTextBox_Pasting); // XAML'de tanımlı
            
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _closeTimer.Tick += CloseTimer_Tick;
        }
        
        private void CloseTimer_Tick(object? sender, EventArgs e)
        {
            _closeTimer.Stop();
            DialogResult = true;
            // Close(); // DialogResult = true zaten kapatır
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
                // Mevcut segmentin (bu potansiyel noktadan önceki) boş olup olmadığını kontrol et
                string textBeforeCaret = currentText.Substring(0, caretIndex);
                string[] parts = textBeforeCaret.Split('.');
                if (string.IsNullOrEmpty(parts.LastOrDefault())) // Son segment boşsa (örn: "1.2..")
                {
                    e.Handled = true;
                    return;
                }
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

            StringBuilder formattedIp = new StringBuilder();
            int segmentDigitCount = 0;
            int dotCount = 0;

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
                    // Fazla girilen rakamlar (PreviewTextInput engellemeli ama yedek kontrol)
                }
                else if (c == '.')
                {
                    // PreviewTextInput geçerli noktaları kabul etti, burada sadece ekle
                    if (dotCount < 3 && segmentDigitCount > 0) // Nokta bir rakamdan sonra gelmeli
                    {
                        formattedIp.Append('.');
                        dotCount++;
                        segmentDigitCount = 0;
                    }
                    // Geçersiz noktalar (PreviewTextInput engellemeli)
                }
                // Diğer karakterler yok sayılır (PreviewTextInput engellemeli)

                // Segment 3 rakama ulaştıysa ve 3 noktadan az varsa otomatik nokta ekle
                // Ancak bir sonraki karakter zaten kullanıcı tarafından girilmiş bir nokta değilse
                if (segmentDigitCount == 3 && dotCount < 3)
                {
                    bool addDot = true;
                    if (i + 1 < currentText.Length && currentText[i + 1] == '.')
                    {
                        addDot = false; // Kullanıcı zaten nokta girmiş/giriyor
                    }
                    
                    if (addDot)
                    {
                        formattedIp.Append('.');
                        dotCount++;
                        segmentDigitCount = 0; 
                    }
                }
            }
            
            // Sonunda gereksiz nokta varsa kaldır (örn: "123.45." gibi bir durum oluşursa)
            // Genellikle Preview ve yukarıdaki mantık bunu engellemeli.
            string newText = formattedIp.ToString();
            if (newText.EndsWith(".") && newText.Count(x => x == '.') > newText.Count(char.IsDigit) / 3.00 && newText.Count(char.IsDigit) % 3 != 0 && dotCount >=3 ) {
                 // Bu sezgisel bir kontrol, eğer sonda nokta varsa ve IP tamamlanmamış gibi görünüyorsa.
                 // Veya daha basitçe: Eğer son karakter nokta ise ve ondan önceki segment 3 rakam değilse ve 3 nokta zaten varsa.
                 // Şimdilik bu karmaşık temizliği pas geçiyoruz, Preview ve ana döngü doğru çalışmalı.
            }


            textBox.Text = newText;
            
            // İmleç pozisyonunu ayarla (Bu kısım karmaşık olabilir ve iyileştirme gerektirebilir)
            // Basit bir yaklaşım: eğer metin uzadıysa ve imleç sondaysa sonda kalsın.
            // Ya da değişikliği yansıtacak şekilde ayarla.
            try
            {
                 // Önceki imleç pozisyonunu korumaya çalış, nokta ekleme/çıkarma durumlarını hesaba kat
                 int newCaret = caretPosition;
                 if (newText.Length > currentText.Length) // karakter eklendi
                 {
                     // Eğer imlecin hemen soluna bir nokta eklendiyse, imleci bir ileri al
                     if (caretPosition > 0 && caretPosition <= newText.Length && 
                         newText[caretPosition-1] == '.' && 
                         (caretPosition-1 >= currentText.Length || currentText[caretPosition-1] != '.'))
                     {
                         // Otomatik nokta eklendiyse ve imleç o noktanın eklenmesinden önceki yerdeyse
                         // Bu durum zor, çünkü caretPosition orijinal metne göre.
                         // Şimdilik, metin uzadığında imleci orantılı olarak ayarlamaya çalışalım.
                         // Veya en basit haliyle:
                         if (caretPosition > 0 && currentText.Length > 0 && caretPosition <= currentText.Length) // currentText boş değilse ve caretPosition geçerliyse
                         {
                            char charBeforeCaretInOld = currentText[Math.Min(caretPosition, currentText.Length) -1];
                            char charBeforeCaretInNew = newText[Math.Min(caretPosition, newText.Length) -1];

                            if (newText.Length > currentText.Length && newText.EndsWith(".") && currentText.EndsWith(charBeforeCaretInOld.ToString()) && char.IsDigit(charBeforeCaretInOld) && newText.Substring(0, newText.Length-1) == currentText)
                            {
                                // "123" -> "123." olduysa ve imleç 3'ten sonraydı, noktanın sonrasına.
                                 if(caretPosition == currentText.Length) newCaret = newText.Length;
                            }
                         }
                     }
                 }
                 // Genel olarak, imleci orijinal konumuna en yakın mantıklı yere koy.
                 // Eğer bir değişiklik olduysa, caret pozisyonu değişebilir.
                 // Bu örnekte en güvenli yol, basitçe sona ayarlamak olabilir eğer çok karmaşıklaşırsa.
                 // Şimdilik, çok fazla değişiklik yapmadan önceki imleç mantığını koruyalım:
                 
                if (newText.Length > currentText.Length && newText.EndsWith(".")) // Otomatik nokta eklendi ve imleç bu noktanın sonuna gelmeli
                {
                    if (caretPosition == currentText.Length && currentText.Count(char.IsDigit) % 3 == 0 && segmentDigitCount == 0) { // 123 -> 123. ise ve imleç sondaydı
                        textBox.CaretIndex = newText.Length;
                    } else {
                        // Diğer durumlar için daha hassas ayar gerekebilir, şimdilik basit tutalım.
                        // Eğer değişiklik metnin sonundaysa ve imleç oradaysa, imleci yeni sona taşı.
                        if(caretPosition == currentText.Length) textBox.CaretIndex = newText.Length;
                        else textBox.CaretIndex = Math.Min(caretPosition + (newText.Length - currentText.Length), newText.Length);

                    }
                }
                else
                {
                    // İmleci yaklaşık olarak koru
                    textBox.CaretIndex = Math.Min(caretPosition, newText.Length);
                }
            }
            catch {
                textBox.CaretIndex = newText.Length; // Hata olursa sona taşı
            }


            _isUpdatingIpText = false;
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Form doğrulama
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ErrorTextBlock.Text = "Bağlantı adı boş olamaz.";
                IpErrorTextBlock.Text = ""; // Diğer hata mesajını temizle
                return;
            }
            
            if (string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
            {
                ErrorTextBlock.Text = "IP adresi boş olamaz.";
                IpErrorTextBlock.Text = ""; // Diğer hata mesajını temizle
                return;
            }
            
            string currentIp = IpAddressTextBox.Text.Trim();
            if (!IpAddressHelper.IsValidIpAddress(currentIp))
            {
                // ErrorTextBlock.Text = "Geçersiz IPv4 adresi formatı. Örnek: 192.168.1.1"; // Bu satır kaldırıldı
                IpErrorTextBlock.Text = "Geçersiz IPv4 adresi formatı. Örnek: 192.168.1.1";
                ErrorTextBlock.Text = ""; // Genel hata mesajını temizle
                return;
            }
            
            currentIp = IpAddressHelper.NormalizeIpAddress(currentIp);
            
            bool isDuplicate = false;
            string? currentIdToExclude = _isEditMode ? Connection.Id : null;
            if (IpAddressHelper.IsDuplicateIp(currentIp, _existingConnections, currentIdToExclude))
            {
                 if (_isEditMode && OriginalIpAddress.Equals(currentIp, StringComparison.OrdinalIgnoreCase))
                 { // Düzenleme modunda IP değişmediyse çakışma sayılmaz
                    isDuplicate = false;
                 }
                 else
                 {
                    isDuplicate = true;
                 }
            }

            if (isDuplicate)
            {
                // ErrorTextBlock.Text = "Bu IP adresi zaten listede mevcut."; // Bu satır kaldırıldı
                IpErrorTextBlock.Text = "Bu IP adresi zaten listede mevcut.";
                ErrorTextBlock.Text = ""; // Genel hata mesajını temizle
                return;
            }
            
            Connection.Name = NameTextBox.Text.Trim();
            Connection.IpAddress = currentIp;
            
            ErrorTextBlock.Text = "";
            IpErrorTextBlock.Text = "";
            successMessage.Visibility = Visibility.Visible;
            
            // btnSave.IsEnabled = false; // XAML'de btnSave yok, doğrudan Style'daki butonlar var
            // btnCancel.IsEnabled = false; // XAML'de btnCancel yok
            
            _closeTimer.Start();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            // Close(); // DialogResult = false zaten kapatır
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