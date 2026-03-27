// VwUSB.Demo.USB.Discovery.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CVwUSBDemoUSBDiscoveryApp:
// See VwUSB.Demo.USB.Discovery.cpp for the implementation of this class
//

class CUSBDemoUSBDiscoveryApp : public CWinApp
{
public:
	CUSBDemoUSBDiscoveryApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CUSBDemoUSBDiscoveryApp theApp;