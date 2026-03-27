using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

//namespace Vieworks_Image_Capture.NET_Class_Library_Test // VWFX
namespace Vieworks
{
    using VWUSB_HANDLE = System.IntPtr;
    using HINTERFACE = System.IntPtr;
    using HCAMERA = System.IntPtr;

    public enum PIXEL_RES					// Pixel Resolution
    {
        PIXEL_RES_MONO_1BIT,
        PIXEL_RES_MONO_2BIT,
        PIXEL_RES_MONO_4BIT,
        PIXEL_RES_MONO_8BIT,
        PIXEL_RES_MONO_10BIT,
        PIXEL_RES_MONO_12BIT,
        PIXEL_RES_MONO_14BIT,
        PIXEL_RES_MONO_16BIT
    };

    public enum MODE
    {
        MODE_SYNC,
        MODE_ASYNC
    };
    public enum EVENT
    {
        EVENT_CAP_START,			// Capture Start Event, Snap Unit
        EVENT_CAP_END				// Capture End Event, Snap Unit
    };
    public enum BUFFER
    {
        BUFFER_INTERNAL,
        BUFFER_EXTERNAL
    };

    /*
        [StructLayout(LayoutKind.Sequential)]
        public class CallbackFnParam			// Parameter for callback function
        {
            public IntPtr pParent;
            public IntPtr pCamera;
            public CallbackFnParam()
            {
                pParent = IntPtr.Zero;
                pCamera = IntPtr.Zero;
            }
        }
    */


    [StructLayout(LayoutKind.Sequential)]
    public class CallbackFnParam			// Parameter for callback function
    {
        public IntPtr pObjectInfo;
        public IntPtr pImageInfo;
        public CallbackFnParam()
        {
            pObjectInfo = IntPtr.Zero;
            pImageInfo = IntPtr.Zero;
        }
    }


    public enum GET_CUSTOM_COMMAND
    {
        GET_CUSTOM_COMMAND_VALUE = 0xF0,	// Value
        GET_CUSTOM_COMMAND_NUM,				// Entry Num
        GET_CUSTOM_COMMAND_MIN,				// Minimum
        GET_CUSTOM_COMMAND_MAX,				// Maximum
        GET_CUSTOM_COMMAND_INC,				// Increment
        GET_CUSTOM_COMMAND_INDEX
    };

    public enum RESULT
    {
        RESULT_SUCCESS,											// Success.

        RESULT_ERROR,											// Unspecified runtime error.
        RESULT_ERROR_OPENED_ALREADY,							// The module handle to open already opened.
        RESULT_ERROR_INVALID_HANDLE,							// Given handle does not support the operation; e.g. function call on wrong handle or NULL pointer.
        RESULT_ERROR_TL_HANDLE,									// Internal error. GenTL Transport handle is invalid.
        RESULT_ERROR_TLOPEN,									// Internal error. GenTL TLOpen function returned error.
        RESULT_ERROR_IF_HANDLE,									// Internal error. GenTL Interface handle is invalid.
        RESULT_ERROR_INITIALIZATION,							// Module was not initialized
        RESULT_ERROR_INVALID_PARAMETER,							// One of the parameter given was not valid or out of range and none of the error codes about fits.
        RESULT_ERROR_DISCOVERY,									// There was an error on the discovery.
        RESULT_ERROR_NO_CAMERAS,								// There are no cameras.
        RESULT_ERROR_CAMERA_NAME_DOES_NOT_EXIST,				// The camera name of which you are trying to open does not exist.
        RESULT_ERROR_ABORTED_ALREADY,							// The Abort command was already completed.
        RESULT_ERROR_ACCESS_DENIED,								// Access to the camera is denied.
        RESULT_ERROR_RESOURCE_IN_USE,							// The handle or resource has already been used.

        RESULT_ERROR_CANNOT_FIND_INTERFACE,						// Failed to find the interface.

