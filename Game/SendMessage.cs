using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class SendMessage
    {
        public static string GetLocalIP()
        {
            try
            {

                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ip = "";
                        ip = IpEntry.AddressList[i].ToString();
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public void CreateClient(ref IPAddress IP, ref int port, ref Socket SocketClient, ref IPEndPoint ipep)//创建一个用于发送信息的客户端
        {        
            ipep = new IPEndPoint(IP, port);
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
           
        }
        public void SendData(string data, ref Socket SocketClient, ref IPEndPoint ipep)//用于向play发送信息
        {
                       
                byte[] data1 = System.Text.Encoding.UTF8.GetBytes(data);
                SocketClient.SendTo(data1, ipep);
        }
    }
}
