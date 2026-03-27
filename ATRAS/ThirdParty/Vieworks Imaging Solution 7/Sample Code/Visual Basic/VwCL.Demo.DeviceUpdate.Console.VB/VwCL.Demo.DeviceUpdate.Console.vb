Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Imports VWCL_HANDLE = System.IntPtr
Imports HCAMERA = System.IntPtr
Imports HDEVICE = System.IntPtr



Module Module1

    Sub Main()
        System.Console.WriteLine("Step 0. Initialize....")
        Dim vwCL As VWCL_HANDLE = IntPtr.Zero
        Dim ret As Vieworks.RESULT = Vieworks.VwCL.OpenVwCL(vwCL)

        If Vieworks.RESULT.RESULT_SUCCESS <> ret Then
            System.Console.WriteLine("Failed(err:{0}))", ret)
            Return
        End If

        System.Console.WriteLine("OK" & vbLf)
        System.Console.WriteLine("Step 1. Open Device.....")
        Dim hCamera As HCAMERA = IntPtr.Zero
        Dim hObjectInfo As New Vieworks.OBJECT_INFO()
        Dim gchobjectInfo As GCHandle = GCHandle.Alloc(hObjectInfo)
        Dim pObjectInfo As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(hObjectInfo))
        Dim nDeviceIndex As Integer = 0
        Dim nImageBufferNumber As Integer = 20
        ret = Vieworks.VwCL.VwOpenCameraByIndex(vwCL, nDeviceIndex, hCamera, nImageBufferNumber, 0, 0, 0, pObjectInfo, IntPtr.Zero, IntPtr.Zero)

        If ret <> Vieworks.RESULT.RESULT_SUCCESS Then

            Select Case ret
                Case Vieworks.RESULT.RESULT_ERROR_DEVCREATEDATASTREAM
                    System.Console.WriteLine("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned")
                Case Vieworks.RESULT.RESULT_ERROR_NO_CAMERAS
                    System.Console.WriteLine("ERROR : RESULT_ERROR_NO_CAMERAS was returned")
                    System.Console.WriteLine("CHECK : Camera connection")
                Case Vieworks.RESULT.RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER
                    System.Console.WriteLine("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned")
                    System.Console.WriteLine("CHECK : Zero-based camera index")
                Case Else
                    System.Console.WriteLine("ERROR : Default error code returned")
            End Select

            Marshal.FreeHGlobal(pObjectInfo)
            If IntPtr.Zero <> hCamera Then Vieworks.VwCL.CameraClose(hCamera)
            If IntPtr.Zero <> vwCL Then Vieworks.VwCL.CloseVwCL(vwCL)
            Return
        End If

        Dim hDev As HDEVICE = IntPtr.Zero
        Vieworks.VwCL.CameraGetDeviceHandle(hCamera, hDev)

MAIN_MENU:
        While (True)
            System.Console.WriteLine(vbLf & vbTab & "Select menu")
            System.Console.WriteLine(vbTab & "1.Firmware Download( PC to Camera )")
            System.Console.WriteLine(vbTab & "2.Defect Download( PC to Camera )")
            System.Console.WriteLine(vbTab & "3.Defect Upload( Camera to PC )")
            System.Console.WriteLine(vbTab & "4.LUT Download( PC to Camera )")
            System.Console.WriteLine(vbTab & "5.LUT Upload( Camera to PC )")
            System.Console.WriteLine(vbTab & "6.FFC Download( PC to Camera )")
            System.Console.WriteLine(vbTab & "7.FFC Upload( Camera to PC )")
            System.Console.WriteLine(vbTab & "8.Quit")
            System.Console.WriteLine(">")


            Dim tKeyInfo As ConsoleKeyInfo = System.Console.ReadKey(False)

            If (tKeyInfo.Key >= ConsoleKey.NumPad1 AndAlso tKeyInfo.Key <= ConsoleKey.NumPad7) OrElse (tKeyInfo.Key >= ConsoleKey.D1 AndAlso tKeyInfo.Key <= ConsoleKey.D7) Then
                Dispatch(tKeyInfo.Key, hDev)
            ElseIf tKeyInfo.Key = ConsoleKey.D8 OrElse tKeyInfo.Key = ConsoleKey.NumPad8 Then
                ' escape
                Exit While
            Else
                'Unknown key
            End If

        End While

        Dim tKeyExit As ConsoleKeyInfo

        Do
            System.Console.WriteLine("** Goto Select menu?")
            System.Console.WriteLine("** Y")
            System.Console.WriteLine("** N")
            System.Console.WriteLine(">")
            tKeyExit = System.Console.ReadKey(False)
        Loop While tKeyExit.Key <> ConsoleKey.Y AndAlso tKeyExit.Key <> ConsoleKey.N

        If tKeyExit.Key = ConsoleKey.Y Then
            GoTo MAIN_MENU
        End If

        System.Console.WriteLine("Step 3. Terminate.....")
        Marshal.FreeHGlobal(pObjectInfo)
        If IntPtr.Zero <> hCamera Then Vieworks.VwCL.CameraClose(hCamera)
        If IntPtr.Zero <> vwCL Then Vieworks.VwCL.CloseVwCL(vwCL)

    End Sub

    Public Sub Dispatch(ByVal Key As System.ConsoleKey, ByVal hCamera As HCAMERA)
        If IntPtr.Zero = hCamera Then Return
        System.Console.WriteLine(vbLf & "Input a name of file.")
        System.Console.WriteLine(">")
        Dim strFileName As String = System.Console.ReadLine()
        Dim pFileName As IntPtr = Marshal.StringToBSTR(strFileName)


        Dim pCallback As Vieworks.VwDeviceMaintenance.ProgressCallbackFn = New Vieworks.VwDeviceMaintenance.ProgressCallbackFn(AddressOf UpdateCallbackFunc)
        Dim gchCallback As GCHandle = GCHandle.Alloc(pCallback)
        Dim ptrCallback As IntPtr = Marshal.GetFunctionPointerForDelegate(pCallback)
        Dim eRet As Vieworks.ERESULT_ERROR = Vieworks.ERESULT_ERROR.ERESULT_SUCCESS

        Select Case Key
            Case ConsoleKey.D1, ConsoleKey.NumPad1
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPDATE_PKG, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D2, ConsoleKey.NumPad2
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_DEFECT, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D3, ConsoleKey.NumPad3
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_DEFECT, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D4, ConsoleKey.NumPad4
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_LUT, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D5, ConsoleKey.NumPad5
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_LUT, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D6, ConsoleKey.NumPad6
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.DOWNLOAD_FFC, hCamera, pFileName, ptrCallback)
            Case ConsoleKey.D7, ConsoleKey.NumPad7
                eRet = Vieworks.VwDeviceMaintenance.VwUpdateDevice(Vieworks.UPDATE_TARGET.UPLOAD_FFC, hCamera, pFileName, ptrCallback)
            Case Else
                System.Console.WriteLine("Unrecognized command")
        End Select

        If Vieworks.ERESULT_ERROR.ERESULT_SUCCESS = eRet Then
            System.Console.WriteLine("The update has been completed successfully")
        Else
            System.Console.WriteLine("The update has been failed. Err({0})", eRet)
        End If
    End Sub

    Public Sub UpdateCallbackFunc(pUserPoint As IntPtr, nProgress As Integer)
        System.Console.WriteLine("Update Progress({0})", nProgress)
    End Sub

End Module