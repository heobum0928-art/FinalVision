#pragma once
#ifdef USE_TLI_V7

#include <GenApi\GenApi.h>
#include <GenTL_v1_3.h>

namespace VWSDK
{

	class VwCXPPort : public GenApi::IPort
	{
	public:
		VwCXPPort(GenICam::Client::PORT_HANDLE hPort);
		virtual      ~VwCXPPort(void);
		void  Read(void *pBuffer, int64_t Address, int64_t Length);
		void  Write(const void *pBuffer, int64_t Address, int64_t Length);
		GenApi::EAccessMode GetAccessMode(void) const;

	private:
		GenICam::Client::PORT_HANDLE   m_hPort;
	};
}

#endif	//USE_TLI_V7