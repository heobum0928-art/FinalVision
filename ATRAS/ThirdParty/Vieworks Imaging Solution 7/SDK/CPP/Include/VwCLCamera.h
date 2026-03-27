#pragma once

#ifndef VWCAMERA_H
#define VWCAMERA_H

#include "VwCL.Global.h"
#include "VwCLInterface.h"
#include "VwSerial.h"
#include <string.h>
#include <list>

#pragma warning (disable:4251)

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

#ifndef DEPRECATED_GET_SET_FUNCTION
#define DEPRECATED_GET_SET_FUNCTION		__declspec(deprecated("** This function or variable is deprecated. Consider using VWSDK::VwCamera::SetCustomCommand or VWSDK::VwCamera::GetCustomCommand instead. To disable deprecation, use _CRT_SECURE_NO_DEPRECATE. **"))
#endif

#if 1
	#ifdef USE_GENICAM_3_0_NAMESPACE
		#include "GenApi/GenApiNamespace.h"
	#else
		#define GENAPI_NAMESPACE GenApi
	#endif

	namespace GENAPI_NAMESPACE
#else
	namespace GenApi
#endif
{
	class CNodeMapRef;
	struct INode;
}

//using namespace std;

namespace VWSDK
{
	// Forward declaration
	class VwCLInterface;
	class MyPort;

	//+++++++++++++++++++++++++++++++++++++++++++++++
	// class VwCamera
	//+++++++++++++++++++++++++++++++++++++++++++++++
	class CLASS_IMPORTEXPORT VwCLCamera
	{
	protected:
		VwCLInterface*		pInterface;
		VwGenICam::DEV_HANDLE			hCamera;
		VwGenICam::DS_HANDLE			hStream;
		string				xml;						// XML red from the camera
		GenApi::CNodeMapRef*		pNodeMap;					// Node map from XML to R/W camera parameters
		MyPort*				port;

		VwSerial			m_Serial;

		BOOL				m_bGrabRunning;			// Flag : Grab thread On/Off
		HANDLE				m_hEvtGigaLinx[2];
		HANDLE				m_hEvtWaitGrabThread;

		HANDLE				m_hEvtGrab;				// Event : Check Grab/Abort

		VwGenICam::EVENT_HANDLE		hEventNewBuffer;
		UINT				m_nCurBufIdx;				// Current Buffer Index	// m_curbufferindex;
		void**				m_ppGigaBuffer;
		void*				m_pImageBufferForCallback;
		OBJECT_INFO			m_ObjectInfo;
		IMAGE_INFO			m_ImageInfo;
		ImageCallbackFn*	m_pImageCallbackFn;
		DisconnectCallbackFn*	m_pDisconnectCallbackFn;	// When the connection has been lost, this function will be called.
		GeneralEventCallbackFn* m_pGeneralEventCallbackFn;
		VwGenICam::EVENT_TYPE			m_eEventType;
		void*				m_pUserPointer;				// for Callback Function

		HANDLE				m_pGrabCallback;
		HANDLE				m_pThrGrabCallback;

		HANDLE				m_pThrGrab;
		list<VwGenICam::BUFFER_HANDLE> m_lstBuffers;
		UINT				m_nBufferNum;
		UINT				m_nPaketsMissing;

		BOOL				m_bIsVXDevice;				// Flag : Vx Device 
		UINT				m_nEvtGetDataTimeOut;		// EventGetData() Timeout Value

		HANDLE				m_pThrDisconnect;			// Check to connect condition.
		BOOL				m_bExit;
		VwGenICam::EVENT_HANDLE m_hEventError;
		__int64				m_nFrameCount;				// Frame count

		VWSDK::PROPERTY_LIST		m_PropList;

		HANDLE				m_pThrGeneralEvent;			// Check to general event callback thread.


	protected:
		BOOL				_bInitEventControl;

		BOOL				_bForcedAcquisitionMode;
		BOOL				_bUseCallbackThread;
		BOOL				_bMonitorMode;
		BOOL				_bClearBuffer;
		UINT64				_unLostImage;
		UINT64				_unCorruptedPacket;
		UINT64				_unResendPacket;
		UINT64				_unLostFrame;
		BOOL				_bUseQueueManager;
		BOOL				_bAlive;
		CCP_PRIVILEGE		_eCCPPrivilege;


	protected:
		VwCLCamera(void);

