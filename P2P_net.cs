using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using common;
using System.IO;
using NATUPNPLib;
//using UnityEngine;

#if UNITY_PS4

using libUpnp;
#endif

namespace p2p_client_test
{
    //public class UdpMsgCls
    //{
    //    public byte[] byteBuff;

    //    public int send_num = 0;

    //    public MsgCls()
    //    {
    //    }
    //    public void Clear()
    //    {
    //        if (byteBuff != null)
    //            Array.Clear(byteBuff, 0, byteBuff.Length);
    //        send_num = 0;
    //    }

    //    /// <summary>
    //    /// 协议包转化为字节
    //    /// </summary>
    //    public void ObjectToByte<T>(T ProtocolPacket, int ProtocolType) where T : IMessage
    //    {
    //        MemoryStream ms = new MemoryStream();
    //        Serializer.Serialize(ms, ProtocolPacket);

    //        byteBuff = new byte[ms.Length + 6];

    //        Endian.WriteShort(byteBuff, 0, (short)(ms.Length + 4));
    //        Endian.WriteInt(byteBuff, 2, ProtocolType);

    //        //         short contentLength = Endian.Switch((short)(ms.Length + 4));
    //        //         byte[] byteLength = BitConverter.GetBytes(contentLength);
    //        //         int iFlag = Endian.Switch(ProtocolType);
    //        //         byte[] byteFlag = BitConverter.GetBytes(iFlag);
    //        //         Array.Copy(byteLength, 0, byteBuff, 0, byteLength.Length);
    //        //         Array.Copy(byteFlag, 0, byteBuff, 2, byteFlag.Length);

    //        ms.Seek(0, SeekOrigin.Begin);
    //        ms.Read(byteBuff, 6, (int)ms.Length);

    //        send_num = byteBuff.Length;

    //        ms.Dispose();

