using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Vieworks
{
    using DEV_HANDLE = System.IntPtr;

    public enum UPDATE_TARGET
    {
        UPDATE_FPGA,
        UPDATE_MCU,
        UPDATE_XML,
        UPDATE_SCRIPT,
        UPDATE_PKG,
        UPLOAD_FFC,
        DOWNLOAD_FFC,
        UPLOAD_DEFECT,
        DOWNLOAD_DEFECT,
        UPLOAD_LUT,
        DOWNLOAD_LUT
    };

    public enum ERESULT_ERROR
    {
        ERESULT_SUCCESS,
        ERESULT_UNSUPPORTED_UPDATE_TYPE,
        ERESULT_INVALID_HANDLE,
        ERESULT_DEVICE_IN_USE,
        ERESULT_UNKNOWN_ERROR
    };
    public class VwDeviceMaintenance   // Camera Link
    {
        public delegate void ProgressCallbackFn(IntPtr pUserPoint, int nProgress);
        [DllImport("VwDeviceMaintenance.NET.DLL", CallingConvention=CallingConvention.Cdecl)]
        public static extern ERESULT_ERROR VwUpdateDevice(UPDATE_TARGET eTarget, DEV_HANDLE hDev, IntPtr strFilePath, IntPtr pProgressCallback, int nTimeout = 5/*min*/ * 60/*sec*/ * 1000/*ms*/);
    }
}