        RESULT_ERROR_XML_UNKNOWN_ARGUMENT,						// The argument value is out of range or cannot be recognized.
        RESULT_ERROR_XML_NODE_ACCESS_FAILED,					// The Node map is abnormal or AccessMode for the node is NI/NA. 
        RESULT_ERROR_XML_NOT_EXIST_NODE,						// The node does not exist.
        RESULT_ERROR_XML_ENTERED_NODE_DOESNT_HAVE_ANY_VALUE,	// The node doesn't have any value like COMMAND, CATEGORY.
        RESULT_ERROR_XML_UNSUPPORTED_COMMAND,					// Unknown command.

        RESULT_ERROR_INVALID_WIDTH,								// Width is not multiples of 4.
        RESULT_ERROR_INVALID_ADDRESS,							// Unknown address.

        RESULT_ERROR_VWINTERFACE_NO_NIC,						// There is no interface card.
        RESULT_ERROR_VWINTERFACE_GETINTERFACENAME,				// Failed to retrieve a interface name.
        RESULT_ERROR_VWINTERFACE_OPENINTERFACE,					// OpenInterface function returned an error.
        RESULT_ERROR_VWINTERFACE_CLOSEINTERFACE,				// CloseInterface function returned an error.
        RESULT_ERROR_VWINTERFACE_GETNUMDEVICES,					// GetNumDevices function returned an error.
        RESULT_ERROR_VWINTERFACE_CANNOT_FIND_DEVICE,			// Failed to find a device.

        RESULT_ERROR_VWCAMERA_INTERFACE_HANDLE,					// The interface handle is invalid.
        RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER,					// The camera index is over the maximum number of cameras.
        RESULT_ERROR_VWCAMERA_GETXML,							// There is a problem with the XML that was retrieved from the camera.
        RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE,					// Width must divide by 4.
        RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE,					// Width must divide by 2.
        RESULT_ERROR_VWCAMERA_READ_ONLY,						// The node is read only.
        RESULT_ERROR_VWCAMERA_EVENTCONTROL_DOESNOT_INIT,		// Event control function did not initialize.
        RESULT_ERROR_VWCAMERA_GRAB_TIMEOUT,						// Time-out is occurred in grab routine.
        RESULT_ERROR_VWCAMERA_CALLBACK_NOT_NULL,				// Callback function pointer is not null.


        RESULT_ERROR_DEVCREATEDATASTREAM,						// Internal error. GenTL DevCreateDataStream function returned error.

        RESULT_ERROR_DATASTREAM_MTU,							// Internal error. The MTU of the NIC is too small to get a image.

        RESULT_ERROR_TLGETNUMINTERFACES,						// Internal error. GenTL TLGetNumInterfaces function returned error.
        RESULT_ERROR_TLOPENINTERFACE,							// Internal error. GenTL TLOpenInterface function returns error.
        RESULT_ERROR_TLCLOSEINTERFACE,							// Internal error. GenTL TLCloseInterface function returns error.
        RESULT_ERROR_TLGETINTERFACENAME,						// Internal error. GenTL TLGetInterfaceName function returned error.
        RESULT_ERROR_TLGETNUMDEVICES,							// Internal error. GenTL TLGetNumDevices function returns error.
        RESULT_ERROR_TLGETDEVICENAME,							// Internal error. GenTL TLGetDeviceName function returns error.
        RESULT_ERROR_TLOPENDEVICE,								// Internal error. GenTL TLOpenDevice function returns error.

        RESULT_ERROR_INSUFFICIENT_RESOURCES,					// Insufficient system resources. Unable to allocate memory as many as defined the number of buffers.
        RESULT_ERROR_MEMORY_ALLOCATION,							// Unable to allocate memory.

        RESULT_ERROR_FILE_STREAM_OPEN_FAILURE,					// Failed to open File stream.
        RESULT_ERROR_FILE_STREAM_READ_FAILURE,					// Failed to read File stream.
        RESULT_ERROR_FILE_STREAM_WRITE_FAILURE,					// Failed to write File stream.
        RESULT_ERROR_FILE_STREAM_CLOSE_FAILURE,					// Failed to close File stream.
        RESULT_ERROR_FILE_STREAM_NOT_CORRECT_FILE_LENGTH,		// File length is incorrect.

