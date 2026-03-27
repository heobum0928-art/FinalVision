using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ImageGlass;
using OpenCvSharp;

using ReringProject.Define;
using ReringProject.Sequence;
using ReringProject.Setting;
using ReringProject.UI;
using ReringProject.Utility;

namespace ReringProject.UI
{
    /// <summary>
    /// SPRAMultiView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SPRAMultiView : UserControl, IMainView
    {
        public ImageBoxEx LoadBox { get => _LoadBox; set => _LoadBox = value; }
        private ImageBoxEx _LoadBox;
        public ImageBoxEx UnLoadBox { get => _UnLoadBox; set => _UnLoadBox = value; }
        private ImageBoxEx _UnLoadBox;
        public Dictionary<string, SequenceContext> ContextList { get; set; }

        private MemoryStream BackgroundImageStream = null;
        private object Interlock = new object();

        public double LoadBoxZoomFactor { get; set; }   // 11.19 Insert
        public double UnLoadBoxZoomFactor { get; set; } // 11.19 Insert

        public SPRAMultiView()
        {
            InitializeComponent();

            LoadBox = new ImageBoxEx();
            LoadBox.Name = "LoadBox";
            LoadBox.AllowClickZoom = true;
            LoadBox.ShowPixelGrid = true;
            LoadBox.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
            LoadBox.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
            LoadBox.Zoom = 70;
            LoadBoxZoomFactor = LoadBox.Zoom;   // 11.19 Insert
            LoadBox.AutoCenter = true;
            //LoadBox.AutoSize = true;
            LoadBox.ZoomToFit();
            LoadBox.MouseWheel += BoxViewer_MouseWheel;
            LoadBox.MouseMove += BoxViewer_MouseMove;

            LoadBox.Refresh();

            LoadImage.Child = LoadBox;

            UnLoadBox = new ImageBoxEx();
            UnLoadBox.Name = "UnLoadBox";
            UnLoadBox.AllowClickZoom = true;
            UnLoadBox.ShowPixelGrid = true;
            UnLoadBox.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
            UnLoadBox.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
            UnLoadBox.Zoom = 70;
            UnLoadBoxZoomFactor = UnLoadBox.Zoom;   // 11.19 Insert
            UnLoadBox.AutoCenter = true;
            //UnLoadBox.AutoSize = true;
            UnLoadBox.ZoomToFit();
            UnLoadBox.MouseWheel += BoxViewer_MouseWheel;
            UnLoadBox.MouseMove += BoxViewer_MouseMove;

            UnLoadBox.Refresh();

            UnLoadImage.Child = UnLoadBox;
        }


        /// <summary>
        /// Converter to ImageGlass Image
        /// </summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        public bool Display(string name, string result, Brush resultBrush, object param = null)
        {
            switch (name)
            {
                case "SPRA_LOAD":
                    break;
                case "SPRA_UNLOAD":
                    break;
            }
            return true;
        }

        public bool Display(string name, Mat img, string result, Brush resultBrush, object param = null)
        {
            SequenceContext cont = ContextList[name];

            string actName = null;
            if (param is string) actName = (string)param;

            try
            {
                lock (Interlock)
                {
                    if (img != null && (img.Empty() == false))
                    {
                        using (BackgroundImageStream = new MemoryStream())
                        {
                            img.WriteToStream(BackgroundImageStream, ".bmp");
                            BackgroundImageStream.Seek(0, SeekOrigin.Begin);
                            BitmapFrame frame = BitmapFrame.Create(BackgroundImageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                            switch (cont.Source.ID)
                            {
                                case ESequence.Load:
                                case ESequence.UnLoad:
                                    SequenceSPRAContext SPRAContext = ContextList[name] as SequenceSPRAContext;
                                    string SourceName = SPRAContext.Source.Name;

                                    if (SPRAContext.bfinish == true)
                                    {
                                        switch (SourceName)
                                        {
                                            case "SPRA_LOAD":
                                                if (actName == "Inspect" || actName == "Calibration")
                                                {
                                                    //Debug.WriteLine($"{actName}");
                                                    LoadBox.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SPRAContext.SPRALoadImage);
                                                    LoadBox.AutoCenter = true;
                                                    LoadBox.ZoomToFit();

                                                    do
                                                    {
                                                        if (LoadBox.Zoom < LoadBoxZoomFactor)
                                                            LoadBox.ZoomIn();
                                                        else if (LoadBox.Zoom > LoadBoxZoomFactor)
                                                            LoadBox.ZoomOut();

                                                    } while (LoadBox.Zoom != LoadBoxZoomFactor);

                                                    LoadBox.Refresh();
                                                }
                                                else
                                                {
                                                    LoadBox.Image = BitmapFromSource(frame);
                                                    LoadBox.AutoCenter = true;
                                                    LoadBox.ZoomToFit();

                                                    do
                                                    {
                                                        if (LoadBox.Zoom < LoadBoxZoomFactor)
                                                            LoadBox.ZoomIn();
                                                        else if (LoadBox.Zoom > LoadBoxZoomFactor)
                                                            LoadBox.ZoomOut();

                                                    } while (LoadBox.Zoom != LoadBoxZoomFactor);


                                                    LoadBox.Refresh();
                                                }
                                                SPRAContext.bfinish = false;
                                                break;

                                            case "SPRA_UNLOAD":
                                                if (actName == "Inspect" || actName == "Calibration")
                                                {
                                                    //Debug.WriteLine($"{actName}");
                                                    UnLoadBox.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(SPRAContext.SPRAUnLoadImage);
                                                    UnLoadBox.AutoCenter = true;
                                                    UnLoadBox.ZoomToFit();
                                                    //UnLoadBox.Zoom = 75;

                                                    do
                                                    {
                                                        if (UnLoadBox.Zoom < UnLoadBoxZoomFactor)
                                                            UnLoadBox.ZoomIn();
                                                        else if (UnLoadBox.Zoom > UnLoadBoxZoomFactor)
                                                            UnLoadBox.ZoomOut();

                                                    } while (UnLoadBox.Zoom != UnLoadBoxZoomFactor);

                                                    UnLoadBox.Refresh();
                                                }
                                                else
                                                {
                                                    UnLoadBox.Image = BitmapFromSource(frame);
                                                    UnLoadBox.AutoCenter = true;
                                                    UnLoadBox.ZoomToFit();

                                                    do
                                                    {
                                                        if (UnLoadBox.Zoom < UnLoadBoxZoomFactor)
                                                            UnLoadBox.ZoomIn();
                                                        else if (UnLoadBox.Zoom > UnLoadBoxZoomFactor)
                                                            UnLoadBox.ZoomOut();

                                                    } while (UnLoadBox.Zoom != UnLoadBoxZoomFactor);

                                                    UnLoadBox.Refresh();
                                                }
                                                SPRAContext.bfinish = false;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (SourceName)
                                        {
                                            case "SPRA_LOAD":

                                                LoadBox.Image = BitmapFromSource(frame);
                                                LoadBox.AutoCenter = true;
                                                LoadBox.ZoomToFit();

                                                //Debug.WriteLine($"ZoomScale:{LoadBox.ZoomFactor}, Zoom:{LoadBox.Zoom}");
                                                do
                                                {
                                                    if (LoadBox.Zoom < LoadBoxZoomFactor)
                                                        LoadBox.ZoomIn();
                                                    else if (LoadBox.Zoom > LoadBoxZoomFactor)
                                                        LoadBox.ZoomOut();
                                                    
                                                } while (LoadBox.Zoom != LoadBoxZoomFactor);

                                                //Debug.WriteLine($"ZoomScale:{LoadBox.ZoomFactor}, Zoom:{LoadBox.Zoom}");

                                                LoadBox.Refresh();

                                                break;

                                            case "SPRA_UNLOAD":

                                                UnLoadBox.Image = BitmapFromSource(frame);
                                                UnLoadBox.AutoCenter = true;
                                                UnLoadBox.ZoomToFit();
                                                //UnLoadBox.Zoom = 100;

                                                do
                                                {
                                                    if (UnLoadBox.Zoom < UnLoadBoxZoomFactor)
                                                        UnLoadBox.ZoomIn();
                                                    else if (UnLoadBox.Zoom > UnLoadBoxZoomFactor)
                                                        UnLoadBox.ZoomOut();

                                                } while (UnLoadBox.Zoom != UnLoadBoxZoomFactor);

                                                UnLoadBox.Refresh();

                                                break;
                                        }
                                    }

                                    break;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {

            }
            return true;
        }

        public string GetXYPixel(int x, int y)
        {
            string str_XY = string.Format($"Pixel X:{x},Y:{y}");
            return str_XY;
        }
        public string GetXYCenterTomm(int x, int y, string strContext)
        {
            //SequenceContext cont = ContextList[strContext];
            SequenceSPRAContext temp = ContextList[strContext] as SequenceSPRAContext;  // 실행된 Sequence의 정보를 가져 오는 것이기 때문에 사용자가 이미지를 수동으로 불러오는 경우, temp는 null 이된다.
                                                                                        // 이때는 실제 PixelResolution 값을 가져 올수가 없어 0,0 으로 표기됨.
            
            double Xmm = 0;
            double Ymm = 0;

            if (temp.ResultString != "None")    // 실행된 Sequence 가 확실한 상태라면, 해당 Context의 Parameter 값을 사용.
            {

                Xmm = (x + 1) * temp.PixelResolution / 1000;    // 이미지 행렬의 0은 픽셀의 최소 사이즈를 가져야 하기 때문에 +1 필요.
                Ymm = (y + 1) * temp.PixelResolution / 1000;    // 이미지 행렬의 0은 픽셀의 최소 사이즈를 가져야 하기 때문에 +1 필요.

                if (Xmm <= temp.ScreenCenter_XMM)
                    Xmm = -1 * (temp.ScreenCenter_XMM - Xmm);
                else
                    Xmm -= temp.ScreenCenter_XMM;

                if (Ymm <= temp.ScreenCenter_YMM)
                    Ymm = -1 * (temp.ScreenCenter_YMM - Ymm);
                else
                    Ymm -= temp.ScreenCenter_YMM;
            }
            else if(temp.Source.Name == "SPRA_LOAD" || temp.Source.Name == "SPRA_UNLOAD")   // Sequence 실행되지 않고, Grab 만 된 상태 또는 Debugging 상태일때 실행 프로젝트 기준의 중앙 mm 값을 지정하여 사용.
            {
                //Debug.WriteLine($"{temp.Source.Name}");

                Xmm = (x + 1) * 30 / 1000;    // 이미지 행렬의 0은 픽셀의 최소 사이즈를 가져야 하기 때문에 +1 필요.
                Ymm = (y + 1) * 30 / 1000;    // 이미지 행렬의 0은 픽셀의 최소 사이즈를 가져야 하기 때문에 +1 필요.

                // Sequence.Name 즉 SPRA project 일때는 
                // Image X Center : 19.2mm, Image Y Center: 15.36 을 고정으로 사용하게 함.
                if (Xmm <= 19.2)
                    Xmm = -1 * (19.2 - Xmm);
                else
                    Xmm -= 19.2;

                if (Ymm <= 15.36)
                    Ymm = -1 * (15.36 - Ymm);
                else
                    Ymm -= 15.36;
            }
            else
            {
                Debug.WriteLine("UnDefine Sequence...");
                Xmm = 0;
                Ymm = 0;
            }

            //string str_XY = string.Format($"mm X:{Xmm},Y:{Ymm}");
            string str_XY = string.Format("mm X:{0:0.000},Y:{1:0.000}", Xmm, Ymm);  // 소수점 고정 - 3자리.
            return str_XY;
        }

        private void BoxViewer_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Drawing.Point cPT = new System.Drawing.Point();  // Client Window 좌표.
            cPT.X = e.X;
            cPT.Y = e.Y;

            System.Drawing.Point iPT = new System.Drawing.Point();  // Image 좌표.

            ImageBoxEx box = sender as ImageBoxEx;

            switch (box.Name)
            {
                case "LoadBox":
                    iPT = LoadBox.PointToImage(cPT, true);
                    LoadBox.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                    LoadBox.TextDisplayMode = ImageBoxGridDisplayMode.Image;
                    LoadBox.TextBackColor = System.Drawing.Color.Beige;
                    LoadBox.Text = GetXYPixel(iPT.X, iPT.Y) + "\r\n" + GetXYCenterTomm(iPT.X, iPT.Y, "SPRA_LOAD");
                    //LoadBox.Text = GetXYPixel(iPT.X, iPT.Y) + "\r\n" + GetXYCenterTomm(iPT.X, iPT.Y, "SPRA_LOAD") +"\r\n"+ "Z:" + LoadBox.Zoom.ToString();
                    break;

                case "UnLoadBox":
                    iPT = UnLoadBox.PointToImage(cPT, true);
                    UnLoadBox.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                    UnLoadBox.TextDisplayMode = ImageBoxGridDisplayMode.Image;
                    UnLoadBox.TextBackColor = System.Drawing.Color.Beige;
                    UnLoadBox.Text = GetXYPixel(iPT.X, iPT.Y) + "\r\n" + GetXYCenterTomm(iPT.X, iPT.Y, "SPRA_UNLOAD");
                    break;

            }

        }

        private void BoxViewer_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Point wPT = new System.Windows.Point();
            wPT.X = e.X;
            wPT.Y = e.Y;

            System.Drawing.Point dPT = new System.Drawing.Point((int)wPT.X, (int)wPT.Y);

            ImageBoxEx box = sender as ImageBoxEx;

            switch (box.Name)
            {
                case "LoadBox":
                    LoadBox.ZoomWithMouseWheel(e.Delta, dPT);
                    //Debug.WriteLine($"LoadBox-ZoomValue:{LoadBox.Zoom}");       // 11.19 insert
                    LoadBoxZoomFactor = LoadBox.Zoom;                           // 11.19 insert
                    break;
                case "UnLoadBox":
                    UnLoadBox.ZoomWithMouseWheel(e.Delta, dPT);
                    //Debug.WriteLine($"UnLoadBox-ZoomValue:{UnLoadBox.Zoom}");   // 11.19 insert
                    UnLoadBoxZoomFactor = UnLoadBox.Zoom;                       // 11.19 insert
                    break;
            }
        }

    }
}
