using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;
using p2p_client_test;
using common;
//using UnityEngine;
using System.IO;
using System.Threading;

namespace p2p_client_test
{
    public class p2p_chat_msg
    {
        public p2p_chat_msg(int nChatLen)
        {
            m_chatLen = nChatLen;
            m_chatBody = new byte[m_chatLen + 6];

            Endian.WriteShort(m_chatBody, 0, (short)(nChatLen+4)); //buf总大小
            Endian.WriteInt(m_chatBody, 2, 65535); //消息号
            

            //byte[] LenByte = new byte[2];
            //    Buffer.BlockCopy(packet, 0, LenByte, 0, 2);
            //    short ProtoLen = BitConverter.ToInt16(LenByte, 0);
            //    ProtoLen = Endian.Switch(ProtoLen);
            //    //Debug.Log("processPacket :1," + nRecvLen.ToString());
            //    if (ProtoLen + 2 <= nRecvLen)
            //    {
            //        byte[] ByteBuff = new byte[ProtoLen + 2];
            //        Buffer.BlockCopy(packet, 0, ByteBuff, 0, ProtoLen + 2);
            //        //ByteBuff.RemoveRange(0, ProtoLen + 2);
            //        //Debug.Log("processPacket :2 " + nRecvLen.ToString());
            //        byte[] FlagByte = new byte[4];  //协议号
            //        byte[] DataByte = new byte[ByteBuff.Length - 6]; //消息内容

            //        Array.Copy(ByteBuff, 2, FlagByte, 0, FlagByte.Length);
            //        Array.Copy(ByteBuff, 6, DataByte, 0, DataByte.Length);

            //        //消息号
            //        int nMsgType = BitConverter.ToInt32(FlagByte, 0);
            //        nMsgType = Endian.Switch(nMsgType);
            //    }
        }

        public byte[] m_chatBody;
        public int m_chatLen;

    }
    public class p2p_player_id
    {
        public p2p_player_id(UInt32 accountID,UInt32 serverId)
        {
            m_nAccountId = accountID;
            m_nServerId = serverId;
        }
        public UInt32 GetAccountID()
        {
            return m_nAccountId;
        }
        public UInt32 GetServerID()
        {
            return m_nServerId;
        }
        private UInt32 m_nAccountId = 0;
        private UInt32 m_nServerId = 0;
    }
    class  CP2P_Info
    {
        public CP2P_Info(UInt64 id)
        {
            m_P2P_Client_Id = id;
        }
        private bool m_bP2P_Success = false; //是否P2P连接成功
        public bool P2P_Success
        {
            get { return m_bP2P_Success; }
            set { m_bP2P_Success = value; }
        }
        private UInt64 m_P2P_Client_Id = 0; //客户端ID
        public UInt32 GetServerId()
        {
            return (UInt32)(m_P2P_Client_Id >> 32);
        }
        public UInt32 GetAccountID()
        {
            return (UInt32)(m_P2P_Client_Id & 0xffffffff);
        }
        public UInt64 GetPlayerID()
        {
            return m_P2P_Client_Id;
        }
        private UInt32 m_ReConnectCount = 0; //重复连接次数
        public UInt32 GetConnectCount()
        {
            return m_ReConnectCount;
        }
        public void AddConnectCount()
        {
            m_ReConnectCount++;
        }
        public void ResetConnectCount()
        {
            m_ReConnectCount = 0;
        }
    }

    //P2P 
    public class P2PMgr : AsynUdpP2PClient
    {
        /// ////////////////////////////////////////////////////////////////////////////////////
        /// 
        ///  公有 方法
        //测试
        static int nameIndex = 1;
        public string sFilePath ;
        private Mutex mut_NoSucList = new Mutex();
        public P2PMgr(UInt32 nAccountID, UInt32 nServerID, string ip, int port, int nMyPort)
        {
            initP2p(nAccountID, nServerID, ip, port, nMyPort);
            TimeReConnectClient();
        }

