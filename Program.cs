using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using p2p_client_test;
using System.Net;
using System.Net.Sockets;
using System.IO;
using common;

namespace P2P_CLient_test
{
    class Program
    {
        //public static string serverip = "120.132.83.168";
        //public static int nport = 10501;
        public static string serverip = "192.168.0.100";
        public static int nport = 10503;
        public static int serverID = 1;
        //public static Dictionary<int, P2PMgr> m_list = new Dictionary<int, P2PMgr>();
        static void Main(string[] args)
        {
            Console.WriteLine("输入accountID：");
            string input = Console.ReadLine();
            int accountId = Int32.Parse(input);

            P2PMgr tmpInfo = new P2PMgr((UInt32)(accountId), (uint)serverID, serverip, nport, 33390);
            Console.WriteLine("输入任何KEY 继续");
            Console.ReadLine();

            Console.WriteLine("输入 send 继续 --向对方发送 大数据");
  
            input = Console.ReadLine();

            Msg_Logic2Client_Chat_Broadcast resPacket = new Msg_Logic2Client_Chat_Broadcast();
            resPacket.m_sender_account_id = 0;
            resPacket.m_sender_hunting_lv = 0;
            resPacket.m_sender_name = "sss";
            string filePath = "j:/12.mp3";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("读取失败！错误原因：可能不存在此文件");
#if UNITY_EDITOR
                          Debug.Log("\n\t读取失败！\n错误原因：可能不存在此文件");
#endif
                return;
            }
            FileStream fs = new FileStream(filePath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            p2p_chat_msg sendMsg = new p2p_chat_msg((int)(br.BaseStream.Length));

            //sendMsg.m_chatLen = (int)(br.BaseStream.Length) + 6;

           byte[] bbuffer = new byte[br.BaseStream.Length];
            int nReadSize = br.Read(bbuffer, 0, bbuffer.Length);
            Buffer.BlockCopy(bbuffer, 0, sendMsg.m_chatBody, 6, bbuffer.Length);

//            if (nReadSize != bbuffer.Length)
//            {
//#if UNITY_EDITOR
//                          Debug.Log("\n\t读取流失败");
//#endif
//                Console.WriteLine("读取流失败");
//                return;
//            }
//            resPacket.m_chat = @System.Text.Encoding.UTF8.GetString(bbuffer);

            //enum_Msg_ChatVoice

            int nTmpLen = resPacket.m_chat.Length;
            UInt64 nClientID = (UInt64)serverID;
            if (accountId == 31448)
                nClientID = (nClientID << 32) + 31449;
            else
                nClientID = (nClientID << 32) + 31448;

            //Console.WriteLine("send buffer : len = {0}", bbuffer.Length);

            //MemoryStream ms = new MemoryStream();
            //Serializer.Serialize(ms, resPacket);
            //Console.WriteLine("send packet : len = {0}", ms.Length);
            tmpInfo.SendUdp(nClientID, sendMsg.m_chatBody, sendMsg.m_chatBody.Length);
    //        tmpInfo.SendUdpMsg<Msg_Logic2Client_Chat_Broadcast>(nClientID, resPacket, (int)MsgType.enum_Msg_Logic2Client_Chat_Broadcast);



            Console.ReadLine();

        }
    }
}
