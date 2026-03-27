// VwGigE.Demo.MultiCast.Console.C.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "conio.h"
// for VwGigE
#include "VwGigE.API.h"

static void	GetImageCallback(VWSDK::OBJECT_INFO* pObjectInfo,VWSDK::IMAGE_INFO* pImageInfo);

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VWGIGE_HANDLE	vwGigE		= NULL;
	VWSDK::RESULT ret					= VWSDK::OpenVwGigE( &vwGigE );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	VWSDK::VwUserLogging(vwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	// 1. Set MULTICAST options
	VWSDK::VwSetMultiCastAddress( vwGigE, 0xe0000501/*224.0.5.1*/ );

	// 2. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::HCAMERA			hCamera     = NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo		= new VWSDK::OBJECT_INFO;

	// The device will be opened CONTROL mode.
	ret = VWSDK::VwOpenCameraByIndex( vwGigE,
								0/*Device Index*/,
								&hCamera,
								2/*The number of buffer*/,
								0/*Auto*/,
								0/*Auto*/,
								0/*Auto*/,
								pObjectInfo/*User Pointer*/,
								GetImageCallback/*Image Callback*/ );
	
	
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
		if ( vwGigE)
		{
			VWSDK::CloseVwGigE( vwGigE );
			vwGigE	= NULL;
		}
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 2. Grab
	_tprintf(_T("Step 2. Start Grab\n"));
	_tprintf(_T("\tPlease check that the other machine can open the device in MONITOR mode.\n"));
	_tprintf(_T("\tAnd then try to grab. The same image will be shown.\n"));
	
	VWSDK::CameraGrab( hCamera );

	_tprintf(_T("\t\tStop : press 'q' key.\n"));

	char cmdline;
	cmdline = _getche();

	if ( cmdline == 'q' )
	{
		VWSDK::CameraAbort( hCamera );
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
		VWSDK::CameraClose( hCamera );
		hCamera = NULL;
	}

	VWSDK::CloseVwGigE( vwGigE );

	return 0;
}

void GetImageCallback( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo )
{
	if ( NULL == pImageInfo )
		return;
	
	_tprintf(_T("Image(%d)\r"), pImageInfo->bufferIndex );
}