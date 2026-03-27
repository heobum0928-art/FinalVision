using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public class DrawableArrowLine : DrawableShape
    {
        #region fields
        private int _ArrowSize;

        private ArrowDirect _ArrowDirect;
        #endregion

        #region propertise
        public int ArrowSize
        {
            get
            {
                return _ArrowSize;
            }
        }
        #endregion

        #region methods
        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            using (System.Drawing.Drawing2D.AdjustableArrowCap arrowCap =
                new System.Drawing.Drawing2D.AdjustableArrowCap(_ArrowSize, _ArrowSize, true))
            {
                if(_ArrowDirect == ArrowDirect.Start)
                {
                    pen.CustomStartCap = arrowCap;
                    //pen.CustomEndCap = null;
                }
                else
                {
                    //pen.CustomStartCap = null;
                    pen.CustomEndCap = arrowCap;

                }        
                gh.DrawLine(pen, (int)_P1.X, (int)_P1.Y, (int)_P2.X, (int)_P2.Y);
               

            }
        }

        protected override void SelectedRectDraw(Graphics gh)
        {
            //if (IsSelected == true)
            //{
            //    Pen _selectedPen = new Pen(_Pen.Color, 1);

            //    gh.FillEllipse(_selectedPen.Brush, GetHitRectPosition(HitLocation.LeftTop));
            //    gh.FillEllipse(_selectedPen.Brush, GetHitRectPosition(HitLocation.RightBottom));
            //}
        }

        public override bool ClickSelect(DPointCoordinates P)
        {
            using (System.Drawing.Pen hitTestPen = new System.Drawing.Pen(System.Drawing.Color.Black, 10f))
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    var rectf = ToRectF();
                    var imgPoint = new System.Drawing.Point((int)P.X, (int)P.Y);

                    path.AddLine(new PointF(rectf.Left, rectf.Top), new PointF(rectf.Left + rectf.Width, rectf.Top + rectf.Height));

                    if (path.IsOutlineVisible(imgPoint, hitTestPen))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override RectangleF GetHitRectPosition(HitLocation location)
        {
            RectangleF rect = new RectangleF();

            var center_y = (int)(LeftTop.Y + RightBottom.Y) / 2;
            var center_x = (int)(LeftTop.X + RightBottom.X) / 2;

            var pen_thickness = (int)_Pen.Width / 2.0;

            switch (location)
            {
                case HitLocation.LeftTop:

                    //rect = new RectangleF((float)(LeftTop.X - _SelectedRectSize - pen_thickness), (float)(LeftTop.Y - _SelectedRectSize - pen_thickness), _SelectedRectSize, _SelectedRectSize);

                    rect = new RectangleF((float)(LeftTop.X - (_SelectedRectSize / 2 )), (float)(LeftTop.Y - (_SelectedRectSize / 2 )), _SelectedRectSize, _SelectedRectSize);
                    break;

                case HitLocation.RightBottom:

                    rect = new RectangleF((float)(RightBottom.X - (_SelectedRectSize / 2)), (float)(RightBottom.Y - (_SelectedRectSize / 2)), _SelectedRectSize, _SelectedRectSize);
                    break;
            }
            return rect;
        }

        public override HitLocation IsHitRectPosition(DPointCoordinates p)
        {
            var hit_rect1 = GetHitRectPosition(HitLocation.LeftTop);

            var hit_rect2 = GetHitRectPosition(HitLocation.RightBottom);

            if (hit_rect1.Contains((float)p.X, (float)p.Y) == true)
            {
                return HitLocation.LeftTop;
            }

            if (hit_rect2.Contains((float)p.X, (float)p.Y) == true)
            {
                return HitLocation.RightBottom;
            }

            return HitLocation.None;
        }

        public override void Move(double x, double y, HitLocation location = HitLocation.None)
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

        #endregion

        #region constructors
        protected DrawableArrowLine() { }

        public DrawableArrowLine(Pen pen, DPointCoordinates P1, DPointCoordinates P2, ArrowDirect ArrowDir, int ArrowSize)
            : base("", pen, Brushes.Transparent, P1, P2)
        {
            this._ArrowSize = ArrowSize;

            this._ArrowDirect = ArrowDir;
        }

        public DrawableArrowLine(string ShapeName, Pen pen, DPointCoordinates P1, DPointCoordinates P2, ArrowDirect ArrowDir, int ArrowSize)
            : base(ShapeName, pen, Brushes.Transparent, P1, P2)
        {
            this._ArrowSize = ArrowSize;

            this._ArrowDirect = ArrowDir;
        }

        #endregion
    }
}
