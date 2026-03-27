#pragma once

#include "VwSDK.h"

class CVwResourceType
{
	// constructor
	CVwResourceType();

	// destructor
	~CVwResourceType();

public:
	static CString GetPixelFormatName( VWSDK::PIXEL_FORMAT ePixelFormat );
	static BOOL ConvertPixelFormat( VWSDK::PIXEL_FORMAT ePixelFormat, BYTE* pDest, BYTE* pSource, int nWidth, int nHeight );
	static int GetPixelBitCount( VWSDK::PIXEL_FORMAT ePixelFormat );
};
