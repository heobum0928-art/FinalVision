using ImageGlass;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Extension;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

namespace Project.UI.Common
{

    /// <summary>
    /// ImageViewEx.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageViewEx : UserControl
    {
        #region fields
        private ImageBoxExEx _IGImageView;   // ImageGlass Bottom Object

        private bool _IsEnablePixelInfo;

        private bool _IsEnableWheelZoom;

        private bool _IsEnableSelected;

        private bool _IsEnableZoomToFit;

        private List<DrawableShape> _ShapeList;

        private DrawableShape _selectedShape = null; // 현재 선택된(이동할) 도형
        private bool _isDragging = false;      // 드래그 상태 여부
        private System.Drawing.Point _lastMousePos; // 직전 마우스 위치 저장
        private HitLocation _selectedHitLocation = HitLocation.None;


        // Center Line
        private bool _IsEnableCenterLine;

        private int _CenterLineLength;

        private System.Drawing.Pen _CenterLinePen;

        // Center Circle
        private bool _IsEnableCenterCircle;

        private int _CenterCircleRadius;

        private System.Drawing.Pen _CenterCirclePen;

        #endregion

        #region propertise
        // Center Line
        public bool IsEnableCenterLine
        {
            get
            {
                return _IsEnableCenterLine;
            }

            set
            {
                _IsEnableCenterLine = value;
                _IGImageView.Invalidate();
            }
        }

        public int CenterLineLength
        {
            get
            {
                return _CenterLineLength;
            }

            set
            {
                _CenterLineLength = value;
                _IGImageView.Invalidate();
            }
        }

        // Center Circle
        public bool IsEnableCenterCircle
        {
            get
            {
                return _IsEnableCenterCircle;
            }

            set
            {
                _IsEnableCenterCircle = value;
                _IGImageView.Invalidate();
            }
        }

        public int CenterCircleRadius
        {
            get
            {
                return _CenterCircleRadius;
            }

            set
            {
                _CenterCircleRadius = value;
                _IGImageView.Invalidate();
            }
        }
        
        public string ViewName
        {
            get
            {
                return _IGImageView.Name;
            }

            set
            {
                _IGImageView.Name = value;
            }
        }

        public bool AllowClickZoom
        {
            get
            {
                return _IGImageView.AllowClickZoom;
            }

            set
            {
                _IGImageView.AllowClickZoom = value;
            }
        }

        public ImageBoxScrollBarStyle VerticalScrollBarStyle
        {
            get
            {
                return _IGImageView.VerticalScrollBarStyle;
            }

            set
            {
                _IGImageView.VerticalScrollBarStyle = value;
            }
        }

        public ImageBoxScrollBarStyle HorizontalScrollBarStyle
        {
            get
            {
                return _IGImageView.HorizontalScrollBarStyle;
            }

            set
            {
                _IGImageView.HorizontalScrollBarStyle = value;
            }
        }

        public bool ShowPixelGrid
        {
            get
            {
                return _IGImageView.ShowPixelGrid;
            }

            set
            {
                _IGImageView.ShowPixelGrid = value;
            }
        }

        public bool IsEnablePixelInfo
        {
            get
            {
                return _IsEnablePixelInfo;
            }

            set
            {
                _IsEnablePixelInfo = value;
            }
        }

        public bool IsEnableWheelZoom
        {
            get
            {
                return _IsEnableWheelZoom;
            }

            set
            {
                //if(_IsEnableZoomToFit == true)
                //    _IsEnableWheelZoom = false;
                //else
                 _IsEnableWheelZoom = value;

                //if(_IsEnableZoomToFit == true)
                //    _IGImageView.ZoomToFit();
            }
        }

        public bool IsEnableZoomToFit
        {
            //get
            //{
            //    return _IsEnableZoomToFit;
            //}

            set
            {
                _IsEnableZoomToFit = value;

                if(_IsEnableZoomToFit == true)
                {
                    //_IsEnableWheelZoom = false;

                    _IGImageView.ZoomToFit();
                }
            }
        }

        public bool IsEnableSelected
        {
            get
            {
                return _IsEnableSelected;
            }

            set
            {
                if(_IsEnableSelected != value)
                {
                    _IsEnableSelected = value;

                    foreach (var s in _ShapeList)
                        s.IsSelected = false;

                    _IGImageView.Invalidate();
                }
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return _IGImageView.Image;
            }

            set
            {
                _IGImageView.Image = value;

                RemoveAllShape();

                if (_IsEnableZoomToFit == true)
                {

                    _IGImageView.ZoomToFit();
                    _IGImageView.CenterToImage();
                }
            }
        }

        public System.Drawing.Size ImageSize
        {
            get
            {
                if (_IGImageView.Image == null)
                    return new System.Drawing.Size(0, 0);
                
                return new System.Drawing.Size(_IGImageView.Image.Width, _IGImageView.Image.Height);
            }
        }

        public DPointCoordinates ImageCenter
        {
            get
            {
                if (_IGImageView.Image == null)
                    return new DPointCoordinates(0, 0);

                return new DPointCoordinates(_IGImageView.Image.Width / 2.0, _IGImageView.Image.Height / 2.0);
            }
        }
        #endregion

        #region methods

        public void SetImage(System.Drawing.Image Image)
        {
            if (_IGImageView.Image != null)
                _IGImageView.Image.Dispose();

            _IGImageView.Image = Image;

            if (_IsEnableZoomToFit == true)
            {

                _IGImageView.ZoomToFit();
                _IGImageView.CenterToImage();
            }

        }

        public void SetImage(ByteImage Image)
        {
            var data = Image.Data;

            // 1. 8비트 인덱스 형식의 비트맵 생성
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Image.Width, Image.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            // 2. 흑백 팔레트 설정 (반드시 필요)
            System.Drawing.Imaging.ColorPalette palette = bmp.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }
            bmp.Palette = palette;

            // 3. 데이터 복사 (Stride 고려)
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;

            // 만약 width가 4의 배수라면 한 번에 복사 가능하지만, 
            // 안전을 위해 줄 단위로 복사하는 것이 좋습니다.
            for (int y = 0; y < Image.Height; y++)
            {
                // 원본 배열의 시작점 위치 계산
                int offset = y * Image.Width;
                // 비트맵 메모리의 시작점 위치로 데이터 복사
                System.Runtime.InteropServices.Marshal.Copy(data, offset, IntPtr.Add(ptr, y * stride), Image.Width);
            }

            bmp.UnlockBits(bmpData);

            _IGImageView.Image = bmp;

            if (_IsEnableZoomToFit == true)
            {

                _IGImageView.ZoomToFit();
                _IGImageView.CenterToImage();
            }

            //var data = Image.Data;

            //var format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            //// 1. 비트맵 객체 생성
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Image.Width, Image.Height, format);

