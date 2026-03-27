using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public static class Utils
    {
        public static double DegreeToRadian(double d)
        {
            return Math.PI * d / 180.0;
        }
        public static double RadianToDegree(double r)
        {
            return 180.0 * r / Math.PI;
        }
        public static double limit(double actual, double lower, double upper)
        {
            return Math.Min(Math.Max(actual, lower), upper);            
        }
        public static int limit(int actual, int lower, int upper)
        {
            return Math.Min(Math.Max(actual, lower), upper);
        }
        public static byte limit(byte actual, byte lower, byte upper)
        {
            return Math.Min(Math.Max(actual, lower), upper);
        }
        public static double Distance(DPointCoordinates p1, DPointCoordinates p2, DistanceDirections dir, double dScale = 1)
        {
            double v = 0.0;

            if(dir == DistanceDirections.Width)
            {
                v = p2.X - p1.X;
            }
            else if(dir== DistanceDirections.Height)
            {
                v = p2.Y - p1.Y;
            }
            else
            {
                v = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
            }

            return limit(v, 0, 99999) * dScale;
        }
        public static DPointCoordinates Center(DPointCoordinates p1, DPointCoordinates p2)
        {
            return new DPointCoordinates((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        public static DPointCoordinates RotatePoint(DPointCoordinates center, DPointCoordinates point, double sin_radian, double cos_radian)
        {
            DPointCoordinates p = new DPointCoordinates();
            double a = point.X - center.X;
            double b = point.Y - center.Y;
            p.X = a * cos_radian - b * sin_radian;
            p.Y = a * sin_radian + b * cos_radian;
            return p;
        }
        
        // 원점을 기준으로 두 점이 이루고 있는 각(첫번째 점과 원점 라인 기준)
        public static double TwoPointDegree(DPointCoordinates p1, DPointCoordinates p2)
        {
            double v = 0.0;
            if ((p2.X - p1.X) != 0.0)
            {
                v = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * (180.0 / Math.PI);

                v = 90.0 - Math.Abs(v);
            }
            return v;
        }
        public static double ThreePointDegree(DPointCoordinates center, DPointCoordinates p1, DPointCoordinates p2)
        {
            DPointCoordinates tc = new DPointCoordinates();
            DPointCoordinates ts = new DPointCoordinates();
            DPointCoordinates te = new DPointCoordinates();

            tc.X = 0.0;
            tc.Y = 0.0;

            ts.X = p1.X - center.X;
            ts.Y = p1.Y - center.Y;

            double a1 = TwoPointDegree(ts, tc);

            if (ts.Y > 0)
            {
                a1 = 90.0 - Math.Abs(a1);
            }
            else if (ts.Y < 0)
            {
                a1 = 90.0 + Math.Abs(a1);
            }
            else
            {
                a1 = Math.Abs(a1);
            }

            te.X = p2.X - center.X;
            te.Y = p2.Y - center.Y;

            double a2 = TwoPointDegree(tc, te);

            if (te.Y > 0)
            {
                a2 = 90.0 - Math.Abs(a2);
            }
            else if (te.Y < 0)
            {
                a2 = 90.0 + Math.Abs(a2);
            }
            else
            {
                a2 = Math.Abs(a2);
            }
            return a1 + a2;
        }
    
        //두점의 좌표와 두점이 이루는 각을 이용하여 Center 좌표를 계산.
        public static DPointCoordinates GetCenterPointByTwoPointAngle(DPointCoordinates p1, DPointCoordinates p2, double degree)
        {
            DPointCoordinates center = new DPointCoordinates();

            double theta = (180.0 - degree) / 2.0;

            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            double xc = (p1.X + p2.X) / 2.0;
            double yc = (p1.Y + p2.Y) / 2.0;

            double d = Math.Sqrt(dx * dx + dy * dy);
            double hd = d / 2.0;

            double rad = theta * Math.PI / 180.0;
            double r = 1.0 / Math.Cos(rad) * hd;


            double fOffset = Math.Sqrt(r * r - hd * hd);

            double fplusx = fOffset * dy / d;
            double fplusy = fOffset * dx / d;

            center.X = xc + fplusx;
            center.Y = yc - fplusy;

            return center;
        }
    }
}
