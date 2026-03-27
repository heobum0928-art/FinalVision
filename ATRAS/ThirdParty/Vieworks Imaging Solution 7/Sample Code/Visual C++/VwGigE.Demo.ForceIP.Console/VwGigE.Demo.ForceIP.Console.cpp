// Descriptions

#include "stdafx.h"
// for VwGigE
#include "VwGigE.h"
#include "VwInterface.h"

#define MAC_DIGIT	6
#define IP_DIGIT	4

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

	// Display information of current device.
	VWSDK::VWCAMERA_INFO tPrevCameraInfo;
	ret = vwGigE.GetCameraInfo( (UINT)0/*index*/, &tPrevCameraInfo );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		// Terminate
		vwGigE.Close();
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
	DWORD dwIP = 0xA9FE007B;/*169.254.0.123*/;

	_tprintf(_T("Step 1. Set ForceIP : %d.%d.%d.%d....\n"), ( dwIP & 0xFF000000 ) >> 24,
														  ( dwIP & 0x00FF0000 ) >> 16,
														  ( dwIP & 0x0000FF00 ) >> 8,
														  ( dwIP & 0x000000FF ) );
	ret = vwGigE.SetForceIP( arrbtMAC, dwIP, 0xFFFF0000, 0 );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("\tFailed(err:%s))\n"), VwErrorReport(ret));
		// Terminate
		vwGigE.Close();
		return -1;
	}
	_tprintf(_T("OK\n"));

	_tprintf(_T("\t<Result>\n"));
	UINT unCameraNum = 0;
	vwGigE.Discovery();	// Discovery again
	vwGigE.GetNumCameras( &unCameraNum );
	for ( int i = 0; i < unCameraNum; i ++ )
	{
		VWSDK::VWCAMERA_INFO tCameraInfo;
		vwGigE.GetCameraInfo( i, &tCameraInfo );


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
	vwGigE.Close();

	return 0;
}

