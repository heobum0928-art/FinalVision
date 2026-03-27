
using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.Extension;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Windows.Shapes;
using System.Windows.Threading;

namespace Project.UI.Common
{
    /// <summary>
    /// ImageView.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public class RoiRectInfo
    {
        #region fields
        public int ROIIndex;

        public System.Windows.Shapes.Rectangle shape_rect;
        public Project.BaseLib.DataStructures.RoiRectangle roi_rect;
        #endregion

        #region propertise

        #endregion

        #region methods

        #endregion

        #region constructors
        public RoiRectInfo()
        {
            ROIIndex = 0;
        }

        #endregion
    }

    public delegate void SelectedRectangle(RoiRectInfo info);

    public delegate void MouseMoveImageInformation(int x, int y, int gray);
    public partial class ImageView : UserControl
    {
        private enum HitType
        {
            None,       /// 해당 무
            Body,       /// 몸체
            UpperLeft,  /// 좌상단
            UpperRight, /// 우상단
            LowerRight, /// 우하단
            LowerLeft,  /// 좌하단
            Left,       /// 왼쪽
            Right,      /// 오른쪽
            Top,        /// 위쪽
            Bottom      /// 아래쪽
        };
        #region fields

        protected event SelectedRectangle selectedRectangleEvent = null;
        protected event MouseMoveImageInformation OnMouseMoveImageInformation = null;
        protected double dScaleRatio = 100;
        protected ILogger logger;

        protected ByteImage _byte_image;
        protected BitmapImage bmpImage;
        private byte[] buffer = null;

        protected bool enableZoomInOut = false;

        protected Point scrollOffset;
        protected Point imgMousePoint;
        protected Point scMousePoint;

        protected Color color;

        protected bool enableSelectShape = false;

        private HitType hitType = HitType.None;

        private Shape clickHitShape = null;      
        private Shape hitShape = null;

        private bool isDragging = false;

        private Point lastPoint;

        private ObservableCollection<RoiRectInfo> roiRectInfos;

        private string _ImageInformation = string.Empty;


        private Point TopLeft;

        #endregion

        #region properties

        public int ROIRectCount
        {
            get { return roiRectInfos.Count; }
        }

        public RoiRectangle [] RoiRectangles
        {
            get
            {
                var rects = roiRectInfos.Select(s => s.roi_rect).ToArray();




                foreach(var rect in rects)
                {
                    GeneralTransform transform = img.TransformToVisual(canv);
                    var LT = transform.Transform(new Point(rect.Left, rect.Top));



                }


                BitmapSource CurrentSource = BitmapSource as BitmapSource;
                double dX = CurrentSource.PixelWidth / canv.ActualWidth;
                double dY = CurrentSource.PixelHeight / canv.ActualHeight;

                return rects;
                return roiRectInfos.Select(s => s.roi_rect).ToArray();
            }
        }

        public List<RoiRectInfo> RoiRectInfos
        {
            get { return roiRectInfos.ToList(); }
        }


