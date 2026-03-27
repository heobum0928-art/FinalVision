using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class MeasureLine
    {
        #region fields
        // ax + by + c = 0
        protected double a;
        protected double b;
        protected double c;
        #endregion

        #region propertise
        public double A
        {
            get { return a; }
        }
        public double B
        {
            get { return b; }
        }
        public double C
        {
            get { return c; }
        }

        #endregion

        #region methods
        public static MeasureLine Create(double sx, double sy, double ex, double ey)
        {
            MeasureLine line = new MeasureLine();
            //sy *= -1;
            //ey *= -1;
            line.a = ey - sy;
            line.b = sx - ex;
            if (line.a == 0)
            {
                line.b = 1;
                line.c = -sy;
            }
            else if (line.b == 0)
            {
                line.a = 1;
                line.c = -sx;
            }
            else
                line.c = (sy - ey) * sx + (ex - sx) * sy;

            return line;
        }
        public static MeasureLine Fit(List<DPointCoordinates> points)
        {
            int i;
            double dSumX, dSumY, dSumXX, dSumXY;
            double dX, dY, dD;

            dSumX = dSumY = dSumXX = dSumXY = 0;

            if (points.Count == 0) return null;

            for (i = 0; i < points.Count; i++)
            {
                dX = points[i].X;
                dY = points[i].Y;

                //dSumX += dX;
                //dSumY += (-1 * dY); // Y좌표를 바꿔서 저장.
                //dSumXX += dX * dX;
                //dSumXY += dX * (-1 * dY);

                dSumX += dX;
                dSumY += dY;
                dSumXX += dX * dX;
                dSumXY += dX * dY;
            }

            dD = dSumXX * points.Count - dSumX * dSumX;  //?



            double a = 0.0;
            double b = 0.0;
            double c = 0.0;

            if (dD == 0)
            {
                a = -1;
                b = 0;
                c = points[0].X;
            }
            else
            {
                a = (dSumXY * points.Count - dSumX * dSumY) / dD;
                b = -1;
                c = -a * dSumX / points.Count - b * dSumY / points.Count;
            }
            MeasureLine line = new MeasureLine();
            line.a = Math.Truncate(a * 100) / 100;
            line.b = Math.Truncate(b * 100) / 100;
            line.c = Math.Truncate(c * 100) / 100;

            return line;
        }
        public static MeasureLine Fit(List<double> array_x, List<double> array_y)
        {
            int i;
            double dSumX, dSumY, dSumXX, dSumXY;
            double dX, dY, dD;

            dSumX = dSumY = dSumXX = dSumXY = 0;

            if (array_x.Count == 0) return null;

            for (i = 0; i < array_x.Count; i++)
            {
                dX = array_x[i];
                dY = array_y[i];

                //dSumX += dX;
                //dSumY += (-1 * dY); // Y좌표를 바꿔서 저장.
                //dSumXX += dX * dX;
                //dSumXY += dX * (-1 * dY);

                dSumX += dX;
                dSumY += dY; 
                dSumXX += dX * dX;
                dSumXY += dX * dY;
            }


            dD = dSumXX * array_x.Count - dSumX * dSumX;  //?
            double a = 0.0;
            double b = 0.0;
            double c = 0.0;
            if (dD == 0)
            {
                a = -1;
                b = 0;
                c = array_x[0];
            }
            else
            {
                a = (dSumXY * array_x.Count - dSumX * dSumY) / dD;
                b = -1;
                c = -a * dSumX / array_x.Count - b * dSumY / array_x.Count;
            }

            MeasureLine line = new MeasureLine();
            line.a = Math.Truncate(a * 1000) / 1000;
            line.b = Math.Truncate(b * 1000) / 1000;
            line.c = Math.Truncate(c * 1000) / 1000;

            return line;
        }
        public MeasureLine Move(double x, double y)
        {
            var line = Duplicate();

            //y *= -1; // Y축 반전
            line.c -= (a * x);
            line.c -= (b * y);

            return line;
        }
        public double GetX(double y)
        {
            if (a == 0)
                return -1;
            else if (b == 0)
                return -c;
            //y *= -1; // Y축 반전
            double dValue = -(b * y + c);
            dValue /= a;
            return dValue;
        }
        public double GetY(double x)
        {
            if (b == 0)
                return -1;
            else if (a == 0)
                return c; //이미지 y축 고려한 것임
            double dValue = -(a * x + c);
            dValue /= b;
            //dValue *= -1; // Y축 반전
            return dValue;
        }
        public void Copy(MeasureLine copy_line)
        {
            if (copy_line == null)
                copy_line = new MeasureLine();

            copy_line.a = a;
            copy_line.b = a;
            copy_line.c = a;
        }
        public MeasureLine Duplicate()
        {
            return new MeasureLine(a, b, c);
        }
        public MeasureLine ConversionPerpendicular(int iAxis, double dPos)
        {
            return new MeasureLine();

            //MeasureLine* PerPendicularLine;

            //double dOtherPos;
            //double dmC_temp;

            //if (iAxis == X_AXIS)
            //{
            //    dOtherPos = YPosition(dPos);
            //    dOtherPos *= -1;
            //    dmC_temp = -dmB * dPos + dmA * dOtherPos;
            //}
            //else if (iAxis == Y_AXIS)
            //{
            //    dOtherPos = XPosition(dPos);
            //    dPos *= -1;
            //    dmC_temp = -dmB * dOtherPos + dmA * dPos;
            //}
            //PerPendicularLine = new MeasureLine(dmB, -dmA, dmC_temp);
            //return PerPendicularLine;
        }
        public DPointCoordinates InterPoint(MeasureLine line)
        {
            DPointCoordinates point = new DPointCoordinates();

            if (a == 0)
            {
                point.Y = c;
                point.X = line.GetX(point.Y);
            }
            else if (b == 0)
            {
                point.X = -c;
                point.Y = line.GetY(point.X);
            }
            else
            {
                point.X = (b * line.c - line.b * c) / (a * line.b - line.a * b);
                point.Y = GetY(point.X);
            }

            return point;
        }
        public MeasureLine ConversionPerpendicular_Point(double dXPos, double dYPos)
        {
            MeasureLine line = new MeasureLine();
            //dYPos *= -1;
            if (a == 0)
            {
                line.a = 1;
                line.b = 0;
                line.c = -dXPos;
            }
            else if (b == 0)
            {
                line.a = 0;
                line.b = 1;
                line.c = -dYPos;
            }
            else
            {
                line.a = b;
                line.b = -a;
                line.c = -(line.a * dXPos + line. b * dYPos);
            }

            return line;
        }
        public double DistancetoPoint(double x, double y)
        {
            y *= -1;
            return Math.Abs(a * x + b * y + c) / Math.Sqrt(a * a + b * b);
        }

        public List<DPointCoordinates> PointToDistance(double distance)
        {
            List<DPointCoordinates> resultPoints = new List<DPointCoordinates>();

            // a와 b가 동시에 0인 경우, 유효한 직선이 아님
            if (a == 0 && b == 0)
            {
                return null;
            }

            // 직선의 법선 벡터의 크기 계산
            double magnitude = Math.Sqrt(a * a + b * b);

            if (magnitude == 0) // 사실 위에서 a=0, b=0 체크했으므로 이 조건은 도달하지 않음
            {
                return null;
            }

            // 단위 법선 벡터 (nx, ny)
            double nx = a / magnitude;
            double ny = b / magnitude;

            // 직선 위의 임의의 한 점을 찾습니다.
            DPointCoordinates pointOnLine;

            if (Math.Abs(b) > 1e-9) // b가 0이 아닌 경우
            {
                // x = 0 일 때, y = -c/b
                pointOnLine = new DPointCoordinates(0, -c / b);
            }
            else if (Math.Abs(a) > 1e-9) // b가 0이고 a가 0이 아닌 경우 (수직선)
            {
                // y = 0 일 때, x = -c/a
                pointOnLine = new DPointCoordinates(-c / a, 0);
            }
            else // a와 b가 모두 0 (위에서 이미 처리했으나 방어적으로 추가)
            {
                return resultPoints;
            }

            // 법선 벡터 방향으로 거리 d만큼 떨어진 첫 번째 점
            double p1X = pointOnLine.X + nx * distance;
            double p1Y = pointOnLine.Y + ny * distance;
            resultPoints.Add(new DPointCoordinates(p1X, p1Y));

            // 반대 법선 벡터 방향으로 거리 d만큼 떨어진 두 번째 점
            double p2X = pointOnLine.X - nx * distance;
            double p2Y = pointOnLine.Y - ny * distance;
            resultPoints.Add(new DPointCoordinates(p2X, p2Y));

            return resultPoints;
        }

        public List<DPointCoordinates> PointToDistance(double x1, double y1, double distance)
        {
            List<DPointCoordinates> resultPoints = new List<DPointCoordinates>();

            // 1. (x1, y1)이 실제로 직선 ax + by + c = 0 위에 있는지 확인 (부동 소수점 오차 고려)
            // ax1 + by1 + c 가 0에 매우 가까워야 함.
            // 이 검사는 입력 데이터의 유효성을 높이지만, 필수는 아님 (수학적으로는 점이 직선 위에 있다고 가정).
            // 그러나 실제 응용에서는 중요합니다.
            double checkOnLine = a * x1 + b * y1 + c;
            if (Math.Abs(checkOnLine) > 1e-6) // 1e-6은 허용 오차. 필요에 따라 조정 가능.
            {
                Console.WriteLine($"경고: 시작점 ({x1}, {y1})은 주어진 직선 {a}x + {b}y + {c} = 0 위에 있지 않습니다. (오차: {checkOnLine:E2})");
                // 만약 시작점이 직선 위에 없으면 계산을 진행하지 않고 싶다면 여기서 return resultPoints; 를 추가할 수 있습니다.
            }

            // 2. 직선의 유효성 검사: a와 b가 동시에 0인 경우는 유효한 직선이 아님
            if (a == 0 && b == 0)
            {
                Console.WriteLine("오류: a와 b가 동시에 0이므로 유효한 직선이 아닙니다.");
                return resultPoints;
            }

            // 3. 직선의 법선 벡터 (a, b)의 크기 계산
            double magnitude = Math.Sqrt(a * a + b * b);

            // 4. 단위 법선 벡터 (nx, ny) 계산
            // 법선 벡터 (a, b)를 크기로 나누어 단위 벡터로 정규화합니다.
            // 이 벡터는 길이가 1이며, 직선 ax + by + c = 0 에 수직입니다.
            double nx = a / magnitude;
            double ny = b / magnitude;

            // 5. 시작점 (x1, y1)에서 단위 법선 벡터 방향으로 거리 d만큼 떨어진 점 계산
            // 두 개의 점이 존재합니다: 법선 벡터 방향과 그 반대 방향.

            // 첫 번째 점: 법선 벡터 (nx, ny) 방향으로 distance 만큼 이동
            double p1X = x1 + nx * distance;
            double p1Y = y1 + ny * distance;
            resultPoints.Add(new DPointCoordinates(p1X, p1Y));

            // 두 번째 점: 법선 벡터의 반대 방향 (-nx, -ny)으로 distance 만큼 이동
            double p2X = x1 - nx * distance;
            double p2Y = y1 - ny * distance;
            resultPoints.Add(new DPointCoordinates(p2X, p2Y));

            return resultPoints;
        }
        public MeasureLine ParallelLine(double x, double y)
        {
            MeasureLine line = new MeasureLine();

            y *= -1;
            line.a = a;
            line.b = b;
            line.c = -(line.a * x + line.b * y);

            return line;
        }

        public void Clear()
        {
            a = 0.0;
            b = 0.0;
            c = 0.0;
        }
        public DPointCoordinates GetTwoLineCrossPoint(MeasureLine line)
        {
            //////////////////////////////////////////////////////////////////////////
            // a1X + b1Y + c1 = 0, a2X + b2Y + c2 = 0 <=== 직선 식 2개
            //////////////////////////////////////////////////////////////////////////
            
            double a1, b1, c1;                                                      ///////////////////	
            double a2, b2, c2;                                                      // 계산 편의상 변수
            double a1c2, a2c1, a2b1, a1b2, b1c2, b2c1;      ///////////////////

            a1 = a; a2 = line.a;
            b1 = b; b2 = line.b;
            c1 = c; c2 = line.c;

            a1c2 = a1 * c2; a2c1 = a2 * c1; a2b1 = a2 * b1;
            a1b2 = a1 * b2; b1c2 = b1 * c2; b2c1 = b2 * c1;

            if ((a2b1 - a1b2) != 0 && (a1b2 - a2b1) != 0)
            {
                DPointCoordinates cross_point = new DPointCoordinates();
                cross_point.Y = (a1c2 - a2c1) / (a2b1 - a1b2);
                cross_point.X = (b1c2 - b2c1) / (a1b2 - a2b1);
                return cross_point;
            }
            return null;
        }
        public double GetAngle(Axis axis)
        {
            double dAngle = 0.0;

            if (a == 0)
                return 0.0;
            else if (b == 0)
                return 90.0;
            else
            {
                dAngle = Math.Atan(-1 * a / b) * 180.0 / Math.PI;

                //dAngle = (-1 * Math.Atan(-1 * a / b) * 180.0 / Math.PI);// Y축 반전
                //if (axis == Axis.Horizontal)
                //{
                //    return dAngle;
                //}
                //else
                if (axis == Axis.Vertical)
                {
                    dAngle = (90 - Math.Abs(dAngle)) * -1;
                }
            }
            return dAngle;

        }
        public double GetRadian(Axis axis)
        {
            double dRadian;

            if (a == 0)
                return 0.0;
            else if (b == 0)
                return 90.0;
            else
            {
                dRadian = Math.Atan(-1.0 * a / b);
                //dRadian = (-1.0 * Math.Atan(-1.0 * a / b)); // Y축 반전
                //if (axis == Axis.Horizontal)
                //{
                //    return dRadian;
                //}
                //else
                if (axis == Axis.Vertical)
                {
                    dRadian = ((Math.PI / 2.0) - Math.Abs(dRadian)) * -1.0;
                    return dRadian;
                }
            }

            return dRadian;
        }
        public static DPointCoordinates GetTwoPointCenter(double x1, double y1, double x2, double y2)
        {
            DPointCoordinates point = new DPointCoordinates();
            if (x1 == x2)                                   // Y축 평행
            {
                point.X = x1;
                point.Y = (y1 + y2) / 2.0;
            }
            else if (y1 == y2)                          //X 축 평행
            {
                point.X = (x1 + x2) / 2.0;
                point.Y = y1;
            }
            else
            {
                point.X = (x2 + x1) / 2.0;
                point.Y = (y1 + y2) / 2.0;
            }

            return point;
        }
        public static double TwoPointDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1)); //dLength;
        }
        public override string ToString()
        {
            return string.Format("Measure Line : A = [{0}], B = [{1}], C = [{2}]", a, b, c);
        }

        #endregion

        #region constructors
        public MeasureLine()
            : this(0, 0, 0)
        {

        }
        public MeasureLine(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        #endregion
    }
}
