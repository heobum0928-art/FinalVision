// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "vwgige.API.h"


static void	DisconnectionCallback( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnectInfo );

BOOL g_bDisconnation = FALSE;

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VWGIGE_HANDLE	vwGigE		= NULL;
	VWSDK::RESULT ret = VWSDK::OpenVwGigE( &vwGigE );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	VWSDK::VwUserLogging(vwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	// 1. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::HCAMERA			hCamera		= NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo		= new VWSDK::OBJECT_INFO;
	ret = VwOpenCameraByIndex( vwGigE,
								0/*Device Index*/,
								&hCamera,
								2/*The number of buffer*/,
								0/*Auto*/,
								0/*Auto*/,
								0/*Auto*/,
								pObjectInfo/*User Pointer*/,
								NULL/*Skip in this sample*/,
								DisconnectionCallback/*Disconnection Callback function*/ );

	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
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

	const UINT unTimeout = 30/*sec*/;
	_tprintf(_T("Step 2. Please disconnect within %d sec.....\n"), unTimeout);

	UINT unCount = 0;
	while ( unCount < unTimeout )
	{
		if ( g_bDisconnation )
		{
			_tprintf(_T("\t**The device connection has been lost!**\n"));
			break;
		}
		else
		{
			_tprintf(_T("\tElapsed time : %d sec.\r"), unCount+1);
		}

		// Delay
		Sleep( 1000 );
		unCount ++;
	}

	if ( unCount == unTimeout )
	{
		_tprintf(_T("\n\tTime out.\n"));
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

	if ( vwGigE)
	{
		VWSDK::CloseVwGigE( vwGigE );
		vwGigE	= NULL;
	}

	return 0;
}

void DisconnectionCallback( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnectInfo )
{
	g_bDisconnation = TRUE;
}