    //    }
    //}
    class netAddrInit
    {
        //初始化 客户端端口，服务器地址
        private const int m_uClientBindPort = 11101;
        private const int m_uServerPort = 11102;
        private const string m_sServerIp = "127.0.0.1";
        public netAddrInit(int port = 11101)
        {
           // m_MyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_uClientBindPort);
            m_MyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);//测试
            m_ServerEndPoint = new IPEndPoint(IPAddress.Parse(m_sServerIp), m_uServerPort);
        }
        public IPEndPoint GetSrvEP()
        {
            return m_ServerEndPoint;
        }
        public IPEndPoint GetMyEP()
        {
            return m_MyEndPoint;
        }
        private IPEndPoint m_MyEndPoint = null;
        private IPEndPoint m_ServerEndPoint = null;
    }
    /// <summary>
    /// 底层udp包 类型
    /// </summary>
    public enum en_udp_msg_type
    {
        en_sendData,    //正常发送数据
        en_identify,    //收包验证消息
        en_packetInfoBeforeSendData, //发包前包信息
    }


    /// <summary>
    /// 底层UDP分包 包头协议 
    /// </summary>
    public class CPacketProtocol
    {
        private byte[] m_buffer = null; 
        private int m_nIdentifyCode = 0;
        private short m_packetIndex = 0;
        private byte m_bMsgType = 0;
        //压包调用接口，添加协议头
        // 2 + 4 + 2 + 1 + 具体msg
        //0 - 1 ，2字节为short 类型         表示包总长度
        //2 - 5， 4字节为int 类型         表示包的验证码（大包分包将产生唯一验证码）
        //6 - 7， 2字节为short类型        表示分包的发包帧（发包序号）
        //8      1字节为byte类型        表示底层的包类型（en_udp_msg_type）
        public CPacketProtocol(byte[] sourceBuf, int nIdentifyCode, short nPacketIndex, byte bMsgType)
        {
            short nLen =  9;
            if(sourceBuf != null)
                nLen += (short)(sourceBuf.Length);

            m_buffer = new byte[nLen];

            Endian.WriteShort(m_buffer, 0, nLen); //buf总大小
            Endian.WriteInt(m_buffer,2, nIdentifyCode); //验证码
            Endian.WriteShort(m_buffer, 6, nPacketIndex); //包索引（第几个包）
            m_buffer[8] = bMsgType; //包类型
            if (sourceBuf != null)
                Buffer.BlockCopy(sourceBuf, 0, m_buffer, 9, sourceBuf.Length);
        }

        //压包
        public CPacketProtocol(int nIdentifyCode, short nPacketIndex, byte bMsgType)
        {
            m_nIdentifyCode = nIdentifyCode;
            m_packetIndex = nPacketIndex;
            m_bMsgType = bMsgType;
        }
        public void copyMsgData(byte[] msgBuf,int msgSize)
        {
            m_buffer = new byte[msgSize + 9];

            Endian.WriteShort(m_buffer, 0, (short)(msgSize + 9)); //buf总大小
            Endian.WriteInt(m_buffer, 2, m_nIdentifyCode); //验证码
            Endian.WriteShort(m_buffer, 6, m_packetIndex); //包索引（第几个包）
            m_buffer[8] = m_bMsgType; //包类型

            Buffer.BlockCopy(msgBuf, 0, m_buffer, 9, msgSize);
        }
        //解包调用接口，解析源数据
        public CPacketProtocol(byte[] recvBuffer)
        {
             byte[] LenByte = new byte[2];
            Buffer.BlockCopy(recvBuffer, 0, LenByte, 0, 2);
            short packetLen = BitConverter.ToInt16(LenByte, 0); //包总大小
            packetLen = Endian.Switch(packetLen);

            short msglen = (short)(packetLen - 9);
            if(msglen > 0)
                m_buffer = new byte[msglen]; //申请buffer

            byte[] IdentifyByte = new byte[4];
            Buffer.BlockCopy(recvBuffer, 2, IdentifyByte, 0, 4); //验证码
            m_nIdentifyCode = BitConverter.ToInt32(IdentifyByte, 0);
            m_nIdentifyCode = Endian.Switch(m_nIdentifyCode);

            Buffer.BlockCopy(recvBuffer, 6, LenByte, 0, 2); //包索引
            m_packetIndex = BitConverter.ToInt16(LenByte, 0);
            m_packetIndex = Endian.Switch(m_packetIndex);

            m_bMsgType = recvBuffer[8]; //消息类型

            if (msglen > 0)
                Buffer.BlockCopy(recvBuffer, 9, m_buffer, 0, packetLen-9); //data 拷贝
        }

        //获取发送buffer
        public byte[] GetSendPacket()
        {
            return m_buffer;
        }
        //获取消息类型
        public byte GetMsgType()
        {
            return m_bMsgType;
        }
        //获取包索引
        public short GetPacketIndex()
        {
            return m_packetIndex;
        }
        //获取消息类型
        static public byte GetMsgType(byte[] recvBuffer)
        {
            return recvBuffer[8];
        }
        public int GetIdentifyCode()
        {
            return m_nIdentifyCode;
        }
        //获取解析后的消息 Data
        public byte[] GetMsg()
        {
            return m_buffer;
        }
    }

    /// <summary>
    /// UDP大包分包，重发，验证 机制
    /// </summary>
    public class CbiggerPacketProtocol
    {
        public CbiggerPacketProtocol()
        {
            initTimer();
        }
        //收包回调
        virtual public bool OnRecvMsg(byte[] recvMsg)
        {

            return true;
        }

        //发包
        public void SendPacket(byte[] PacketBuf,int nSize)
        {
            beginSend(PacketBuf, nSize);
        }

        //底层发包接口
        virtual public bool sendudp(byte[] sendBuffer, int nSize)
        {
            return true;
        }
        //原始收包 拼接包
        public bool recvudp(byte[] recvBufffer, int nSize)
        {
            CPacketProtocol tmpProtoPacket = new CPacketProtocol(recvBufffer);
            switch ((int)tmpProtoPacket.GetMsgType())
            {
                case (int)en_udp_msg_type.en_packetInfoBeforeSendData:
                    {
                        //Console.WriteLine("收到发包前的包信息 大小 ");
                        RecvPacketLen(tmpProtoPacket.GetMsg(), tmpProtoPacket.GetIdentifyCode());
                        break;
                    }
                case (int)en_udp_msg_type.en_sendData:
                    {
                        //接收数据
                        {
                            //Console.WriteLine("接收数据,长度：{0},index = {1}", tmpProtoPacket.GetMsg().Length, tmpProtoPacket.GetPacketIndex());
                            SpliceBuf(tmpProtoPacket.GetMsg(), tmpProtoPacket.GetMsg().Length,tmpProtoPacket.GetIdentifyCode(), tmpProtoPacket.GetPacketIndex());
                        }
                        break;
                    }
                case (int)en_udp_msg_type.en_identify:
                    {
                        //Console.WriteLine("收到验证消息");
                        checkIdentifyPacket(tmpProtoPacket.GetIdentifyCode(), tmpProtoPacket.GetPacketIndex());
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }

        //发送
        private int m_nSize = 0; //包总大小
        private byte[] m_SendDataBuf = null; //发送数据buf 
        private static int m_one_packet_max_size = 1024*10; //单个包最大 大小
        private byte[] m_SendBuf = new byte[m_one_packet_max_size]; //单个发包buf
        private int m_sendSize = m_one_packet_max_size;//单个发包 大小

        private int m_sendCount = 0; //发送帧个数
        private int m_sendIndex = 0; //发送索引
        private int m_WaitIdentifyIndex = 0; //确认等待帧索引
        private int m_WaitSendCount = 2; //发送等待个数
        private bool m_bStartSend = false; //是否正在发送
        private int m_SendIdentifyCode = 0; //验证码

        /*****************************************/
        //下面三个变量 用于一个包丢包次数太多，或者对方断线没有收到 会一直重复发下去
        private int m_ResendCount = 0;//重发次数
        private int m_ResendPacketIndex = 0; //重发的包
        private int m_ResendOverCount = 50;//重发总次数次数
        /*****************************************/
        //丢包定时器检测，如果500毫秒内没有收到确认包，则认为丢包，重发
        private System.Timers.Timer check_time_ = new System.Timers.Timer(300);

        //接收
        private int m_recvIndex = 0;//接受索引
        private int m_recvCount = 0; //接收帧个数
        //private byte[] m_recvBuf = new byte[m_one_packet_max_size]; //单个收包buf
        private int m_RecvSize = m_one_packet_max_size;//收包 大小
        private byte[] m_RecvDataBuf = null; //接受数据buf
        private bool m_bStartRecv = false; // 是否开始接收
        private int m_RecvIdentifyCode = 0; //验证码

        //解析后的收包，拼接包
        private bool SpliceBuf(byte[] recvbuf, int nSize,int nIdentify, int nPacketIndex)
        {
            //只是一个包 没有分包
            if (nPacketIndex == 1 && nIdentify == 0)
            {

                OnRecvMsg(recvbuf);
                return true;
            }
            //拼接包
            if (nPacketIndex == m_recvIndex && m_bStartRecv && nIdentify == m_RecvIdentifyCode)
            {
               //接收数据
#if WYS_TEST
                Debug.Log(" testchat splice buf.... index = " + m_recvIndex.ToString() + ",recvCount = " + m_recvCount.ToString());
#endif
                Console.WriteLine(" testchat splice buf.... index = " + m_recvIndex.ToString() + ",recvCount = " + m_recvCount.ToString());
                int nOffset = (m_recvIndex - 1) * m_one_packet_max_size;
                Buffer.BlockCopy(recvbuf, 0, m_RecvDataBuf, nOffset, nSize); //包索引
                if (m_recvIndex == m_recvCount)
                {
                    Console.WriteLine("m_recvIndex = {0},m_recvCount = {1},接收完毕！处理包！", m_recvIndex, m_recvCount);
                    m_bStartRecv = false;
                    OnRecvMsg(m_RecvDataBuf);
                   // Console.WriteLine();
                    
                    return true;
                }
                Console.WriteLine("拼接包，offset:{0},addsize:{1}", nOffset, nSize);
                //发送确认包
                sendConfirm();
            }
            return true;
        }
        //添加确认包
        private byte[] addIdentifyPacket(int nPacketIndex)
        {
            CPacketProtocol tmpProtoPacket = new CPacketProtocol(null, m_RecvIdentifyCode, (short)nPacketIndex, (byte)en_udp_msg_type.en_identify);
            return tmpProtoPacket.GetSendPacket();
        }
        //接收确认包
        private bool checkIdentifyPacket(int nIdentifyCode,int nPacketIndex)
        {
            Console.WriteLine("接收确认包 nPacketIndex = {0}，等待包index = {1}  ", nPacketIndex, m_WaitIdentifyIndex);
#if WYS_TEST
                 Debug.Log(" testchat 接收确认包 nPacketIndex = " + nPacketIndex.ToString() + "，等待包index =" + m_WaitIdentifyIndex.ToString());
#endif
            if (nIdentifyCode != m_SendIdentifyCode)
                return false;
            m_ResendPacketIndex = nPacketIndex + 1;
            if (nPacketIndex != m_WaitIdentifyIndex)
            {
                //丢包 重发
                ResendPacket();
                return true;
            }
            else
                m_WaitIdentifyIndex++;

            if (m_sendCount == nPacketIndex)
            {
                m_bStartSend = false;
                return true;
            }
            sendContinue();
            return true;
        }
        //重发
         private void  ResendPacket()
        {
#if WYS_TEST
                 Debug.Log(" testchat 丢包重发 m_sendIndex = " + m_sendIndex.ToString());
#endif
            Console.WriteLine(" testchat 丢包重发 m_sendIndex = " + m_sendIndex.ToString());
            //检测一个包的重发次数是否超出上限 超出后就不发了
            if (m_WaitIdentifyIndex == m_ResendPacketIndex )
            {
                m_ResendCount++;
                if (m_ResendCount >= m_ResendOverCount)
                {
                    m_bStartSend = false;
                    return;
                }
            }
            m_sendIndex = m_ResendPacketIndex;
            m_WaitIdentifyIndex = m_ResendPacketIndex;

            sendContinue();
        }
        //发送接收数据确认包
        private bool sendConfirm()
         {
#if WYS_TEST
                  Debug.Log(" testchat 发送确认包.... m_recvIndex = " + m_recvIndex.ToString());
#endif
             Console.WriteLine(" testchat 发送确认包.... m_recvIndex = " + m_recvIndex.ToString());
            byte[] sBuf = addIdentifyPacket(m_recvIndex);
            sendudp(sBuf, sBuf.Length);
            m_recvIndex++;
            return true;
        }

        //分包前发送包长度
       private bool SendPacketInfoBeforeSubpackage()
       {
            int nMsgLen = m_SendDataBuf.Length;
            byte[] msgDataLen = new byte[4];
            Endian.WriteInt(msgDataLen, 0, nMsgLen);

#if WYS_TEST
                 Debug.Log(" testchat 分包前发送包长度信息，长度： " + nMsgLen.ToString());
#endif
            Console.WriteLine(" testchat 分包前发送包长度信息，长度： " + nMsgLen.ToString());
            CPacketProtocol tmpProtoPacket = new CPacketProtocol(msgDataLen, m_SendIdentifyCode, 1, (byte)en_udp_msg_type.en_packetInfoBeforeSendData);
            byte[] desByte = tmpProtoPacket.GetSendPacket();
            //发送
            return sendudp(desByte, desByte.Length);
       }
        private void RecvPacketLen(byte[] msgData,int nIdentify)
        {

            int nLen = BitConverter.ToInt32(msgData, 0);
            nLen = Endian.Switch(nLen);

            m_RecvIdentifyCode = nIdentify;
            m_RecvDataBuf = new byte[nLen];
            int nPacketCount = nLen / m_one_packet_max_size;
            if (0 != nLen % m_one_packet_max_size)
                nPacketCount++;
            m_recvCount = nPacketCount;
            m_bStartRecv = true;
            m_recvIndex = 0;
#if WYS_TEST
             Debug.Log(" testchat 接收对方发送的包长度信息len =  " + nLen.ToString());
#endif
            Console.WriteLine(" testchat 接收对方发送的包长度信息len =  " + nLen.ToString());
            //Console.WriteLine("接收对方发送的包长度信息len = {0}  ", nLen);

            sendConfirm();
        }
        
        //开始发送
        private bool beginSend(byte[] buffer,int nSize)
        {
#if WYS_TEST
           Debug.Log(" testchat send udp begin." );
#endif
            //Console.WriteLine(" testchat send udp begin.");
            //buffer赋值
            m_SendDataBuf = buffer;
            m_nSize = nSize;
            
            //计算一共要发多少帧
            m_sendCount = nSize/m_sendSize;
            if (nSize % m_sendSize != 0)
                m_sendCount++;

            //生成发送验证码
           m_SendIdentifyCode =  Environment.TickCount & Int32.MaxValue;

            //初始化数据
            m_sendIndex = 0; //发送索引
            m_bStartSend = true; //开始发送
            m_WaitIdentifyIndex = 0; //确认等待帧
            //Console.WriteLine("需要拆分的包个数{0}，验证码：{1}", m_sendCount,m_SendIdentifyCode);
            sendContinue();
            return true;
        }
        //继续发包
        private bool sendContinue()
        {
            //如果只有一个包怎不做分包 和 检查 直接发包
            if (m_sendCount == 1)
            {
                CPacketProtocol tmpPr = new CPacketProtocol(m_SendDataBuf, 0, 1, (byte)en_udp_msg_type.en_sendData);
                byte[] desByte = tmpPr.GetSendPacket();
                //发送
                sendudp(desByte, desByte.Length);
                //sendudp(m_SendDataBuf, m_SendDataBuf.Length);
                return true;
            }
            /******************* 分包 *********************************/

#if WYS_TEST
           Debug.Log(" testchat send udp begin fengbao....");
#endif
            Console.WriteLine(" testchat send udp begin fengbao....");

            //定时器暂停
            StopTimer();
            if (!m_bStartSend)
                return true;
            if (m_sendIndex > m_sendCount || m_WaitIdentifyIndex > m_sendCount)
            {
                //Console.WriteLine("包已发完");
#if WYS_TEST
            Debug.Log(" testchat 包已发完 " );
#endif
                Console.WriteLine(" testchat 包已发完 ");
                m_bStartSend = false;
                return true; //包已发完
            }
            if (m_sendIndex == 0)
            {
#if WYS_TEST
            Debug.Log(" testchat 分包前先发送包长度 packetLen =  " + m_SendDataBuf.Length.ToString());
#endif
                Console.WriteLine(" testchat 分包前先发送包长度 packetLen =  " + m_SendDataBuf.Length.ToString());
                SendPacketInfoBeforeSubpackage();
                m_sendIndex++;
            }
            while (m_WaitSendCount > m_sendIndex - m_WaitIdentifyIndex && m_sendIndex <= m_sendCount) 
            {
                //计算将要发送的数据指针位置与大小
                int nBufIndex = 0; //指向位置
                int nBufferSize = 0;//大小
                if (m_sendIndex == m_sendCount)
                {
                    nBufferSize = m_nSize % m_sendSize;
                    nBufIndex = (m_sendIndex - 1) * m_one_packet_max_size;
                }
                else
                {
                    nBufferSize = m_one_packet_max_size;
                    nBufIndex = (m_sendIndex - 1) * m_one_packet_max_size;
                }
                //copy
                Buffer.BlockCopy(m_SendDataBuf, nBufIndex, m_SendBuf, 0, nBufferSize);
               
                //加包头
                CPacketProtocol tmpProtoPacket = new CPacketProtocol(m_SendIdentifyCode, (short)m_sendIndex, (byte)en_udp_msg_type.en_sendData);
                tmpProtoPacket.copyMsgData(m_SendBuf, nBufferSize);
                byte[] desByte = tmpProtoPacket.GetSendPacket();
                //发送
                sendudp(desByte, desByte.Length);
#if WYS_TEST
            Debug.Log(" testchat send subpacket index :  " + m_sendIndex.ToString() + ",m_WaitIdentifyIndex = " + m_WaitIdentifyIndex.ToString() + ",m_sendCount = " + m_sendCount.ToString());
#endif
                Console.WriteLine(" testchat send subpacket index :  " + m_sendIndex.ToString() + ",m_WaitIdentifyIndex = " + m_WaitIdentifyIndex.ToString() + ",m_sendCount = " + m_sendCount.ToString());
                m_sendIndex++;
            }
            beginTimer();
            return true;
        }
        //初始化定时器
        private void initTimer()
        {
            check_time_.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeCheck);
            check_time_.AutoReset = false;
        }
        //开始定时器
        private void beginTimer()
        {
           // Console.WriteLine("开始定时器检查超时没有收到验证包 ");
            check_time_.Start();
        }
        //定时器回调
         private void OnTimeCheck(object sender, System.Timers.ElapsedEventArgs e)
         {
             if(m_bStartSend)
             {
                 //丢包重发
                 ResendPacket();
             }
         }
        //重置定时器
         private void ResetTimer()
         {
             check_time_.Stop();
             check_time_.Start();
         }
        //停止定时器
         private void StopTimer()
         {
            // Console.WriteLine("关闭定时器");
             check_time_.Stop();
         }
    }
    /// <summary>
    /// P2P客户端地址
    /// </summary>
    class clientAgent : CbiggerPacketProtocol
    {
        //p2p 通信握手
        public enum p2p_handshake_NextState
        {
            //p2p_req, //客户端之间请求P2P(首先发出的请求不可到达对方，将会打洞，后发的请求能够到达对方)
            //p2p_ret, //首先发出请求方，收到返回信息 
            //p2p_affirm, //再次发送确认
            p2p_failed,
            p2p_success,
        }
        public clientAgent(string ip, int nPort, UInt64 nClientID, AsynUdpP2PClient udpsrv)
        {
            m_clientEP = new IPEndPoint(IPAddress.Parse(ip), nPort);
            m_nClientID = nClientID;

            UInt64 tmpAddr = (UInt64)m_clientEP.Address.GetHashCode();
            m_clientAddrId = tmpAddr << 32;
            m_clientAddrId |= (UInt32)m_clientEP.Port;
            m_udp_srv = udpsrv;
        }
        public IPEndPoint GetEp()
        {
            return m_clientEP;
        }
        public UInt64 GetClientId()
        {
            return m_nClientID;
        }
        public UInt64 GetAddrId()
        {
            return m_clientAddrId;
        }
        public void SetRecvTime()
        {
            m_lastRecvTime = Environment.TickCount & Int32.MaxValue;
        }
        public int GetLastRecvTime()
        {
            return m_lastRecvTime;
        }
        private bool m_bFirstRecv = true;
        public bool FirstRecv
        {
            get { return m_bFirstRecv; }
            set { m_bFirstRecv = value; }
        }
        private int m_handshake_state = 0;
        public int HandshakeState
        {
            get { return m_handshake_state; }
            set { m_handshake_state = value; }
        }
        override public bool sendudp(byte[] sendBuffer, int nSize)
        {
#if WYS_TEST
           Debug.Log(" testchat send udp continue..realsend:" + m_clientEP.ToString());
#endif
           // Console.WriteLine(" testchat send udp continue..realsend:" + m_clientEP.ToString());
            m_udp_srv.m_udpClient.BeginSendTo(sendBuffer, 0, nSize, SocketFlags.None, (EndPoint)m_clientEP,
                new AsyncCallback(m_udp_srv.SendCallback), m_udp_srv.m_udpClient);
            return true;
        }
        override public bool OnRecvMsg(byte[] recvMsg)
        {
            m_udp_srv.processPacket(m_nClientID, recvMsg, recvMsg.Length);
            return true;
        }
        public void SendUdp<T>(UInt64 nClientID, T ProtocolPacket, int ProtocolType) where T : IMessage
        {
            MsgCls msgcls = new MsgCls();
            msgcls.ObjectToByte<T>(ProtocolPacket, ProtocolType);
            
            SendPacket(msgcls.byteBuff, msgcls.send_num);
            return;
        }
        private IPEndPoint m_clientEP = null;
        private UInt64 m_nClientID = 0;
        private UInt64 m_clientAddrId = 0;
        private Int32 m_lastRecvTime = 0;
        private AsynUdpP2PClient m_udp_srv = null;
    }

    //客户端地址队列
    class clientaddrlist
    {
        public clientaddrlist()
        {
            m_clientListByClientId = new Dictionary<UInt64, clientAgent>();
            m_clientListByAddrId = new Dictionary<UInt64, clientAgent>();
        }
        public void pushClient(clientAgent paramAgent)//添加客户端地址
        {
            if (!m_clientListByClientId.ContainsKey(paramAgent.GetClientId()))
            {
                m_clientListByAddrId.Add(paramAgent.GetAddrId(), paramAgent);
                m_clientListByClientId.Add(paramAgent.GetClientId(), paramAgent);
            }
            else
            {
                m_clientListByClientId[paramAgent.GetClientId()] = paramAgent;
                m_clientListByAddrId[paramAgent.GetAddrId()] = paramAgent;
            }
        }
        public clientAgent GetClientByID(UInt64 nClientID)//获取客户端地址
        {
            if (m_clientListByClientId.ContainsKey(nClientID))
            {
                return m_clientListByClientId[nClientID];
            }
            return null;
        }
        public clientAgent GetClientByAddr(UInt64 nAddrID)//获取客户端地址
        {
            if (m_clientListByAddrId.ContainsKey(nAddrID))
            {
                return m_clientListByAddrId[nAddrID];
            }
            return null;
        }
        public void deleteClientByID(UInt64 nClientID)//删除客户端地址
        {
            if (m_clientListByClientId.ContainsKey(nClientID))
            {
                UInt64 addrId = m_clientListByClientId[nClientID].GetAddrId();
                m_clientListByClientId.Remove(nClientID);
                if (m_clientListByAddrId.ContainsKey(addrId))
                    m_clientListByAddrId.Remove(addrId);
            }
        }
        public void deleteClientByAddr(UInt64 nAddrID)//删除客户端地址
        {
            if (m_clientListByAddrId.ContainsKey(nAddrID))
            {
                UInt64 clientId = m_clientListByClientId[nAddrID].GetAddrId();
                m_clientListByAddrId.Remove(nAddrID);
                if (m_clientListByClientId.ContainsKey(clientId))
                    m_clientListByClientId.Remove(clientId);
            }
        }
        public bool checkExistByAddr(UInt64 nAddrID)
        {
            return m_clientListByAddrId.ContainsKey(nAddrID);
        }
        public bool checkExistById(UInt64 nClientID)
        {
            return m_clientListByClientId.ContainsKey(nClientID);
        }
        public UInt64 GetClientIdByAddr(UInt64 nAddrID)
        {
            if (m_clientListByAddrId.ContainsKey(nAddrID))
                return m_clientListByAddrId[nAddrID].GetClientId();
            return 0;
        }
        public UInt64 GetClientIdById(UInt64 nClientID)
        {
            if (m_clientListByClientId.ContainsKey(nClientID))
                return m_clientListByClientId[nClientID].GetAddrId();
            return 0;
        }
        public void clear()
        {
            m_clientListByClientId.Clear();
            m_clientListByAddrId.Clear();
        }
        public void setRecvTime(UInt64 nClientID)
        {
            if (m_clientListByClientId.ContainsKey(nClientID))
                m_clientListByClientId[nClientID].SetRecvTime();
        }
        public bool firstRecv(UInt64 nClientID)
        {
            if (m_clientListByClientId.ContainsKey(nClientID))
                return m_clientListByClientId[nClientID].FirstRecv;
            return false;
        }
        public Dictionary<UInt64, clientAgent> m_clientListByClientId = null;
        public Dictionary<UInt64, clientAgent> m_clientListByAddrId = null;
    }
    /// <summary>
    /// Udp通讯类
    /// </summary>
    public class AsynUdpP2PClient
    {
        //测试用
        //初始化 客户端端口，服务器地址
           // m_MyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_uClientBindPort);
        private IPEndPoint m_MyEndPoint = null;//测试
        private IPEndPoint m_ServerEndPoint = null;
        private IPAddress m_Lan_ipAddr = null;
        //客户端套接字
        public Socket m_udpClient = null;
        //接收信息缓冲区
        private byte[] RecvBuffer = new byte[1024*20];
        //远程终端节点
        private EndPoint m_remoteEP;
        //本地与服务器IP
        private netAddrInit m_addr = new netAddrInit();
        //最后一次收到服务器包时间
        private Int32 m_lastRecvFromSrvTime = Environment.TickCount & Int32.MaxValue;
        //所有需要p2p的地址队列
        private clientaddrlist m_clientList = new clientaddrlist();

        private const int nTimeOut = 30000; //默认10秒没有收到消息 或者ping包 就超时
        private const int nCheckTimeOutInterval = 50000; //定时器超时检测时间间隔
        private const int nPingInterval = 10000; //ping包时间间隔

        // 异步状态同步
        private ManualResetEvent sendDone = new ManualResetEvent(true);
        private ManualResetEvent receiveDone = new ManualResetEvent(true);
        private Mutex mut_clientList = new Mutex();

        //超时检测定时器
        private System.Timers.Timer check_time_ = new System.Timers.Timer(nCheckTimeOutInterval);
        //ping包定时器
        private System.Timers.Timer ping_time_ = new System.Timers.Timer(nPingInterval);

        //client iD
        private UInt32 m_AccountId = 0;
        private UInt32 m_ServerId = 0;
        private bool m_bConnectSrv = false;
        private Int32 m_lan_port = 0; //内网端口
        private Int32 m_e_port = 0;  //外网端口
        #if UNITY_PS4
        private Upnp_Mgr m_upnpMgr = null;
        #endif
        private UInt32 IpToInt(IPAddress addr)
        {
            byte[] IpList = addr.GetAddressBytes();
            if(IpList.Length != 4)
            {
                return 0;
            }
            return (UInt32)((IpList[3] << 24) + (IpList[2] << 16) + (IpList[1] << 8) + IpList[0]);
        }
        public void setConnectSrv(bool b)
        {
            m_bConnectSrv = b;
        }
        public bool GetConnectSrvStatus()
        {
            return m_bConnectSrv;
        }
        public int GetMyBindPort()
        {
            return m_MyEndPoint.Port;
        }
        public void CloseSrv()
        {
            check_time_.AutoReset = false;
            check_time_.Enabled = false;
            ping_time_.AutoReset = false;
            ping_time_.Enabled = false;
            m_bConnectSrv = false;

//            //删除upnp 静态端口映射
//            var upnpnat = new UPnPNAT();
//            var mappings = upnpnat.StaticPortMappingCollection;
//            //错误判断
//            if (mappings == null)
//            {
//                //Console.WriteLine("没有检测到路由器，或者路由器不支持UPnP功能。");
//                return;
//            }
//            try
//            {
//                IStaticPortMapping mappings1 = mappings[m_lan_port, "UDP"];
//                mappings.Remove(m_lan_port, "UDP");

//            }
//            catch
//            {

//            }

//#if UNITY_PS4
//            m_upnpMgr.RemoveMappingProt();
//#endif 
              
        }
        #region 客户端初始化
        public void InitClient(string ip, int port, int myPort, UInt32 myAccountId, UInt32 ServerId)
        {
            m_lan_port = myPort;
            int m_ex_port = myPort;
            string hostname = Dns.GetHostName();
            if (!string.IsNullOrEmpty(hostname))
            {
                IPHostEntry localhost = Dns.GetHostEntry(hostname);
                if(localhost.AddressList.Length >= 1)
                    m_Lan_ipAddr = localhost.AddressList[0];
            }

            ////创建COM类型
            //var upnpnat = new UPnPNAT();
            //var mappings = upnpnat.StaticPortMappingCollection;
            ////错误判断
            //if (mappings == null)
            //{
            //    Console.WriteLine("没有检测到路由器，或者路由器不支持UPnP功能。");
                
            //}

            //else
            //{
            //    //检测端口是否已经被映射 绑定的话端口加一 再次测试
            //    try
            //    {
            //        while (true)
            //        {

            //            IStaticPortMapping mappings1 = mappings[m_ex_port, "UDP"];
            //            //Console.WriteLine("检测到端口{0}已经绑定。。", aa);
            //            m_ex_port++;
            //        }
            //    }
            //    catch
            //    {
            //        //抛异常说明 外网端口没有被映射
            //    }
            //    string name = Dns.GetHostName();
            //    //从当前Host Name解析IP地址，筛选IPv4地址是本机的内网IP地址。
            //    m_lan_port = m_ex_port;
            //    if (!string.IsNullOrEmpty(name))
            //    {
            //        //var ipv4 = Dns.GetHostEntry(name).AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
            //        IPHostEntry ipv4 = Dns.GetHostEntry(name);
            //        if (ipv4.AddressList.Length >= 1)
            //        {
            //            IPAddress lanIp = ipv4.AddressList[0];

            //            IStaticPortMapping mapping_ = mappings.Add(m_ex_port, "UDP", m_lan_port, lanIp.ToString(), true, "WXHXMAP");
            //            if (mapping_ != null)
            //            {
            //                Console.WriteLine("my lan ip:" + m_Lan_ipAddr.ToString());
            //                //m_MyEndPoint = new IPEndPoint(localaddr, myPort);

            //                Console.WriteLine("mapping net port success .port:" + m_e_port.ToString());

            //            }
            //            else
            //            Console.WriteLine("mapping net port failed.");
            //        }
            //    }
            //}
            
          
            m_ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            m_udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            while(true)
            {
                try
                {
                    m_MyEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), m_lan_port);
                    m_udpClient.Bind(m_MyEndPoint);
                    break;
                }
                catch
                {
                    m_lan_port++;
                }
            }
            m_remoteEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));

            // m_clientList.pushClient(new clientAgent(m_addr.GetMyEP().Address.ToString(), m_addr.GetMyEP().Port, 0));
            m_clientList.pushClient(new clientAgent(ip, port, 0, this));
            m_AccountId = myAccountId;
            m_ServerId = ServerId;
            //尝试静态映射端口到外网端口 upnp
            //m_upnpMgr = new Upnp_Mgr(m_lan_port, m_lan_port, "UDP", "wxhx mapping");
            //m_e_port = m_upnpMgr.MappingNetProt();
            //m_lan_port = m_e_port;
            //myPort = m_e_port;

            //if (m_e_port == 0)
            //{
            //    Console.WriteLine("mapping net port failed.");
            //}
            //else
            //{
            //    Console.WriteLine("mapping net port success .port:" + m_e_port.ToString());
            //}
                 
        }
        #endregion

        #region UDP 启动
        public void startUdp(string ip, int port, int myPort, UInt32 myAccountId, UInt32 ServerId)
        {
            InitClient(ip, port, myPort, myAccountId, ServerId);
            if(!m_bConnectSrv)
            {
                Console.WriteLine("testchat connect chat udp ip:" + ip.ToString() + " port:" + port.ToString());
#if WYS_TEST
           Debug.Log("testchat connect chat udp ip:" + ip.ToString() + " port:" + port.ToString());
#endif
                connectPacket(0);//连接服务器
                AsynRecive(); //开始接收
            }
        }
        #endregion

        #region 请求连接
        public void connectPacket(UInt64 nClientID)
        {
            Msg_Client2Chat_UdpConnect_Req reqConnect = null;
            if (m_Lan_ipAddr == null)
                reqConnect = new Msg_Client2Chat_UdpConnect_Req(m_AccountId, m_ServerId, 0,0);
            else
                reqConnect = new Msg_Client2Chat_UdpConnect_Req(m_AccountId, m_ServerId, IpToInt(m_Lan_ipAddr),(uint) m_lan_port);
            SendUdp<Msg_Client2Chat_UdpConnect_Req>(nClientID, reqConnect, (int)common.MsgType.enum_Msg_Client2Chat_UdpConnect_Req);
        }
        public void connectPacket_ret(UInt64 nClientID)
        {
            Msg_Chat2Client_UdpConnect_Res resConnect = new Msg_Chat2Client_UdpConnect_Res();
            resConnect.m_res = (UInt32)common.LogicRes.Connect_Success;
            SendUdp<Msg_Chat2Client_UdpConnect_Res>(nClientID, resConnect, (int)common.MsgType.enum_Msg_Chat2Client_UdpConnect_Res);
        }
        #endregion

        #region 主动对其他客户端或服务器断开
        public void DisConnectPacket(UInt64 nClientID) //主动对其他客户端或服务器断开
        {
            Msg_Chat2Client_Disconnect handshakePacket = new Msg_Chat2Client_Disconnect();
            handshakePacket.m_req_account_id = m_AccountId;
            handshakePacket.m_req_server_id = m_ServerId;
            SendUdp<Msg_Chat2Client_Disconnect>(nClientID, handshakePacket, (int)common.MsgType.enum_Msg_Chat2Client_Disconnect);
        }
        #endregion

        #region 心跳包
        private void pingPacket(UInt64 nClientID) //心跳包
        {
            Msg_Ping_Req pingMsg = new Msg_Ping_Req();
            SendUdp<Msg_Ping_Req>(nClientID, pingMsg, (int)common.MsgType.enum_Msg_Ping_Req);
        }
        #endregion

        #region 异步接收来自其他终端发送的消息
        private void AsynRecive()
        {
            m_udpClient.BeginReceiveFrom(RecvBuffer, 0, RecvBuffer.Length, SocketFlags.None, ref m_remoteEP,
                new AsyncCallback(ReciveCallback), null);
            //receiveDone.WaitOne();

        }
        #endregion

        #region 异步接收来自其他终端发送的消息回调函数
        private void ReciveCallback(IAsyncResult asyncResult)
        {
            try
            {
                //信息接收完成
                if (asyncResult.IsCompleted)
                {
                    // 测试
                  // Debug.Log("UDP RECV ========== ");

                    IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint tmpIpep = (EndPoint)ipep;
                    int nRecvLen = m_udpClient.EndReceiveFrom(asyncResult, ref tmpIpep);
                    if (nRecvLen != 0)
                    {
#if WYS_TEST
            Debug.Log("UDP RECV ========== " + ((IPEndPoint)tmpIpep).ToString());
#endif
                    //    Console.WriteLine(" UDP RECV ==========" + ((IPEndPoint)tmpIpep).ToString());
                        UInt64 tmpAddrId = RetAddrId((IPEndPoint)tmpIpep);
                        if (m_clientList.checkExistByAddr(tmpAddrId))//地址存在
                        {
                            clientAgent tmpAgent = m_clientList.GetClientByAddr(tmpAddrId);

                            if (nRecvLen >= 6)
                            {
                                tmpAgent.SetRecvTime();
                                if (tmpAgent.GetClientId() != 0) 
                                    tmpAgent.recvudp(RecvBuffer, nRecvLen);
                                else//服务器的包不需要分包解包
                                    processPacket(tmpAgent.GetClientId(),RecvBuffer, nRecvLen);
                            }
                            else
                            {
#if WYS_TEST
            Debug.Log("recv one udp packet,check len error! from:" + tmpIpep.ToString());
#endif
                                Console.WriteLine("recv one udp packet,check len error! from:" + tmpIpep.ToString());
                            }
                        }
                        else
                        {
#if WYS_TEST
                            Debug.Log("recv one udp Packet,check address error!" + ((IPEndPoint)tmpIpep).ToString());
#endif
                            Console.WriteLine("recv one udp Packet,check address error!" + ((IPEndPoint)tmpIpep).ToString());
                        }
                    }
                    else
                    {
#if WYS_TEST
                        Debug.Log("client" + tmpIpep.ToString() + ":recv error");
#endif
                        Console.WriteLine("client" + tmpIpep.ToString() + ":recv error");
                    }
                }
            }
            catch (ObjectDisposedException ode)
            {
#if WYS_TEST
                Debug.Log("UDP RECV ObjectDisposedException:----" + ode.ToString());
#endif
                Console.WriteLine("UDP RECV ObjectDisposedException:----" + ode.ToString());
            }
            finally
            {
                //receiveDone.Set();
                if(m_bConnectSrv)
                    AsynRecive();
            }
        }
        #endregion

        #region 处理包
        public void processPacket(UInt64 sourId,byte[] packet, int nRecvLen)
        {
            if (nRecvLen < 2)
                return;
            try
            {
#if WYS_TEST
                Debug.Log("processPacket : sourid = " + sourId.ToString() + " ,nRecvLen = " + nRecvLen.ToString());
#endif
                if (nRecvLen > 20480)
                    Console.WriteLine("process error message  because the  packet length is too big.nRecvLen ={0}", nRecvLen);
                //解析包头
                byte[] LenByte = new byte[2];
                Buffer.BlockCopy(packet, 0, LenByte, 0, 2);
                short ProtoLen = BitConverter.ToInt16(LenByte, 0);
                ProtoLen = Endian.Switch(ProtoLen);
                //Debug.Log("processPacket :1," + nRecvLen.ToString());
                if (ProtoLen + 2 <= nRecvLen)
                {
                    byte[] ByteBuff = new byte[ProtoLen + 2];
                    Buffer.BlockCopy(packet, 0, ByteBuff, 0, ProtoLen + 2);
                    //ByteBuff.RemoveRange(0, ProtoLen + 2);
                    //Debug.Log("processPacket :2 " + nRecvLen.ToString());
                    byte[] FlagByte = new byte[4];  //协议号
                    byte[] DataByte = new byte[ByteBuff.Length - 6]; //消息内容

                    Array.Copy(ByteBuff, 2, FlagByte, 0, FlagByte.Length);
                    Array.Copy(ByteBuff, 6, DataByte, 0, DataByte.Length);

                    //消息号
                    int nMsgType = BitConverter.ToInt32(FlagByte, 0);
                    nMsgType = Endian.Switch(nMsgType);
                    //Debug.Log("processPacket :3 " + nRecvLen.ToString());
                    //解析完成，底层连接包，ping包，与断开包 直接处理
                    //请求连接包返回
                    if (nMsgType == (int)common.MsgType.enum_Msg_Client2Chat_UdpConnect_Req)
                    {
                        ConnectUdp(sourId);
                    }
                    else if (nMsgType == (int)common.MsgType.enum_Msg_Chat2Client_UdpConnect_Res)
                    {
                        clientAgent ca = m_clientList.GetClientByID(sourId);
                        if (ca != null)
                        {
                            Console.WriteLine("recv connect_Res from {0}", sourId);
                            if (ca.HandshakeState != (int)clientAgent.p2p_handshake_NextState.p2p_success)
                            {
                                ca.HandshakeState = (int)clientAgent.p2p_handshake_NextState.p2p_success;
                                if (sourId == 0) //请求服务器连接返回
                                {
                                    m_bConnectSrv = true;
                                    TimeCheckClientList();
#if WYS_TEST
                                Debug.Log("connect udp chat srv success===" );
#endif
                                }
                                OnConnect_Udp(sourId);
                            }
                        }
                        return;
                    }
                    else if (nMsgType == (int)common.MsgType.enum_Msg_Ping_Req) //其他客户端ping包
                    {
                        if (sourId != 0)
                         Console.WriteLine("UDP RECVother client ping packet----,id = " + sourId.ToString());
                        clientAgent ca = m_clientList.GetClientByID(sourId);
                        if (ca != null && ca.HandshakeState != (int)clientAgent.p2p_handshake_NextState.p2p_success)
                        {
                            ca.HandshakeState = (int)clientAgent.p2p_handshake_NextState.p2p_success;
                              OnConnect_Udp(sourId);
                        }
                        return;
                    }
                    else if (nMsgType == (int)common.MsgType.enum_Msg_Ping_Res) // 服务器 返回ping包
                    {
                        return;
                    }
                    else if (nMsgType == (int)common.MsgType.enum_Msg_Chat2Client_Disconnect) //断开包
                    {
                        CloseUdp(sourId);
                        deleteClient(sourId);
                    }
                    else
                    {
                        //其他逻辑消息包
                        RecvUdp(sourId, nMsgType, ref DataByte, DataByte.Length);
                    }

                }
            }
            catch (Exception ex_)
            {
#if WYS_TEST
                Debug.Log("parse/process Packet error===:" + ex_.ToString());
#endif
            }
           
        }
        #endregion

        #region 异步发送消息
        public void SendUdp<T>(UInt64 nClientID, T ProtocolPacket, int ProtocolType) where T : IMessage
        {
            clientAgent ca = m_clientList.GetClientByID(nClientID);
            if (ca == null)
                return;
            if (nClientID != 0)
                ca.SendUdp(nClientID, ProtocolPacket, ProtocolType);
            else//发往服务器不需要大包分发，底层包结构不变
            {
                MsgCls msgcls = new MsgCls();
                msgcls.ObjectToByte<T>(ProtocolPacket, ProtocolType);

                m_udpClient.BeginSendTo(msgcls.byteBuff, 0, msgcls.send_num, SocketFlags.None, (EndPoint)ca.GetEp(),
                    new AsyncCallback(SendCallback), m_udpClient);
            }
            ////sendDone.WaitOne();
            return;
        }
        public void SendUdp(UInt64 nClientID,byte[] packet, int nLen)
        {
            clientAgent ca = m_clientList.GetClientByID(nClientID);
            if (ca == null)
                return;
            if (nClientID != 0)
                ca.SendPacket(packet, nLen);
            else//发往服务器不需要大包分发，底层包结构不变
            {
                m_udpClient.BeginSendTo(packet, 0, nLen, SocketFlags.None, (EndPoint)ca.GetEp(),
                    new AsyncCallback(SendCallback), m_udpClient);
            }
            ////sendDone.WaitOne();
            return;
        }
        #endregion
        #region 广播消息
        public void BroadcastSendUdp<T>(List<UInt64> nClientList, T ProtocolPacket, int ProtocolType) where T : IMessage
        {
            foreach (UInt64 kv in nClientList)
            {
                clientAgent ca = m_clientList.GetClientByID(kv);
                if (ca == null)
                    return;
                MsgCls msgcls = new MsgCls();
                msgcls.ObjectToByte<T>(ProtocolPacket, ProtocolType);

                m_udpClient.BeginSendTo(msgcls.byteBuff, 0, msgcls.send_num, SocketFlags.None, ca.GetEp(),
                    new AsyncCallback(SendCallback), null);
                sendDone.WaitOne();
            }
            return;
        }
        #endregion
        #region 异步发送消息回调函数
        public void SendCallback(IAsyncResult asyncResult)
        {
            //消息发送完成
            try
            {
                if (asyncResult.IsCompleted)
                {
                    int sendLen = m_udpClient.EndSendTo(asyncResult);
                }
            }
            catch (SocketException ode)
            {
#if WYS_TEST
                Debug.Log("UDP Send SocketException:----" + ode.ToString());
                int nErrorCode = ode.ErrorCode;
                Debug.Log("UDP Send errorCode:" + nErrorCode.ToString());
#endif
            }
            finally
            {
                //sendDone.Set();
            }
           
        }
        #endregion
        //ip 转 uint64
        private UInt64 RetAddrId(IPEndPoint ip)
        {
            UInt64 tmpAddr = (UInt64)ip.Address.GetHashCode();
            tmpAddr <<= 32;
            tmpAddr |= (UInt32)ip.Port;
            return tmpAddr;
        }

        #region 解析包结构
        public static void parsePacket<T>(ref T packet_, ref byte[] buffer, int nSize) where T : IMessage, new()
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(buffer, 0, nSize);
            ms.Seek(0, SeekOrigin.Begin);
            packet_ = common.Serializer.Deserialize<T>(ms, packet_);
        }
        #endregion

        #region 收包回调
        virtual public void RecvUdp(UInt64 nClientID, int nMsgType, ref byte[] buffer, int nSize)
        {

        }
        #endregion

        #region 客户端关闭回调
        virtual public void CloseUdp(UInt64 nClientID)
        {

        }
        #endregion
        #region 客户端连接回调
        virtual public void OnConnect_Udp(UInt64 nClientID)
        {

        }
        #endregion
        private void ConnectUdp(UInt64 nClientID)
        {
            clientAgent ca = m_clientList.GetClientByID(nClientID);
            if (ca != null)
            {
                Console.WriteLine("recv connect from {0}", nClientID);
                if (nClientID != 0) //如果不是服务器返回 则再次发送返回确认信息
                {
                    connectPacket_ret(nClientID);
                }     
                if (ca.HandshakeState != (int)clientAgent.p2p_handshake_NextState.p2p_success)
                {
                    ca.HandshakeState = (int)clientAgent.p2p_handshake_NextState.p2p_success;                           
                    OnConnect_Udp(nClientID);
                }
            }

        }

        #region 启动定时器
        private void TimeCheckClientList()
        {
            check_time_.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeCheckClient);
            check_time_.AutoReset = true;
            check_time_.Enabled = true;
            ping_time_.Elapsed += new System.Timers.ElapsedEventHandler(OnTimePing);
            ping_time_.AutoReset = true;
            ping_time_.Enabled = true;
        }
        #endregion
        #region 定时检测超时
        private void OnTimeCheckClient(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!m_bConnectSrv)
            {
                check_time_.AutoReset = false;
                check_time_.Enabled = false;
                return;
            }
            try
            {
                List<UInt64> TmpDeleteList = new List<UInt64>();
                int nCurTime = Environment.TickCount & Int32.MaxValue;//当前ticket time

                if (mut_clientList.WaitOne(500))
                {
                    try
                    {
                        foreach (KeyValuePair<UInt64, clientAgent> kv in m_clientList.m_clientListByClientId)
                        {
                            if (kv.Value == null)
                            {
#if WYS_TEST
                                Debug.Log("check timeout error cannot found client data by clientID:" + kv.ToString());
#endif
                                continue;
                            }
                            if (kv.Value.HandshakeState == (int)clientAgent.p2p_handshake_NextState.p2p_success && (nCurTime - kv.Value.GetLastRecvTime()) > nTimeOut)
                            {
                                Console.WriteLine("nTimeOut == {0}", (nCurTime - kv.Value.GetLastRecvTime()));
                                //超时 通知逻辑处理
                                CloseUdp(kv.Key);

                                //用户key 添加到删除队列，
                                //TmpDeleteList.Add(kv.Key);
                                kv.Value.HandshakeState = (int)clientAgent.p2p_handshake_NextState.p2p_failed;
                                if (kv.Key == 0) //服务器的id
                                {
                                    m_bConnectSrv = false;
                                }
                            }
                        }
                        mut_clientList.ReleaseMutex();
                    }
                    catch
                    {
                        mut_clientList.ReleaseMutex();
                    }
                }
                //删除客户端缓存地址
                //foreach (UInt64 key in TmpDeleteList)
                //{
                //    m_clientList.deleteClientByID(key);
                //}
            }
            catch (Exception ex_)
            {
#if WYS_TEST
                Debug.Log("udp check timeout exceptional!");
                Debug.Log("check timeout error===:" + ex_.ToString());
#endif
            }
        }
        #endregion
        #region 定时发ping包
        private void OnTimePing(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!m_bConnectSrv)
            {
                ping_time_.AutoReset = false;
                ping_time_.Enabled = false;
                return;
            }
            try
            {
                if (mut_clientList.WaitOne(500))
                {
                    try
                    {
                        foreach (KeyValuePair<UInt64, clientAgent> kv in m_clientList.m_clientListByClientId)
                        {
                            if (kv.Value == null)
                            {
#if WYS_TEST
                                Debug.Log("send ping packet error. cannot found Address by clientID:" + kv.ToString());
#endif
                                continue;
                            }

                            if (kv.Value.HandshakeState == (int)clientAgent.p2p_handshake_NextState.p2p_success)
                                pingPacket(kv.Key);
                        }
                        mut_clientList.ReleaseMutex();
                    }
                    catch
                    {
                        mut_clientList.ReleaseMutex();
                    }
                }

//                foreach (KeyValuePair<UInt64, clientAgent> kv in m_clientList.m_clientListByClientId)
//                {
//                    if (kv.Value == null)
//                    {
//#if WYS_TEST
//                        Debug.Log("send ping packet error. cannot found Address by clientID:" + kv.ToString());
//#endif
//                        continue;
//                    }
                        
//                    if (kv.Value.HandshakeState == (int)clientAgent.p2p_handshake_NextState.p2p_affirm)
//                        pingPacket(kv.Key);
//                }
            }
            catch (Exception ex_)
            {
#if WYS_TEST
                Debug.Log("udp ping exceptional!");
                Debug.Log("ping error===:" + ex_.ToString());
#endif
            }
        }
        #endregion
        #region 添加客户端地址
        public void addClient(UInt64 nClientId, string ip, int nPort)
        {
            if (mut_clientList.WaitOne(500))
            {
                try
                {
                    m_clientList.pushClient(new clientAgent(ip, nPort, nClientId,this));
                    mut_clientList.ReleaseMutex();
                }
                catch
                {
                    mut_clientList.ReleaseMutex();
                }
            }
            else
            {
#if WYS_TEST
                Debug.Log("udp add client deadlock!");
#endif
            }
        }
        #endregion

        #region 删除客户端
        public void deleteClient(UInt64 nClientId)
        {
            if (mut_clientList.WaitOne(500))
            {
                try
                {
                    m_clientList.deleteClientByID(nClientId);
                    mut_clientList.ReleaseMutex();
                }
                catch
                {
                    mut_clientList.ReleaseMutex();
                }
            }
            else
            {
#if WYS_TEST
                Debug.Log("udp add client deadlock!");
#endif
            }
        }
        #endregion
    }
}
