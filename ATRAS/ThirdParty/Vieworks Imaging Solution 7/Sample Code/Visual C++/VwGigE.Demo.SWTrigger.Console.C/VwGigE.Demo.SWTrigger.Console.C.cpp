// VwGigE.Demo.SWTrigger.Console.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "VwGigE.Api.h"
#include "conio.h"

using namespace VWSDK;

static void	GetImageEvent(OBJECT_INFO* pObjectInfo,IMAGE_INFO* pImageInfo); //Image capture event

HANDLE hAlarm = ::CreateEvent(NULL, FALSE, FALSE, NULL);

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWGIGE_HANDLE	vwGigE		= NULL;
	RESULT ret = OpenVwGigE( &vwGigE );
	if ( RESULT_SUCCESS != ret )
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
	HCAMERA			hCamera	= NULL;
	OBJECT_INFO*	pObjectInfo	= new OBJECT_INFO;
	ret = VwOpenCameraByIndex( vwGigE,
		0/*Device Index*/,
		&hCamera,
		2/*The number of buffer*/,
		0/*Auto*/,
		0/*Auto*/,
		0/*Auto*/,
		pObjectInfo/*User Pointer*/,
		GetImageEvent);

	if ( RESULT_SUCCESS != ret )
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
			CloseVwGigE( vwGigE );
			vwGigE	= NULL;
		}
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 2. Extend AcquisitionTimeout
	CameraSetAcquisitionTimeOut( hCamera, 0/*Infinite*/ );		// Wait for image until stop grabbing.

	// 3. Set Trigger Mode
	_tprintf(_T("Step 2. Set Trigger Mode.....\n"));

	const unsigned int ARRAY_MAX				= 128;
	char caTriggerMode[ARRAY_MAX]				= {"TriggerMode"};
	char caTriggerModeValue[ARRAY_MAX]			= {"On"};
	char caTriggerModePrevValue[ARRAY_MAX]		= {NULL,};

	size_t nPrevValueSize						= ARRAY_MAX;

	// Save current 'TriggerMode' value in order to recover original state.
	ret = CameraGetCustomCommand( hCamera, caTriggerMode, caTriggerModePrevValue, &nPrevValueSize, GET_CUSTOM_COMMAND_VALUE );
	if( RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}

	// TriggerMode - On	
	ret = CameraSetCustomCommand( hCamera, caTriggerMode, caTriggerModeValue );
	if( RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}

	char caTriggerSource[ARRAY_MAX]				= {"TriggerSource"};
	char caTriggerSourceValue[ARRAY_MAX]		= {"Software"};
	char caTriggerSourcePrevValue[ARRAY_MAX]	= {NULL,};

	nPrevValueSize								= ARRAY_MAX;

	// Save current 'TriggerSource' value in order to recover original state.
	ret = CameraGetCustomCommand( hCamera, caTriggerSource, caTriggerSourcePrevValue, &nPrevValueSize, GET_CUSTOM_COMMAND_VALUE );
	if( RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}	

	// TriggerSource - Software
	ret = CameraSetCustomCommand( hCamera, caTriggerSource, caTriggerSourceValue );
	if( RESULT_SUCCESS !=ret )
	{
		goto TERMINATE;
	}

	Sleep(100);

	// 3. Grab
	_tprintf(_T("Step 3. Grab.....\n"));
	ret = CameraGrab( hCamera );
	if( RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}


	// 5. Wait Image
	_tprintf(_T("Step 4. Wait Image.....\n"));
	_tprintf(_T("\t\tSWTrigger : press 't' key.\n"));

	char cmdline;
	cmdline = _getche();

	if ( cmdline == 't' )
	{
		// 4. Trigger
		_tprintf(_T("\nStep 5. Trigger.....\n"));
		//Trigger Shot

		char caTriggerSoftware[ARRAY_MAX]			= {"TriggerSoftware"};

		ret = CameraSetCustomCommand( hCamera, caTriggerSoftware, NULL );
		if( RESULT_SUCCESS != ret )
		{
			goto TERMINATE;
		}
	}

	if( WAIT_TIMEOUT == ::WaitForSingleObject( hAlarm, 10000/*msec*/ ))
	{
		_tprintf(_T("\nTIME_OUT.....\n"));
		goto TERMINATE;
	}

TERMINATE:
	// 6. Terminate
	_tprintf(_T("Step 6. Terminate.....\n"));

	// Recover all properties
	if( strcmp(caTriggerModePrevValue, "") != 0)
	{
		ret = CameraSetCustomCommand( hCamera, caTriggerMode, caTriggerModePrevValue );
	}

	if( strcmp(caTriggerSourcePrevValue, "") != 0)
	{
		ret = CameraSetCustomCommand( hCamera, caTriggerMode, caTriggerSourcePrevValue );
	}


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

	if ( vwGigE)
	{
		CloseVwGigE( vwGigE );
		vwGigE	= NULL;
	}

	return 0;
}

void GetImageEvent(OBJECT_INFO* pObjectInfo,IMAGE_INFO* pImageInfo)
{
	::SetEvent( hAlarm );
	_tprintf(_T("\t**Image Received successfully.**\n"));
}