        RESULT_ERROR_EXCEPTION,									// An exception is occurred.

        RESULT_LAST												// The count of error items. Don't use it.
    };


    public enum PIXEL_FORMAT
    {
        PIXEL_FORMAT_MONO8					= 0x01080001,
		PIXEL_FORMAT_MONO8_SIGNED			= 0x01080002,
		PIXEL_FORMAT_MONO10					= 0x01100003,
        PIXEL_FORMAT_MONO10_P               = 0x010A0046,
		PIXEL_FORMAT_MONO10_PACKED			= 0x010C0004,
		PIXEL_FORMAT_MONO12					= 0x01100005,
        PIXEL_FORMAT_MONO12_P               = 0x010C0047,
		PIXEL_FORMAT_MONO12_PACKED			= 0x010C0006,
		PIXEL_FORMAT_MONO14					= 0x01100025,
		PIXEL_FORMAT_MONO16					= 0x01100007,
		PIXEL_FORMAT_BAYGR8					= 0x01080008,
		PIXEL_FORMAT_BAYRG8					= 0x01080009,
		PIXEL_FORMAT_BAYGB8					= 0x0108000A,
		PIXEL_FORMAT_BAYBG8					= 0x0108000B,
		PIXEL_FORMAT_BAYGR10 				= 0x0110000C,
		PIXEL_FORMAT_BAYRG10 				= 0x0110000D,
		PIXEL_FORMAT_BAYGB10 				= 0x0110000E,
		PIXEL_FORMAT_BAYBG10 				= 0x0110000F,
		PIXEL_FORMAT_BAYGR10_PACKED			= 0x010C0026,
		PIXEL_FORMAT_BAYGR12_PACKED			= 0x010C002A,
		PIXEL_FORMAT_BAYGR12 				= 0x01100010,
		PIXEL_FORMAT_BAYRG12 				= 0x01100011,
		PIXEL_FORMAT_BAYGB12 				= 0x01100012,
		PIXEL_FORMAT_BAYBG12 				= 0x01100013,
		PIXEL_FORMAT_BAYRG10_PACKED			= 0x010C0027,
		PIXEL_FORMAT_BAYRG12_PACKED			= 0x010C002B,
		PIXEL_FORMAT_RGB8					= 0x02180014,
		PIXEL_FORMAT_BGR8					= 0x02180015,
		PIXEL_FORMAT_RGB10					= 0x02300018,
		PIXEL_FORMAT_BGR10					= 0x02300019,
		PIXEL_FORMAT_RGB12					= 0x0230001A,
		PIXEL_FORMAT_BGR12					= 0x0230001B,
		PIXEL_FORMAT_YUV422_UYVY			= 0x0210001F,
		PIXEL_FORMAT_YUV422_YUYV			= 0x02100032,
        PIXEL_FORMAT_YUV422_10_PACKED       = unchecked((int)0x80180001),
        PIXEL_FORMAT_YUV422_12_PACKED       = unchecked((int)0x80180002),
		PIXEL_FORMAT_YUV411					= 0x020C001E,
        PIXEL_FORMAT_YUV411_10_PACKED       = unchecked((int)0x80120004),
        PIXEL_FORMAT_YUV411_12_PACKED       = unchecked((int)0x80120005),
		PIXEL_FORMAT_BGR10V1_PACKED			= 0x0220001C,
		PIXEL_FORMAT_BGR10V2_PACKED			= 0x0220001D,
		PIXEL_FORMAT_RGB12_PACKED			= 0x0230001A,
		PIXEL_FORMAT_BGR12_PACKED			= 0x0230001B,
		PIXEL_FORMAT_YUV444					= 0x02180020,
		PIXEL_FORMAT_PAL_INTERLACED			= 0x02100001,
		PIXEL_FORMAT_NTSC_INTERLACED		= 0x02100002,
        PIXEL_FORMAT_YCBCR8                 = 0x0218005B,
        PIXEL_FORMAT_YCBCR8_CBYCR           = 0x0218003A,
        PIXEL_FORMAT_YCBCR411_8             = 0x020C005A
    };

