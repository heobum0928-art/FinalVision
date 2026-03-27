// VwGigE.Demo.OptionsControl.Console.C.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
// for VwGigE
#include "VwGigE.API.h"

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize.....\n"));
	VWSDK::VWGIGE_HANDLE	vwGigE		= NULL;
	VWSDK::RESULT ret					= VWSDK::OpenVwGigE( &vwGigE );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		return -1;
	}

	VWSDK::VwUserLogging(vwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	_tprintf(_T("Step 1. Set options\n"));
	// 1. Set Global Options
	// 1-1. Timeout of discovery
	UINT unTimeout_ms = 0;
	VWSDK::VwGetDiscoveryTimeout( vwGigE, unTimeout_ms );
	_tprintf(_T("\tCurrent DiscoveryTimeout : %d\n"), unTimeout_ms);

	UINT unNewTimeout_ms = unTimeout_ms + 1000/*msec*/;
	VWSDK::VwSetDiscoveryTimeout( vwGigE, unNewTimeout_ms );
	_tprintf(_T("\tNew DiscoveryTimeout : %d\n"), unNewTimeout_ms);

	// 1-2. Ignore Subnet
	bool bIgnore = FALSE;
	VWSDK::VwGetIgnoreSubnet( vwGigE, bIgnore );
	_tprintf(_T("\tCurrent IgnoreSubnet : %d\n"), bIgnore);

	bool bNewIgnore = ! bIgnore;
	VWSDK::VwSetIgnoreSubnet( vwGigE, bNewIgnore );
	_tprintf(_T("\tNew IgnoreSubnet : %d\n"), bNewIgnore);

	// 1-3. GVCPTimeout
	UINT unGVCPTimeout_ms = 0;
	VWSDK::VwGetGVCPTimeout( vwGigE, unGVCPTimeout_ms );
	_tprintf(_T("\tCurrent GVCPTimeout : %d\n"), unGVCPTimeout_ms);

	UINT unNewGVCPTimeout_ms = unGVCPTimeout_ms + 100/*msec*/;
	VWSDK::VwSetGVCPTimeout( vwGigE, unNewGVCPTimeout_ms );
	_tprintf(_T("\tNew GVCPTimeout : %d\n"), unNewGVCPTimeout_ms);

	// 1-4. MTUOptimize
	VWSDK::VwUseMTUOptimize( vwGigE, TRUE );
	_tprintf(_T("\tTurn on MTUOptimize.\n"));
	//VwGigE.UseMTUOptimize( FALSE );	//	Default setting is FALSE.

	// 2. Open interface
	_tprintf(_T("Step 2. Open Interface.....\n"));
	VWSDK::HINTERFACE		pvwInterface	= NULL;
	ret = VWSDK::VwOpenInterfaceByIndex( vwGigE, (UINT)0, &pvwInterface );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}

	// 2-1. ImageReceivingTimout
	UINT unImageReceivingTimeout	= 0;
	VWSDK::InterfaceGetImageReceivingTimeout( pvwInterface, unImageReceivingTimeout );
	_tprintf(_T("\tCurrent ImageReceivingTimout : %d\n"), unImageReceivingTimeout);

	UINT unNewImageReceivingTimeout	= unImageReceivingTimeout + 1000/*msec*/;
	VWSDK::InterfaceSetImageReceivingTimeout( pvwInterface, unNewImageReceivingTimeout );
	_tprintf(_T("\tNew ImageReceivingTimout : %d\n"), unNewImageReceivingTimeout);

	// 2-2. ImageRetransTimeout
	UINT unImageRetransTimeout		= 0;
	VWSDK::InterfaceGetImageRetransTimeout( pvwInterface, unImageRetransTimeout );
	_tprintf(_T("\tCurrent ImageRetransTimeout : %d\n"), unImageRetransTimeout);

	UINT unNewImageRetransTimeout	= unImageRetransTimeout + 100/*msec*/;
	VWSDK::InterfaceSetImageRetransTimeout( pvwInterface, unNewImageRetransTimeout );
	_tprintf(_T("\tNew ImageRetransTimeout : %d\n"), unNewImageRetransTimeout);

	// 2-3. PacketResend
	VWSDK::InterfaceUsePacketResend( pvwInterface, FALSE );
	//InterfaceUsePacketResend( TRUE );	// Default setting is TRUE
	_tprintf(_T("\tTurn off PacketResend"));


	// Recover all options
	_tprintf(_T("\n\n\tRecover all options..\n"));
	VWSDK::VwSetDiscoveryTimeout( vwGigE, unTimeout_ms );
	_tprintf(_T("\tDiscoveryTimeout : %d\n"), unTimeout_ms);

	VWSDK::VwSetIgnoreSubnet( vwGigE, bIgnore );
	_tprintf(_T("\tIgnoreSubnet : %d\n"), bIgnore);

	VWSDK::VwSetGVCPTimeout( vwGigE, unGVCPTimeout_ms );
	_tprintf(_T("\tGVCPTimeout : %d\n"), unGVCPTimeout_ms);

	VWSDK::VwUseMTUOptimize( vwGigE, FALSE );
	_tprintf(_T("\tTurn off MTUOptimize.\n"));

	
	VWSDK::InterfaceSetImageReceivingTimeout( pvwInterface, unImageReceivingTimeout );
	_tprintf(_T("\tImageReceivingTimout : %d\n"), unImageReceivingTimeout);

	VWSDK::InterfaceSetImageRetransTimeout( pvwInterface, unImageRetransTimeout );
	_tprintf(_T("\tImageRetransTimeout : %d\n"), unImageRetransTimeout);

	VWSDK::InterfaceUsePacketResend( pvwInterface, TRUE );
	_tprintf(_T("\tTurn on PacketResend\n"));

TERMINATE:
	// 3. Terminate
	_tprintf(_T("Step 3. Terminate.....\n"));
	if ( pvwInterface )
	{
		VWSDK::InterfaceCloseInterface( pvwInterface );
		pvwInterface = NULL;
	}

	if ( vwGigE )
	{
		VWSDK::CloseVwGigE( vwGigE );
		vwGigE = NULL;
	}

	return 0;
}


