using System.Net;
using System.Net.Sockets;

namespace EventCore.Common.System
{
    public class Network
    {
        public static IPAddress GetPrimaryAddress()
        {
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address;
                }
            }

            return null;
        }
    }
}