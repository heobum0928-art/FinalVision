namespace VwUSB.Demo.MultiCam.Window.Advance.CS
{
    partial class CVwUSB_Demo_MultiCam_Window_Advance_CS
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ImageBox = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDeviceID = new System.Windows.Forms.Label();
            this.txtDeviceVersion = new System.Windows.Forms.Label();
            this.txtModelName = new System.Windows.Forms.Label();
            this.txtVendorName = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.edtNumBuffers = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCloseCamera = new System.Windows.Forms.Button();
            this.btnOpenCamera = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnSnap = new System.Windows.Forms.Button();
            this.btnGrab = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.edtFrame = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbFPS = new System.Windows.Forms.Label();
            this.cbxTestPattern = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFPS = new System.Windows.Forms.Label();
            this.edtHeight = new System.Windows.Forms.TextBox();
            this.edtWidth = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.cbxPixelFormat = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.v_timer = new System.Windows.Forms.Timer(this.components);
            this.listBoxDeviceList = new System.Windows.Forms.ListBox();
            this.gbDeviceList = new System.Windows.Forms.GroupBox();
            this.btnDiscovery = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbDeviceList.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageBox
            // 
            this.ImageBox.Location = new System.Drawing.Point(12, 12);
            this.ImageBox.Name = "ImageBox";
            this.ImageBox.Size = new System.Drawing.Size(772, 672);
            this.ImageBox.TabIndex = 3;
            this.ImageBox.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDeviceID);
            this.groupBox1.Controls.Add(this.txtDeviceVersion);
            this.groupBox1.Controls.Add(this.txtModelName);
            this.groupBox1.Controls.Add(this.txtVendorName);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.edtNumBuffers);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnCloseCamera);
            this.groupBox1.Controls.Add(this.btnOpenCamera);
            this.groupBox1.Location = new System.Drawing.Point(918, 167);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 186);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Device";
            // 
            // txtDeviceID
            // 
            this.txtDeviceID.AutoSize = true;
            this.txtDeviceID.Location = new System.Drawing.Point(113, 158);
            this.txtDeviceID.Name = "txtDeviceID";
            this.txtDeviceID.Size = new System.Drawing.Size(0, 12);
            this.txtDeviceID.TabIndex = 11;
            // 
            // txtDeviceVersion
            // 
            this.txtDeviceVersion.AutoSize = true;
            this.txtDeviceVersion.Location = new System.Drawing.Point(113, 136);
            this.txtDeviceVersion.Name = "txtDeviceVersion";
            this.txtDeviceVersion.Size = new System.Drawing.Size(0, 12);
            this.txtDeviceVersion.TabIndex = 10;
            // 
            // txtModelName
            // 
            this.txtModelName.AutoSize = true;
            this.txtModelName.Location = new System.Drawing.Point(113, 112);
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(0, 12);
            this.txtModelName.TabIndex = 9;
            // 
            // txtVendorName
            // 
            this.txtVendorName.AutoSize = true;
            this.txtVendorName.Location = new System.Drawing.Point(113, 88);
            this.txtVendorName.Name = "txtVendorName";
            this.txtVendorName.Size = new System.Drawing.Size(0, 12);
            this.txtVendorName.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "Device ID :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "Device version :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "Model name :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Vendor name :";
            // 
            // edtNumBuffers
            // 
            this.edtNumBuffers.Location = new System.Drawing.Point(151, 60);
            this.edtNumBuffers.Name = "edtNumBuffers";
            this.edtNumBuffers.Size = new System.Drawing.Size(67, 21);
            this.edtNumBuffers.TabIndex = 3;
            this.edtNumBuffers.TextChanged += new System.EventHandler(this.edtNumBuffers_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "The number of Buffers";
            // 
            // btnCloseCamera
            // 
            this.btnCloseCamera.Location = new System.Drawing.Point(156, 22);
            this.btnCloseCamera.Name = "btnCloseCamera";
            this.btnCloseCamera.Size = new System.Drawing.Size(125, 26);
            this.btnCloseCamera.TabIndex = 1;
            this.btnCloseCamera.Text = "Close camera";
            this.btnCloseCamera.UseVisualStyleBackColor = true;
            this.btnCloseCamera.Click += new System.EventHandler(this.btnCloseCamera_Click);
            // 
            // btnOpenCamera
            // 
            this.btnOpenCamera.Location = new System.Drawing.Point(12, 22);
            this.btnOpenCamera.Name = "btnOpenCamera";
            this.btnOpenCamera.Size = new System.Drawing.Size(129, 25);
            this.btnOpenCamera.TabIndex = 0;
            this.btnOpenCamera.Text = "Open camera";
            this.btnOpenCamera.UseVisualStyleBackColor = true;
            this.btnOpenCamera.Click += new System.EventHandler(this.btnOpenCamera_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnAbort);
            this.groupBox2.Controls.Add(this.btnSnap);
            this.groupBox2.Controls.Add(this.btnGrab);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.edtFrame);
            this.groupBox2.Location = new System.Drawing.Point(918, 359);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(295, 108);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Device control";
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(160, 24);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(116, 26);
            this.btnAbort.TabIndex = 4;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnSnap
            // 
            this.btnSnap.Location = new System.Drawing.Point(161, 64);
            this.btnSnap.Name = "btnSnap";
            this.btnSnap.Size = new System.Drawing.Size(116, 27);
            this.btnSnap.TabIndex = 3;
            this.btnSnap.Text = "Snap";
            this.btnSnap.UseVisualStyleBackColor = true;
            this.btnSnap.Click += new System.EventHandler(this.btnSnap_Click);
            // 
            // btnGrab
            // 
            this.btnGrab.Location = new System.Drawing.Point(17, 23);
            this.btnGrab.Name = "btnGrab";
            this.btnGrab.Size = new System.Drawing.Size(117, 27);
            this.btnGrab.TabIndex = 2;
            this.btnGrab.Text = "Continuous grab";
            this.btnGrab.UseVisualStyleBackColor = true;
            this.btnGrab.Click += new System.EventHandler(this.btnGrab_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(90, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(44, 12);
            this.label10.TabIndex = 1;
            this.label10.Text = "frames";
            // 
            // edtFrame
            // 
            this.edtFrame.Location = new System.Drawing.Point(19, 68);
            this.edtFrame.Name = "edtFrame";
            this.edtFrame.Size = new System.Drawing.Size(54, 21);
            this.edtFrame.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lbFPS);
            this.groupBox3.Controls.Add(this.cbxTestPattern);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtFPS);
            this.groupBox3.Controls.Add(this.edtHeight);
            this.groupBox3.Controls.Add(this.edtWidth);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.cbxPixelFormat);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Location = new System.Drawing.Point(918, 473);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(295, 189);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Image information";
            // 
            // lbFPS
            // 
            this.lbFPS.AutoSize = true;
            this.lbFPS.Location = new System.Drawing.Point(18, 152);
            this.lbFPS.Name = "lbFPS";
            this.lbFPS.Size = new System.Drawing.Size(28, 12);
            this.lbFPS.TabIndex = 8;
            this.lbFPS.Text = "FPS";
            // 
            // cbxTestPattern
            // 
            this.cbxTestPattern.FormattingEnabled = true;
            this.cbxTestPattern.Location = new System.Drawing.Point(106, 59);
            this.cbxTestPattern.Name = "cbxTestPattern";
            this.cbxTestPattern.Size = new System.Drawing.Size(130, 20);
            this.cbxTestPattern.TabIndex = 7;
            this.cbxTestPattern.SelectedIndexChanged += new System.EventHandler(this.cbxTestPattern_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "Test Pattern";
            // 
            // txtFPS
            // 
            this.txtFPS.AutoSize = true;
            this.txtFPS.Location = new System.Drawing.Point(106, 152);
            this.txtFPS.Name = "txtFPS";
            this.txtFPS.Size = new System.Drawing.Size(0, 12);
            this.txtFPS.TabIndex = 1;
            // 
            // edtHeight
            // 
            this.edtHeight.Location = new System.Drawing.Point(106, 121);
            this.edtHeight.Name = "edtHeight";
            this.edtHeight.Size = new System.Drawing.Size(80, 21);
            this.edtHeight.TabIndex = 5;
            // 
            // edtWidth
            // 
            this.edtWidth.Location = new System.Drawing.Point(106, 91);
            this.edtWidth.Name = "edtWidth";
            this.edtWidth.Size = new System.Drawing.Size(80, 21);
            this.edtWidth.TabIndex = 4;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(18, 124);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(40, 12);
            this.label13.TabIndex = 3;
            this.label13.Text = "Height";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(18, 94);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 12);
            this.label12.TabIndex = 2;
            this.label12.Text = "Width";
            // 
            // cbxPixelFormat
            // 
            this.cbxPixelFormat.FormattingEnabled = true;
            this.cbxPixelFormat.Location = new System.Drawing.Point(106, 24);
            this.cbxPixelFormat.Name = "cbxPixelFormat";
            this.cbxPixelFormat.Size = new System.Drawing.Size(131, 20);
            this.cbxPixelFormat.TabIndex = 1;
            this.cbxPixelFormat.SelectionChangeCommitted += new System.EventHandler(this.PixelFormatSelChange);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(18, 27);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 12);
            this.label11.TabIndex = 0;
            this.label11.Text = "Pixel format";
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(1057, 668);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(157, 29);
            this.btnExit.TabIndex = 7;
            this.btnExit.Text = "Exit program";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // v_timer
            // 
            this.v_timer.Interval = 1000;
            this.v_timer.Tick += new System.EventHandler(this.v_timer_Tick);
            // 
            // listBoxDeviceList
            // 
            this.listBoxDeviceList.FormattingEnabled = true;
            this.listBoxDeviceList.ItemHeight = 12;
            this.listBoxDeviceList.Location = new System.Drawing.Point(12, 20);
            this.listBoxDeviceList.Name = "listBoxDeviceList";
            this.listBoxDeviceList.Size = new System.Drawing.Size(270, 76);
            this.listBoxDeviceList.TabIndex = 9;
            this.listBoxDeviceList.SelectedIndexChanged += new System.EventHandler(this.listBoxDeviceList_SelectedIndexChanged);
            // 
            // gbDeviceList
            // 
            this.gbDeviceList.Controls.Add(this.btnDiscovery);
            this.gbDeviceList.Controls.Add(this.listBoxDeviceList);
            this.gbDeviceList.Location = new System.Drawing.Point(918, 12);
            this.gbDeviceList.Name = "gbDeviceList";
            this.gbDeviceList.Size = new System.Drawing.Size(295, 149);
            this.gbDeviceList.TabIndex = 10;
            this.gbDeviceList.TabStop = false;
            this.gbDeviceList.Text = "Device list";
            // 
            // btnDiscovery
            // 
            this.btnDiscovery.Location = new System.Drawing.Point(12, 102);
            this.btnDiscovery.Name = "btnDiscovery";
            this.btnDiscovery.Size = new System.Drawing.Size(270, 27);
            this.btnDiscovery.TabIndex = 10;
            this.btnDiscovery.Text = "Discovery";
            this.btnDiscovery.UseVisualStyleBackColor = true;
            this.btnDiscovery.Click += new System.EventHandler(this.btnDiscovery_Click);
            // 
            // CVwUSB_Demo_MultiCam_Window_Advance_CS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1224, 707);
            this.Controls.Add(this.gbDeviceList);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ImageBox);
            this.Name = "CVwUSB_Demo_MultiCam_Window_Advance_CS";
            this.Text = "VwUSB.Demo.MultiCam.Window.Advance.CS";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CVwUSB_Demo_MultiCam_Window_Advance_CS_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.gbDeviceList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox ImageBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label txtDeviceID;
        private System.Windows.Forms.Label txtDeviceVersion;
        private System.Windows.Forms.Label txtModelName;
        private System.Windows.Forms.Label txtVendorName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox edtNumBuffers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCloseCamera;
        private System.Windows.Forms.Button btnOpenCamera;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnSnap;
        private System.Windows.Forms.Button btnGrab;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox edtFrame;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox edtHeight;
        private System.Windows.Forms.TextBox edtWidth;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cbxPixelFormat;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label txtFPS;
        private System.Windows.Forms.Timer v_timer;
        private System.Windows.Forms.ListBox listBoxDeviceList;
        private System.Windows.Forms.GroupBox gbDeviceList;
        private System.Windows.Forms.Button btnDiscovery;
        private System.Windows.Forms.ComboBox cbxTestPattern;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbFPS;
    }
}

