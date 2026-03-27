
#pragma once

#include <atlstr.h>

#define UPGRADE_ERROR_CANNOT_READ_FILE						-1
#define UPGRADE_ERROR_CANNOT_CHANGE_ACCESS_MODE				-2
#define UPGRADE_ERROR_CANNOT_READ_CAMERA_CONTROL_REGISTER	-3
#define UPGRADE_ERROR_DOWNLOAD_FAILED						-4
#define UPGRADE_ERROR_CANNOT_CHANGE_STATUS_REGISTER			-5
#define	UPGRADE_ERROR_INVALIDE_ADDRESS						-6
#define	UPGRADE_ERROR_FAILED_DECOMPRESS						-7
#define	UPGRADE_ERROR_UNKNOWN								-8
#define UPGRADE_ERROR_FAILED_READ_DATA						-9
#define UPGRADE_ERROR_FAILED_READ_FILE						-10
#define UPGRADE_ERROR_INVALID_HANDLE						-11
#define UPGRADE_ERROR_CANCEL_OPERATION						-12
#define UPGRADE_SUCCESS										-1000

enum UPDATE_TARGET
{
	UPDATE_FPGA,
	UPDATE_MCU,
	UPDATE_XML,
	UPDATE_SCRIPT,
	UPDATE_PKG,
	UPLOAD_FFC,
	DOWNLOAD_FFC,
	UPLOAD_DEFECT,
	DOWNLOAD_DEFECT,
	UPLOAD_LUT,
	DOWNLOAD_LUT
};

enum ERESULT_ERROR
{
	ERESULT_SUCCESS,
	ERESULT_UNSUPPORTED_UPDATE_TYPE,
	ERESULT_INVALID_HANDLE,
	ERESULT_DEVICE_IN_USE,
	ERESULT_UNKNOWN_ERROR
};

namespace VwSDK
{
	namespace VwGenICam
	{
		typedef void* DEV_HANDLE;
	}
}

#ifndef USE_GENICAM_3_0_NAMESPACE
namespace GenApi
{
	class CNodeMapRef;
	class IPort;
}
#endif

class CDeviceMaintenanceDlg;


typedef void (ProgressCallbackFn)( void* pUserPoint, int nProgress );

#ifdef	__cplusplus
extern "C" {
#endif


#ifdef	_USRDLL
	#define	VwDeviceMaintenance			__declspec(dllexport)
#else
	#define	VwDeviceMaintenance
#endif

#if 1
	VwDeviceMaintenance void RunVwDeviceMaintenance( VwSDK::VwGenICam::DEV_HANDLE hDev, GenApi::CNodeMapRef* pNodemapRef , BOOL bInitOpt, UINT unTimeout = 5/*min*/ * 60/*sec*/ * 1000/*ms*/  );
	VwDeviceMaintenance void RunVwDeviceMaintenanceEx( VwSDK::VwGenICam::DEV_HANDLE hDev, GenApi::IPort* hPort, const char* strXML, size_t cbBuffer, BOOL bZip, BOOL bInitOpt, UINT unTimeout = 5/*min*/ * 60/*sec*/ * 1000/*ms*/ );
#else
	VwDeviceMaintenance void RunVwDeviceMaintenance( DEV_HANDLE hDev, int nSensorWidth, int nSensorHeight );
#endif
	VwDeviceMaintenance ERESULT_ERROR VwUpdateDevice(UPDATE_TARGET eTarget, VwSDK::VwGenICam::DEV_HANDLE hDev, BSTR strFilePath, ProgressCallbackFn* pProgressCallback, UINT unTimeout = 5/*min*/ * 60/*sec*/ * 1000/*ms*/);
	VwDeviceMaintenance ERESULT_ERROR VwUpdateDeviceEx(UPDATE_TARGET eTarget, VwSDK::VwGenICam::DEV_HANDLE hDev, BSTR strFilePath, ProgressCallbackFn* pProgressCallback, int nWidth, int nHeight, int nDefectivePixelDataFormat = 0, UINT unTimeout = 5/*min*/ * 60/*sec*/ * 1000/*ms*/);


#ifdef	__cplusplus
};
#endif
