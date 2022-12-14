using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game
{
    public partial class Start : Form
    {
        private Calculate calculate; 
        private SendMessage sendmessage;
        private ReceiveMessage receivemessage;
        private Visualization visualization;
        public Thread UDPReceive;
        public static List<user> users;
        public static List<user> Rank;//存储出局玩家的端口和ip信息
        public static int loseorder=0;//代表出局顺序
        public int[] poker = new int[52];//接竹竿不需用到大小王
        public bool is_All_Start=false;//玩家是否都同意开始游戏
        public static IPAddress IP;
        public static IPAddress playerIP;
        public static Socket ServerSocket;//用于接收play中的信息
        public static IPEndPoint ipep;
        public static IPEndPoint playeripep;
        //public Thread UDPReceive;//用于接受
        public static int port;
        public static int playerport;
        public static bool isStart;//玩家是否已点击开始
        public static bool isDeal;//玩家是否已经收到牌
      
        //0-12表示黑桃   A,2,3,... 10,J,Q,K

        //13-25表示红桃  A,2,3,... 10, J, Q, K

        //26-38表示草花  A,2,3,... 10, J, Q, K

        //39-51表示方块  A,2,3,... 10, J, Q, K

        List<int> Pubpoker = new List<int>();//放置已打出手牌的列表，相当于”牌桌“或”牌堆“
    
        //public List<int> Rank = new List<int>();//存储玩家排名，（先输的玩家排名靠后）
        public int user1_isFailedFlag = 0;
       
        public static int playernumber=0;
        public Start()
        {
            InitializeComponent();
        }

        private void initial_users()//初始化users列表，在此处得出玩家数
        {
            users = new List<user>();
            while (true)
            {
                user temp_user = new user();
                users.Add(temp_user);
                while (true)
                {
                    receivemessage.ReceiveData();
                    is_All_Start = false;
                    if (playernumber > 1)
                    {
                        for (int i = 0; i < playernumber; i++)
                        {
                            is_All_Start = true;
                            if (users[i].isStart == false)//有玩家点了加入游戏但没点开始
                            {
                                is_All_Start = false;
                                break;
                            }
                        }
                    }
                    if (is_All_Start)
                    {
                        users.RemoveAt(playernumber);
                        break;
                    }
                    for (int i = 0; i < playernumber; i++)
                    {
                        if (temp_user.port == users[i].port)
                        {
                            temp_user.IP = null;
                            temp_user.port = 0;
                            temp_user.isAttend = false;
                            break;
                        }                 
                    }
                
                    if (temp_user.IP == null && temp_user.port == 0 && temp_user.isAttend == false)
                        break;
                    else if (temp_user.IP == null || temp_user.port == 0 || temp_user.isAttend == false)
                        continue;
                    else
                        break;
                }
                if (temp_user.IP != null && temp_user.port != 0 && temp_user.isAttend == true)
                    sendmessage.CreateClient(ref temp_user.IP, ref temp_user.port, ref temp_user.socket, ref temp_user.ipep);
                if(!is_All_Start||playernumber==0)
                for (int i = 0; i < playernumber; i++)
                {
                    if (temp_user.port == users[i].port)
                    {
                        users.RemoveAt(playernumber);         
                        playernumber = playernumber - 1;
                        break;
                    }
                    if (temp_user.IP == null || temp_user.port == 0)
                    {
                        users.RemoveAt(playernumber);
                        playernumber = playernumber - 1;
                        break;
                    }

                }
                if (!is_All_Start || playernumber == 0)
                {
                    playernumber++;
                    temp_user.Mark = playernumber;
                }
                if (playernumber >= 2)
                {

                    foreach (var temp in users)
                    {
                        is_All_Start = true;
                        if (temp.isStart == false)//有玩家点了加入游戏但没点开始
                        {
                            sendmessage.SendData("waiting", ref temp.socket, ref temp.ipep);//向该玩家发送等待信息
                            is_All_Start = false;
                            break;
                        }
                    }
                    if (is_All_Start)
                        break;
                }
            }
        }
        private void initial_poker()//初始化users里每一个user拿到的牌,相当于发牌
        {
            calculate = new Calculate();
            calculate.initialcard(poker);
            if (playernumber == 2)
            {
                calculate.deal(poker, users[0].userpoker, users[1].userpoker);
            }
            else if (playernumber == 3)
            {
                calculate.deal(poker, users[0].userpoker, users[1].userpoker, users[2].userpoker);
            }
            else if (playernumber == 4)
            {
                calculate.deal(poker, users[0].userpoker, users[1].userpoker, users[2].userpoker, users[3].userpoker);
            }
            else if (playernumber == 5)
            {
                calculate.deal(poker, users[0].userpoker, users[1].userpoker, users[2].userpoker, users[3].userpoker, users[4].userpoker);
            }
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
           
            receivemessage = new ReceiveMessage();
            receivemessage.StartMonitor();
            sendmessage = new SendMessage();
            initial_users();
            initial_poker();
            startgame();
        }
        private void game_rank(user temp)
        {
            if (calculate.game_over(temp.userpoker, temp.User_isFailedFlag))//可按游戏终止规则判断是否继续出牌
            {
                temp.loseorder = loseorder;
                Rank.Add(temp);
                while (true)
                {
                    sendmessage.SendData("over," + port.ToString(), ref temp.socket, ref temp.ipep);
                    receivemessage.ReceiveData();
                    if (temp.isOver)
                        break;
                }
                users.Remove(temp);//该玩家不再参与游戏
                loseorder++;
                playernumber = playernumber - 1;
            }
        }
        public void CanIput(user temp, int num)
        {
            if (temp.Mark == 1)//是第一位玩家
            {
                if (num == 1)//是第一位玩家且是第一轮默认能发牌
                    while (!temp.isPut)
                    {
                        sendmessage.SendData("pleaseput," + port.ToString(), ref temp.socket, ref temp.ipep);
                        receivemessage.ReceiveData();                      
                    }
                else if (users[users.Count - 1].isPut)//看最后一位玩家出没出，如果出就向玩家发送可以出牌的消息
                {

                    while (!temp.isPut)
                    {
                        sendmessage.SendData("pleaseput," + port.ToString(), ref temp.socket, ref temp.ipep);
                        receivemessage.ReceiveData();                     
                    }
                    users[users.Count - 1].isPut = false;
                }
            }
            else//其他玩家看上一位玩家是否出牌
            {
                if (users[temp.Mark - 2].isPut)
                {
                    while (!temp.isPut)
                    {
                        sendmessage.SendData("pleaseput," + port.ToString(), ref temp.socket, ref temp.ipep);
                        receivemessage.ReceiveData();                   
                    }
                    users[temp.Mark - 2].isPut = false;

                }
            }
        }
        private void CanIReceive(user temp)//玩家是否发送收牌消息（必须发牌才能收牌)并且下一个玩家选择出牌前都可以选择收牌
        {
            if(temp.isPut)
            {
                if(temp.Mark==playernumber)//是最后一名玩家则看第一名玩家是否出牌
                {
                    while (!users[0].isPut)//第一名玩家已出牌则不能再收牌
                    {
                        sendmessage.SendData("pleaseput," + port.ToString(), ref users[0].socket, ref users[0].ipep);
                        receivemessage.ReceiveData();
                        if (temp.isGet)//接收到收牌消息也停止发送消息
                        {                                           
                            break;                           
                        }
                    }   
                    if(users[0].isPut)
                        temp.isGet = false; //没来得及提出收牌
                }
                else
                {
                    while (!users[temp.Mark].isPut)//其他玩家看下一名玩家（编号比实际索引多1）

                    {
                        sendmessage.SendData("pleaseput," + port.ToString(), ref users[temp.Mark].socket, ref users[temp.Mark].ipep);
                        receivemessage.ReceiveData();
                        if (temp.isGet)
                        {                                          
                            break;
                        }
                    }
                    if (users[temp.Mark].isPut)
                        temp.isGet = false; //没来得及在下家出牌前提出收牌
                }

               
            }
        }
        private void printList(string temp_string,List<int> poker,user temp)//发送牌组
        {
            for (int i = 0; i < poker.Count; i++)
            {
                //循环体list[i]
                temp_string = temp_string + poker[i].ToString() + ",";
            }
            while (true)
            {
                sendmessage.SendData(temp_string, ref temp.socket, ref temp.ipep);
                receivemessage.ReceiveData();
                if (temp.isDeal)
                    break;
            }
            temp.isDeal = false;
        }
        private void printCount(string temp_string, user temp)//发送牌组
        {          
            while (true)
            {
                sendmessage.SendData(temp_string, ref temp.socket, ref temp.ipep);
                receivemessage.ReceiveData();
                if (temp.isDeal)
                    break;
            }
            temp.isDeal = false;
        }
        private void startgame()//游戏进行中
        {
            Rank=new List<user>();
            string temp_string = "";
            int num = 0;//统计运行次数
            while (true)
            {
                foreach (var temp in users)
                {
                    isDeal = false;
                    if(playernumber==1)//只剩最后一名玩家
                    {
                        temp.loseorder = loseorder;
                        Rank.Add(temp);
                        users.Remove(temp);
                    }
                    if (num != 0)//不是第一次发牌，则需要判定玩家有没有选择出牌，有没有出牌的条件:上一位玩家已出牌）
                    {
                        CanIput(temp, num);       
                        if (temp.isPut)
                        {
                            calculate.play_remove(Pubpoker, temp.userpoker, temp.User_isFailedFlag);
                            game_rank(temp);
                            temp.isSend = 0;
                            
                            for (int i = 0; i < users.Count; i++)//向每一个玩家展示牌桌
                            {
                                temp_string = "Pub,";
                                printList(temp_string, Pubpoker, users[i]);
                            }
                            temp_string = "";
                            printList(temp_string, temp.userpoker, temp);
                            temp_string = "pokercount,";
                            for (int i = 0; i < users.Count; i++)
                            {
                                //循环体list[i]
                                temp_string = temp_string + users[i].userpoker.Count.ToString() + ",";
                            }

                            for (int i = 0; i < users.Count; i++)//向每一个玩家发送其他玩家的牌数
                            {
                                printCount(temp_string, users[i]);
                            }
                            temp_string = "";
                            printList(temp_string, temp.userpoker, temp);
                        }
                        CanIReceive(temp);//判定是否收牌……待完成
                        temp.isDeal = false;
                        if (temp.isGet)
                        {
                            bool tempbool = calculate.play_receive(Pubpoker, temp.userpoker);//进行出牌
                            while (true)
                            {
                                if (tempbool)
                                {
                                    sendmessage.SendData("can," + port.ToString(), ref temp.socket, ref temp.ipep);                                   
                                    temp.isSend = 1;//提出要收牌实际上也能收牌
                                }
                                else
                                {
                                    sendmessage.SendData("cannot," + port.ToString(), ref temp.socket, ref temp.ipep);
                                    temp.isSend = 2;//提出要收牌实际上不能收牌
                                }
                                receivemessage.ReceiveData();
                                if (temp.isDeal)
                                    break;
                            }
                            temp.isDeal = false;
                        }
                        if (temp.isSend==1)
                        {
                            calculate.play_receive(Pubpoker, temp.userpoker);//进行收牌
                            for (int i = 0; i < users.Count; i++)//向每一个玩家展示牌桌
                            {
                                temp_string = "Pub,";
                                printList(temp_string, Pubpoker, users[i]);
                            }
                            temp_string = "pokercount,";
                            for (int i = 0; i < users.Count; i++)
                            {
                                //循环体list[i]
                                temp_string = temp_string + users[i].userpoker.Count.ToString() + ",";
                            }
                          
                            for (int i = 0; i < users.Count; i++)//向每一个玩家发送其他玩家的牌数
                            {
                                printCount(temp_string, users[i]);
                            }
                            temp_string = "";
                            printList(temp_string, temp.userpoker, temp);
                           
                            temp.isPut = false;//收牌成功可以继续出牌，为temp.isPut赋false值,那么下家此时将无法出牌也就是一下轮var temp那个循环将被跳过
                        }
                        temp.isGet = false;
                    }                  
                    else//第一次仅发牌
                    {
                        temp_string = "";
                        printList(temp_string, temp.userpoker, temp);
                        temp_string = "pokercount,";
                        for (int i = 0; i < users.Count; i++)
                        {
                            //循环体list[i]
                            temp_string = temp_string + users[i].userpoker.Count.ToString() + ",";
                        }
                        printCount(temp_string, temp);
                        
                    }                
                }
               
                num++;
            }
        }
     
    }
}