        //p2p通信 收包回调
        override public void RecvUdp(UInt64 nClientID, int nMsgType, ref byte[] buffer, int nSize)
        {
#if WYS_TEST
            Debug.Log("testchat recv one msg. clientid = "+ nClientID.ToString() );
#endif
            Console.WriteLine("testchat recv one msg. clientid = " + nClientID.ToString());
            if (nClientID == 0)
                setConnectSrv(true);
            switch(nMsgType)
            {
                case (int)MsgType.enum_Msg_Chat2Client_TeamMember_IpPort_Res:
                    {
                        if(nClientID == 0) //0 默认为服务器id号
                        {
#if WYS_TEST
            Debug.Log("testchat req p2p.. " );
                           
#endif
                            Console.WriteLine("testchat req p2p.. ");
                            Msg_Chat2Client_TeamMember_IpPort_Res resPacket = new Msg_Chat2Client_TeamMember_IpPort_Res();
                            parsePacket<Msg_Chat2Client_TeamMember_IpPort_Res>(ref resPacket, ref  buffer, nSize);
                            P2P_S2C_P2PRes(resPacket);
                        }
                    }
                    break;
                    //other message
                    {
                        //to do
                    }
                case (int)MsgType.enum_Msg_Logic2Client_Chat_Broadcast:
                    {
                        //Debug.Log("testchat recv chat msg." );
                        Console.WriteLine("testchat recv chat msg.");
                        Msg_Logic2Client_Chat_Broadcast resPacket = new Msg_Logic2Client_Chat_Broadcast();
                        parsePacket<Msg_Logic2Client_Chat_Broadcast>(ref resPacket, ref  buffer, nSize);
                        //测试
                        //FileStream fs = new FileStream(, FileMode.Create);
                        nameIndex++;
                        //string testName = AssetPathManager.Instance.assetDepositDirectory + "testVoice";
                        //testName += nameIndex.ToString();
                        //testName += ".wav";
                        string testName = "j:/sss";
                        testName += nameIndex.ToString();
                        testName += ".mp3";
                        BinaryWriter writer = new BinaryWriter(File.Open(testName, FileMode.Create));   
                        //System.Text.Encoding.UTF8.GetBytes(resPacket.m_chat);
                        byte[] writeFile = System.Text.Encoding.UTF8.GetBytes(resPacket.m_chat);
                        //开始写入  
                        writer.Write(writeFile, 0, writeFile.Length);
                        //清空缓冲区、关闭流  
                        writer.Flush();
                        writer.Close();

                        //System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                        //player.Play();

                        //Console.Write(resPacket.m_chat);
                    }
                    break;
                case  65535:
                    {
                        nameIndex++;
                        //string testName = AssetPathManager.Instance.assetDepositDirectory + "testVoice";
                        //testName += nameIndex.ToString();
                        //testName += ".wav";
                        string testName = "j:/sss";
                        testName += nameIndex.ToString();
                        testName += ".mp3";
                        BinaryWriter writer = new BinaryWriter(File.Open(testName, FileMode.Create));

                        //开始写入
                        writer.Write(buffer, 0, nSize);
                        //清空缓冲区、关闭流  
                        writer.Flush();
                        writer.Close();
                    }
                    break;
                default:
                    break;
            }

        }
        //客户端关闭回调
        override public void CloseUdp(UInt64 nClientID)
        {
#if WYS_TEST
            Debug.Log("testchat close udp. clientid = " + nClientID.ToString());
#endif
            Console.WriteLine("testchat close udp. clientid = {0}", nClientID);
            deleteRole(nClientID);

            //other something
            {
                //to do
            }
        }
        //客户端连接回调
        override public void OnConnect_Udp(UInt64 nClientID)
        {
#if WYS_TEST
            Debug.Log(" 连接成功！！！！！！！！testchat connect udp. clientid = " + nClientID.ToString());
#endif
            Console.WriteLine(" 连接成功！！！！！！！！testchat connect udp. clientid =  {0}", nClientID);

            if (mut_NoSucList.WaitOne(500))
            {
                try
                {
                    //检测此连接ID 所在P2P的所有客户端是否都已连接成功
                    if (m_NoSuccessList.ContainsKey(nClientID))
                    {
                        if (!m_p2p_Ok_List.ContainsKey(nClientID))
                        {
                            CP2P_Info tmpInfo = m_NoSuccessList[nClientID];
                            tmpInfo.P2P_Success = true;
                            m_p2p_Ok_List.Add(nClientID, tmpInfo);
                        }
                        m_NoSuccessList.Remove(nClientID);
                    }
                    mut_NoSucList.ReleaseMutex();
                }
                catch
                {
                    mut_NoSucList.ReleaseMutex();
                }
            }
           

            //other something
            {
                //to do
            }
        }
        public void P2P_DisConnect(UInt64 nClientId) //主动向其他客户端断开P2P通信
        {
            if (m_p2p_Ok_List.ContainsKey(nClientId))
            {
                CP2P_Info tmpCli = m_p2p_Ok_List[nClientId];
                UInt64 nCliID = tmpCli.GetPlayerID();
                DisConnectPacket(nCliID);

                m_p2p_Ok_List.Remove(nCliID); //自己也删掉对方信息
                deleteClient(nCliID);
            }
        }
        //发送udp 消息
         public bool SendUdpMsg<T>(UInt64 nClientID, T ProtocolPacket, int ProtocolType) where T : IMessage
        {
#if WYS_TEST
            Debug.Log(" testchat send udp. clientid = " + nClientID.ToString());
#endif
            Console.WriteLine(" testchat send udp. clientid =  {0}", nClientID);
            if (m_p2p_Ok_List.ContainsKey(nClientID) || 0 == nClientID) //0 为服务器ID
             {
                SendUdp<T>(nClientID,ProtocolPacket,ProtocolType);
                 return true;
             }
                return false;
        }
        
