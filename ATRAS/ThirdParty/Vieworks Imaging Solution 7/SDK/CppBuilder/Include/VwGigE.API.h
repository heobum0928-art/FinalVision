
//#pragma once
#ifndef VWGIGE_API_H
#define VWGIGE_API_H

#define IN
#define OUT
#define INOUT

#include "VwGigE.Global.h"
#include <Windows.h>

#ifdef _USRDLL
#define FUNCTION_IMPORTEXPORT __declspec(dllexport)
#else if
#define FUNCTION_IMPORTEXPORT __declspec(dllimport)
#endif

#ifndef DEPRECATED_GET_SET_FUNCTION
#define DEPRECATED_GET_SET_FUNCTION		__declspec(deprecated("** This function or variable is deprecated. Consider using CameraSetCustomCommand or CameraGetCustomCommand instead. To disable deprecation, use _CRT_SECURE_NO_DEPRECATE. **"))
#endif


#ifdef __cplusplus
extern "C"
{
#endif

namespace VWSDK
{

	typedef void* VWGIGE_HANDLE;
	typedef void* HINTERFACE;
	typedef void* HCAMERA;

	FUNCTION_IMPORTEXPORT RESULT OpenVwGigE(VWGIGE_HANDLE* hVwGigE);
	FUNCTION_IMPORTEXPORT RESULT CloseVwGigE(VWGIGE_HANDLE& hVwGigE);
	FUNCTION_IMPORTEXPORT RESULT VwGetNumInterfaces(VWGIGE_HANDLE hVwGigE, UINT* pNumInterfaces);
	
	FUNCTION_IMPORTEXPORT RESULT VwGetNumCameras(IN VWGIGE_HANDLE hVwGigE, OUT UINT* aPNumCamera);
	FUNCTION_IMPORTEXPORT RESULT VwGetNumCamerasByIndex(IN VWGIGE_HANDLE hVwGigE, IN UINT nInterfaceIndex, OUT UINT* aPNumCamera);
	FUNCTION_IMPORTEXPORT RESULT VwGetNumCamerasByName(IN VWGIGE_HANDLE hVwGigE, IN char* pInterfaceName, OUT UINT* aPNumCamera);

	FUNCTION_IMPORTEXPORT RESULT VwOpenCameraByIndex(VWGIGE_HANDLE hVwGigE, UINT nCameraIndex, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
		UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
	FUNCTION_IMPORTEXPORT RESULT VwOpenCameraByName(VWGIGE_HANDLE hVwGigE, char* pCameraName, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
		UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
	FUNCTION_IMPORTEXPORT RESULT VwOpenInterfaceByIndex(VWGIGE_HANDLE hVwGigE, UINT aNIndex, HINTERFACE* phInterface);
	FUNCTION_IMPORTEXPORT RESULT VwOpenInterfaceByName(VWGIGE_HANDLE hVwGigE, char* pInterfaceName, HINTERFACE* phInterface);
	
	FUNCTION_IMPORTEXPORT RESULT VwDiscovery(VWGIGE_HANDLE hVwGigE);

	FUNCTION_IMPORTEXPORT RESULT VwDiscoveryInterfaceInfo(VWGIGE_HANDLE hVwGigE,UINT nIndex,INTERFACE_INFO_STRUCT* pInterfaceInfoStruct);
	FUNCTION_IMPORTEXPORT RESULT VwDiscoveryCameraInfo(VWGIGE_HANDLE hVwGigE,UINT nIndex,CAMERA_INFO_STRUCT* pCameraInfoStruct);

	FUNCTION_IMPORTEXPORT RESULT VwForceIP(VWGIGE_HANDLE hVwGigE, PBYTE pMAC, DWORD nIP, DWORD nSubnet, DWORD nGateway);
	FUNCTION_IMPORTEXPORT RESULT IPAddressToIPDecimal(IN VWGIGE_HANDLE hVwGigE, IN char* pIPAddress, IN size_t nSize, OUT DWORD* pIPDecimal);
	FUNCTION_IMPORTEXPORT RESULT IPDecimalToIPAddress(IN VWGIGE_HANDLE hVwGigE, IN DWORD dwIPDecimal, OUT char* pIPAddress, INOUT size_t* pArgSize);

	FUNCTION_IMPORTEXPORT RESULT VwUseMTUOptimize( IN VWGIGE_HANDLE hVwGigE, IN bool bUse );
	FUNCTION_IMPORTEXPORT RESULT VwSetMTUTimeout( IN VWGIGE_HANDLE hVwGigE, IN UINT uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwGetMTUTimeout( IN VWGIGE_HANDLE hVwGigE, OUT UINT& uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwSetGVCPTimeout( IN VWGIGE_HANDLE hVwGigE, IN UINT uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwGetGVCPTimeout( IN VWGIGE_HANDLE hVwGigE, OUT UINT& uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwSetMultiCastAddress( IN VWGIGE_HANDLE hVwGigE, IN DWORD dwMultiCastAddress );
	FUNCTION_IMPORTEXPORT RESULT VwGetMultiCastAddress( IN VWGIGE_HANDLE hVwGigE, OUT DWORD& dwMultiCastAddress );
	FUNCTION_IMPORTEXPORT RESULT VwSetIgnoreSubnet( IN VWGIGE_HANDLE hVwGigE, IN bool bUse );
	FUNCTION_IMPORTEXPORT RESULT VwGetIgnoreSubnet( IN VWGIGE_HANDLE hVwGigE, OUT bool& bUse );
	FUNCTION_IMPORTEXPORT RESULT VwSetDiscoveryTimeout( IN VWGIGE_HANDLE hVwGigE, IN UINT uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwGetDiscoveryTimeout( IN VWGIGE_HANDLE hVwGigE, OUT UINT& uTimeout );
	FUNCTION_IMPORTEXPORT RESULT VwSetCCPPrivilege( IN VWGIGE_HANDLE hVwGigE, IN CCP_PRIVILEGE eCCPPrivilige );
	FUNCTION_IMPORTEXPORT RESULT VwUserLogging(IN VWGIGE_HANDLE hVwGigE, IN TCHAR* pModuleName, IN TCHAR* pFileName, IN TCHAR* pFuncName, IN UINT unLine, IN TCHAR* pLogMsg);

	FUNCTION_IMPORTEXPORT RESULT InterfaceOpenCameraByIndex(IN HINTERFACE hInstance,IN void* pCallbackParent,IN UINT nDevIndex,HCAMERA* phCamera, UINT nNumBuffer,
		UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);

	FUNCTION_IMPORTEXPORT RESULT InterfaceOpenCameraByName(HINTERFACE hInterface, void* pParent, char* pName,HCAMERA* phCamera, UINT nNumBuffer,
		UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetNumCameras(HINTERFACE hInterface, UINT* aPNumDevices);
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetCameraName(HINTERFACE hInterface, UINT aNDevIndex, char* aName, size_t* aPNameSize);
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetCameraID(HINTERFACE hInterface, UINT aNDevIndex, char* aID, size_t* aPIDSize);

	FUNCTION_IMPORTEXPORT RESULT InterfaceGetIPAddress(HINTERFACE hInterface, char* pszIPAddress, size_t* paIPAddress);
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetSubnet(HINTERFACE hInterface, char* pszSubnet, size_t* paSubnet);
	FUNCTION_IMPORTEXPORT RESULT InterfaceUsePacketResend(IN HINTERFACE hInterface, IN BOOL bUse);
	FUNCTION_IMPORTEXPORT RESULT InterfaceUsePacketResendFrequently(IN HINTERFACE hInterface, IN BOOL bUse);
	FUNCTION_IMPORTEXPORT RESULT InterfaceUseLAGMode(IN HINTERFACE hInterface, IN BOOL bUse);
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetImageReceivingTimeout( IN HINTERFACE hInterface, OUT UINT& unTimeout );
	FUNCTION_IMPORTEXPORT RESULT InterfaceSetImageReceivingTimeout( IN HINTERFACE hInterface, IN UINT unTimeout );
	FUNCTION_IMPORTEXPORT RESULT InterfaceGetImageRetransTimeout( IN HINTERFACE hInterface, OUT UINT& unTimeout );
	FUNCTION_IMPORTEXPORT RESULT InterfaceSetImageRetransTimeout( IN HINTERFACE hInterface, IN UINT unTimeout );

	FUNCTION_IMPORTEXPORT RESULT InterfaceCloseInterface(HINTERFACE hInterface);

	FUNCTION_IMPORTEXPORT RESULT InterfaceGetVwGigEHandle(IN HINTERFACE hInterface,OUT VWGIGE_HANDLE* phVwGigEHandle);

	FUNCTION_IMPORTEXPORT RESULT CameraClose(HCAMERA hCamera);
	FUNCTION_IMPORTEXPORT RESULT CameraGrab(HCAMERA hCamera);

	FUNCTION_IMPORTEXPORT RESULT CameraGetGrabData(IN HCAMERA hCamera, OUT IMAGE_INFO* pImageInfo );

	FUNCTION_IMPORTEXPORT RESULT CameraSnap(HCAMERA hCamera, UINT unFrame);
	FUNCTION_IMPORTEXPORT RESULT CameraAbort(HCAMERA hCamera);
	FUNCTION_IMPORTEXPORT RESULT CameraGetGrabCondition(HCAMERA hCamera, BOOL& bIsGrabbing);
	
	FUNCTION_IMPORTEXPORT RESULT CameraGetDeviceModelName(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
	FUNCTION_IMPORTEXPORT RESULT CameraGetDeviceVersion(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
	FUNCTION_IMPORTEXPORT RESULT CameraGetDeviceVendorName(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
	FUNCTION_IMPORTEXPORT RESULT CameraGetDeviceID(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);

	FUNCTION_IMPORTEXPORT RESULT CameraGetWidth(HCAMERA hCamera, UINT* punWidth);
	FUNCTION_IMPORTEXPORT RESULT CameraGetHeight(HCAMERA hCamera, UINT* punHeight);
	FUNCTION_IMPORTEXPORT RESULT CameraSetWidth(HCAMERA hCamera, UINT pnWidth);
	FUNCTION_IMPORTEXPORT RESULT CameraSetHeight(HCAMERA hCamera, UINT pnHeight);
	FUNCTION_IMPORTEXPORT RESULT CameraSetPixelFormat(HCAMERA hCamera, PIXEL_FORMAT pixelFormat);
	FUNCTION_IMPORTEXPORT RESULT CameraGetPixelFormat(HCAMERA hCamera, PIXEL_FORMAT* pPixelFormat);

	FUNCTION_IMPORTEXPORT RESULT CameraSetAcquisitionTimeOut(HCAMERA hCamera, UINT nTimeOut);
	FUNCTION_IMPORTEXPORT RESULT CameraGetAcquisitionTimeOut(HCAMERA hCamera, UINT* pnTimeOut);

	FUNCTION_IMPORTEXPORT RESULT CameraChangeBufferFormat(HCAMERA hCamera, UINT nBufferNum, UINT nWidth, UINT nHeight, PIXEL_FORMAT pixelFormat);
	FUNCTION_IMPORTEXPORT RESULT CameraGetBufferInfo(IN HCAMERA hCamera, OUT UINT& nBufferNum, OUT UINT& nWidth, OUT UINT& nHeight, OUT PIXEL_FORMAT& pixelFormat);
	FUNCTION_IMPORTEXPORT RESULT CameraGetInterfaceHandle(IN HCAMERA hCamera, OUT HINTERFACE* phInterface);
	FUNCTION_IMPORTEXPORT RESULT CameraGigEGetColorRGBGain(IN HCAMERA hCamera, IN int nRGBType, OUT double* dpRGBGainValue);
	FUNCTION_IMPORTEXPORT RESULT CameraGigESetColorRGBGain(IN HCAMERA hCamera, IN int nRGBType, IN double dRGBGainValue);

	FUNCTION_IMPORTEXPORT RESULT CameraGetStreamInfo( IN HCAMERA hCamera, IN STREAM_INFO streamInfo, OUT UINT& nInfo );

	FUNCTION_IMPORTEXPORT RESULT CameraGigEGetTemperature( IN HCAMERA hCamera, OUT double* dpTemperature );
	FUNCTION_IMPORTEXPORT RESULT CameraGigEGetSerialNumber( IN HCAMERA hCamera, char capSerialNumber[], size_t& cbpSerialNumberSize );
	 
	FUNCTION_IMPORTEXPORT RESULT CameraSetCustomCommand(IN HCAMERA hCamera, IN char* pCommand, IN char* pArg, IN size_t cbArgSize = 0, IN BOOL bSyncCMD = TRUE);
	FUNCTION_IMPORTEXPORT RESULT CameraGetCustomCommand( IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, INOUT size_t* pArgSize, IN int nCmdType = GET_CUSTOM_COMMAND_VALUE );

	FUNCTION_IMPORTEXPORT RESULT CameraGetPropertyCount( IN HCAMERA hCamera, OUT int* pnCount );
	FUNCTION_IMPORTEXPORT RESULT CameraGetEnumPropertyItems(IN HCAMERA hCamera, IN const char* pPropertyName, OUT char* pArg, INOUT size_t* pArgSize);
	FUNCTION_IMPORTEXPORT RESULT CameraGetPropertyInfo( IN HCAMERA hCamera, IN const char* cpCommand, OUT PROPERTY* ptPropInfo );
	FUNCTION_IMPORTEXPORT RESULT CameraGetPropertyInfoUsingIndex( IN HCAMERA hCamera, IN int nIndex, OUT PROPERTY* ptPropInfo );

	FUNCTION_IMPORTEXPORT RESULT CameraSetUARTCustomCommand( IN HCAMERA hCamera, IN char* pCommand );
	FUNCTION_IMPORTEXPORT RESULT CameraGetUARTCustomCommand( IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, IN size_t* pArgSize );

	FUNCTION_IMPORTEXPORT RESULT CameraReadData( IN HCAMERA hCamera, void *pBuffer, __int64 Address, __int64* Length );
	FUNCTION_IMPORTEXPORT RESULT CameraWriteData( IN HCAMERA hCamera, void *pBuffer, __int64 Address, size_t* Length );

	FUNCTION_IMPORTEXPORT RESULT CameraSetHeartBeatTimeoutTryCount( IN HCAMERA hCamera, IN int nCount );
	FUNCTION_IMPORTEXPORT RESULT CameraGetHeartBeatTimeoutTryCount( IN HCAMERA hCamera, OUT int& nCount );

	FUNCTION_IMPORTEXPORT RESULT CameraSetEventControl( IN HCAMERA hCamera, IN UINT unEventID, IN const char* pcaEventName, IN size_t szEventName, EventControlCallbackFn* pEventCallback );
	FUNCTION_IMPORTEXPORT RESULT CameraUseIndependentCallback( IN HCAMERA hCamera, IN BOOL bUse );
	FUNCTION_IMPORTEXPORT RESULT CameraSetImageCallback( IN HCAMERA hCamera, IN ImageCallbackFn* pImageCallbackFn );

	FUNCTION_IMPORTEXPORT RESULT CameraReadFileStream( IN HCAMERA hCamera, IN const char* cpFileName, OUT char* cpBuff, INOUT size_t& szBuff  );
	FUNCTION_IMPORTEXPORT RESULT CameraWriteFileStream( IN HCAMERA hCamera, IN const char* cpFileName, IN char* cpBuff, IN size_t szBuff );
	
}//namespace VWSDK

#ifdef __cplusplus
}
#endif

#endif
