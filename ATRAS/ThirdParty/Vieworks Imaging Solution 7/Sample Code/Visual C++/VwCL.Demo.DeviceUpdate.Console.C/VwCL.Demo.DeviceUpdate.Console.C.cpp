// Descriptions
//

#include "stdafx.h"

// for VwCL
#include "VwCL.h"
//#include "VwCLCamera.h"
#include "VwCL.API.h"
#include "VwDeviceMaintenanceIntl.h"

using namespace VWSDK::CL;

char* Dispatch(char *cmdline, int cmdSize, HCAMERA hCamera);
static void UpdateCallbackFunc(void* pUserPoint, int nProgress);

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWCL_HANDLE	vwCL		= NULL;
	RESULT ret					= OpenVwCL( &vwCL );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	VwUserLogging(vwCL, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	// 1. Open device
	_tprintf(_T("Step 1. Open Device....."));
	HCAMERA			hCamera			= NULL;
	OBJECT_INFO*	pObjectInfo			= new VWSDK::OBJECT_INFO;
	ret = VwOpenCameraByIndex( vwCL,
								0/*Device Index*/,
								&hCamera,
								2/*The number of buffer*/,
								0/*Auto*/,
								0/*Auto*/,
								0/*Auto*/,
								pObjectInfo/*User Pointer*/,
								NULL/*Skip in this sample*/ );

	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		// Terminate
		if ( pObjectInfo )
		{
			delete pObjectInfo;
			pObjectInfo = NULL;
		}
		if ( vwCL)
		{
			CloseVwCL( vwCL );
			vwCL	= NULL;
		}
		return -1;
	}

	HDEVICE hDev = NULL;
	CameraGetDeviceHandle(hCamera, &hDev);

	_tprintf(_T("OK\n"));

MAIN_MENU:
	char chDone = 0;
	char chCmdline[80] = {0,};

	while(1)
	{
		printf("\n\tSelect menu\n");
		printf("\t1.Firmware Download( PC to Camera )\n");
		printf("\t2.Defect Download( PC to Camera )\n");
		printf("\t3.Defect Upload( Camera to PC )\n");
		printf("\t4.LUT Download( PC to Camera )\n");
		printf("\t5.LUT Upload( Camera to PC )\n");
		printf("\t6.FFC Download( PC to Camera )\n");
		printf("\t7.FFC Upload( Camera to PC )\n");

		printf("\t8.Quit\n");
		printf(">");

		gets_s(chCmdline, sizeof(chCmdline));
		if(0 == strlen(chCmdline))
		{
			/* do nothing */
		}
		else if(!_strnicmp("8", chCmdline, 2))
		{
			break;
		}
		else
		{
			Dispatch((char *)chCmdline, sizeof(chCmdline), hDev);
		}
	}

	do 
	{
		printf("** Goto Select menu?\n");
		printf("** Y\n");
		printf("** N\n");	
		printf(">");	
		gets_s(chCmdline, sizeof(chCmdline));
	} while ( ( _strnicmp("Y", chCmdline, 1) ) &&
			  ( _strnicmp("N", chCmdline, 1) ) );

	if( !(_strnicmp("Y", chCmdline, 1))) 
	{
		goto MAIN_MENU;
	}

	// 3. Terminate
	_tprintf(_T("Step 3. Terminate.....\n"));
	if ( pObjectInfo )
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if ( hCamera )
	{
		CameraClose( hCamera );
		hCamera = NULL;
	}

	if ( vwCL)
	{
		CloseVwCL( vwCL );
		vwCL	= NULL;
	}

	return 0;
}


char* Dispatch(char *cmdline, int cmdSize, HCAMERA hCamera)
{
	if ( NULL == hCamera )
	{
		return NULL;
	}

	if (0 == strlen(cmdline))
	{
		return cmdline;
	}
	
	char caFileName[80] = { 0, };

	printf("\nInput a name of file.\n");
	printf(">");

	gets_s(caFileName, sizeof(caFileName));

	CString strFileName = CA2T(caFileName);

	ERESULT_ERROR eRet = ERESULT_SUCCESS;
	if (!(_strnicmp("1", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::UPDATE_PKG, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("2", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::DOWNLOAD_DEFECT, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("3", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::UPLOAD_DEFECT, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("4", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::DOWNLOAD_LUT, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("5", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::UPLOAD_LUT, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("6", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::DOWNLOAD_FFC, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else if (!(_strnicmp("7", cmdline, 2)))
	{
		eRet = VwUpdateDevice(UPDATE_TARGET::UPLOAD_FFC, hCamera, strFileName.AllocSysString(), UpdateCallbackFunc);
	}
	else
	{
		strncpy_s(cmdline, cmdSize, "Unrecognized command\n", cmdSize);

		return cmdline;
	}

	if (ERESULT_SUCCESS == eRet)
	{
		_tprintf(_T("The update has been completed successfully\n"));
	}
	else
	{
		_tprintf(_T("The update has been failed. Err(%d)\n"), eRet);
	}


	return cmdline;
}


static void UpdateCallbackFunc(void* pUserPoint, int nProgress)
{
	CStringA strDebug;
	strDebug.Format("Update Progress(%d)\n", nProgress);
	printf(strDebug);
	return;
}