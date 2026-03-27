// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the VWCXP_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// VWCXP_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
// #ifdef VWCXP_EXPORTS
// #define VWCXP_API __declspec(dllexport)
// #else
// #define VWCXP_API __declspec(dllimport)
// #endif
// 
// // This class is exported from the VwCXP.dll
// class VWCXP_API CVwCXP {
// public:
// 	CVwCXP(void);
// 	// TODO: add your methods here.
// };
// 
// extern VWCXP_API int nVwCXP;
// 
// VWCXP_API int fnVwCXP(void);





#pragma once
#include "VwCXP.Global.h"



#include <list>



#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

using namespace std;
using namespace GenICam;
using namespace GenICam::Client;


namespace VWSDK
{



	class VwCXPGrabberBoard;

#define CXP_IF_MAX		4	

	class CLASS_IMPORTEXPORT VwCXP
	{
	private:
		TL_HANDLE					m_hTL;
		IF_HANDLE					m_hInterface[CXP_IF_MAX];

		char*						m_pszInterfaceID;
		char*						m_pszDisplayName;

		list<VwCXPGrabberBoard*>	m_listGrabberBoard;

	public:
		VwCXP(void);
		virtual ~VwCXP(void);

		RESULT Discovery();

		RESULT OpenCamera(VwCXPGrabberBoard* pGrabberBoard);

		RESULT GetGrabberBoardCount(UINT& _grabberCount);
		RESULT GetGrabberBoard(VwCXPGrabberBoard** _pGrabberBoard, UINT _grabberIndex);

		RESULT UserLogging(IN TCHAR* pModuleName, IN TCHAR* pFileName, IN TCHAR* pFuncName, IN UINT unLine, IN TCHAR* pLogMsg);


	private:
		RESULT _DiscoveryGrabberBoard(UINT _interfaceIndex);

		RESULT _ResetGrabberBoard();
	};

	class VWCAMERA_INFO
	{
	public:
		BOOL				error;
		RESULT				errorResult;
		UINT				index;
		char				name[256];
		char				vendor[128];
		char				model[128];
		char				ip[32];
		char				mac[32];

	public:
		VWCAMERA_INFO()
		{
			error = FALSE;
			errorResult = RESULT_LAST;
			index = 0;
			memset(name, 0, sizeof(name));
			memset(vendor, 0, sizeof(vendor));
			memset(model, 0, sizeof(model));
			memset(vendor, 0, sizeof(vendor));
			memset(ip, 0, sizeof(ip));
			memset(mac, 0, sizeof(mac));
		}
	};

	class CLASS_IMPORTEXPORT VWINTERFACE_INFO
	{
	public:
		BOOL				error;
		RESULT				errorCause;
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
}//namespace VWSDK

#ifndef SAFE_DELETE
#define SAFE_DELETE(p)	if(p){ delete (p); (p) = NULL;} 
#endif //SAFE_DELETE

#ifndef SAFE_DELETE_ARRAY
#define SAFE_DELETE_ARRAY(p)	if(p){ delete[] (p); (p) = NULL;}
#endif //SAFE_DELETE_ARRAY

