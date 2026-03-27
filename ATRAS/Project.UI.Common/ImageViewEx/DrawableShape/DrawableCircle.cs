using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public class DrawableCircle : DrawableShape
    {
        #region fields

        #endregion

        #region propertise
        public double CenterX
        {
            get
            {
                if(IsValid() == true)
                    return (_P1.X + _P2.X) / 2.0;

                return 0.0;
            }
        }

        public double CenterY
        {
            get
            {
                if (IsValid() == true)
                    return (_P1.Y + _P2.Y) / 2.0;

                return 0.0;
            }
        }

        public double Radius
        {
            get
            {
                if (IsValid() == true)
                {
                    return (_P2.X - _P1.X) / 2.0;
                }

                return 0.0;
            }
        }
        #endregion

        #region methods
        protected override void ShapeDraw(Pen pen, Graphics gh)
        {
            var r = ToRect();
            if (_Brush != null && _Brush != Brushes.Transparent)
            {
                gh.FillEllipse(_Brush, r);
            }

            gh.DrawEllipse(pen, r);
        }

        public override bool ClickSelect(DPointCoordinates P)
        {
            // 원의 정보 (예시: 클래스 멤버 변수라고 가정)
            double centerX = CenterX;
            double centerY = CenterY;
            double radius = Radius;

            // 1. x축과 y축의 차이 계산
            double dx = P.X - centerX;
            double dy = P.Y - centerY;

            // 2. 거리의 제곱 계산 (dx^2 + dy^2)
            double distanceSquared = (dx * dx) + (dy * dy);

            // 3. 반지름의 제곱과 비교
            // Math.Sqrt(루트)를 사용하지 않는 것이 성능상 유리합니다.
            return distanceSquared <= (radius * radius);
        }
        #endregion

        #region constructors
        protected DrawableCircle() { }

        public DrawableCircle(Pen pen, double centerx, double centery, double radius)
            : base("", pen, Brushes.Transparent, new DPointCoordinates(centerx - radius, centery - radius), new DPointCoordinates(centerx + radius, centery + radius))
        {

        }

        public DrawableCircle(string ShapeName, Pen pen, double centerx, double centery, double radius)
            : base(ShapeName, pen, Brushes.Transparent, new DPointCoordinates(centerx - radius, centery - radius), new DPointCoordinates(centerx + radius, centery + radius))
        {

        }


        public DrawableCircle(Pen pen, Brush brush, double centerx, double centery, double radius)
            : base("", pen, brush, new DPointCoordinates(centerx - radius, centery - radius), new DPointCoordinates(centerx + radius, centery + radius))
        {

        }

        public DrawableCircle(string ShapeName, Pen pen, Brush brush, double centerx, double centery, double radius)
            : base(ShapeName, pen, brush, new DPointCoordinates(centerx - radius, centery - radius), new DPointCoordinates(centerx + radius, centery + radius))
        {

        }
        #endregion

    }
}
