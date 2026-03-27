// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"


int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VwGigE	vwGigE;
	vwGigE.UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	VWSDK::RESULT	ret = vwGigE.Open();
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 1. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::VwCamera*		pvwCamera		= NULL;
	VWSDK::OBJECT_INFO*		pObjectInfo		= new VWSDK::OBJECT_INFO;

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
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
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

	// 2. Close File Stream
	_tprintf(_T("Step 2. Close the existing file stream.....\n"));
	//Close the exsting file selected by FileSelector in the device.
	char caArgument[32]   = {NULL, };
	size_t unArgumentSize = sizeof(caArgument);

	ret = pvwCamera->GetCustomCommand( "FileOperationSelector", caArgument, &unArgumentSize, VWSDK::GET_CUSTOM_COMMAND_VALUE );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("\tError occurred.....\n"));
		goto TERMINATE;
	}

	if( strcmp( caArgument, "Close" ) == 0 )
	{
		ret = pvwCamera->SetCustomCommand( "FileOperationSelector", "Close" );
		if( VWSDK::RESULT_SUCCESS != ret )
		{
			_tprintf(_T("\tError occurred.....\n"));
			goto TERMINATE;
		}

		ret = pvwCamera->SetCustomCommand( "FileOperationExecute", NULL );
		if( VWSDK::RESULT_SUCCESS != ret )
		{
			_tprintf(_T("\tError occurred.....\n"));
			goto TERMINATE;
		}
	}

	// 3. Write File Stream
	_tprintf(_T("Step 3. Write File Stream.....\n"));
	//WriteFileStream
	char caWriteBuffer[128] = {NULL, };
	size_t unWriteSize  = sizeof(caWriteBuffer);
	fputs("\tInput string to camera : ", stdout);
	gets_s(caWriteBuffer, unWriteSize);

	// The name of File Selector can differ depending on each device model.
	char* pchFileSelector = "UserData";
	ret = pvwCamera->WriteFileStream( pchFileSelector, caWriteBuffer, unWriteSize );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("\tError occurred.....\n"));
		goto TERMINATE;
	}

	// 4. Read File Stream
	_tprintf(_T("Step 4. Read File Stream.....\n"));
	//ReadFileStream
	char caReadBuffer[128] = {NULL, };
	size_t unReadSize  = sizeof(caReadBuffer);

	ret = pvwCamera->ReadFileStream( pchFileSelector, caReadBuffer, unReadSize );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("\tError occurred.....\n"));
		goto TERMINATE;
	}

	printf("\tRead string from camera : %s\n", caReadBuffer);

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

