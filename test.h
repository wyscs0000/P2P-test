#include <iostream>
#include <string>
#include <WinSock2.h>
#pragma comment(lib,"ws2_32.lib")

struct stIpEndPoint
{
	stIpEndPoint() :m_nIp(0), m_nPort(0)
	{
	}
	stIpEndPoint(unsigned int nIp, int nPort)
	{
		m_nIp = nIp;
		m_nPort = nPort;
	}
	stIpEndPoint(std::string sIp, int nPort)
	{
		initIp(sIp, nPort);
	}
	void initIp(std::string sIp, int nPort)
	{
		m_nPort = nPort;

		int nIp[4];
		std::string strTemp;
		size_t pos;
		size_t i = 3;
		do  {
			pos = sIp.find(".");
			if (pos != std::string::npos)   {
				strTemp = sIp.substr(0, pos);
				nIp[i] = atoi(strTemp.c_str());
				i--;
				sIp.erase(0, pos + 1);
			}
			else   {
				strTemp = sIp;
				nIp[i] = atoi(strTemp.c_str());
				break;
			}
		} while (i >= 0);

		m_nIp = (nIp[3] << 24) + (nIp[2] << 16) + (nIp[1] << 8) + nIp[0];

	}
	std::string IpToString()
	{
		char tmpIp[20] = { '\0' };
		sprintf_s(tmpIp, 20, "%d.%d.%d.%d", m_nIp & 0x000000ff, (m_nIp & 0x0000ff00) >> 8, (m_nIp & 0x00ff0000) >> 16, (m_nIp & 0xff000000) >> 24);
		return std::string(tmpIp);
	}
	static std::string IpToString(unsigned int nIp)
	{
		char tmpIp[20] = { '\0' };
		/*sprintf_s(tmpIp, 20, "%d.%d.%d.%d", nIp >> 24, (8 << nIp) >> 24, (16 << nIp) >> 24, (24 << nIp) >> 24);*/
		sprintf_s(tmpIp, 20, "%d.%d.%d.%d", nIp & 0x000000ff , (nIp & 0x0000ff00) >> 8, (nIp & 0x00ff0000) >> 16,(nIp & 0xff000000) >> 24 );
		return std::string(tmpIp);
	}
	static unsigned int IpToInt(std::string sIp)
	{
		int nIp[4];
		std::string strTemp;
		size_t pos;
		size_t i = 3;
		do  {
			pos = sIp.find(".");
			if (pos != std::string::npos)   {
				strTemp = sIp.substr(0, pos);
				nIp[i] = atoi(strTemp.c_str());
				i--;
				sIp.erase(0, pos + 1);
			}
			else   {
				strTemp = sIp;
				nIp[i] = atoi(strTemp.c_str());
				break;
			}
		} while (i >= 0);

		int tmpIp = (nIp[0] << 24) + (nIp[1] << 16) + (nIp[2] << 8) + nIp[3];
		return tmpIp;
	}
	int PortToNet()
	{
		return htonl(m_nPort);
	}

	unsigned int m_nIp;
	int m_nPort;
};

class netAddrInit
{
public:
	netAddrInit()
	{
		m_nClientBindPort = 33389;
		m_ServerEndPoint.initIp("127.0.0.1", 10501);

	}
	stIpEndPoint GetSrvEP()
	{
		return m_ServerEndPoint;
	}
	stIpEndPoint GetMyEP()
	{
		return m_MyEndPoint;
	}
private:
	stIpEndPoint m_MyEndPoint;
	stIpEndPoint m_ServerEndPoint;
	int				m_nClientBindPort;
};

class CtestNet
{
public:
	CtestNet()
	{

	}
	~CtestNet()
	{

	}
	void initSock()
	{
		
	}


private:
	netAddrInit m_addr;
	SOCKET		m_socket;
};
