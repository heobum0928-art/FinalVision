Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Runtime.InteropServices
Imports DEV_HANDLE = System.IntPtr

Namespace Vieworks
    Public Enum UPDATE_TARGET
        UPDATE_FPGA
        UPDATE_MCU
        UPDATE_XML
        UPDATE_SCRIPT
        UPDATE_PKG
        UPLOAD_FFC
        DOWNLOAD_FFC
        UPLOAD_DEFECT
        DOWNLOAD_DEFECT
        UPLOAD_LUT
        DOWNLOAD_LUT
    End Enum

    Public Enum ERESULT_ERROR
        ERESULT_SUCCESS
        ERESULT_UNSUPPORTED_UPDATE_TYPE
        ERESULT_INVALID_HANDLE
        ERESULT_DEVICE_IN_USE
        ERESULT_UNKNOWN_ERROR
    End Enum

    Public Class VwDeviceMaintenance
        Public Delegate Sub ProgressCallbackFn(ByVal pUserPoint As IntPtr, ByVal nProgress As Integer)
        <DllImport("VwDeviceMaintenance.NET.DLL", CallingConvention:=CallingConvention.Cdecl)>
        Public Shared Function VwUpdateDevice(ByVal eTarget As UPDATE_TARGET, ByVal hDev As DEV_HANDLE, ByVal strFilePath As IntPtr, ByVal pProgressCallback As IntPtr, Optional ByVal nTimeout As Integer = 5 * 60 * 1000) As ERESULT_ERROR
        End Function
    End Class
End Namespace
