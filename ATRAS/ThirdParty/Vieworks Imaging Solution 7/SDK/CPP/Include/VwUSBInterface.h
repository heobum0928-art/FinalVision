#pragma once

#ifndef VWINTERFACE_H
#define VWINTERFACE_H

// #ifdef USE_TLI_V7
// #include "GenTL_v1_3.h"
// #else
// #include "GenTL.V1.2.h"
// #endif

#include "VwUSB.Global.h" 
#include "VwUSBCamera.h"
#include <list>
#include <map>
#include <string>

struct INFO_DATATYPE;

// Needs to have dll-interface to be used by clients of class 'VwInterface'
#pragma warning ( disable : 4251 )

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

using namespace std;

namespace VWSDK
{

	// forward declaration
	class VwUSBCamera;
	class VwUSB;

	//+++++++++++++++++++++++++++++++++++++++++++++++
	// class VwInterface
	//+++++++++++++++++++++++++++++++++++++++++++++++
	class CLASS_IMPORTEXPORT VwUSBInterface
	{
	protected:
		VwGenICam::TL_HANDLE	hTL;
		VwGenICam::IF_HANDLE	hInterface;
		map<string, VwUSBCamera*>	vwCameraList;

		char		caName[128];
		VwUSB*		m_pVwUSB;

		UINT		m_unFilterType;
		BOOL		_bUseQueueManager;
		CCP_PRIVILEGE	_eCCPPrivilege;

	protected:
		VwUSBInterface(void);

	public:
		VwUSBInterface(VwGenICam::TL_HANDLE _hTL, UINT unFilterType);
		~VwUSBInterface(void);

	public:
		VwGenICam::IF_HANDLE GetInterfaceHandle()	{ return hInterface; }
		std::map<std::string, VwUSBCamera*>* GetCameraList()	{ return &vwCameraList; }
		VWSDK::RESULT GetVwUSB(VwUSB* pVwUSB);
		VWSDK::RESULT OpenInterface(char* name, IN VwUSB* pVwUSB);
		VWSDK::RESULT CloseInterface();
		VWSDK::RESULT RemoveCameraList();
		VWSDK::RESULT GetNumCameras(UINT* pNumDevices);
		VWSDK::RESULT GetCameraName(UINT unDevIndex, char* pcaName, size_t* pcbNameSize);
		VWSDK::RESULT GetCameraID(UINT unDevIndex, char* pcaID, size_t* pcbNameSize);
		VWSDK::RESULT OpenCamera(char* name, VwUSBCamera** ppVwCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight, UINT nPacketSize,
			void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT OpenCamera(UINT nIndex, VwUSBCamera** ppVwCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight, UINT nPacketSize,
			void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT GetInterfaceInfo(const char* sIfaceID, VwGenICam::INTERFACE_INFO_CMD_LIST iInfoCmd, VwGenICam::INFO_DATATYPE* piType, void* pBuffer, size_t* piSize);

		VWSDK::RESULT GetFilterType(UINT* unpFilterType);
		VWSDK::RESULT SetFilterType(UINT unFilterType);

		VWSDK::RESULT UseQueueManager(BOOL bUse)						{ _bUseQueueManager = bUse;				return VWSDK::RESULT_SUCCESS; };
		VWSDK::RESULT SetCCPPrivilege(IN CCP_PRIVILEGE eCCPPrivilege)	{ _eCCPPrivilege = eCCPPrivilege;		return VWSDK::RESULT_SUCCESS; };
	};
}

#endif