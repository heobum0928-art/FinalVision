
#pragma once

#ifndef WINVER				
#define WINVER 0x0501		
#endif

#ifndef _WIN32_WINNT		
#define _WIN32_WINNT 0x0501	
#endif						

#ifndef _WIN32_WINDOWS		
#define _WIN32_WINDOWS 0x0410 
#endif

#ifndef _WIN32_IE			
#define _WIN32_IE 0x0600	
#endif

#include <stdio.h>
#include <tchar.h>
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN		
#endif

#include <afx.h>
#include <afxwin.h>         
#include <afxext.h>         
#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdtctl.h>		
#endif
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			
#endif 

#include <iostream>

