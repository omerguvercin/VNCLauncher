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
using System.Windows.Input;
using System.Windows.Data;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

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
        tabControl.SelectionChanged += TabControl_SelectionChanged;

        // LoadConnectionsAsync(); // MainWindow_Loaded içinde çağrılıyor
    }
    
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeAsync();
        await LoadConnectionsAsync();
        await LoadSettingsAsync();
        CheckDependencies();
    }
    
    private async Task InitializeAsync()
    {
        await _vncLauncherService.InitializeAsync();
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
            var connections = await _jsonDataService.LoadConnectionsAsync();
            _connections.Clear();
            foreach (var connection in connections)
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
        if (item is not VncConnection connection) return false;
        string searchText = txtSearchConnections.Text.ToLower();
        string filter = (cmbConnectionFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tümü";

        bool matchesSearch = string.IsNullOrEmpty(searchText) ||
            connection.Name.ToLower().Contains(searchText) ||
            connection.IpAddress.ToLower().Contains(searchText);

        bool matchesFilter = filter switch
        {
            "Erişilebilenler" => connection.IsAvailable,
            "Erişilemeyenler" => !connection.IsAvailable,
            _ => true
        };

        return matchesSearch && matchesFilter;
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
                var errorDialog = new ConnectionErrorDialog("Bağlantı Hatası", $"{selectedConnection.Name} erişilemiyor!");
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
            await _jsonDataService.SaveConnectionsAsync(_connections.ToList());
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
            // Temel ayarları doğrula
            if (string.IsNullOrWhiteSpace(txtVncPath.Text) || !File.Exists(txtVncPath.Text))
            {
                MessageBox.Show("Lütfen geçerli bir TightVNC yolu seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtVncPort.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Lütfen geçerli bir port numarası girin (1-65535).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var settings = new AppSettings
            {
                VncPath = txtVncPath.Text,
                VncPort = port,
                StartWithWindows = chkStartWithWindows.IsChecked ?? false
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

            // Başarılı mesajı göster
            var successDialog = new VNCLauncher.Views.DeleteSuccessDialog("Ayarlar Kaydedildi", "Ayarlar başarıyla kaydedildi.");
            successDialog.Owner = this;
            successDialog.ShowDialog();
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
        if (!_vncLauncherService.IsVncInstalled())
        {
            txtDependencyStatus.Text = "TightVNC yüklü değil";
            btnInstallVnc.Visibility = Visibility.Visible;
        }
        else
        {
            txtDependencyStatus.Text = "TightVNC yüklü";
            btnInstallVnc.Visibility = Visibility.Collapsed;
        }

        // .NET Runtime kontrolü
        try
        {
            var dotNetVersion = Environment.Version;
            txtDotNetStatus.Text = $".NET Runtime {dotNetVersion.Major}.{dotNetVersion.Minor} yüklü";
        }
        catch
        {
            txtDotNetStatus.Text = ".NET Runtime kontrol edilemedi";
        }
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

    private SemaphoreSlim _scanSemaphore = new SemaphoreSlim(10, 10); // Varsayılan değerlerle başlat
    private int _completedScans = 0;
    private int _foundCount = 0;
    private int _totalIpsToScan = 0;
    private DateTime _scanStartTime;

    private async void BtnScanNetwork_Click(object sender, RoutedEventArgs e)
    {
        if (_isScanning)
        {
            _scanCancellationTokenSource?.Cancel();
            return;
        }

        var settings = await _jsonDataService.LoadSettingsAsync();
        _scanSemaphore = new SemaphoreSlim(settings.MaxConcurrentScans, settings.MaxConcurrentScans);

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
        _completedScans = 0;
        _foundCount = 0;
        _scanStartTime = DateTime.Now;
        
        txtScanStatus.Text = "Tarama başlatılıyor...";
        scanProgressBar.Visibility = Visibility.Visible;
        scanProgressBar.Value = 0;
        btnScanNetwork.Content = "Durdur";
        _isScanning = true;
        _scanCancellationTokenSource = new CancellationTokenSource();
        var token = _scanCancellationTokenSource.Token;

        List<IPAddress> ipAddressesToScan = GetIpRange(startIpAddr, endIpAddr);
        _totalIpsToScan = ipAddressesToScan.Count;
        
        if (_totalIpsToScan == 0)
        {
            txtScanStatus.Text = "Belirtilen aralıkta taranacak IP adresi bulunamadı.";
            _isScanning = false;
            btnScanNetwork.Content = "Ağı Tara";
            scanProgressBar.Visibility = Visibility.Collapsed;
            return;
        }
        
        scanProgressBar.Maximum = _totalIpsToScan;
        var scanTasks = new List<Task>();

        try
        {
            foreach (var ip in ipAddressesToScan)
            {
                token.ThrowIfCancellationRequested();
                await _scanSemaphore.WaitAsync(token);
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ScanSingleIpAsync(ip, token);
                    }
                    finally
                    {
                        _scanSemaphore.Release();
                        int completed = Interlocked.Increment(ref _completedScans);
                        
                        // UI güncellemeleri ana thread'de yapılmalı
                        Dispatcher.Invoke(() =>
                        {
                            scanProgressBar.Value = completed;
                            var elapsed = DateTime.Now - _scanStartTime;
                            var remaining = TimeSpan.FromTicks(
                                elapsed.Ticks * (_totalIpsToScan - completed) / Math.Max(1, completed));
                            
                            int percentage = _totalIpsToScan > 0 ? (completed * 100 / _totalIpsToScan) : 0;
                            string remainingTime = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
                            txtScanStatus.Text = $"Taranıyor: {completed}/{_totalIpsToScan} " +
                                              $"({percentage}%) | " +
                                              $"Bulunan: {_foundCount} | " +
                                              $"Kalan süre: {remainingTime}";
                        });
                    }
                }, token);
                
                scanTasks.Add(task);
                
                // Başlangıçta tüm görevleri hızlıca başlatmak için küçük bir gecikme
                if (scanTasks.Count < settings.MaxConcurrentScans * 2)
                {
                    await Task.Delay(10, token);
                }
            }

            await Task.WhenAll(scanTasks);
            
            if (!token.IsCancellationRequested)
            {
                Dispatcher.Invoke(() =>
                {
                    TimeSpan duration = DateTime.Now - _scanStartTime;
                    txtScanStatus.Text = $"Tarama tamamlandı. {_foundCount} cihaz bulundu. " +
                                      $"Süre: {duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
                });
            }
        }
        catch (OperationCanceledException)
        {
            Dispatcher.Invoke(() => txtScanStatus.Text = "Tarama kullanıcı tarafından iptal edildi.");
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => txtScanStatus.Text = $"Tarama sırasında bir hata oluştu: {ex.Message}");
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

    private async Task<bool> IsHostReachableAsync(string ipAddress, int timeoutMs, CancellationToken token)
    {
        if (token.IsCancellationRequested) return false;
        try
        {
            using (var ping = new Ping())
            {
                var pingTask = ping.SendPingAsync(ipAddress, timeoutMs);
                await Task.WhenAny(pingTask, Task.Delay(timeoutMs + 100, token));
                token.ThrowIfCancellationRequested();
                return pingTask.IsCompletedSuccessfully && pingTask.Result.Status == IPStatus.Success;
            }
        }
        catch (OperationCanceledException) { return false; }
        catch { return false; }
    }

    private async Task<bool> IsVncPortOpenAsync(string ipAddress, int port, int timeoutMs, CancellationToken token)
    {
        if (token.IsCancellationRequested) return false;
        try
        {
            using (var client = new TcpClient())
            using (var cts = new CancellationTokenSource(timeoutMs))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token))
            {
                await client.ConnectAsync(ipAddress, port, linkedCts.Token);
                return client.Connected;
            }
        }
        catch (OperationCanceledException) { return false; }
        catch (SocketException) { return false; }
        catch { return false; }
    }

    private async Task ScanSingleIpAsync(IPAddress ip, CancellationToken token)
    {
        var settings = await _jsonDataService.LoadSettingsAsync();
        string ipString = ip.ToString();
        bool isReachable = settings.SkipPingScan || await IsHostReachableAsync(ipString, settings.PingTimeoutMs, token);
        if (isReachable)
        {
            token.ThrowIfCancellationRequested();
            bool isVncOpen = await IsVncPortOpenAsync(ipString, settings.VncPort, settings.PortCheckTimeoutMs, token);
            if (isVncOpen)
            {
                token.ThrowIfCancellationRequested();
                string hostname = await GetHostnameAsync(ipString, token);
                Dispatcher.Invoke(() =>
                {
                    _scanResults.Add(new ScanResult
                    {
                        IpAddress = ipString,
                        Hostname = hostname,
                        IsVncPortOpen = true
                    });
                });
                Interlocked.Increment(ref _foundCount);
            }
        }
    }

    private async Task<string> GetHostnameAsync(string ipAddress, CancellationToken token)
    {
        if (token.IsCancellationRequested) return "N/A";
        try
        {
            var settings = await _jsonDataService.LoadSettingsAsync();
            var hostEntryTask = Dns.GetHostEntryAsync(ipAddress);
            await Task.WhenAny(hostEntryTask, Task.Delay(settings.PingTimeoutMs, token));
            token.ThrowIfCancellationRequested();

            if (hostEntryTask.IsCompletedSuccessfully)
            {
                return hostEntryTask.Result.HostName;
            }
            return "N/A";
        }
        catch (OperationCanceledException) { return "N/A"; }
        catch (SocketException) { return "N/A"; }
        catch { return "N/A"; }
    }

    private void HeaderCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_scanResults != null)
        {
            foreach (var item in _scanResults)
            {
                item.IsSelected = true;
            }
            UpdateSelectedCount();
        }
    }

    public void UpdateSelectedCount()
    {
        if (txtSelectedCount == null || btnAddSelectedToAddressBook == null) return;
        
        int selectedCount = _scanResults?.Count(r => r.IsSelected) ?? 0;
        txtSelectedCount.Text = $"{selectedCount} öğe seçildi";
        btnAddSelectedToAddressBook.Visibility = selectedCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void HeaderCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_scanResults != null)
        {
            foreach (var item in _scanResults)
            {
                item.IsSelected = false;
            }
            UpdateSelectedCount();
        }
    }

    private async void BtnAddSelectedToAddressBook_Click(object sender, RoutedEventArgs e)
    {
        await AddSelectedToAddressBookAsync();
    }

    public async Task AddSelectedToAddressBookAsync()
    {
        var selectedItems = _scanResults?.Where(r => r.IsSelected).ToList();
        if (selectedItems == null || !selectedItems.Any())
        {
            var warningDialog = new DeleteSuccessDialog("Uyarı", "Lütfen adres defterine eklemek için en az bir cihaz seçin.");
            warningDialog.Owner = this;
            warningDialog.ShowDialog();
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
        var successDialog = new DeleteSuccessDialog("Bilgi", message);
        successDialog.Owner = this;
        successDialog.ShowDialog();

        foreach (var item in selectedItems) { item.IsSelected = false; } // Seçimi temizle
        // dgScanResults.Items.Refresh(); // INotifyPropertyChanged ile otomatik güncellenmeli
    }

    #endregion

    #region ContextMenu Handlers

    public async Task ConnectToVnc(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return;

        try
        {
            await _vncLauncherService.LaunchVncConnectionAsync(new VncConnection
            {
                IpAddress = ipAddress,
                Name = ipAddress
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"VNC bağlantısı başlatılamadı: {ex.Message}", "Hata", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void ConnectToSelectedVnc()
    {
        if (dgScanResults?.SelectedItem is ScanResult selected)
        {
            await ConnectToVnc(selected.IpAddress);
        }
    }

    public void CopySelectedIp()
    {
        if (dgScanResults?.SelectedItem is ScanResult selected)
        {
            Clipboard.SetText(selected.IpAddress);
        }
    }

    public void CopySelectedHostname()
    {
        if (dgScanResults?.SelectedItem is ScanResult selected)
        {
            Clipboard.SetText(selected.Hostname ?? "N/A");
        }
    }

    public void CopyAllSelectedInfo()
    {
        if (dgScanResults?.SelectedItem is ScanResult selected)
        {
            string info = $"IP: {selected.IpAddress}\r\nHostname: {selected.Hostname ?? "N/A"}";
            Clipboard.SetText(info);
        }
    }

    public void ClearSelection()
    {
        if (dgScanResults != null)
        {
            dgScanResults.UnselectAll();
            
            if (_scanResults != null)
            {
                foreach (var item in _scanResults)
                {
                    item.IsSelected = false;
                }
            }
            
            UpdateSelectedCount();
        }
    }

    private void ContextMenuVncConnect_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem)
        {
            var connection = new VncConnection
            {
                Name = string.IsNullOrWhiteSpace(selectedItem.Hostname) || selectedItem.Hostname == "N/A" 
                    ? selectedItem.IpAddress 
                    : selectedItem.Hostname,
                IpAddress = selectedItem.IpAddress
            };
            _ = _vncLauncherService.LaunchVncConnectionAsync(connection);
        }
    }

    private void ContextMenuCopyIp_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem)
        {
            Clipboard.SetText(selectedItem.IpAddress);
        }
    }

    private void ContextMenuCopyHostname_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem && 
            !string.IsNullOrWhiteSpace(selectedItem.Hostname) && 
            selectedItem.Hostname != "N/A")
        {
            Clipboard.SetText(selectedItem.Hostname);
        }
    }

    private void ContextMenuCopyAll_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem)
        {
            string text = $"IP: {selectedItem.IpAddress}\r\n" +
                         $"Hostname: {selectedItem.Hostname}";
            Clipboard.SetText(text);
        }
    }

    private async void ContextMenuAddToAddressBook_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem)
        {
            if (_connections.Any(c => c.IpAddress.Equals(selectedItem.IpAddress, StringComparison.OrdinalIgnoreCase)))
            {
                var warningDialog = new DeleteSuccessDialog("Uyarı", "Bu IP adresi zaten adres defterinizde mevcut.");
                warningDialog.Owner = this;
                warningDialog.ShowDialog();
                return;
            }

            var newConnection = new VncConnection
            {
                Name = string.IsNullOrWhiteSpace(selectedItem.Hostname) || selectedItem.Hostname == "N/A" 
                    ? selectedItem.IpAddress 
                    : selectedItem.Hostname,
                IpAddress = selectedItem.IpAddress,
                CreatedDate = DateTime.Now,
                IsAvailable = await _connectionCheckService.IsHostReachableAsync(selectedItem.IpAddress)
            };

            _connections.Add(newConnection);
            await SaveConnectionsAsync();

            var successDialog = new DeleteSuccessDialog("Başarılı", "Cihaz adres defterinize eklendi.");
            successDialog.Owner = this;
            successDialog.ShowDialog();
        }
    }

    private void ContextMenuClearSelection_Click(object sender, RoutedEventArgs e)
    {
        if (dgScanResults.SelectedItem is ScanResult selectedItem)
        {
            selectedItem.IsSelected = false;
        }
    }

    private void ContextMenuCopy_Click(object sender, RoutedEventArgs e)
    {
        // Ana menü tıklandığında bir şey yapma, alt menüler işleyecek
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu && menu.PlacementTarget is DataGrid dataGrid)
        {
            var selectedItem = dataGrid.SelectedItem as ScanResult;
            bool hasSelection = selectedItem != null;

            foreach (var item in menu.Items.OfType<MenuItem>())
            {
                // Tüm menü öğelerini etkinleştir/devre dışı bırak
                item.IsEnabled = hasSelection;
                
                // Alt menüleri de işle
                foreach (var subItem in item.Items.OfType<MenuItem>())
                {
                    subItem.IsEnabled = hasSelection;
                }
            }
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (dgScanResults.SelectedItem is ScanResult selectedItem && tabControl.SelectedItem == tabScan)
                    {
                        ContextMenuVncConnect_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    ContextMenuClearSelection_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.C:
                    ContextMenuCopyAll_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.D:
                    if (tabControl.SelectedItem == tabScan)
                    {
                        ContextMenuAddToAddressBook_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
            }
        }
        else if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            switch (e.Key)
            {
                case Key.I:
                    ContextMenuCopyIp_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.H:
                    ContextMenuCopyHostname_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.A:
                    ContextMenuCopyAll_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }
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

    private void CmbConnectionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _connectionsView?.Refresh();
    }

    private void TxtIpAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Sadece rakam ve nokta
        e.Handled = !Regex.IsMatch(e.Text, "[0-9]");
    }

    private void TxtIpAddress_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string text = textBox.Text;
            int caret = textBox.CaretIndex;
            string newText = text;
            int dots = text.Count(c => c == '.');
            // Otomatik nokta ekle
            if (text.Length > 0 && text.Length <= 15)
            {
                string[] parts = text.Split('.');
                if (parts.Length < 4)
                {
                    string digits = text.Replace(".", "");
                    if (digits.Length > 3 * (parts.Length - 1))
                    {
                        newText = string.Join(".", Regex.Matches(digits, ".{1,3}").Select(m => m.Value));
                        if (newText.Length > text.Length) caret++;
                    }
                }
            }
            if (newText != text)
            {
                textBox.Text = newText;
                textBox.CaretIndex = caret;
            }
        }
    }
}