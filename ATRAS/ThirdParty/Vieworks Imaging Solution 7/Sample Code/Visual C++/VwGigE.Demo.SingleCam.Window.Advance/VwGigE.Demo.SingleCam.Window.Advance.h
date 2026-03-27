// VwGigE.Demo.SingleCam.Window.Advance.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CVwGigEDemoSingleCamWindowAdvanceApp:
// See VwGigE.Demo.SingleCam.Window.Advance.cpp for the implementation of this class
//

class CVwGigEDemoSingleCamWindowAdvanceApp : public CWinApp
{
public:
	CVwGigEDemoSingleCamWindowAdvanceApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CVwGigEDemoSingleCamWindowAdvanceApp theApp;