        public bool EnableSelectShape
        {
            get { return enableSelectShape; }

            set { enableSelectShape = value; }
        }
        public bool EnableCenterCrossLine
        {
            get
            {
                return vLine.Visibility == Visibility.Visible ? true : false;
            }

            set
            {
                vLine.Visibility = value == true ? Visibility.Visible : Visibility.Hidden;
                hLine.Visibility = value == true ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public BitmapSource BitmapSource
        {
            get 
            { 
                return img.Source as BitmapSource; 
            }

            set
            {
                img.Source = value;
                dScaleRatio = 100;
            }
        }
        public Color Color
        {
            get { return color; }

            set { color = value; }
        }
        public Stretch Stretch
        {
            get { return img.Stretch; }

            set
            {
                img.Stretch = value;
            }
        }
        public Image Image
        {
            get { return img; }

            set
            {
                img = value;
            }
        }
        public double PixelWidth
        {

            get
            {
                if (img.Source != null)
                    return (img.Source as BitmapSource).PixelWidth;
                else
                    return 0.0;
            }
        }
        public double PixelHeight
        {
            get
            {
                if (img.Source != null)
                    return (img.Source as BitmapSource).PixelHeight;
                else
                    return 0.0;
            }
        }
        public double ImageActualWidth
        {
            get { return img.ActualWidth; }
        }
        public double ImageActualHeight
        {
            get { return img.ActualHeight; }
        }
        public bool EnableZoomInOut
        {
            set { enableZoomInOut = value; }
        }
        public bool ScrollBarHidden
        {
            set
            {
                if (value == true)
                {
                    sc.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        public Rectangle SelectedRectangle
        {
            get
            {
                if (clickHitShape == null)
                    return null;

                return clickHitShape as Rectangle;

            }
        }
        #endregion

        #region constructor
        public ImageView()
        {
            InitializeComponent();
            logger = LogManager.GetLogger("ImageView");

            sc.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sc.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            DataContext = new ImageViewModel();

            this.roiRectInfos = new ObservableCollection<RoiRectInfo>();

            Loaded += ImageView_Loaded;

            canv.PreviewMouseLeftButtonDown += canv_PreviewMouseLeftButtonDown;
            canv.PreviewMouseMove += canvas_MouseMove;
            canv.MouseDown  += canvas_MouseDown;
            canv.MouseUp    += canvas_MouseUp;
        }
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //logger.Debug()("canvas_MouseMove");

            if (enableSelectShape == false)
                return;

            if (!this.isDragging)
            {
                FindHit(Mouse.GetPosition(this.canv));

                SetMouseCursor();
            }
            else if (this.hitShape is Polygon)
            {
                // 다각형은 처리하지 않는다.
            }
            else
            {
                Point point = Mouse.GetPosition(this.canv);

                double offsetX = point.X - this.lastPoint.X;
                double offsetY = point.Y - this.lastPoint.Y;

                double newX = Canvas.GetLeft(this.hitShape);
                double newY = Canvas.GetTop(this.hitShape);

                double newWidth = this.hitShape.Width;
                double newHeight = this.hitShape.Height;

                switch (this.hitType)
                {
                    case HitType.Body:

                        newX += offsetX;
                        newY += offsetY;

                        break;

                    case HitType.UpperLeft:

                        newX += offsetX;
                        newY += offsetY;

                        newWidth -= offsetX;
                        newHeight -= offsetY;

                        break;

                    case HitType.UpperRight:

                        newY += offsetY;

                        newWidth += offsetX;
                        newHeight -= offsetY;

                        break;

                    case HitType.LowerRight:

                        newWidth += offsetX;
                        newHeight += offsetY;

                        break;

                    case HitType.LowerLeft:

                        newX += offsetX;

                        newWidth -= offsetX;
                        newHeight += offsetY;

                        break;

                    case HitType.Left:

                        newX += offsetX;

                        newWidth -= offsetX;

                        break;

                    case HitType.Right:

                        newWidth += offsetX;

                        break;

                    case HitType.Bottom:

                        newHeight += offsetY;

                        break;

                    case HitType.Top:

                        newY += offsetY;

                        newHeight -= offsetY;

                        break;
                }

                if ((newWidth > 0) && (newHeight > 0))
                {
                    Canvas.SetLeft(this.hitShape, newX);
                    Canvas.SetTop(this.hitShape, newY);

                    this.hitShape.Width = newWidth;
                    this.hitShape.Height = newHeight;

                    this.lastPoint = point;
                }
            }
        }
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.isDragging = false;
            SetRoiRectangle();
        }
        private void canv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //logger.Debug()("canv_PreviewMouseLeftButtonDown");
            FindClickHit(Mouse.GetPosition(this.canv));

            SetMouseCursor();

            if (this.hitType == HitType.None)
            {
                return;
            }

            this.lastPoint = Mouse.GetPosition(this.canv);

            this.isDragging = true;
        }
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FindClickHit(Mouse.GetPosition(this.canv));

            SetMouseCursor();

            if (this.hitType == HitType.None)
            {
                return;
            }

            this.lastPoint = Mouse.GetPosition(this.canv);

            this.isDragging = true;
        }
        private void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            double gap = 1;

            double ah = ActualHeight;
            double aw = ActualWidth;

            double width = (ActualWidth / 5) - (gap * 2);
            double height = (ActualHeight / 5) - (gap * 2);




            ////for (int i = 0; i < ROIRectCount; i++)
            //for(int j = 0; j < 4; j++)
            //for(int i = 0; i < 5; i++)
            //{
            //    Rectangle rect = new Rectangle();
            //    rect.Fill = Brushes.Transparent;
            //    rect.StrokeThickness = 2;
            //    rect.StrokeDashArray = new DoubleCollection { 1, 2, 4 };
            //    rect.Stroke = new SolidColorBrush(ROIColors.GetColor(j*4 + i));

            //    rect.Width = width;
            //    rect.Height = height;

            //    rectList.Add(rect);
            //    canv.Children.Add(rect);

            //    Canvas.SetLeft(rect, i * width + 2);
            //    Canvas.SetTop(rect, j * height + 2);
            //}


            double iaw = img.ActualWidth;
            double iah = img.ActualHeight;
        }

        #endregion

        #region Event Methods

        #endregion

