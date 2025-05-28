using System;

namespace VNCLauncher.Models
{
    public class AppSettings
    {
        // TightVNC yolu
        public string VncPath { get; set; } = @"C:\Program Files\TightVNC\tvnviewer.exe";
        
        // Windows başlangıcında çalıştırma ayarı
        public bool StartWithWindows { get; set; } = false;

        // VNC bağlantı portu
        public int VncPort { get; set; } = 5900; // Varsayılan port 5900

        public int MaxConcurrentScans { get; set; } = 10;
        public int PingTimeoutMs { get; set; } = 1000;
        public int PortCheckTimeoutMs { get; set; } = 1000;
        public bool SkipPingScan { get; set; } = false;
    }
} 