
using OpenCvSharp;
using FinalVisionProject.Setting;
using FinalVisionProject.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FinalVisionProject.Device {
    public enum ECameraType {
        Virtual,
        HIK,    // Basler 제거 (HIK 전용)
    }

    /// <summary>
    /// 캡쳐 모드
    /// </summary>
    public enum ECaptureModeType {
        Stop,
        Streaming,
        Trigger,
    }

    public enum ETriggerSource {
        Software,
        Hardware_Line0,
        Hardware_Line1,
        Hardware_Line2,
        Hardware_Line3,
    }

    /// <summary>
    /// 캡쳐 이미지 타입
    /// </summary>
    public enum ECaptureImageType {
        Color24,
        Gray8,
    }

    /// <summary>
    /// 이미지 회전 유형
    /// </summary>
    public enum ERotateAngleType {
        _0,
        _90,
        _180,
        _270,
    }

    //Event 유형
    public delegate void StateEvent(string name);
    public delegate void GrabEvent(string name, Mat bmp);


    /// <summary>
    /// 가상 카메라. 모든 카메라는 이 Class를 상속받아서 구현하는 것이 원칙이다.
    /// 그래야만 FormDeviceSelector에 노출되고, 카메라 목록으로써 사용될 수 있다.
    /// </summary>
    public class VirtualCamera {
        protected DeviceInfo Info;
        public DisplayConfig pConfig { get; private set; }

        public ECameraType CamType { get; private set; }

        public ECaptureModeType CaptureMode { get; protected set; } = ECaptureModeType.Stop;

        public ETriggerSource TriggerSource { get; protected set; } = ETriggerSource.Software;
        
        public string Name { get; protected set; }

        public bool IsOpen { get; protected set; }

        public bool IsGrabbing { get; protected set; }

        public ERotateAngleType RotateAngle { get; set; } = ERotateAngleType._0;

        //lock object 
        protected object Interlock = new object();

        //카메라는 UI단으로 호출될 수 있는 콜백 이벤트를 가진다.
        public virtual event StateEvent GuiCameraOpened = null;
        public virtual event StateEvent GuiCameraClosed = null;
        //public virtual event GrabEvent GuiCammeraGrabed = null;
        public virtual event StateEvent GuiConnectionLost = null;
        public virtual event StateEvent GuiReadyForDisplay = null;

        //ErrorCount 
        protected long prevImageCount = 0;
        protected long imageCount = 0;
        public long ImageCount { get { return imageCount; } }
        protected long errorCount = 0;
        public long ErrorCount { get { return errorCount; } }
        public virtual TimeSpan ElapsedTime { get { return new TimeSpan(); } }


        private System.Windows.Media.Pen DrawPen = null;
        /// <summary>
        /// 그랩 카운트를 초기화한다.
        /// </summary>
        public void ResetGrabCount() {
            Interlocked.Exchange(ref imageCount, 0);
            Interlocked.Exchange(ref errorCount, 0);
        }
        /// <summary>
        /// 촬영된 마지막 이미지를 삭제 및 그랩 카운트를 초기화한다.
        /// </summary>
        public virtual void ClearLastFrame() {
            ResetGrabCount();
            lock (Interlock) {
                if (LastGrabImage != null) {
                    LastGrabImage.Dispose();
                    LastGrabImage = null;
                }
                if(DisplayStream != null) {
                    DisplayStream.Close();
                    DisplayStream.Dispose();
                    DisplayStream = null;
                }
            }
        }
        
        protected MemoryStream DisplayStream = null;
        protected Mat LastGrabImage = null;

        //background image
        protected Mat BackgroundImage = null;
        public int BackgroundImageIndex { get; private set; } = 0;

        public bool IsGrabFromFile { get; private set; }
        public string SelectedImageFile { get; private set; }

        //background image

        private string _BackgroundImagePath;
        public string BackgroundImagePath {
            get {
                return _BackgroundImagePath;
            }
            set {
                if (value != _BackgroundImagePath) {
                    _BackgroundImagePath = value;

                    BackgroundImageFileList.Clear();
                    if (_BackgroundImagePath == null) return;

                    //path 내에 존재하는 image 파일을 로드
                    string[] extensions = { ".bmp", ".jpg", ".jpeg", ".png", ".tiff" };
                    if (Directory.Exists(_BackgroundImagePath)) {
                        IEnumerable<string> fileList = Directory.EnumerateFiles(_BackgroundImagePath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(s => extensions.Any(ext => ext == Path.GetExtension(s)));

                        BackgroundImageFileList.AddRange(fileList);
                    }
                }
            }
        }
        public List<string> BackgroundImageFileList { get; } = new List<string>();

        //property
        public VirtualCameraProperty Properties { get; set; }

        public VirtualCamera(DisplayConfig config, DeviceInfo info, ECameraType camType = ECameraType.Virtual) {
            Info = info;
            pConfig = config;

            this.Name = Info.Identifier;
            this.CamType = camType;

            DrawPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Fuchsia, 4);
            DrawPen.DashStyle = DashStyles.Dash;
        }

        public double CenterX {
            get {
                return this.Properties.Width / 2;
            }
        }

        public double CenterY {
            get {
                return this.Properties.Height / 2;
            }
        }

        public int BackgroundImageCount {
            get {
                return BackgroundImageFileList.Count;
            }
        }

        public string CurrentBackgroundImageFile {
            get {
                if (BackgroundImageIndex >= BackgroundImageFileList.Count) return null;
                return BackgroundImageFileList[BackgroundImageIndex];
            }
        }

        public void IncreaseBackgroundImageIndex() {
            BackgroundImageIndex++;
        }

        public void DecreaseBackgroundImageIndex() {
            BackgroundImageIndex--;
            if (BackgroundImageIndex < 0) BackgroundImageIndex = 0;
        }

        /// <summary>
        /// 마지막 촬영된 이미지를 반환한다.
        /// </summary>
        /// <returns></returns>
        public virtual Mat LastImage {
            get {
                lock (Interlock) {
                    if (BackgroundImagePath == null) {
                        IsGrabFromFile = false;
                        return LastGrabImage;
                    }

                    string selectedImageFile = null;
                    while (selectedImageFile == null) {
                        if (BackgroundImageIndex >= BackgroundImageFileList.Count) {
                            BackgroundImageIndex = 0;
                            break;
                        }
                        selectedImageFile = BackgroundImageFileList[BackgroundImageIndex];
                        if (File.Exists(selectedImageFile) == false) selectedImageFile = null;
                    }
                    if ((SelectedImageFile == selectedImageFile) && (BackgroundImage != null) && (BackgroundImage.Width == Properties.Width) && (BackgroundImage.Height == Properties.Height)) return BackgroundImage;

                    SelectedImageFile = selectedImageFile;
                    if (!String.IsNullOrEmpty(SelectedImageFile)) {
                        Mat resizedImage = new Mat();
                        Mat backgroundImage = null;
                        if (Info.ImageType == ECaptureImageType.Color24) backgroundImage = new Mat(SelectedImageFile, ImreadModes.Color);
                        else if (Info.ImageType == ECaptureImageType.Gray8) backgroundImage = new Mat(SelectedImageFile, ImreadModes.Color);

                        Cv2.Resize(backgroundImage, resizedImage, new OpenCvSharp.Size(Properties.Width, Properties.Height));
                        BackgroundImage = resizedImage;

                        backgroundImage.Dispose();

                        /*
                        //set property width, height
                        Properties.Width = resizedImage.Width;
                        Properties.Height = resizedImage.Height;
                        
                        //virtual camera 이면
                        if (!IsOpen) {
                            Properties[ECameraPropertyType.Width] = resizedImage.Width;
                            Properties[ECameraPropertyType.Height] = resizedImage.Height;
                        }
                        */

                        IsGrabFromFile = true;
                        return resizedImage;
                    }
                    IsGrabFromFile = false;
                    return LastGrabImage;
                }
            }
        }

        public virtual void RenderCenterLine(DrawingContext dc) {
            double ScaledCenterX = CenterX; // * pConfig.DrawScale;
            double ScaledCenterY = CenterY; // * pConfig.DrawScale;
            double ScaledWidth = Properties.Width; // * pConfig.DrawScale;
            double ScaledHeight = Properties.Height; // * pConfig.DrawScale;
            
            if (pConfig.DrawCenterLine) {
                dc.DrawLine(DrawPen, new System.Windows.Point(0, ScaledCenterY), new System.Windows.Point(ScaledWidth, ScaledCenterY));
                dc.DrawLine(DrawPen, new System.Windows.Point(ScaledCenterX, 0), new System.Windows.Point(ScaledCenterX, ScaledHeight));
            }
            if (pConfig.DrawCenterRect) {
                double ScaledRectWidth = pConfig.CenterRectWidth; // * pConfig.DrawScale;
                double ScaledRectHeight = pConfig.CenterRectHeight; // * pConfig.DrawScale;

                double left = ScaledCenterX - (ScaledRectWidth / 2) + pConfig.CenterRectOffsetX;
                double top = ScaledCenterY - (ScaledRectHeight / 2) + pConfig.CenterRectOffsetY;
                double width = ScaledRectWidth;
                double height = ScaledRectHeight;
                System.Windows.Rect rect = new System.Windows.Rect(left, top, width, height);
                dc.DrawRectangle(System.Windows.Media.Brushes.Transparent, DrawPen, rect);
            }
            if (pConfig.DrawCenterCircle) {
                double ScaledCircleRadius = pConfig.CenterCircleRadius; // * pConfig.DrawScale;
                dc.DrawEllipse(System.Windows.Media.Brushes.Transparent, DrawPen, new System.Windows.Point(ScaledCenterX, ScaledCenterY), ScaledCircleRadius, ScaledCircleRadius);
            }
        }
        
        public virtual bool Display(System.Windows.Controls.Image control) {
            lock (Interlock) {
                Mat grabbedImage = LastImage;
                if (grabbedImage == null) return false;

                try {
                    double scaledWidth = grabbedImage.Width * pConfig.DrawScale;
                    double scaledHeight = grabbedImage.Height * pConfig.DrawScale;
                    if ((DisplayStream == null) || (control.Width != scaledWidth) || (control.Height != scaledHeight)) {
                        DisplayStream = new MemoryStream();
                        /*
                        if (Properties == null) {
                            control.Width = scaledWidth;
                            control.Height = scaledHeight;
                        }
                        else {
                            control.Width = (int)scaledWidth;
                            control.Height = (int)scaledHeight;
                        }
                        */
                    }

                    grabbedImage.WriteToStream(DisplayStream, ".bmp");
                    BitmapFrame frame = BitmapFrame.Create(DisplayStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    control.Source = frame;
                }
                catch(Exception e) {
                    Logging.PrintLog((int)ELogType.Error, "VirtualCamera.Display() Fail :" + e.Message);
                    return false;
                }
            }
            return true;
        }

        public virtual bool SaveImage(string fileName) {
            bool result = false;
            try {
                lock (Interlock) {
                    Mat grabbedImage = LastImage;
                    if (grabbedImage == null) return false;
                    if (Info.ImageType == ECaptureImageType.Gray8) {
                        Mat grayImage = new Mat(grabbedImage.Size(), MatType.CV_8UC1);
                        Cv2.CvtColor(grabbedImage, grayImage, ColorConversionCodes.BGR2GRAY);
                        result = grayImage.SaveImage(fileName);
                        grayImage.Dispose();
                    }
                    else if(Info.ImageType == ECaptureImageType.Color24) {
                        result = grabbedImage.SaveImage(fileName);
                    }
                }
            }
            catch(Exception e) {
                Debug.WriteLine($"[351-VirtualCamera.cs] Exception:{e.ToString()}");
                return false;
            }
            return result;
        }
        

        /// <summary>
        /// 주어진 parameter 의 장치를 연다.
        /// </summary>
        /// <param name="param">주어진 parameterㅇ</param>
        /// <returns></returns>
        public virtual bool Open(params object[] param) {
            if(Properties == null) {
                Properties = new VirtualCameraProperty();
                if (param != null && param.Length >= 2) {
                    if (param[0] is int) {
                        int width = (int)param[0];
                        Properties.Width = width;
                    }
                    if (param[1] is int) {
                        int height = (int)param[1];
                        Properties.Height = height;
                    }
                }
            }

            if((Info.RotateAngle == ERotateAngleType._90) || (Info.RotateAngle == ERotateAngleType._270)) {
                int temp = Properties.Height;
                Properties.Height = Properties.Width;
                Properties.Width = temp;
            }
            return true;
        }

        /// <summary>
        /// 해당 장치를 닫는다.
        /// </summary>
        public virtual void Close() {
            StopStream();

            if(BackgroundImage != null) {
                BackgroundImage.Dispose();
                BackgroundImage = null;
            }
            
        }

        /// <summary>
        /// 파일로부터 프로퍼티 정보를 로드한다.
        /// </summary>
        /// <param name="loadFile">파일 경로</param>
        /// <param name="group">그룹명</param>
        /// <returns>로드 성공하면 true, 실패하면 false.</returns>
        public virtual bool LoadProperties(IniFile loadFile, string group) {
            bool result = true;
            Name = loadFile[group]["Name"].ToString();
            RotateAngle = (ERotateAngleType)loadFile[group]["RotateAngle"].ToInt();
            int propCount = loadFile[group]["PropertyCount"].ToInt(0);
            for (int i = 0; i < propCount; i++) {
                string subGroupName = "Property_" + i.ToString();
                ECameraPropertyType propType = (ECameraPropertyType)loadFile[group][subGroupName + "_Type"].ToInt();

                Properties[propType] = (decimal)loadFile[group][subGroupName + "_Value"].ToDouble();
                
                //if (!WriteProperty(propType, Properties[propType])) result = false;
            }
            return result;
        }

        /// <summary>
        /// 파일로부터 프로퍼티 정보를 로드한다.
        /// </summary>
        /// <param name="filePath">로드할 파일 경로</param>
        /// <param name="group">속성의 그룹명(장치 이름 등)</param>
        /// <returns>성공이면 true, 실패면 false</returns>
        public virtual bool LoadProperties(string filePath, string group) {
            if (!File.Exists(filePath)) return false;

            IniFile loadFile = new IniFile();
            loadFile.Load(filePath);

            return LoadProperties(loadFile, group);
        }

        /// <summary>
        /// 프로퍼티 정보를 저장한다.
        /// </summary>
        /// <param name="saveFile">저장할 파일경로</param>
        /// <param name="group">속성의 그룹명(장치 이름 등)</param>
        /// <returns>성공이면 true, 실패면 false</returns>
        public virtual bool SaveProperties(IniFile saveFile, string group) {
            saveFile[group]["Name"] = Name;
            saveFile[group]["RotateAngle"] = (int)RotateAngle;
            saveFile[group]["PropertyCount"] = Properties.Count;
            for (int i = 0; i < Properties.Count; i++) {
                string subGroupName = "Property_" + i.ToString();
                ECameraPropertyType propType = Properties.GetPropType(i);
                saveFile[group][subGroupName + "_Type"] = (int)propType;
                saveFile[group][subGroupName + "_Value"] = (double)Properties[i];
            }
            return true;
        }
        /// <summary>
        /// 프로퍼티 정보를 저장한다.
        /// </summary>
        /// <param name="filePath">저장할 파일 경로</param>
        /// <param name="group">속성의 그룹명(장치 이름 등)</param>
        /// <returns>성공이면 true, 실패면 false</returns>
        public virtual bool SaveProperties(string filePath, string group) {
            IniFile saveFile = new IniFile();

            SaveProperties(saveFile, group);
            
            saveFile.Save(filePath);
            return true;
        }

        public virtual bool SetTriggerMode(ETriggerSource source, bool forcing=false, bool threading = false) {
            CaptureMode = ECaptureModeType.Trigger;
            TriggerSource = source;
            return true;
        }

        public virtual bool SetSoftwareTriggerMode(bool threading = false) {
            CaptureMode = ECaptureModeType.Trigger;
            TriggerSource = ETriggerSource.Software;
            return true;
        }

        public virtual bool ExecuteSoftwareTrigger() {
            return true;
        }

        public virtual bool IsGrabbed() {
            return true;
        }

        public string ModeString {
            get {
                if (BackgroundImage != null) return "Image Loaded";
                else if (CaptureMode == ECaptureModeType.Stop) return "Stop";
                else if (CaptureMode == ECaptureModeType.Streaming) return "Streaming";
                else if (CaptureMode == ECaptureModeType.Trigger) return "Trigger";
                return "Stop";
            }
        }

        public string StateString {
            get {
                if (BackgroundImagePath != null) return string.Format("{0}({1}/{2})", CurrentBackgroundImageFile, BackgroundImageIndex, BackgroundImageCount);
                else if (IsGrabbing) return "Grabbing";
                else return "Ready";
            }
        }

        /// <summary>
        /// 프로퍼티 개수를 반환
        /// </summary>
        /// <returns></returns>
        public int PropertyCount {
            get => Properties.Count;
        }

        /// <summary>
        /// 해당 index의 프로퍼티를 반환한다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public decimal GetProperty(int index) {
            if (index >= Properties.Count) return 0;
            return Properties[index];
        }

        /// <summary>
        /// 이미지 그랩을 수행하고 결과 이미지를 반환
        /// </summary>
        /// <returns></returns>
        public virtual Mat GrabImage() {
            SetSoftwareTriggerMode();
            return LastImage;
        }

        /// <summary>
        /// 스트림을 시작한다.
        /// </summary>
        /// <returns></returns>
        public virtual bool StartStream() {
            CaptureMode = ECaptureModeType.Streaming;

            if(GuiReadyForDisplay != null) {
                GuiReadyForDisplay(Name);
            }
            return true;
        }
        
        /// <summary>
        /// 스트림을 정지한다.
        /// </summary>
        public virtual void StopStream() {
            CaptureMode = ECaptureModeType.Stop;
            ClearLastFrame();
        }

        /// <summary>
        /// 대기 중에, trigger된 이미지를 반환한다.
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public virtual Mat WaitForTrigger(bool clone = true, int timeOut = 3000) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true) {
                if (watch.ElapsedMilliseconds > timeOut) break;
            }
            if ((LastImage != null) && clone) return LastImage.Clone();
            return LastImage;
        }
    }
}
