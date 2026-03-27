
//#pragma once
#ifndef VWCL_API_H
#define VWCL_API_H

#define IN
#define OUT
#define INOUT

#include "VwCL.Global.h"
#include <Windows.h>

#ifdef _USRDLL
#define FUNCTION_IMPORTEXPORT __declspec(dllexport)
#else if
#define FUNCTION_IMPORTEXPORT __declspec(dllimport)
#endif

using namespace VWSDK;

#ifdef __cplusplus
extern "C"
{
#endif
	namespace VWSDK
	{
		namespace CL
		{
			typedef void* VWCL_HANDLE;
			typedef void* HINTERFACE;
			typedef void* HCAMERA;
			typedef void* HDEVICE;

			FUNCTION_IMPORTEXPORT VWSDK::RESULT OpenVwCL(VWCL_HANDLE* hVwCL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CloseVwCL(VWCL_HANDLE& hVwCL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetNumInterfaces(VWCL_HANDLE hVwCL, UINT* pNumInterfaces);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetNumCameras(VWCL_HANDLE hVwCL, UINT* aPNumCamera);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenCameraByIndex(VWCL_HANDLE hVwCL, UINT nCameraIndex, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
				UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenCameraByName(VWCL_HANDLE hVwCL, char* pCameraName, HCAMERA* phCamera, UINT nNumBuffer, UINT nWidth, UINT nHeight,
				UINT nPacketSize, void* pUserPointer, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenInterfaceByIndex(VWCL_HANDLE hVwCL, UINT aNIndex, HINTERFACE* phInterface);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwOpenInterfaceByName(VWCL_HANDLE hVwCL, char* pInterfaceName, HINTERFACE* phInterface);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscovery(VWCL_HANDLE hVwCL);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscoveryInterfaceInfo(VWCL_HANDLE hVwCL, UINT nIndex, INTERFACE_INFO_STRUCT* pInterfaceInfoStruct);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwDiscoveryCameraInfo(VWCL_HANDLE hVwCL, UINT nIndex, CAMERA_INFO_STRUCT* pCameraInfoStruct);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwSetDiscoveryTimeout(IN VWCL_HANDLE hVwCL, IN UINT uTimeout);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwGetDiscoveryTimeout(IN VWCL_HANDLE hVwCL, OUT UINT& uTimeout);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwSetCCPPrivilege(IN VWCL_HANDLE hVwCL, IN CCP_PRIVILEGE eCCPPrivilige);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT VwUserLogging(IN VWCL_HANDLE hVwCL, IN TCHAR* pModuleName, IN TCHAR* pFileName, IN TCHAR* pFuncName, IN UINT unLine, IN TCHAR* pLogMsg);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceOpenCameraByIndex(IN HINTERFACE hInstance, IN void* pCallbackParent, IN UINT nDevIndex, HCAMERA* phCamera, UINT nNumBuffer,
				UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceOpenCameraByName(HINTERFACE hInterface, void* pParent, char* pName, HCAMERA* phCamera, UINT nNumBuffer,
				UINT nWidth, UINT nHeight, UINT nPacketSize, ImageCallbackFn* pImageCallbackFn, DisconnectCallbackFn* pDisconnectCallbackFn = NULL);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetNumCameras(HINTERFACE hInterface, UINT* aPNumDevices);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetCameraName(HINTERFACE hInterface, UINT aNDevIndex, char* aName, size_t* aPNameSize);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceCloseInterface(HINTERFACE hInterface);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT InterfaceGetVwCLHandle(IN HINTERFACE hInterface, OUT VWCL_HANDLE* phVwCLHandle);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraClose(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGrab(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSnap(HCAMERA hCamera, UINT unFrame);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraAbort(HCAMERA hCamera);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetGrabCondition(HCAMERA hCamera, BOOL& bIsGrabbing);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetAcquisitionTimeOut(HCAMERA hCamera, UINT nTimeOut);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetAcquisitionTimeOut(HCAMERA hCamera, UINT* pnTimeOut);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceID(IN HCAMERA hCamera, IN int nIndex, OUT char* pInfo, INOUT size_t* pInfoSize);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetWidth(IN HCAMERA hCamera, OUT UINT* pWidth);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetHeight(IN HCAMERA hCamera, OUT UINT* pHeight);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetWidth(IN HCAMERA hCamera, IN UINT nWidth);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetHeight(IN HCAMERA hCamera, IN UINT nHeight);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetPixelFormat(HCAMERA hCamera, PIXEL_FORMAT pixelFormat);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetPixelFormat(HCAMERA hCamera, PIXEL_FORMAT* pPixelFormat);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraChangeBufferFormat(HCAMERA hCamera, UINT nBufferNum, UINT nWidth, UINT nHeight, VWSDK::PIXEL_FORMAT pixelFormat);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetBufferInfo(IN HCAMERA hCamera, OUT UINT& nBufferNum, OUT UINT& nWidth, OUT UINT& nHeight, OUT VWSDK::PIXEL_FORMAT& pixelFormat);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetInterfaceHandle(IN HCAMERA hCamera, OUT HINTERFACE* phInterface);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetStreamInfo(IN HCAMERA hCamera, IN STREAM_INFO streamInfo, OUT UINT& nInfo);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetCustomCommand(IN HCAMERA hCamera, IN char* pCommand, IN char* pArg, IN size_t cbArgSize = 0, IN BOOL bSyncCMD = TRUE);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetCustomCommand(IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, INOUT size_t* pArgSize, IN int nCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetPropertyCount(IN HCAMERA hCamera, OUT int* pnCount);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetPropertyInfo(IN HCAMERA hCamera, IN const char* cpCommand, OUT PROPERTY* ptPropInfo);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetPropertyInfoUsingIndex(IN HCAMERA hCamera, IN int nIndex, OUT PROPERTY* ptPropInfo);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraSetUARTCustomCommand(IN HCAMERA hCamera, IN char* pCommand);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetUARTCustomCommand(IN HCAMERA hCamera, IN char* pCommand, OUT char* pArg, IN size_t* pArgSize);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraReadData(IN HCAMERA hCamera, void *pBuffer, __int64 Address, __int64* Length);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraWriteData(IN HCAMERA hCamera, void *pBuffer, __int64 Address, size_t* Length);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraReadFileStream(IN HCAMERA hCamera, IN const char* cpFileName, OUT char* cpBuff, INOUT size_t& szBuff);
			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraWriteFileStream(IN HCAMERA hCamera, IN const char* cpFileName, IN char* cpBuff, IN size_t szBuff);

			FUNCTION_IMPORTEXPORT VWSDK::RESULT CameraGetDeviceHandle(IN HCAMERA hCamera, OUT HDEVICE* pDevHandle);

		}
	}
#ifdef __cplusplus
}
#endif

#endif
