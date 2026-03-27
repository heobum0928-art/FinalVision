// Descriptions

#include "stdafx.h"

// for VwGigE
#include "VwGigE.API.h"

#define MAC_DIGIT	6

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VWGIGE_HANDLE	vwGigE		= NULL;
	VWSDK::RESULT ret					= VWSDK::OpenVwGigE( &vwGigE );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	VWSDK::VwUserLogging(vwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	// Display information of current device.
	VWSDK::CAMERA_INFO_STRUCT tPrevCameraInfo;
	ret = VWSDK::VwDiscoveryCameraInfo( vwGigE, (UINT)0/*index*/, &tPrevCameraInfo );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		// Terminate
		VWSDK::CloseVwGigE( vwGigE );
		return -1;
	}
	_tprintf(_T("\t<Device Information>\n"));
	printf("\tDevice Name	: %s\n", tPrevCameraInfo.name);
	printf("\tMAC Address	: %s\n", tPrevCameraInfo.mac);
	printf("\tIP Address	: %s\n", tPrevCameraInfo.ip);

	// Change the MAC Address string to Hex
	unsigned __int8 arrbtMAC[MAC_DIGIT] = {0,};
	UINT unCount = 0;
	char* pchtoken = strtok(tPrevCameraInfo.mac, ":");
	while( NULL != pchtoken )
	{
		char* pchPtr = NULL;
		arrbtMAC[ unCount ] = strtoul( pchtoken, &pchPtr, 16 ); 
		unCount ++;
		pchtoken = strtok(NULL, ":");
	}

	// 1. Set IP address to a device.
	DWORD dwIP = 0xA9FE007B;/*169.254.0.123*/
	_tprintf(_T("Step 1. Set ForceIP : %d.%d.%d.%d....\n"), ( dwIP & 0xFF000000 ) >> 24,
		( dwIP & 0x00FF0000 ) >> 16,
		( dwIP & 0x0000FF00 ) >> 8,
		( dwIP & 0x000000FF ) );
	ret = VWSDK::VwForceIP( vwGigE, arrbtMAC, dwIP, 0xFFFF0000, 0 );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("\tFailed(err:%s))\n"), VwErrorReport(ret));
		// Terminate
		if ( vwGigE )
		{
			VWSDK::CloseVwGigE( vwGigE );
			vwGigE = NULL;
		}
		return -1;
	}
	_tprintf(_T("OK\n"));

	_tprintf(_T("\t<Result>\n"));
	UINT unCameraNum = 0;
	VWSDK::VwDiscovery( vwGigE );	// Discovery again
	VWSDK::VwGetNumCameras( vwGigE, &unCameraNum );
	for ( int i = 0; i < unCameraNum; i ++ )
	{
		VWSDK::CAMERA_INFO_STRUCT tCameraInfo;
		VWSDK::VwDiscoveryCameraInfo( vwGigE, i, &tCameraInfo );

		char chMAC[ MAC_DIGIT * 4 ] = {0,};
		sprintf( chMAC, "%d:%d:%d:%d:%d:%d", arrbtMAC[0],
											arrbtMAC[1],
											arrbtMAC[2],
											arrbtMAC[3],
											arrbtMAC[4],
											arrbtMAC[5] );

		if ( ! strcmp( chMAC, tCameraInfo.mac ) );
		{
			printf("\tCurrent MAC Address : %s\n", tCameraInfo.mac);
			printf("\tCurrent IP Address : %s\n", tCameraInfo.ip);
			break;
		}
	}

	// 2. Terminate
	_tprintf(_T("Step 2. Terminate.....\n"));
	VWSDK::CloseVwGigE( vwGigE );
}

