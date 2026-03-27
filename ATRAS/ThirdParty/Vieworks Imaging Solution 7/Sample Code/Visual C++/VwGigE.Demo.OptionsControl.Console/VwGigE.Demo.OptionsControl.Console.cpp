// Descriptions

#include "stdafx.h"
// for VwGigE
#include "VwGigE.h"
#include "vwinterface.h"

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
	
	_tprintf(_T("Step 1. Set options\n"));
	// 1. Set Global Options
	// 1-1. Timeout of discovery
	UINT unTimeout_ms = 0;
	vwGigE.GetDiscoveryTimeout( unTimeout_ms );
	_tprintf(_T("\tCurrent DiscoveryTimeout : %d\n"), unTimeout_ms);

	UINT unNewTimeout_ms = unTimeout_ms + 1000/*msec*/;
	vwGigE.SetDiscoveryTimeout( unNewTimeout_ms );
	_tprintf(_T("\tNew DiscoveryTimeout : %d\n"), unNewTimeout_ms);
	
	// 1-2. Ignore Subnet
	bool bIgnore = FALSE;
	vwGigE.GetIgnoreSubnet( bIgnore );
	_tprintf(_T("\tCurrent IgnoreSubnet : %d\n"), bIgnore);

	bool bNewIgnore = ! bIgnore;
	vwGigE.SetIgnoreSubnet( bNewIgnore );
	_tprintf(_T("\tNew IgnoreSubnet : %d\n"), bNewIgnore);

	// 1-3. GVCPTimeout
	UINT unGVCPTimeout_ms = 0;
	vwGigE.GetGVCPTimeout( unGVCPTimeout_ms );
	_tprintf(_T("\tCurrent GVCPTimeout : %d\n"), unGVCPTimeout_ms);

	UINT unNewGVCPTimeout_ms = unGVCPTimeout_ms + 100/*msec*/;
	vwGigE.SetGVCPTimeout( unNewGVCPTimeout_ms );
	_tprintf(_T("\tNew GVCPTimeout : %d\n"), unNewGVCPTimeout_ms);

	// 1-4. MTUOptimize
	vwGigE.UseMTUOptimize( TRUE );
	_tprintf(_T("\tTurn on MTUOptimize.\n"));
	//VwGigE.UseMTUOptimize( FALSE );	//	Default setting is FALSE.
	
	// 2. Open interface
	_tprintf(_T("Step 2. Open Interface.....\n"));
	VWSDK::VwInterface*	pvwInterface	= NULL;
	ret = vwGigE.OpenInterface( (UINT)0, &pvwInterface  );
	if( VWSDK::RESULT_SUCCESS != ret )
	{
		goto TERMINATE;
	}

	// 2-1. ImageReceivingTimout
	UINT unImageReceivingTimeout	= 0;
	pvwInterface->GetImageReceivingTimeout( unImageReceivingTimeout );
	_tprintf(_T("\tCurrent ImageReceivingTimout : %d\n"), unImageReceivingTimeout);

	UINT unNewImageReceivingTimeout	= unImageReceivingTimeout + 1000/*msec*/;
	pvwInterface->SetImageReceivingTimeout( unNewImageReceivingTimeout );
	_tprintf(_T("\tNew ImageReceivingTimout : %d\n"), unNewImageReceivingTimeout);

	// 2-2. ImageRetransTimeout
	UINT unImageRetransTimeout		= 0;
	pvwInterface->GetImageRetransTimeout( unImageRetransTimeout );
	_tprintf(_T("\tCurrent ImageRetransTimeout : %d\n"), unImageRetransTimeout);
	
	UINT unNewImageRetransTimeout	= unImageRetransTimeout + 100/*msec*/;
	pvwInterface->SetImageRetransTimeout( unNewImageRetransTimeout );
	_tprintf(_T("\tNew ImageRetransTimeout : %d\n"), unNewImageRetransTimeout);

	// 2-3. PacketResend
	pvwInterface->UsePacketResend( FALSE );
	//pvwInterface->UsePacketResend( TRUE );	// Default setting is TRUE
	_tprintf(_T("\tTurn off PacketResend\n"));
	

	// Recover all options
	_tprintf(_T("\n\tRecover all options..\n"));
	vwGigE.SetDiscoveryTimeout( unTimeout_ms );
	_tprintf(_T("\tDiscoveryTimeout : %d\n"), unTimeout_ms);
	
	vwGigE.SetIgnoreSubnet( bIgnore );
	_tprintf(_T("\tIgnoreSubnet : %d\n"), bIgnore);

	vwGigE.SetGVCPTimeout( unGVCPTimeout_ms );
	_tprintf(_T("\tGVCPTimeout : %d\n"), unGVCPTimeout_ms);
	
	vwGigE.UseMTUOptimize( FALSE );
	_tprintf(_T("\tTurn off MTUOptimize.\n"));

	pvwInterface->SetImageReceivingTimeout( unImageReceivingTimeout );
	_tprintf(_T("\tImageReceivingTimout : %d\n"), unImageReceivingTimeout);

	pvwInterface->SetImageRetransTimeout( unImageRetransTimeout );
	_tprintf(_T("\tImageRetransTimeout : %d\n"), unImageRetransTimeout);

	pvwInterface->UsePacketResend( TRUE );
	_tprintf(_T("\tTurn on PacketResend\n"));

TERMINATE:
	// 3. Terminate
	_tprintf(_T("Step 3. Terminate.....\n"));
	if ( pvwInterface )
	{
		pvwInterface->CloseInterface();
		pvwInterface = NULL;
	}

	vwGigE.Close();

	return 0;
}