        #region Calculate Methods
        private Point DeviceToReal(double deviceWidth, double deviceHeight, double deviceX, double deviceY, double realWidth, double realHeight)
        {
            // deviceWidth, deviceHeight = 이미지 최종 크기(확대/축소된 후)
            // deviceX, deviceY : 마우스 좌표
            // realWidth, realHeight : 이미지 사이즈
            double x1 = realWidth * deviceX / deviceWidth; // 마우스 좌표에 확대/축소 비율 곱
            double y1 = realHeight * deviceY / deviceHeight;

            double x = Math.Truncate(x1); // Math.Truncate = 소수점 올림, Math.Ceiling = 내림, Math.Round = 반올림.
            double y = Math.Truncate(y1);

            return new Point(x, y);
        }
        private double ChangeScale(double scale, int delta, int min = 10, int max = 1000)
        {
            if (delta > 0)
            {
                if (scale < max)
                    scale = scale >= 100 ? scale + 100 : scale * 2;
            }
            else
            {
                if (scale > min)
                    scale = scale > 100 ? scale - 100 : scale / 2;
            }
            return scale;
        }
        private List<byte> GetData(byte[] data, int start, int gap, int index, List<byte> result)
        {
            if (start + index * gap >= data.Length)
                return result;
            result.Add(data[start + index * gap]);
            GetData(data, start, gap, ++index, result);
            return result;
        }
        private byte[] GetProfileX(byte[] data, int y)
        {
            int stride = data.Length / bmpImage.PixelHeight;
            int gap = stride / bmpImage.PixelWidth;
            byte[] row = new byte[stride];
            Array.Copy(data, stride * y, row, 0, row.Length);

            byte[] result = GetData(row, 0, 4, 0, new List<byte>()).ToArray();
            return result;
        }
        private byte[] GetProfileY(byte[] data, int x)
        {
            int stride = data.Length / bmpImage.PixelHeight;
            int gap = stride / bmpImage.PixelWidth;

            byte[] result = GetData(buffer, x, stride, 0, new List<byte>()).ToArray();
            return result;
        }
        private Color GetPixelColor(Point CurrentPoint, ref int x, ref int y)
        {
            byte[] Pixels = new byte[4];
            BitmapSource CurrentSource = BitmapSource as BitmapSource;

            // 비트맵 내의 좌표 값 계산
            CurrentPoint.X *= CurrentSource.PixelWidth / canv.ActualWidth;
            CurrentPoint.Y *= CurrentSource.PixelHeight / canv.ActualHeight;

            //x = (int)CurrentPoint.X;
            //y = (int)CurrentPoint.Y;
            x = (int)MathExtensions.limit(CurrentPoint.X, 0, CurrentSource.PixelWidth -1);
            y = (int)MathExtensions.limit(CurrentPoint.Y, 0, CurrentSource.PixelHeight - 1);

            if (CurrentSource.Format == PixelFormats.Bgra32 || CurrentSource.Format == PixelFormats.Bgr32)
            {
                // 32bit stride = (width * bpp + 7) /8
                int Stride = (CurrentSource.PixelWidth * CurrentSource.Format.BitsPerPixel + 7) / 8;
                // 한 픽셀 복사
                CurrentSource.CopyPixels(
                    new Int32Rect((int)CurrentPoint.X, (int)CurrentPoint.Y, 1, 1), Pixels, Stride, 0);

                // 컬러로 변환 후 리턴
                return Color.FromArgb(Pixels[3], Pixels[2], Pixels[1], Pixels[0]);
            }
            else if (CurrentSource.Format == PixelFormats.Indexed8 || CurrentSource.Format == PixelFormats.Gray8)
            {
                byte[] Pixel = new byte[1];

                int stride = (int)PixelWidth * ((CurrentSource.Format.BitsPerPixel + 7) / 8);


                //CurrentSource.CopyPixels(
                //                new Int32Rect((int)CurrentPoint.X, (int)CurrentPoint.Y, 1, 1), Pixel, stride, 0);

                CurrentSource.CopyPixels(new Int32Rect(x, y, 1, 1), Pixel, stride, 0);

                Pixels[3] = Pixels[2] = Pixels[1] = Pixels[0] = Pixel[0];

                _ImageInformation = string.Format("X = [{0}], Y = [{1}], Gray = [{2}]", x, y, Pixel[0]);

                if (OnMouseMoveImageInformation != null)
                    OnMouseMoveImageInformation(x, y, (int)Pixel[0]);

            }
            //else if(CurrentSource.Format == PixelFormats.Gray8)
            //{

            //}
            else
            {
                //MessageBox.Show("지원되지 않는 포맷형식");

                //logger.Error()("지원되지 않는 포맷형식");
            }


            return Color.FromArgb(Pixels[3], Pixels[2], Pixels[1], Pixels[0]);
        }
        #endregion

        #region Mouse Events


        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double pw = PixelWidth;
            double ph = PixelHeight;

            double aw = ActualWidth;
            double ah = ActualHeight;

