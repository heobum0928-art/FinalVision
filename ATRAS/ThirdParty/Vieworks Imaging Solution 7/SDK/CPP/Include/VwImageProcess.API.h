
//#pragma once
#ifndef VWIMAGEPROCESS_API_H
#define VWIMAGEPROCESS_API_H

#define IN
#define OUT
#define INOUT

#include <Windows.h>

#ifdef _USRDLL
#define FUNCTION_IMPORTEXPORT __declspec(dllexport)
#else if
#define FUNCTION_IMPORTEXPORT __declspec(dllimport)
#endif


#ifdef __cplusplus
extern "C"
{
#endif

	namespace VWSDK
	{
		FUNCTION_IMPORTEXPORT void ConvertBAYBG10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYBG12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYBG8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYBGToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		FUNCTION_IMPORTEXPORT void ConvertBAYGB10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGB10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGB12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGB12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGB8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGBToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		FUNCTION_IMPORTEXPORT void ConvertBAYGR10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGR10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGR12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGR12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGR8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYGRToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		FUNCTION_IMPORTEXPORT void ConvertBAYRG10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYRG10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYRG12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYRG12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYRG8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		FUNCTION_IMPORTEXPORT void ConvertBAYRGToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		FUNCTION_IMPORTEXPORT void ConvertBGR10pToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertBGR10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertBGR10V2PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertBGR12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertBGRpToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		FUNCTION_IMPORTEXPORT void ConvertBGRToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		FUNCTION_IMPORTEXPORT void ConvertBGRToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		FUNCTION_IMPORTEXPORT void ConvertMono10lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10msbToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10PackedlsbToMono16msb(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10PackedToMono16bit(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10PackedToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono10pToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12msbToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12PackedlsbToMono16msb(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12PackedToMono16bit(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono12pToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono14lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono14ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono14ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono16PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMono8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertMonoPackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGB10pToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGB10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGB12PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGB12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGB8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertRGBpToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		FUNCTION_IMPORTEXPORT void ConvertRGBToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		FUNCTION_IMPORTEXPORT void ConvertYUV411PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV411ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV422_UYVYToBGR8(const BYTE* pbSrc, const unsigned int nWidth, const unsigned int nHeight, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV422_YUYVToBGR8(const BYTE* pbSrc, const unsigned int nWidth, const unsigned int nHeight, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV422PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV422ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYUV422ToBGR8Interlaced(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, bool bOdd, DWORD width, bool blend, bool _signed);
		FUNCTION_IMPORTEXPORT void ConvertYUV444ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYCbCr8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYCbCr8_CbYCrToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		FUNCTION_IMPORTEXPORT void ConvertYCbCr411_8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);

	}//namespace VWSDK

#ifdef __cplusplus
}
#endif

#endif
