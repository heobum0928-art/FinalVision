using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public enum MEASURE_DIRECT
    {
        WIDTH,
        HEIGHT,
        LINEAR
    }
    public static class CoordinateCalculator
    {
        public static double GetDistance(DPointCoordinates p1, DPointCoordinates p2, MEASURE_DIRECT dir)
        {
            double v = 0.0;
            if(dir == MEASURE_DIRECT.WIDTH)
            {
                v = p2.X - p1.X;
            }
            else if(dir == MEASURE_DIRECT.HEIGHT)
            {
                v = p2.Y - p1.Y;
            }
            else
            {
                v = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
            }

            return v;
        }
        public static double GetDistance(DPointCoordinates p1, DPointCoordinates p2, MEASURE_DIRECT dir, double scaleX, double scaleY)
        {
            double v = 0.0;
            if(dir == MEASURE_DIRECT.WIDTH)
            {
                v = GetDistance(p1, p2, dir) * scaleX;
            }
            else if(dir == MEASURE_DIRECT.HEIGHT)
            {
                v = GetDistance(p1, p2, dir) * scaleY;
            }
            else
            {
                double x = (p2.X - p1.X) * scaleX;
                double y = (p2.Y - p1.Y) * scaleY;
                v = Math.Sqrt((x) * (x) + (y) * (y));
            }
            return v;
        }

        public static DPointCoordinates CenterPosition(DPointCoordinates p1, DPointCoordinates p2)
        {
            DPointCoordinates d = new DPointCoordinates();

            d.X = (p2.X + p1.X) / 2.0;
            d.Y = (p2.Y + p1.X) / 2.0;

            return d;
        }

        //public static DPointCoordinates RotatePoint(DPointCoordinates p1, DPointCoordinates p2, double sinRad, double cosRad)
        //{
        //    DPointCoordinates d = new DPointCoordinates();

        //    double a = p1.X - p2.X;
        //    double b = p1.Y - p2.Y;
        //    d.X = a * cosRad - b * sinRad;
        //    d.Y = a * sinRad + b * cosRad;

        //    return d;
        //}


        public static DPointCoordinates RotatePoint1(DPointCoordinates pointToRotate, DPointCoordinates centerPoint, double degree)
        {
            // 1. 각도를 도(degrees)에서 라디안(radians)으로 변환
            double angleInRadians = degree * (Math.PI / 180.0);

            // 2. 회전시킬 점을 원점(0,0)을 기준으로 이동 (중심점을 (0,0)으로 이동)
            // pointToRotate에서 centerPoint의 X, Y를 뺍니다.
            double tempX = pointToRotate.X - centerPoint.X;
            double tempY = pointToRotate.Y - centerPoint.Y;

            // 3. 원점을 기준으로 회전 변환 공식 적용
            // x' = x * cos(theta) - y * sin(theta)
            // y' = x * sin(theta) + y * cos(theta)
            double rotatedX = tempX * Math.Cos(angleInRadians) - tempY * Math.Sin(angleInRadians);
            double rotatedY = tempX * Math.Sin(angleInRadians) + tempY * Math.Cos(angleInRadians);

            // 4. 회전된 점을 원래 위치로 다시 이동 (중심점을 원래 위치로 복원)
            // 회전된 X, Y에 centerPoint의 X, Y를 더합니다.
            double finalX = rotatedX + centerPoint.X;
            double finalY = rotatedY + centerPoint.Y;

            return new DPointCoordinates(finalX, finalY);
        }

        public static DPointCoordinates RotatePoint2(DPointCoordinates pointToRotate, DPointCoordinates centerPoint, double radian)
        {
            // 1. 각도를 도(degrees)에서 라디안(radians)으로 변환
            //double angleInRadians = degree * (Math.PI / 180.0);

            // 2. 회전시킬 점을 원점(0,0)을 기준으로 이동 (중심점을 (0,0)으로 이동)
            // pointToRotate에서 centerPoint의 X, Y를 뺍니다.
            double tempX = pointToRotate.X - centerPoint.X;
            double tempY = pointToRotate.Y - centerPoint.Y;

            // 3. 원점을 기준으로 회전 변환 공식 적용
            // x' = x * cos(theta) - y * sin(theta)
            // y' = x * sin(theta) + y * cos(theta)
            double rotatedX = tempX * Math.Cos(radian) - tempY * Math.Sin(radian);
            double rotatedY = tempX * Math.Sin(radian) + tempY * Math.Cos(radian);

            // 4. 회전된 점을 원래 위치로 다시 이동 (중심점을 원래 위치로 복원)
            // 회전된 X, Y에 centerPoint의 X, Y를 더합니다.
            double finalX = rotatedX + centerPoint.X;
            double finalY = rotatedY + centerPoint.Y;

            return new DPointCoordinates(finalX, finalY);
        }


        public static DPointCoordinates LinePointX(DPointCoordinates p1, DPointCoordinates p2, double y)
        {
            DPointCoordinates d = new DPointCoordinates();

            double a = 0.0;
            double b = 0.0;

            if ((p2.X - p1.X) == 0)
                a = 0.0;
            else
                a = (p2.Y - p1.Y) / (p2.X - p1.X);

            b = p1.Y - a * p1.X;

            if (a == 0.0)
                d.X = 0.0;
            else
                d.X = (y - b) / a;
            d.Y = y;

            return d;
        }
        public static DPointCoordinates LinePointY(DPointCoordinates p1, DPointCoordinates p2, double x)
        {
            DPointCoordinates d = new DPointCoordinates();

            double a = 0.0;
            double b = 0.0;

            if ((p2.X - p1.X) == 0)
                a = 0.0;
            else
                a = (p2.Y - p1.Y) / (p2.X - p1.X);

            b = p1.Y - a * p1.X;

            d.X = x;
            d.Y = x * a + b;

            return d;
        }

        // 2점각 구하기
        public static double GetAngle_2P(DPointCoordinates p1, DPointCoordinates p2)
        {
            double v = 0.0;
            if ((p2.X - p1.X) != 0.0)
            {
                v = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * (180.0 / Math.PI);

                v = 90.0 - Math.Abs(v);
            }
            return v;
        }

        // 3점으로 이루어진 각 구하기
        public static double GetAngle_3P(DPointCoordinates c, DPointCoordinates s, DPointCoordinates e)
        {
            DPointCoordinates tc = new DPointCoordinates();
            DPointCoordinates ts = new DPointCoordinates();
            DPointCoordinates te = new DPointCoordinates();

            tc.X = 0.0;
            tc.Y = 0.0;

            ts.X = s.X - c.X;
            ts.Y = s.Y - c.Y;

            double a1 = GetAngle_2P(ts, tc);

            if (ts.Y > 0.0)
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

            te.X = e.X - c.X;
            te.Y = e.Y - c.Y;

            double a2 = GetAngle_2P(tc, te);

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

        // 두직선의 교차점
        public static DPointCoordinates IntersectPoint(DPointCoordinates AP1, DPointCoordinates AP2, DPointCoordinates BP1, DPointCoordinates BP2)
        {
            DPointCoordinates IP = new DPointCoordinates();

            IP.X = 0.0;
            IP.Y = 0.0;

            if (AP1.X == AP2.X && AP1.Y == AP2.Y)
                return null;

            if (BP1.X == BP2.X && BP1.Y == BP2.Y)
                return null;

            double a1 = 0.0;
            double b1 = 0.0;
            double a2 = 0.0;
            double b2 = 0.0;

            if ((AP2.X - AP1.X) == 0)
                a1 = 0.0;
            else
                a1 = (AP2.Y - AP1.Y) / (AP2.X - AP1.X);
            b1 = AP2.Y - a1 * AP2.X;

            if ((BP2.X - BP1.X) == 0)
                a2 = 0.0;
            else
                a2 = (BP2.Y - BP1.Y) / (BP2.X - BP1.X);
            b2 = BP1.Y - a2 * BP1.X;

            if ((a1 - a2) == 0.0)
                IP.X = 0.0;
            else
                IP.X = (b2 - b1) / (a1 - a2);
            IP.Y = a1 * IP.X + b1;

            return IP;
        }

        // p1을  angle 만큼 회전시켜 p2가 되었을때 Center 포인트
        public static DPointCoordinates TwoAngleCenterPoint(DPointCoordinates PT1, DPointCoordinates PT2, double angle)
        {
            DPointCoordinates center = new DPointCoordinates();

            double theta = (180.0 - angle) / 2.0;

            double dx = PT1.X - PT2.X;
            double dy = PT1.Y - PT2.Y;
            double xc = (PT1.X + PT2.X) / 2.0;
            double yc = (PT1.Y + PT2.Y) / 2.0;

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