            if (Stretch == Stretch.None)
            {
                canv.Width = PixelWidth;
                canv.Height = PixelHeight;
            }
            else if (Stretch == Stretch.Uniform)
            {
                img.Measure(new Size(sc.ActualWidth, sc.ActualHeight));



                canv.Width = img.DesiredSize.Width;
                canv.Height = img.DesiredSize.Height;

                var dWidth = img.DesiredSize.Width;
                var dHeight = img.DesiredSize.Height;

                double dwratio = sc.ActualWidth / dWidth;
                double dhratio = sc.ActualHeight / dHeight;


                if (sc.ActualWidth <= dWidth && sc.ActualHeight <= dHeight)
                {
                    if (dwratio > dhratio)
                        img.Width = dWidth;
                    else
                        img.Height = dHeight;

                    //if (dWidth >= dHeight)
                    //    img.Width = dWidth;
                    //else
                    //    img.Height = dHeight;
                }
                else if (sc.ActualWidth >= dWidth && sc.ActualHeight >= dHeight)
                {
                    if (sc.ActualWidth >= sc.ActualHeight)
                        img.Width = dWidth;
                    else
                        img.Height = dHeight;
                }
                else
                {

                }



                //if(sc.ActualWidth >= sc.ActualHeight)
                //{


                //    img.Height = sc.ActualHeight;
                //}
                //else if(sc.ActualWidth <= sc.ActualHeight)
                //{
                //    if(dWidth >= dHeight)
                //    {
                //        img.Width = dWidth;
                //    }
                //    else
                //    {
                //        img.Height = dHeight;
                //    }
                //}
                //else
                //{
                //    img.Width = dWidth;
                //}







                //if (sc.ActualHeight == dHeight)
                //    img.Height = img.DesiredSize.Height;
                //else
                //{
                //    if (sc.ActualWidth == dWidth)
                //    {
                //        img.Height = img.DesiredSize.Height;
                //    }
                //    else
                //        img.Width = img.DesiredSize.Width;
                //}
            }


            GeneralTransform transform = img.TransformToVisual(canv);
            TopLeft = transform.Transform(new Point(0, 0));

            vLine.X1 = TopLeft.X + img.ActualWidth / 2.0;
            vLine.X2 = TopLeft.X + img.ActualWidth / 2.0;
            vLine.Y1 = TopLeft.Y + 0.0;
            vLine.Y2 = TopLeft.Y + img.ActualHeight;



            hLine.Y1 = TopLeft.Y + img.ActualHeight / 2.0;
            hLine.Y2 = TopLeft.Y + img.ActualHeight / 2.0;
            hLine.X1 = TopLeft.X + 0.0;
            hLine.X2 = TopLeft.X + img.ActualWidth;

            //logger.Debug()("===============================");
            //logger.Debug()("scrollViewer: {0}, {1}", sc.ActualWidth, sc.ActualHeight);
            //logger.Debug()("canvas: {0}, {1}", canv.Width, canv.Height);
            //logger.Debug()("img.DesiredSize: {0}, {1}", img.DesiredSize.Width, img.DesiredSize.Height);
            //logger.Debug()("img.PixelSize: {0}, {1}", PixelWidth, PixelHeight);
            //logger.Debug()("img.size: {0}, {1}", img.Width, img.Height);
            //logger.Debug()("img.Actual: {0}, {1}", img.ActualWidth, img.ActualHeight);
        }


        private void img_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            var pt = e.GetPosition((IInputElement)sender);

            imgMousePoint = pt;

            int x = 0;
            int y = 0;

            Color = GetPixelColor(pt, ref x, ref y);

