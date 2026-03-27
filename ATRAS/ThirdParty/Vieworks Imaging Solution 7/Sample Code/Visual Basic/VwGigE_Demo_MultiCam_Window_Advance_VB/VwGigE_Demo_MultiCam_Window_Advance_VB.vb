Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging

Imports VWGIGE_HANDLE = System.IntPtr
Imports HINTERFACE = System.IntPtr
Imports HCAMERA = System.IntPtr
Imports System.Reflection


'Namespace VwGigE.Demo.SingleCam.window.Advance.C
Partial Public Class CVwGigE_Demo_SingleCam_Window_Advance_CS
    Inherits Form
    <System.Runtime.InteropServices.DllImport("Kernel32.dll")> _
    Private Shared Function QueryPerformanceCounter(ByRef perfcount As Long) As Boolean
    End Function
    <System.Runtime.InteropServices.DllImport("Kernel32.dll")> _
    Private Shared Function QueryPerformanceFrequency(ByRef freq As Long) As Boolean
    End Function

    Private m_pvwGigE As VWGIGE_HANDLE = IntPtr.Zero
    Private m_lstCamera As HCAMERA() = New HCAMERA(3) {}
    Private m_pCamera As HCAMERA
    Private m_imagebuffernumber As Integer
    Private m_curFPS As Double() = New Double(3) {}
    Private m_imageTimeStamps As List(Of Long)() = New List(Of Long)(3) {}
    Private m_liLastDisplayTime As Long() = New Long(3) {}
    Private m_liFreq As Long
    Private m_nMinInterFrameTime As Long = 0

    Private m_pobjectInfo As IntPtr() = New IntPtr(3) {}
    Private gchCallback As GCHandle
    Private gchobjectInfo As GCHandle
    Private gchGigE As GCHandle

    Private m_nCurrentDeviceIndex As Integer = -1
    Private m_nOldDeviceIndex As Integer = -1
    Private m_deviceState As CDeviceState() = New CDeviceState(3) {}


    Public Sub New()
        InitializeComponent()

        m_imagebuffernumber = 2
        edtNumBuffers.Text = [String].Format("{0}", m_imagebuffernumber)
        edtFrame.Text = "1"

        Dim result As Vieworks.RESULT = Vieworks.VwGigE.OpenVwGigE(m_pvwGigE)
        gchGigE = GCHandle.Alloc(m_pvwGigE)

        If result <> Vieworks.RESULT.RESULT_SUCCESS Then
            MessageBox.Show("Cannot open the camera. Please restart this program.")

            btnCloseCamera.Enabled = False
            btnOpenCamera.Enabled = False
            btnAbort.Enabled = False
            btnGrab.Enabled = False
            btnSnap.Enabled = False
        Else
            btnCloseCamera.Enabled = False
            btnOpenCamera.Enabled = False
            btnAbort.Enabled = False
            edtNumBuffers.Enabled = False

            btnGrab.Enabled = False
            btnSnap.Enabled = False
            cbxPixelFormat.Enabled = False
            cbxPixelSize.Enabled = False
            edtWidth.Enabled = False
            edtHeight.Enabled = False
            edtFrame.Enabled = False
        End If

        QueryPerformanceFrequency(m_liFreq)
        m_nMinInterFrameTime = CLng(m_liFreq \ 30)

        For i As Integer = 0 To 3
            m_deviceState(i) = New CDeviceState()
            m_imageTimeStamps(i) = New List(Of Long)()

        Next
    End Sub

    Public Function ByteArrayToString(btArgResult As Byte(), nSize As Integer) As String

        Dim acTmpHashedData As Char() = New Char(nSize - 1) {}
        Dim nTmpValidCount As Integer = 0
        For i As Integer = 0 To nSize - 1

            If btArgResult(i) = 0 Then
                Exit For
            End If

            nTmpValidCount += 1
        Next
        acTmpHashedData = Encoding.[Default].GetChars(btArgResult, 0, nTmpValidCount)
        Dim strTmpArg As String = New String(acTmpHashedData)
        Return strTmpArg

    End Function

    Public Function ByteArrayToInt(btArgResult As Byte(), nSize As Integer) As Integer

        Dim acTmpHashedData As Char() = New Char(nSize - 1) {}
        Dim nTmpValidCount As Integer = 0
        For i As Integer = 0 To nSize - 1

            If btArgResult(i) = 0 Then
                Exit For
            End If

            nTmpValidCount += 1
        Next
        acTmpHashedData = Encoding.[Default].GetChars(btArgResult, 0, nTmpValidCount)
        Dim strTmpArg As String = New String(acTmpHashedData)
        Dim nTmpArg As Integer = Integer.Parse(strTmpArg)
        Return nTmpArg

    End Function

    Public Function ImageConvert(CamPixelFormat As Vieworks.PIXEL_FORMAT, pImage As IntPtr, dlg As CVwGigE_Demo_SingleCam_Window_Advance_CS) As Bitmap
        Dim nWidth As Integer = 0
        Vieworks.VwGigE.CameraGetWidth(m_lstCamera(0), nWidth)
        Dim nHeight As Integer = 0
        Vieworks.VwGigE.CameraGetHeight(m_lstCamera(0), nHeight)
        Dim biBitCount As Integer = 0

        Dim strPixelFormat As String = ""

        Select Case CamPixelFormat
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8
                If True Then
                    biBitCount = 8
                    strPixelFormat = "Pixel Format : Mono 8" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : Mono 10" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : Mono 12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO14
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : Mono 14" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO16
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : Mono 16" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYGR8" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYRG8" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB8
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : RGB8" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR8
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BGR8" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB10
                If True Then
                    biBitCount = 48
                    strPixelFormat = "Pixel Format : RGB10" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10
                If True Then
                    biBitCount = 48
                    strPixelFormat = "Pixel Format : BGR10" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12
                If True Then
                    biBitCount = 48
                    strPixelFormat = "Pixel Format : RGB12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12
                If True Then
                    biBitCount = 48
                    strPixelFormat = "Pixel Format : BGR12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12
                If True Then
                    biBitCount = 48
                    strPixelFormat = "Pixel Format : BAYGR12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYGR12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYRG10" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYRG12" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYGR10 Packed" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYGR12 Packed" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYRG10 Packed" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : BAYRG12 Packed" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : YUV422 UYVY" & vbLf
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Pixel Format : YUV422 YUYV" & vbLf
                End If
                Exit Select
            Case Else
                If True Then
                    biBitCount = 24
                    strPixelFormat = "Unknown pixel format" & vbLf
                End If
                Exit Select
        End Select


        If pImage = IntPtr.Zero Then
            Return Nothing
        End If

        Dim nSize As Integer = nWidth * nHeight * (biBitCount \ 8)
        Dim array As Byte() = New Byte(nWidth * nHeight * 3 - 1) {}
        Dim arrayUnpacked As Byte() = New Byte(nWidth * nHeight * 3 - 1) {}

        Dim DrawPixelFormat As PixelFormat = PixelFormat.Format8bppIndexed


        Select Case CamPixelFormat
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8
                If True Then
                    array = New Byte(nSize - 1) {}
                    Marshal.Copy(pImage, array, 0, nSize)
                    DrawPixelFormat = PixelFormat.Format8bppIndexed
                End If
                Exit Select

            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10
                If True Then
                    Vieworks.VwImageProcess.ConvertMono10ToBGR8(pImage, nWidth * nHeight * 2, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12
                If True Then
                    Vieworks.VwImageProcess.ConvertMono12ToBGR8(pImage, nWidth * nHeight * 2, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO14
                If True Then
                    Vieworks.VwImageProcess.ConvertMono14ToBGR8(pImage, nWidth * nHeight * 2, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select

            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10_PACKED, Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12_PACKED
                If True Then
                    Vieworks.VwImageProcess.ConvertMonoPackedToBGR8(pImage, CInt(Math.Truncate(1.5 * nWidth * nHeight)), array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select

            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO16
                If True Then
                    Vieworks.VwImageProcess.ConvertMono16PackedToBGR8(pImage, CInt(2 * nWidth * nHeight), array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8
                If True Then
                    'Convert BAYER -> RGB;
                    Vieworks.VwImageProcess.ConvertBAYGR8ToBGR8(pImage, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8
                If True Then
                    'Convert BAYER -> RGB;
                    Vieworks.VwImageProcess.ConvertBAYRG8ToBGR8(pImage, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10
                If True Then
                    Vieworks.VwImageProcess.ConvertBAYGR10ToBGR8(pImage, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb

                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10
                If True Then
                    Vieworks.VwImageProcess.ConvertBAYRG10ToBGR8(pImage, array, nWidth, nHeight)

                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12
                If True Then
                    Vieworks.VwImageProcess.ConvertBAYGR12ToBGR8(pImage, array, nWidth, nHeight)

                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12
                If True Then
                    Vieworks.VwImageProcess.ConvertBAYRG12ToBGR8(pImage, array, nWidth, nHeight)

                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB8
                If True Then
                    Vieworks.VwImageProcess.ConvertRGB8ToBGR8(pImage, nSize, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR8
                If True Then
                    array = New Byte(nSize - 1) {}
                    Marshal.Copy(pImage, array, 0, nSize)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB10
                If True Then
                    Vieworks.VwImageProcess.ConvertRGB10ToBGR8(pImage, nSize, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10
                If True Then
                    Vieworks.VwImageProcess.ConvertBGR10ToBGR8(pImage, nSize, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12
                If True Then
                    Vieworks.VwImageProcess.ConvertRGB12ToBGR8(pImage, nSize, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12
                If True Then
                    Vieworks.VwImageProcess.ConvertBGR12ToBGR8(pImage, nSize, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED
                If True Then
                    Vieworks.VwImageProcess.ConvertMono10PackedToMono16bit(pImage, nWidth, nHeight, arrayUnpacked)
                    Dim unmanagedUnpacked As IntPtr = Marshal.AllocHGlobal(arrayUnpacked.Length)
                    Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length)

                    Vieworks.VwImageProcess.ConvertBAYGR10ToBGR8(unmanagedUnpacked, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb

                    Marshal.FreeHGlobal(unmanagedUnpacked)

                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED
                If True Then
                    Vieworks.VwImageProcess.ConvertMono12PackedToMono16bit(pImage, nWidth, nHeight, arrayUnpacked)
                    Dim unmanagedUnpacked As IntPtr = Marshal.AllocHGlobal(arrayUnpacked.Length)
                    Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length)

                    Vieworks.VwImageProcess.ConvertBAYGR12ToBGR8(unmanagedUnpacked, array, nWidth, nHeight)

                    DrawPixelFormat = PixelFormat.Format24bppRgb

                    Marshal.FreeHGlobal(unmanagedUnpacked)
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED
                If True Then
                    Vieworks.VwImageProcess.ConvertMono10PackedToMono16bit(pImage, nWidth, nHeight, arrayUnpacked)
                    Dim unmanagedUnpacked As IntPtr = Marshal.AllocHGlobal(arrayUnpacked.Length)
                    Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length)

                    Vieworks.VwImageProcess.ConvertBAYRG10ToBGR8(unmanagedUnpacked, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb

                    Marshal.FreeHGlobal(unmanagedUnpacked)

                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED
                If True Then
                    Vieworks.VwImageProcess.ConvertMono12PackedToMono16bit(pImage, nWidth, nHeight, arrayUnpacked)
                    Dim unmanagedUnpacked As IntPtr = Marshal.AllocHGlobal(arrayUnpacked.Length)
                    Marshal.Copy(arrayUnpacked, 0, unmanagedUnpacked, arrayUnpacked.Length)

                    Vieworks.VwImageProcess.ConvertBAYRG12ToBGR8(unmanagedUnpacked, array, nWidth, nHeight)
                    DrawPixelFormat = PixelFormat.Format24bppRgb

                    Marshal.FreeHGlobal(unmanagedUnpacked)
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV
                If True Then
                    Vieworks.VwImageProcess.ConvertYUV422_YUYVToBGR8(pImage, nWidth, nHeight, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select
            Case Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY
                If True Then
                    Vieworks.VwImageProcess.ConvertYUV422_UYVYToBGR8(pImage, nWidth, nHeight, array)
                    DrawPixelFormat = PixelFormat.Format24bppRgb
                End If
                Exit Select

            Case Else
                If True Then
                    Return Nothing
                End If
        End Select


        Dim bitmap As Bitmap = dlg.CreateBitmap(nWidth, nHeight, array, DrawPixelFormat)

        Return bitmap
    End Function

    Public Sub GetImageEvent1(pObjectInfo As IntPtr, ByRef pImageInfo As Vieworks.IMAGE_INFO)
        Dim dlg As CVwGigE_Demo_SingleCam_Window_Advance_CS = Me

        ' FPS
        Dim liTime As Long
        QueryPerformanceCounter(liTime)

        Dim i As Integer = 0
        While dlg.m_imageTimeStamps(0).Count > 30
            dlg.m_imageTimeStamps(0).RemoveAt(i)
            i += 1
        End While

        dlg.m_imageTimeStamps(0).Add(liTime)

        Dim arr As Long() = dlg.m_imageTimeStamps(0).ToArray()
        Dim diff As Long = liTime - arr(0)

        If diff > 0 Then
            dlg.m_curFPS(0) = CDbl(dlg.m_liFreq * CLng(dlg.m_imageTimeStamps(0).Count - 1)) / diff
        Else
            dlg.m_curFPS(0) = 0.0
        End If

        If liTime - dlg.m_liLastDisplayTime(0) > dlg.m_nMinInterFrameTime Then

            Dim pixelFormat__1 As Vieworks.PIXEL_FORMAT = pImageInfo.pixelFormat

            Dim bitmap As Bitmap = ImageConvert(pixelFormat__1, pImageInfo.pImage, dlg)

            Dim rtSize As New Rectangle(0, 0, 772 \ 2, 672 \ 2)

            Dim g As Graphics = dlg.ImageBox.CreateGraphics()
            g.DrawImage(bitmap, rtSize)
        End If

    End Sub

    Public Sub GetImageEvent2(pObjectInfo As IntPtr, ByRef pImageInfo As Vieworks.IMAGE_INFO)
        Dim dlg As CVwGigE_Demo_SingleCam_Window_Advance_CS = Me

        ' FPS
        Dim liTime As Long
        QueryPerformanceCounter(liTime)

        Dim i As Integer = 0
        While dlg.m_imageTimeStamps(1).Count > 30
            dlg.m_imageTimeStamps(1).RemoveAt(i)
            i += 1
        End While

        dlg.m_imageTimeStamps(1).Add(liTime)

        Dim arr As Long() = dlg.m_imageTimeStamps(1).ToArray()
        Dim diff As Long = liTime - arr(0)

        If diff > 0 Then
            dlg.m_curFPS(1) = (dlg.m_liFreq * CLng(dlg.m_imageTimeStamps(1).Count - 1)) \ diff
        Else
            dlg.m_curFPS(1) = 0
        End If

        If liTime - dlg.m_liLastDisplayTime(1) > dlg.m_nMinInterFrameTime Then

            Dim pixelFormat__1 As Vieworks.PIXEL_FORMAT = pImageInfo.pixelFormat

            Dim bitmap As Bitmap = ImageConvert(pixelFormat__1, pImageInfo.pImage, dlg)

            Dim rtSize As New Rectangle(772 \ 2, 0, 772, 672 \ 2)

            Dim g As Graphics = dlg.ImageBox.CreateGraphics()
            g.DrawImage(bitmap, rtSize)
        End If
    End Sub

    Public Sub GetImageEvent3(pObjectInfo As IntPtr, ByRef pImageInfo As Vieworks.IMAGE_INFO)
        Dim dlg As CVwGigE_Demo_SingleCam_Window_Advance_CS = Me

        ' FPS
        Dim liTime As Long
        QueryPerformanceCounter(liTime)

        Dim i As Integer = 0
        While dlg.m_imageTimeStamps(2).Count > 30
            dlg.m_imageTimeStamps(2).RemoveAt(i)
            i += 1
        End While

        dlg.m_imageTimeStamps(2).Add(liTime)

        Dim arr As Long() = dlg.m_imageTimeStamps(2).ToArray()
        Dim diff As Long = liTime - arr(0)

        If diff > 0 Then
            dlg.m_curFPS(2) = (dlg.m_liFreq * CLng(dlg.m_imageTimeStamps(2).Count - 1)) \ diff
        Else
            dlg.m_curFPS(2) = 0
        End If

        If liTime - dlg.m_liLastDisplayTime(2) > dlg.m_nMinInterFrameTime Then

            Dim pixelFormat__1 As Vieworks.PIXEL_FORMAT = pImageInfo.pixelFormat

            Dim bitmap As Bitmap = ImageConvert(pixelFormat__1, pImageInfo.pImage, dlg)

            Dim rtSize As New Rectangle(0, 672 \ 2, 772 \ 2, 672)

            Dim g As Graphics = dlg.ImageBox.CreateGraphics()
            g.DrawImage(bitmap, rtSize)
        End If
    End Sub

    Public Sub GetImageEvent4(pObjectInfo As IntPtr, ByRef pImageInfo As Vieworks.IMAGE_INFO)
        Dim dlg As CVwGigE_Demo_SingleCam_Window_Advance_CS = Me

        ' FPS
        Dim liTime As Long
        QueryPerformanceCounter(liTime)

        Dim i As Integer = 0
        While dlg.m_imageTimeStamps(3).Count > 30
            dlg.m_imageTimeStamps(3).RemoveAt(i)
            i += 1
        End While

        dlg.m_imageTimeStamps(3).Add(liTime)

        Dim arr As Long() = dlg.m_imageTimeStamps(3).ToArray()
        Dim diff As Long = liTime - arr(0)

        If diff > 0 Then
            dlg.m_curFPS(3) = (dlg.m_liFreq * CLng(dlg.m_imageTimeStamps(3).Count - 1)) \ diff
        Else
            dlg.m_curFPS(3) = 0
        End If

        If liTime - dlg.m_liLastDisplayTime(3) > dlg.m_nMinInterFrameTime Then

            Dim pixelFormat__1 As Vieworks.PIXEL_FORMAT = pImageInfo.pixelFormat

            Dim bitmap As Bitmap = ImageConvert(pixelFormat__1, pImageInfo.pImage, dlg)

            Dim rtSize As New Rectangle(772 \ 2, 672 \ 2, 772, 672)

            Dim g As Graphics = dlg.ImageBox.CreateGraphics()
            g.DrawImage(bitmap, rtSize)
        End If
    End Sub

    Private Function CreateBitmap(nWidth As Integer, nHeight As Integer, RawData As [Byte](), pixelFormat__1 As PixelFormat) As Bitmap
        Try
            'Bitmap Canvas = new Bitmap(nWidth, nHeight, PixelFormat.Format8bppIndexed);
            Dim Canvas As New Bitmap(nWidth, nHeight, pixelFormat__1)

            Dim CanvasData As BitmapData = Canvas.LockBits(New Rectangle(0, 0, nWidth, nHeight), ImageLockMode.[WriteOnly], pixelFormat__1)

            Dim ptr As IntPtr = CanvasData.Scan0
            Marshal.Copy(RawData, 0, ptr, RawData.Length)

            Canvas.UnlockBits(CanvasData)

            If PixelFormat.Format8bppIndexed = pixelFormat__1 Then
                SetGrayscalePalette(Canvas)
            End If

            Return Canvas
        Catch generatedExceptionName As Exception
            Return Nothing
        End Try

    End Function

    Private Shared Sub SetGrayscalePalette(Image As Bitmap)
        Dim GrayscalePalette As ColorPalette = Image.Palette

        For i As Integer = 0 To 255
            GrayscalePalette.Entries(i) = Color.FromArgb(i, i, i)
        Next

        Image.Palette = GrayscalePalette

    End Sub

    Public Sub GetDeviceInfo(nIndex As Integer, ByRef strVenderName As String, ByRef strModelName As String, ByRef strDeviceVersion As String, ByRef strDeviceID As String)
        If m_pCamera = IntPtr.Zero Then
            Return
        End If

        Const STR_SIZE As Integer = 256
        Dim btVenderName As [Byte]() = New [Byte](STR_SIZE - 1) {}
        Dim pcbVendor As IntPtr = Marshal.AllocHGlobal(4)

        Dim btModelName As [Byte]() = New [Byte](STR_SIZE - 1) {}
        Dim pcbModel As IntPtr = Marshal.AllocHGlobal(4)

        Dim btVersion As [Byte]() = New [Byte](STR_SIZE - 1) {}
        Dim pcbVersion As IntPtr = Marshal.AllocHGlobal(4)

        Dim btID As [Byte]() = New [Byte](STR_SIZE - 1) {}
        Dim pcbID As IntPtr = Marshal.AllocHGlobal(4)


        Dim nSize As Integer() = New Integer(0) {}
        If Vieworks.VwGigE.CameraGetDeviceVendorName(m_pCamera, nIndex, btVenderName, pcbVendor) = Vieworks.RESULT.RESULT_SUCCESS Then
            Marshal.Copy(pcbVendor, nSize, 0, 1)
            Dim acHashedData As Char() = New Char(nSize(0) - 1) {}

            Dim nValidCount As Integer = 0
            For i As Integer = 0 To nSize(0) - 1
                If btVenderName(i) = 0 Then
                    Exit For
                End If
                nValidCount += 1
            Next

            acHashedData = Encoding.[Default].GetChars(btVenderName, 0, nValidCount)

            Dim temp As New String(acHashedData)
            strVenderName = temp
        End If

        If Vieworks.VwGigE.CameraGetDeviceModelName(m_pCamera, nIndex, btModelName, pcbModel) = Vieworks.RESULT.RESULT_SUCCESS Then
            Marshal.Copy(pcbModel, nSize, 0, 1)
            Dim acHashedData As Char() = New Char(nSize(0) - 1) {}

            Dim nValidCount As Integer = 0
            For i As Integer = 0 To nSize(0) - 1
                If btModelName(i) = 0 Then
                    Exit For
                End If
                nValidCount += 1
            Next

            acHashedData = Encoding.[Default].GetChars(btModelName, 0, nValidCount)

            Dim temp As New String(acHashedData)
            strModelName = temp
        End If

        If Vieworks.VwGigE.CameraGetDeviceVersion(m_pCamera, nIndex, btVersion, pcbVersion) = Vieworks.RESULT.RESULT_SUCCESS Then
            Marshal.Copy(pcbVersion, nSize, 0, 1)
            Dim acHashedData As Char() = New Char(nSize(0) - 1) {}

            Dim nValidCount As Integer = 0
            For i As Integer = 0 To nSize(0) - 1
                If btVersion(i) = 0 Then
                    Exit For
                End If
                nValidCount += 1
            Next
            acHashedData = Encoding.[Default].GetChars(btVersion, 0, nValidCount)

            Dim temp As New String(acHashedData)
            strDeviceVersion = temp
        End If

        If Vieworks.VwGigE.CameraGetDeviceID(m_pCamera, nIndex, btID, pcbID) = Vieworks.RESULT.RESULT_SUCCESS Then
            Marshal.Copy(pcbID, nSize, 0, 1)
            Dim acHashedData As Char() = New Char(nSize(0) - 1) {}

            Dim nValidCount As Integer = 0
            For i As Integer = 0 To nSize(0) - 1
                If btID(i) = 0 Then
                    Exit For
                End If
                nValidCount += 1
            Next
            acHashedData = Encoding.[Default].GetChars(btID, 0, nValidCount)

            Dim temp As New String(acHashedData)
            strDeviceID = temp
        End If

        Marshal.FreeHGlobal(pcbVendor)
        Marshal.FreeHGlobal(pcbModel)
        Marshal.FreeHGlobal(pcbVersion)
        Marshal.FreeHGlobal(pcbID)
    End Sub

    Public Function GetPixelFormatFromEnum(pixelFormat As Vieworks.PIXEL_FORMAT) As String
        Dim i As Integer = 0
        For i = 0 To PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT - 1
            If PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT(i) = pixelFormat Then
                Exit For
            End If
        Next
        Return PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT(i)
    End Function

    Public Sub SetUIResolutionInfo(tempwidth As Integer, tempheight As Integer, pixelFormat As Vieworks.PIXEL_FORMAT, strPixelSize As String)
        Dim strWidth As String = [String].Format("{0}", tempwidth)
        edtWidth.Text = strWidth

        Dim strHeight As String = [String].Format("{0}", tempheight)
        edtHeight.Text = strHeight

        Dim strPixelFormat As String = "Pixel Format : "

        For i As Integer = 0 To PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT - 1
            If PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT(i) = pixelFormat Then
                strPixelFormat += PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT(i)
                cbxPixelFormat.Text = PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT(i)
                Exit For
            End If
        Next

        cbxPixelSize.Text = strPixelSize

    End Sub


    Public Function GetPixelTypeIndex(strType As String) As Integer
        Return cbxPixelFormat.Items.IndexOf(strType)
    End Function

    Private Sub btnSnap_Click(sender As Object, e As EventArgs) Handles btnSnap.Click
        If IntPtr.Zero = m_pCamera Then
            Return
        End If

        Dim bGrabbing As Boolean = False
        Vieworks.VwGigE.CameraGetGrabCondition(m_pCamera, bGrabbing)

        If bGrabbing Then
            MessageBox.Show("Now grabbing... Please 'Abort' first.")
            Return
        End If

        ' Set Width, Height
        Dim nInputWidth As Integer = Int32.Parse(edtWidth.Text)
        If False = SetWidthCamera(nInputWidth) Then
            ' Rollback
            Dim nWidth As Integer = 0
            Vieworks.VwGigE.CameraGetWidth(m_pCamera, nWidth)
            edtWidth.Text = [String].Format("{0}", nWidth)
        End If

        ' Set Width, Height
        Dim nInputHeight As Integer = Int32.Parse(edtHeight.Text)
        If False = SetHeightCamera(nInputHeight) Then
            Dim nHeight As Integer = 0
            Vieworks.VwGigE.CameraGetHeight(m_pCamera, nHeight)
            edtHeight.Text = [String].Format("{0}", nHeight)
        End If

        Dim nFrame As Integer = Int32.Parse(edtFrame.Text)

        'Exception
        If nFrame < 1 Then
            MessageBox.Show("Must be greater than 0.")
            nFrame = 1
        ElseIf nFrame > 255 Then
            MessageBox.Show("Must be less than 256.")
            nFrame = 255
        End If

        edtFrame.Text = nFrame.ToString()

        ' Success
        If Vieworks.VwGigE.CameraSnap(m_pCamera, nFrame) = Vieworks.RESULT.RESULT_SUCCESS Then
        Else
            ' Fail
            MessageBox.Show("Failed : Snap")
            Return
        End If

        ' Update resolution info.
        Dim nCurWidth As Integer = 0
        Dim nCurHeight As Integer = 0
        Dim pixelFormat As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8

        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nCurWidth)
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nCurHeight)
        Vieworks.VwGigE.CameraGetPixelFormat(m_pCamera, pixelFormat)

        Dim sFeatureName As String = "PixelSize"
        Const STR_SIZE As Integer = 256
        Dim btArg As Byte() = New Byte(255) {}
        Dim nSize As Integer = STR_SIZE
        Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))
        Dim strArg As String = ByteArrayToString(btArg, nSize)

        SetUIResolutionInfo(nCurWidth, nCurHeight, pixelFormat, strArg)

    End Sub


    Public Function SetWidthCamera(nWidth As Integer) As Boolean
        Dim nCurrWidth As Integer = 0

        If Vieworks.RESULT.RESULT_SUCCESS <> Vieworks.VwGigE.CameraGetWidth(m_pCamera, nCurrWidth) Then
            Return False
        End If

        If nWidth <> nCurrWidth Then
            Dim ret As Vieworks.RESULT = Vieworks.VwGigE.CameraSetWidth(m_pCamera, nWidth)

            If Vieworks.RESULT.RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE = ret Then
                MessageBox.Show("Error : Width must be a multiple of 4!")

                Return False
            End If
        End If

        Return True
    End Function

    Public Function SetHeightCamera(nHeight As Integer) As Boolean
        Dim nCurrHeight As Integer = 0
        If Vieworks.RESULT.RESULT_SUCCESS <> Vieworks.VwGigE.CameraGetHeight(m_pCamera, nCurrHeight) Then
            Return False
        End If

        If nHeight <> nCurrHeight Then
            Dim ret As Vieworks.RESULT = Vieworks.VwGigE.CameraSetHeight(m_pCamera, nHeight)

            If Vieworks.RESULT.RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE = ret Then
                MessageBox.Show("Error : Height must be a multiple of 2!")

                Return False
            End If
        End If

        Return True
    End Function

    Private Sub btnCloseCamera_Click(sender As Object, e As EventArgs) Handles btnCloseCamera.Click
        If IntPtr.Zero <> m_pCamera Then
            If Vieworks.VwGigE.CameraClose(m_pCamera) = Vieworks.RESULT.RESULT_SUCCESS Then
                'Success
            Else
                'Fail
            End If

            m_pCamera = IntPtr.Zero
            m_lstCamera(m_nCurrentDeviceIndex) = IntPtr.Zero

        End If

        If gchCallback.IsAllocated Then
            gchCallback.Free()
        End If
        If gchobjectInfo.IsAllocated Then
            gchobjectInfo.Free()
        End If

        btnCloseCamera.Enabled = False
        btnOpenCamera.Enabled = True
        btnAbort.Enabled = False
        edtNumBuffers.Enabled = True

        btnGrab.Enabled = False
        btnSnap.Enabled = False
        cbxPixelFormat.Enabled = False
        cbxPixelSize.Enabled = False
        edtWidth.Enabled = False
        edtHeight.Enabled = False
        edtFrame.Enabled = False

        btnDiscovery.Enabled = True
        For i As Integer = 0 To 3
            If IntPtr.Zero <> m_lstCamera(i) Then
                btnDiscovery.Enabled = False
            End If
        Next

    End Sub

    Private Sub btnAbort_Click(sender As Object, e As EventArgs) Handles btnAbort.Click
        If IntPtr.Zero = m_pCamera Then
            System.Diagnostics.Trace.WriteLine("Camera == zero")
            Return
        End If

        Vieworks.VwGigE.CameraAbort(m_pCamera)

        v_timer.Enabled = False

        btnCloseCamera.Enabled = True
        btnGrab.Enabled = True
        btnSnap.Enabled = True
        edtFrame.Enabled = True
        cbxPixelFormat.Enabled = True
        cbxPixelSize.Enabled = True
        edtWidth.Enabled = True
        edtHeight.Enabled = True
    End Sub

    Private Sub btnGrab_Click(sender As Object, e As EventArgs) Handles btnGrab.Click
        If IntPtr.Zero = m_pCamera Then
            Return
        End If

        Dim bGrabbing As Boolean = False
        Vieworks.VwGigE.CameraGetGrabCondition(m_pCamera, bGrabbing)

        If bGrabbing Then
            MessageBox.Show("Now grabbing... Please 'Abort' first.")
            Return
        End If

        Dim nWidth As Integer = 0
        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nWidth)
        Dim nHeight As Integer = 0
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nHeight)

        ' Set Width, Height
        Dim nInputWidth As Integer = Int32.Parse(edtWidth.Text)

        If False = SetWidthCamera(nInputWidth) Then
            ' Rollback
            Dim nCurWidth As Integer = 0
            Vieworks.VwGigE.CameraGetWidth(m_pCamera, nCurWidth)
            Dim strWidth As String = [String].Format("{0}", nCurWidth)
            edtWidth.Text = strWidth
        End If

        ' Set Width, Height
        Dim nInputHeight As Integer = Int32.Parse(edtHeight.Text)

        If False = SetHeightCamera(nInputHeight) Then
            ' Rollback
            Dim nCurHeight As Integer = 0
            Vieworks.VwGigE.CameraGetHeight(m_pCamera, nCurHeight)
            Dim strHeight As String = [String].Format("{0}", nCurHeight)
            edtHeight.Text = strHeight
        End If

        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nInputWidth)
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nInputHeight)

        Dim pixelFormat As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8

        Vieworks.VwGigE.CameraGetPixelFormat(m_pCamera, pixelFormat)

        If nInputWidth <> nWidth OrElse nInputHeight <> nHeight Then
            If Vieworks.RESULT.RESULT_SUCCESS <> Vieworks.VwGigE.CameraChangeBufferFormat(m_pCamera, m_imagebuffernumber, nInputWidth, nInputHeight, pixelFormat) Then
                MessageBox.Show("Can't change the camera buffer.")

                Return
            End If
        End If



        If Vieworks.VwGigE.CameraGrab(m_pCamera) = Vieworks.RESULT.RESULT_SUCCESS Then
            System.Diagnostics.Trace.WriteLine("GameraGrab")
        Else
            Return
        End If

        v_timer.Enabled = True

        ' Disable buttons
        btnCloseCamera.Enabled = False
        edtFrame.Enabled = False
        btnGrab.Enabled = False
        btnSnap.Enabled = False

        cbxPixelFormat.Enabled = False
        cbxPixelSize.Enabled = False
        edtWidth.Enabled = False
        edtHeight.Enabled = False

        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nWidth)
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nHeight)
        Vieworks.VwGigE.CameraGetPixelFormat(m_pCamera, pixelFormat)
        Dim sFeatureName As String = "PixelSize"
        Const STR_SIZE As Integer = 256
        Dim btArg As Byte() = New Byte(255) {}
        Dim nSize As Integer = STR_SIZE
        Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))
        Dim strArg As String = ByteArrayToString(btArg, nSize)

        SetUIResolutionInfo(nWidth, nHeight, pixelFormat, strArg)
    End Sub


    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Dim bEmpty As Boolean = True
        For i As Integer = 0 To 3
            If IntPtr.Zero <> m_lstCamera(i) Then
                bEmpty = False
                Exit For
            End If
        Next

        If False = bEmpty Then
            MessageBox.Show("First, Close device.")
            Return
        End If

        For i As Integer = 0 To 3
            If m_pobjectInfo(i) <> IntPtr.Zero Then
                Marshal.FreeHGlobal(m_pobjectInfo(i))
            End If
        Next

        Application.[Exit]()
    End Sub

    Private Sub PixelFormatSelChange(sender As Object, e As EventArgs) Handles cbxPixelFormat.SelectionChangeCommitted

        If IntPtr.Zero = m_pCamera Then
            Return
        End If

        Dim str As String = cbxPixelFormat.SelectedItem.ToString()
        Dim pixelFormatItem As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8
        For i As Integer = 0 To PIXEL_FORMAT_ARRAY.PIXEL_FORMAT_COUNT - 1
            If PIXEL_FORMAT_ARRAY.STR_PIXEL_FORMAT(i) = str Then
                pixelFormatItem = PIXEL_FORMAT_ARRAY.ARR_PIXEL_FORMAT(i)
                Exit For
            End If
        Next

        Dim ret As Vieworks.RESULT = Vieworks.VwGigE.CameraSetPixelFormat(m_pCamera, pixelFormatItem)

        Select Case ret
            Case Vieworks.RESULT.RESULT_SUCCESS
                Exit Select
            Case Vieworks.RESULT.RESULT_ERROR_INVALID_PARAMETER
                MessageBox.Show("Invalid pixelformat.")
                Return
            Case Else
                MessageBox.Show("Can't change the pixelformat.")
                Return
        End Select

        Dim nWidth As Integer = 0
        Dim nHeight As Integer = 0
        Dim pixelFormat As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8
        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nWidth)
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nHeight)
        Vieworks.VwGigE.CameraGetPixelFormat(m_pCamera, pixelFormat)

        If Vieworks.RESULT.RESULT_SUCCESS <> Vieworks.VwGigE.CameraChangeBufferFormat(m_pCamera, m_imagebuffernumber, nWidth, nHeight, pixelFormat) Then
            MessageBox.Show("Can't change the camera buffer.")
            Return
        End If

        Dim nPixelSize As Integer = 0
        Dim sFeatureName As String = "PixelSize"
        Const STR_SIZE As Integer = 256
        Dim btArg As Byte() = New Byte(255) {}
        Dim nSize As Integer = STR_SIZE
        Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))
        Dim strCurPixelSize As String = ByteArrayToString(btArg, nSize)
        cbxPixelSize.SelectedItem = strCurPixelSize
    End Sub

    Private Sub v_timer_Tick(sender As Object, e As EventArgs) Handles v_timer.Tick
        If m_curFPS(m_nCurrentDeviceIndex) > 0 Then
            Dim strFPS As String = [String].Format("{0:F}fps", m_curFPS(m_nCurrentDeviceIndex))
            txtFPS.Text = strFPS
        End If
    End Sub


    Private Sub btnSelectDevice_Click(sender As Object, e As EventArgs)
        If listBoxDeviceList.SelectedItem Is Nothing Then
            MessageBox.Show("Please, select device.")
            Return
        End If
        Dim m_nCurrentDeviceIndex As Integer = listBoxDeviceList.Items.IndexOf(listBoxDeviceList.SelectedItem)

        m_pCamera = m_lstCamera(m_nCurrentDeviceIndex)

        If IntPtr.Zero <> m_pCamera Then
            SaveDeviceState(m_nOldDeviceIndex)
            UpdateDeviceState(m_nCurrentDeviceIndex)
        Else
            btnOpenCamera.Enabled = True
        End If
        m_nOldDeviceIndex = m_nCurrentDeviceIndex
    End Sub

    Private Sub SaveDeviceState(nDeviceNum As Integer)
        Dim deviceState As New CDeviceState()

        deviceState.m_bOpen = btnOpenCamera.Enabled
        deviceState.m_bClose = btnCloseCamera.Enabled
        deviceState.m_bImageBuffer = edtNumBuffers.Enabled
        deviceState.m_nBuffer = Int32.Parse(edtNumBuffers.Text)

        deviceState.m_strVendorName = txtVendorName.Text
        deviceState.m_strModelName = txtModelName.Text
        deviceState.m_strDeviceVersion = txtDeviceVersion.Text
        deviceState.m_strDeviceID = txtDeviceID.Text
        deviceState.m_nSnapBuffer = Int32.Parse(edtFrame.Text)
        deviceState.m_bSnapBuffer = edtFrame.Enabled

        deviceState.m_bGrab = btnGrab.Enabled
        deviceState.m_bSnap = btnSnap.Enabled
        deviceState.m_bAbort = btnAbort.Enabled

        deviceState.m_bPixelFormat = cbxPixelFormat.Enabled
        deviceState.m_strPixelFormat = cbxPixelFormat.SelectedText
        deviceState.m_bPixelSize = cbxPixelSize.Enabled
        deviceState.m_strPixelSize = cbxPixelSize.SelectedText

        deviceState.m_bWidth = edtWidth.Enabled
        If edtWidth.Text <> "" Then
            deviceState.m_nWidth = Int32.Parse(edtWidth.Text)
        End If

        deviceState.m_bHeight = edtHeight.Enabled
        If edtHeight.Text <> "" Then
            deviceState.m_nHeight = Int32.Parse(edtHeight.Text)
        End If

        m_deviceState(nDeviceNum) = deviceState
    End Sub

    Private Sub UpdateDeviceState(nDeviceNum As Integer)
        Dim deviceState As New CDeviceState()
        deviceState = m_deviceState(nDeviceNum)

        btnOpenCamera.Enabled = deviceState.m_bOpen
        btnCloseCamera.Enabled = deviceState.m_bClose
        edtNumBuffers.Enabled = deviceState.m_bImageBuffer
        edtNumBuffers.Text = deviceState.m_nBuffer.ToString()

        txtVendorName.Text = deviceState.m_strVendorName
        txtModelName.Text = deviceState.m_strModelName
        txtDeviceVersion.Text = deviceState.m_strDeviceVersion
        txtDeviceID.Text = deviceState.m_strDeviceID
        edtFrame.Text = deviceState.m_nSnapBuffer.ToString()
        edtFrame.Enabled = deviceState.m_bSnapBuffer

        btnGrab.Enabled = deviceState.m_bGrab
        btnSnap.Enabled = deviceState.m_bSnap
        btnAbort.Enabled = deviceState.m_bAbort

        cbxPixelFormat.Enabled = deviceState.m_bPixelFormat
        cbxPixelFormat.SelectedText = deviceState.m_strPixelFormat
        cbxPixelSize.Enabled = deviceState.m_bPixelSize
        cbxPixelSize.SelectedText = deviceState.m_strPixelSize



        edtWidth.Enabled = deviceState.m_bWidth
        edtWidth.Text = deviceState.m_nWidth.ToString()

        edtHeight.Enabled = deviceState.m_bHeight
        edtHeight.Text = deviceState.m_nHeight.ToString()

        ' Update resolution info.
        Dim nWidth As Integer = 0
        Dim nHeight As Integer = 0
        Dim ePixelFormat As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8

        Vieworks.VwGigE.CameraGetWidth(m_pCamera, nWidth)
        Vieworks.VwGigE.CameraGetHeight(m_pCamera, nHeight)
        Vieworks.VwGigE.CameraGetPixelFormat(m_pCamera, ePixelFormat)
        Dim sFeatureName As String = "PixelSize"
        Const STR_SIZE As Integer = 256
        Dim btArg As Byte() = New Byte(255) {}
        Dim nSize As Integer = STR_SIZE
        Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))
        Dim strArg As String = ByteArrayToString(btArg, nSize)

        SetUIResolutionInfo(nWidth, nHeight, ePixelFormat, strArg)


    End Sub

    Private Sub listBoxDeviceList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles listBoxDeviceList.SelectedIndexChanged
        If listBoxDeviceList.SelectedItem Is Nothing Then
            'MessageBox.Show("Please, select device.");
            Return
        End If
        m_nCurrentDeviceIndex = listBoxDeviceList.Items.IndexOf(listBoxDeviceList.SelectedItem)

        If m_nCurrentDeviceIndex >= 4 Then
            MessageBox.Show("Can not use more than 5 cameras.")
        End If
        m_pCamera = m_lstCamera(m_nCurrentDeviceIndex)

        If m_nOldDeviceIndex = -1 Then
            UpdateDeviceState(m_nCurrentDeviceIndex)
        Else
            If m_nOldDeviceIndex <> m_nCurrentDeviceIndex Then
                SaveDeviceState(m_nOldDeviceIndex)
                UpdateDeviceState(m_nCurrentDeviceIndex)
            Else
            End If
        End If

        m_nOldDeviceIndex = m_nCurrentDeviceIndex

    End Sub


    Private Sub UpdatePixelFormat()
        If IntPtr.Zero = m_pCamera Then
            Return
        End If

        Dim btArgResult As Byte() = New Byte(255) {}
        Dim nArgResultSize As Integer = 256
        Dim nItemNum As Integer = 0

        Dim ret As Vieworks.RESULT = Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, "PixelFormat", btArgResult, nArgResultSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM))
        If Vieworks.RESULT.RESULT_SUCCESS = ret Then
            nItemNum = ByteArrayToInt(btArgResult, nArgResultSize)
            cbxPixelFormat.Items.Clear()

            For i As Integer = 0 To nItemNum - 1
                nArgResultSize = 256
                Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, "PixelFormat", btArgResult, nArgResultSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX) + i)
                Dim strItemName As String = ByteArrayToString(btArgResult, nArgResultSize)
                cbxPixelFormat.Items.Add(strItemName)

            Next
        End If
    End Sub

    Private Sub CVwGigE_Demo_SingleCam_Window_Advance_CS_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If IntPtr.Zero <> m_pCamera Then
            MessageBox.Show("First, Close device" & vbLf)

            e.Cancel = True
            Return
        End If

        If IntPtr.Zero <> m_pvwGigE Then
            Vieworks.VwGigE.CloseVwGigE(m_pvwGigE)
            m_pvwGigE = IntPtr.Zero
        End If
    End Sub

    Private Sub btnDiscovery_Click(sender As System.Object, e As System.EventArgs) Handles btnDiscovery.Click
        If IntPtr.Zero = m_pvwGigE Then
            MessageBox.Show("m_pvwGigE has NULL pointer.")
            Return
        End If

        Dim ret As Vieworks.RESULT = Vieworks.VwGigE.VwDiscovery(m_pvwGigE)

        If Vieworks.RESULT.RESULT_SUCCESS <> ret Then
            MessageBox.Show("Failed to discovery.")
        End If
        listBoxDeviceList.Items.Clear()


        Dim nCameraNum As Integer = 0
        ret = Vieworks.VwGigE.VwGetNumCameras(m_pvwGigE, nCameraNum)

        If Vieworks.RESULT.RESULT_SUCCESS <> ret Then
            MessageBox.Show("Failed to access camera.")
            Return
        End If

        For i As Integer = 0 To nCameraNum - 1
            Dim cameraInfoStruct As New Vieworks.CAMERA_INFO_STRUCT()

            Vieworks.VwGigE.VwDiscoveryCameraInfo(m_pvwGigE, i, cameraInfoStruct)
            listBoxDeviceList.Items.Add(cameraInfoStruct.name)
        Next
    End Sub
    Private Function GetFuncAddr(ByVal FuncName As Long) As String
        GetFuncAddr = FuncName
    End Function
    Private Sub btnOpenCamera_Click(sender As System.Object, e As System.EventArgs) Handles btnOpenCamera.Click
        Dim pCamera As HCAMERA = IntPtr.Zero

        Dim m_objectInfo As New Vieworks.OBJECT_INFO()

        m_imagebuffernumber = Int32.Parse(edtNumBuffers.Text)

        gchobjectInfo = GCHandle.Alloc(m_objectInfo)
        ' allocation

        m_pobjectInfo(m_nCurrentDeviceIndex) = Marshal.AllocHGlobal(Marshal.SizeOf(m_objectInfo))

        ' struct -> pointer
        Marshal.StructureToPtr(m_objectInfo, m_pobjectInfo(m_nCurrentDeviceIndex), True)
        GCHandle.Alloc(m_pobjectInfo)

        Dim result As Vieworks.RESULT = Vieworks.RESULT.RESULT_ERROR

        Dim pCallback As Vieworks.VwGigE.ImageCallbackFn

        Select Case m_nCurrentDeviceIndex
            Case 0
                If True Then
                    pCallback = New Vieworks.VwGigE.ImageCallbackFn(AddressOf GetImageEvent1)
                End If
                Exit Select
            Case 1
                If True Then
                    pCallback = New Vieworks.VwGigE.ImageCallbackFn(AddressOf GetImageEvent2)
                End If
                Exit Select
            Case 2
                If True Then
                    pCallback = New Vieworks.VwGigE.ImageCallbackFn(AddressOf GetImageEvent3)
                End If
                Exit Select
            Case 3
                If True Then
                    pCallback = New Vieworks.VwGigE.ImageCallbackFn(AddressOf GetImageEvent4)
                End If
                Exit Select
            Case Else
                Return

        End Select

        gchCallback = GCHandle.Alloc(pCallback)
        Dim ptrCallback As IntPtr = Marshal.GetFunctionPointerForDelegate(pCallback)

        result = Vieworks.VwGigE.VwOpenCameraByIndex(m_pvwGigE, m_nCurrentDeviceIndex, pCamera, m_imagebuffernumber, 0, 0, _
         0, m_pobjectInfo(m_nCurrentDeviceIndex), ptrCallback, IntPtr.Zero)
        If result <> Vieworks.RESULT.RESULT_SUCCESS Then
            Select Case result

                Case Vieworks.RESULT.RESULT_ERROR_DEVCREATEDATASTREAM
                    If True Then
                        MessageBox.Show("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned")
                    End If
                    Exit Select
                Case Vieworks.RESULT.RESULT_ERROR_NO_CAMERAS
                    If True Then
                        MessageBox.Show("ERROR : RESULT_ERROR_NO_CAMERAS was returned")
                        MessageBox.Show("CHECK : NIC properties")
                    End If
                    Exit Select
                Case Vieworks.RESULT.RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER
                    If True Then
                        MessageBox.Show("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned")
                        MessageBox.Show("CHECK : Zero-based camera index")
                    End If
                    Exit Select
                Case Vieworks.RESULT.RESULT_ERROR_DATASTREAM_MTU
                    If True Then
                        MessageBox.Show("ERROR : RESULT_ERROR_STREAM_MTU was returned")
                        MessageBox.Show("CHECK : Check NIC MTU")
                    End If
                    Exit Select
                Case Else
                    If True Then
                        MessageBox.Show("ERROR : Default error code returned")
                    End If
                    Exit Select
            End Select
            Return
        End If

        m_lstCamera(m_nCurrentDeviceIndex) = pCamera
        m_pCamera = pCamera

        ' Get device information
        Dim strVendorName As String = ""
        Dim strModelName As String = ""
        Dim strVersion As String = ""
        Dim strID As String = ""

        GetDeviceInfo(m_nCurrentDeviceIndex, strVendorName, strModelName, strVersion, strID)

        'Get image width,height 
        Dim nWidth As Integer = 0
        Dim nHeight As Integer = 0
        Dim pixelFormat As Vieworks.PIXEL_FORMAT = Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8

        Vieworks.VwGigE.CameraGetWidth(pCamera, nWidth)
        Vieworks.VwGigE.CameraGetHeight(pCamera, nHeight)

        Dim plstPixelFormat As New List(Of Vieworks.PIXEL_FORMAT)()
        Dim btArgResult As Byte() = New Byte(255) {}
        Dim nArgResultSize As Integer = 256
        result = Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, "PixelSize", btArgResult, nArgResultSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM))

        If result = Vieworks.RESULT.RESULT_SUCCESS Then

            Dim nPixelSizeNum As Integer = ByteArrayToInt(btArgResult, nArgResultSize)

            For i As Integer = 0 To nPixelSizeNum - 1
                nArgResultSize = 256
                Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, "PixelSize", btArgResult, nArgResultSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX) + i)
                Dim strPixelSize As String = ByteArrayToString(btArgResult, nArgResultSize)
                cbxPixelSize.Items.Add(strPixelSize)
            Next
        End If


        Dim nPixelFormatIndex As Integer = 0
        Dim nCount As Integer = 0
        cbxPixelFormat.Items.Clear()

        For Each s As Vieworks.PIXEL_FORMAT In plstPixelFormat
            Dim strTemp As String = ""
            strTemp = GetPixelFormatFromEnum(s)
            cbxPixelFormat.Items.Add(strTemp)
        Next

        Vieworks.VwGigE.CameraGetPixelFormat(pCamera, pixelFormat)

        ' Set resolution info.
        Dim tempwidth As Integer = 0
        Dim tempheight As Integer = 0
        Vieworks.VwGigE.CameraGetWidth(pCamera, tempwidth)
        Vieworks.VwGigE.CameraGetHeight(pCamera, tempheight)

        Dim sFeatureName As String = "PixelSize"
        Const STR_SIZE As Integer = 256
        Dim btArg As Byte() = New Byte(255) {}
        Dim nSize As Integer = STR_SIZE
        Vieworks.VwGigE.CameraGetCustomCommand(m_pCamera, sFeatureName.ToCharArray(), btArg, nSize, CInt(Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE))
        Dim strArg As String = ByteArrayToString(btArg, nSize)

        SetUIResolutionInfo(tempwidth, tempheight, pixelFormat, strArg)
        UpdatePixelFormat()


        txtVendorName.Text = strVendorName
        txtModelName.Text = strModelName
        txtDeviceVersion.Text = strVersion
        txtDeviceID.Text = strID

        btnCloseCamera.Enabled = True
        btnOpenCamera.Enabled = False
        btnAbort.Enabled = True
        edtNumBuffers.Enabled = False

        btnGrab.Enabled = True
        btnSnap.Enabled = True
        cbxPixelFormat.Enabled = True
        cbxPixelSize.Enabled = True
        edtWidth.Enabled = True
        edtHeight.Enabled = True
        edtFrame.Enabled = True
        btnDiscovery.Enabled = False

    End Sub
