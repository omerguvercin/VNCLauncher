using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VNCLauncher.Services
{
    public class ConnectionCheckService
    {
        private const int DefaultPingTimeout = 1000; // 1 saniye
        private const int VncDefaultPort = 5900; // VNC için varsayılan port
        private const int TcpConnectionTimeout = 500; // 0.5 saniye
        
        // Ping kontrolü - asenkron
        public async Task<bool> CheckPingAsync(string ipAddress)
        {
            try
            {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(ipAddress, DefaultPingTimeout);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
        
        // TCP port kontrolü - asenkron
        public async Task<bool> CheckTcpPortAsync(string ipAddress, int port = VncDefaultPort)
        {
            try
            {
                using TcpClient client = new();
                var connectTask = client.ConnectAsync(ipAddress, port);
                
                if (await Task.WhenAny(connectTask, Task.Delay(TcpConnectionTimeout)) == connectTask)
                {
                    // Bağlantı başarılı
                    return client.Connected;
                }
                else
                {
                    // Zaman aşımı
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        
        // Birleşik asenkron kontrol - Ping ve TCP port kontrolü dener
        public async Task<bool> IsHostReachableAsync(string ipAddress)
        {
            // Önce ping dene
            bool pingResult = await CheckPingAsync(ipAddress);
            
            if (pingResult)
            {
                return true;
            }
            
            // Ping başarısızsa TCP port kontrolü dene
            return await CheckTcpPortAsync(ipAddress);
        }
    }
} 