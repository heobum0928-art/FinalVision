using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Vieworks;

using VWUSB_HANDLE = System.IntPtr;
using HINTERFACE = System.IntPtr;
using HCAMERA = System.IntPtr;




namespace VwUSB.Demo.MultiCam.Window.Advance.CS
{
    public partial class CVwUSB_Demo_MultiCam_Window_Advance_CS : Form
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")] 
        private static extern bool QueryPerformanceCounter(out long perfcount); 
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")] 
        private static extern bool QueryPerformanceFrequency(out long freq); 

        VWUSB_HANDLE m_pvwUSB = IntPtr.Zero;
        HCAMERA[] m_lstCamera = new HCAMERA[4];
        HCAMERA m_pCamera;
        int m_imagebuffernumber;
        double[] m_curFPS = new double[4];
        List<long>[] m_imageTimeStamps = new List<long>[4];
        long[] m_liLastDisplayTime = new long[4];
        long m_liFreq;
        long m_nMinInterFrameTime = 0;

        IntPtr[] m_pobjectInfo = new IntPtr[4];
        GCHandle[] gchCallback = new GCHandle[4];
        GCHandle gchobjectInfo;
        GCHandle gchUSB;

        int m_nCurrentDeviceIndex = -1;
        int m_nOldDeviceIndex = -1;
        CDeviceState[] m_deviceState = new CDeviceState[4];
        
       
        public CVwUSB_Demo_MultiCam_Window_Advance_CS()
        {
            InitializeComponent();

            m_imagebuffernumber = 2;
            edtNumBuffers.Text = String.Format( "{0}", m_imagebuffernumber );
            edtFrame.Text = "1";

            Vieworks.RESULT result = Vieworks.VwUSB.OpenVwUSB( ref m_pvwUSB);
            gchUSB = GCHandle.Alloc(m_pvwUSB);

            if (result != Vieworks.RESULT.RESULT_SUCCESS)
            {
                MessageBox.Show("Cannot open the camera. Please restart this program.");

                btnCloseCamera.Enabled = false;
                btnOpenCamera.Enabled = false;
                btnAbort.Enabled = false;
                btnGrab.Enabled = false;
                btnSnap.Enabled = false;
            }
            else
            {
                btnCloseCamera.Enabled = false;
                btnOpenCamera.Enabled = false;
                btnAbort.Enabled = false;
                edtNumBuffers.Enabled = false;

                btnGrab.Enabled = false;
                btnSnap.Enabled = false;
                cbxPixelFormat.Enabled = false;
                cbxTestPattern.Enabled = false;
                edtWidth.Enabled = false;
                edtHeight.Enabled = false;
                edtFrame.Enabled = false;
            }

            QueryPerformanceFrequency( out m_liFreq);
            m_nMinInterFrameTime = (long)(m_liFreq / 30);

            for (int i = 0; i < 4; i++)
            {
                m_deviceState[i] = new CDeviceState();
                m_imageTimeStamps[i] = new List<long>();
            }
       
        }


        public Vieworks.RESULT GetCustomCommand(HCAMERA hCamera, string sFeatureName, ref int value, Vieworks.GET_CUSTOM_COMMAND eCmdType = Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            unsafe
            {
                Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
                int arrSize = 1024;
                IntPtr pSize = new IntPtr(&arrSize);
                Byte[] btArgument = new Byte[arrSize];
                int nCmdType = (int)eCmdType;
                eRet = Vieworks.VwUSB.CameraGetCustomCommand(hCamera, btFeatureName, btArgument, pSize, nCmdType);
                if (eRet == Vieworks.RESULT.RESULT_SUCCESS)
                {
                    string str = Encoding.Default.GetString(btArgument).TrimEnd('\0');
                    value = Convert.ToInt32(str);
                }
            }

            return eRet;
        }

        public Vieworks.RESULT GetCustomCommandString(HCAMERA hCamera, string sFeatureName, ref string sValue, Vieworks.GET_CUSTOM_COMMAND eCmdType = Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            unsafe
            {
                Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
                int arrSize = 1024;
                IntPtr pSize = new IntPtr(&arrSize);
                Byte[] btArgument = new Byte[arrSize];
                int nCmdType = (int)eCmdType;
                eRet = Vieworks.VwUSB.CameraGetCustomCommand(hCamera, btFeatureName, btArgument, pSize, nCmdType);
                if (eRet == Vieworks.RESULT.RESULT_SUCCESS)
                {
                    sValue = Encoding.Default.GetString(btArgument).TrimEnd('\0');
                }
            }

            return eRet;
        }

        public Vieworks.RESULT SetCustomCommand(HCAMERA hCamera, string sFeatureName,  string sArgument)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
            Byte[] btArgument = Encoding.UTF8.GetBytes(sArgument);
            eRet = Vieworks.VwUSB.CameraSetCustomCommand(hCamera, btFeatureName, btArgument);

