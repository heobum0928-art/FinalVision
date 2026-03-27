using ImageGlass;
using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public class DrawableRectangle : DrawableShape
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            if (_Brush != null && _Brush != Brushes.Transparent)
            {
                gh.FillRectangle(_Brush, (int)_P1.X, (int)_P1.Y, (int)Width, (int)Height);
            }

            gh.DrawRectangle(pen, (int)_P1.X, (int)_P1.Y, (int)Width, (int)Height);
        }

        #endregion

        #region constructors

        protected DrawableRectangle() { }

        public DrawableRectangle(string ShapeName, Pen pen, Brush brush, DPointCoordinates P1, DPointCoordinates P2)
            : base(ShapeName, pen, brush, P1, P2)
        {

        }

        public DrawableRectangle(Pen pen, Brush brush, DPointCoordinates P1, DPointCoordinates P2)
            : base("", pen, brush, P1, P2)
        {

        }

        public DrawableRectangle(Pen pen, DPointCoordinates P1, DPointCoordinates P2)
            : base("", pen, Brushes.Transparent, P1, P2)
        {

        }

        public DrawableRectangle(string ShapeName, Pen pen, DPointCoordinates P1, DPointCoordinates P2)
            : base(ShapeName, pen, Brushes.Transparent, P1, P2)
        {

        }

        #endregion
    }
}
