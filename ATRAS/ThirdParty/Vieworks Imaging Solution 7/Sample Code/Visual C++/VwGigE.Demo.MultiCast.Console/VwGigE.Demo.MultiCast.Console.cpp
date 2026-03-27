// Descriptions

#include "stdafx.h"
#include "conio.h"
// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"

static void	GetImageCallback(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo);

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
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 1. Set MULTICAST options
	vwGigE.SetMultiCastAddress( 0xe0000501/*224.0.5.1*/ );

	// 2. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::VwCamera*		pCamera		= NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo		= new VWSDK::OBJECT_INFO;

	// The device will be opened CONTROL mode.
	ret = vwGigE.OpenCamera( (UINT)0/*Device Index*/,
								&pCamera,
								2/*The number of buffer*/,
								0/*Auto*/,
								0/*Auto*/,
								0/*Auto*/,
								pObjectInfo/*User Pointer*/,
								GetImageCallback/*Image Callback*/ );
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

	// 2. Grab
	_tprintf(_T("Step 2. Start Grab\n"));
	_tprintf(_T("\tPlease check that the other machine can open the device in MONITOR mode.\n"));
	_tprintf(_T("\tAnd then try to grab. The same image will be shown.\n"));
	pCamera->Grab();

	_tprintf(_T("\t\tStop : press 'q' key.\n"));

	char cmdline;
	cmdline = _getche();
	
	if ( cmdline == 'q' )
	{
		pCamera->Abort();
	}
	
	
	// 3. Terminate
	_tprintf(_T("Step 3. Terminate.....\n"));
	if ( pObjectInfo )
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if ( pCamera )
	{
		pCamera->CloseCamera();
		pCamera = NULL;
	}

	vwGigE.Close();

	return 0;
}

void GetImageCallback( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo )
{
	if ( NULL == pImageInfo )
		return;

	_tprintf(_T("Image(%d)\r"), pImageInfo->bufferIndex );
}