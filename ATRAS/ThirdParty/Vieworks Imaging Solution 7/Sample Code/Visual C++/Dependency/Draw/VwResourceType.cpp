#include "stdafx.h"
#include "VwResourceType.h"
#include "VwGigE.API.h"
#include "VwImageProcess.API.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// Constructor
CVwResourceType::CVwResourceType()
{

}

// Destructor
CVwResourceType::~CVwResourceType()
{
}

CString CVwResourceType::GetPixelFormatName( VWSDK::PIXEL_FORMAT ePixelFormat )
{
	CString strTmp;

	switch( ePixelFormat )
	{
	case VWSDK::PIXEL_FORMAT_MONO8:
		strTmp = _T("Mono8");
		break;
	case VWSDK::PIXEL_FORMAT_MONO8_SIGNED:
		strTmp = _T("Mono8signed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
		strTmp = _T("Mono10");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_P:
		strTmp = _T("Mono10p");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
		strTmp = _T("Mono10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12:
		strTmp = _T("Mono12");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12_P:
		strTmp = _T("Mono12p");
		break; 
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		strTmp = _T("Mono12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO14:
		strTmp = _T("Mono14");
		break;
	case VWSDK::PIXEL_FORMAT_MONO16:
		strTmp = _T("Mono16");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR8:
		strTmp = _T("BayerGR8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG8:
		strTmp = _T("BayerRG8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB8:
		strTmp = _T("BayerGB8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG8:
		strTmp = _T("BayerBG8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10:
		strTmp = _T("BayerGR10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10:
		strTmp = _T("BayerRG10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB10:
		strTmp = _T("BayerGB10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG10:
		strTmp = _T("BayerBG10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
		strTmp = _T("BayerGR10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
		strTmp = _T("BayerRG10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12:
		strTmp = _T("BayerGR12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12:
		strTmp = _T("BayerRG12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB12:
		strTmp = _T("BayerGB12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG12:
		strTmp = _T("BayerBG12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		strTmp = _T("BayerRG12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
		strTmp = _T("BayerGR12Packed");
		break;	
	case VWSDK::PIXEL_FORMAT_RGB8:
		strTmp = _T("RGB8");
		break;
	case VWSDK::PIXEL_FORMAT_BGR8:
		strTmp = _T("BGR8");
		break;
	case VWSDK::PIXEL_FORMAT_RGB10:
		strTmp = _T("RGB10");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10:
		strTmp = _T("BGR10");
		break;
	case VWSDK::PIXEL_FORMAT_RGB12:
		strTmp = _T("RGB12");
		break;
	case VWSDK::PIXEL_FORMAT_BGR12:
		strTmp = _T("BGR12");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
		strTmp = _T("YUV422Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
		strTmp = _T("YUV422YUVYPacked");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
		strTmp = _T("YUV42210Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		strTmp = _T("YUV42212Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
		strTmp = _T("YUV411");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
		strTmp = _T("YUV41110Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
		strTmp = _T("YUV41112Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		strTmp = _T("BGR10V1Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V2_PACKED:
		strTmp = _T("BGR10V2Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV444:
		strTmp = _T("YUV444");
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
		strTmp = _T("PALInterlaced");
		break;
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		strTmp = _T("NTSCInterlaced");
		break;
	default:
		strTmp = _T("Unrecognized pixel format\n");
	}

	return strTmp;
}

BOOL CVwResourceType::ConvertPixelFormat( VWSDK::PIXEL_FORMAT ePixelFormat, BYTE* pDest, BYTE* pSource, int nWidth, int nHeight )
{
	if ( NULL == pDest ||
		 NULL == pSource )
	{
		return FALSE;
	}
	
	if ( 0 == nWidth || 0 == nHeight )
	{
		return FALSE;
	}

	BOOL bRet = TRUE;
	BYTE* bpConvertPixelFormat =	new BYTE[ nWidth * nHeight * 2 ];

	switch( ePixelFormat )
	{
		//-----------------------------------------------------------------
		// about MONO Pixel Format Series ---------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_MONO8:
		memcpy( pDest, pSource, nWidth * nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
		VWSDK::ConvertMono10ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_P:
		VWSDK::ConvertMono10pToMono8(PBYTE(pSource), nWidth*nHeight * 1.25, bpConvertPixelFormat);
		VWSDK::ConvertMono8ToBGR8(bpConvertPixelFormat, nWidth*nHeight, pDest);
		break;
	case VWSDK::PIXEL_FORMAT_MONO12:
		VWSDK::ConvertMono12ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break; 
	case VWSDK::PIXEL_FORMAT_MONO12_P:
		VWSDK::ConvertMono12pToMono8(PBYTE(pSource), nWidth*nHeight * 1.5, bpConvertPixelFormat);
		VWSDK::ConvertMono8ToBGR8(bpConvertPixelFormat, nWidth*nHeight, pDest);
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		VWSDK::ConvertMonoPackedToBGR8( pSource,
												UINT(1.5*nWidth*nHeight),
												pDest );
		break;

	case VWSDK::PIXEL_FORMAT_MONO14:
		VWSDK::ConvertMono14ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break;

	case VWSDK::PIXEL_FORMAT_MONO16:
		VWSDK::ConvertMono16PackedToBGR8( pSource,
												UINT(2*nWidth*nHeight),
												pDest );
		break;
		//-----------------------------------------------------------------
		// about BAYER Pixel Format Series --------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_BAYGR8:
		VWSDK::ConvertBAYGR8ToBGR8( pSource,
											pDest,
											nWidth,
											nHeight );
		break;

/// Delete::20150629, Dong-Uk Lee, Basler USB 카메라 테스트용
// 	case VWSDK::PIXEL_FORMAT_BAYBG8:
// 		ConvertBAYBG8ToBGR8( pSource,
// 											pDest,
// 											nWidth,
// 											nHeight );
// 		break;
/// Delete::20150629, Dong-Uk Lee, 
/// 
	case VWSDK::PIXEL_FORMAT_BAYRG8:
		VWSDK::ConvertBAYRG8ToBGR8( pSource,
											pDest,
											nWidth,
											nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10:
		VWSDK::ConvertBAYGR10ToBGR8( (WORD*)(pSource),
											pDest,
											nWidth,
											nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10:
		VWSDK::ConvertBAYRG10ToBGR8( (WORD*)(pSource),
											pDest,
											nWidth,
											nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12:
		VWSDK::ConvertBAYGR12ToBGR8( (WORD*)(pSource),
											pDest,
											nWidth,
											nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12:
		VWSDK::ConvertBAYRG12ToBGR8( (WORD*)(pSource),
											pDest,
											nWidth,
											nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
		VWSDK::ConvertMono10PackedToMono16bit( (PBYTE)pSource,
														nWidth,
														nHeight,
														bpConvertPixelFormat );
		VWSDK::ConvertBAYGR10ToBGR8( (WORD*)bpConvertPixelFormat,
												pDest,
												nWidth,
												nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
		VWSDK::ConvertMono10PackedToMono16bit( (PBYTE)pSource,
														nWidth,
														nHeight,
														bpConvertPixelFormat );
		VWSDK::ConvertBAYRG10ToBGR8( (WORD*)bpConvertPixelFormat,
												pDest,
												nWidth,
												nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
		VWSDK::ConvertMono12PackedToMono16bit( (PBYTE)pSource,
														nWidth,
														nHeight,
														bpConvertPixelFormat );
		VWSDK::ConvertBAYGR12ToBGR8( (WORD*)bpConvertPixelFormat,
												pDest,
												nWidth,
												nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		VWSDK::ConvertMono12PackedToMono16bit( (PBYTE)pSource,
														nWidth,
														nHeight,
														bpConvertPixelFormat );
		VWSDK::ConvertBAYRG12ToBGR8( (WORD*)bpConvertPixelFormat,
													pDest,
													nWidth,
													nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_RGB8:
		VWSDK::ConvertRGB8ToBGR8( (PBYTE)pSource,
											UINT(3*nWidth*nHeight),
											pDest );
		break;
	case VWSDK::PIXEL_FORMAT_BGR8:
		memcpy(pDest, pSource, UINT(3 * nWidth*nHeight));
		break;
	case VWSDK::PIXEL_FORMAT_RGB10:
		VWSDK::ConvertRGB10ToBGR8((PBYTE)pSource,
			UINT(6 * nWidth*nHeight),
			pDest);
		break;
	case VWSDK::PIXEL_FORMAT_BGR10:
		VWSDK::ConvertBGR10ToBGR8((PBYTE)pSource,
			UINT(6 * nWidth*nHeight),
			pDest);
		break;
	case VWSDK::PIXEL_FORMAT_RGB12:
		VWSDK::ConvertRGB12ToBGR8((PBYTE)pSource,
			UINT(6 * nWidth*nHeight),
			pDest);
		break;
	case VWSDK::PIXEL_FORMAT_BGR12:
		VWSDK::ConvertBGR12ToBGR8((PBYTE)pSource,
			UINT(6 * nWidth*nHeight),
			pDest);
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
		VWSDK::ConvertYUV411ToBGR8( (PBYTE)pSource,
											UINT(1.5*nWidth*nHeight),
											pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
		VWSDK::ConvertYUV422_UYVYToBGR8( (PBYTE)pSource,
												nWidth,
												nHeight,
												pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
		VWSDK::ConvertYUV422_YUYVToBGR8( (PBYTE)pSource,
												nWidth,
												nHeight,
												pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV444:
		VWSDK::ConvertYUV444ToBGR8( (PBYTE)pSource,
											UINT(1.5*nWidth*nHeight),
											pDest );
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		bRet = FALSE;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
		VWSDK::ConvertYUV411PackedToBGR8( (PBYTE)pSource,
													UINT(2.25*nWidth*nHeight),
													pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		VWSDK::ConvertYUV422PackedToBGR8( (PBYTE)pSource,
													UINT(3*nWidth*nHeight),
													pDest );
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		break;
	default:
		{
			bRet = FALSE;
		}
	}

	if ( NULL != bpConvertPixelFormat )
	{
		delete [] bpConvertPixelFormat;
	}
	
	return bRet;
}

int CVwResourceType::GetPixelBitCount( VWSDK::PIXEL_FORMAT ePixelFormat )
{
	int nRet = 0;
	switch( ePixelFormat )
	{
		//-----------------------------------------------------------------
		// about MONO Pixel Format Series ---------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_MONO8:
		nRet	=	8;
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
	case VWSDK::PIXEL_FORMAT_MONO12:
	case VWSDK::PIXEL_FORMAT_MONO10_P:
	case VWSDK::PIXEL_FORMAT_MONO12_P:
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO14:
	case VWSDK::PIXEL_FORMAT_MONO16:
		nRet	=	24;
		break;
		//-----------------------------------------------------------------
		// about BAYER Pixel Format Series --------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_BAYGR8:
	case VWSDK::PIXEL_FORMAT_BAYRG8:
	case VWSDK::PIXEL_FORMAT_BAYGR10:
	case VWSDK::PIXEL_FORMAT_BAYRG10:
	case VWSDK::PIXEL_FORMAT_BAYGR12:
	case VWSDK::PIXEL_FORMAT_BAYRG12:
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_RGB8:
	case VWSDK::PIXEL_FORMAT_BGR8:
	case VWSDK::PIXEL_FORMAT_RGB10:
	case VWSDK::PIXEL_FORMAT_BGR10:
	case VWSDK::PIXEL_FORMAT_RGB12:
	case VWSDK::PIXEL_FORMAT_BGR12:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V2_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
	case VWSDK::PIXEL_FORMAT_YUV444:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		nRet	=	32;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		nRet	=	24;
		break;
	default:
		{
		}
	}

	return nRet;
}