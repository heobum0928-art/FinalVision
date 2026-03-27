Imports System.Collections.Generic
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Vieworks
    Public Class VwImageProcess

        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYBG10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYBG12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYBG8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYBGToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB10ToBGR10(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB12ToBGR12(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGBToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR10ToBGR10(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR12ToBGR12(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGRToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG10ToBGR10(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG12ToBGR12(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRGToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGR10pToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGR10ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGR10V2PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGR12ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGRpToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGRToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGRToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10lsbToMono16msb(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10msbToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10PackedlsbToMono16msb(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10PackedToMono16bit(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10PackedToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10ToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10pToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12lsbToMono16msb(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12msbToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12PackedlsbToMono16msb(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12PackedToMono16bit(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12ToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12pToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono14lsbToMono16msb(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono14ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono14ToMono8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono16PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono8ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMonoPackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB10pToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB10ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB12PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB12ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB8ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGBpToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGBToBGR(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer, dataBitCount As UInteger)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV411PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV411ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422_UYVYToBGR8(pbSrc As IntPtr, nWidth As UInteger, nHeight As UInteger, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422_YUYVToBGR8(pbSrc As IntPtr, nWidth As UInteger, nHeight As UInteger, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422ToBGR8Interlaced(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte](), bOdd As Boolean, width As Integer, blend As Boolean, _signed As Boolean)
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV444ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYCbCr8ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYCbCr8_CbYCrToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwImageProcess.NET.V7.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYCbCr411_8ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
    End Class
End Namespace