		// protected method
	protected:
		void Initialize(void);
		VWSDK::RESULT CreateBuffer(IN const UINT nImageWidth, IN const UINT nImageHeight, IN const UINT nBitsPerPixel, IN const UINT nBufferNumber,
			IN list<VwGenICam::BUFFER_HANDLE>& lstBuffers, IN const VwGenICam::DS_HANDLE hStream, OUT void*** pppGigaBuffer);
		VWSDK::RESULT DeleteBuffer();

		static VWSDK::RESULT RearrangeQueue(IN list<VwGenICam::BUFFER_HANDLE>& lstBuffers, IN const VwGenICam::DS_HANDLE hStream);

		static VWSDK::RESULT StartAcquisition(IN const VwGenICam::DS_HANDLE hStream);
		static VWSDK::RESULT StopAcquisition(IN const VwGenICam::DS_HANDLE hStream);

		static VWSDK::RESULT SetRegisterEvent(IN const VwGenICam::DS_HANDLE hStream, IN HANDLE* m_hEvtGigaLicx, OUT VwGenICam::EVENT_HANDLE m_hEventNewBuffer);

		VWSDK::RESULT GetPixelFormatFromString(std::string strPixelFormat, VWSDK::PIXEL_FORMAT* pPixelFormat);
		VWSDK::RESULT GetPixelFormatName(IN VWSDK::PIXEL_FORMAT pixelFormat, std::string* strPixelFormat);

		VWSDK::RESULT IsValidPixelSize(IN UINT nPixelSize, OUT BOOL* pbValid);

		BOOL GetVxDevice();
		void SetVxDevice(BOOL bVal);

		void LoadPropertyList();
		void RetrieveNode(GenApi::INode* pNode);

		void SetImage(OBJECT_INFO* pObjectInfo, IMAGE_INFO* pImageInfo);

	protected:
		UINT ThrGrabContinuosOpt();
		UINT ThrGrabContinuos();
		UINT ErrorCheckThread();
		UINT GeneralEventCheckThread();
		UINT GrabCallbackThread();
		static unsigned int WINAPI myGrabContinuosThread(LPVOID me);
		static unsigned int WINAPI thCheckConnection(LPVOID me);
		static unsigned int WINAPI ThrGrabCallbackThread(LPVOID me);
		static unsigned int WINAPI thGeneralEventCallback(LPVOID me);

	protected:
		void ftoa(double dInput, char* chRet, int digit);
		void PrintBufferStateInWindowsEvent(UINT64& nPrevCorruptedImage, UINT64& nPrevResendPacket, UINT nBlockID);
		void ExecuteEventCallback(int nPropertyAll);

	public:
		VWSDK::RESULT GetPixelFormatEntry(OUT list<std::string>* plstPixelFormat);
		VWSDK::RESULT ChangeBufferFormat(IN UINT nBufferNum, IN UINT nWidth, IN UINT nHeight, IN VWSDK::PIXEL_FORMAT pixelFormat);

	public:
		VwCLCamera(VwCLInterface* _pInterface);
		~VwCLCamera(void);

		VWSDK::RESULT GetVwInterface(OUT VwCLInterface** pVwInterface);

		VWSDK::RESULT GetDeviceHandle(OUT VwGenICam::DEV_HANDLE* pDevHandle);

