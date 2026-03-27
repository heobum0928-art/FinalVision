

'namespace Vieworks_Image_Capture.NET_Class_Library_Test // VWFX
Imports VWGIGE_HANDLE = System.IntPtr
Imports HINTERFACE = System.IntPtr
Imports HCAMERA = System.IntPtr
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
        RESULT_SUCCESS
        RESULT_ERROR
        RESULT_ERROR_OPENED_ALREADY
        RESULT_ERROR_INVALID_HANDLE
        RESULT_ERROR_TL_HANDLE
        RESULT_ERROR_TLOPEN
        RESULT_ERROR_IF_HANDLE
        RESULT_ERROR_VWGIGE_INITIALIZATION
        ' VWGIGE Module was not initialized
        RESULT_ERROR_INVALID_PARAMETER
        ' Parameter is invalid
        RESULT_ERROR_DISCOVERY
        RESULT_ERROR_NO_CAMERAS
        ' There are no cameras
        RESULT_ERROR_INVALID_WIDTH

        RESULT_ERROR_VWINTERFACE_GETINTERFACENAME
        RESULT_ERROR_VWINTERFACE_OPENINTERFACE
        RESULT_ERROR_VWINTERFACE_CLOSEINTERFACE
        RESULT_ERROR_VWINTERFACE_GETNUMDEVICES

        RESULT_ERROR_VWCAMERA_INTERFACE
        RESULT_ERROR_VWCAMERA_INTERFACE_HANDLE
        RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER
        RESULT_ERROR_VWCAMERA_GETXML
        RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE
        RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE

        RESULT_ERROR_DEVCREATEDATASTREAM

        RESULT_ERROR_DATASTREAM_MTU
        ' Datastream MTU error
        RESULT_ERROR_TLGETNUMINTERFACES
        RESULT_ERROR_TLOPENINTERFACE
        RESULT_ERROR_TLCLOSEINTERFACE
        RESULT_ERROR_TLGETINTERFACENAME
        RESULT_ERROR_TLGETNUMDEVICES
        RESULT_ERROR_TLGETDEVICENAME
        RESULT_ERROR_TLOPENDEVICE
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
        PIXEL_FORMAT_MONO10_PACKED = &H10C0004
        PIXEL_FORMAT_MONO12 = &H1100005
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
        'PIXEL_FORMAT_YUV422_10_PACKED = &H80180001UI
        'PIXEL_FORMAT_YUV422_12_PACKED = &H80180002UI
        PIXEL_FORMAT_YUV411 = &H20C001E
        'PIXEL_FORMAT_YUV411_10_PACKED = &H80120004UI
        'PIXEL_FORMAT_YUV411_12_PACKED = &H80120005UI
        PIXEL_FORMAT_BGR10V1_PACKED = &H220001C
        PIXEL_FORMAT_BGR10V2_PACKED = &H220001D
        PIXEL_FORMAT_RGB12_PACKED = &H230001A
        PIXEL_FORMAT_BGR12_PACKED = &H230001B
        PIXEL_FORMAT_YUV444 = &H2180020
        PIXEL_FORMAT_PAL_INTERLACED = &H2100001
        PIXEL_FORMAT_NTSC_INTERLACED = &H2100002
    End Enum


    Public Enum TESTIMAGE
        TESTIMAGE_OFF
        TESTIMAGE_BLACK
        TESTIMAGE_WHITE
        TESTIMAGE_GREYHORIZONTALRAMP
        TESTIMAGE_GREYVERTICALRAMP
        TESTIMAGE_GREYHORIZONTALRAMPMOVING
        TESTIMAGE_GREYVERTICALRAMPMOVING
        TESTIMAGE_GREYCROSSRAMP
        TESTIMAGE_GREYCROSSRAMPMOVING
    End Enum

    Public Enum STROBE_POLARITY
        STROBE_POLARITY_ACTIVEHIGH
        STROBE_POLARITY_ACTIVELOW
    End Enum


    Public Enum BLACKLEVEL_SEL
        BLACKLEVEL_SEL_TAP1
        BLACKLEVEL_SEL_TAP2
        BLACKLEVEL_SEL_TAP3
        BLACKLEVEL_SEL_TAP4
    End Enum

    Public Enum EXPOSURE_MODE
        EXPOSURE_MODE_TIMED
        EXPOSURE_MODE_TRIGGERWIDTH
    End Enum

    Public Enum READOUT
        READOUT_NORMAL
        READOUT_AOI
        READOUT_BINNING
        READOUT_HORIZONTALSTART
        READOUT_HORIZONTALEND
        READOUT_VERTICALSTART
        READOUT_VERTICALEND
        READOUT_BINNINGFATOR
    End Enum

    Public Enum GAIN_SEL
        GAIN_ANALOG_ALL
        GAIN_ANALOG_TAP1
        GAIN_ANALOG_TAB2
        GAIN_ANALOG_TAB3
        GAIN_ANALOG_TAB4
    End Enum

    Public Enum TRIGGER_SOURCE
        TRIGGER_SOURCE_SW
        TRIGGER_SOURCE_EXT
    End Enum

    Public Enum TRIGGER_ACTIVATION
        TRIGGER_ACTIVATION_RISINGEDGE
        TRIGGER_ACTIVATION_FALLINGEDGE
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




    Public Class VwGigE
        ' GIGE
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function OpenVwGigE(ByRef hVwGigE As VWGIGE_HANDLE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CloseVwGigE(ByRef hVwGigE As VWGIGE_HANDLE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwGetNumInterfaces(hVwGigE As VWGIGE_HANDLE, pNumInterfaces As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwGetNumCameras(hVwGigE As VWGIGE_HANDLE, ByRef aPNumCamera As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenCameraByIndex(hVwGigE As VWGIGE_HANDLE, nCameraIndex As Integer, ByRef phCamera As HCAMERA, nNumBuffer As Integer, nWidth As Integer, nHeight As Integer, _
   nPacketSize As Integer, pUserPointer As IntPtr, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <UnmanagedFunctionPointer(CallingConvention.Cdecl)> _
        Public Delegate Sub ImageCallbackFn(pObjectInfo As IntPtr, ByRef pImageInfo As IMAGE_INFO)
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenCameraByName(hVwGigE As VWGIGE_HANDLE, pCameraName As IntPtr, ByRef phCamera As HCAMERA, nNumBuffer As Integer, nWidth As Integer, nHeight As Integer, _
   nPacketSize As Integer, pUserPointer As IntPtr, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenInterfaceByIndex(hVwGigE As VWGIGE_HANDLE, aNIndex As Integer, ByRef phInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwOpenInterfaceByName(hVwGigE As VWGIGE_HANDLE, pInterfaceName As IntPtr, ByRef phInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscovery(hVwGigE As VWGIGE_HANDLE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscoveryInterfaceInfo(hVwGigE As VWGIGE_HANDLE, nIndex As Integer, ByRef pInterfaceInfoStruct As INTERFACE_INFO_STRUCT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwDiscoveryCameraInfo(hVwGigE As VWGIGE_HANDLE, nIndex As Integer, ByRef pCameraInfoStruct As CAMERA_INFO_STRUCT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwForceIP(hVwGigE As VWGIGE_HANDLE, pMAC As IntPtr, nIP As Integer, nSubnet As Integer, nGateway As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwUseMTUOptimize(hVwGigE As VWGIGE_HANDLE, bUse As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwSetMultiCastAddress(hVwGigE As VWGIGE_HANDLE, dwMultiCastAddress As UInteger) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function VwGetMultiCastAddress(hVwGigE As VWGIGE_HANDLE, ByRef dwMultiCastAddress As UInteger) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceOpenCameraByIndex(hInstance As HINTERFACE, pCallbackParent As IntPtr, nDevIndex As Integer, phCamera As IntPtr, nNumBuffer As Integer, nWidth As Integer, _
   nHeight As Integer, nPacketSize As Integer, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceOpenCameraByName(hInterface As HINTERFACE, pParent As IntPtr, pName As IntPtr, phCamera As IntPtr, nNumBuffer As Integer, nWidth As Integer, _
   nHeight As Integer, nPacketSize As Integer, pImageCallbackFn As IntPtr, pDisconnectCallbackFn As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetNumCameras(hInterface As HINTERFACE, aPNumDevices As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetCameraName(hInterface As HINTERFACE, aNDevIndex As Integer, aName As IntPtr, aPNameSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetIP(hInterface As HINTERFACE, pInterfaceName As IntPtr, pIP As IntPtr, aIP As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetSubnet(hInterface As HINTERFACE, pInterfaceName As IntPtr, pSubnet As IntPtr, aSubnet As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceCloseInterface(hInterface As HINTERFACE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function InterfaceGetVwGigEHandle(hInterface As HINTERFACE, phVwGigEHandle As IntPtr) As RESULT
        End Function

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraClose(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGrab(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSnap(hCamera As HCAMERA, aNFrame As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraAbort(hCamera As HCAMERA) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetGrabCondition(hCamera As HCAMERA, ByRef bIsGrabbing As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetWidth(hCamera As HCAMERA, ByRef aPWidth As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetHeight(hCamera As HCAMERA, ByRef aPHeight As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetWidth(hCamera As HCAMERA, aNWidth As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetHeight(hCamera As HCAMERA, aNHeight As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetPixelSize(hCamera As HCAMERA, nPixelSize As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelSize(hCamera As HCAMERA, ByRef nPixelSize As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetPixelFormat(hCamera As HCAMERA, pixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelFormat(hCamera As HCAMERA, ByRef pPixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelFormatLineup(hCamera As HCAMERA, nIndex As Integer, ByRef pPixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelFormatLineupNum(hCamera As HCAMERA, ByRef npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelSizeLineup(hCamera As HCAMERA, nIndex As Integer, ByRef nPixelSize As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetPixelSizeLineupNum(hCamera As HCAMERA, ByRef npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetTestImage(hCamera As HCAMERA, aTestImage As TESTIMAGE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTestImage(hCamera As HCAMERA, ByRef pTestImage As TESTIMAGE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTestImageLineup(hCamera As HCAMERA, nIndex As Integer, ByRef pTestImage As TESTIMAGE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTestImageLineupNum(hCamera As HCAMERA, ByRef npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetAcquisitionTimeOut(hCamera As HCAMERA, npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetAcquisitionTimeOut(hCamera As HCAMERA, ByRef pnTimeOut As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetReadoutMode(hCamera As HCAMERA, aReadout As READOUT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetHorizontalStart(hCamera As HCAMERA, uStart As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetHorizontalStart(hCamera As HCAMERA, ByRef uStart As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetHorizontalEnd(hCamera As HCAMERA, uEnd As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetHorizontalEnd(hCamera As HCAMERA, ByRef uEnd As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetVerticalStart(hCamera As HCAMERA, uStart As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetVerticalStart(hCamera As HCAMERA, ByRef uStart As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetVerticalEnd(hCamera As HCAMERA, uEnd As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetVerticalEnd(hCamera As HCAMERA, ByRef uEnd As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceModelName(hCamera As HCAMERA, uIndex As Integer, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceVersion(hCamera As HCAMERA, uIndex As Integer, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceVendorName(hCamera As HCAMERA, uIndex As Integer, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceManufacturerInfo(hCamera As HCAMERA, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetDeviceID(hCamera As HCAMERA, uIndex As Integer, pInfo As [Byte](), pInfoSize As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetTriggerMode(hCamera As HCAMERA, bSet As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerMode(hCamera As HCAMERA, ByRef bSet As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerModeLineup(hCamera As HCAMERA, nIndex As Integer, ByRef nTriggerMode As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerModeLineupNum(hCamera As HCAMERA, ByRef npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetTriggerSource(hCamera As HCAMERA, triggerSource As TRIGGER_SOURCE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerSource(hCamera As HCAMERA, ByRef pTriggerSource As TRIGGER_SOURCE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerSourceLineup(hCamera As HCAMERA, nIndex As Integer, ByRef pTriggerSource As TRIGGER_SOURCE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetTriggerSourceLineupNum(hCamera As HCAMERA, ByRef npNum As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetTriggerActivation(hCamera As HCAMERA, triggerActivation As TRIGGER_ACTIVATION) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetExposureMode(hCamera As HCAMERA, aExpmode As EXPOSURE_MODE) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetExposureTime(hCamera As HCAMERA, aExptime_microsec As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetGain(hCamera As HCAMERA, gainSel As GAIN_SEL, nGainValue As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetBlackLevel(hCamera As HCAMERA, blackLevelSel As BLACKLEVEL_SEL, aBlacklevelVal As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetReverseX(hCamera As HCAMERA, aBSet As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetStrobeOffset(hCamera As HCAMERA, nOffset As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetStrobePolarity(hCamera As HCAMERA, aStrobePolarity As STROBE_POLARITY) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetForceIP(hCamera As HCAMERA, dwIP As Integer, dwSubnet As Integer, dwGateway As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraChangeBufferFormat(hCamera As HCAMERA, nBufferNum As Integer, nWidth As Integer, nHeight As Integer, pixelFormat As PIXEL_FORMAT) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetBufferInfo(hCamera As HCAMERA, nBufferNum As IntPtr, nWidth As IntPtr, nHeight As IntPtr, pixelFormat As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetInterfaceHandle(hCamera As HCAMERA, phInterface As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigESetCurrentIpConfigurationDHCP(hCamera As HCAMERA, bSet As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigESetCurrentIpConfigurationPersistentIP(hCamera As HCAMERA, bSet As Boolean) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigEGetCurrentIpConfigurationDHCP(hCamera As HCAMERA, pbSet As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigEGetCurrentIpConfigurationPersistentIP(hCamera As HCAMERA, pbSet As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigEGetPersistentSubnetMask(hCamera As HCAMERA, pnSubnetMask As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigEGetCurrentMACAddress(hCamera As HCAMERA, pNameSize As IntPtr, pszMACAddress As IntPtr) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigEGetColorRGBGain(hCamera As HCAMERA, nRGBType As Integer, ByRef dpRGBGainValue As Double) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGigESetColorRGBGain(hCamera As HCAMERA, nRGBType As Integer, dRGBGainValue As Double) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetStreamInfo(hCamera As HCAMERA, streamInfo As STREAM_INFO, nInfo As IntPtr) As RESULT
        End Function

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetCustomCommand(hCamera As HCAMERA, pCommand As [Byte](), pArg As [Byte]()) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetCustomCommand(hCamera As HCAMERA, pCommand As [Byte](), pArg As [Byte](), pArgSize As IntPtr, nCmdType As Integer) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraSetUARTCustomCommand(hCamera As HCAMERA, pCommand As [Byte]()) As RESULT
        End Function
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Function CameraGetUARTCustomCommand(hCamera As HCAMERA, pCommand As [Byte](), pArg As [Byte](), ByRef nArgSize As Integer) As RESULT
        End Function

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub Convert8a_4b_4a_8bTo16_16_MMX_I(Src As IntPtr, Dest As [Byte](), LenSrcBytes As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422toBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422toBGR8Interlaced(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte](), bOdd As Boolean, width As Integer, blend As Boolean, _
   _signed As Boolean)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422PackedtoBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV411toBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV411PackedtoBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB12PackedtoBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertRGB8toBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBGR10V2PackedtoBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV444toBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono16PackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMonoPackedToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR8ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGB10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono14ToBGR8(pbSrc As IntPtr, cbSrc As Integer, pbDst As [Byte]())
        End Sub

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono10PackedToMono16bit(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertMono12PackedToMono16bit(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub

        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR10ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYGR12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertBAYRG12ToBGR8(pbSrc As IntPtr, pbDst As [Byte](), width As Integer, height As Integer)
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422_UYVYtoBGR8(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub
        <DllImport("VwGigE.NET.V6.DLL", CallingConvention:=CallingConvention.Cdecl)> _
        Public Shared Sub ConvertYUV422_YUYVtoBGR8(pbSrc As IntPtr, width As Integer, height As Integer, pbDst As [Byte]())
        End Sub

    End Class
End Namespace
