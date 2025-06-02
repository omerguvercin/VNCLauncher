using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using VNCLauncher.Models; // VncConnection için

namespace VNCLauncher.Helpers
{
    public static class IpAddressHelper
    {
        // Sadece IPv4 adreslerini doğrular (örn: 192.168.1.1)
        public static bool IsValidIpAddress(string? ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString)) return false;

            // IPv4 regex
            var ipv4Regex = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$" );
            return ipv4Regex.IsMatch(ipString);
        }

        // IP adresini normalleştirir (ön sıfırları kaldırır)
        public static string NormalizeIpAddress(string ipString)
        {
            if (!IsValidIpAddress(ipString)) return ipString; // Geçersizse dokunma
            
            return string.Join(".", ipString.Split('.').Select(part => int.Parse(part).ToString()));
        }

        // Verilen IP adresinin listede (hariç tutulan ID dışında) olup olmadığını kontrol eder
        public static bool IsDuplicateIp(string ipAddress, IEnumerable<VncConnection> connections, string? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || connections == null) return false;
            
            string normalizedIp = NormalizeIpAddress(ipAddress);
            return connections.Any(c => c.IpAddress.Equals(normalizedIp, System.StringComparison.OrdinalIgnoreCase) && (excludeId == null || c.Id != excludeId));
        }

        // İki IP adresini karşılaştırır (a < b ise -1, a == b ise 0, a > b ise 1 döndürür)
        public static int CompareIpAddresses(IPAddress? ipA, IPAddress? ipB)
        { 
            if (ipA == null && ipB == null) return 0;
            if (ipA == null) return -1;
            if (ipB == null) return 1;

            byte[] bytesA = ipA.GetAddressBytes();
            byte[] bytesB = ipB.GetAddressBytes();

            // Sadece IPv4 için (4 byte)
            if (bytesA.Length != 4 || bytesB.Length != 4) return 0; // Veya hata fırlat

            for (int i = 0; i < 4; i++)
            {
                if (bytesA[i] < bytesB[i]) return -1;
                if (bytesA[i] > bytesB[i]) return 1;
            }
            return 0;
        }
    }
} 