            //AppLogger.Debug()(string.Format("Mouse move point : {0}, {1} // real point : {2}, {3} // Color R[{4}], G[{5}], B[{6}]",
            //    pt.X, pt.Y, x, y, Color.R, Color.G, Color.B));
        }
        private void img_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            //if (!enableZoomInOut)
            //{
            //    logger.Debug()("Zoom In / Out is not enabled.");
            //    return;
            //}
            //dScaleRatio = ChangeScale(dScaleRatio, e.Delta, 50);

            //double ratio = dScaleRatio / 100.0;

            //double ratio_w = canv.Width * ratio;
            //double ratio_h = canv.Height * ratio;

            //if (ratio_w < canv.Width)
            //{
            //    ratio = (sc.ActualWidth - 1) / canv.Width;
            //    ScaleTransform st = new ScaleTransform(ratio, ratio);
            //    canv.LayoutTransform = st;
            //}
            //else if (ratio_h < canv.Height)
            //{
            //    ratio = (sc.ActualHeight - 1) / canv.Height;
            //    ScaleTransform st = new ScaleTransform(ratio, ratio);
            //    canv.LayoutTransform = st;
            //}
            //else
            //{
            //    ScaleTransform st = new ScaleTransform(ratio, ratio);
            //    canv.LayoutTransform = st;
            //}

            //logger.Debug()("Image Size: {0}, {1}", img.Width, img.Height);
            //logger.Debug()("Image Actual: {0}, {1}", img.ActualWidth, img.ActualHeight);
            //logger.Debug()("scrollViewer: {0}, {1}", sc.ActualWidth, sc.ActualHeight);
            //logger.Debug()("dScaleRatio : {0}", ratio);
            //logger.Debug()("canv size : {0}, {1}", canv.Width, canv.Height);
        }
        #endregion

        #region Scroll Events
        private void sc_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //logger.Debug("sc_MouseLeftButtonDown");

            if(clickHitShape == null)
            {
                var pt = e.GetPosition((IInputElement)sender);

                scMousePoint = pt;
                scrollOffset.X = sc.HorizontalOffset;
                scrollOffset.Y = sc.VerticalOffset;
                sc.CaptureMouse();
            }

        }
        private void sc_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //logger.Debug("sc_PreviewMouseLeftButtonDown");
            //var pt = e.GetPosition((IInputElement)sender);

            //scMousePoint = pt;
            //scrollOffset.X = sc.HorizontalOffset;
            //scrollOffset.Y = sc.VerticalOffset;

            //logger.Debug()("Scroll Mouse Left Down and MouseCaptured");
            //sc.CaptureMouse();
        }
        private void sc_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //logger.Debug("sc_PreviewMouseLeftButtonUp");
            sc.ReleaseMouseCapture();

            //logger.Debug()("Scroll Mouse Left Up and ReleseMouseCaptured");
        }
        private void sc_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //logger.Debug("sc_PreviewMouseMove");
            if (sc.IsMouseCaptured)
            {
                sc.ScrollToHorizontalOffset(scrollOffset.X + (scMousePoint.X - e.GetPosition(sc).X));
                sc.ScrollToVerticalOffset(scrollOffset.Y + (scMousePoint.Y - e.GetPosition(sc).Y));
            }

            //logger.Debug()("Scroll Mouse move");

        }
        private void sc_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!enableZoomInOut)
            {
                //logger.Debug()("Zoom In / Out is not enabled.");
                return;
            }
            dScaleRatio = ChangeScale(dScaleRatio, e.Delta, 50);

            double ratio = dScaleRatio / 100.0;

            double ratio_w = canv.Width * ratio;
            double ratio_h = canv.Height * ratio;

            if (ratio_w < canv.Width && ratio_h < canv.Height)
            {

            }
            else
            {
                ScaleTransform st = new ScaleTransform(ratio, ratio);
                canv.LayoutTransform = st;
            }

            //logger.Debug()("Image Size: {0}, {1}", img.Width, img.Height);
            //logger.Debug()("Image Actual: {0}, {1}", img.ActualWidth, img.ActualHeight);
            //logger.Debug()("scrollViewer: {0}, {1}", sc.ActualWidth, sc.ActualHeight);
            //logger.Debug()("dScaleRatio : {0}", ratio);
            //logger.Debug()("canv size : {0}, {1}", canv.Width, canv.Height);
        }
        #endregion

        #region Draw Function
        public void DrawRectangle(int x, int y, int width, int height, Brush Stroke = null, double StrokeThickness = 1.0, Brush Fill = null, double radiusX = 0.0, double radiusY = 0.0)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (Stroke == null)
                    Stroke = Brushes.Black;

                if (Fill == null)
                    Fill = Brushes.Transparent;

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double dx = x * ratio_w;
                double dy = y * ratio_h;

                Rectangle rect = new Rectangle();


                rect.Stroke = Stroke;

                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.HorizontalAlignment = HorizontalAlignment.Left;

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                rect.Fill = Fill;

                rect.StrokeThickness = StrokeThickness;

                Canvas.SetLeft(rect, dx);
                Canvas.SetTop(rect, dy);

                canv.Children.Add(rect);
            }));

        }

        public void DrawCrossLine(double x, double y, int length, int thickness, Brush color)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double dx = x * ratio_w;
                double dy = y * ratio_h;

                Line h_line = new Line();
                //h_line.Fill = new SolidColorBrush(color);
                h_line.Stroke = color;
                //h_line.Stroke = Brushes.Red;
                h_line.StrokeThickness = thickness;

                h_line.X1 = (x - length) * ratio_w;
                h_line.X2 = (x + length) * ratio_w;

                h_line.Y1 = y * ratio_h;
                h_line.Y2 = y * ratio_h;

                Line v_line = new Line();
                //v_line.Fill = new SolidColorBrush(color);
                v_line.Stroke = color;
                //
                //v_line.Stroke = Brushes.Red;

                v_line.StrokeThickness = thickness;

                v_line.X1 = x * ratio_w;
                v_line.X2 = x * ratio_w;

                v_line.Y1 = (y - length) * ratio_h;
                v_line.Y2 = (y + length) * ratio_h;

                canv.Children.Add(v_line);
                canv.Children.Add(h_line);
            }));
        }

        public void DrawLine(double x1, double y1, double x2, double y2, int thickness, Brush color, double [] dash_array, int dash_offset)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double dx1 = x1 * ratio_w;
                double dy1 = y1 * ratio_h;

                double dx2 = x2 * ratio_w;
                double dy2 = y2 * ratio_h;

                Line line = new Line();
                line.Stroke = color;
                line.StrokeThickness = thickness;

                line.X1 = dx1;
                line.X2 = dx2;

                line.Y1 = dy1;
                line.Y2 = dy2;

                line.StrokeDashOffset = dash_offset;

                foreach (var d in dash_array)
                    line.StrokeDashArray.Add(d);

                canv.Children.Add(line);
            }));
        }

        #endregion

        #region Shape Methods

        private void SetMouseCursor()
        {
            Cursor cursor = Cursors.Arrow;

            switch (this.hitType)
            {
                case HitType.None: cursor = Cursors.Arrow; break;
                case HitType.Body: cursor = Cursors.ScrollAll; break;
                case HitType.UpperLeft:
                case HitType.LowerRight: cursor = Cursors.SizeNWSE; break;
                case HitType.LowerLeft:
                case HitType.UpperRight: cursor = Cursors.SizeNESW; break;
                case HitType.Top:
                case HitType.Bottom: cursor = Cursors.SizeNS; break;
                case HitType.Left:
                case HitType.Right: cursor = Cursors.SizeWE; break;
            }

            if (Cursor != cursor)
            {
                Cursor = cursor;
            }
        }

        private HitType GetHitType(Shape shape, Point point)
        {
            double left;
            double right;
            double top;
            double bottom;

            SetLRTB(shape, out left, out right, out top, out bottom);

            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;
            if (point.Y < top) return HitType.None;
            if (point.Y > bottom) return HitType.None;

            const double GAP = 3;

            if (point.X - left < GAP)
            {
                if (point.Y - top < GAP)
                {
                    return HitType.UpperLeft;
                }

                if (bottom - point.Y < GAP)
                {
                    return HitType.LowerLeft;
                }

                return HitType.Left;
            }

            if (right - point.X < GAP)
            {
                if (point.Y - top < GAP)
                {
                    return HitType.UpperRight;
                }

                if (bottom - point.Y < GAP)
                {
                    return HitType.LowerRight;
                }

                return HitType.Right;
            }

            if (point.Y - top < GAP)
            {
                return HitType.Top;
            }

            if (bottom - point.Y < GAP)
            {
                return HitType.Bottom;
            }

            return HitType.Body;
        }
        
        private void FindHit(Point point)
        {
            this.hitShape = null;
            this.hitType = HitType.None;

            //SetShapeHitList();

            //var list = roiRectInfos.Select(s => s.shape_rect).ToArray();

            //foreach (Shape shape in list)
            for (int i = 0; i < canv.Children.Count; i++)
            {
                if(canv.Children[i] is Rectangle)
                {
                    var shape = canv.Children[i] as Rectangle;
                    this.hitType = GetHitType(shape, point);

                    if (this.hitType != HitType.None)
                    {
                        this.hitShape = shape;

                        return;
                    }
                }

            }

            return;
        }
        
        private void FindClickHit(Point point)
        {
            this.clickHitShape = null;
            this.hitType = HitType.None;

            //SetShapeClickHitList();
            //var list = roiRectInfos.Select(s => s.shape_rect).ToArray();

            //foreach (Shape shape in list)
            for (int i = 0; i < canv.Children.Count; i++)
            {
                if (canv.Children[i] is Rectangle)
                {
                    var shape = canv.Children[i] as Shape;
                    shape.StrokeDashArray = null;
                }
            }

            for (int i = 0; i < canv.Children.Count; i++)
            {
                if(canv.Children[i] is Rectangle)
                {
                    var shape = canv.Children[i] as Shape;
                    this.hitType = GetHitType(shape, point);
                    shape.StrokeDashArray = null;
                    if (this.hitType != HitType.None)
                    {
                        shape.StrokeDashArray = new DoubleCollection { 6, 1 };
                        this.clickHitShape = shape;

                        if (selectedRectangleEvent != null)
                        {
                            if(roiRectInfos.Count > 0)
                            {
                                var find = roiRectInfos.First(f => f.shape_rect == shape);
                                if(find != null)
                                    selectedRectangleEvent(find);
                            }
                        }
                        return;
                    }
                }

            }

            return;
        }

        private void SetLRTB(Shape shape, out double left, out double right, out double top, out double bottom)
        {
            if (!(shape is Polygon))
            {
                left = Canvas.GetLeft(shape);
                top = Canvas.GetTop(shape);
                right = left + shape.ActualWidth;
                bottom = top + shape.ActualHeight;

                return;
            }

            Polygon polygon = shape as Polygon;

            left = polygon.Points[0].X;
            right = left;
            top = polygon.Points[0].Y;
            bottom = top;

            foreach (Point point in polygon.Points)
            {
                if (left > point.X)
                {
                    left = point.X;
                }

                if (right < point.X)
                {
                    right = point.X;
                }

                if (top > point.Y)
                {
                    top = point.Y;
                }

                if (bottom < point.Y)
                {
                    bottom = point.Y;
                }
            }

            left += Canvas.GetLeft(shape);
            right += Canvas.GetLeft(shape);
            top += Canvas.GetTop(shape);
            bottom += Canvas.GetTop(shape);
        }

        public void RegisterSelectedRectangleEvent(SelectedRectangle selectedRectangleEvent)
        {
            this.selectedRectangleEvent += selectedRectangleEvent;
        }

        public void RegisterImageInformationEvent(MouseMoveImageInformation mouseMoveImageInformation)
        {
            this.OnMouseMoveImageInformation += mouseMoveImageInformation;
        }

        public void RemoveAllShape()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (canv.Children.Count > 3)
                {
                    canv.Children.RemoveRange(3, canv.Children.Count - 3);
                    roiRectInfos = new ObservableCollection<RoiRectInfo>();
                }
            }));
        }

        public void RemoveShapes()
        {

        }

        #endregion

        #region ROI Rectangle draw
        public void SetRectangle(int idx, int x, int y, int width, int height, Color color, ROITypes roi_type, int roi_index)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (BitmapSource == null)
                    return;

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                //rect.StrokeDashArray = new DoubleCollection { 6, 1 };
                rect.Stroke = new SolidColorBrush(ROIColors.GetColor(idx));

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.shape_rect = rect;
                info.ROIIndex = roi_index;
                info.roi_rect = new RoiRectangle(y, x, y + height, x + width, color, roi_type);
                info.roi_rect.RoiIndex = roi_index;

                roiRectInfos.Add(info);
                //
                canv.Children.Add(rect);

                //double daw = (x + TopLeft.X) * ratio_w;
                //double dah = (y + TopLeft.Y) * ratio_h;

                double daw = x  * ratio_w + TopLeft.X;
                double dah = y  * ratio_h + TopLeft.Y;

                Canvas.SetLeft(rect, (int)daw);
                Canvas.SetTop(rect, (int)dah);
            }));

        }
        public void SetRectangle(RoiRectangle roiRectangle)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (BitmapSource == null)
                    return;

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                //rect.StrokeDashArray = new DoubleCollection { 6, 1 };
                rect.Stroke = new SolidColorBrush(roiRectangle.Color);

                rect.Width = roiRectangle.Size.Width * ratio_w;
                rect.Height = roiRectangle.Size.Height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.ROIIndex = roiRectangle.RoiIndex;
                info.shape_rect = rect;
                info.roi_rect = roiRectangle.Duplicate();
                roiRectInfos.Add(info);
                //
                canv.Children.Add(rect);

                double daw = roiRectangle.Left * ratio_w;
                double dah = roiRectangle.Top * ratio_h;

                Canvas.SetLeft(rect, (int)daw);
                Canvas.SetTop(rect, (int)dah);
            }));
        }
        public void AddROI()
        {
            Dispatcher.Invoke(new Action(delegate
            {

                if (ROIColors.strColors.Count() - 1 == ROIRectCount)
                    return;

                if (BitmapSource == null)
                    return;

                Random rnd = new Random();

                int width = (int)PixelWidth / 7;
                int height = (int)PixelHeight / 7;

                int x = rnd.Next((int)PixelWidth - width);
                int y = rnd.Next((int)PixelHeight - height);

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double daw = x * ratio_w;
                double dah = y * ratio_h;

                int idx = ROIRectCount;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                rect.Stroke = new SolidColorBrush(ROIColors.GetColor(idx));

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.ROIIndex = idx;
                info.shape_rect = rect;
                info.roi_rect = new RoiRectangle(y, x, y + height, x + width, ROIColors.GetColor(idx));
                info.roi_rect.RoiIndex = idx;
                roiRectInfos.Add(info);
                //

                canv.Children.Add(rect);

                Canvas.SetLeft(rect, daw);
                Canvas.SetTop(rect, dah);
            }));

        }
        public void AddROI(int index)
        {
            Dispatcher.Invoke(new Action(delegate
            {

                if (ROIColors.strColors.Count() - 1 == ROIRectCount)
                    return;

                if (BitmapSource == null)
                    return;

                Random rnd = new Random();

                int width = (int)PixelWidth / 7;
                int height = (int)PixelHeight / 7;

                int x = rnd.Next((int)PixelWidth - width);
                int y = rnd.Next((int)PixelHeight - height);

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double daw = x * ratio_w;
                double dah = y * ratio_h;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                rect.Stroke = new SolidColorBrush(ROIColors.GetColor(index));

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.shape_rect = rect;
                info.roi_rect = new RoiRectangle(y, x, y + height, x + width, ROIColors.GetColor(index));
                info.roi_rect.RoiIndex = index;
                info.ROIIndex = index;
                roiRectInfos.Add(info);
                //

                canv.Children.Add(rect);

                Canvas.SetLeft(rect, daw);
                Canvas.SetTop(rect, dah);
            }));

        }
        public void AddROI(int index, int width, int height)
        {

            Dispatcher.Invoke(new Action(delegate
            {

                if (ROIColors.strColors.Count() - 1 == ROIRectCount)
                    return;

                if (BitmapSource == null)
                    return;

                Random rnd = new Random();



                int x = rnd.Next((int)PixelWidth - width);
                int y = rnd.Next((int)PixelHeight - height);

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double daw = x * ratio_w;
                double dah = y * ratio_h;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                rect.Stroke = new SolidColorBrush(ROIColors.GetColor(index));

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.ROIIndex = index;
                info.shape_rect = rect;
                info.roi_rect = new RoiRectangle(y, x, y + height, x + width, ROIColors.GetColor(index));
                info.roi_rect.RoiIndex = index;
                info.ROIIndex = index;
                roiRectInfos.Add(info);
                //

                canv.Children.Add(rect);

                Canvas.SetLeft(rect, daw);
                Canvas.SetTop(rect, dah);
            }));

        }
        public void AddROI(RoiRectangle roi)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (ROIColors.strColors.Count() - 1 == ROIRectCount)
                    return;

                if (BitmapSource == null)
                    return;

                 Random rnd = new Random();

                int width = roi.Width;
                int height = roi.Height;
                int index = roi.RoiIndex;

                int x = roi.Left + (int)TopLeft.X;
                int y = roi.Top + (int)TopLeft.Y;

                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                double daw = x * ratio_w;
                double dah = y * ratio_h;

                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Transparent;
                rect.StrokeThickness = 2;
                rect.Stroke = new SolidColorBrush(ROIColors.GetColor(index));

                rect.Width = width * ratio_w;
                rect.Height = height * ratio_h;

                // roi rect info
                RoiRectInfo info = new RoiRectInfo();
                info.shape_rect = rect;
                info.roi_rect = roi.Duplicate();
                info.roi_rect.RoiIndex = index;
                info.ROIIndex = index;
                roiRectInfos.Add(info);
                //

                canv.Children.Add(rect);

                Canvas.SetLeft(rect, daw);
                Canvas.SetTop(rect, dah);
            }));

        }

        public void RemoveSelectedROI()
        {
            canv.Children.Remove(clickHitShape);

            if (roiRectInfos.Count > 0)
            {
                var item = roiRectInfos.First(s => s.shape_rect == clickHitShape);
                if (item != null)
                    roiRectInfos.Remove(item);
            }
            //rectList.Remove(clickHitShape as Rectangle);
        }
        public void RemoveAllROI()
        {
            foreach (var info in roiRectInfos)
            {
                canv.Children.Remove(info.shape_rect);
            }
            roiRectInfos = new ObservableCollection<RoiRectInfo>();
        }
        public RoiRectangle GetRoiRectangle(ref Rectangle rect)
        {
            var shape = rect as Shape;
            var info = roiRectInfos.First(f => f.shape_rect == shape);
            if (info != null)
            {
                return info.roi_rect;
            }
            return null;

            //int idx = canv.Children.IndexOf(rect);

            //var child = canv.Children[idx] as Rectangle;

            //double ratio_w = ImageActualWidth / PixelWidth;
            //double ratio_h = ImageActualHeight / PixelHeight;

            //double l = Canvas.GetLeft(child) / ratio_w;
            //double t = Canvas.GetTop(child) / ratio_h;

            //left  = (int)(Canvas.GetLeft(child) / ratio_w);
            //top = (int)(Canvas.GetTop(child) / ratio_h);
            //width = (int)(rect.Width / ratio_w);
            //height = (int)(rect.Height / ratio_h);
        }
        public void SetRoiRectangle()
        {
            if(clickHitShape != null)
            {
                double ratio_w = ImageActualWidth / PixelWidth;
                double ratio_h = ImageActualHeight / PixelHeight;

                int left = (int)Canvas.GetLeft(clickHitShape);
                int top = (int)Canvas.GetTop(clickHitShape);

                var find = roiRectInfos.FirstOrDefault(f => f.shape_rect == clickHitShape);
                if(find != null)
                {
                    left = (int)(left / ratio_w);
                    top = (int)(top / ratio_h);

                    find.roi_rect.TopLeft.X = left;
                    find.roi_rect.TopLeft.Y = top;

                    find.roi_rect.Size.Width = (int)(clickHitShape.Width / ratio_w);
                    find.roi_rect.Size.Height = (int)(clickHitShape.Height / ratio_h);

                    Color color = find.roi_rect.Color;
                }
            }
        }

        public RoiRectangle [] GetRoiRectangles(int index)
        {
            return roiRectInfos.Where(w => w.ROIIndex == index).Select(s=>s.roi_rect).ToArray();
        }

        public RoiRectangle [] GetAllRoiRectangles()
        {
            return roiRectInfos.Select(s => s.roi_rect).ToArray();
        }

        #endregion

        public void SetByteImage(Project.BaseLib.DataStructures.ByteImage image)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                RemoveAllROI();
                if (image == null)
                    BitmapSource = null;
                else
                    BitmapSource = image.BitmapSource;
            }));
        }

        public ByteImage GetByteImage()
        {
            BitmapSource CurrentSource = BitmapSource as BitmapSource;

            var width = CurrentSource.PixelWidth;
            var height = CurrentSource.PixelHeight;

            ByteImage image = new ByteImage(width, height);

            int Stride = (CurrentSource.PixelWidth * CurrentSource.Format.BitsPerPixel + 7) / 8;

            CurrentSource.CopyPixels(
                new Int32Rect(0, 0, width-1, height-1), image.Data, Stride, 0);

            return image;
        }
    }
}
