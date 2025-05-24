using System;

namespace VNCLauncher.Models
{
    public class VncConnection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastConnected { get; set; } = DateTime.MinValue;
        public bool IsAvailable { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Varsayılan constructor
        public VncConnection() 
        {
            CreatedDate = DateTime.Now;
        }
        
        // Parametreli constructor
        public VncConnection(string name, string ipAddress)
        {
            Name = name;
            IpAddress = ipAddress;
            CreatedDate = DateTime.Now;
        }
        
        public override string ToString()
        {
            return $"{Name} ({IpAddress})";
        }
    }
} 