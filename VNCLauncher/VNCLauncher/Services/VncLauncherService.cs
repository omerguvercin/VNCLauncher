using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VNCLauncher.Models;
using VNCLauncher.Services; // JsonDataService için
using System.Windows; // MessageBox için

namespace VNCLauncher.Services
{
    public class VncLauncherService
    {
        private readonly JsonDataService _jsonDataService;
        private string _vncPath = string.Empty;
        private int _vncPort = 5900; // Varsayılan port
        
        public VncLauncherService(JsonDataService jsonDataService)
        {
            _jsonDataService = jsonDataService;
            // LoadVncPathFromSettingsAsync().ConfigureAwait(false).GetAwaiter().GetResult(); // Bu satır kaldırılacak
        }
        
        public async Task InitializeAsync()
        {
            await LoadVncPathFromSettingsAsync();
            await Task.CompletedTask;
        }
        
        private async Task LoadVncPathFromSettingsAsync()
        {
            var settings = await _jsonDataService.LoadSettingsAsync();
            _vncPath = settings.VncPath;
            _vncPort = settings.VncPort; // Port numarasını yükle
        }
        
        // TightVNC uygulamasını çalıştır
        public Task<bool> LaunchVncConnectionAsync(VncConnection connection)
        {
            if (connection == null || string.IsNullOrWhiteSpace(connection.IpAddress))
            {
                MessageBox.Show("Bağlantı bilgileri geçersiz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
<<<<<<< HEAD
                return await Task.FromResult(false);
=======
                return Task.FromResult(false);
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
            }

            if (string.IsNullOrEmpty(_vncPath) || !File.Exists(_vncPath))
            {
                var errorDialog = new VNCLauncher.Views.ConnectionErrorDialog("VNC Hatası", "VNC bulunamadı. Lütfen Ayarlardan VNC dosya yolunu doğrulayın.");
                errorDialog.ShowDialog();
<<<<<<< HEAD
                return await Task.FromResult(false);
=======
                return Task.FromResult(false);
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = _vncPath,
                    Arguments = $"{connection.IpAddress}::{_vncPort}", // IP::Port formatını kullan
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
                connection.LastConnected = DateTime.Now; // LastConnection -> LastConnected
<<<<<<< HEAD
                return await Task.FromResult(true);
=======
                // Adres defterini burada doğrudan güncellemek yerine MainWindow sorumluluğunda olmalı.
                // Ancak son bağlantı zamanını modelde güncelledik, bu yeterli olabilir.
                return Task.FromResult(true);
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
            }
            catch (Exception ex)
            {
                MessageBox.Show($"TightVNC başlatılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
<<<<<<< HEAD
                return await Task.FromResult(false);
=======
                return Task.FromResult(false);
>>>>>>> c7478286e1e510039b6d4d27e953454cea9033a8
            }
        }
        
        // TightVNC yolu geçerli mi kontrol et
        public bool IsVncPathValid(string? vncPath)
        {
            _vncPath = vncPath ?? string.Empty;
            return !string.IsNullOrEmpty(_vncPath) && File.Exists(_vncPath);
        }
        
        // TightVNC uygulaması yüklü mü kontrol et
        public bool IsVncInstalled()
        {
            // Program Files içinde arama yap (hem x86 hem de normal)
            string[] programFilesPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var pfPath in programFilesPaths)
            {
                string tightVncPath = Path.Combine(pfPath, "TightVNC", "tvnviewer.exe");
                if (File.Exists(tightVncPath))
                {
                    if (string.IsNullOrEmpty(_vncPath)) // Eğer ayarlarda yol boşsa, bulunan yolu ata
                    {
                        _vncPath = tightVncPath;
                        // Ayarlara da kaydetmek iyi bir fikir olabilir.
                        // Task.Run(async () => await SaveVncPathToSettingsAsync(tightVncPath)); 
                    }
                    return true;
                }
            }
            return false;
        }
        
        // TightVNC indirme sayfasını aç
        public void OpenTightVncDownloadPage()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.tightvnc.com/download.php",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İndirme sayfası açılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}