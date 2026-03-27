using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Vieworks;

using VWCL_HANDLE = System.IntPtr;
using HCAMERA = System.IntPtr;
using HDEVICE = System.IntPtr;

namespace VwCL.Demo.DeviceUpdate.Console.CS
{
    class Program
    {
        static void Main(string[] args)
        {
            // 0. Initialize
            System.Console.WriteLine("Step 0. Initialize....");
            VWCL_HANDLE vwCL = IntPtr.Zero;
            Vieworks.RESULT ret = Vieworks.VwCL.OpenVwCL(ref vwCL);

            if (Vieworks.RESULT.RESULT_SUCCESS != ret)
            {
                // Error handling
                System.Console.WriteLine("Failed(err:{0}))", ret);
                return;
            }
            System.Console.WriteLine("OK\n");

            // 1. Open Device
            System.Console.WriteLine("Step 1. Open Device.....");

            HCAMERA hCamera = IntPtr.Zero;
            OBJECT_INFO hObjectInfo = new OBJECT_INFO();
            GCHandle gchobjectInfo = GCHandle.Alloc(hObjectInfo);

            IntPtr pObjectInfo = Marshal.AllocHGlobal(Marshal.SizeOf(hObjectInfo));

            int nDeviceIndex = 0;
            int nImageBufferNumber = 20;

            ret = Vieworks.VwCL.VwOpenCameraByIndex(vwCL,
                                                    nDeviceIndex,   // Device Index
                                                    ref hCamera,    // Camera handle
                                                    nImageBufferNumber, // The number of buffer
                                                    0,              // Auto
                                                    0,              // Auto
                                                    0,              // Auto
                                                    pObjectInfo,    // User Pointer
                                                    IntPtr.Zero,    // Skip in this sample
                                                    IntPtr.Zero);   // Skip in this sample
            if (ret != Vieworks.RESULT.RESULT_SUCCESS)
            {
                switch (ret)
                {
                    default:
                        {
                            System.Console.WriteLine("ERROR : Default error code returned");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_DEVCREATEDATASTREAM:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_NO_CAMERAS:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_NO_CAMERAS was returned");
                            System.Console.WriteLine("CHECK : Camera connection");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned");
                            System.Console.WriteLine("CHECK : Zero-based camera index");
                        }
                        break;
                }

                Marshal.FreeHGlobal(pObjectInfo);
                if (IntPtr.Zero != hCamera)
                    Vieworks.VwCL.CameraClose(hCamera);
                if (IntPtr.Zero != vwCL)
                    Vieworks.VwCL.CloseVwCL(ref vwCL);
                return;
            }

            HDEVICE hDev = IntPtr.Zero;
            Vieworks.VwCL.CameraGetDeviceHandle(hCamera, ref hDev);

        MAIN_MENU:

            while (true)
            {
                System.Console.WriteLine("\n\tSelect menu");
                System.Console.WriteLine("\t1.Firmware Download( PC to Camera )");
                System.Console.WriteLine("\t2.Defect Download( PC to Camera )");
                System.Console.WriteLine("\t3.Defect Upload( Camera to PC )");
                System.Console.WriteLine("\t4.LUT Download( PC to Camera )");
                System.Console.WriteLine("\t5.LUT Upload( Camera to PC )");
                System.Console.WriteLine("\t6.FFC Download( PC to Camera )");
                System.Console.WriteLine("\t7.FFC Upload( Camera to PC )");

                System.Console.WriteLine("\t8.Quit");
                System.Console.WriteLine(">");


                ConsoleKeyInfo tKeyInfo = System.Console.ReadKey(false);

                if ((tKeyInfo.Key >= ConsoleKey.NumPad1 &&
                     tKeyInfo.Key <= ConsoleKey.NumPad7) ||
                    (tKeyInfo.Key >= ConsoleKey.D1 &&
                    tKeyInfo.Key <= ConsoleKey.D7))
                {
                    Dispatch(tKeyInfo.Key, hDev);
                }
                else if (tKeyInfo.Key == ConsoleKey.D8 ||
                          tKeyInfo.Key == ConsoleKey.NumPad8)
                {
                    break;
                }
                else
                {
                    // Unknown key
                }
            }

            ConsoleKeyInfo tKeyExit;
            do
            {
                System.Console.WriteLine("** Goto Select menu?");
                System.Console.WriteLine("** Y");
                System.Console.WriteLine("** N");
                System.Console.WriteLine(">");
                tKeyExit = System.Console.ReadKey(false);
            } while (tKeyExit.Key != ConsoleKey.Y &&
                    tKeyExit.Key != ConsoleKey.N);

            if (tKeyExit.Key == ConsoleKey.Y)
            {
                goto MAIN_MENU;
            }

            // 3. Terminate
            System.Console.WriteLine("Step 3. Terminate.....");

            Marshal.FreeHGlobal(pObjectInfo);
            if (IntPtr.Zero != hCamera)
                Vieworks.VwCL.CameraClose(hCamera);
            if (IntPtr.Zero != vwCL)
                Vieworks.VwCL.CloseVwCL(ref vwCL);



            System.Console.ReadLine();
        }

        static public void Dispatch(System.ConsoleKey Key, HCAMERA hCamera)
        {
            if (IntPtr.Zero == hCamera)
                return;

            System.Console.WriteLine("\nInput a name of file.");
            System.Console.WriteLine(">");

            string strFileName = System.Console.ReadLine();
            IntPtr pFileName = Marshal.StringToBSTR(strFileName);

            Vieworks.VwDeviceMaintenance.ProgressCallbackFn CallbackFunc = UpdateCallbackFunc;
            Vieworks.VwDeviceMaintenance.ProgressCallbackFn pCallback = new Vieworks.VwDeviceMaintenance.ProgressCallbackFn(CallbackFunc);
            GCHandle gchCallback = GCHandle.Alloc(pCallback);
            IntPtr ptrCallback = Marshal.GetFunctionPointerForDelegate(pCallback);


            ERESULT_ERROR eRet = ERESULT_ERROR.ERESULT_SUCCESS;
            switch (Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPDATE_PKG, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_DEFECT, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_DEFECT, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_LUT, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_LUT, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_FFC, hCamera, pFileName, ptrCallback);
                    }
                    break;
                case ConsoleKey.D7:
                case ConsoleKey.NumPad7:
                    {
                        eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_FFC, hCamera, pFileName, ptrCallback);
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Unrecognized command");
                    }
                    break;
            }

            if (ERESULT_ERROR.ERESULT_SUCCESS== eRet)
            {
                System.Console.WriteLine("The update has been completed successfully");
            }
            else
            {
                System.Console.WriteLine("The update has been failed. Err({0})", eRet);
            }
        }


        static public unsafe void UpdateCallbackFunc(IntPtr pUserPoint, int nProgress)
        {
            System.Console.WriteLine("Update Progress({0})", nProgress);
        }
    }
}