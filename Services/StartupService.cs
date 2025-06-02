using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace VNCLauncher.Services
{
    public class StartupService
    {
        private const string AppName = "VNC Launcher";
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        
        // Uygulamayı Windows başlangıcına ekle
        public bool AddToStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                
                if (key == null)
                {
                    return false;
                }
                
                string executablePath = Assembly.GetExecutingAssembly().Location;
                executablePath = executablePath.Replace(".dll", ".exe");
                
                key.SetValue(AppName, executablePath);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Uygulamayı Windows başlangıcından kaldır
        public bool RemoveFromStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                
                if (key == null)
                {
                    return false;
                }
                
                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName);
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Uygulama Windows başlangıcında var mı kontrol et
        public bool IsInStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                
                if (key == null)
                {
                    return false;
                }
                
                return key.GetValue(AppName) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 