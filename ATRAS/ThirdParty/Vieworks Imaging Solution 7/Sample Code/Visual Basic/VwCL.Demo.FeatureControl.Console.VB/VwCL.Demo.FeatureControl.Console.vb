Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices

Imports VWCL_HANDLE = System.IntPtr
Imports HCAMERA = System.IntPtr

Module Module1

    Sub Main()
        System.Console.WriteLine("Step 0. Initialize....")
        Dim vwCL As VWCL_HANDLE = IntPtr.Zero
        Dim ret As Vieworks.RESULT = Vieworks.VwCL.OpenVwCL(vwCL)

        If Vieworks.RESULT.RESULT_SUCCESS <> ret Then
            System.Console.WriteLine("Failed(err:{0}))", ret)
            Return
        End If

        System.Console.WriteLine("OK")
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

MAIN_MENU:
        While (True)
            System.Console.WriteLine(vbLf & vbTab & "Select menu")
            System.Console.WriteLine(vbTab & "1.Get All Feature List")
            System.Console.WriteLine(vbTab & "2.Get Feature Information")
            System.Console.WriteLine(vbTab & "3.Set Feature Data")
            System.Console.WriteLine(vbTab & "4.Get Feature Data")
            System.Console.WriteLine(vbTab & "5.Get Feature Min/Max")
            System.Console.WriteLine(vbTab & "6.Get list of Enumeration Feature")
            System.Console.WriteLine(vbTab & "7.Quit")
            System.Console.WriteLine(">")

            Dim tKeyInfo As ConsoleKeyInfo = System.Console.ReadKey(False)

            If (tKeyInfo.Key >= ConsoleKey.NumPad1 AndAlso tKeyInfo.Key <= ConsoleKey.NumPad6) OrElse (tKeyInfo.Key >= ConsoleKey.D1 AndAlso tKeyInfo.Key <= ConsoleKey.D6) Then
                Dispatch(tKeyInfo.Key, hCamera)
            ElseIf tKeyInfo.Key = ConsoleKey.D7 OrElse tKeyInfo.Key = ConsoleKey.NumPad7 Then
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

        Select Case Key
            Case ConsoleKey.D1, ConsoleKey.NumPad1
                Dim nPropCount As Integer = 0
                Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetPropertyCount(hCamera, nPropCount)

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                Else
                    Dim tPropertyInfo As New Vieworks.[PROPERTY]

                    For nIdx As Integer = 0 To nPropCount - 1
                        ret = Vieworks.VwCL.CameraGetPropertyInfoUsingIndex(hCamera, nIdx, tPropertyInfo)

                        If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                            System.Console.WriteLine("Failed(err:{0}))", ret)
                            Exit For
                        Else
                            System.Console.WriteLine(tPropertyInfo.caName)
                        End If
                    Next
                End If

            Case ConsoleKey.D2, ConsoleKey.NumPad2
                System.Console.WriteLine(vbLf & "Input a name of Feature")
                System.Console.WriteLine(">")
                Dim sFeatureName As String = System.Console.ReadLine()
                Dim tPropertyInfo As New Vieworks.[PROPERTY]
                Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), tPropertyInfo)

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                Else

                    If Vieworks.PROPERTY_ACCESS_MODE.READ_ONLY = tPropertyInfo.eAccessMode Then
                        System.Console.WriteLine("Access Mode : Read Only")
                    ElseIf Vieworks.PROPERTY_ACCESS_MODE.WRITE_ONLY = tPropertyInfo.eAccessMode Then
                        System.Console.WriteLine("Access Mode : Write Only")
                    ElseIf Vieworks.PROPERTY_ACCESS_MODE.READ_WRITE = tPropertyInfo.eAccessMode Then
                        System.Console.WriteLine("Access Mode : Read Write")
                    ElseIf Vieworks.PROPERTY_ACCESS_MODE.NOT_AVAILABLE = tPropertyInfo.eAccessMode Then
                        System.Console.WriteLine("Access Mode : Not Available")
                    Else
                        System.Console.WriteLine("Access Mode : Not Implement")
                    End If

                    If Vieworks.PROPERTY_TYPE.ATTR_BOOLEAN = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Boolean")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_CATEGORY = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Category")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_COMMAND = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Command")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_ENUM = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Enumeration")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_FLOAT = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Float")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_STRING = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : String")
                    ElseIf Vieworks.PROPERTY_TYPE.ATTR_UINT = tPropertyInfo.ePropType Then
                        System.Console.WriteLine("Property Type : Integer")
                    Else
                        System.Console.WriteLine("Property Type : Unknown")
                    End If

                    If Vieworks.PROPERTY_VISIBILITY.BEGINNER = tPropertyInfo.eVisibility Then
                        System.Console.WriteLine("Property Visibility : Beginner")
                    ElseIf Vieworks.PROPERTY_VISIBILITY.EXPERT = tPropertyInfo.eVisibility Then
                        System.Console.WriteLine("Property Visibility : Expert")
                    ElseIf Vieworks.PROPERTY_VISIBILITY.GURU = tPropertyInfo.eVisibility Then
                        System.Console.WriteLine("Property Visibility : Guru")
                    ElseIf Vieworks.PROPERTY_VISIBILITY.INVISIBLE = tPropertyInfo.eVisibility Then
                        System.Console.WriteLine("Property Visibility : Invisible")
                    Else
                        System.Console.WriteLine("Property Visibility : Undefined")
                    End If
                End If

            Case ConsoleKey.D3, ConsoleKey.NumPad3
                System.Console.WriteLine(vbLf & "Input a name of Feature")
                System.Console.WriteLine(">")
                Dim sFeatureName As String = System.Console.ReadLine()
                System.Console.WriteLine(vbLf & "Input a value")
                System.Console.WriteLine(">")
                Dim sArgument As String = System.Console.ReadLine()
                Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraSetCustomCommand(hCamera, sFeatureName.ToCharArray(), sArgument.ToCharArray())

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                End If

            Case ConsoleKey.D4, ConsoleKey.NumPad4
                System.Console.WriteLine(vbLf & "Input a name of Feature")
                System.Console.WriteLine(">")
                Dim sFeatureName As String = System.Console.ReadLine()
                Dim tPropInfo As Vieworks.[PROPERTY] = New Vieworks.[PROPERTY]()
                Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), tPropInfo)

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                End If

                Const STR_SIZE As Integer = 256
                Dim btArg As Byte() = New Byte(255) {}
                Dim nSize As Integer = STR_SIZE
                ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                Else
                    Dim acHashedData As Char() = New Char(nSize - 1) {}
                    Dim nValidCount As Integer = 0

                    For i As Integer = 0 To nSize - 1

                        If btArg(i) = 0 Then
                            Exit For
                        End If

                        nValidCount += 1
                    Next

                    acHashedData = Encoding.[Default].GetChars(btArg, 0, nValidCount)
                    Dim strArg As String = New String(acHashedData)

                    Select Case tPropInfo.ePropType
                        Case Vieworks.PROPERTY_TYPE.ATTR_BOOLEAN
                            Dim nValue As Integer = Convert.ToInt32(strArg)

                            If nValue = 0 Then
                                System.Console.WriteLine("Value : FALSE")
                            Else
                                System.Console.WriteLine("Value : TRUE")
                            End If

                            Exit Select
                        Case Vieworks.PROPERTY_TYPE.ATTR_UINT, Vieworks.PROPERTY_TYPE.ATTR_FLOAT

                            Select Case tPropInfo.eRepresentation
                                Case Vieworks.PROPERTY_REPRESENTATION.REP_HEXNUMBER
                                    Dim nIP As Integer = Convert.ToInt32(strArg)
                                    System.Console.WriteLine("Value : {0}", nIP)
                                    Exit Select
                                Case Vieworks.PROPERTY_REPRESENTATION.REP_IPV4ADDRESS
                                    Dim ulIP As Int64 = Convert.ToInt64(strArg)
                                    
                                    Exit Select
                                Case Else
                                    System.Console.WriteLine("Value : {0}", strArg)
                            End Select

                            Exit Select
                        Case Vieworks.PROPERTY_TYPE.ATTR_ENUM, Vieworks.PROPERTY_TYPE.ATTR_STRING
                            System.Console.WriteLine("Value : {0}", strArg)
                            Exit Select
                        Case Vieworks.PROPERTY_TYPE.ATTR_CATEGORY
                            System.Console.WriteLine("Value : This is Category type feature.")
                            Exit Select
                        Case Vieworks.PROPERTY_TYPE.ATTR_COMMAND
                            System.Console.WriteLine("Value : This is Command type feature.")
                            Exit Select
                        Case Else
                            System.Console.WriteLine("Value : {0}", strArg)
                    End Select
                End If

            Case ConsoleKey.D5, ConsoleKey.NumPad5
                System.Console.WriteLine(vbLf & "Input a name of Feature")
                System.Console.WriteLine(">")
                Dim sFeatureName As String = System.Console.ReadLine()

                If True Then
                    Const STR_SIZE As Integer = 256
                    Dim btArg As Byte() = New Byte(255) {}
                    Dim nSize As Integer = STR_SIZE
                    Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_MAX))

                    If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                        System.Console.WriteLine("Failed(err:{0}))", ret)
                    Else
                        Dim acHashedData As Char() = New Char(nSize - 1) {}
                        Dim nValidCount As Integer = 0

                        For i As Integer = 0 To nSize - 1

                            If btArg(i) = 0 Then
                                Exit For
                            End If

                            nValidCount += 1
                        Next

                        acHashedData = Encoding.[Default].GetChars(btArg, 0, nValidCount)
                        Dim strArg As String = New String(acHashedData)
                        System.Console.WriteLine("Max value : {0}", strArg)
                    End If
                End If

                If True Then
                    Const STR_SIZE As Integer = 256
                    Dim btArg As Byte() = New Byte(255) {}
                    Dim nSize As Integer = STR_SIZE
                    Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_MIN))

                    If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                        System.Console.WriteLine("Failed(err:{0}))", ret)
                    Else
                        Dim acHashedData As Char() = New Char(nSize - 1) {}
                        Dim nValidCount As Integer = 0

                        For i As Integer = 0 To nSize - 1

                            If btArg(i) = 0 Then
                                Exit For
                            End If

                            nValidCount += 1
                        Next

                        acHashedData = Encoding.[Default].GetChars(btArg, 0, nValidCount)
                        Dim strArg As String = New String(acHashedData)
                        System.Console.WriteLine("Min value : {0}", strArg)
                    End If
                End If

            Case ConsoleKey.D6, ConsoleKey.NumPad6
                System.Console.WriteLine(vbLf & "Input a name of Feature")
                System.Console.WriteLine(">")
                Dim sFeatureName As String = System.Console.ReadLine()
                Dim tPropInfo As Vieworks.[PROPERTY] = New Vieworks.[PROPERTY]()
                Dim ret As Vieworks.RESULT = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), tPropInfo)

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                End If

                Dim nCount As Integer = 0
                Const STR_SIZE As Integer = 256
                Dim btArg As Byte() = New Byte(255) {}
                Dim nSize As Integer = STR_SIZE
                ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM))

                If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                    System.Console.WriteLine("Failed(err:{0}))", ret)
                Else
                    Dim acHashedData As Char() = New Char(nSize - 1) {}
                    Dim nValidCount As Integer = 0

                    For i As Integer = 0 To nSize - 1

                        If btArg(i) = 0 Then
                            Exit For
                        End If

                        nValidCount += 1
                    Next

                    acHashedData = Encoding.[Default].GetChars(btArg, 0, nValidCount)
                    Dim strArg As String = New String(acHashedData)
                    System.Console.WriteLine("The number of item : {0}", strArg)
                    nCount = Convert.ToInt32(strArg)
                End If

                For i As Integer = 0 To nCount - 1
                    System.Array.Clear(btArg, 0, STR_SIZE)
                    nSize = STR_SIZE
                    ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX) + i)

                    If ret <> Vieworks.RESULT.RESULT_SUCCESS Then
                        System.Console.WriteLine("Failed(err:{0}))", ret)
                    Else
                        Dim acHashedData As Char() = New Char(nSize - 1) {}
                        Dim nValidCount As Integer = 0

                        For nIdx As Integer = 0 To nSize - 1

                            If btArg(nIdx) = 0 Then
                                Exit For
                            End If

                            nValidCount += 1
                        Next

                        acHashedData = Encoding.[Default].GetChars(btArg, 0, nValidCount)
                        Dim strArg As String = New String(acHashedData)
                        System.Console.WriteLine(vbTab & "{0}", strArg)
                    End If
                Next

            Case Else
                System.Console.WriteLine("Unrecognized command")
        End Select
    End Sub

End Module