        //请求客户端P2P
        //@ClientIdList ： 此次需要P2P的客户端
         public bool P2P_C2S_P2PReq(List<p2p_player_id> clientIdList)//向服务端请求p2p通信
        {
            common.Msg_Client2Chat_TeamMember_IpPort_Req reqPacket = new Msg_Client2Chat_TeamMember_IpPort_Req();
            reqPacket.m_account_id = (UInt32)(m_myClientID & 0xffffffff);
            reqPacket.m_server_id = (UInt32)(m_myClientID >> 32);

            List<UInt32> accountIdList = reqPacket.m_req_account_id;
            List<UInt32> serverIdList = reqPacket.m_req_server_id;
            foreach (p2p_player_id v in clientIdList)
            {
                accountIdList.Add(v.GetAccountID());
                serverIdList.Add(v.GetServerID());
            }
            SendUdpMsg<common.Msg_Client2Chat_TeamMember_IpPort_Req>(0, reqPacket, (int)MsgType.enum_Msg_Client2Chat_TeamMember_IpPort_Req);
            return true;
        }

         ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 私有方法


         //服务端返回请求p2p通信 结果
         private bool P2P_S2C_P2PRes(Msg_Chat2Client_TeamMember_IpPort_Res res)
        {
            if (res.has_m_req_account_id && res.has_m_req_server_id && res.has_m_addrList)
            {
                //添加此次通信的P2P id号，添加此次客户端队列
                //Console.WriteLine(" input everyone  continue.");
                
                //string ne = Console.ReadLine();

                addNewP2P(res.m_addrList);
                return true;
            }
            return false;
        }
         private bool addNewP2P(List<ClientAddr> addrList)//添加新的P2P通信
        {
             string sMyIp = null;
             foreach (ClientAddr kv in addrList)
            {
                 UInt64 tmpDesId = kv.m_ServerID;
                 if (tmpDesId == m_myClientID)//自己则不进入队列
                 {
                     sMyIp = kv.m_clientIp_LAN;
                     break;
                 }
             }
            foreach (ClientAddr kv in addrList)
            {
#if WYS_TEST
             Debug.Log(" testchat in addrlist ip port: " + kv.m_clientIp + ": " + kv.m_clientPort.ToString() + "--" + kv.m_AccountID.ToString());
#endif
                Console.WriteLine(" testchat in addrlist ip port: {0} : {1} -- {2}", kv.m_clientIp, kv.m_clientPort, kv.m_AccountID);
                UInt64 tmpDesId = kv.m_ServerID;
                tmpDesId = (tmpDesId << 32) + kv.m_AccountID;

                if (tmpDesId == m_myClientID)//自己则不进入队列
                    continue;
                if (m_p2p_Ok_List.ContainsKey(tmpDesId)) //已经在成功队列，则不用重复加入
                    continue;
                if (m_NoSuccessList.ContainsKey(tmpDesId)) //已经在没有成功队列，则重置重连次数与IP地址,
                {
#if WYS_TEST
             Debug.Log(" testchat add ip port: " + kv.m_clientIp + ": " + kv.m_clientPort.ToString() + "--" + kv.m_AccountID.ToString());
#endif
                    if (sMyIp != kv.m_clientIp)
                        addClient(tmpDesId, kv.m_clientIp, (int)kv.m_clientPort);
                    else
                        addClient(tmpDesId, kv.m_clientIp_LAN, (int)kv.m_port_lan);

                    CP2P_Info tmpInfo = m_NoSuccessList[tmpDesId];
                    tmpInfo.ResetConnectCount();

                    continue;
                }
                if (mut_NoSucList.WaitOne(500))
                {
                    try
                    {
                        m_NoSuccessList.Add(tmpDesId, new CP2P_Info(tmpDesId));
                        mut_NoSucList.ReleaseMutex();
                    }
                    catch
                    {
                        mut_NoSucList.ReleaseMutex();
                    }
                }
               
                addClient(tmpDesId, kv.m_clientIp, (int)kv.m_clientPort); //添加客户端地址
            }
            return true;
        }
         private bool deleteRole(UInt64 nClientId) //删除客户端 p2p
        {
            if (m_p2p_Ok_List.ContainsKey(nClientId))
            {
                m_p2p_Ok_List.Remove(nClientId);
            }
            if (m_NoSuccessList.ContainsKey(nClientId))
            {
                if (mut_NoSucList.WaitOne(500))
                {
                    try
                    {
                        m_NoSuccessList.Remove(nClientId);
                        mut_NoSucList.ReleaseMutex();
                    }
                    catch
                    {
                        mut_NoSucList.ReleaseMutex();
                    }
                }
               
            }
            return true;
        }
        private void TimeReConnectClient()
        {
            reconnect.Elapsed += new System.Timers.ElapsedEventHandler(SendConnect);
            reconnect.AutoReset = true;
            reconnect.Enabled = true;
        }
        private void SendConnect(object sender, System.Timers.ElapsedEventArgs e) //p2p 向目标客户端打洞 开始连接
        {
            if (!GetConnectSrvStatus())
            {
                reconnect.AutoReset = false;
                reconnect.Enabled = false;
                return;
            } 

            if (m_NoSuccessList.Count != 0)
            {
                if (mut_NoSucList.WaitOne(500))
                {
                    try
                    {
                        foreach (KeyValuePair<UInt64, CP2P_Info> v in m_NoSuccessList)
                        {
                            connectPacket(v.Key);
                            v.Value.AddConnectCount();
                        }
                        mut_NoSucList.ReleaseMutex();
                    }
                    catch
                    {
                        mut_NoSucList.ReleaseMutex();
                    }
                }               
            }
        }
        private void initP2p(UInt32 nAccountID, UInt32 nServerID, string ip, int port, int nMyPort)
        {
            startUdp(ip, port, nMyPort, nAccountID, nServerID);
            m_p2p_Ok_List = new Dictionary<UInt64, CP2P_Info>();
            m_NoSuccessList = new Dictionary<UInt64, CP2P_Info>();
            m_myClientID = nServerID;
            m_myClientID = (m_myClientID << 32) + nAccountID;
        }

        private Dictionary<UInt64, CP2P_Info> m_p2p_Ok_List = null; //留着判断P2P 中是否所有客户端都已成功建立连接
        private Dictionary<UInt64, CP2P_Info> m_NoSuccessList = null; //没有P2P成功的队列
        private UInt64 m_myClientID = 0;
        private System.Timers.Timer reconnect = new System.Timers.Timer(1000); //重新P2P连接
    }
}
