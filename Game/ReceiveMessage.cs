using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    public class ReceiveMessage
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
        
        public void StartMonitor()//开始监听
        {
            Start.port = 7000;
            Start.IP = IPAddress.Parse(GetLocalIP());
            Start.ipep = new IPEndPoint(Start.IP, Start.port);
            Start.ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Start.ServerSocket.Bind(Start.ipep);
        }
   
        public void ReceiveData()//用于接收play发来的信息
        {
      
                byte[] datasize = new byte[4]; //用于传输数据长度的信息

                int size = 1024;
                //还剩余多少数据没有传输
                int dataleft = size;
                //传输的数据都存储到data数组中
                byte[] data = new byte[size];

                EndPoint pep = (EndPoint)Start.ipep;
                //IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);

                Start.ServerSocket.ReceiveFrom(data, ref pep);
                string receiveString = Encoding.Default.GetString(data);
                 handleData(receiveString, ref Start.playernumber);
        }

        public void handleData(string receiveString,ref int playernumber)//用于处理数据判断传来的是什么信息，并对对应的user属性赋值
        {           
            receiveString = receiveString.TrimEnd('\0');
            string []newstr=receiveString .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (newstr.Length > 0)
            {
                if (newstr[0] == "IP")
                {                  
                        Start.users[playernumber].IP = IPAddress.Parse(newstr[1]);
                }
                else if (newstr[0] == "port")//检测客户端的端口以后续用client发送牌桌上的牌的信息
                {                  
                        Start.users[playernumber].port = Convert.ToInt32(newstr[1]);
                }
                else if (newstr[0] == "attend")//检测到加入
                {
                    if (newstr[1] == Start.users[playernumber].port.ToString())
                        Start.users[playernumber].isAttend = true;
                }
                else if (newstr[0] == "start")//是否开始游戏
                {
                    foreach (var temp in Start.users)
                    {
                        if (temp.port == Convert.ToInt32(newstr[1]))//检测具体是哪一个玩家
                            temp.isStart = true;
                    }
                }
                else if (newstr[0] == "receive")//是否收到牌
                {
                    foreach (var temp in Start.users)
                    {
                        if (temp.port == Convert.ToInt32(newstr[1]))//检测具体是哪一个玩家
                            temp.isDeal = true;
                    }
                }
                else if (newstr[0] == "put")//是否决定出牌
                {
                    foreach (var temp in Start.users)
                    {
                        if (temp.port == Convert.ToInt32(newstr[1]))
                        {
                            temp.isPut = true;
                            temp.isDeal = false;
                        }
                    }
                }
                else if (newstr[0] == "get")//是否决定收牌
                {
                    foreach (var temp in Start.users)
                    {
                        if (temp.port == Convert.ToInt32(newstr[1]))
                        {
                            temp.isGet = true;
                            temp.isDeal = false;
                        }
                    }
                }
             
                else if (newstr[0] == "over")//玩家是否知道自己已经输了？
                {
                    foreach (var temp in Start.users)
                    {
                        if (temp.port == Convert.ToInt32(newstr[1]))
                        {
                            temp.isOver = true;
                            temp.isDeal = false;
                        }
                    }
                }
            }
      
        }
    }
}
