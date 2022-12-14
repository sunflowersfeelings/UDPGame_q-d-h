using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Game
{
    class Calculate
    {

        public void initialcard(int[] poker)//洗牌
        {
            Random rd = new Random();
            //初始化牌组
            for (int i = 0; i < 52; i++)
                poker[i] = i;
            for (int i = 0; i < 52; i++)
            {
              int rnd = rd.Next(0, 51);
                /*指定任意一张牌,与当前牌进行交换*/
                int temp = poker[i];
                poker[i] = poker[rnd];
                poker[rnd] = temp;
            }
        }
        public void deal(int[] poker,List<int> user1poker, List<int> user2poker)//发牌，玩家数目为2时
        {
            int poker_max = 26;//用于此次发牌的数目
            for (int i = 0; i < poker_max; i++)
            {
                user1poker.Add(poker[i]);
                user2poker.Add(poker[i+ poker_max]);
            }
        }
        public void deal(int[] poker, List<int> user1poker, List<int> user2poker, List<int> user3poker)//发牌，玩家数目为3时
        {
            int poker_max = 17;//用于此次发牌的数目
            for (int i = 0; i < poker_max; i++)
            {
                user1poker.Add(poker[i]);
                user2poker.Add(poker[i + poker_max]);
                user3poker.Add(poker[i + poker_max*2]);
            }
        }
        public void deal(int[] poker, List<int> user1poker, List<int> user2poker, List<int> user3poker, List<int> user4poker)//发牌，玩家数目为4时
        {
            int poker_max = 13;//用于此次发牌的数目
            for (int i = 0; i < poker_max; i++)
            {
                user1poker.Add(poker[i]);
                user2poker.Add(poker[i + poker_max]);
                user3poker.Add(poker[i + poker_max * 2]);
                user4poker.Add(poker[i + poker_max * 3]);
            }
        }
        public void deal(int[] poker, List<int> user1poker, List<int> user2poker, List<int> user3poker, List<int> user4poker, List<int> user5poker)//发牌，玩家数目为5时
        {
            int poker_max = 10;//用于此次发牌的数目
            for (int i = 0; i < poker_max; i++)
            {
                user1poker.Add(poker[i]);
                user2poker.Add(poker[i + poker_max]);
                user3poker.Add(poker[i + poker_max * 2]);
                user4poker.Add(poker[i + poker_max * 3]);
                user5poker.Add(poker[i + poker_max * 4]);
            }
        }
        public void play_remove(List<int> Pubpoker, List<int> userpoker,int userpokerflag )//出牌
        {
           
                Pubpoker.Add(userpoker[0]);//则将用户所出的牌添加至牌堆中
                userpoker.RemoveAt(0);//玩家出牌后在所拥有的手牌中删除当前所出牌                   
        }    
        public bool play_receive(List<int> Pubpoker, List<int> userpoker)//收牌
        {
            for (int i = 0; i < Pubpoker.Count-1; i++)
            {
                int m = Pubpoker[i];               
                if (m % 13 == Pubpoker[Pubpoker.Count - 1] % 13)
                    {
                        int x = Pubpoker.Count();
                        int y = Pubpoker.IndexOf(m);
                    for (int s = y; s < x; s++)
                    {
                        userpoker.Add(Pubpoker[s]);//将收回的手牌放入玩家的牌堆中
                    }
                        Pubpoker.RemoveRange(y, x-y);//如有相同牌，则去除牌堆中相应的牌
                        return true;
                    }
                
            }
            
            return false;
        }
    
        public bool game_over(List<int> userpoker, int user_isFailedFlag)//判断游戏是否结束
        {
            //返回值bool：true---游戏结束  false---游戏继续
            if (userpoker.Count == 0 && user_isFailedFlag == 0)//玩家1拥有牌空&&玩家1的“输赢”flag仍为0（未输）
            {
                //Rank.Add(2);//把输玩家2加入排名
              //  Rank.Add(1);//把胜利玩家1加入排名
              //排名是否可以再分一个函数？
                user_isFailedFlag = 1;//把玩家1的“输赢”flag调为1（输）
                return true;
            }         
                return false;//不符合以上结束游戏的条件，游戏继续
        }   

      
    }
}
