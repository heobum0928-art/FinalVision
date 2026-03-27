using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.DataStructures;

namespace Project.UI.Common
{
    public class DrawableString : DrawableShape
    {
        #region fields
        protected Font _Font;
        protected string _String;

        protected StringAlignment _Alignment; // 가로 정렬을 
        protected StringAlignment _LineAlignment; // 세로 정렬을 

        protected SizeF _DrawSizeF;
        #endregion

        #region propertise
        public string String
        {
            get
            {
                return _String;
            }

            //set
            //{
            //    _String = value;
            //    OnPropertyChanged();
            //}
        }
        
        public Font Font
        {
            get
            {
                return _Font;
            }

            //set
            //{
            //    _Font = value;
            //    OnPropertyChanged();
            //}
        }
        #endregion

        #region methods
        public override void Dispose()
        {
            base.Dispose();

            if (_Font != null)
                _Font.Dispose();
        }

        public override Rectangle ToRect()
        {
            Rectangle rect = new Rectangle();

            float left = 0;
            float top = 0;
            float right = 0;
            float bottom = 0;

            if (_DrawSizeF.Width > 0 && 
                _DrawSizeF.Height > 0)
            {
                switch(_Alignment)
                {
                    case StringAlignment.Center:

                        left = (float)_P1.X - _DrawSizeF.Width / 2.0F;
                        right = (float)_P1.X + _DrawSizeF.Width / 2.0F;
                        break;

                    case StringAlignment.Far: // 기준점 왼쪽
                        left = (float)_P1.X;
                        right = (float)_P1.X + _DrawSizeF.Width ;

                        break;

                    case StringAlignment.Near: // 기준점 오른쪽

                        left = (float)_P1.X;
                        right = (float)_P1.X + _DrawSizeF.Width;

                        break;
                }

                switch (_LineAlignment)
                {
                    case StringAlignment.Center:

                        top = (float)_P1.Y - _DrawSizeF.Height / 2.0F;
                        bottom = (float)_P1.Y + _DrawSizeF.Height / 2.0F;

                        break;

                    case StringAlignment.Far: // 기준점이 글자의 하단.

                        top = (float)_P1.Y;
                        bottom = (float)_P1.Y + _DrawSizeF.Height;

                        break;

                    case StringAlignment.Near: // 기준점이 글자의 상단

                        top = (float)_P1.Y - _DrawSizeF.Height;
                        bottom = (float)_P1.Y;

                        break;
                }
            }

            rect = new Rectangle((int)left, (int)top, (int)_DrawSizeF.Width, (int)_DrawSizeF.Height);
            
            return rect;
        }

        public override RectangleF ToRectF()
        {
            RectangleF rect = new RectangleF();

            float left = 0;
            float top = 0;
            float right = 0;
            float bottom = 0;

            if (_DrawSizeF.Width > 0 &&
                _DrawSizeF.Height > 0)
            {
                switch (_Alignment)
                {
                    case StringAlignment.Center:

                        left = (float)_P1.X - _DrawSizeF.Width / 2.0F;
                        right = (float)_P1.X + _DrawSizeF.Width / 2.0F;
                        break;

                    case StringAlignment.Far: // 기준점 왼쪽
                        left = (float)_P1.X;
                        right = (float)_P1.X + _DrawSizeF.Width;

                        break;

                    case StringAlignment.Near: // 기준점 오른쪽

                        left = (float)_P1.X;
                        right = (float)_P1.X + _DrawSizeF.Width;

                        break;
                }

                switch (_LineAlignment)
                {
                    case StringAlignment.Center:

                        top = (float)_P1.Y - _DrawSizeF.Height / 2.0F;
                        bottom = (float)_P1.Y + _DrawSizeF.Height / 2.0F;

                        break;

                    case StringAlignment.Far: // 기준점이 글자의 하단.

                        top = (float)_P1.Y;
                        bottom = (float)_P1.Y + _DrawSizeF.Height;

                        break;

                    case StringAlignment.Near: // 기준점이 글자의 상단

                        top = (float)_P1.Y - _DrawSizeF.Height;
                        bottom = (float)_P1.Y;

                        break;
                }
            }

            rect = new RectangleF(left, top, _DrawSizeF.Width, _DrawSizeF.Height);

            return rect;
            //return new RectangleF((float)_P1.X, (float)_P1.Y, (float)(_P2.X - _P1.X), (float)(_P2.Y - _P1.Y));
        }

        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            _DrawSizeF = gh.MeasureString(_String, _Font);

            StringFormat drawFormat = new StringFormat();
            drawFormat.Alignment = _Alignment; 
            drawFormat.LineAlignment = _LineAlignment; 

            gh.DrawString(_String, _Font, _Brush, (float)_P1.X, (float)_P1.Y, drawFormat);
        }

        public override bool ClickSelect(DPointCoordinates P)
        {
            return false;

            if(ToRect().Contains(new Point((int)P.X, (int)P.Y)))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region constructors
        protected DrawableString() { }

        public DrawableString(string message, Font font, Brush brush, double x, double y)
          : this("", message, font, brush, x, y, StringAlignment.Center, StringAlignment.Center)
        {
        }

        public DrawableString(string ShapeName, string message, Font font, Brush brush, double x, double y)
            : this(ShapeName, message, font, brush, x, y, StringAlignment.Center, StringAlignment.Center)
        {

        }

        public DrawableString(string ShapeName, string message, Font font, Brush brush, double x, double y, StringAlignment Alignment, StringAlignment LineAlignment)
            : base(ShapeName, null, brush, new DPointCoordinates(x, y), new DPointCoordinates(0, 0))
        {
            _Alignment = Alignment;
            _LineAlignment = LineAlignment;

            _String = message;

            _Font = font;

            _DrawSizeF = new SizeF(0, 0);
        }
        #endregion
    }
}
