using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{

    public abstract class DrawableShape : IDrawableShape, IDisposable, INotifyPropertyChanged
    {
        #region fields
        protected string _ShapeName;


        public event PropertyChangedEventHandler PropertyChanged;
        
        protected Pen _Pen;

        protected Brush _Brush;

        protected DPointCoordinates _P1;

        protected DPointCoordinates _P2;

        protected bool _IsSelected;

        protected HitLocation _LastHit;

        protected const int _SelectedRectSize = 5;

        #endregion

        #region propertise
        public string ShapeName
        {
            get
            {
                return _ShapeName;
            }
        }

        public DPointCoordinates LeftTop
        {
            get
            {
                return _P1;
            }

            set
            {
                _P1 = value;
                OnPropertyChanged();
            }
        }

        public DPointCoordinates RightBottom
        {
            get
            {
                return _P2;
            }

            set
            {
                _P2 = value;
                OnPropertyChanged();
            }
        }

        public DPointCoordinates Center
        {
            get
            {
                var cx = (_P1.X + _P2.X) / 2.0;
                var cy = (_P1.Y + _P2.Y) / 2.0;
                return new DPointCoordinates(cx, cy);
            }
        }

        public double Width
        {
            get
            {
                if (_P1 == null || _P2 == null)
                    return 0;

                return (_P2.X - _P1.X);
            }
        }

        public double Height
        {
            get
            {
                if (_P1 == null || _P2 == null)
                    return 0;

                return (_P2.Y - _P1.Y);
            }
        }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }

            set
            {
                _IsSelected = value;
                OnPropertyChanged();
            }
        }

        public Pen Pen
        {
            get
            {
                return _Pen;
            }
        }

        public Brush Brush
        {
            get
            {
                return _Brush;
            }
        }

        public HitLocation LastHit
        {
            get
            {
                return _LastHit;
            }

            set
            {
                _LastHit = value;
                OnPropertyChanged();
            }

        
        }
        #endregion

        #region methods
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public virtual void Move(double x, double y, HitLocation location = HitLocation.None)
        {
            if (IsValid() == true)
            {
                DPointCoordinates _TP1;
                DPointCoordinates _TP2;

                switch (location)
                {
                    case HitLocation.None:
                        _TP1 = _P1.TransferOffset(x, y);
                        _TP2 = _P2.TransferOffset(x, y);

                        break;

                    case HitLocation.Left:
                        _TP1 = _P1.TransferOffset(x, 0);
                        _TP2 = _P2;

                        break;

                    case HitLocation.Top:
                        _TP1 = _P1.TransferOffset(0, y);
                        _TP2 = _P2;
                        break;

                    case HitLocation.Right:
                        _TP1 = _P1;
                        _TP2 = _P2.TransferOffset(x, 0);
                        break;

                    case HitLocation.Bottom:
                        _TP1 = _P1;
                        _TP2 = _P2.TransferOffset(0, y);
                        break;

                    case HitLocation.LeftTop:
                        _TP1 = _P1.TransferOffset(x, y);
                        _TP2 = _P2;
                        break;

                    case HitLocation.LeftBottom:
                        _TP1 = _P1.TransferOffset(x, 0);
                        _TP2 = _P2.TransferOffset(0, y);
                        break;

                    case HitLocation.RightTop:
                        _TP1 = _P1.TransferOffset(0, y);
                        _TP2 = _P2.TransferOffset(x, 0);
                        break;

                    case HitLocation.RightBottom:
                        _TP1 = _P1;
                        _TP2 = _P2.TransferOffset(x, y);
                        break;
                    default:

                        _TP1 = new DPointCoordinates();
                        _TP2 = new DPointCoordinates();
                        break;

                }

                Rectangle rect = new Rectangle((int)_TP1.X, (int)_TP1.Y, (int)(_TP2.X - _TP1.X), (int)(_TP2.Y - _TP1.Y));

                if (rect.Width < 1 || rect.Height < 1)
                    return;

                _P1 = _TP1;
                _P2 = _TP2;
            }


            //if (IsValid() == true)
            //{

            //    DPointCoordinates _TP1;
            //    DPointCoordinates _TP2;

            //    switch(location)
            //    {
            //        case HitLocation.None:
            //            _P1 = _P1.TransferOffset(x, y);
            //            _P2 = _P2.TransferOffset(x, y);

            //            break;

            //        case HitLocation.Left:
            //            _P1 = _P1.TransferOffset(x, 0);
            //            break;

            //        case HitLocation.Top:
            //            _P1 = _P1.TransferOffset(0, y);
            //            break;

            //        case HitLocation.Right:
            //            _P2 = _P2.TransferOffset(x, 0);
            //            break;

            //        case HitLocation.Bottom:
            //            _P2 = _P2.TransferOffset(0, y);
            //            break;

            //        case HitLocation.LeftTop:
            //            _P1 = _P1.TransferOffset(x, y);
            //            break;

            //        case HitLocation.LeftBottom:
            //            _P1 = _P1.TransferOffset(x, 0);
            //            _P2 = _P2.TransferOffset(0, y);
            //            break;

            //        case HitLocation.RightTop:
            //            _P1 = _P1.TransferOffset(0, y);
            //            _P2 = _P2.TransferOffset(x, 0);
            //            break;

            //        case HitLocation.RightBottom:
            //            _P2 = _P2.TransferOffset(x, y);
            //            break;

            //    }                
            //}
        }

        public bool IsValid()
        {
            if (_P1 != null && _P2 != null)
                return true;

            return false;
        }
        
        public virtual Rectangle ToRect()
        {
            return new Rectangle((int)_P1.X, (int)_P1.Y, (int)(_P2.X - _P1.X), (int)(_P2.Y - _P1.Y));
        }

        public virtual RectangleF ToRectF()
        {
            return new RectangleF((float)_P1.X, (float)_P1.Y, (float)(_P2.X - _P1.X), (float)(_P2.Y - _P1.Y));
        }
        
        public virtual void Dispose()
        {
            if(_Pen != null)
                _Pen.Dispose();

            if (_Brush != null)
                _Brush.Dispose();
        }

        public void Draw(Graphics gh)
        {
            if (Width == 0 || Height == 0)
                return;

            Pen selectedPen = null;

            Pen pen = _Pen;

            if (IsSelected == true)
            {
                selectedPen = new Pen(_Pen.Color, _Pen.Width);
                selectedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                pen = selectedPen;
            }

            ShapeDraw(pen, gh);

            SelectedRectDraw(gh);

            if (selectedPen != null)
                selectedPen.Dispose();
        }

        public virtual RectangleF GetHitRectPosition(HitLocation location)
        {
            RectangleF rect = new RectangleF();

            var center_y = (int)(LeftTop.Y + RightBottom.Y) / 2;
            var center_x = (int)(LeftTop.X + RightBottom.X) / 2;

            int pen_thickness = 0; 
            if(_Pen != null)
                pen_thickness = (int)_Pen.Width / 2;

            switch (location)
            {
                case HitLocation.Left:

                    rect = new RectangleF((float)(LeftTop.X - _SelectedRectSize - pen_thickness), (float)(center_y - (_SelectedRectSize /2.0)), _SelectedRectSize, _SelectedRectSize);

                    break;

                case HitLocation.Top:

                    rect = new RectangleF((float)center_x - (_SelectedRectSize / 2), (float)(LeftTop.Y - _SelectedRectSize - pen_thickness), _SelectedRectSize, _SelectedRectSize);

                    break;

                case HitLocation.Right:

                    rect = new RectangleF((float)(RightBottom.X + pen_thickness), (float)center_y - (_SelectedRectSize / 2), _SelectedRectSize, _SelectedRectSize);

                    break;

                case HitLocation.Bottom:

                    rect = new RectangleF((float)center_x - (_SelectedRectSize / 2), (float)(RightBottom.Y + pen_thickness), _SelectedRectSize, _SelectedRectSize);

                    break;

                case HitLocation.LeftTop:

                    rect = new RectangleF((float)(LeftTop.X - _SelectedRectSize - pen_thickness), (float)(LeftTop.Y - _SelectedRectSize - pen_thickness), _SelectedRectSize, _SelectedRectSize);
                    break;

                case HitLocation.RightTop:

                    rect = new RectangleF((float)(RightBottom.X + pen_thickness), (float)(LeftTop.Y - _SelectedRectSize - pen_thickness), _SelectedRectSize, _SelectedRectSize);
                    break;
                    
                case HitLocation.RightBottom:

                    rect = new RectangleF((float)(RightBottom.X + pen_thickness), (float)(RightBottom.Y + pen_thickness), _SelectedRectSize, _SelectedRectSize);
                    break;

                case HitLocation.LeftBottom:

                    rect = new RectangleF((float)(LeftTop.X - _SelectedRectSize - pen_thickness), (float)(RightBottom.Y + pen_thickness), _SelectedRectSize, _SelectedRectSize);
                    break;
            }
            return rect;
        }

        public virtual HitLocation IsHitRectPosition(DPointCoordinates p)
        {
            foreach (HitLocation location in Enum.GetValues(typeof(HitLocation)))
            {
                var hit_rect = GetHitRectPosition(location);

                if(hit_rect.Contains((float)p.X, (float)p.Y) == true)
                {
                    return location;
                }
            }

            return HitLocation.None;
        }

        protected virtual void SelectedRectDraw(Graphics gh)
        {

            if (IsSelected == true)
            {
                Pen _selectedPen = new Pen(_Pen.Color, 1);

                //int left = (int)_P1.X - _SelectedRectSize;
                //int top = (int)_P1.Y - _SelectedRectSize;
                //int width = _SelectedRectSize;
                //int height = _SelectedRectSize;

                //// LeftTop
                //gh.FillRectangle(_selectedPen.Brush, left, top, width, height);
                //gh.DrawRectangle(_selectedPen, left, top, width, height);

                //int left = (int)_P1.X;
                //int top = (int)_P1.Y;

                //int right = (int)
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.Left));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.Top));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.Right));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.Bottom));

                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.LeftTop));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.LeftBottom));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.RightTop));
                gh.FillRectangle(_selectedPen.Brush, GetHitRectPosition(HitLocation.RightBottom));
            }
        }
        #endregion

        #region abstract
        protected abstract void ShapeDraw(Pen pen, Graphics gh);
               
        public virtual bool ClickSelect(DPointCoordinates P)
        {
            if (_P1.X < P.X && _P2.X > P.X &&
                _P1.Y < P.Y && _P2.Y > P.Y)
                return true;

            return false;
        }
        
        #endregion

        #region constructors
        protected DrawableShape() { }

        //public DrawableShape(Pen pen, DPointCoordinates P1, DPointCoordinates P2)
        //    : this("", pen, Brushes.Transparent, P1, P2)
        //{

        //}

        //public DrawableShape(Pen pen, Brush brush, DPointCoordinates P1, DPointCoordinates P2)
        //{
        //    _Pen = pen;
        //    _Brush = brush;

        //    _P1 = P1;
        //    _P2 = P2;

        //    _IsSelected = false;

        //    _LastHit = HitLocation.None;
        //}

        public DrawableShape(string ShapeName, Pen pen, Brush brush, DPointCoordinates P1, DPointCoordinates P2)
        {
            _Pen = pen;
            _Brush = brush;

            _P1 = P1;
            _P2 = P2;

            _IsSelected = false;

            _ShapeName = ShapeName;

            _LastHit = HitLocation.None;
        }
        #endregion
    }
}
