<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CVwGigE_Demo_SingleCam_Window_Advance_CS
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.btnSnap = New System.Windows.Forms.Button()
        Me.groupBox3 = New System.Windows.Forms.GroupBox()
        Me.lbFPS = New System.Windows.Forms.Label()
        Me.cbxPixelSize = New System.Windows.Forms.ComboBox()
        Me.label6 = New System.Windows.Forms.Label()
        Me.txtFPS = New System.Windows.Forms.Label()
        Me.edtHeight = New System.Windows.Forms.TextBox()
        Me.edtWidth = New System.Windows.Forms.TextBox()
        Me.label13 = New System.Windows.Forms.Label()
        Me.label12 = New System.Windows.Forms.Label()
        Me.cbxPixelFormat = New System.Windows.Forms.ComboBox()
        Me.label11 = New System.Windows.Forms.Label()
        Me.btnAbort = New System.Windows.Forms.Button()
        Me.btnExit = New System.Windows.Forms.Button()
        Me.v_timer = New System.Windows.Forms.Timer(Me.components)
        Me.listBoxDeviceList = New System.Windows.Forms.ListBox()
        Me.btnDiscovery = New System.Windows.Forms.Button()
        Me.btnGrab = New System.Windows.Forms.Button()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtDeviceID = New System.Windows.Forms.Label()
        Me.txtDeviceVersion = New System.Windows.Forms.Label()
        Me.txtModelName = New System.Windows.Forms.Label()
        Me.txtVendorName = New System.Windows.Forms.Label()
        Me.label5 = New System.Windows.Forms.Label()
        Me.label4 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.label2 = New System.Windows.Forms.Label()
        Me.edtNumBuffers = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.btnCloseCamera = New System.Windows.Forms.Button()
        Me.btnOpenCamera = New System.Windows.Forms.Button()
        Me.ImageBox = New System.Windows.Forms.PictureBox()
        Me.groupBox2 = New System.Windows.Forms.GroupBox()
        Me.label10 = New System.Windows.Forms.Label()
        Me.edtFrame = New System.Windows.Forms.TextBox()
        Me.gbDeviceList = New System.Windows.Forms.GroupBox()
        Me.groupBox3.SuspendLayout()
        Me.groupBox1.SuspendLayout()
        CType(Me.ImageBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.groupBox2.SuspendLayout()
        Me.gbDeviceList.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSnap
        '
        Me.btnSnap.Location = New System.Drawing.Point(161, 64)
        Me.btnSnap.Name = "btnSnap"
        Me.btnSnap.Size = New System.Drawing.Size(116, 27)
        Me.btnSnap.TabIndex = 3
        Me.btnSnap.Text = "Snap"
        Me.btnSnap.UseVisualStyleBackColor = True
        '
        'groupBox3
        '
        Me.groupBox3.Controls.Add(Me.lbFPS)
        Me.groupBox3.Controls.Add(Me.cbxPixelSize)
        Me.groupBox3.Controls.Add(Me.label6)
        Me.groupBox3.Controls.Add(Me.txtFPS)
        Me.groupBox3.Controls.Add(Me.edtHeight)
        Me.groupBox3.Controls.Add(Me.edtWidth)
        Me.groupBox3.Controls.Add(Me.label13)
        Me.groupBox3.Controls.Add(Me.label12)
        Me.groupBox3.Controls.Add(Me.cbxPixelFormat)
        Me.groupBox3.Controls.Add(Me.label11)
        Me.groupBox3.Location = New System.Drawing.Point(918, 473)
        Me.groupBox3.Name = "groupBox3"
        Me.groupBox3.Size = New System.Drawing.Size(295, 189)
        Me.groupBox3.TabIndex = 14
        Me.groupBox3.TabStop = False
        Me.groupBox3.Text = "Image information"
        '
        'lbFPS
        '
        Me.lbFPS.AutoSize = True
        Me.lbFPS.Location = New System.Drawing.Point(18, 152)
        Me.lbFPS.Name = "lbFPS"
        Me.lbFPS.Size = New System.Drawing.Size(28, 12)
        Me.lbFPS.TabIndex = 8
        Me.lbFPS.Text = "FPS"
        '
        'cbxPixelSize
        '
        Me.cbxPixelSize.FormattingEnabled = True
        Me.cbxPixelSize.Location = New System.Drawing.Point(106, 59)
        Me.cbxPixelSize.Name = "cbxPixelSize"
        Me.cbxPixelSize.Size = New System.Drawing.Size(130, 20)
        Me.cbxPixelSize.TabIndex = 7
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(18, 62)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(61, 12)
        Me.label6.TabIndex = 6
        Me.label6.Text = "Pixel size"
        '
        'txtFPS
        '
        Me.txtFPS.AutoSize = True
        Me.txtFPS.Location = New System.Drawing.Point(106, 152)
        Me.txtFPS.Name = "txtFPS"
        Me.txtFPS.Size = New System.Drawing.Size(0, 12)
        Me.txtFPS.TabIndex = 1
        '
        'edtHeight
        '
        Me.edtHeight.Location = New System.Drawing.Point(106, 121)
        Me.edtHeight.Name = "edtHeight"
        Me.edtHeight.Size = New System.Drawing.Size(80, 21)
        Me.edtHeight.TabIndex = 5
        '
        'edtWidth
        '
        Me.edtWidth.Location = New System.Drawing.Point(106, 91)
        Me.edtWidth.Name = "edtWidth"
        Me.edtWidth.Size = New System.Drawing.Size(80, 21)
        Me.edtWidth.TabIndex = 4
        '
        'label13
        '
        Me.label13.AutoSize = True
        Me.label13.Location = New System.Drawing.Point(18, 124)
        Me.label13.Name = "label13"
        Me.label13.Size = New System.Drawing.Size(40, 12)
        Me.label13.TabIndex = 3
        Me.label13.Text = "Height"
        '
        'label12
        '
        Me.label12.AutoSize = True
        Me.label12.Location = New System.Drawing.Point(18, 94)
        Me.label12.Name = "label12"
        Me.label12.Size = New System.Drawing.Size(35, 12)
        Me.label12.TabIndex = 2
        Me.label12.Text = "Width"
        '
        'cbxPixelFormat
        '
        Me.cbxPixelFormat.FormattingEnabled = True
        Me.cbxPixelFormat.Location = New System.Drawing.Point(106, 24)
        Me.cbxPixelFormat.Name = "cbxPixelFormat"
        Me.cbxPixelFormat.Size = New System.Drawing.Size(131, 20)
        Me.cbxPixelFormat.TabIndex = 1
        '
        'label11
        '
        Me.label11.AutoSize = True
        Me.label11.Location = New System.Drawing.Point(18, 27)
        Me.label11.Name = "label11"
        Me.label11.Size = New System.Drawing.Size(72, 12)
        Me.label11.TabIndex = 0
        Me.label11.Text = "Pixel format"
        '
        'btnAbort
        '
        Me.btnAbort.Location = New System.Drawing.Point(160, 24)
        Me.btnAbort.Name = "btnAbort"
        Me.btnAbort.Size = New System.Drawing.Size(116, 26)
        Me.btnAbort.TabIndex = 4
        Me.btnAbort.Text = "Abort"
        Me.btnAbort.UseVisualStyleBackColor = True
        '
        'btnExit
        '
        Me.btnExit.Location = New System.Drawing.Point(1057, 668)
        Me.btnExit.Name = "btnExit"
        Me.btnExit.Size = New System.Drawing.Size(157, 29)
        Me.btnExit.TabIndex = 15
        Me.btnExit.Text = "Exit program"
        Me.btnExit.UseVisualStyleBackColor = True
        '
        'v_timer
        '
        Me.v_timer.Interval = 1000
        '
        'listBoxDeviceList
        '
        Me.listBoxDeviceList.FormattingEnabled = True
        Me.listBoxDeviceList.ItemHeight = 12
        Me.listBoxDeviceList.Location = New System.Drawing.Point(12, 20)
        Me.listBoxDeviceList.Name = "listBoxDeviceList"
        Me.listBoxDeviceList.Size = New System.Drawing.Size(270, 76)
        Me.listBoxDeviceList.TabIndex = 9
        '
        'btnDiscovery
        '
        Me.btnDiscovery.Location = New System.Drawing.Point(12, 102)
        Me.btnDiscovery.Name = "btnDiscovery"
        Me.btnDiscovery.Size = New System.Drawing.Size(270, 27)
        Me.btnDiscovery.TabIndex = 10
        Me.btnDiscovery.Text = "Discovery"
        Me.btnDiscovery.UseVisualStyleBackColor = True
        '
        'btnGrab
        '
        Me.btnGrab.Location = New System.Drawing.Point(17, 23)
        Me.btnGrab.Name = "btnGrab"
        Me.btnGrab.Size = New System.Drawing.Size(117, 27)
        Me.btnGrab.TabIndex = 2
        Me.btnGrab.Text = "Continuous grab"
        Me.btnGrab.UseVisualStyleBackColor = True
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.txtDeviceID)
        Me.groupBox1.Controls.Add(Me.txtDeviceVersion)
        Me.groupBox1.Controls.Add(Me.txtModelName)
        Me.groupBox1.Controls.Add(Me.txtVendorName)
        Me.groupBox1.Controls.Add(Me.label5)
        Me.groupBox1.Controls.Add(Me.label4)
        Me.groupBox1.Controls.Add(Me.label3)
        Me.groupBox1.Controls.Add(Me.label2)
        Me.groupBox1.Controls.Add(Me.edtNumBuffers)
        Me.groupBox1.Controls.Add(Me.label1)
        Me.groupBox1.Controls.Add(Me.btnCloseCamera)
        Me.groupBox1.Controls.Add(Me.btnOpenCamera)
        Me.groupBox1.Location = New System.Drawing.Point(918, 167)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(296, 186)
        Me.groupBox1.TabIndex = 12
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "Device"
        '
        'txtDeviceID
        '
        Me.txtDeviceID.AutoSize = True
        Me.txtDeviceID.Location = New System.Drawing.Point(113, 158)
        Me.txtDeviceID.Name = "txtDeviceID"
        Me.txtDeviceID.Size = New System.Drawing.Size(0, 12)
        Me.txtDeviceID.TabIndex = 11
        '
        'txtDeviceVersion
        '
        Me.txtDeviceVersion.AutoSize = True
        Me.txtDeviceVersion.Location = New System.Drawing.Point(113, 136)
        Me.txtDeviceVersion.Name = "txtDeviceVersion"
        Me.txtDeviceVersion.Size = New System.Drawing.Size(0, 12)
        Me.txtDeviceVersion.TabIndex = 10
        '
        'txtModelName
        '
        Me.txtModelName.AutoSize = True
        Me.txtModelName.Location = New System.Drawing.Point(113, 112)
        Me.txtModelName.Name = "txtModelName"
        Me.txtModelName.Size = New System.Drawing.Size(0, 12)
        Me.txtModelName.TabIndex = 9
        '
        'txtVendorName
        '
        Me.txtVendorName.AutoSize = True
        Me.txtVendorName.Location = New System.Drawing.Point(113, 88)
        Me.txtVendorName.Name = "txtVendorName"
        Me.txtVendorName.Size = New System.Drawing.Size(0, 12)
        Me.txtVendorName.TabIndex = 8
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(12, 158)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(66, 12)
        Me.label5.TabIndex = 7
        Me.label5.Text = "Device ID :"
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(12, 136)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(96, 12)
        Me.label4.TabIndex = 6
        Me.label4.Text = "Device version :"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(12, 112)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(84, 12)
        Me.label3.TabIndex = 5
        Me.label3.Text = "Model name :"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(12, 88)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(89, 12)
        Me.label2.TabIndex = 4
        Me.label2.Text = "Vendor name :"
        '
        'edtNumBuffers
        '
        Me.edtNumBuffers.Location = New System.Drawing.Point(151, 60)
        Me.edtNumBuffers.Name = "edtNumBuffers"
        Me.edtNumBuffers.Size = New System.Drawing.Size(67, 21)
        Me.edtNumBuffers.TabIndex = 3
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(10, 63)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(131, 12)
        Me.label1.TabIndex = 2
        Me.label1.Text = "The number of Buffers"
        '
        'btnCloseCamera
        '
        Me.btnCloseCamera.Location = New System.Drawing.Point(156, 22)
        Me.btnCloseCamera.Name = "btnCloseCamera"
        Me.btnCloseCamera.Size = New System.Drawing.Size(125, 26)
        Me.btnCloseCamera.TabIndex = 1
        Me.btnCloseCamera.Text = "Close camera"
        Me.btnCloseCamera.UseVisualStyleBackColor = True
        '
        'btnOpenCamera
        '
        Me.btnOpenCamera.Location = New System.Drawing.Point(12, 22)
        Me.btnOpenCamera.Name = "btnOpenCamera"
        Me.btnOpenCamera.Size = New System.Drawing.Size(129, 25)
        Me.btnOpenCamera.TabIndex = 0
        Me.btnOpenCamera.Text = "Open camera"
        Me.btnOpenCamera.UseVisualStyleBackColor = True
        '
        'ImageBox
        '
        Me.ImageBox.Location = New System.Drawing.Point(12, 12)
        Me.ImageBox.Name = "ImageBox"
        Me.ImageBox.Size = New System.Drawing.Size(772, 672)
        Me.ImageBox.TabIndex = 11
        Me.ImageBox.TabStop = False
        '
        'groupBox2
        '
        Me.groupBox2.Controls.Add(Me.btnAbort)
        Me.groupBox2.Controls.Add(Me.btnSnap)
        Me.groupBox2.Controls.Add(Me.btnGrab)
        Me.groupBox2.Controls.Add(Me.label10)
        Me.groupBox2.Controls.Add(Me.edtFrame)
        Me.groupBox2.Location = New System.Drawing.Point(918, 359)
        Me.groupBox2.Name = "groupBox2"
        Me.groupBox2.Size = New System.Drawing.Size(295, 108)
        Me.groupBox2.TabIndex = 13
        Me.groupBox2.TabStop = False
        Me.groupBox2.Text = "Device control"
        '
        'label10
        '
        Me.label10.AutoSize = True
        Me.label10.Location = New System.Drawing.Point(90, 71)
        Me.label10.Name = "label10"
        Me.label10.Size = New System.Drawing.Size(44, 12)
        Me.label10.TabIndex = 1
        Me.label10.Text = "frames"
        '
        'edtFrame
        '
        Me.edtFrame.Location = New System.Drawing.Point(19, 68)
        Me.edtFrame.Name = "edtFrame"
        Me.edtFrame.Size = New System.Drawing.Size(54, 21)
        Me.edtFrame.TabIndex = 0
        '
        'gbDeviceList
        '
        Me.gbDeviceList.Controls.Add(Me.btnDiscovery)
        Me.gbDeviceList.Controls.Add(Me.listBoxDeviceList)
        Me.gbDeviceList.Location = New System.Drawing.Point(918, 12)
        Me.gbDeviceList.Name = "gbDeviceList"
        Me.gbDeviceList.Size = New System.Drawing.Size(295, 149)
        Me.gbDeviceList.TabIndex = 16
        Me.gbDeviceList.TabStop = False
        Me.gbDeviceList.Text = "Device list"
        '
        'CVwGigE_Demo_SingleCam_Window_Advance_CS
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1230, 710)
        Me.Controls.Add(Me.groupBox3)
        Me.Controls.Add(Me.btnExit)
        Me.Controls.Add(Me.groupBox1)
        Me.Controls.Add(Me.ImageBox)
        Me.Controls.Add(Me.groupBox2)
        Me.Controls.Add(Me.gbDeviceList)
        Me.Name = "CVwGigE_Demo_SingleCam_Window_Advance_CS"
        Me.Text = "CVwGigE.Demo.SingleCam.Window.Advance.VB"
        Me.groupBox3.ResumeLayout(False)
        Me.groupBox3.PerformLayout()
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        CType(Me.ImageBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.groupBox2.ResumeLayout(False)
        Me.groupBox2.PerformLayout()
        Me.gbDeviceList.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents btnSnap As System.Windows.Forms.Button
    Private WithEvents groupBox3 As System.Windows.Forms.GroupBox
    Private WithEvents lbFPS As System.Windows.Forms.Label
    Private WithEvents cbxPixelSize As System.Windows.Forms.ComboBox
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents txtFPS As System.Windows.Forms.Label
    Private WithEvents edtHeight As System.Windows.Forms.TextBox
    Private WithEvents edtWidth As System.Windows.Forms.TextBox
    Private WithEvents label13 As System.Windows.Forms.Label
    Private WithEvents label12 As System.Windows.Forms.Label
    Private WithEvents cbxPixelFormat As System.Windows.Forms.ComboBox
    Private WithEvents label11 As System.Windows.Forms.Label
    Private WithEvents btnAbort As System.Windows.Forms.Button
    Private WithEvents btnExit As System.Windows.Forms.Button
    Private WithEvents v_timer As System.Windows.Forms.Timer
    Private WithEvents listBoxDeviceList As System.Windows.Forms.ListBox
    Private WithEvents btnDiscovery As System.Windows.Forms.Button
    Private WithEvents btnGrab As System.Windows.Forms.Button
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents txtDeviceID As System.Windows.Forms.Label
    Private WithEvents txtDeviceVersion As System.Windows.Forms.Label
    Private WithEvents txtModelName As System.Windows.Forms.Label
    Private WithEvents txtVendorName As System.Windows.Forms.Label
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents edtNumBuffers As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents btnCloseCamera As System.Windows.Forms.Button
    Private WithEvents btnOpenCamera As System.Windows.Forms.Button
    Public WithEvents ImageBox As System.Windows.Forms.PictureBox
    Private WithEvents groupBox2 As System.Windows.Forms.GroupBox
    Private WithEvents label10 As System.Windows.Forms.Label
    Private WithEvents edtFrame As System.Windows.Forms.TextBox
    Private WithEvents gbDeviceList As System.Windows.Forms.GroupBox

End Class
