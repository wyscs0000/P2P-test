#include "test.h"
#include "CommonLogicStruct.pb.h"
#include "CommonLogicMsg.pb.h"
#include "CommonMsgType.pb.h"

#include<Upnphost.h>

using namespace common;
using namespace common::ffd;
using namespace common::ffd::msgtype;
using namespace common::ffd::logic;
int main()
{
	int nAccountID = 0;
	printf("input account id:");
	scanf("%d", &nAccountID);

	WORD wVersionRequested;
	WSADATA wsaData;

	int err;

	wVersionRequested = MAKEWORD(2, 2);

	err = WSAStartup(wVersionRequested, &wsaData);

	if (err != 0)
	{
		return 0 ;
	}

	if (LOBYTE(wsaData.wVersion) != 2 || HIBYTE(wsaData.wVersion) != 2)
	{
		WSACleanup();
		return 0;
	}

	SOCKET sockClient = socket(AF_INET, SOCK_DGRAM, 0);
	SOCKADDR_IN myaddr;
	
	myaddr.sin_addr.S_un.S_addr = inet_addr("0.0.0.0");
	myaddr.sin_family = AF_INET;
	myaddr.sin_port = htons(33389);

	bind(sockClient, (sockaddr*)&myaddr, sizeof(myaddr));

	SOCKADDR_IN addrSrv;
	/*addrSrv.sin_addr.S_un.S_addr = inet_addr("120.132.83.168");
	addrSrv.sin_port = htons(10501);*/
	addrSrv.sin_addr.S_un.S_addr = inet_addr("192.168.0.100");
	addrSrv.sin_port = htons(10503);
	addrSrv.sin_family = AF_INET;
	

	
	//连接服务器
	Msg_Client2Chat_UdpConnect_Req resConnect;
	resConnect.set_m_account_id(nAccountID);
	resConnect.set_m_server_id(1);
	resConnect.set_m_ip_lan(0);
	resConnect.set_m_port_lan(0);

	char buf[1024] = {'\0'};
	int nMsgLen = resConnect.ByteSize();
	short msgLen = 4 + nMsgLen;
	int nMsgType = enum_Msg_Client2Chat_UdpConnect_Req;
	nMsgType = htonl(nMsgType);
	int netMsgLen = htons(msgLen);
	memcpy(buf, &netMsgLen, 2);
	memcpy(buf + 2, &nMsgType, 4);
	resConnect.SerializeToArray(buf + 6, nMsgLen);
	sendto(sockClient, buf, msgLen + 2, 0, (sockaddr*)&addrSrv, sizeof(addrSrv));
	printf("connect to srv..\n");
	char recvBuffer[1024] = { '\0' };
	
	while (1)
	{
		SOCKADDR_IN addrOtherClient;
		int nOtherAddr = sizeof(addrOtherClient);
		int nRecvLen = recvfrom(sockClient, recvBuffer, 1024, 0, (sockaddr*)&addrOtherClient, &nOtherAddr);
		if (nRecvLen > 0)
		{
			short nLen;
			int nMsgType_ = 0;
			/*if (addrOtherClient.sin_addr.S_un.S_addr == stIpEndPoint::IpToInt("120.132.83.168"))*/
			if (addrOtherClient.sin_addr.S_un.S_addr == stIpEndPoint::IpToInt("192.168.0.100"))
			{
				memcpy(&nLen, recvBuffer, 2);
				nLen = ntohs(nLen);				
				memcpy(&nMsgType_, recvBuffer + 2, 4);
				nMsgType_ = ntohl(nMsgType_);
			}
			else
			{
				memcpy(&nLen, recvBuffer+9, 2);
				nLen = ntohs(nLen);
				memcpy(&nMsgType_, recvBuffer + 2+9, 4);
				nMsgType_ = ntohl(nMsgType_);
			}
			printf("recv one msg. nLen = %d..msgType = %d,ip = %s\n", nLen, nMsgType_, (stIpEndPoint::IpToString(addrOtherClient.sin_addr.S_un.S_addr)).c_str());


			if (addrOtherClient.sin_addr.S_un.S_addr == stIpEndPoint::IpToInt("192.168.0.100"))
			/*if (addrOtherClient.sin_addr.S_un.S_addr == stIpEndPoint::IpToInt("120.132.83.168"))*/
			{
				printf("服务器连接成功.\n");
				if (enum_Msg_Chat2Client_TeamMember_IpPort_Res == nMsgType_)
				{
					/*printf(" input everyone  continue.\n");
					int ne;
					scanf("%d",&ne);*/
					Msg_Chat2Client_TeamMember_IpPort_Res resPacket;
					resPacket.ParseFromArray(recvBuffer + 6, nRecvLen - 6);
					for (int i = 0; i < resPacket.m_addrlist_size(); i++)
					{
						const ClientAddr tmpAddr = resPacket.m_addrlist(i);
						if (nAccountID == tmpAddr.m_accountid())
							continue;
						printf("req p2p....ip:%s,port:%d\n", tmpAddr.m_clientip().c_str(), tmpAddr.m_clientport());
						SOCKADDR_IN addrOther;
						addrOther.sin_addr.S_un.S_addr = inet_addr(tmpAddr.m_clientip().c_str());
						addrOther.sin_family = AF_INET;
						addrOther.sin_port = htons((short)tmpAddr.m_clientport());

						char sendBuf[1024] = {'\0'};
						short nnLen = 9 + msgLen + 2;
						nnLen = htons(nnLen);
						memcpy(sendBuf, &nnLen, 2);

						int 	nIdentifyCode = 0;
						nIdentifyCode = htonl(nIdentifyCode);
						memcpy(sendBuf + 2, &nIdentifyCode, 4);

						short nPacketIndex = 1;
						nPacketIndex = htons(nPacketIndex);
						memcpy(sendBuf + 6, &nPacketIndex, 2);

						
						sendBuf[8] = 0; //包类型
						
						memcpy(sendBuf + 9, buf, msgLen + 2);

						/*sendto(sockClient, buf, msgLen + 2, 0, (sockaddr*)&addrOther, sizeof(addrOther));*/
						sendto(sockClient, sendBuf, ntohs(nnLen), 0, (sockaddr*)&addrOther, sizeof(addrOther));
					}
				}
			}
			else
			{
				printf("ip ！= 服务器.\n");
				if (enum_Msg_Client2Chat_UdpConnect_Req == nMsgType_)
				{
					printf("recv enum_Msg_Client2Chat_UdpConnect_Req.\n");
					Msg_Chat2Client_UdpConnect_Res resConnect;
					resConnect.set_m_res(Connect_Success);
					char buf[1024] = { '\0' };
					int nMsgLen = resConnect.ByteSize();
					short msgLen = 4 + nMsgLen;
					int nMsgType = enum_Msg_Chat2Client_UdpConnect_Res;
					nMsgType = htonl(nMsgType);
					int netMsgLen = htons(msgLen);
					memcpy(buf, &netMsgLen, 2);
					memcpy(buf + 2, &nMsgType, 4);
					resConnect.SerializeToArray(buf + 6, nMsgLen);

					char sendBuf[1024] = { '\0' };
					short nnLen = 9 + msgLen + 2;
					nnLen = htons(nnLen);
					memcpy(sendBuf, &nnLen, 2);

					int 	nIdentifyCode = 0;
					nIdentifyCode = htonl(nIdentifyCode);
					memcpy(sendBuf + 2, &nIdentifyCode, 4);

					short nPacketIndex = 1;
					nPacketIndex = htons(nPacketIndex);
					memcpy(sendBuf + 6, &nPacketIndex, 2);


					sendBuf[8] = 0; //包类型

					memcpy(sendBuf + 9, buf, msgLen + 2);

					sendto(sockClient, sendBuf, ntohs(nnLen), 0, (sockaddr*)&addrOtherClient, sizeof(addrOtherClient));

				}
				if (enum_Msg_Chat2Client_UdpConnect_Res == nMsgType_)
				{
					printf("recv enum_Msg_Chat2Client_UdpConnect_Res.\n");
				}
			}
			
		}
		Sleep(10);
	}
	


	return 0;
}