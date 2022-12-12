using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace player
{
    public partial class Form1 : Form
    {
        IPAddress IP;
        IPAddress playerIP;
        string dataSend = "";
        bool isFirstPrint = true;
        Socket SocketClient;//用于向Game中的接受端发送信息
        Socket ServerSocket;//用于接收Game中的信息
        IPEndPoint ipep;
        IPEndPoint playeripep;       
        Thread UDPSend;//用于发送加入信息、开始信息、每次发送出牌信息
        Thread UDPReceive;//用于接受服务端发送牌桌中牌的信息，以及服务端接受的其他玩家客户端的信息
        int port;
        int playerport;
        int playerMark;//玩家号码
        int pokernumber;//牌的数目
        bool isFirstClick;//是否第一次点击加入游戏
        bool isReceive = false;//是否接收到了服务器的信息
        bool isClickStart=false;//是否点击游戏开始
        bool isPutPocker = false;//是否选择出牌
        bool isReceivePocker = false;//是否选择收牌
        bool isCanPut = false;//是否能够出牌
        int isCanReceive = 0;
        bool isOver=false;
        List<int> userpoker = new List<int>();
        List<int> usercount = new List<int>();
        int record_pokercount = 0;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        public void StartMonitor()//开始监听
        {
            playerport = GetRandomPort();
            playerIP = IPAddress.Parse(GetLocalIP());
            playeripep = new IPEndPoint(playerIP, playerport);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ServerSocket.Bind(playeripep);
            textBox2.Text = playerport.ToString();

        }
        public void StartReceive()
        {
            UDPReceive = new Thread(new ThreadStart(GetData));
            UDPReceive.Start();
        }
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
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = GetLocalIP();
            port = 7000;
            textBox2.Text = Convert.ToString(port);
            isFirstClick = true;
        }

        /// <summary>        

        /// 获取操作系统已用的端口号        

        /// </summary>        

        /// <returns></returns>        

        public static IList PortIsUsed()

        {

            //获取本地计算机的网络连接和通信统计数据的信息            

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序            

            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序            

            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。            

            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();

            foreach (IPEndPoint ep in ipsTCP)

            {

                allPorts.Add(ep.Port);

            }

            foreach (IPEndPoint ep in ipsUDP)

            {

                allPorts.Add(ep.Port);

            }

            foreach (TcpConnectionInformation conn in tcpConnInfoArray)

            {

                allPorts.Add(conn.LocalEndPoint.Port);

            }

            return allPorts;

        }

        private int GetRandomPort()
        {
            IList HasUsedPort = PortIsUsed();
            int port = 0;
            bool IsRandomOk = true;
            Random random = new Random((int)DateTime.Now.Ticks);
            while (IsRandomOk)
            {
                port = random.Next(1024, 65535);
                IsRandomOk = HasUsedPort.Contains(port);
            }
            return port;
        }

        public void SendData()
        {
            int num = 0;//用于记录发送次数
            byte[] data1;
            while (true)
            {
                num++;
                dataSend = "";
                if (!isClickStart)
                {
                    if (num % 4 == 1)
                        dataSend = "IP:" + textBox1.Text;
                    else if (num % 4 == 2)
                        dataSend = "port:" + playerport;
                    else if (num % 4 == 3)
                        dataSend = "attend:" + playerport.ToString();
                }
                else
                {
                    if(isClickStart)
                    dataSend = "start:" + playerport.ToString();
                    if(isReceive)
                    dataSend="receive:" + playerport.ToString();//已经收到了发的牌
                    if (isPutPocker)//是否选择出牌
                    {
                        dataSend = "put:" + playerport.ToString();
                        data1 = System.Text.Encoding.UTF8.GetBytes(dataSend);
                        SocketClient.SendTo(data1, ipep);
                    }
                    if (isReceivePocker)
                    {
                        dataSend = "get:" + playerport.ToString();
                        data1 = System.Text.Encoding.UTF8.GetBytes(dataSend);
                        SocketClient.SendTo(data1, ipep);
                    }
                    if (isCanReceive != 0)
                    {
                        if (isCanReceive == 1)
                            dataSend = "can:" + playerport.ToString();
                        else if (isCanReceive == 2)
                            dataSend = "cannot:" + playerport.ToString();
                        data1 = System.Text.Encoding.UTF8.GetBytes(dataSend);
                        SocketClient.SendTo(data1, ipep);
                    }
                    if (isOver)//是否结束
                     dataSend = "over:" + playerport.ToString();                      
                }
             
                 data1 = System.Text.Encoding.UTF8.GetBytes(dataSend);
                SocketClient.SendTo(data1, ipep);
                // Thread.Sleep(1000);
                textBox3.Text = dataSend;

            }
    
        }
        public void GetData()//用于接收Game发来的信息
        {
            while (true)
            {
                byte[] datasize = new byte[4]; //用于传输数据长度的信息

                int size = 1024;
                //还剩余多少数据没有传输
                int dataleft = size;
                //传输的数据都存储到data数组中
                byte[] data = new byte[size];
                try
                {
                    EndPoint pep = (EndPoint)ipep;
                    //IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
                    ServerSocket.ReceiveFrom(data, ref pep);
                    string receiveString = Encoding.Default.GetString(data);
                    handleData(receiveString);
                    //listBox1.Items.Add((object)(receiveString));
                }
                catch
                {

                }
            }
     
        }

        private void handleData(string receiveString)//用于处理数据
        {
           
            receiveString = receiveString.TrimEnd('\0');
            textBox4.Text = receiveString;
            if (receiveString != "waiting")
            {
                string[] newstr = receiveString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                isFirstPrint= true;
                bool isGetPoker = false;
                if (newstr[0] == "pleaseput" )
                {

                   // MessageBox.Show("到您出牌了");
                    isCanPut = true;
                   // isPutPocker = true;
                    isReceive = false;
                }
                else if (newstr[0] == "pokercount")
                {
                    usercount.Clear();
                    textBox5.Text = "";
                    for (int i = 1; i < newstr.Length; i++)
                    {
                        usercount.Add(Convert.ToInt32(newstr[i]));
              
                    }
                    for (int i = 0; i < usercount.Count; i++)
                        textBox5.Text = textBox5.Text + usercount[i].ToString() + ",";
                    isReceivePocker = false;
                    isCanReceive = 0;
                    isPutPocker = false;
                    isFirstPrint = false;
                    isCanPut = false;
                    isReceive = true;
                }
                else if (newstr[0] != "pleaseput")
                {
                    if (newstr[0] != "Pub"&& newstr[0] != "over"&& newstr[0] != "can" && newstr[0] != "cannot")
                    {
                        isReceive = false;
                        userpoker.Clear();
                        for (int i = 1; i < newstr.Length; i++)
                        {
                            isGetPoker = false;
                            foreach (int m in userpoker)//遍历牌堆中牌寻找是否有相同牌
                            {
                                if (Convert.ToInt32(newstr[i]) == m)
                                {
                                    isGetPoker = true;//刚好收完一副牌
                                    break;
                                }
                            }
                            if (!isGetPoker)
                                userpoker.Add(Convert.ToInt32(newstr[i]));
                            else
                                break;
                        }
                        if (record_pokercount != userpoker.Count)//当收来的数组发生变化说明出牌成功且接收到出了之后的牌
                        {
                            isPutPocker = false;
                            isReceive = true;
                            listBox2.Items.Clear();
                            for (int i = 0; i < userpoker.Count; i++)
                            {
                                listBox2.Items.Add(userpoker[i]);

                            }
                            record_pokercount = userpoker.Count;
                        }
                        isReceivePocker = false;
                        isCanReceive = 0;
                        isPutPocker = false;
                        isFirstPrint = false;                    
                        isCanPut = false;
                        isReceive = true;
                    }
                    else if (newstr[0] == "Pub" )
                    {
                        isReceive = false;
                        userpoker.Clear();
                        for (int i = 1; i < newstr.Length; i++)
                        {
                            isGetPoker = false;
                            foreach (int m in userpoker)//遍历牌堆中牌寻找是否有相同牌
                            {
                                if (Convert.ToInt32(newstr[i]) == m)
                                {
                                    isGetPoker = true;//刚好收完一副牌
                                    break;
                                }
                            }
                            if (!isGetPoker)
                                userpoker.Add(Convert.ToInt32(newstr[i]));
                            else
                                break;
                        }

                        if (record_pokercount != userpoker.Count)//当收来的数组发生变化说明出牌成功且接收到出了之后的牌
                        {
                            isPutPocker = false;
                            isReceive = true;
                            listBox1.Items.Clear();
                            for (int i = 0; i < userpoker.Count; i++)
                            {
                                listBox1.Items.Add(userpoker[i]);

                            }
                            record_pokercount = userpoker.Count;
                        }


                        isReceivePocker = false;
                        isCanReceive = 0;
                        isPutPocker = false;
                        isFirstPrint = false;
                        isCanPut = false;
                        isReceive = true;
                    }
                    else if(newstr [0]=="can")
                    {
                        isReceivePocker = false;
                        isCanReceive = 0;
                        isPutPocker = false;
                        isFirstPrint = false;
                        isCanPut = false;
                        isReceive = true;

                    }
                    else if(newstr[0]=="cannot")
                    {
                        //MessageBox.Show("当前没有可收的牌");
                        isReceivePocker = false;
                        isCanReceive = 0;
                        isPutPocker = false;
                        isFirstPrint = false;
                        isCanPut = false;
                        isReceive = true;
                    }
                    else if (newstr[0] == "over")
                    {
                        isOver = true;
                        MessageBox.Show("游戏结束");
                        Thread.Sleep(5000);
                        System.Environment.Exit(0);
                    }
                }
               
            }
                
        }
        private void button2_Click(object sender, EventArgs e)
        {          
            isClickStart = true;
            userpoker.Clear();
        }

        private void button1_Click_1(object sender, EventArgs e)//服务器ip默认是主机ip,实际可能并非如此
        {
            //初始化开始界面
            //listBox1.Items.Add("来自：" + tcpClientUpper.Client.RemoteEndPoint.ToString());
            // UDPSend = new Thread(new ThreadStart(attendGame));
            // UDPSend.Start();
            if (isFirstClick)
            {
                isFirstClick = false;
                StartMonitor();
                IP = IPAddress.Parse(textBox1.Text);
                port = 7000;
                ipep = new IPEndPoint(IP, port);
                SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
              
                UDPSend = new Thread(new ThreadStart(SendData));
                UDPSend.Start();
                StartReceive();
            }
          
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isCanPut)
                isPutPocker = true;
            else
                MessageBox.Show("请等待上一位玩家出牌……");
            isReceive = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isReceivePocker = true;
            isReceive = false;
        }
    }
}