    public enum STREAM_INFO
    {
        STREAM_INFO_NUM_OF_FRAMES_LOST,
        STREAM_INFO_NUM_PACKETS_MISSING

    };

    public struct OBJECT_INFO
    {
        public IntPtr pUserPointer;
        public IntPtr pVwCamera;
    };


    public unsafe struct IMAGE_INFO
    {
        public RESULT callbackResult;
        public int bufferIndex;
        public PIXEL_FORMAT pixelFormat;
        public int width;
        public int height;
        public System.UInt64 unTimeStamp;
        public int ImageStatus;
        public byte btReserved;
        public IntPtr pImage;
    };
    public struct INTERFACE_INFO_STRUCT
    {
        public bool error;
        public RESULT errorCause;
        public int index;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public char[] name;
    };

    public struct CAMERA_INFO_STRUCT
    {
        public bool error;
        public RESULT errorResult;
        public int index;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string vendor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string model;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ip;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string mac;

    };

    public enum PROPERTY_TYPE
    {
        UNKNOWN = 0,
        ATTR_UINT = 1,
        ATTR_FLOAT = 2,
        ATTR_ENUM = 3,
        ATTR_BOOLEAN = 4,
        ATTR_STRING = 5,
        ATTR_COMMAND = 6,
        ATTR_CATEGORY = 7
    }

    public enum PROPERTY_ACCESS_MODE
    {
        NOT_IMPLEMENT = 0,
        NOT_AVAILABLE = 1,
        WRITE_ONLY = 2,
        READ_ONLY = 3,
        READ_WRITE = 4
    }

    public enum PROPERTY_VISIBILITY
    {
        BEGINNER = 0,
        EXPERT = 1,
        GURU = 2,
        INVISIBLE = 3,
        UNDEFINE = 4
    }

    public struct PROPERTY
    {
        PROPERTY_TYPE ePropType;
        PROPERTY_ACCESS_MODE eAccessMode;
        PROPERTY_VISIBILITY eVisibility;
        uint unPollingTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public char[] caDisplay;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public char[] caDescription;
    };

