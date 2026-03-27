using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.DataStructures;

namespace Project.UI.Common
{
    public class DrawableLine : DrawableShape
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            gh.DrawLine(pen, (int)_P1.X, (int)_P1.Y, (int)_P2.X, (int)_P2.Y);
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
        #endregion

        #region constructors
        protected DrawableLine() { }

        //public DrawableLine(Pen pen, Brush brush, DPointCoordinates P1, DPointCoordinates P2)
        //    : base("", pen, brush, P1, P2)
        //{

        //}

        public DrawableLine(Pen pen, DPointCoordinates P1, DPointCoordinates P2)
            : base("", pen, Brushes.Transparent, P1, P2)
        {

        }

        public DrawableLine(string ShapeName, Pen pen, DPointCoordinates P1, DPointCoordinates P2)
            : base(ShapeName, pen, Brushes.Transparent, P1, P2)
        {

        }
        #endregion
    }
}