End Class

Public Class CDeviceState
    Public Sub New()
        m_bOpen = True
        m_bClose = False
        m_bImageBuffer = True
        m_nBuffer = 2
        m_bGrab = False
        m_nSnapBuffer = 2
        m_bSnapBuffer = False
        m_bSnap = False
        m_bAbort = False
        m_bPixelFormat = False
        m_bPixelSize = False
        m_nWidth = 0
        m_bWidth = False
        m_nHeight = 0
        m_bHeight = False
    End Sub
    Public m_bOpen As Boolean
    Public m_bClose As Boolean
    Public m_bImageBuffer As Boolean
    Public m_nBuffer As Integer
    Public m_strVendorName As String
    Public m_strModelName As String
    Public m_strDeviceVersion As String
    Public m_strDeviceID As String
    Public m_bGrab As Boolean
    Public m_nSnapBuffer As Integer
    Public m_bSnapBuffer As Boolean
    Public m_bSnap As Boolean
    Public m_bAbort As Boolean
    Public m_strPixelFormat As String
    Public m_bPixelFormat As Boolean
    Public m_strPixelSize As String
    Public m_bPixelSize As Boolean
    Public m_nWidth As Integer
    Public m_bWidth As Boolean
    Public m_nHeight As Integer
    Public m_bHeight As Boolean

