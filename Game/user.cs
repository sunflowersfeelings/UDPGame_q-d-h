using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class user
    {
        public List<int> userpoker = new List<int>();
        public int User_isFailedFlag;//记录玩家“输赢”状态
        public IPAddress IP;
        public IPEndPoint ipep;
        public Socket socket;
        public int port;
        public bool isAttend;//是否加入
        public bool isStart;//是否开始
        public bool isDeal;//是否收到牌
        public int isSend;//是否能收牌
        public bool isPut;//是否决定出牌
        public bool isGet;//是否决定收牌
        public bool isOver;//是否结束
        public int Mark;//玩家编号
        public int loseorder;//出局的顺序
        public user()
        {
            port = 0;
            User_isFailedFlag = 0;
            isAttend = false;
            isStart = false;
            isDeal = false;
            isSend = 0;
            isPut = false;
            isGet = false;
            isOver = false;
        }
    }
}
