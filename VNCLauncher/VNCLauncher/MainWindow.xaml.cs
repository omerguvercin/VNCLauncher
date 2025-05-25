using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using VNCLauncher.Helpers;
using VNCLauncher.Models;
using VNCLauncher.Services;
using VNCLauncher.Views;
using System.IO;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Windows.Data;

namespace VNCLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly JsonDataService _jsonDataService;
    private readonly ConnectionCheckService _connectionCheckService;
    private readonly VncLauncherService _vncLauncherService;
    private readonly StartupService _startupService;
    
    private ObservableCollection<VncConnection> _connections;
    private ICollectionView? _connectionsView;
    private ObservableCollection<ScanResult> _scanResults;
    private bool _isClosing = false;

    private bool _isScanning = false;
    private CancellationTokenSource? _scanCancellationTokenSource;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Servisleri başlat
        _jsonDataService = new JsonDataService();
        _connectionCheckService = new ConnectionCheckService();
        _vncLauncherService = new VncLauncherService(_jsonDataService);
        _startupService = new StartupService();
        
        // Bağlantıları yükle
        _connections = new ObservableCollection<VncConnection>();
        dgConnections.ItemsSource = _connections;
        
        // Tarama sonuçları için koleksiyonu başlat
        _scanResults = new ObservableCollection<ScanResult>();
        dgScanResults.ItemsSource = _scanResults;

        // Yeni IP Aralığı Girişi için Handler'ı ekle
        DataObject.AddPastingHandler(txtIpRange, IpRangeTextBox_Pasting);
        
        // Pencere olaylarını ayarla
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        
        // TabControl içindeki sekme değişimini izle
        MainTabControl.SelectionChanged += TabControl_SelectionChanged;

        // LoadConnectionsAsync(); // MainWindow_Loaded içinde çağrılıyor
    }
    
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // VNC Launcher servisini başlat ve ayarlarını yükle
        await _vncLauncherService.InitializeAsync();

        // Ayarları yükle
        await LoadSettingsAsync();
        
        // Bağlantıları yükle
        await LoadConnectionsAsync();
        
        // Bağlantı durumlarını kontrol et
        await CheckConnectionStatusAsync();
        
        // Bağımlılık kontrolü
        CheckDependencies();
    }
    
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_isClosing)
        {
            e.Cancel = true;
            Hide();
        }
    }
    
    #region Bağlantı Yönetimi
    
    private async Task LoadConnectionsAsync()
    {
        try
        {
            var loadedConnections = await _jsonDataService.LoadAddressBookAsync();
            _connections.Clear();
            foreach (var connection in loadedConnections)
            {
                _connections.Add(connection);
            }

            // CollectionViewSource oluştur ve DataGrid'e bağla
            _connectionsView = CollectionViewSource.GetDefaultView(_connections);
            if (_connectionsView != null)
            {
                _connectionsView.Filter = ConnectionsFilter; // Filtreleme metodunu ata
                _connectionsView.SortDescriptions.Clear();
                _connectionsView.SortDescriptions.Add(new SortDescription("IsFavorite", ListSortDirection.Descending));
                _connectionsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
            dgConnections.ItemsSource = _connectionsView; 
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bağlantılar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private bool ConnectionsFilter(object item)
    {
        if (string.IsNullOrEmpty(txtSearchConnections.Text))
            return true; // Arama metni boşsa her şeyi göster

        if (item is VncConnection connection)
        {
            string searchText = txtSearchConnections.Text.ToLower();
            return connection.Name.ToLower().Contains(searchText) || 
                   connection.IpAddress.ToLower().Contains(searchText);
        }
        return false;
    }

    private void TxtSearchConnections_TextChanged(object sender, TextChangedEventArgs e)
    {
        _connectionsView?.Refresh(); // Filtreyi yeniden uygula
    }
    
    private async Task CheckConnectionStatusAsync()
    {
        foreach (var connection in _connections)
        {
            connection.IsAvailable = await _connectionCheckService.IsHostReachableAsync(connection.IpAddress);
        }
    }
    
    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ConnectionDialog(_connections.ToList());
        
        if (dialog.ShowDialog() == true)
        {
            dialog.Connection.IsAvailable = await _connectionCheckService.IsHostReachableAsync(dialog.Connection.IpAddress);
            _connections.Add(dialog.Connection);
            await SaveConnectionsAsync();
            await LoadConnectionsAsync();
        }
    }
    
    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (dgConnections.SelectedItem is VncConnection selectedConnection)
        {
            var connectionToEditInDialog = _connections.FirstOrDefault(c => c.Id == selectedConnection.Id);
            if(connectionToEditInDialog == null) return;

            var connectionCopy = new VncConnection
            {
                Id = connectionToEditInDialog.Id,
                Name = connectionToEditInDialog.Name,
                IpAddress = connectionToEditInDialog.IpAddress,
                LastConnected = connectionToEditInDialog.LastConnected,
                IsAvailable = connectionToEditInDialog.IsAvailable,
                CreatedDate = connectionToEditInDialog.CreatedDate,
                IsFavorite = connectionToEditInDialog.IsFavorite
            };
            var dialog = new ConnectionDialog(_connections.ToList(), connectionCopy);
            if (dialog.ShowDialog() == true)
            {
                connectionToEditInDialog.Name = dialog.Connection.Name;
                connectionToEditInDialog.IpAddress = dialog.Connection.IpAddress;
                connectionToEditInDialog.IsFavorite = dialog.Connection.IsFavorite;
                connectionToEditInDialog.IsAvailable = await _connectionCheckService.IsHostReachableAsync(connectionToEditInDialog.IpAddress);
                await SaveConnectionsAsync();
                _connectionsView?.Refresh();
            }
        }
        else
        {
            MessageBox.Show("Lütfen düzenlemek için bir bağlantı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (dgConnections.SelectedItem is VncConnection selectedConnection)
        {
            var connectionToRemove = _connections.FirstOrDefault(c => c.Id == selectedConnection.Id);
            if (connectionToRemove != null) 
            {
                var confirmDialog = new DeleteConfirmationDialog(connectionToRemove);
                confirmDialog.Owner = this;
                if (confirmDialog.ShowDialog() == true)
                {
                    _connections.Remove(connectionToRemove);
                    await SaveConnectionsAsync();
                    await LoadConnectionsAsync();
                    var successDialog = new DeleteSuccessDialog("Silme Başarılı", $"'{connectionToRemove.Name}' bağlantısı başarıyla silindi.");
                    successDialog.Owner = this;
                    successDialog.ShowDialog();
                }
            }
        }
        else
        {
            MessageBox.Show("Lütfen silinecek bir bağlantı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private async void DgConnections_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (dgConnections.SelectedItem is VncConnection selectedConnection)
        {
            // Bağlantı kurmadan önce erişilebilirliği kontrol et
            if (!selectedConnection.IsAvailable)
            {
                // Gerekirse anlık bir kontrol daha yapılabilir, ancak genellikle periyodik kontrol yeterli olmalı
                // bool stillAvailable = await _connectionCheckService.IsHostReachableAsync(selectedConnection.IpAddress);
                // if (!stillAvailable) 
                // {
                //     selectedConnection.IsAvailable = false; // UI'ı da güncellemek gerekebilir
                //     dgConnections.Items.Refresh(); 
                // }

                var errorDialog = new ConnectionErrorDialog(selectedConnection.Name, selectedConnection.IpAddress);
                errorDialog.ShowDialog();
                return; // Bağlantıyı başlatma
            }

            // TightVNC ile bağlantı kur
            bool result = await _vncLauncherService.LaunchVncConnectionAsync(selectedConnection);
            
            // if (!result)
            // {
            //     MessageBox.Show("TightVNC başlatılırken bir hata oluştu. Lütfen TightVNC yolunu kontrol edin.", 
            //                   "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            // }
            // else
            {
                // Son bağlantı zamanını güncelle (UI)
                selectedConnection.LastConnected = DateTime.Now;
                // Ana koleksiyondaki öğeyi bul ve güncelle, ardından görünümü yenile
                var originalConnection = _connections.FirstOrDefault(c => c.Id == selectedConnection.Id);
                if (originalConnection != null)
                {
                    originalConnection.LastConnected = selectedConnection.LastConnected;
                }
                _connectionsView?.Refresh(); // ICollectionView kullanılıyorsa bu daha doğru
                // dgConnections.Items.Refresh(); // Eğer doğrudan ObservableCollection ise bu da çalışır ama CollectionView varken Refresh() daha iyi.
            }
        }
    }
    
    private async Task SaveConnectionsAsync()
    {
        try
        {
            await _jsonDataService.SaveAddressBookAsync(_connections.ToList());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bağlantılar kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    #endregion
    
    #region Ayarlar
    
    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _jsonDataService.LoadSettingsAsync();
            
            txtVncPath.Text = settings.VncPath;
            txtVncPort.Text = settings.VncPort.ToString();
            chkStartWithWindows.IsChecked = settings.StartWithWindows;
            
            // Windows başlangıç ayarını kontrol et
            if (settings.StartWithWindows != _startupService.IsInStartup())
            {
                if (settings.StartWithWindows)
                {
                    _startupService.AddToStartup();
                }
                else
                {
                    _startupService.RemoveFromStartup();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ayarlar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = new AppSettings
            {
                VncPath = txtVncPath.Text,
                StartWithWindows = chkStartWithWindows.IsChecked ?? false,
                VncPort = int.TryParse(txtVncPort.Text, out int port) ? port : 5900
            };
            
            // Ayarları kaydet
            await _jsonDataService.SaveSettingsAsync(settings);
            
            // Windows başlangıç ayarını güncelle
            if (settings.StartWithWindows)
            {
                _startupService.AddToStartup();
            }
            else
            {
                _startupService.RemoveFromStartup();
            }
            
            // Bağımlılık kontrolü
            CheckDependencies();

            // Ek kontrol: TightVNC yolu geçerli değilse Ayarlar sekmesine yönlendir
            if (!_vncLauncherService.IsVncPathValid(settings.VncPath))
            {
                MainTabControl.SelectedIndex = 2; // Ayarlar sekmesi
            }
            // Başarılı mesajı göster
            else
            {
                var successDialog = new VNCLauncher.Views.DeleteSuccessDialog("Ayarlar Kaydedildi", "Ayarlar başarıyla kaydedildi.");
                successDialog.Owner = this;
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ayarlar kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "TightVNC Viewer Seç",
            Filter = "Executable files (*.exe)|*.exe",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };
        
        if (dialog.ShowDialog() == true)
        {
            txtVncPath.Text = dialog.FileName;
        }
    }
    
    private void TxtVncPort_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        // Sadece rakam girişine izin ver
        e.Handled = !IsTextAllowed(e.Text);
    }

    private static bool IsTextAllowed(string text)
    {
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+"); //sadece rakam
        return !regex.IsMatch(text);
    }
    
    private void CheckDependencies()
    {
        // TightVNC kontrolü
        bool isVncPathValid = _vncLauncherService.IsVncPathValid(txtVncPath.Text);
        bool isVncInstalled = _vncLauncherService.IsVncInstalled();
        
        if (isVncPathValid)
        {
            txtDependencyStatus.Text = "TightVNC Viewer bulundu ve kullanıma hazır.";
            btnInstallVnc.Visibility = Visibility.Collapsed;
        }
        else if (isVncInstalled)
        {
            txtDependencyStatus.Text = "TightVNC Viewer yüklü ancak belirtilen yolda bulunamadı. Lütfen ayarlardan doğru yolu belirtin.";
            btnInstallVnc.Visibility = Visibility.Collapsed;
        }
        else
        {
            txtDependencyStatus.Text = "TightVNC Viewer bulunamadı. Bu uygulama TightVNC Viewer'a bağımlıdır. Lütfen TightVNC'yi yükleyin.";
            btnInstallVnc.Visibility = Visibility.Visible;
        }
        
        // .NET Runtime kontrolü - uygulama çalışıyorsa zaten yüklü demektir
        txtDependencyStatus.Text += "\n\n.NET 8.0 Runtime yüklü ve çalışıyor.";
    }
    
    private void BtnInstallVnc_Click(object sender, RoutedEventArgs e)
    {
        _vncLauncherService.OpenTightVncDownloadPage();
    }
    
    #endregion
    
    #region Tray Icon
    
    private void ShowWindowSimple()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }
    
    private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowWindowSimple();
    }
    
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        Application.Current.Shutdown();
    }
    
    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        Application.Current.Shutdown();
    }
    
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }
    
    #endregion

    private void ExitTab_Selected(object sender, RoutedEventArgs e)
    {
        // Küçük bir gecikme ile çıkış yapmak daha iyi olur
        Task.Delay(100).ContinueWith(_ => 
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                _isClosing = true;
                Application.Current.Shutdown();
            });
        });
    }

    private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Bu event artık doğrudan çıkış yapmayacak, sadece sekme seçimine izin verecek.
        // Çıkış işlemi sadece sekme içindeki buton ile yapılacak.
        if (e.Source is TabControl)
        {
            // Gelecekte sekme değişimleriyle ilgili başka işlemler gerekirse buraya eklenebilir.
        }
    }

    #region Ağ Tarama

    private void ScanIpTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
    }

    private void ScanIpTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!Regex.IsMatch(text, @"^[0-9.]+$"))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void ScanIpTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Anlık IP formatı doğrulaması burada yapılabilir,
            // ancak BtnScanNetwork_Click içinde zaten yapılıyor.
            // Kullanıcıya anlık geri bildirim vermek için eklenebilir.
            // Örneğin:
            // if (!string.IsNullOrWhiteSpace(textBox.Text) && !IpAddressHelper.IsValidIpAddress(textBox.Text))
            // {
            //     txtScanStatus.Text = "Geçersiz IP formatı.";
            //     textBox.Style = FindResource("ErrorTextBox") as Style; // ErrorTextBox stili App.xaml'da olmalı veya burada tanımlanmalı
            // }
            // else
            // {
            //    if(txtScanStatus.Text == "Geçersiz IP formatı.") txtScanStatus.Text = "";
            //    textBox.Style = FindResource("ModernTextBox") as Style;
            // }
        }
    }

    private void IpRangeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Sadece sayılar, nokta ve tire işaretine izin ver
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.-]+$");
    }

    private void IpRangeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!Regex.IsMatch(text, @"^[0-9.-]+$"))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void IpRangeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Kullanıcı yazarken anlık format kontrolü ve geri bildirim için kullanılabilir.
        // Şimdilik BtnScanNetwork_Click içinde ana doğrulama yapılıyor.
        // Örnek: if (!TryParseIpRange(txtIpRange.Text, out _, out _)) { txtScanStatus.Text = "Geçersiz IP aralığı formatı."; }
    }

    private bool TryParseIpRange(string rangeInput, out IPAddress? startIp, out IPAddress? endIp)
    {
        startIp = null;
        endIp = null;
        if (string.IsNullOrWhiteSpace(rangeInput)) return false;

        string[] parts = rangeInput.Split('-');
        if (parts.Length == 1) // Tek IP adresi
        {
            if (IpAddressHelper.IsValidIpAddress(parts[0]))
            {
                startIp = IPAddress.Parse(parts[0]);
                endIp = startIp; // Başlangıç ve bitiş aynı
                return true;
            }
            return false;
        }
        else if (parts.Length == 2)
        {
            string startIpStr = parts[0].Trim();
            string endIpStr = parts[1].Trim();

            if (!IpAddressHelper.IsValidIpAddress(startIpStr)) return false;
            
            startIp = IPAddress.Parse(startIpStr);

            // Bitiş IP'si tam bir IP mi yoksa sadece son oktet mi?
            if (IpAddressHelper.IsValidIpAddress(endIpStr)) // Tam IP: 192.168.1.1-192.168.1.254
            {
                endIp = IPAddress.Parse(endIpStr);
            }
            else if (byte.TryParse(endIpStr, out byte endOctet)) // Sadece son oktet: 192.168.1.1-254
            {
                byte[] startBytes = startIp.GetAddressBytes();
                if (startBytes.Length == 4) // Sadece IPv4 için geçerli
                {
                    endIp = new IPAddress(new byte[] { startBytes[0], startBytes[1], startBytes[2], endOctet });
                }
                else return false; // IPv6 için bu format desteklenmiyor
            }
            else return false; // Geçersiz bitiş formatı
            
            return endIp != null;
        }
        return false; // Geçersiz format (birden fazla tire vb.)
    }

    private async void BtnScanNetwork_Click(object sender, RoutedEventArgs e)
    {
        if (_isScanning)
        {
            _scanCancellationTokenSource?.Cancel();
            // Durdurma işlemleri (isScanning = false, buton metni vs.) try-finally bloğunda yapılacak
            return;
        }

        if (!TryParseIpRange(txtIpRange.Text, out IPAddress? startIpAddr, out IPAddress? endIpAddr) || startIpAddr == null || endIpAddr == null)
        {
            txtScanStatus.Text = "Geçersiz IP aralığı formatı. Örn: 192.168.1.1-254 veya 192.168.1.10";
            txtIpRange.Focus();
            return;
        }

        if (IpAddressHelper.CompareIpAddresses(startIpAddr, endIpAddr) > 0)
        {
            txtScanStatus.Text = "Başlangıç IP adresi, bitiş IP adresinden büyük olamaz.";
            return;
        }

        _scanResults.Clear();
        txtScanStatus.Text = "Tarama başlatılıyor...";
        scanProgressBar.Visibility = Visibility.Visible;
        scanProgressBar.Value = 0;
        btnScanNetwork.Content = "Durdur";
        _isScanning = true;
        _scanCancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _scanCancellationTokenSource.Token;

        List<IPAddress> ipAddressesToScan = GetIpRange(startIpAddr, endIpAddr);
        if (!ipAddressesToScan.Any())
        {
            txtScanStatus.Text = "Belirtilen aralıkta taranacak IP adresi bulunamadı.";
             _isScanning = false;
            btnScanNetwork.Content = "Ağı Tara";
            scanProgressBar.Visibility = Visibility.Collapsed;
            return;
        }
        scanProgressBar.Maximum = ipAddressesToScan.Count;

        int foundCount = 0;
        try
        {
            for (int i = 0; i < ipAddressesToScan.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                IPAddress currentIp = ipAddressesToScan[i];
                txtScanStatus.Text = $"Taranıyor: {currentIp} ({i + 1}/{ipAddressesToScan.Count})";
                scanProgressBar.Value = i + 1;

                bool isReachable = await IsHostReachableAsync(currentIp.ToString(), token);
                if (isReachable)
                {
                    token.ThrowIfCancellationRequested();
                    bool isVncOpen = await IsVncPortOpenAsync(currentIp.ToString(), 5900, 500, token);
                    token.ThrowIfCancellationRequested();
                    string hostname = await GetHostnameAsync(currentIp.ToString(), token);
                    
                    if (isVncOpen)
                    {
                        _scanResults.Add(new ScanResult { IpAddress = currentIp.ToString(), Hostname = hostname, IsVncPortOpen = isVncOpen });
                        foundCount++;
                    }
                }
                 if (i % 10 == 0) 
                    await Task.Delay(1, token);
            }
            txtScanStatus.Text = $"Tarama tamamlandı. {foundCount} cihaz bulundu.";
        }
        catch (OperationCanceledException)
        {
            txtScanStatus.Text = "Tarama kullanıcı tarafından iptal edildi.";
        }
        catch (Exception ex)
        {
            txtScanStatus.Text = $"Tarama sırasında bir hata oluştu: {ex.Message}";
        }
        finally
        {
            _isScanning = false;
            btnScanNetwork.Content = "Ağı Tara";
            scanProgressBar.Visibility = Visibility.Collapsed;
            // Butonun IsEnabled durumu, tarama bitince veya iptal edilince true olmalı, ancak zaten hep true tutuluyor.
            _scanCancellationTokenSource?.Dispose();
            _scanCancellationTokenSource = null;
        }
    }

    private List<IPAddress> GetIpRange(IPAddress startIP, IPAddress endIP)
    {
        var list = new List<IPAddress>();
        uint sIP = BitConverter.ToUInt32(startIP.GetAddressBytes().Reverse().ToArray(), 0);
        uint eIP = BitConverter.ToUInt32(endIP.GetAddressBytes().Reverse().ToArray(), 0);
        // Güvenlik için tarama yapılacak IP sayısını sınırla (örneğin 65536 = 2^16, yani /16'lık bir subnet)
        const uint maxScanCount = 65536; 
        uint currentCount = 0;

        while (sIP <= eIP && currentCount < maxScanCount)
        {
            list.Add(new IPAddress(BitConverter.GetBytes(sIP).Reverse().ToArray()));
            sIP++;
            currentCount++;
        }
        if (currentCount >= maxScanCount && sIP <= eIP)
        {
             Dispatcher.Invoke(() => txtScanStatus.Text = $"Tarama aralığı çok geniş. Maksimum {maxScanCount} IP tarandı.");
        }
        return list;
    }

    private async Task<bool> IsHostReachableAsync(string ipAddress, CancellationToken token, int timeout = 1000)
    {
        if (token.IsCancellationRequested) return false;
        try
        {
            using (var ping = new Ping())
            {
                // Ping.SendPingAsync CancellationToken kabul etmiyor, bu yüzden manuel kontrol
                var pingTask = ping.SendPingAsync(ipAddress, timeout);
                await Task.WhenAny(pingTask, Task.Delay(timeout + 100, token)); // Ping timeout'undan biraz fazla bekle
                token.ThrowIfCancellationRequested();
                return pingTask.IsCompletedSuccessfully && pingTask.Result.Status == IPStatus.Success;
            }
        }
        catch (OperationCanceledException) { return false; } // Token iptali
        catch { return false; } // Diğer ping hataları
    }

    private async Task<bool> IsVncPortOpenAsync(string ipAddress, int port = 5900, int timeout = 500, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return false;
        try
        {
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(ipAddress, port);
                var timeoutTask = Task.Delay(timeout, token);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (token.IsCancellationRequested) return false;

                if (completedTask == connectTask && connectTask.IsCompletedSuccessfully)
                {
                    return true; // Bağlantı başarılı
                }
                return false; // Timeout veya bağlantı hatası
            }
        }
        catch (OperationCanceledException) { return false; } // Token iptali
        catch (SocketException) { return false; } // Port kapalı veya host bulunamadı
        catch { return false; } // Diğer hatalar
    }

    private async Task<string> GetHostnameAsync(string ipAddress, CancellationToken token)
    {
        if (token.IsCancellationRequested) return "N/A";
        try
        {
            // Dns.GetHostEntryAsync CancellationToken kabul etmiyor.
            // Bu nedenle, bu işlemi kısa bir zaman aşımıyla sarmak veya doğrudan çağırmak gerekebilir.
            // Şimdilik doğrudan çağırıyoruz, ancak uzun sürebilir.
            var hostEntryTask = Dns.GetHostEntryAsync(ipAddress);
            await Task.WhenAny(hostEntryTask, Task.Delay(1000, token)); // 1 saniye timeout
            token.ThrowIfCancellationRequested();

            if (hostEntryTask.IsCompletedSuccessfully)
            {
                 return hostEntryTask.Result.HostName;
            }
            return "N/A";
        }
        catch (OperationCanceledException) { return "N/A"; }
        catch (SocketException) { return "N/A"; } // Hostname çözümlenemedi
        catch { return "N/A"; } // Diğer hatalar
    }

    private void HeaderCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        foreach (var item in _scanResults)
        {
            item.IsSelected = true;
        }
        //dgScanResults.Items.Refresh(); // INotifyPropertyChanged ile otomatik güncellenmeli
    }

    private void HeaderCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        foreach (var item in _scanResults)
        {
            item.IsSelected = false;
        }
        //dgScanResults.Items.Refresh(); // INotifyPropertyChanged ile otomatik güncellenmeli
    }

    private async void BtnAddSelectedToAddressBook_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = _scanResults.Where(r => r.IsSelected).ToList();
        if (!selectedItems.Any())
        {
            MessageBox.Show("Lütfen adres defterine eklemek için en az bir cihaz seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int addedCount = 0;
        int duplicateCount = 0;

        foreach (var item in selectedItems)
        {
            if (_connections.Any(c => c.IpAddress.Equals(item.IpAddress, StringComparison.OrdinalIgnoreCase)))
            {
                duplicateCount++;
                continue;
            }

            var newConnection = new VncConnection
            {
                Name = string.IsNullOrWhiteSpace(item.Hostname) || item.Hostname == "N/A" ? item.IpAddress : item.Hostname,
                IpAddress = item.IpAddress,
                CreatedDate = DateTime.Now,
                IsAvailable = await _connectionCheckService.IsHostReachableAsync(item.IpAddress)
            };
            _connections.Add(newConnection);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await SaveConnectionsAsync();
        }
        
        string message = $"{addedCount} cihaz adres defterine eklendi.";
        if (duplicateCount > 0)
        {
            message += $"\n{duplicateCount} cihaz zaten mevcut olduğu için eklenmedi.";
        }
        MessageBox.Show(message, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

        foreach (var item in selectedItems) { item.IsSelected = false; } // Seçimi temizle
        if(headerCheckBox != null) headerCheckBox.IsChecked = false; // Ana checkbox'ı da temizle
        // dgScanResults.Items.Refresh(); // INotifyPropertyChanged ile otomatik güncellenmeli
    }

    #endregion

    private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowWindowSimple();
    }

    private async void QuickConnectMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var quickConnectDialog = new QuickConnectDialog();
        if (quickConnectDialog.ShowDialog() == true)
        {
            string ipAddress = quickConnectDialog.IpAddress;
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var tempConnection = new VncConnection { IpAddress = ipAddress, Name = $"Hızlı Bağlantı ({ipAddress})" };
                await _vncLauncherService.LaunchVncConnectionAsync(tempConnection);
            }
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        Application.Current.Shutdown();
    }

    private async void FavoriteCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is VncConnection connection)
        {
            // IsFavorite özelliği zaten data binding ile güncellenmiş olmalı.
            // Değişiklikleri kaydet
            await SaveConnectionsAsync();
            // Görünümü yenilemek için sıralamayı yeniden uygula
            _connectionsView?.Refresh(); 
        }
    }
}