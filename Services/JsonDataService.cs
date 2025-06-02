using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VNCLauncher.Models;

namespace VNCLauncher.Services
{
    public class JsonDataService
    {
        private const string DataFolderName = "data";
        private const string AddressBookFileName = "VNC_Adresbook.json";
        private const string ConfigFileName = "VNC_Config.json";
        
        private string GetDataFolderPath()
        {
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string dataFolderPath = Path.Combine(appDir, DataFolderName);
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }
            return dataFolderPath;
        }

        public string GetAddressBookFilePath()
        {
            return Path.Combine(GetDataFolderPath(), AddressBookFileName);
        }
        
        public string GetConfigFilePath()
        {
            return Path.Combine(GetDataFolderPath(), ConfigFileName);
        }
        
        // Adres defterini yükle
        public async Task<List<VncConnection>> LoadAddressBookAsync()
        {
            string filePath = GetAddressBookFilePath();
            
            try
            {
                if (!File.Exists(filePath))
                {
                    return new List<VncConnection>();
                }

                string jsonContent = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new List<VncConnection>();
                }
                
                var connections = JsonConvert.DeserializeObject<List<VncConnection>>(jsonContent);
                return connections ?? new List<VncConnection>();
            }
            catch (Exception ex)
            {
                // Hata durumunda dosyayı yedekleme
                await BackupAndRecreateFileAsync(filePath);
                throw new Exception($"Adres defteri yüklenirken hata oluştu: {ex.Message}");
            }
        }
        
        // Adres defterini kaydet
        public async Task SaveAddressBookAsync(List<VncConnection> connections)
        {
            string filePath = GetAddressBookFilePath();
            
            try
            {
                string jsonContent = JsonConvert.SerializeObject(connections, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Adres defteri kaydedilirken hata oluştu: {ex.Message}");
            }
        }
        
        // Ayarları yükle
        public async Task<AppSettings> LoadSettingsAsync()
        {
            string filePath = GetConfigFilePath();
            
            try
            {
                if (!File.Exists(filePath))
                {
                    return new AppSettings();
                }

                string jsonContent = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new AppSettings();
                }
                
                var settings = JsonConvert.DeserializeObject<AppSettings>(jsonContent);
                return settings ?? new AppSettings();
            }
            catch (Exception)
            {
                // Hata durumunda dosyayı yedekleme
                await BackupAndRecreateFileAsync(filePath);
                return new AppSettings();
            }
        }
        
        // Ayarları kaydet
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            string filePath = GetConfigFilePath();
            
            try
            {
                string jsonContent = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ayarlar kaydedilirken hata oluştu: {ex.Message}");
            }
        }
        
        // Hata durumunda dosyayı yedekle ve yeniden oluştur
        private async Task BackupAndRecreateFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string backupPath = $"{filePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
                    File.Copy(filePath, backupPath);
                    File.Delete(filePath);
                }
                
                // Dosya türüne göre yeni dosya oluştur
                if (filePath.EndsWith(AddressBookFileName))
                {
                    await SaveAddressBookAsync(new List<VncConnection>());
                }
                else if (filePath.EndsWith(ConfigFileName))
                {
                    await SaveSettingsAsync(new AppSettings());
                }
            }
            catch
            {
                // Yedekleme sırasındaki hatayı yutuyoruz
            }
        }

        public async Task<List<VncConnection>> LoadConnectionsAsync()
        {
            try
            {
                if (File.Exists(GetAddressBookFilePath()))
                {
                    string json = await File.ReadAllTextAsync(GetAddressBookFilePath());
                    var connections = JsonConvert.DeserializeObject<List<VncConnection>>(json) ?? new List<VncConnection>();

                    // Yüklenen bağlantıların isimlerini kontrol et
                    foreach (var connection in connections)
                    {
                        if (string.IsNullOrWhiteSpace(connection.Name))
                        {
                            // Eğer isim boşsa, yerine IP adresini kullan
                            connection.Name = connection.IpAddress;
                        }
                    }

                    return connections;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantılar yüklenirken hata: {ex.Message}");
            }
            return new List<VncConnection>();
        }

        public async Task SaveConnectionsAsync(List<VncConnection> connections)
        {
            try
            {
                string json = JsonConvert.SerializeObject(connections, Formatting.Indented);
                await File.WriteAllTextAsync(GetAddressBookFilePath(), json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantılar kaydedilirken hata: {ex.Message}");
            }
        }
    }
} 