            return eRet;
        }


        public Bitmap ImageConvert(IntPtr pImage, PIXEL_FORMAT pixelFormat, CVwUSB_Demo_MultiCam_Window_Advance_CS dlg)
        {
            
            int nWidth = 0;
            GetCustomCommand(m_lstCamera[0], "Width", ref nWidth);
         
            int nHeight = 0;
            GetCustomCommand(m_lstCamera[0], "Height", ref nHeight);

            int biBitCount = 0;
            string strPixelFormat ="";
            switch (pixelFormat)
            {
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8:
                    {
                        biBitCount = 8;
                        strPixelFormat = "Pixel Format : Mono 8\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 10\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 12\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO14:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 14\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO16:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 16\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10_P:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 10p\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12_P:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : Mono 12p\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYGR8\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYRG8\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB8:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : RGB8\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR8:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BGR8\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB10:
                    {
                        biBitCount = 48;
                        strPixelFormat = "Pixel Format : RGB10\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10:
                    {
                        biBitCount = 48;
                        strPixelFormat = "Pixel Format : BGR10\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12:
                    {
                        biBitCount = 48;
                        strPixelFormat = "Pixel Format : RGB12\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12:
                    {
                        biBitCount = 48;
                        strPixelFormat = "Pixel Format : BGR12\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYGR10\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYGR12\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYRG10\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYRG12\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYGR10 Packed\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYGR12 Packed\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYRG10 Packed\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : BAYRG12 Packed\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : YUV422 UYVY\n";
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Pixel Format : YUV422 YUYV\n";
                    }
                    break;
                default:
                    {
                        biBitCount = 24;
                        strPixelFormat = "Unknown pixel format\n";
                    }
                    break;
            }

            if (IntPtr.Zero == pImage)
            {
                System.Diagnostics.Trace.WriteLine("Image pointer is NULL !!");

                return null;
            }

            int nSize = nWidth * nHeight * (biBitCount / 8);
            byte[] array = new byte[nWidth * nHeight * 3];
            byte[] arrayUnpacked = new byte[nWidth * nHeight * 3];

            PixelFormat DrawPixelFormat = PixelFormat.Format8bppIndexed;


            switch (pixelFormat)
            {
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8:
                    {
                        array = new byte[nSize];
                        Marshal.Copy(pImage, array, 0, nSize);
                        DrawPixelFormat = PixelFormat.Format8bppIndexed;
                    }
                    break;
                //////////////////////////////////////////////////////////////////////////
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10:
                    {
                        Vieworks.VwImageProcess.ConvertMono10ToBGR8(
                            pImage,
                            nWidth * nHeight * 2,
                            array);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12:
                    {
                        Vieworks.VwImageProcess.ConvertMono12ToBGR8(
                                pImage,
                                nWidth * nHeight * 2,
                                array);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO14:
                    {
                        Vieworks.VwImageProcess.ConvertMono14ToBGR8(
                                pImage,
                                nWidth * nHeight * 2,
                                array);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                //////////////////////////////////////////////////////////////////////////
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10_P:
                    {
                        Vieworks.VwImageProcess.ConvertMono10pToMono8(
                            pImage,
                            (int)(nWidth * nHeight * 1.25),
                            arrayUnpacked);

                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertMono8ToBGR8(unmanagedUnpacked,
                                                                        nWidth * nHeight,
                                                                        array
                                                                        );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                        Marshal.FreeHGlobal(unmanagedUnpacked);
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12_P:
                    {
                        Vieworks.VwImageProcess.ConvertMono12pToMono8(
                                pImage,
                                (int)(nWidth * nHeight * 1.5),
                                arrayUnpacked);

                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertMono8ToBGR8(unmanagedUnpacked,
                                                                        nWidth * nHeight,
                                                                        array
                                                                        );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                        Marshal.FreeHGlobal(unmanagedUnpacked);
                    }
                    break;

                //////////////////////////////////////////////////////////////////////////
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10_PACKED:
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12_PACKED:
                    {
                        Vieworks.VwImageProcess.ConvertMonoPackedToBGR8(
                                                                pImage,
                                                                checked((int)(1.5 * nWidth * nHeight)),
                                                                array);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;

                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO16:
                    {
                        Vieworks.VwImageProcess.ConvertMono16PackedToBGR8(
                            pImage,
                            checked((int)(2 * nWidth * nHeight)),
                            array);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8:
                    {
                        //Convert BAYER -> RGB;
                        Vieworks.VwImageProcess.ConvertBAYGR8ToBGR8(
                            pImage,
                            array,
                            nWidth,
                            nHeight
                            );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8:
                    {
                        //Convert BAYER -> RGB;
                        Vieworks.VwImageProcess.ConvertBAYRG8ToBGR8(
                            pImage,
                            array,
                            nWidth,
                            nHeight
                            );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10:
                    {
                        Vieworks.VwImageProcess.ConvertBAYGR10ToBGR8(
                                                            pImage,
                                                            array,
                                                            nWidth,
                                                            nHeight
                                                            );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10:
                    {
                        Vieworks.VwImageProcess.ConvertBAYRG10ToBGR8(
                                                            pImage,
                                                            array,
                                                            nWidth,
                                                            nHeight);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12:
                    {
                        Vieworks.VwImageProcess.ConvertBAYGR12ToBGR8(pImage,
                                                            array,
                                                            nWidth,
                                                            nHeight);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12:
                    {
                        Vieworks.VwImageProcess.ConvertBAYRG12ToBGR8(pImage,
                                                            array,
                                                            nWidth,
                                                            nHeight);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB8:
                    {
                        Vieworks.VwImageProcess.ConvertRGB8ToBGR8(pImage,
                                                            nSize,
                                                            array);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR8:
                    {
                        array = new byte[nSize];
                        Marshal.Copy(pImage, array, 0, nSize);
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB10:
                    {
                        Vieworks.VwImageProcess.ConvertRGB10ToBGR8(pImage,
                                                            nSize,
                                                            array);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10:
                    {
                        Vieworks.VwImageProcess.ConvertBGR10ToBGR8(pImage,
                                                            nSize,
                                                            array);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12:
                    {
                        Vieworks.VwImageProcess.ConvertRGB12ToBGR8(pImage,
                                                            nSize,
                                                            array);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12:
                    {
                        Vieworks.VwImageProcess.ConvertBGR12ToBGR8(pImage,
                                                            nSize,
                                                            array);

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED:
                    {
                        Vieworks.VwImageProcess.ConvertMono10PackedToMono16bit(
                                                                       pImage,
                                                                       nWidth,
                                                                       nHeight,
                                                                       arrayUnpacked
                                                                       );
                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertBAYGR10ToBGR8(
                                                                        unmanagedUnpacked,
                                                                        array,
                                                                        nWidth,
                                                                        nHeight
                                                                        );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                        Marshal.FreeHGlobal(unmanagedUnpacked);

                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED:
                    {
                        Vieworks.VwImageProcess.ConvertMono12PackedToMono16bit(
                                                                            pImage,
                                                                            nWidth,
                                                                            nHeight,
                                                                            arrayUnpacked
                                                                            );
                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertBAYGR12ToBGR8(
                                                                            unmanagedUnpacked,
                                                                            array,
                                                                            nWidth,
                                                                            nHeight
                                                                            );

                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                        Marshal.FreeHGlobal(unmanagedUnpacked);
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED:
                    {
                        Vieworks.VwImageProcess.ConvertMono10PackedToMono16bit(
                                                                       pImage,
                                                                       nWidth,
                                                                       nHeight,
                                                                       arrayUnpacked
                                                                       );
                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertBAYRG10ToBGR8(
                                                                        unmanagedUnpacked,
                                                                        array,
                                                                        nWidth,
                                                                        nHeight
                                                                        );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;

                        Marshal.FreeHGlobal(unmanagedUnpacked);

                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED:
                    {
                        Vieworks.VwImageProcess.ConvertMono12PackedToMono16bit(
                                                                            pImage,
                                                                            nWidth,
                                                                            nHeight,
                                                                            arrayUnpacked
                                                                            );
                        IntPtr unmanagedUnpacked = Marshal.AllocHGlobal(arrayUnpacked.Length);
                        Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length);

                        Vieworks.VwImageProcess.ConvertBAYRG12ToBGR8(
                                                                            unmanagedUnpacked,
                                                                            array,
                                                                            nWidth,
                                                                            nHeight
                                                                            );

                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                        Marshal.FreeHGlobal(unmanagedUnpacked);
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV:
                    {
                        Vieworks.VwImageProcess.ConvertYUV422_YUYVToBGR8(
                                                            pImage,
                                                            (uint)nWidth,
                                                            (uint)nHeight,
                                                            array
                                                            );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY:
                    {
                        Vieworks.VwImageProcess.ConvertYUV422_UYVYToBGR8(
                                                            pImage,
                                                            (uint)nWidth,
                                                            (uint)nHeight,
                                                            array
                                                            );
                        DrawPixelFormat = PixelFormat.Format24bppRgb;
                    }
                    break;
                default:
                    {
                        return null;
                    }
            }

            
            Bitmap bitmap = dlg.CreateBitmap(nWidth, nHeight, array, DrawPixelFormat);

            if (bitmap == null)
            {
                return null;
            }

            return bitmap;
        }

        public unsafe void GetImageEvent1(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            CVwUSB_Demo_MultiCam_Window_Advance_CS dlg = this;
            
            // FPS
	        long liTime;
	        QueryPerformanceCounter( out liTime );

            int i = 0;
            while (dlg.m_imageTimeStamps[0].Count > 30)
            {
                dlg.m_imageTimeStamps[0].RemoveAt(i);
                i++;
            }

            dlg.m_imageTimeStamps[0].Add(liTime);

            long[] arr = dlg.m_imageTimeStamps[0].ToArray();
	        long diff = liTime-arr[0];
            
	        if( diff > 0 )
                dlg.m_curFPS[0] = (double) (dlg.m_liFreq * (long)(dlg.m_imageTimeStamps[0].Count - 1)) / diff;
	        else
		        dlg.m_curFPS[0] = 0.0;

            if (liTime - dlg.m_liLastDisplayTime[0] > dlg.m_nMinInterFrameTime)
            {

                PIXEL_FORMAT pixelFormat = pImageInfo.pixelFormat;

                Bitmap bitmap = ImageConvert(pImageInfo.pImage, pixelFormat, dlg);

                Rectangle rtSize = new Rectangle(0, 0, 772 / 2, 672 /2);

                dlg.m_liLastDisplayTime[0] = liTime;
                Graphics g = dlg.ImageBox.CreateGraphics();
                g.DrawImage(bitmap, rtSize);
            }
 
       }

        public unsafe void GetImageEvent2(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            CVwUSB_Demo_MultiCam_Window_Advance_CS dlg = this;

            // FPS
            long liTime;
            QueryPerformanceCounter(out liTime);

            int i = 0;
            while (dlg.m_imageTimeStamps[1].Count > 30)
            {
                dlg.m_imageTimeStamps[1].RemoveAt(i);
                i++;
            }

            dlg.m_imageTimeStamps[1].Add(liTime);

            long[] arr = dlg.m_imageTimeStamps[1].ToArray();
            long diff = liTime - arr[0];

            if (diff > 0)
                dlg.m_curFPS[1] = (dlg.m_liFreq * (long)(dlg.m_imageTimeStamps[1].Count - 1)) / diff;
            else
                dlg.m_curFPS[1] = 0;

            if (liTime - dlg.m_liLastDisplayTime[1] > dlg.m_nMinInterFrameTime)
            {

                PIXEL_FORMAT pixelFormat = pImageInfo.pixelFormat;

                Bitmap bitmap = ImageConvert(pImageInfo.pImage, pixelFormat, dlg);

                Rectangle rtSize = new Rectangle(772 / 2, 0, 772, 672/2);

                Graphics g = dlg.ImageBox.CreateGraphics();
                g.DrawImage(bitmap, rtSize);
            }
        }

        public unsafe void GetImageEvent3(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            CVwUSB_Demo_MultiCam_Window_Advance_CS dlg = this;

            // FPS
            long liTime;
            QueryPerformanceCounter(out liTime);

            int i = 0;
            while (dlg.m_imageTimeStamps[2].Count > 30)
            {
                dlg.m_imageTimeStamps[2].RemoveAt(i);
                i++;
            }

            dlg.m_imageTimeStamps[2].Add(liTime);

            long[] arr = dlg.m_imageTimeStamps[2].ToArray();
            long diff = liTime - arr[0];

            if (diff > 0)
                dlg.m_curFPS[2] = (dlg.m_liFreq * (long)(dlg.m_imageTimeStamps[2].Count - 1)) / diff;
            else
                dlg.m_curFPS[2] = 0;

            if (liTime - dlg.m_liLastDisplayTime[2] > dlg.m_nMinInterFrameTime)
            {

                PIXEL_FORMAT pixelFormat = pImageInfo.pixelFormat;

                Bitmap bitmap = ImageConvert(pImageInfo.pImage, pixelFormat, dlg);

                Rectangle rtSize = new Rectangle(0, 672/2, 772/2, 672);

                Graphics g = dlg.ImageBox.CreateGraphics();
                g.DrawImage(bitmap, rtSize);
            }
        }

        public unsafe void GetImageEvent4(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            CVwUSB_Demo_MultiCam_Window_Advance_CS dlg = this;

            // FPS
            long liTime;
            QueryPerformanceCounter(out liTime);

            int i = 0;
            while (dlg.m_imageTimeStamps[3].Count > 30)
            {
                dlg.m_imageTimeStamps[3].RemoveAt(i);
                i++;
            }

            dlg.m_imageTimeStamps[3].Add(liTime);

            long[] arr = dlg.m_imageTimeStamps[3].ToArray();
            long diff = liTime - arr[0];

            if (diff > 0)
                dlg.m_curFPS[3] = (dlg.m_liFreq * (long)(dlg.m_imageTimeStamps[3].Count - 1)) / diff;
            else
                dlg.m_curFPS[3] = 0;

            if (liTime - dlg.m_liLastDisplayTime[3] > dlg.m_nMinInterFrameTime)
            {

                PIXEL_FORMAT pixelFormat = pImageInfo.pixelFormat;

                Bitmap bitmap = ImageConvert(pImageInfo.pImage, pixelFormat, dlg);

                Rectangle rtSize = new Rectangle(772/2, 672 / 2, 772, 672);

                Graphics g = dlg.ImageBox.CreateGraphics();
                g.DrawImage(bitmap, rtSize);
            }
        }

        private Bitmap CreateBitmap(int nWidth, int nHeight, Byte[] RawData, PixelFormat pixelFormat )
        {
            try
            {
                //Bitmap Canvas = new Bitmap(nWidth, nHeight, PixelFormat.Format8bppIndexed);
                Bitmap Canvas = new Bitmap(nWidth, nHeight, pixelFormat);

                BitmapData CanvasData = Canvas.LockBits(new Rectangle(0, 0, nWidth, nHeight),
                                                         ImageLockMode.WriteOnly, pixelFormat);

                IntPtr ptr = CanvasData.Scan0;
                Marshal.Copy(RawData, 0, ptr, RawData.Length);

                Canvas.UnlockBits(CanvasData);

                if (PixelFormat.Format8bppIndexed == pixelFormat)
                {
                    SetGrayscalePalette(Canvas);
                }

                return Canvas;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private static void SetGrayscalePalette(Bitmap Image)
        {
            ColorPalette GrayscalePalette = Image.Palette;

            for (int i = 0; i < 256; i++)
            {
                GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            Image.Palette = GrayscalePalette;

        }

        private unsafe void btnOpenCamera_Click(object sender, EventArgs e)
        {
            HCAMERA pCamera = IntPtr.Zero;

            OBJECT_INFO m_objectInfo = new OBJECT_INFO();

            m_imagebuffernumber = Int32.Parse(edtNumBuffers.Text);

            gchobjectInfo = GCHandle.Alloc(m_objectInfo);
            // allocation
           
            m_pobjectInfo[m_nCurrentDeviceIndex] = Marshal.AllocHGlobal(Marshal.SizeOf(m_objectInfo));
               
            // struct -> pointer
            Marshal.StructureToPtr(m_objectInfo, m_pobjectInfo[m_nCurrentDeviceIndex], true);
            GCHandle.Alloc(m_pobjectInfo);
            
            RESULT	result = Vieworks.RESULT.RESULT_ERROR;

            Vieworks.VwUSB.ImageCallbackFn CallbackFunc;
            
            switch (m_nCurrentDeviceIndex)
            {
                case 0:
                    {
                        CallbackFunc = GetImageEvent1;
                    }
                    break;
                case 1:
                    {
                        CallbackFunc = GetImageEvent2;
                    }
                    break;
                case 2:
                    {
                        CallbackFunc = GetImageEvent3;
                    }
                    break;
                case 3:
                    {
                        CallbackFunc = GetImageEvent4;
                    }
                    break;
                default:
                    return;

            }

            Vieworks.VwUSB.ImageCallbackFn pCallback = new Vieworks.VwUSB.ImageCallbackFn(CallbackFunc);
            gchCallback[m_nCurrentDeviceIndex] = GCHandle.Alloc(pCallback);
            IntPtr ptrCallback = Marshal.GetFunctionPointerForDelegate(pCallback);

            result = Vieworks.VwUSB.VwOpenCameraByIndex(m_pvwUSB, m_nCurrentDeviceIndex, ref pCamera, m_imagebuffernumber, 0, 0,
                                                         0, m_pobjectInfo[m_nCurrentDeviceIndex], ptrCallback, IntPtr.Zero);
            if (result != Vieworks.RESULT.RESULT_SUCCESS)
            {
                switch (result)
                {
                    default:
                        {
                            MessageBox.Show("ERROR : Default error code returned");
                        }
			        break;
                case Vieworks.RESULT.RESULT_ERROR_DEVCREATEDATASTREAM:
			        {
				        MessageBox.Show("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned");
			        }
			        break;
                case Vieworks.RESULT.RESULT_ERROR_NO_CAMERAS:
			        {
				        MessageBox.Show("ERROR : RESULT_ERROR_NO_CAMERAS was returned");
				        MessageBox.Show("CHECK : NIC properties");
			        }
			        break;
                case Vieworks.RESULT.RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER:
			        {
				        MessageBox.Show("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned");
				        MessageBox.Show("CHECK : Zero-based camera index");
			        }
			        break;
                case Vieworks.RESULT.RESULT_ERROR_DATASTREAM_MTU:
			        {
				        MessageBox.Show("ERROR : RESULT_ERROR_STREAM_MTU was returned");
				        MessageBox.Show("CHECK : Check NIC MTU");
			        }
			        break;
		        }
		        return ;
	        }
        	
	        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ((OBJECT_INFO*)m_pobjectInfo[m_nCurrentDeviceIndex])->pVwCamera = pCamera;
            m_lstCamera[m_nCurrentDeviceIndex] = pCamera;
            m_pCamera = pCamera;

            // Get device information
            string strVendorName = "";
            string strModelName = "";
            string strVersion = "";
            string strID = "";

            GetDeviceInfo( m_nCurrentDeviceIndex, ref strVendorName, ref strModelName, ref strVersion, ref strID);
            
	        //Get image width,height 
	        int  nWidth = 0;
	        int  nHeight = 0;
            PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

            GetCustomCommand(pCamera, "Width", ref nWidth);
            GetCustomCommand(pCamera, "Height", ref nHeight);

            List<PIXEL_FORMAT> plstPixelFormat = new List<PIXEL_FORMAT>();
            int nLineupNum = 0;
            GetCustomCommand(pCamera, "PixelFormat", ref nLineupNum, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);

            for (int i = 0; i < nLineupNum; i++)
            {
                string strPixelFormat = "";
                GetCustomCommandString(pCamera, "PixelFormat", ref strPixelFormat, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                for (int j = 0; j < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; j++)
                {
                    if (PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[j] == strPixelFormat)
                    {
                        pixelFormat = PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[j];
                        break;
                    }
                }
                plstPixelFormat.Add(pixelFormat);
            }
            
            cbxTestPattern.Items.Clear();
            nLineupNum = 0;
            GetCustomCommand(pCamera, "TestPattern", ref nLineupNum, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);

            for (int i = 0; i < nLineupNum; i++)
            {
                string strTestPattern = "";
                GetCustomCommandString(pCamera, "TestPattern", ref strTestPattern, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                cbxTestPattern.Items.Add(strTestPattern);
            }
	        //
	        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	        cbxPixelFormat.Items.Clear();

            foreach (PIXEL_FORMAT s in plstPixelFormat)
            {
                string strTemp = "";
                strTemp = GetPixelFormatFromEnum( s );
                cbxPixelFormat.Items.Add(strTemp);
            }

	        Vieworks.VwUSB.CameraGetPixelFormat( pCamera, ref pixelFormat );
	  
	        // Set resolution info.
	        int tempwidth = 0;
	        int tempheight = 0;
            GetCustomCommand(pCamera, "Width", ref tempwidth);
            GetCustomCommand(pCamera, "Height", ref tempheight);

            string strCurTestPattern = "";

            GetCustomCommandString(pCamera, "TestPattern", ref strCurTestPattern);
            SetUIResolutionInfo(tempwidth, tempheight, pixelFormat, strCurTestPattern );

	      
            txtVendorName.Text = strVendorName;
            txtModelName.Text = strModelName;
            txtDeviceVersion.Text = strVersion;
            txtDeviceID.Text = strID;
            
            btnCloseCamera.Enabled = true;
            btnOpenCamera.Enabled = false;
            btnAbort.Enabled = true;
            edtNumBuffers.Enabled = false;

            btnGrab.Enabled = true;
            btnSnap.Enabled = true;
            cbxPixelFormat.Enabled = true;
            cbxTestPattern.Enabled = true;
            edtWidth.Enabled = true;
            edtHeight.Enabled = true;
            edtFrame.Enabled = true;
            btnDiscovery.Enabled = false;
            //listBoxDeviceList.Enabled = false;
        }

        public unsafe void GetDeviceInfo( int nIndex, ref string strVenderName, ref string strModelName, ref string strDeviceVersion, ref string strDeviceID )
        {
	        if ( m_pCamera == IntPtr.Zero )
	        {
		        return;
	        }

	        const int STR_SIZE	=	256;
            Byte[] btVenderName = new Byte[STR_SIZE];
            IntPtr pcbVendor = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btModelName = new Byte[STR_SIZE];
            IntPtr pcbModel = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btVersion = new Byte[STR_SIZE];
            IntPtr pcbVersion = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btID = new Byte[STR_SIZE];
            IntPtr pcbID = Marshal.AllocHGlobal(sizeof(int));

            
            int[] nSize = new int[1];
            if (Vieworks.VwUSB.CameraGetDeviceVendorName(m_pCamera, nIndex, btVenderName, pcbVendor) == Vieworks.RESULT.RESULT_SUCCESS)
	        {
                Marshal.Copy(pcbVendor, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btVenderName[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }

                acHashedData = Encoding.Default.GetChars(btVenderName, 0, nValidCount);

                string temp = new string(acHashedData);
                strVenderName = temp;
	        }

            if (Vieworks.VwUSB.CameraGetDeviceModelName(m_pCamera, nIndex, btModelName, pcbModel) == Vieworks.RESULT.RESULT_SUCCESS)
	        {
                Marshal.Copy(pcbModel, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btModelName[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }

                acHashedData = Encoding.Default.GetChars(btModelName, 0, nValidCount);

                string temp = new string(acHashedData);
                strModelName = temp;
	        }

            if (Vieworks.VwUSB.CameraGetDeviceVersion(m_pCamera, nIndex, btVersion, pcbVersion) == Vieworks.RESULT.RESULT_SUCCESS)
	        {
                Marshal.Copy(pcbVersion, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btVersion[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }
                acHashedData = Encoding.Default.GetChars(btVersion, 0, nValidCount);

                string temp = new string(acHashedData);
                strDeviceVersion = temp;
	        }

            if (Vieworks.VwUSB.CameraGetDeviceID(m_pCamera, nIndex, btID, pcbID) == Vieworks.RESULT.RESULT_SUCCESS)
	        {
                Marshal.Copy(pcbID, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btID[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }
                acHashedData = Encoding.Default.GetChars(btID, 0, nValidCount);

                string temp = new string(acHashedData);
                strDeviceID = temp;
	        }

            Marshal.FreeHGlobal(pcbVendor);
            Marshal.FreeHGlobal(pcbModel);
            Marshal.FreeHGlobal(pcbVersion);
            Marshal.FreeHGlobal(pcbID);
        }
      
        public unsafe string GetPixelFormatFromEnum(Vieworks.PIXEL_FORMAT pixelFormat)
        {
            int i = 0;
            for (i = 0; i < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; i++)
            {
                if (PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[i] == pixelFormat)
                {
                    break;
                }
            }
            return PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[i];
        }

        public void SetUIResolutionInfo(int tempwidth, int tempheight, Vieworks.PIXEL_FORMAT pixelFormat, string strPixelSize)
        {
            string strWidth = String.Format("{0}", tempwidth);
            edtWidth.Text = strWidth;

            string strHeight = String.Format("{0}", tempheight);
            edtHeight.Text = strHeight;

            string strPixelFormat = "Pixel Format : ";
            
            for ( int i = 0; i < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; i ++ )
            {
                if ( PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[i] == pixelFormat )
                {
                    strPixelFormat += PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[i];
                    cbxPixelFormat.Text = PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[i];
                    break;
                }
            }
            cbxTestPattern.Text = strPixelSize;
        }

        
        public int GetPixelTypeIndex( string strType )
        {
            return cbxPixelFormat.Items.IndexOf(strType);
        }
        
        private void btnSnap_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero == m_pCamera)
            {
                return;
            }

            bool bGrabbing = false;
            Vieworks.VwUSB.CameraGetGrabCondition(m_pCamera, ref bGrabbing);

            if (bGrabbing)
            {
                MessageBox.Show("Now grabbing... Please 'Abort' first.");
                return;
            }

            // Set Width, Height
            int nInputWidth = Int32.Parse(edtWidth.Text);
            if (false == SetWidthCamera(nInputWidth))
            {
                // Rollback
                int nWidth = 0;
                GetCustomCommand(m_pCamera, "Width", ref nWidth);
                edtWidth.Text = String.Format("{0}", nWidth);
            }

            // Set Width, Height
            int nInputHeight = Int32.Parse(edtHeight.Text);
            if (false == SetHeightCamera(nInputHeight))
            {
                int nHeight = 0;
                GetCustomCommand(m_pCamera, "Height", ref nHeight);
                edtHeight.Text = String.Format("{0}", nHeight);
            }

            int nFrame = Int32.Parse( edtFrame.Text );

            //Exception
            if (nFrame < 1)
            {
                MessageBox.Show("Must be greater than 0.");
                nFrame = 1;
            }
            else if (nFrame > 255)
            {
                MessageBox.Show("Must be less than 256.");
                nFrame = 255;
            }

            edtFrame.Text = nFrame.ToString();

            if (Vieworks.VwUSB.CameraSnap(m_pCamera, nFrame) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                // Success
            }
            else
            {
                // Fail
                MessageBox.Show("Failed : Snap");
                return;
            }

            // Update resolution info.
            int nCurWidth = 0;
            int nCurHeight = 0;
            
            Vieworks.PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

            GetCustomCommand(m_pCamera, "Width", ref nCurWidth);
            GetCustomCommand(m_pCamera, "Height", ref nCurHeight);
            Vieworks.VwUSB.CameraGetPixelFormat(m_pCamera, ref pixelFormat);

            string strTestPattern = "";
            GetCustomCommandString(m_pCamera, "TestPattern", ref strTestPattern);

            SetUIResolutionInfo(nCurWidth, nCurHeight, pixelFormat, strTestPattern );
        }

                
        public bool SetWidthCamera( int nWidth )
        {
	        int nCurrWidth = 0;

            if (Vieworks.RESULT.RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Width", ref nCurrWidth))
	        {
                return false;
	        }

	        if ( nWidth != nCurrWidth )
	        {
                RESULT ret = SetCustomCommand(m_pCamera, "Width", nWidth.ToString());

                if (Vieworks.RESULT.RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE == ret)
		        {
                    MessageBox.Show("Error : Width must be a multiple of 4!");

                    return false;
		        }
	        }

            return true;
        }

        public bool SetHeightCamera( int nHeight )
        {
	        int nCurrHeight = 0;
            if (Vieworks.RESULT.RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Height", ref nCurrHeight))
	        {
		        return false;
	        }

	        if ( nHeight != nCurrHeight )
	        {
                RESULT ret = SetCustomCommand(m_pCamera, "Height", nHeight.ToString());

                if (Vieworks.RESULT.RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE == ret)
                {
                    MessageBox.Show("Error : Height must be a multiple of 2!");

                    return false;
                }
	        }

            return true;
        }

        private void btnCloseCamera_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero != m_pCamera)
            {
                if (Vieworks.VwUSB.CameraClose(m_pCamera) == Vieworks.RESULT.RESULT_SUCCESS)
                {
                    // Success
                }
                else
                {
                    // Fail
                }
            }
            m_pCamera = IntPtr.Zero;
            m_lstCamera[m_nCurrentDeviceIndex] = IntPtr.Zero;

            if (gchCallback[m_nCurrentDeviceIndex].IsAllocated)
            {
                gchCallback[m_nCurrentDeviceIndex].Free();
            }
            if (gchobjectInfo.IsAllocated)
            {
                gchobjectInfo.Free();
            }

            btnCloseCamera.Enabled = false;
            btnOpenCamera.Enabled = true;
            btnAbort.Enabled = false;
            edtNumBuffers.Enabled = true;

            btnGrab.Enabled = false;
            btnSnap.Enabled = false;
            cbxPixelFormat.Enabled = false;
            cbxTestPattern.Enabled = false;
            edtWidth.Enabled = false;
            edtHeight.Enabled = false;
            edtFrame.Enabled = false;

            btnDiscovery.Enabled = true;
            for (int i = 0; i < 4; i++)
            {
                if ( IntPtr.Zero != m_lstCamera[ i ] )
                {
                    btnDiscovery.Enabled = false;
                }
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero == m_pCamera)
            {
                System.Diagnostics.Trace.WriteLine("Camera == zero");
                return;
            }

            Vieworks.RESULT ret = Vieworks.VwUSB.CameraAbort(m_pCamera);

            if ( ret == Vieworks.RESULT.RESULT_SUCCESS ||
                 ret == Vieworks.RESULT.RESULT_ERROR_ABORTED_ALREADY)
            {
                System.Diagnostics.Debug.WriteLine("CameraAbort Successed.");
                m_imageTimeStamps[m_nCurrentDeviceIndex].Clear();
            }

      
            v_timer.Enabled = false;

            btnCloseCamera.Enabled = true;
            btnGrab.Enabled = true;
            btnSnap.Enabled = true;
            edtFrame.Enabled = true;
            cbxPixelFormat.Enabled = true;
            cbxTestPattern.Enabled = true;
            edtWidth.Enabled = true;
            edtHeight.Enabled = true;
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero == m_pCamera)
            {
                return;
            }

            bool bGrabbing = false;
            Vieworks.VwUSB.CameraGetGrabCondition(m_pCamera, ref bGrabbing);

            if (bGrabbing)
            {
                MessageBox.Show("Now grabbing... Please 'Abort' first.");
                return;
            }

            int nWidth = 0;
            GetCustomCommand(m_pCamera, "Width", ref nWidth);
            int nHeight = 0;
            GetCustomCommand(m_pCamera, "Height", ref nHeight);

            // Set Width, Height
            int nInputWidth = Int32.Parse(edtWidth.Text);
            
            if (false == SetWidthCamera(nInputWidth))
            {
                // Rollback
                int nCurWidth = 0;
                GetCustomCommand(m_pCamera, "Width", ref nCurWidth);
                string strWidth = String.Format("{0}", nCurWidth);
                edtWidth.Text = strWidth;
            }

            // Set Width, Height
            int nInputHeight = Int32.Parse(edtHeight.Text);
                        
            if (false == SetHeightCamera(nInputHeight))
            {
                // Rollback
                int nCurHeight = 0;
                GetCustomCommand(m_pCamera, "Height", ref nCurHeight);
                string strHeight = String.Format("{0}", nCurHeight);
                edtHeight.Text = strHeight;
            }

            GetCustomCommand(m_pCamera, "Width", ref nInputWidth);
            GetCustomCommand(m_pCamera, "Height", ref nInputHeight);

            SetCustomCommand(m_pCamera, "HorizontalStart", "0");
            SetCustomCommand(m_pCamera, "HorizontalEnd", (nInputWidth - 1).ToString());
            SetCustomCommand(m_pCamera, "VerticalStart", "0");
            SetCustomCommand(m_pCamera, "VerticalEnd", (nInputHeight - 1).ToString());

            PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

            Vieworks.VwUSB.CameraGetPixelFormat(m_pCamera, ref pixelFormat);

            if (nInputWidth != nWidth ||
                 nInputHeight != nHeight)
            {
                if (Vieworks.RESULT.RESULT_SUCCESS != Vieworks.VwUSB.CameraChangeBufferFormat(m_pCamera, m_imagebuffernumber, nInputWidth, nInputHeight, pixelFormat))
                {
                    MessageBox.Show("Can't change the camera buffer.");

                    return;
                }
            }



            if (Vieworks.VwUSB.CameraGrab(m_pCamera) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                System.Diagnostics.Trace.WriteLine("GameraGrab");
            }
            else
            {
                return;
            }

            v_timer.Enabled = true;

            // Disable buttons
            btnCloseCamera.Enabled = false;
            edtFrame.Enabled = false;
            btnGrab.Enabled = false;
            btnSnap.Enabled = false;
            
            cbxPixelFormat.Enabled = false;
            edtWidth.Enabled = false;
            edtHeight.Enabled = false;


            GetCustomCommand(m_pCamera, "Width", ref nWidth);
            GetCustomCommand(m_pCamera, "Height", ref nHeight);
            Vieworks.VwUSB.CameraGetPixelFormat(m_pCamera, ref pixelFormat);

            string strTestPattern = "";
            GetCustomCommandString(m_pCamera, "TestPattern", ref strTestPattern);
            SetUIResolutionInfo(nWidth, nHeight, pixelFormat, strTestPattern);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            bool bEmpty = true;
            for (int i = 0; i < 4; i++)
            {
                if (IntPtr.Zero != m_lstCamera[i])
                {
                    bEmpty = false;
                    break;
                }
            }

            if (false == bEmpty)
            {
                MessageBox.Show("First, Close device.");
                return;
            }
           
            for ( int i = 0; i < 4; i ++ )
            {
                if (m_pobjectInfo[i] != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(m_pobjectInfo[i]);
                }
            }

            Application.Exit();
        }

        private void edtNumBuffers_TextChanged(object sender, EventArgs e)
        {
            if ("-" == edtNumBuffers.Text ||
                "" == edtNumBuffers.Text)
            {
                return;
            }
            m_imagebuffernumber = Int32.Parse(edtNumBuffers.Text);
            
            if ( 0 >= m_imagebuffernumber )
            {
                MessageBox.Show("Must be grater than 0.");
                edtNumBuffers.Text = "1";
                return;
            }
        }

        private void cbxTestPattern_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IntPtr.Zero == m_pCamera)
            {
                return;
            }

            string strTestPattern = cbxTestPattern.SelectedItem.ToString();

            string strCurTestPattern = "";
            GetCustomCommandString(m_pCamera, "TestPattern", ref strCurTestPattern);
            if (strCurTestPattern == strTestPattern)
            {
                return;
            }

            RESULT ret = SetCustomCommand(m_pCamera, "TestPattern", strTestPattern);

            switch (ret)
            {
                case Vieworks.RESULT.RESULT_SUCCESS:
                    break;
                case Vieworks.RESULT.RESULT_ERROR_INVALID_PARAMETER:
                    MessageBox.Show("Invalid test patten.");

                    cbxTestPattern.SelectedItem = strTestPattern;
                    return;
                default:
                    MessageBox.Show("Can't change the test patten.");
                    cbxTestPattern.SelectedItem = strTestPattern;
                    return;
            }
        }

        private void PixelFormatSelChange(object sender, EventArgs e)
        {
        
            if (IntPtr.Zero == m_pCamera)
            {
                return;
            }

            string str = cbxPixelFormat.SelectedItem.ToString();
            PIXEL_FORMAT pixelFormatItem = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;
            for (int i = 0; i < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; i++)
            {
                if (PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[i] == str)
                {
                    pixelFormatItem = PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[i];
                    break;
                }
            }

            Vieworks.RESULT ret =Vieworks.VwUSB.CameraSetPixelFormat(m_pCamera, pixelFormatItem);

            switch (ret)
            {
                case Vieworks.RESULT.RESULT_SUCCESS:
                    break;
                case Vieworks.RESULT.RESULT_ERROR_INVALID_PARAMETER:
                    MessageBox.Show( "Invalid pixelformat.");
                    return;
                default:
                    MessageBox.Show("Can't change the pixelformat.");
                    return;
            }

            int nWidth = 0;
            int nHeight = 0;
            Vieworks.PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;
            GetCustomCommand(m_pCamera, "Width", ref nWidth);
            GetCustomCommand(m_pCamera, "Height", ref nHeight);
            Vieworks.VwUSB.CameraGetPixelFormat( m_pCamera, ref pixelFormat );

            if (Vieworks.RESULT.RESULT_SUCCESS != Vieworks.VwUSB.CameraChangeBufferFormat( m_pCamera, m_imagebuffernumber, nWidth, nHeight, pixelFormat))
            {
                MessageBox.Show( "Can't change the camera buffer." );
                return;
            }
        }

        private void v_timer_Tick(object sender, EventArgs e)
        {
            if ( m_curFPS[m_nCurrentDeviceIndex] > 0 )
            {
                string strFPS = String.Format("{0:F}fps", m_curFPS[m_nCurrentDeviceIndex]);
                txtFPS.Text = strFPS;
            }
        }

        private void btnDiscovery_Click(object sender, EventArgs e)
        {
            if (IntPtr.Zero == m_pvwUSB)
            {
                MessageBox.Show("m_pvwUSB has NULL pointer.");
                return;
            }

            RESULT ret = Vieworks.VwUSB.VwDiscovery(m_pvwUSB);

            if (Vieworks.RESULT.RESULT_SUCCESS != ret)
            {
                MessageBox.Show("Failed to discovery.");
            }
            listBoxDeviceList.Items.Clear();


            int nCameraNum = 0;
            ret = Vieworks.VwUSB.VwGetNumCameras( m_pvwUSB, ref nCameraNum );

            if (Vieworks.RESULT.RESULT_SUCCESS != ret)
            {
                MessageBox.Show("Failed to access camera.");
                return;
            }
            
            for (int i = 0; i < nCameraNum; i++)
            {
                Vieworks.CAMERA_INFO_STRUCT cameraInfoStruct = new Vieworks.CAMERA_INFO_STRUCT();

                Vieworks.VwUSB.VwDiscoveryCameraInfo(m_pvwUSB, i, ref cameraInfoStruct);
                listBoxDeviceList.Items.Add(cameraInfoStruct.name);
            }

        }

        private void btnSelectDevice_Click(object sender, EventArgs e)
        {
            if (listBoxDeviceList.SelectedItem == null)
            {
                MessageBox.Show("Please, select device.");
                return;
            }
            int m_nCurrentDeviceIndex = listBoxDeviceList.Items.IndexOf(listBoxDeviceList.SelectedItem);

            m_pCamera = m_lstCamera[m_nCurrentDeviceIndex];

            if (IntPtr.Zero != m_pCamera)
            {
                SaveDeviceState(m_nOldDeviceIndex);
                UpdateDeviceState(m_nCurrentDeviceIndex);
            }
            else
            {
                btnOpenCamera.Enabled = true;
            }
            m_nOldDeviceIndex = m_nCurrentDeviceIndex;
        }
                      
        private void  SaveDeviceState( int nDeviceNum )
        {
	        CDeviceState deviceState = new CDeviceState();

            deviceState.m_bOpen = btnOpenCamera.Enabled;
            deviceState.m_bClose = btnCloseCamera.Enabled;
            deviceState.m_bImageBuffer = edtNumBuffers.Enabled;
            deviceState.m_nBuffer = Int32.Parse(edtNumBuffers.Text);

            deviceState.m_strVendorName = txtVendorName.Text;
            deviceState.m_strModelName = txtModelName.Text;
            deviceState.m_strDeviceVersion = txtDeviceVersion.Text;
            deviceState.m_strDeviceID = txtDeviceID.Text;
            deviceState.m_nSnapBuffer = Int32.Parse( edtFrame.Text );
            deviceState.m_bSnapBuffer = edtFrame.Enabled;

            deviceState.m_bGrab = btnGrab.Enabled;
            deviceState.m_bSnap = btnSnap.Enabled;
            deviceState.m_bAbort = btnAbort.Enabled;

            deviceState.m_bPixelFormat = cbxPixelFormat.Enabled;
            deviceState.m_strPixelFormat = cbxPixelFormat.SelectedText;
            deviceState.m_bTestPattern = cbxTestPattern.Enabled;
            deviceState.m_strTestPattern = cbxTestPattern.SelectedText;

            deviceState.m_bWidth = edtWidth.Enabled;
            if (edtWidth.Text != "")
            {
                deviceState.m_nWidth = Int32.Parse(edtWidth.Text);
            }

            deviceState.m_bHeight = edtHeight.Enabled;
            if (edtHeight.Text != "")
            {
                deviceState.m_nHeight = Int32.Parse(edtHeight.Text);
            }

            m_deviceState[nDeviceNum] = deviceState;
        }

        private void UpdateDeviceState(int nDeviceNum)
        {
            CDeviceState deviceState = new CDeviceState();
            deviceState = m_deviceState[nDeviceNum];

            btnOpenCamera.Enabled = deviceState.m_bOpen;
            btnCloseCamera.Enabled = deviceState.m_bClose;
            edtNumBuffers.Enabled = deviceState.m_bImageBuffer;
            edtNumBuffers.Text = deviceState.m_nBuffer.ToString();

            txtVendorName.Text = deviceState.m_strVendorName;
            txtModelName.Text = deviceState.m_strModelName;
            txtDeviceVersion.Text = deviceState.m_strDeviceVersion;
            txtDeviceID.Text = deviceState.m_strDeviceID;
            edtFrame.Text = deviceState.m_nSnapBuffer.ToString();
            edtFrame.Enabled = deviceState.m_bSnapBuffer;

            btnGrab.Enabled = deviceState.m_bGrab;
            btnSnap.Enabled = deviceState.m_bSnap;
            btnAbort.Enabled = deviceState.m_bAbort;

            cbxPixelFormat.Enabled = deviceState.m_bPixelFormat;
            cbxPixelFormat.SelectedText = deviceState.m_strPixelFormat;
            cbxTestPattern.Enabled = deviceState.m_bTestPattern;
            cbxTestPattern.SelectedText = deviceState.m_strTestPattern;;

            edtWidth.Enabled = deviceState.m_bWidth;
            edtWidth.Text = deviceState.m_nWidth.ToString();

            edtHeight.Enabled = deviceState.m_bHeight;
            edtHeight.Text = deviceState.m_nHeight.ToString();

            // Update resolution info.
            int unWidth = 0;
            int unHeight = 0;
            string strTestPattern = "";
            Vieworks.PIXEL_FORMAT ePixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

            GetCustomCommand(m_pCamera, "Width", ref unWidth);
            GetCustomCommand(m_pCamera, "Height", ref unHeight);
            Vieworks.VwUSB.CameraGetPixelFormat(m_pCamera, ref ePixelFormat);

            GetCustomCommandString(m_pCamera, "TestPattern", ref strTestPattern);

            SetUIResolutionInfo(unWidth, unHeight, ePixelFormat, strTestPattern);


        }

        private void listBoxDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDeviceList.SelectedItem == null)
            {
                //MessageBox.Show("Please, select device.");
                return;
            }
            m_nCurrentDeviceIndex = listBoxDeviceList.Items.IndexOf(listBoxDeviceList.SelectedItem);

            if (m_nCurrentDeviceIndex >= 4)
            {
                MessageBox.Show("Can not use more than 5 cameras.");
            }
            m_pCamera = m_lstCamera[m_nCurrentDeviceIndex];

            if (m_nOldDeviceIndex == -1)
            {
                UpdateDeviceState(m_nCurrentDeviceIndex);
            }
            else
            {
                if (m_nOldDeviceIndex != m_nCurrentDeviceIndex)
                {
                    SaveDeviceState(m_nOldDeviceIndex);
                    UpdateDeviceState(m_nCurrentDeviceIndex);
                }
                else
                {
                }
            }

            m_nOldDeviceIndex = m_nCurrentDeviceIndex;

            PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

            List<PIXEL_FORMAT> plstPixelFormat = new List<PIXEL_FORMAT>();
            int nLineupNum = 0;
            GetCustomCommand(m_pCamera, "PixelFormat", ref nLineupNum, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);

            for (int i = 0; i < nLineupNum; i++)
            {
                string strPixelFormat = "";
                GetCustomCommandString(m_pCamera, "PixelFormat", ref strPixelFormat, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                for (int j = 0; j < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; j++)
                {
                    if (PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[j] == strPixelFormat)
                    {
                        pixelFormat = PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[j];
                        break;
                    }
                }
                plstPixelFormat.Add(pixelFormat);
            }

            string strTestPattern = "";
            nLineupNum = 0;
            cbxTestPattern.Items.Clear();
            GetCustomCommand(m_pCamera, "TestPattern", ref nLineupNum, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);

            for (int i = 0; i < nLineupNum; i++)
            {
                GetCustomCommandString(m_pCamera, "TestPattern", ref strTestPattern, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                cbxTestPattern.Items.Add(strTestPattern);
            }
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            cbxPixelFormat.Items.Clear();

            foreach (PIXEL_FORMAT s in plstPixelFormat)
            {
                string strTemp = "";
                strTemp = GetPixelFormatFromEnum(s);
                cbxPixelFormat.Items.Add(strTemp);
            }

        }

                
        private void UpdatePixelFormat()
        {
            if ( IntPtr.Zero == m_pCamera )
            {
	            return;
            }

             PIXEL_FORMAT pixelFormat = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8;

	        List<PIXEL_FORMAT> plstPixelFormat = new List<PIXEL_FORMAT>();
	        int nLineupNum = 0;
            GetCustomCommand(m_pCamera, "PixelFormat", ref nLineupNum, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);

            for (int i = 0; i < nLineupNum; i++)
            {
                string strPixelFormat = "";
                GetCustomCommandString(m_pCamera, "PixelFormat", ref strPixelFormat, Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                for (int j = 0; j < PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT; j++)
                {
                    if (PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT[j] == strPixelFormat)
                    {
                        pixelFormat = PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT[j];
                        break;
                    }
                }
                plstPixelFormat.Add(pixelFormat);
            }

            cbxPixelFormat.Items.Clear();

            foreach (PIXEL_FORMAT s in plstPixelFormat)
            {
                string strTemp = "";
                strTemp = GetPixelFormatFromEnum( s );
                cbxPixelFormat.Items.Add(strTemp);
            }
        }

        private void CVwUSB_Demo_MultiCam_Window_Advance_CS_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IntPtr.Zero != m_pCamera)
            {
                MessageBox.Show("First, Close device\n");

                e.Cancel = true;
                return;
            }

            if (IntPtr.Zero != m_pvwUSB)
            {
                Vieworks.VwUSB.CloseVwUSB(ref m_pvwUSB);
                m_pvwUSB = IntPtr.Zero;
            }
        }
       
    }

    public class CDeviceState
    {
        public CDeviceState()
        {
            m_bOpen = true;
            m_bClose = false;
            m_bImageBuffer = true;
            m_nBuffer = 2;
            m_bGrab = false;
            m_nSnapBuffer = 2;
            m_bSnapBuffer = false;
            m_bSnap = false;
            m_bAbort = false;
            m_bPixelFormat = false;
            m_bTestPattern = false;
            m_nWidth = 0;
            m_bWidth = false;
            m_nHeight = 0;
            m_bHeight = false;
        }
        public bool m_bOpen;
        public bool m_bClose;
        public bool m_bImageBuffer;
        public int m_nBuffer;
        public string m_strVendorName;
        public string m_strModelName;
        public string m_strDeviceVersion;
        public string m_strDeviceID;
        public bool m_bGrab;
        public int m_nSnapBuffer;
        public bool m_bSnapBuffer;
        public bool m_bSnap;
        public bool m_bAbort;
        public string m_strPixelFormat;
        public bool m_bPixelFormat;
        public string m_strTestPattern;
        public bool m_bTestPattern;
        public int m_nWidth;
        public bool m_bWidth;
        public int m_nHeight;
        public bool m_bHeight;

    }

    static public class PIXEL_FORMAT_ARRAY
    {
        public static int PIXEL_FORMAT_COUNT = 49;

        public static string[] STR_PIXEL_FORMAT = {
    "Mono8",
	"Mono8signed",
	"Mono10",
    "Mono10p",
	"Mono10Packed",
	"Mono12",
    "Mono12p",
	"Mono12Packed",
	"Mono14",
	"Mono16",
	"BayerGR8",
	"BayerRG8",
	"BayerGB8",
	"BayerBG8",
	"BayerGR10",
	"BayerRG10",
	"BayerGB10",
	"BayerBG10",
	"BayerGR10Packed",
	"BayerRG10Packed",
	"BayerGR12",
	"BayerRG12",
	"BayerGB12",
	"BayerBG12",
	"BayerRG12Packed",
	"BayerGR12Packed",
	"RGB8",
	"BGR8",
	"RGB10",
	"BGR10",
	"RGB12",
	"BGR12",
	"YUV422_8_UYVY",
	"YUV422_8",
	"YUV42210Packed",
	"YUV42212Packed",
	"YUV411_8_UYYVYY",
	"YUV41110Packed",
	"YUV41112Packed",
	"BGR10V1Packed",
	"BGR10V2Packed",
	"RGB12Packed",
	"BGR12Packed",
	"YUV444",
	"PALInterlaced",
	"NTSCInterlaced",
    "YCbCr8",
    "YCbCr8_CbYCr",
    "YCbCr411_8"
    };



        public static PIXEL_FORMAT[] ARR_PIXEL_FORMAT = {
            PIXEL_FORMAT.PIXEL_FORMAT_MONO8,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO8_SIGNED,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO10,
            PIXEL_FORMAT.PIXEL_FORMAT_MONO10_P,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO10_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO12,
            PIXEL_FORMAT.PIXEL_FORMAT_MONO12_P,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO14,
	        PIXEL_FORMAT.PIXEL_FORMAT_MONO16,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGB8,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYBG8,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGB10,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYBG10,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGB12,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYBG12,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_RGB8,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR8,
	        PIXEL_FORMAT.PIXEL_FORMAT_RGB10,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR10,
	        PIXEL_FORMAT.PIXEL_FORMAT_RGB12,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR12,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV422_10_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV422_12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV411,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV411_10_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV411_12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR10V1_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR10V2_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_RGB12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_BGR12_PACKED,
	        PIXEL_FORMAT.PIXEL_FORMAT_YUV444,
	        PIXEL_FORMAT.PIXEL_FORMAT_PAL_INTERLACED,
	        PIXEL_FORMAT.PIXEL_FORMAT_NTSC_INTERLACED,
            PIXEL_FORMAT.PIXEL_FORMAT_YCBCR8,
	        PIXEL_FORMAT.PIXEL_FORMAT_YCBCR8_CBYCR,
	        PIXEL_FORMAT.PIXEL_FORMAT_YCBCR411_8
        };
    }   
}   