		VWSDK::RESULT OpenCamera(char* name, UINT nNumBuffer, UINT nWidth, UINT nHeight, UINT nPacketSize, void* pUserPointer,
			ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
		VWSDK::RESULT CloseCamera();

		BOOL GetXML(void* hPort, char** pBuffer, size_t& cbBuffer, BOOL& bZip);

		VWSDK::RESULT Snap(UINT nFrameCount);
		VWSDK::RESULT Grab();
		VWSDK::RESULT Abort();
		VWSDK::RESULT GetGrabCondition(BOOL& bIsGrabbing)		{ bIsGrabbing = m_bGrabRunning; return VWSDK::RESULT_SUCCESS; };

		VWSDK::RESULT GetWidth(UINT* pWidth);
		VWSDK::RESULT GetHeight(UINT* pHeight);
		VWSDK::RESULT SetWidth(UINT width);
		VWSDK::RESULT SetHeight(UINT height);
		VWSDK::RESULT SetPixelFormat(VWSDK::PIXEL_FORMAT format);
		VWSDK::RESULT GetPixelFormat(VWSDK::PIXEL_FORMAT* pPixelFormat);

		VWSDK::RESULT GetDeviceModelName(IN int nIndex, OUT  char* pName, OUT size_t* pNameSize);
		VWSDK::RESULT GetDeviceVersion(IN int nIndex, char* pVersion, size_t* pVersionSize);
		VWSDK::RESULT GetDeviceVendorName(IN int nIndex, OUT char* pInfo, OUT size_t* pInfoSize);
		VWSDK::RESULT GetDeviceID(IN int nIndex, char* pInfo, size_t* pInfoSize);

		VWSDK::RESULT GetBufferInfo(OUT UINT& nBufferNum, OUT UINT& nWidth, OUT UINT& nHeight, OUT VWSDK::PIXEL_FORMAT& pixelFormat);

		VWSDK::RESULT GetStreamInfo(IN STREAM_INFO streamInfo, OUT UINT& nInfo);
				
		VWSDK::RESULT ReadData(void *pBuffer, __int64 Address, __int64* Length);
		VWSDK::RESULT WriteData(void *pBuffer, __int64 Address, size_t* Length);

		VWSDK::RESULT InvalidateNodes();
		VWSDK::RESULT SetCustomCommand(IN char* cpCommand, IN char* cpArg, IN size_t cbArgSize = 0, IN bool bSyncCMD = TRUE, IN bool bVerify = TRUE);
		VWSDK::RESULT GetCustomCommand(IN char* cpCommand, OUT char* cpArg, INOUT size_t* cbpArgSize, IN int nCmdType = GET_CUSTOM_COMMAND_VALUE);

		VWSDK::RESULT SetUARTCustomCommand(IN char* pCommand);
		VWSDK::RESULT GetUARTCustomCommand(IN char* pCommand, OUT char* pArg, IN size_t* pArgSize);

		VWSDK::RESULT GetHeartBeatTimeoutTryCount(OUT int& nCount);
		VWSDK::RESULT SetHeartBeatTimeoutTryCount(IN int nCount);

		VWSDK::RESULT InitEventControl();
		VWSDK::RESULT SetEventControlCallback(IN UINT unEventID, IN const char* pcaEventName, IN size_t szEventName, IN EventControlCallbackFn* pEventCallback);

		VWSDK::RESULT GetPropertyCount(OUT int* pCount);
		VWSDK::RESULT GetPropertyInfo(IN int nIndex, OUT VWSDK::PROPERTY* pPropInfo);
		VWSDK::RESULT GetPropertyInfo(IN const char* cpCommand, OUT VWSDK::PROPERTY* pPropInfo);

		VWSDK::RESULT ReadFileStream(IN const char* cpFileName, OUT char* cpBuff, INOUT size_t& szBuff);
		VWSDK::RESULT WriteFileStream(IN const char* cpFileName, IN char* cpBuff, IN size_t szBuff);

		VWSDK::RESULT SetImageCallback(ImageCallbackFn* pImageCallbackFn);

		VWSDK::RESULT SetGeneralEventCallback(GeneralEventCallbackFn* pGeneralEventCallbackFn);

		VWSDK::RESULT GetFileAccessControlStatus(char* pStatus);

		VWSDK::RESULT GetPollingTime(GenApi::INode* pNode, __int64& nPollingTime);

		VWSDK::RESULT GetNumCorruptedImage(UINT64& nCorruptImage);
		VWSDK::RESULT GetNumDeliveredFrame(UINT64& nFrameCount);
		VWSDK::RESULT GetNumLostImage(UINT64& nLostPacket);
		VWSDK::RESULT GetNumResendSuccessfulPackets(UINT64& nResendPacket);
		VWSDK::RESULT GetNumQueuedBuffer(UINT64& nQueuedBuffer);
		VWSDK::RESULT GetNumAnnouncedBuffer(UINT64& nAnnouncedBuffer);

		DWORD WaitGrabThread(UINT dwTimeOut);

		VWSDK::RESULT AcquisitionStop();

		VWSDK::RESULT AcquisitionModeOpt(BOOL bUse);

		VWSDK::RESULT UseIndependentCallback(BOOL bUse)	{ _bUseCallbackThread = bUse;		return VWSDK::RESULT_SUCCESS; };

		BOOL IsValidEventType(UINT unEventType);

		VWSDK::RESULT SetClearBuffer(BOOL bOn);

		VWSDK::RESULT UseQueueManager(BOOL bUse)			{ _bUseQueueManager = bUse;	return VWSDK::RESULT_SUCCESS; };

		VWSDK::RESULT SetCCPPrivilege(CCP_PRIVILEGE ePrivilege)	{ _eCCPPrivilege = ePrivilege;	return VWSDK::RESULT_SUCCESS; };


	};
}

#endif
