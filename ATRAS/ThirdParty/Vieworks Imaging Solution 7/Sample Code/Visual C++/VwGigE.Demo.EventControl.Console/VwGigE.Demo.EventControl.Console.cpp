// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"

static HANDLE g_hAlarm = ::CreateEvent( NULL, FALSE, FALSE, NULL );

static void EventCallbackFunction( char* pcaEventName, VwGenICam::EVENT_DATA* ptEventData )
{
	_tprintf(_T("EventData Information:\n"));
	_tprintf(_T("\tBlockID: %I64d\n"), ptEventData->llnBlockID);
	_tprintf(_T("\tTimestamp: %I64d\n"), ptEventData->llnTimestamp);
	_tprintf(_T("\tEventID: %d\n"), ptEventData->wEventID);
	_tprintf(_T("\tStreamChannelIndex: %d\n"), ptEventData->wStreamChannelIndex);
	_tprintf(_T("\tDeviceHandle: 0x%0X\n"), ptEventData->pDeviceHandle);
	_tprintf(_T("\tUserPointer: 0x%0X\n"), ptEventData->pUserPointer);

	::SetEvent(g_hAlarm);
}

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VwGigE	vwGigE;
	vwGigE.UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	VWSDK::RESULT ret = vwGigE.Open();
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 1. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::VwCamera*	pvwCamera		= NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo		= new VWSDK::OBJECT_INFO;

	ret = vwGigE.OpenCamera( (UINT)0/*Device Index*/,
								&pvwCamera,
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
		vwGigE.Close();
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 2. Set Event Control
	_tprintf(_T("Step 2. Set Event Control.....\n"));
	//Select EventSelector
	char caEventName[32] = { "ExposureEnd" };
	ret = pvwCamera->SetCustomCommand( "EventSelector", caEventName );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed to change (%s) : EventSelector-ExposureEnd\n"), VwErrorReport( ret ));
		goto TERMINATE;
	}

	//Set EventCallbackfunction
	UINT unLen = (UINT)strlen( caEventName );
	ret = pvwCamera->SetEventControlCallback( NULL, caEventName, unLen, EventCallbackFunction);
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed to execute (%s) : EventExposureEnd-%s\n"), VwErrorReport( ret ), caEventName);
		goto TERMINATE;
	}

	//EventNotification On
	ret = pvwCamera->SetCustomCommand( "EventNotification", "On" );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed to change (%s) : EventNotification-On\n"), VwErrorReport( ret ));
		goto TERMINATE;
	}

	// 3. Grab
	printf("Step 3. Grab.....\n");
	//Grab
	ret = pvwCamera->Grab();
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed to execute (%s) : Grab\n"), VwErrorReport( ret ));
		goto TERMINATE;
	}

	// Timeout
	WaitForSingleObject( g_hAlarm , 10000/*msec*/ );
	::CloseHandle( g_hAlarm );
	g_hAlarm = NULL;

	//EventNotification Off
	ret = pvwCamera->SetCustomCommand("EventNotification", "Off");
	if (VWSDK::RESULT_SUCCESS != ret)
	{
		_tprintf(_T("Failed to change (%s) : EventNotification-Off\n"), VwErrorReport(ret));
		goto TERMINATE;
	}

	// 4. Abort
	printf("Step 4. Abort.....\n");
	//Grab
	ret = pvwCamera->Abort();
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed to execute (%s) : Abort\n"), VwErrorReport( ret ));
		goto TERMINATE;
	}

TERMINATE:
	// 5. Terminate
	_tprintf(_T("Step 5. Terminate.....\n"));
	if( pObjectInfo )
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if ( pvwCamera )
	{
		pvwCamera->CloseCamera();
		pvwCamera = NULL;
	}

	vwGigE.Close();

	return 0;
}

