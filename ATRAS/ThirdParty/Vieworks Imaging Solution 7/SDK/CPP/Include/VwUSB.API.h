
//#pragma once
#ifndef VWUSB_API_H
#define VWUSB_API_H

#define IN
#define OUT
#define INOUT

#include "VwUSB.Global.h"
#include <Windows.h>

#ifdef _USRDLL
#define FUNCTION_IMPORTEXPORT __declspec(dllexport)
#else if
#define FUNCTION_IMPORTEXPORT __declspec(dllimport)
#endif

#ifndef DEPRECATED_GET_SET_FUNCTION
#define DEPRECATED_GET_SET_FUNCTION		__declspec(deprecated("** This function or variable is deprecated. Consider using CameraSetCustomCommand or CameraGetCustomCommand instead. To disable deprecation, use _CRT_SECURE_NO_DEPRECATE. **"))
#endif

using namespace VWSDK;

#ifdef __cplusplus
extern "C"
{
#endif

	namespace VWSDK
	{
		namespace USB
		{

			typedef void* VWUSB_HANDLE;
			typedef void* HINTERFACE;
			typedef void* HCAMERA;

			FUNCTION_IMPORTEXPORT VWSDK::RESULT OpenVwUSB(VWUSB_HANDLE* hVwUSB);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CloseVwUSB(VWUSB_HANDLE& hVwUSB);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetNumInterfaces(VWUSB_HANDLE hVwUSB, UINT* pNumInterfaces);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetNumCameras(VWUSB_HANDLE hVwUSB, UINT* aPNumCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenCameraByIndex(VWUSB_HANDLE hVwUSB, UINT nCameraIndex, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
				UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenCameraByName(VWUSB_HANDLE hVwUSB, char* pCameraName, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
				UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenInterfaceByIndex(VWUSB_HANDLE hVwUSB, UINT aNIndex, HINTERFACE* phInterface);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenInterfaceByName(VWUSB_HANDLE hVwUSB, char* pInterfaceName, HINTERFACE* phInterface);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscovery(VWUSB_HANDLE hVwUSB);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscoveryInterfaceInfo(VWUSB_HANDLE hVwUSB, UINT nIndex, INTERFACE_INFO_STRUCT* pInterfaceInfoStruct);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscoveryCameraInfo(VWUSB_HANDLE hVwUSB, UINT nIndex, CAMERA_INFO_STRUCT* pCameraInfoStruct);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwSetDiscoveryTimeout(IN VWUSB_HANDLE hVwUSB, IN UINT uTimeout);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetDiscoveryTimeout(IN VWUSB_HANDLE hVwUSB, OUT UINT& uTimeout);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwUserLogging(IN VWUSB_HANDLE hVwUSB, IN TCHAR* pModuleName, IN TCHAR* pFileName, IN TCHAR* pFuncName, IN UINT unLine, IN TCHAR* pLogMsg);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceOpenCameraByIndex(IN HINTERFACE hInstance, IN void* pCallbackParent, IN UINT nDevIndex, HCAMERA* phCamera, UINT nNumBuffer,
				UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceOpenCameraByName(HINTERFACE hInterface, void* pParent, char* pName, HCAMERA* phCamera, UINT nNumBuffer,
				UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetNumCameras(HINTERFACE hInterface, UINT* aPNumDevices);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetCameraName(HINTERFACE hInterface, UINT aNDevIndex, char* aName, size_t* aPNameSize);


			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceCloseInterface(HINTERFACE hInterface);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetVwUSBHandle(IN HINTERFACE hInterface, OUT VWUSB_HANDLE* phVwUSBHandle);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraClose(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGrab(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraAcquisitionStop(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSnap(HCAMERA hCamera, UINT unFrame);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraAbort(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetGrabCondition(HCAMERA hCamera, BOOL& bIsGrabbing);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetAcquisitionTimeOut(HCAMERA hCamera, UINT nTimeOut);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetAcquisitionTimeOut(HCAMERA hCamera, UINT* pnTimeOut);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceModelName(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceVersion(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceVendorName(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceID(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);

			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraGetWidth(HCAMERA hCamera, UINT* punWidth);
			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraGetHeight(HCAMERA hCamera, UINT* punHeight);
			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraSetWidth(HCAMERA hCamera, UINT pnWidth);
			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraSetHeight(HCAMERA hCamera, UINT pnHeight);
			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraSetPixelFormat(HCAMERA hCamera, VWSDK::PIXEL_FORMAT pixelFormat);
			FUNCTION_IMPORTEXPORT  VWSDK::RESULT CameraGetPixelFormat(HCAMERA hCamera, VWSDK::PIXEL_FORMAT* pPixelFormat);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraChangeBufferFormat(HCAMERA hCamera, UINT nBufferNum, UINT nWidth, UINT nHeight, VWSDK::PIXEL_FORMAT pixelFormat);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetBufferInfo(IN HCAMERA hCamera, OUT UINT& nBufferNum, OUT UINT& nWidth, OUT UINT& nHeight, OUT VWSDK::PIXEL_FORMAT& pixelFormat);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetInterfaceHandle(IN HCAMERA hCamera, OUT HINTERFACE* phInterface);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetStreamInfo(IN HCAMERA hCamera, IN STREAM_INFO streamInfo, OUT UINT& nInfo);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetCustomCommand(IN HCAMERA hCamera, IN char* pCommand, IN char* pArg, IN size_t cbArgSize = 0, IN BOOL bSyncCMD = TRUE);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetCustomCommand(IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, INOUT size_t* pArgSize, IN int nCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraPropertyCount(IN HCAMERA hCamera, OUT int* pnCount);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetEnumPropertyItems(IN HCAMERA hCamera, IN const char* pPropertyName, OUT char* pArg, INOUT size_t* pArgSize);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraPropertyInfo(IN HCAMERA hCamera, IN const char* cpCommand, OUT VWSDK::PROPERTY* ptPropInfo);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraPropertyInfoUsingIndex(IN HCAMERA hCamera, IN int nIndex, OUT VWSDK::PROPERTY* ptPropInfo);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetUARTCustomCommand(IN HCAMERA hCamera, IN char* pCommand);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetUARTCustomCommand(IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, IN size_t* pArgSize);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraReadData(IN HCAMERA hCamera, void *pBuffer, __int64 Address, __int64* Length);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraWriteData(IN HCAMERA hCamera, void *pBuffer, __int64 Address, size_t* Length);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetEventControl(IN HCAMERA hCamera, IN UINT unEventID, IN const char* pcaEventName, IN size_t szEventName, EventControlCallbackFn* pEventCallback);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraUseIndependentCallback(IN HCAMERA hCamera, IN BOOL bUse);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetImageCallback(IN HCAMERA hCamera, IN ImageCallbackFn* pImageCallbackFn);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraReadFileStream(IN HCAMERA hCamera, IN const char* cpFileName, OUT char* cpBuff, INOUT size_t& szBuff);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraWriteFileStream(IN HCAMERA hCamera, IN const char* cpFileName, IN char* cpBuff, IN size_t szBuff);
		}
	}
#ifdef __cplusplus
}
#endif

#endif