            //// 2. 비트맵의 메모리 영역에 직접 쓰기 위해 Lock 적용
            //System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
            //    new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height),
            //    System.Drawing.Imaging.ImageLockMode.WriteOnly,
            //    format);

            //// 3. 바이트 배열의 데이터를 비트맵 메모리로 복사
            //System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            //// 4. Lock 해제
            //bmp.UnlockBits(bmpData);

            //_IGImageView.Image = bmp;
        }

        public System.Drawing.Image GetDrawingImage()
        {
            if(_IGImageView.Image != null)
            {
                return (System.Drawing.Image)_IGImageView.Image.Clone();
            }

            return null;
        }

        public Bitmap GetBitmapImage()
        {
            if (_IGImageView.Image != null)
            {
                return _IGImageView.Image.Clone() as Bitmap;
            }
            return null;
        }

        public ByteImage GetByteImage()
        {
            ByteImage byte_image = null;
            if (_IGImageView.Image != null)
            {
                var bytes = GetBitmapImage().ImageToByte();
                var size = ImageSize;


                byte_image = new ByteImage(size.Width, size.Height, size.Width, bytes, 0);
            }

            return byte_image;
        }
        
        private PointF CtrlToImg(System.Drawing.Point pt)
        {
            return _IGImageView.PointToImage(pt);
        }

        public async Task<bool> SaveImageAsync(string filePath, ImageSaveTypes SaveType = ImageSaveTypes.OnlyImage)
        {
            return await Task.Run(() =>
            {
                return SaveImage(filePath, SaveType);
            });
        }
        
        public bool SaveImage(string filePath, ImageSaveTypes SaveType = ImageSaveTypes.OnlyImage)
        {
            if (_IGImageView.Image == null)
                return false;

            var _size = ImageSize;

            using (Bitmap bmp = new Bitmap(_size.Width, _size.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // 2. 고품질 렌더링 설정
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    // 3. 배경 원본 이미지 그리기
                    g.DrawImage(_IGImageView.Image, 0, 0, _size.Width, _size.Height);

                    if(SaveType == ImageSaveTypes.WithShapes)
                    {
                        // 4. 그 위에 도형 그리기
                        // 이때 zoomFactor는 1.0(100%)으로 전달해야 원본 좌표 그대로 그려집니다.
                        foreach (var s in _ShapeList)
                        {
                            // ROI_Paint에서 사용하던 Draw 메서드를 그대로 호출
                            // 원본 해상도에 그리는 것이므로 배율 보정 없이(1.0f) 그립니다.
                            s.Draw(g);
                        }

                        if (_IsEnableCenterLine == true)
                        {
                            // Horz
                            g.DrawLine(_CenterLinePen, 0, (float)ImageCenter.Y, ImageSize.Width - 1, (float)ImageCenter.Y);

                            // Vert
                            g.DrawLine(_CenterLinePen, (float)ImageCenter.X, 0, (float)ImageCenter.X, ImageSize.Height - 1);
                        }

                        if (_IsEnableCenterCircle == true)
                        {
                            var rectf = new RectangleF((float)(ImageCenter.X - _CenterCircleRadius), (float)(ImageCenter.Y - _CenterCircleRadius), _CenterCircleRadius * 2, _CenterCircleRadius * 2);
                            g.DrawEllipse(_CenterCirclePen, rectf);
                        }
                    }
                }
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            return true;                       
        }

        #endregion

        #region Events
        private void ImageViewMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_IsEnableWheelZoom != true)
                return;

            ImageBoxEx IGbox = (sender as ImageBoxEx);

            IGbox.ZoomWithMouseWheel(e.Delta, e.Location);

            IGbox.Invalidate();
        }
        private void ImageViewMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_IsEnablePixelInfo != true)
                return;

            ImageBoxEx IGbox = (sender as ImageBoxEx);
            Bitmap bmp = IGbox.Image as Bitmap;

            if (bmp == null)
                return;

            System.Drawing.Point wPT = new System.Drawing.Point();  // wPT is Client Postion. System.Windows.Point -> System.Drawing.Point
            wPT.X = e.X;
            wPT.Y = e.Y;

            System.Drawing.Point iPT = new System.Drawing.Point();  // iPT is Image Position. System.Windows.Point -> System.Drawing.Point
            iPT = IGbox.PointToImage(wPT, true);

            System.Drawing.Color c = bmp.GetPixel(iPT.X, iPT.Y);

            string str = "Pixel Point:" + "(" + iPT.X.ToString() + "," + iPT.Y.ToString() + ")" + "\r\n"
                + "(R,G,B:" + c.R + "," + c.G + "," + c.B + ")";
                                   
            IGbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;          // 11.01
            IGbox.TextDisplayMode = ImageBoxGridDisplayMode.Image;
            IGbox.TextBackColor = System.Drawing.Color.Beige;

            IGbox.Text = str;
            //IGbox.Text = str + "\r\n" + GetXYCenterTomm(iPT.X, iPT.Y);    // 12.17 Insert (mouse mm Coordination)
            IGbox.Refresh();
            // 07.04 Modify End
        }
        private void ImageViewMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var box = (ImageGlass.ImageBoxEx)sender;
            if (box.Image == null) return;
                       
            // 화면 좌표를 이미지 좌표로 변환
            System.Drawing.Point imgPoint = box.PointToImage(e.Location);
            using (System.Drawing.Pen hitTestPen = new System.Drawing.Pen(System.Drawing.Color.Black, 10f))
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    foreach (var s in _ShapeList)
                    {
                        if(s.ClickSelect(new DPointCoordinates(imgPoint.X, imgPoint.Y)))
                        {
                            foreach (var s1 in _ShapeList) s1.IsSelected = false;

                            s.IsSelected = true;
                            break; // 가장 위에 있는 도형 하나만 선택
                        }
                    }
                }
            }
            // 3. 화면 갱신
            box.Invalidate();
        }

        private void LineHit_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_IsEnableSelected != true) return;

            ImageBoxEx IGbox = (sender as ImageBoxEx);
            System.Drawing.Point wPT = new System.Drawing.Point(e.X, e.Y);
            System.Drawing.Point iPT = IGbox.PointToImage(wPT, true);

            System.Windows.Forms.Cursor newCursor = System.Windows.Forms.Cursors.Arrow; // 기본값 설정
            bool hitFound = false;

            var is_selected = _ShapeList.All(a => a.IsSelected == false);
            if (is_selected == true)
            {
                this.Cursor = Cursors.Arrow;
                return;
            }

            foreach (var s in _ShapeList)
            {
                if (s.IsSelected)
                {
                    // 최적화: 모든 Enum을 도는 대신 해당 도형의 HitTest 메서드를 호출하는 것이 좋습니다.
                    foreach (HitLocation location in Enum.GetValues(typeof(HitLocation)))
                    {
                        if (location == HitLocation.None) continue;

                        var hit_rect = s.GetHitRectPosition(location);
                        if (hit_rect.Contains(iPT.X, iPT.Y)) // .Contains 메서드 활용
                        {
                            s.LastHit = location;
                            newCursor = GetCursorForLocation(location);
                            hitFound = true;
                            break;
                        }

                        if (s.ToRect().Contains(iPT.X, iPT.Y))
                        {
                            s.LastHit = HitLocation.Body;
                            newCursor = System.Windows.Forms.Cursors.Hand;
                            hitFound = true;
                        }
                    }
                    if (hitFound) break;
                }
            }

            if (IGbox.Cursor != newCursor)
            {
                IGbox.Cursor = newCursor;
            }
        }

        // 가독성을 위한 커서 매핑 함수 분리
        private System.Windows.Forms.Cursor GetCursorForLocation(HitLocation location)
        {
            switch (location)
            {
                case HitLocation.LeftTop:
                case HitLocation.RightBottom: return System.Windows.Forms.Cursors.SizeNWSE;
                case HitLocation.LeftBottom:
                case HitLocation.RightTop: return System.Windows.Forms.Cursors.SizeNESW;
                case HitLocation.Top:
                case HitLocation.Bottom: return System.Windows.Forms.Cursors.SizeNS;
                case HitLocation.Left:
                case HitLocation.Right: return System.Windows.Forms.Cursors.SizeWE;
                default: return System.Windows.Forms.Cursors.Arrow;
            }
        }
        
        private void ROI_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_IsEnableSelected != true) return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            var IGbox = sender as ImageBoxExEx;
            // 화면 좌표를 이미지 좌표로 변환
            System.Drawing.Point iPT = IGbox.PointToImage(e.Location, true);

            _selectedShape = null;
            var mouse_pt =  new DPointCoordinates(iPT.X, iPT.Y);

            foreach (var s1 in _ShapeList) s1.IsSelected = false;

            // 역순으로 탐색 (가장 위에 그려진 도형부터 잡기 위함)
            for (int i = _ShapeList.Count - 1; i >= 0; i--)
            {
                //if (_ShapeList[i].ToRect().Contains(iPT.X, iPT.Y))
                _selectedHitLocation = _ShapeList[i].IsHitRectPosition(mouse_pt);

                if (_ShapeList[i].ClickSelect(mouse_pt) || _selectedHitLocation != HitLocation.None)
                {


                    _ShapeList[i].IsSelected = true;

                    _selectedShape = _ShapeList[i];
                    _isDragging = true;
                    _lastMousePos = iPT; // 현재 이미지 좌표 저장

                    IGbox.AutoPan = false;
                    break;
                }
            }
        }

        private void ROI_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var IGbox = sender as ImageBoxExEx;
            System.Drawing.Point iPT = IGbox.PointToImage(e.Location, true);


            if (_isDragging && _selectedShape != null)
            {


                if( iPT.X >0 && 
                    iPT.Y >0 &&
                    iPT.X < IGbox.Image.Size.Width &&
                    iPT.Y < IGbox.Image.Size.Height)
                {
                    // 1. 이동 거리 계산 (이미지 좌표 기준)
                    float dx = iPT.X - _lastMousePos.X;
                    float dy = iPT.Y - _lastMousePos.Y;


                //AppLogger.Info()("ROI_MouseMove : iPT.X[{0}], iPT.Y[{1}], _lastMousePos.X[{2}], _lastMousePos.Y[{3}].",
                    //iPT.X, iPT.Y,
                    //_lastMousePos.X, _lastMousePos.Y);

                    //if(dx > 0 && dy > 0)
                    //if (iPT.X > 0 &&
                    //    iPT.Y > 0 &&
                    //    iPT.X < IGbox.Image.Size.Width &&
                    //    iPT.Y < IGbox.Image.Size.Height)
                    {
                        _selectedShape.Move(dx, dy, _selectedHitLocation);

                        // 3. 현재 좌표를 다음 계산을 위해 저장
                        _lastMousePos = iPT;
                    }

                    // 4. 화면 다시 그리기
                    IGbox.Invalidate();
                }
            }
            //else
            //{
            //    // 이전 단계에서 만든 커서 변경 로직(Hand/Arrow)을 여기에 배치합니다.
            //    UpdateCursorLogic(IGbox, iPT);
            //}
        }

        private void ROI_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var IGBox = sender as ImageBoxExEx;
            _isDragging = false;
            _selectedShape = null;

            IGBox.AutoPan = true;

            _selectedHitLocation = HitLocation.None;
        }

        private void ROI_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var box = (ImageGlass.ImageBoxEx)sender;
            if (box.Image == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. 배율 계산
            float zoomFactor = (float)box.Zoom / 100f;

            // 2. 줌이 적용된 실제 이미지의 크기 계산
            float displayedWidth = box.Image.Width * zoomFactor;
            float displayedHeight = box.Image.Height * zoomFactor;

            // 3. 중앙 정렬 오프셋 계산 (컨트롤 크기와 비교)
            float offsetX = 0;
            float offsetY = 0;

            if (box.Width > displayedWidth)
            {
                // 이미지가 컨트롤보다 작으면 빈 공간의 절반만큼 이동
                offsetX = (box.Width - displayedWidth) / 2f;
            }
            else
            {
                // 이미지가 컨트롤보다 크면 스크롤 위치를 따름
                offsetX = box.AutoScrollPosition.X;
            }

            if (box.Height > displayedHeight)
            {
                offsetY = (box.Height - displayedHeight) / 2f;
            }
            else
            {
                offsetY = box.AutoScrollPosition.Y;
            }

            // 4. 행렬 적용
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();

            // [순서 중요] 먼저 중앙 정렬/스크롤 위치로 이동 후 배율 적용
            matrix.Translate(offsetX, offsetY);
            matrix.Scale(zoomFactor, zoomFactor);

            g.Transform = matrix;

            box.BeginUpdate();

            // 5. 도형 그리기
            foreach (var s in _ShapeList)
            {
                s.Draw(g);
            }
            
            if(_IsEnableCenterLine == true)
            {
                // Horz
                g.DrawLine(_CenterLinePen, 0, (float)ImageCenter.Y, ImageSize.Width - 1, (float)ImageCenter.Y);

                // Vert
                g.DrawLine(_CenterLinePen, (float)ImageCenter.X, 0, (float)ImageCenter.X, ImageSize.Height -1);
            }

            if(_IsEnableCenterCircle == true)
            {
                var rectf = new RectangleF((float)(ImageCenter.X - _CenterCircleRadius), (float)(ImageCenter.Y - _CenterCircleRadius), _CenterCircleRadius * 2, _CenterCircleRadius * 2);
                g.DrawEllipse(_CenterCirclePen, rectf);
            }


            box.EndUpdate();

            g.ResetTransform();
        }

        private void UpdateCursorLogic(ImageGlass.ImageBoxEx IGbox, System.Drawing.Point mouseLocation)
        {
            // 1. 화면 좌표를 이미지 좌표로 변환
            System.Drawing.Point iPT = IGbox.PointToImage(mouseLocation, true);

            // 2. 기본 커서 설정
            System.Windows.Forms.Cursor targetCursor = System.Windows.Forms.Cursors.Arrow;
            float zoomFactor = (float)IGbox.Zoom / 100f;
            float hitTolerance = 8f / zoomFactor; // 줌 배율을 고려한 테두리 감지 범위

            foreach (var s in _ShapeList)
            {
                var rect = s.ToRectF();

                // 3. 테두리(Edge) 위치 판별 로직
                bool isLeft = Math.Abs(iPT.X - rect.Left) <= hitTolerance && iPT.Y >= rect.Top && iPT.Y <= rect.Bottom;
                bool isRight = Math.Abs(iPT.X - rect.Right) <= hitTolerance && iPT.Y >= rect.Top && iPT.Y <= rect.Bottom;
                bool isTop = Math.Abs(iPT.Y - rect.Top) <= hitTolerance && iPT.X >= rect.Left && iPT.X <= rect.Right;
                bool isBottom = Math.Abs(iPT.Y - rect.Bottom) <= hitTolerance && iPT.X >= rect.Left && iPT.X <= rect.Right;

                // 4. 위치에 따른 커서 결정 (우선순위: 모서리 > 변 > 몸체)
                if ((isLeft && isTop) || (isRight && isBottom))
                    targetCursor = System.Windows.Forms.Cursors.SizeNWSE;
                else if ((isRight && isTop) || (isLeft && isBottom))
                    targetCursor = System.Windows.Forms.Cursors.SizeNESW;
                else if (isLeft || isRight)
                    targetCursor = System.Windows.Forms.Cursors.SizeWE;
                else if (isTop || isBottom)
                    targetCursor = System.Windows.Forms.Cursors.SizeNS;
                else if (rect.Contains(iPT))
                    targetCursor = System.Windows.Forms.Cursors.Hand; // 몸체 위에서는 손가락 모양

                if (targetCursor != System.Windows.Forms.Cursors.Arrow) break;
            }

            // [중요] 깜빡임 방지: 현재 커서와 다를 때만 강제 적용
            if (System.Windows.Forms.Cursor.Current != targetCursor)
            {
                IGbox.Cursor = targetCursor;
                System.Windows.Forms.Cursor.Current = targetCursor;
            }
        }
        #endregion

        #region Draw function
        public void RemoveSelectedShape()
        {
            var item = _ShapeList.FirstOrDefault(f => f.IsSelected == true);
            if (item != null)
            {
                _ShapeList.Remove(item);
                _IGImageView.Invalidate();

                item.Dispose();
                item = null;
            }
        }

        public bool RemoveNameShape(string name)
        {
            var item = _ShapeList.FirstOrDefault(f => f.ShapeName == name);
            if (item == null)
                return false;

            _ShapeList.Remove(item);

            _IGImageView.Invalidate();

            item.Dispose();
            item = null;

            return true;
        }

        public void RemoveAllShape()
        {
            if(_ShapeList != null && _ShapeList.Count > 0)
            {
                foreach(var _s in _ShapeList)
                {
                    _s.Dispose();
                }
            }

            _ShapeList = new List<DrawableShape>();

            _IGImageView.Invalidate();
        }

        public void DrawRectangle(
                                    System.Drawing.Pen pen, 
                                    System.Drawing.Brush brush,
                                    double x1, double y1, double x2, double y2)
        {
            if (_IGImageView.Image == null) return;

            DrawableRectangle rect = new DrawableRectangle(pen, brush, new DPointCoordinates(x1, y1), new DPointCoordinates(x2, y2));

            _ShapeList.Add(rect);

            _IGImageView.Invalidate();
        }

        public void DrawCircle(
                                System.Drawing.Pen pen,
                                System.Drawing.Brush brush,
                                double centerx, double centery, double radius)
        {
            if (_IGImageView.Image == null) return;

            DrawableCircle circle = new DrawableCircle(pen, brush, centerx, centery, radius);

            _ShapeList.Add(circle);

            _IGImageView.Invalidate();
        }

        public void DrawLine(System.Drawing.Pen pen, double x1, double y1, double x2, double y2)
        {
            if (_IGImageView.Image == null) return;

            DrawableLine line = new DrawableLine(pen, new DPointCoordinates(x1, y1), new DPointCoordinates(x2, y2));

            _ShapeList.Add(line);

            _IGImageView.Invalidate();
        }

        public void DrawCrossLine(System.Drawing.Pen pen, double x, double y, int length)
        {
            if (_IGImageView.Image == null) return;

            DrawableCrossLine cross = new DrawableCrossLine(pen, new DPointCoordinates(x, y), length);

            _ShapeList.Add(cross);

            _IGImageView.Invalidate();
        }

        public void DrawArrowLine(System.Drawing.Pen pen, double x1, double y1, double x2, double y2, ArrowDirect ArrowDir, int ArrowSize)
        {
            if (_IGImageView.Image == null) return;

            DrawableArrowLine arrow_line = new DrawableArrowLine(pen, new DPointCoordinates(x1, y1), new DPointCoordinates(x2, y2), ArrowDir, ArrowSize);

            _ShapeList.Add(arrow_line);

            _IGImageView.Invalidate();
        }

        public void DrawString(string name, string message,
            Font font, System.Drawing.Brush brush, 
            double x, double y, 
            StringAlignment Alignment = StringAlignment.Center, 
            StringAlignment LineAlignment = StringAlignment.Center)
        {
            if (_IGImageView.Image == null) return;

            DrawableString draw_string = new DrawableString(name, message, font, brush, x, y, Alignment, LineAlignment);

            _ShapeList.Add(draw_string);

            _IGImageView.Invalidate();
        }

        public RoiRectangle[] GetAllRoiRectangles()
        {
            List<RoiRectangle> rects = new List<RoiRectangle>();
            foreach (var shape in _ShapeList)
            {
                if(shape is DrawableRectangle)
                {
                    var shape_rect = shape as DrawableRectangle;

                    RoiRectangle rect = new RoiRectangle(shape_rect.ToRect().Top, shape_rect.ToRect().Left, shape_rect.ToRect().Bottom, shape_rect.ToRect().Right);

                    rects.Add(rect);
                }
            }
            return rects.ToArray();
        }

        #endregion

        #region constructors
        public ImageViewEx()
        {
            InitializeComponent();

            _IGImageView = new ImageBoxExEx();
            WindowHost.Child = _IGImageView;

            _IGImageView.MouseWheel += ImageViewMouseWheel;

            _IGImageView.MouseMove += ImageViewMouseMove;
            _IGImageView.MouseMove += LineHit_MouseMove;

            _IGImageView.MouseDown += ROI_MouseDown;
            _IGImageView.MouseMove += ROI_MouseMove;

            _IGImageView.MouseUp += ROI_MouseUp;
            _IGImageView.Paint += ROI_Paint;





            _IsEnablePixelInfo = false;
            _IsEnableWheelZoom = false;
            _IsEnableWheelZoom = false;
            _IsEnableZoomToFit = false;

            _ShapeList = new List<DrawableShape>();
            
            // Center Line
            _IsEnableCenterLine = false;

            _CenterLineLength = 5;

            _CenterLinePen = new System.Drawing.Pen(System.Drawing.Brushes.Aqua, 2);
            _CenterLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;

            // Center Circle
            _IsEnableCenterCircle = false;

            _CenterCircleRadius = 50;

            _CenterCirclePen = new System.Drawing.Pen(System.Drawing.Brushes.Aqua, 2);
            _CenterCirclePen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
        }
        #endregion
    }
}
