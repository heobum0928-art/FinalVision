using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public class DrawableCrossLine : DrawableShape
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            var rect = ToRectF();

            // Horz
            gh.DrawLine(pen, rect.Left, (float)Center.Y, rect.Right, (float)Center.Y);

            // Vert
            gh.DrawLine(pen, (float)Center.X, rect.Top, (float)Center.X, rect.Bottom);

        }
        #endregion

        #region constructors
        protected DrawableCrossLine() { }

        public DrawableCrossLine(Pen pen, DPointCoordinates p, int length)
            : base("", pen, Brushes.Transparent, new DPointCoordinates(p.X - length, p.Y - length), new DPointCoordinates(p.X + length, p.Y + length))
        {
        }


        public DrawableCrossLine(string ShapeName, Pen pen, DPointCoordinates p, int length)
            : base(ShapeName, pen, Brushes.Transparent, new DPointCoordinates(p.X - length, p.Y - length), new DPointCoordinates(p.X + length, p.Y + length))
        {
        }
        #endregion
    }
}
