#pragma once 

//#include <Client.h>

#include "VwCL.Global.h"

// #include "VwInterface.h"		// remove dependency
// #include "VwCamera.h"		// remove dependency

#include <list>
#include <map>

#ifdef _USRDLL
	#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
	#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

// Needs to have dll-interface to be used by clients of class 'VwInterface'
#pragma warning ( disable : 4251 )

using namespace std;

namespace VWSDK
{

	class VwCLInterface;
	class VwCLCamera;

	namespace CL
	{

		class VWCAMERA_INFO
		{
		public:
			BOOL				error;
			VWSDK::RESULT				errorResult;
			UINT				index;
			// CAMERA_HANDLE	hCam;
			char				name[256];
			char				vendor[128];
			char				model[128];
			char				ip[32];
			char				mac[32];

		public:
			VWCAMERA_INFO()
			{
				error = FALSE;
				errorResult = VWSDK::RESULT_LAST;
				index = 0;
				// hCam = NULL;
				memset(name, 0, sizeof(name));
				memset(vendor, 0, sizeof(vendor));
				memset(model, 0, sizeof(model));
				// Add::2014721, 김은표, 초기화 코드 추가
				memset(vendor, 0, sizeof(vendor));
				memset(ip, 0, sizeof(ip));
				memset(mac, 0, sizeof(mac));
			}
		};

		class CLASS_IMPORTEXPORT VWINTERFACE_INFO
		{
		public:
			BOOL				error;
			VWSDK::RESULT				errorCause;
			UINT				index;
			char				name[256];
			list<VWCAMERA_INFO*>	cameraInfoList;
		public:
			VWINTERFACE_INFO()
			{
				error = FALSE;
				index = 0;
				memset(name, 0, sizeof(name));
				cameraInfoList.clear();
			}
			~VWINTERFACE_INFO()
			{
				if (cameraInfoList.size() == 0)
					return;

				for (list<VWCAMERA_INFO*>::iterator iterCam = cameraInfoList.begin(); iterCam != cameraInfoList.end();)
				{
					if ((*iterCam) != NULL)
					{
						delete (*iterCam);
						(*iterCam) = NULL;
						iterCam = cameraInfoList.erase(iterCam);
					}
					else
					{
						iterCam++;
					}
				}
			}

			void AddCameraInfo(VWCAMERA_INFO* pCameraInfo)
			{
				cameraInfoList.push_back(pCameraInfo);
			}
		};


		class DISCOVERY_INFO
		{
		public:
			DISCOVERY_INFO()
			{

			}
			~DISCOVERY_INFO()
			{
			}
		public:
			list<VWINTERFACE_INFO*>	interfaceInfoList;
		public:
			VWINTERFACE_INFO*	GetInterfaceInfo(UINT nIndex)
			{
				UINT ifCount = 0;
				list<VWINTERFACE_INFO*>::iterator ifInfoIter;
				for (ifInfoIter = interfaceInfoList.begin(); ifInfoIter != interfaceInfoList.end(); ifInfoIter++)
				{
					if (nIndex == ifCount)
					{
						VWINTERFACE_INFO** pInterfaceInfo = (&(*ifInfoIter));
						return *pInterfaceInfo;
					}
					ifCount++;
				}
				return NULL;
			}
			UINT GetNumInterfaces()
			{
				return (UINT)interfaceInfoList.size();
			}

			void Clear()
			{
				for (list<VWINTERFACE_INFO*>::iterator iterDis = interfaceInfoList.begin(); iterDis != interfaceInfoList.end();)
				{
					if ((*iterDis) != NULL)
					{
						VWINTERFACE_INFO* pInterfaceInfo = (VWINTERFACE_INFO*)(*iterDis);
						if (pInterfaceInfo)
						{
							/*
							for(list<VWCAMERA_INFO*>::iterator iterCam = pInterfaceInfo->cameraInfoList.begin(); iterCam != pInterfaceInfo->cameraInfoList.end(); )
							{
							if ( (*iterCam) != NULL )
							{
							delete (*iterCam);
							iterCam = pInterfaceInfo->cameraInfoList.erase( iterCam );

							}
							else
							{
							iterCam ++;
							}
							}
							*/
							delete pInterfaceInfo;
							iterDis = interfaceInfoList.erase(iterDis);

						}
					}
					else
					{
						iterDis++;
					}
				}
			}
		};
	}


	//+++++++++++++++++++++++++++++++++++++++++++++++
	// class VwGigE
	//+++++++++++++++++++++++++++++++++++++++++++++++
	class CLASS_IMPORTEXPORT VwCL
	{
	private:
		VwGenICam::TL_HANDLE		hTL;					// System handle for GigaLinx
		VWSDK::CL::DISCOVERY_INFO	discoveryInfo;			// After calling Discovery() 
		BOOL			isDiscovered;			// Discover completed?
		std::map<string, VwCLInterface*>	vwInterfaceList;

