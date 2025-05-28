using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace VNCLauncher.Services
{
    public class ConnectionCheckService
    {
        public async Task<bool> IsHostReachableAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
} 