using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Vieworks
{
    public class VwImageProcess
    {
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYBG10ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYBG12ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYBG8ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYBGToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGB10ToBGR10(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGB10ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGB12ToBGR12(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGB12ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGB8ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGBToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGR10ToBGR10(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGR10ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGR12ToBGR12(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGR12ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGR8ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYGRToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRG10ToBGR10(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRG10ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRG12ToBGR12(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRG12ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRG8ToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBAYRGToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGR10pToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGR10ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGR10V2PackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGR12ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGRpToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGRToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertBGRToBGR8(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10lsbToMono16msb(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10msbToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10PackedlsbToMono16msb(IntPtr pbSrc, int width, int height, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10PackedToMono16bit(IntPtr pbSrc, int width, int height, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10PackedToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10ToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono10pToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12lsbToMono16msb(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12msbToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12PackedlsbToMono16msb(IntPtr pbSrc, int width, int height, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12PackedToMono16bit(IntPtr pbSrc, int width, int height, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12ToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono12pToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono14lsbToMono16msb(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono14ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono14ToMono8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono16PackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMono8ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertMonoPackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGB10pToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGB10ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGB12PackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGB12ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGB8ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGBpToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertRGBToBGR(IntPtr pbSrc, Byte[] pbDst, int width, int height, uint dataBitCount);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV411PackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV411ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV422_UYVYToBGR8(IntPtr pbSrc, uint nWidth, uint nHeight, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV422_YUYVToBGR8(IntPtr pbSrc, uint nWidth, uint nHeight, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV422PackedToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV422ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV422ToBGR8Interlaced(IntPtr pbSrc, int cbSrc, Byte[] pbDst, bool bOdd, int width, bool blend, bool _signed);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYUV444ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYCbCr8ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYCbCr8_CbYCrToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
        [DllImport("VwImageProcess.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConvertYCbCr411_8ToBGR8(IntPtr pbSrc, int cbSrc, Byte[] pbDst);
    }
}
