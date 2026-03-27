

'namespace Vieworks_Image_Capture.NET_Class_Library_Test // VWFX
Imports VWCL_HANDLE = System.IntPtr
Imports HINTERFACE = System.IntPtr
Imports HCAMERA = System.IntPtr
Imports HDEVICE = System.IntPtr
Imports System.Collections.Generic
Imports System.Text
Imports System.Runtime.InteropServices
Namespace Vieworks

    Public Enum PIXEL_RES
        ' Pixel Resolution
        PIXEL_RES_MONO_1BIT
        PIXEL_RES_MONO_2BIT
        PIXEL_RES_MONO_4BIT
        PIXEL_RES_MONO_8BIT
        PIXEL_RES_MONO_10BIT
        PIXEL_RES_MONO_12BIT
        PIXEL_RES_MONO_14BIT
        PIXEL_RES_MONO_16BIT
    End Enum

    Public Enum MODE
        MODE_SYNC
        MODE_ASYNC
    End Enum
    Public Enum [EVENT]
        EVENT_CAP_START
        ' Capture Start Event, Snap Unit
        EVENT_CAP_END
        ' Capture End Event, Snap Unit
    End Enum
    Public Enum BUFFER
        BUFFER_INTERNAL
        BUFFER_EXTERNAL
    End Enum

    <StructLayout(LayoutKind.Sequential)> _
    Public Class CallbackFnParam
        ' Parameter for callback function
        Public pObjectInfo As IntPtr
        Public pImageInfo As IntPtr
        Public Sub New()
            pObjectInfo = IntPtr.Zero
            pImageInfo = IntPtr.Zero
        End Sub
    End Class



    Public Enum RESULT
        RESULT_SUCCESS      'Success.

        RESULT_ERROR        'Unspecified runtime error.
        RESULT_ERROR_OPENED_ALREADY 'The module handle to open already opened.
        RESULT_ERROR_INVALID_HANDLE 'Given handle does not support the operation; e.g. function call on wrong handle or NULL pointer.
        RESULT_ERROR_TL_HANDLE      'Internal error. GenTL Transport handle is invalid.
        RESULT_ERROR_TLOPEN         'Internal error. GenTL TLOpen function returned error.
        RESULT_ERROR_IF_HANDLE      'Internal error. GenTL Interface handle is invalid.
        RESULT_ERROR_INITIALIZATION 'Module was not initialized
        RESULT_ERROR_INVALID_PARAMETER  'One of the parameter given was not valid or out of range and none of the error codes about fits.
        RESULT_ERROR_DISCOVERY          'There was an error on the discovery.
        RESULT_ERROR_NO_CAMERAS         'There are no cameras.
        RESULT_ERROR_CAMERA_NAME_DOES_NOT_EXIST     'The camera name of which you are trying to open does not exist.
        RESULT_ERROR_ABORTED_ALREADY                'The Abort command was already completed.
        RESULT_ERROR_ACCESS_DENIED                  'Access to the camera is denied.
        RESULT_ERROR_RESOURCE_IN_USE                'The handle or resource has already been used.

        RESULT_ERROR_CANNOT_FIND_INTERFACE          'Failed to find the interface.

        RESULT_ERROR_XML_UNKNOWN_ARGUMENT           'The argument value is out of range or cannot be recognized.
        RESULT_ERROR_XML_NODE_ACCESS_FAILED         'The Node map is abnormal or AccessMode for the node is NI/NA. 
        RESULT_ERROR_XML_NOT_EXIST_NODE             'The node does not exist.
        RESULT_ERROR_XML_ENTERED_NODE_DOESNT_HAVE_ANY_VALUE         'The node doesn't have any value like COMMAND, CATEGORY.
        RESULT_ERROR_XML_UNSUPPORTED_COMMAND                        'Unknown command.

        RESULT_ERROR_INVALID_WIDTH                  'Width is not multiples of 4.
        RESULT_ERROR_INVALID_ADDRESS                'Unknown address.

        RESULT_ERROR_VWINTERFACE_NO_NIC             'There is no interface card.
        RESULT_ERROR_VWINTERFACE_GETINTERFACENAME   'Failed to retrieve a interface name.
        RESULT_ERROR_VWINTERFACE_OPENINTERFACE      'OpenInterface function returned an error.
        RESULT_ERROR_VWINTERFACE_CLOSEINTERFACE     'CloseInterface function returned an error.
        RESULT_ERROR_VWINTERFACE_GETNUMDEVICES      'GetNumDevices function returned an error.
        RESULT_ERROR_VWINTERFACE_CANNOT_FIND_DEVICE 'Failed to find a device.

        RESULT_ERROR_VWCAMERA_INTERFACE_HANDLE      'The interface handle is invalid.
        RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER      'The camera index is over the maximum number of cameras.
        RESULT_ERROR_VWCAMERA_GETXML                'There is a problem with the XML that was retrieved from the camera.
        RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE      'Width must divide by 4.
        RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE      'Width must divide by 2.
        RESULT_ERROR_VWCAMERA_READ_ONLY             'The node is read only.
        RESULT_ERROR_VWCAMERA_EVENTCONTROL_DOESNOT_INIT         'Event control function did not initialize.
        RESULT_ERROR_VWCAMERA_GRAB_TIMEOUT                      'Time-out is occurred in grab routine.
        RESULT_ERROR_VWCAMERA_CALLBACK_NOT_NULL                 'Callback function pointer is not null.


        RESULT_ERROR_DEVCREATEDATASTREAM                        'Internal error. GenTL DevCreateDataStream function returned error.

        RESULT_ERROR_DATASTREAM_MTU                             'Internal error. The MTU of the NIC is too small to get a image.

        RESULT_ERROR_TLGETNUMINTERFACES                         'Internal error. GenTL TLGetNumInterfaces function returned error.
        RESULT_ERROR_TLOPENINTERFACE                            'Internal error. GenTL TLOpenInterface function returns error.
        RESULT_ERROR_TLCLOSEINTERFACE                           'Internal error. GenTL TLCloseInterface function returns error.
        RESULT_ERROR_TLGETINTERFACENAME                         'Internal error. GenTL TLGetInterfaceName function returned error.
        RESULT_ERROR_TLGETNUMDEVICES                            'Internal error. GenTL TLGetNumDevices function returns error.
        RESULT_ERROR_TLGETDEVICENAME                            'Internal error. GenTL TLGetDeviceName function returns error.
        RESULT_ERROR_TLOPENDEVICE                               'Internal error. GenTL TLOpenDevice function returns error.

        RESULT_ERROR_INSUFFICIENT_RESOURCES                     'Insufficient system resources. Unable to allocate memory as many as defined the number of buffers.
        RESULT_ERROR_MEMORY_ALLOCATION                          'Unable to allocate memory.

        RESULT_ERROR_FILE_STREAM_OPEN_FAILURE                   'Failed to open File stream.
        RESULT_ERROR_FILE_STREAM_READ_FAILURE                   'Failed to read File stream.
        RESULT_ERROR_FILE_STREAM_WRITE_FAILURE                  'Failed to write File stream.
        RESULT_ERROR_FILE_STREAM_CLOSE_FAILURE                  'Failed to close File stream.
        RESULT_ERROR_FILE_STREAM_NOT_CORRECT_FILE_LENGTH        'File length is incorrect.

        RESULT_ERROR_EXCEPTION                                  'An exception is occurred.

        RESULT_LAST                                             'The count of error items. Don't use it.
        ' Don't Use it.
    End Enum

    Enum GET_CUSTOM_COMMAND
        GET_CUSTOM_COMMAND_VALUE = &HF0
        ' Value
        GET_CUSTOM_COMMAND_NUM
        ' Entry Num
        GET_CUSTOM_COMMAND_MIN
        ' Minimum
        GET_CUSTOM_COMMAND_MAX
        ' Maximum
        GET_CUSTOM_COMMAND_INC
        ' Increment
        GET_CUSTOM_COMMAND_INDEX
    End Enum


    Public Enum PIXEL_FORMAT
        PIXEL_FORMAT_MONO8 = &H1080001
        PIXEL_FORMAT_MONO8_SIGNED = &H1080002
        PIXEL_FORMAT_MONO10 = &H1100003
        PIXEL_FORMAT_MONO10_P = &H10A0046
        PIXEL_FORMAT_MONO10_PACKED = &H10C0004
        PIXEL_FORMAT_MONO12 = &H1100005
        PIXEL_FORMAT_MONO12_P = &H10A0047
        PIXEL_FORMAT_MONO12_PACKED = &H10C0006
        PIXEL_FORMAT_MONO14 = &H1100025
        PIXEL_FORMAT_MONO16 = &H1100007
        PIXEL_FORMAT_BAYGR8 = &H1080008
        PIXEL_FORMAT_BAYRG8 = &H1080009
        PIXEL_FORMAT_BAYGB8 = &H108000A
        PIXEL_FORMAT_BAYBG8 = &H108000B
        PIXEL_FORMAT_BAYGR10 = &H110000C
        PIXEL_FORMAT_BAYRG10 = &H110000D
        PIXEL_FORMAT_BAYGB10 = &H110000E
        PIXEL_FORMAT_BAYBG10 = &H110000F
        PIXEL_FORMAT_BAYGR10_PACKED = &H10C0026
        PIXEL_FORMAT_BAYGR12_PACKED = &H10C002A
        PIXEL_FORMAT_BAYGR12 = &H1100010
        PIXEL_FORMAT_BAYRG12 = &H1100011
        PIXEL_FORMAT_BAYGB12 = &H1100012
        PIXEL_FORMAT_BAYBG12 = &H1100013
        PIXEL_FORMAT_BAYRG10_PACKED = &H10C0027
        PIXEL_FORMAT_BAYRG12_PACKED = &H10C002B

        PIXEL_FORMAT_RGB8_PACKED = &H2180014
        PIXEL_FORMAT_BGR8_PACKED = &H2180015
        PIXEL_FORMAT_YUV422_UYVY = &H210001F
        PIXEL_FORMAT_YUV422_YUYV = &H2100032
        PIXEL_FORMAT_YUV422_10_PACKED = &H80180001
        PIXEL_FORMAT_YUV422_12_PACKED = &H80180002
        PIXEL_FORMAT_YUV411 = &H20C001E
        PIXEL_FORMAT_YUV411_10_PACKED = &H80120004
        PIXEL_FORMAT_YUV411_12_PACKED = &H80120005
        PIXEL_FORMAT_BGR10V1_PACKED = &H220001C
        PIXEL_FORMAT_BGR10V2_PACKED = &H220001D
        PIXEL_FORMAT_RGB12_PACKED = &H230001A
        PIXEL_FORMAT_BGR12_PACKED = &H230001B
        PIXEL_FORMAT_YUV444 = &H2180020
        PIXEL_FORMAT_PAL_INTERLACED = &H2100001
        PIXEL_FORMAT_NTSC_INTERLACED = &H2100002
    End Enum

    Public Enum GAIN_COLOR
        GAIN_COLOR_RED
        GAIN_COLOR_GREEN
        GAIN_COLOR_BLUE
    End Enum

    Public Enum STREAM_INFO
        STREAM_INFO_NUM_OF_FRAMES_LOST
        STREAM_INFO_NUM_PACKETS_MISSING

    End Enum

    Public Structure OBJECT_INFO
        Public pUserPointer As IntPtr
        Public pVwCamera As IntPtr
    End Structure


    Public Structure IMAGE_INFO
        Public callbackResult As RESULT
        Public bufferIndex As Integer
        Public pixelFormat As PIXEL_FORMAT
        Public width As Integer
        Public height As Integer
        Public unTimeStamp As System.UInt64
        Public ImageStatus As Integer
        Public btReserved As Byte
        Public pImage As IntPtr
    End Structure

    Public Structure INTERFACE_INFO_STRUCT
        Public [error] As Boolean
        Public errorCause As RESULT
        Public index As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public name As Char()
    End Structure

    Public Structure CAMERA_INFO_STRUCT
        Public [error] As Boolean
        Public errorResult As RESULT
        Public index As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public name As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public vendor As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public model As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public ip As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)> _
        Public mac As String

    End Structure

    Public Enum PROPERTY_TYPE
        UNKNOWN = 0
        ATTR_UINT = 1
        ATTR_FLOAT = 2
        ATTR_ENUM = 3
        ATTR_BOOLEAN = 4
        ATTR_STRING = 5
        ATTR_COMMAND = 6
        ATTR_CATEGORY = 7
    End Enum

    Public Enum PROPERTY_ACCESS_MODE
        NOT_IMPLEMENT = 0
        NOT_AVAILABLE = 1
        WRITE_ONLY = 2
        READ_ONLY = 3
        READ_WRITE = 4
    End Enum

    Public Enum PROPERTY_VISIBILITY
        BEGINNER = 0
        EXPERT = 1
        GURU = 2
        INVISIBLE = 3
        UNDEFINE = 4
    End Enum

    Public Enum PROPERTY_REPRESENTATION
        REP_LINEAR
        REP_LOGARITHMIC
        REP_BOOLEAN
        REP_PURENUMBER
        REP_HEXNUMBER
        REP_IPV4ADDRESS
        REP_MACADDRESS
        REP_UNDEFINE
    End Enum

    Public Structure [PROPERTY]
        Public ePropType As PROPERTY_TYPE
        Public eAccessMode As PROPERTY_ACCESS_MODE
        Public eVisibility As PROPERTY_VISIBILITY
        Public eRepresentation As PROPERTY_REPRESENTATION
        Private unPollingTime As UInteger
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
        Public caName As Char()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
        Public caDisplay As Char()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2048)>
        Public caDescription As Char()
    End Structure





    Public Class VwCL
        ' CL
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function OpenVwCL(ByRef hVwCL As VWCL_HANDLE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CloseVwCL(ByRef hVwCL As VWCL_HANDLE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwGetNumInterfaces(hVwCL As VWCL_HANDLE, pNumInterfaces As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwGetNumCameras(hVwCL As VWCL_HANDLE, ByRef aPNumCamera As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenCameraByIndex(hVwCL As VWCL_HANDLE, nCameraIndex As Integer, ByRef phCamera As HCAMERA, nNumBuffer As Integer, nWidth As Integer, nHeight As Integer, _
   nPacketSize As Integer, pUserPointer As IntPtr, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
        Public Delegate Sub ImageCallbackFn(pObjectInfo As IntPtr, ByRef pImageInfo As IMAGE_INFO)
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenCameraByName(hVwCL As VWCL_HANDLE, pCameraName As IntPtr, ByRef phCamera As HCAMERA, nNumBuffer As Integer, nWidth As Integer, nHeight As Integer, _
   nPacketSize As Integer, pUserPointer As IntPtr, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenInterfaceByIndex(hVwCL As VWCL_HANDLE, aNIndex As Integer, ByRef phInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenInterfaceByName(hVwCL As VWCL_HANDLE, pInterfaceName As IntPtr, ByRef phInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscovery(hVwCL As VWCL_HANDLE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscoveryInterfaceInfo(hVwCL As VWCL_HANDLE, nIndex As Integer, ByRef pInterfaceInfoStruct As INTERFACE_INFO_STRUCT) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscoveryCameraInfo(hVwCL As VWCL_HANDLE, nIndex As Integer, ByRef pCameraInfoStruct As CAMERA_INFO_STRUCT) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceOpenCameraByIndex(hInstance As HINTERFACE, pCallbackParent As IntPtr, nDevIndex As Integer, phCamera As IntPtr, nNumBuffer As Integer, nWidth As Integer, _
   nHeight As Integer, nPacketSize As Integer, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceOpenCameraByName(hInterface As HINTERFACE, pParent As IntPtr, pName As IntPtr, phCamera As IntPtr, nNumBuffer As Integer, nWidth As Integer, _
   nHeight As Integer, nPacketSize As Integer, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetNumCameras(hInterface As HINTERFACE, aPNumDevices As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetCameraName(hInterface As HINTERFACE, aNDevIndex As Integer, aName As IntPtr, aPNameSize As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceCloseInterface(hInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetVwCLHandle(hInterface As HINTERFACE, phVwCLHandle As IntPtr) As RESULT
        End Function

        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraClose(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGrab(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSnap(hCamera As HCAMERA, aNFrame As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraAbort(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetGrabCondition(hCamera As HCAMERA, ByRef bIsGrabbing As Boolean) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetWidth(hCamera As HCAMERA, ByRef aPWidth As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetHeight(hCamera As HCAMERA, ByRef aPHeight As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetWidth(hCamera As HCAMERA, aNWidth As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetHeight(hCamera As HCAMERA, aNHeight As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetPixelFormat(hCamera As HCAMERA, pixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelFormat(hCamera As HCAMERA, ByRef pPixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetAcquisitionTimeOut(hCamera As HCAMERA, npNum As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetAcquisitionTimeOut(hCamera As HCAMERA, ByRef pnTimeOut As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceID(hCamera As HCAMERA, uIndex As Integer, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraChangeBufferFormat(hCamera As HCAMERA, nBufferNum As Integer, nWidth As Integer, nHeight As Integer, pixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetBufferInfo(hCamera As HCAMERA, nBufferNum As IntPtr, nWidth As IntPtr, nHeight As IntPtr, pixelFormat As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetInterfaceHandle(hCamera As HCAMERA, phInterface As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetStreamInfo(hCamera As HCAMERA, streamInfo As STREAM_INFO, nInfo As IntPtr) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetCustomCommand(ByVal hCamera As HCAMERA, ByVal pCommand As Char(), ByVal pArg As Char()) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetCustomCommand(ByVal hCamera As HCAMERA, ByVal pCommand As Char(), ByVal pArg As Byte(), ByRef pArgSize As Integer, ByVal nCmdType As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetUARTCustomCommand(hCamera As HCAMERA, pCommand As [Byte]()) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetUARTCustomCommand(hCamera As HCAMERA, pCommand As [Byte](), pArg As [Byte](), ByRef nArgSize As Integer) As RESULT
        End Function

        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPropertyCount(ByVal hCamera As HCAMERA, ByRef pCount As Integer) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPropertyInfo(ByVal hCamera As HCAMERA, ByVal pCommand As Char(), ByRef ptPropInfo As [PROPERTY]) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPropertyInfoUsingIndex(ByVal hCamera As HCAMERA, ByVal nIndex As Integer, ByRef ptPropInfo As [PROPERTY]) As RESULT
        End Function
        <DllImport("VwCL.NET.DLL", CallingConvention:=CallingConvention.Cdecl)>
        Public Shared Function CameraGetDeviceHandle(ByVal hCamera As HCAMERA, ByRef pDevice As HDEVICE) As RESULT
        End Function
    End Class
End Namespace
