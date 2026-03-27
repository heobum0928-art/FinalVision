using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute]
    public class RansacLineResult : NotifyPropertyChanged
    {
        #region fields
        protected double _Slope; // 기울기

        protected double _Intercept; // y 절편

        protected List<DPointCoordinates> _Inliers;
        #endregion

        #region propertise
        public double Slope
        {
            get
            {
                return _Slope;
            }

            set
            {
                _Slope = value;
                OnPropertyChanged();
            }
        }
        public double Intercept
        {
            get
            {
                return _Intercept;
            }

            set
            {
                _Intercept = value;
                OnPropertyChanged();
            }
        }
        public List<DPointCoordinates> Inliers
        {
            get
            {
                return _Inliers;
            }

            set
            {
                _Inliers = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public RansacLineResult()
        {
            _Slope = 0.0; // 기울기

            _Intercept = 0.0; // y 절편.

            _Inliers = new List<DPointCoordinates>();
        }
        #endregion
    }
    
    [DataContract]
    [System.SerializableAttribute]
    public class CircleResult : CircleData
    {
        #region fields
        //protected double _CenterX;
        //protected double _CenterY;
        //protected double _Radius;
        protected List<DPointCoordinates> _Inliers;
        #endregion

        #region propertise
        //public double CenterX
        //{
        //    get
        //    {
        //        return _CenterX;
        //    }
        //    set
        //    {
        //        _CenterX = value;
        //        OnPropertyChanged();
        //    }
        //}
        //public double CenterY
        //{
        //    get
        //    {
        //        return _CenterY;
        //    }
        //    set
        //    {
        //        _CenterY = value;
        //        OnPropertyChanged();
        //    }
        //}
        //public double Radius
        //{
        //    get
        //    {
        //        return _Radius;
        //    }
        //    set
        //    {
        //        _Radius = value;
        //        OnPropertyChanged();
        //    }
        //}
        public List<DPointCoordinates> Inliers
        {
            get
            {
                return _Inliers;
            }
            set
            {
                _Inliers = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return base.ToString() +string.Format(", ") + string.Format("Inliers.Count = {0}", _Inliers.Count);
        }
        #endregion

        #region constructors
        public CircleResult()
        {
            //_CenterX = 0.0;
            //_CenterY = 0.0;
            //_Radius = 0.0;
            _Inliers = new List<DPointCoordinates>();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public static class RANSAC_Utils
    {
        #region fields
        private static Random rand;
        #endregion

        #region propertise

        #endregion

        #region methods
        // Line
        static RansacLineResult FintLine(List<DPointCoordinates> points, int iterations = 1000, double threshold = 1.0)
        {
            double bestSlope = 0;
            double bestIntercept = 0;

            int bestInlierCount = 0;

            List<DPointCoordinates> bestInliers = new List<DPointCoordinates>();

            for (int i = 0; i < iterations; i++)
            {
                var p1 = points[rand.Next(points.Count)];
                var p2 = points[rand.Next(points.Count)];

                if (p1.X == p2.X)
                    continue; // 수직선(기울기 무한대) 회피

                // 2. 모델(직선) 계산: y = ax + b
                double slope = (p2.Y - p1.Y) / (p2.X - p1.X);
                double intercept = p1.Y - slope * p1.X;

                // 3. 인라이어 계산
                var inliers = new List<DPointCoordinates>();
                foreach (var p in points)
                {
                    double yEst = slope * p.X + intercept;
                    double error = Math.Abs(yEst - p.Y);
                    if (error < threshold)
                        inliers.Add(p);
                }

                // 4. 최고의 모델 갱신
                if (inliers.Count > bestInlierCount)
                {
                    bestInlierCount = inliers.Count;
                    bestSlope = slope;
                    bestIntercept = intercept;
                    bestInliers = inliers;
                }

            }

            // 5. 인라이어로 최종 선형 회귀 (선택사항)
            if (bestInliers.Count >= 2)
            {
                double avgX = bestInliers.Average(p => p.X);
                double avgY = bestInliers.Average(p => p.Y);

                double numerator = bestInliers.Sum(p => (p.X - avgX) * (p.Y - avgY));
                double denominator = bestInliers.Sum(p => Math.Pow(p.X - avgX, 2));

                double refinedSlope = numerator / denominator;
                double refinedIntercept = avgY - refinedSlope * avgX;

                bestSlope = refinedSlope;
                bestIntercept = refinedIntercept;
            }

            return new RansacLineResult
            {
                Slope = bestSlope,
                Intercept = bestIntercept,
                Inliers = bestInliers
            };
        }


        // Circle
        private static bool FitCircle3Pts(DPointCoordinates p1,
                                    DPointCoordinates p2,
                                    DPointCoordinates p3,
                                    out double cx, out double cy, out double r)
        {
            cx = cy = r = 0;

            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;
            double x3 = p3.X, y3 = p3.Y;

            double a = x2 - x1;
            double b = y2 - y1;
            double c = x3 - x1;
            double d = y3 - y1;

            double e = a * (x1 + x2) + b * (y1 + y2);
            double f = c * (x1 + x3) + d * (y1 + y3);
            double g = 2.0 * (a * (y3 - y2) - b * (x3 - x2));

            if (Math.Abs(g) < 1e-12)
                return false; // Points are collinear or too close

            cx = (d * e - b * f) / g;
            cy = (a * f - c * e) / g;
            r = Math.Sqrt((cx - x1) * (cx - x1) + (cy - y1) * (cy - y1));

            return true;
        }


        public static CircleResult FitCircleRansac(
                                    List<DPointCoordinates> points,
                                    int iterations = 2000,
                                    double threshold = 1.5)
        {
            CircleResult bestResult = new CircleResult();
            int numPoints = points.Count;

            for (int i = 0; i < iterations; i++)
            {
                // Pick 3 random points
                var p1 = points[rand.Next(numPoints)];
                var p2 = points[rand.Next(numPoints)];
                var p3 = points[rand.Next(numPoints)];

                if (!FitCircle3Pts(p1, p2, p3, out double cx, out double cy, out double r))
                    continue;

                var inliers = new List<DPointCoordinates>();

                foreach (var p in points)
                {
                    double dist = Math.Sqrt((p.X - cx) * (p.X - cx) + (p.Y - cy) * (p.Y - cy));
                    if (Math.Abs(dist - r) < threshold)
                        inliers.Add(p);
                }

                if (inliers.Count > bestResult.Inliers.Count)
                {
                    bestResult.X = cx;
                    bestResult.Y = cy;
                    bestResult.R = r;
                    bestResult.Inliers = inliers;
                }
            }

            return bestResult;
        }
        #endregion

        #region constructors
        static RANSAC_Utils()
        {
            rand = new Random();
        }
        #endregion
    }
}
