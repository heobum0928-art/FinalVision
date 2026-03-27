//#pragma once

#ifndef VWIMAGEPROCESS_H
#define VWIMAGEPROCESS_H
#include <windows.h>


#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

namespace VWSDK
{

	static BYTE Const1[16] = { 0xf0, 0x0f, 0xff, 0x0f,
		0xf0, 0x0f, 0xff, 0x0f,
		0xf0, 0x0f, 0xff, 0x0f,
		0xf0, 0x0f, 0xff, 0x0f };

	static BYTE Const2[16] = { 0, 0xf0, 0, 0,
		0, 0xf0, 0, 0,
		0, 0xf0, 0, 0,
		0, 0xf0, 0, 0 };


	class CLASS_IMPORTEXPORT VwImageProcess
	{
	public:
		VwImageProcess(void);
	public:
		~VwImageProcess(void);

		static void ConvertBAYBG10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYBG12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYBG8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYBGToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		static void ConvertBAYGB10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGB10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGB12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGB12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGB8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGBToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		static void ConvertBAYGR10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGR10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGR12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGR12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGR8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYGRToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		static void ConvertBAYRG10ToBGR10(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYRG10ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYRG12ToBGR12(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height);
		static void ConvertBAYRG12ToBGR8(const WORD* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYRG8ToBGR8(const BYTE* pbSrc, BYTE* pbDst, DWORD width, DWORD height);
		static void ConvertBAYRGToBGR(const WORD* pbSrc, WORD* pbDst, DWORD width, DWORD height, UINT unDataBitCount);
		static void ConvertBGR10pToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertBGR10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertBGR10V2PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertBGR12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertBGRpToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		static void ConvertBGRToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		static void ConvertBGRToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		static void ConvertMono10lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono10msbToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono10PackedlsbToMono16msb(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		static void ConvertMono10PackedToMono16bit(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		static void ConvertMono10PackedToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono10ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono10pToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono12lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono12msbToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono12PackedlsbToMono16msb(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		static void ConvertMono12PackedToMono16bit(const BYTE* pbSrc, UINT width, UINT height, BYTE* pbDst);
		static void ConvertMono12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono12ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono12pToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono14lsbToMono16msb(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono14ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono14ToMono8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono16PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMono8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertMonoPackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGB10pToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGB10ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGB12PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGB12ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGB8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertRGBpToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		static void ConvertRGBToBGR(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, UINT unDataBitcount);
		static void ConvertYUV411PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYUV411ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYUV422_UYVYToBGR8(const BYTE* pbSrc, const unsigned int nWidth, const unsigned int nHeight, BYTE* pbDst);
		static void ConvertYUV422_YUYVToBGR8(const BYTE* pbSrc, const unsigned int nWidth, const unsigned int nHeight, BYTE* pbDst);
		static void ConvertYUV422PackedToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYUV422ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYUV422ToBGR8Interlaced(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst, bool bOdd, DWORD width, bool blend, bool _signed);
		static void ConvertYUV444ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYCbCr8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYCbCr8_CbYCrToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
		static void ConvertYCbCr411_8ToBGR8(const BYTE* pbSrc, DWORD cbSrc, BYTE* pbDst);
	};

}//namespace VWSDK

#endif