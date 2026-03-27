// VwGigE.Demo.GigE.Discovery.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CVwGigEDemoGigEDiscoveryApp:
// See VwGigE.Demo.GigE.Discovery.cpp for the implementation of this class
//

class CVwGigEDemoGigEDiscoveryApp : public CWinApp
{
public:
	CVwGigEDemoGigEDiscoveryApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CVwGigEDemoGigEDiscoveryApp theApp;