		DWORD			_dwMultiCastAddress;
		bool			_bIgnoreSubnet;

		INT64			_nDiscoveryTimeout;

		BOOL			_bUseQueueManager;
		CCP_PRIVILEGE	_eCCPPrivilege;
		
	public:
		VwCL();
		~VwCL();

		// FUNCTION Open() --------------------------------------
		// 1. Description 
		// (1) Open hTL for GenTL TL_HANDLE
		// (2) Class relationship is
		// VwGigE (Wrapping TL_HANDLE) -> VwInterface (Wrapping IF_HANDLE) -> VwCamera (Wrapping DEV_HANDLE)
		// (3) It must be called to use VwGigE
		// 2. Parameters
		// None
		// 3. Return 
		// VWSDK::RESULT
		// 4. Activity
		// (1) Open TL_HANDLE
		// (2) If discovery didn't called, it calls discovery function
		VWSDK::RESULT Open();

		// FUNCTION Open() --------------------------------------
		// 1. Description 
		// (1) Close hTL for GenTL TL_HANDLE
		// (2) Close all of the interfaces and delete VwInterface objects
		// (3) Close all of the cameras and delete VwCamera objects
		// 2. Parameters
		// None
		// 3. Return 
		// VWSDK::RESULT
		// 4. Activity
		// (1) Delete discovery info.
		VWSDK::RESULT Close();

		// Get the number of cameras connected
		VWSDK::RESULT Discovery();
		VWSDK::RESULT GetDiscoveryInfo(OUT VWSDK::CL::DISCOVERY_INFO& pDiscoveryInfo);

		VWSDK::RESULT GetNumCameras(UINT* pNumCameras);
		VWSDK::RESULT GetNumCameras(IN UINT _nInterfaceIndex, OUT UINT* _pNumCameras);
		VWSDK::RESULT GetNumCameras(IN char* pInterfaceName, OUT UINT* pNumCameras);

		VWSDK::RESULT GetNumInterfaces(UINT* pNumInterfaces);
		VWSDK::RESULT OpenInterface(UINT nIndex, VwCLInterface** pVwInterface);
		VWSDK::RESULT OpenInterface(char* name, VwCLInterface** ppVwInterface);
		VWSDK::RESULT CloseCamera(VwCLCamera* pVwCamera);
		VWSDK::RESULT OpenCamera(UINT nCameraIndex, OUT VwCLCamera** ppCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight, UINT nPacketSize,
			void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT OpenCamera(char* pCameraName, OUT VwCLCamera** ppCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight, UINT nPacketSize,
			void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT OpenCamera(char* pInterfaceName, UINT nCameraIndex, OUT VwCLCamera** ppCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
			UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn,
			DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT GetCameraInfo(IN UINT nIndex, OUT VWSDK::CL::VWCAMERA_INFO* pCameraInfo);
		VWSDK::RESULT GetCameraInfo(IN char* pCameraName, OUT VWSDK::CL::VWCAMERA_INFO* pCameraInfo);
		VWSDK::RESULT GetInterfaceInfo(IN UINT nIndex, OUT VWSDK::CL::VWINTERFACE_INFO* pInterfaceInfo);
		VWSDK::RESULT GetInterfaceInfo(IN char* pInterfaceName, OUT VWSDK::CL::VWINTERFACE_INFO* pInterfaceInfo);

		
		VWSDK::RESULT SetDiscoveryTimeout(IN UINT uTimeOut)				{ _nDiscoveryTimeout = uTimeOut;				return VWSDK::RESULT_SUCCESS; };
		VWSDK::RESULT GetDiscoveryTimeout(OUT UINT& uTimeOut)			{ uTimeOut = _nDiscoveryTimeout;	return VWSDK::RESULT_SUCCESS; };

		VWSDK::RESULT UseQueueManager(IN BOOL bUse)						{ _bUseQueueManager = bUse;				return VWSDK::RESULT_SUCCESS; };
		VWSDK::RESULT SetCCPPrivilege(IN CCP_PRIVILEGE eCCPPrivilege)	{ _eCCPPrivilege = eCCPPrivilege;		return VWSDK::RESULT_SUCCESS; };

		VWSDK::RESULT UserLogging(IN TCHAR* pModuleName, IN TCHAR* pFileName, IN TCHAR* pFuncName, IN UINT unLine, IN TCHAR* pLogMsg);

	};
}