    public class VwUSB   // USB
    {
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT OpenVwUSB(ref VWUSB_HANDLE hVwUSB);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CloseVwUSB(ref VWUSB_HANDLE hVwUSB);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwGetNumInterfaces(VWUSB_HANDLE hVwUSB, IntPtr pNumInterfaces);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwGetNumCameras(VWUSB_HANDLE hVwUSB, ref int aPNumCamera);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwOpenCameraByIndex(VWUSB_HANDLE hVwUSB, int nCameraIndex, ref HCAMERA phCamera, int nNumBuffer, int nWidth, int nHeight,
                                                         int nPacketSize, IntPtr pUserPointer, IntPtr pImageCallbackFn, IntPtr pDisconnectCallbackFn);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ImageCallbackFn(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwOpenCameraByName(VWUSB_HANDLE hVwUSB, IntPtr pCameraName, ref HCAMERA phCamera, int nNumBuffer, int nWidth,
                                                        int nHeight, int nPacketSize, IntPtr pUserPointer, IntPtr pImageCallbackFn, IntPtr pDisconnectCallbackFn);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwOpenInterfaceByIndex(VWUSB_HANDLE hVwUSB, int aNIndex, ref HINTERFACE phInterface);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwOpenInterfaceByName(VWUSB_HANDLE hVwUSB, IntPtr pInterfaceName, ref HINTERFACE phInterface);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwDiscovery(VWUSB_HANDLE hVwUSB);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwDiscoveryInterfaceInfo(VWUSB_HANDLE hVwUSB, int nIndex, ref INTERFACE_INFO_STRUCT pInterfaceInfoStruct);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT VwDiscoveryCameraInfo(VWUSB_HANDLE hVwUSB, int nIndex, ref CAMERA_INFO_STRUCT pCameraInfoStruct);

        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceOpenCameraByIndex(HINTERFACE hInstance, IntPtr pCallbackParent, int nDevIndex, IntPtr phCamera, int nNumBuffer,
                                                                int nWidth, int nHeight, int nPacketSize, IntPtr pImageCallbackFn, IntPtr pDisconnectCallbackFn);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceOpenCameraByName(HINTERFACE hInterface, IntPtr pParent, IntPtr pName, IntPtr phCamera, int nNumBuffer,
                                                               int nWidth, int nHeight, int nPacketSize, IntPtr pImageCallbackFn, IntPtr pDisconnectCallbackFn);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceGetNumCameras(HINTERFACE hInterface, IntPtr aPNumDevices);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceGetCameraName(HINTERFACE hInterface, int aNDevIndex, IntPtr aName, IntPtr aPNameSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceCloseInterface(HINTERFACE hInterface);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT InterfaceGetVwUSBHandle(HINTERFACE hInterface, IntPtr phVwUSBHandle);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraClose(HCAMERA hCamera);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGrab(HCAMERA hCamera);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraAcquisitionStop(HCAMERA hCamera);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSnap(HCAMERA hCamera, int aNFrame);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraAbort(HCAMERA hCamera);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetGrabCondition(HCAMERA hCamera, ref bool bIsGrabbing);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetWidth(HCAMERA hCamera, ref int aPWidth);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetHeight(HCAMERA hCamera, ref int aPHeight);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetWidth(HCAMERA hCamera, int aNWidth);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetHeight(HCAMERA hCamera, int aNHeight);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetPixelFormat(HCAMERA hCamera, PIXEL_FORMAT pixelFormat);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetPixelFormat(HCAMERA hCamera, ref PIXEL_FORMAT pPixelFormat);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetAcquisitionTimeOut(HCAMERA hCamera, int nTimeOut);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetAcquisitionTimeOut(HCAMERA hCamera, ref int pnTimeOut);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetDeviceModelName(HCAMERA hCamera, int uIndex, Byte[] pInfo, IntPtr pInfoSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetDeviceVersion(HCAMERA hCamera, int uIndex, Byte[] pInfo, IntPtr pInfoSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetDeviceVendorName(HCAMERA hCamera, int uIndex, Byte[] pInfo, IntPtr pInfoSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetDeviceID(HCAMERA hCamera, int uIndex, Byte[] pInfo, IntPtr pInfoSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraChangeBufferFormat(HCAMERA hCamera, int nBufferNum, int nWidth, int nHeight, PIXEL_FORMAT pixelFormat);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetBufferInfo(HCAMERA hCamera, IntPtr nBufferNum, IntPtr nWidth, IntPtr nHeight, IntPtr pixelFormat);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetInterfaceHandle(HCAMERA hCamera, IntPtr phInterface);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetStreamInfo(HCAMERA hCamera, STREAM_INFO streamInfo, IntPtr nInfo);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetCustomCommand(HCAMERA hCamera, Byte[] pCommand, Byte[] pArg);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetCustomCommand(HCAMERA hCamera, Byte[] pCommand, Byte[] pArg, IntPtr pArgSize, int nCmdType);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraSetUARTCustomCommand(HCAMERA hCamera, Byte[] pCommand);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetUARTCustomCommand(HCAMERA hCamera, Byte[] pCommand, Byte[] pArg, ref int nArgSize);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetPropertyCount(HCAMERA hCamera, IntPtr pCount);
        [DllImport("VwGigE.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetEnumPropertyItems(HCAMERA hCamera, Byte[] pPropertyName,  Byte[] pArg, IntPtr pArgSize );
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetPropertyInfo(HCAMERA hCamera, Byte[] pCommand, ref PROPERTY ptPropInfo);
        [DllImport("VwUSB.NET.V7.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern RESULT CameraGetPropertyInfoUsingIndex(HCAMERA hCamera, int nIndex, ref PROPERTY ptPropInfo);


    }
}