End Class

Public NotInheritable Class PIXEL_FORMAT_ARRAY
    Private Sub New()
    End Sub

    Public Shared PIXEL_FORMAT_COUNT As Integer = 44

    Public Shared STR_PIXEL_FORMAT As String() = {"Mono8",
    "Mono8signed",
    "Mono10",
    "Mono10Packed",
    "Mono12",
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
    "YUV422Packed",
    "YUV422YUVYPacked",
    "YUV42210Packed",
    "YUV42212Packed",
    "YUV411",
    "YUV41110Packed",
    "YUV41112Packed",
    "BGR10V1Packed",
    "BGR10V2Packed",
    "RGB12Packed",
    "BGR12Packed",
    "YUV444",
    "PALInterlaced",
    "NTSCInterlaced"}


    Public Shared ARR_PIXEL_FORMAT As Vieworks.PIXEL_FORMAT() = {
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO8_SIGNED,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO10_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO12_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO14,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_MONO16,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGB8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYBG8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGB10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYBG10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR10_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG10_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGB12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYBG12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYRG12_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BAYGR12_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR8,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_UYVY,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_YUYV,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_10_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV422_12_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV411,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV411_10_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV411_12_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10V1_PACKED,
     Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR10V2_PACKED,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_RGB12_PACKED,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_BGR12_PACKED,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_YUV444,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_PAL_INTERLACED,
    Vieworks.PIXEL_FORMAT.PIXEL_FORMAT_NTSC_INTERLACED
                                                                                              }


End Class
'End Namespace


Public Class VwGigE_Demo_MultiCam_Window_Advance_VB

End Class
