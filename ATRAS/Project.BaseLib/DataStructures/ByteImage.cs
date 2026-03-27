using Project.BaseLib.Enums;
using Project.BaseLib.Extension;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Project.BaseLib.DataStructures
{
    public class FeaturePoint
    {
        public Point Position { get; set; }
        public List<Point> Neighbors { get; set; } = new List<Point>();
        public List<double> DirectionVectors { get; set; } = new List<double>(); // 각도(rad)
        public int[] GeometricSignature { get; set; }  // 방향 벡터 히스토그램
    }

    public class TemplateModel
    {
        public List<FeaturePoint> Features { get; set; } = new List<FeaturePoint>();
        public int NumBins { get; set; } = 8;
    }

    public class MatchCandidate
    {
        public Point Translation { get; set; }
        public int Votes { get; set; }
        public double AvgScore { get; set; }
    }

    public class FeatureExtractor
    {
        private static readonly int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        // edgeMap: 1차원 byte[] 배열, width: 이미지 너비, height: 이미지 높이
        public static List<FeaturePoint> ExtractFeaturePoints(byte[] edgeMap, int width, int height)
        {
            bool[] visited = new bool[width * height];
            List<FeaturePoint> features = new List<FeaturePoint>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    if (edgeMap[idx] > 0 && !visited[idx])
                    {
                        FeaturePoint fp = new FeaturePoint();
                        fp.Position = new Point(x, y);

                        // 주변 이웃점 탐색
                        for (int k = 0; k < 8; k++)
                        {
                            int nx = x + dx[k];
                            int ny = y + dy[k];

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                int nIdx = ny * width + nx;
                                if (edgeMap[nIdx] > 0)
                                {
                                    fp.Neighbors.Add(new Point(nx, ny));

                                    // 방향 벡터 계산 (중심 -> 이웃)
                                    double angle = Math.Atan2(ny - y, nx - x);
                                    fp.DirectionVectors.Add(angle);
                                }
                            }
                        }

                        visited[idx] = true;
                        features.Add(fp);
                    }
                }
            }

            return features;
        }
    }

    public class GeometricSignatureGenerator
    {
        // numBins: 히스토그램 bin 수 (예: 8방향 → 8)
        public static void GenerateSignatures(List<FeaturePoint> features, int numBins = 8)
        {
            double binSize = 2 * Math.PI / numBins; // 전체 원 360°를 bin 수로 나눔

            foreach (var f in features)
            {
                int[] histogram = new int[numBins];

                foreach (var angle in f.DirectionVectors)
                {
                    // angle을 0~2π 범위로 변환
                    double normalized = angle;
                    if (normalized < 0)
                        normalized += 2 * Math.PI;

                    int bin = (int)(normalized / binSize);
                    if (bin >= numBins) bin = numBins - 1;

                    histogram[bin]++;
                }

                f.GeometricSignature = histogram;
            }
        }

        // 히스토그램을 shift만큼 원형시프트한 새 배열 반환
        public static double[] CircularShift(double[] hist, int shift)
        {
            int n = hist.Length;
            var outH = new double[n];
            for (int i = 0; i < n; i++)
                outH[i] = hist[(i - shift + n) % n];
            return outH;
        }

        // 두 히스토그램 간의 코사인 유사도 (둘 다 정규화되어있다면 단순 내적)
        public static double CosineSimilarity(double[] a, double[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return 0.0;
            double dot = 0;
            double na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            double denom = Math.Sqrt(na) * Math.Sqrt(nb);
            if (denom < 1e-12) return 0.0;
            return dot / denom;
        }

        // 두 히스토그램 사이의 최적 회전(shift)과 그 유사도 반환
        // 반환값: Tuple(bestSimilarity, bestShift)
        public static (double bestSim, int bestShift) BestShiftSimilarity(double[] a, double[] b)
        {
            int n = a.Length;
            double bestSim = -1.0;
            int bestShift = 0;
            for (int shift = 0; shift < n; shift++)
            {
                var bShift = CircularShift(b, shift);
                double sim = CosineSimilarity(a, bShift);
                if (sim > bestSim)
                {
                    bestSim = sim;
                    bestShift = shift;
                }
            }
            return (bestSim, bestShift);
        }
    }
    // ========== 템플릿 모델 구성 ==========
    public static class TemplateBuilder
    {
        // 템플릿 edgeMap에서 특징점 추출 + signature 생성하여 TemplateModel 반환
        public static TemplateModel BuildFromEdgeMap(byte[] templateEdgeMap, int width, int height, int numBins = 8)
        {
            var features = FeatureExtractor.ExtractFeaturePoints(templateEdgeMap, width, height);
            GeometricSignatureGenerator.GenerateSignatures(features, numBins);

            return new TemplateModel { Features = features, NumBins = numBins };
        }
    }


    // ========== 모델 매칭 (투표 기반) ==========
    public static class ModelMatcher
    {
        // 매칭 결과: translation 좌표로 투표를 집계
        // thresholdSim: 최소 유사도(0..1) — 이보다 크면 매칭으로 간주
        // returns: top N MatchCandidate (translation, votes, avgScore)
        public static List<MatchCandidate> MatchTemplate(
            TemplateModel template,
            byte[] imageEdgeMap, int imageWidth, int imageHeight,
            int voteWindow = 3, // 투표 윈도우 으로 이웃 translation을 합산할 때 반지름
            double thresholdSim = 0.7, // 유사도 임계값 (튜닝 필요)
            int topK = 5)
        {
            // 1) 이미지 특징점과 시그니처 생성
            var imageFeatures = FeatureExtractor.ExtractFeaturePoints(imageEdgeMap, imageWidth, imageHeight);
            GeometricSignatureGenerator.GenerateSignatures(imageFeatures, template.NumBins);

            // 2) 투표 테이블 (dictionary: (tx,ty) -> (votes, scoreSum))
            var voteMap = new Dictionary<(int tx, int ty), (int votes, double scoreSum)>();

            // 3) 모든 쌍에 대해 best shift 와 similarity 계산
            foreach (var imgF in imageFeatures)
            {
                if (imgF.GeometricSignature == null) continue;
                foreach (var tmplF in template.Features)
                {
                    if (tmplF.GeometricSignature == null) continue;

                    //var (bestSim, bestShift) = GeometricSignatureGenerator.BestShiftSimilarity(imgF.GeometricSignature, tmplF.GeometricSignature);

                    var (bestSim, bestShift) = GeometricSignatureGenerator.BestShiftSimilarity(Array.ConvertAll(imgF.GeometricSignature, x => (double)x), Array.ConvertAll(tmplF.GeometricSignature, x => (double)x));

                    if (bestSim >= thresholdSim)
                    {
                        // 템플릿 기준 위치는 tmplF.Position (템플릿에서의 좌표)
                        // 회전은 히스토그램 shift로 처리했으므로, 위치는 템플릿 좌표 그대로 사용
                        // translation = 이미지 feature 위치 - 템플릿 feature 위치
                        int tx = (int)(imgF.Position.X - tmplF.Position.X);
                        int ty = (int)(imgF.Position.Y - tmplF.Position.Y);

                        // 반올림(Int) translation을 voteMap에 누적
                        var key = (tx, ty);

                        if (!voteMap.ContainsKey(key))
                            voteMap[key] = (0, 0.0);

                        var curr = voteMap[key];
                        curr.votes += 1;
                        curr.scoreSum += bestSim;
                        voteMap[key] = curr;
                    }
                }
            }

            if (voteMap.Count == 0) return new List<MatchCandidate>(); // 매칭 없음

            // 4) 근접한 translation들을 합쳐 peak를 만들기 (voteWindow 반지름 이용)
            // 먼저 모든 keys를 리스트화
            var keys = voteMap.Keys.ToList();
            var aggregated = new Dictionary<(int tx, int ty), (int votes, double scoreSum)>();

            foreach (var k in keys)
            {
                // 이미 처리된 인접 키는 skip
                if (aggregated.ContainsKey(k)) continue;

                int sumVotes = 0; double sumScore = 0;
                // 합산 영역: (tx - voteWindow .. tx + voteWindow)
                for (int dx = -voteWindow; dx <= voteWindow; dx++)
                    for (int dy = -voteWindow; dy <= voteWindow; dy++)
                    {
                        var nk = (k.tx + dx, k.ty + dy);
                        if (voteMap.TryGetValue(nk, out var v))
                        {
                            sumVotes += v.votes;
                            sumScore += v.scoreSum;
                        }
                    }

                aggregated[k] = (sumVotes, sumScore);
            }

            // 5) 후보 정렬 (votes 우선, 그 다음 평균 score)
            var candidates = aggregated
                .Select(kv => new MatchCandidate
                {
                    Translation = new Point(kv.Key.tx, kv.Key.ty),
                    Votes = kv.Value.votes,
                    AvgScore = kv.Value.votes > 0 ? kv.Value.scoreSum / kv.Value.votes : 0.0
                })
                .OrderByDescending(c => c.Votes)
                .ThenByDescending(c => c.AvgScore)
                .Take(topK)
                .ToList();

            return candidates;
        }
    }

    [DataContract]
    [SerializableAttribute()]
    public class ByteImage : ByteImageBase
    {
        protected class Visited
        {
            public bool bVisitedFlag;
            public PointCoordinates ptReturnPoint;

            public Visited()
            {
                bVisitedFlag = false;
                ptReturnPoint = new PointCoordinates();
            }
        }

        #region fields
        private int _Offset;
        private BitmapSource bmpSource = null;
        #endregion

        #region propertise
        public BitmapSource BitmapSource
        {
            get
            {
                var pixels = this.Data;
                BitmapPalette myPalette = BitmapPalettes.Gray256;
                bmpSource = BitmapSource.Create(dimension.Width, dimension.Height, 96, 96, System.Windows.Media.PixelFormats.Gray8, myPalette, pixels, dimension.Pitch); ;

                return bmpSource;
            }
        }
        public int Width
        {
            get { return dimension.Width; }
        }
        public int Height
        {
            get { return dimension.Height; }
        }
        public int Pitch
        {
            get { return dimension.Pitch; }
        }
        public int Offset
        {
            get { return _Offset; }

            set
            {
                _Offset = value;
            }
        }
        public byte this[int index]
        {
            get
            {
                return Data[index + Offset];
            }
            set
            {
                Data[index + Offset] = value;
            }
        }
        public byte this[int x, int y]
        {
            get
            {
                return Data[y * this.Dimension.Pitch + x + this.Offset];
            }
            set
            {
                Data[y * this.Dimension.Pitch + x + this.Offset] = value;
            }
        }
        #endregion

        #region constructors
        public ByteImage()
        {
            _Offset = 0;
        }
        public ByteImage(string path)
        {
            Load(path);
        }
        public ByteImage(int width, int height)
            : base(width, height)
        {
            _Offset = 0;
        }
        public ByteImage(int width, int height, int pitch, byte[] data, int offset)
            : base(width, height, pitch, data)
        {
            this._Offset = offset;
        }
        public ByteImage(ImageDimension dimension)
            : base(dimension)
        {
            _Offset = 0;
        }
        public ByteImage(ImageDimension dimension, byte[] data, int offset)
            : base(dimension, data)
        {
            this._Offset = offset;
        }
        #endregion

        #region methods - image processing
        public List<PixelCoordinates> GetHorizontalEdgeArray()
        {
            for (int i = 0; i < Width; i++)
            {

                for (int j = 0; j < Height; j++)
                {

                }

                for (int j = 0; j < Height; j++)
                {

                }
            }

            return new List<PixelCoordinates>();
        }

        public List<PixelCoordinates> GetVerticalEdgeArray()
        {
            List<PixelCoordinates> pos = new List<PixelCoordinates>();

            //List<int> temp = new List<int>();
            for (int j = 0; j < Height; j++)
            {
                int left = 0;
                int right = 0;
                for (int i = 0; i < Width; i++)
                {
                    if (Data[j * Width + i] == 255)
                    {
                        left = i;
                        break;
                    }
                }

                for (int i = Width - 1; i > 0; i--)
                {
                    if (Data[j * Width + i] == 255)
                    {
                        right = i;
                        break;
                    }
                }

                if (left > right)
                {
                    continue;
                }

                PixelCoordinates p = new PixelCoordinates((left + right) / 2, j);

                pos.Add(p);
            }

            return pos;
        }


        public List<PixelCoordinates> LeftSearchEdgeArray(BlobTypes type)
        {
            //List<PixelCoordinates> pos = new List<PixelCoordinates>();

            //int blob_value = type == BlobTypes.Black ? 0 : 255;

            //for (int j = 0; j < Height; j++)
            //{
            //    for (int i = 0; i < Width; i++)
            //    {
            //        if (Data[j * Width + i] == blob_value)
            //        {
            //            PixelCoordinates pc = new PixelCoordinates(i, j);
            //            pos.Add(pc);
            //            break;
            //        }
            //    }
            //}

            //return pos;


            return LeftSearchEdgeArray(0, 0, Width, Height, type);
        }

        public List<PixelCoordinates> LeftSearchEdgeArray(int L, int T, int R, int B, BlobTypes type)
        {
            if (L < 0 || R > Width || T < 0 || B > Height)
                return null;

            List<PixelCoordinates> pos = new List<PixelCoordinates>();

            int blob_value = type == BlobTypes.Black ? 0 : 255;

            for (int j = T; j < B; j++)
            {
                for (int i = L; i < R; i++)
                {
                    if (Data[j * Width + i] == blob_value)
                    {
                        PixelCoordinates pc = new PixelCoordinates(i, j);
                        pos.Add(pc);
                        break;
                    }
                }
            }

            return pos;
        }

        public List<PixelCoordinates> RightSearchEdgeArray(BlobTypes type)
        {
            return RightSearchEdgeArray(0, 0, Width, Height, type);
            //List<PixelCoordinates> pos = new List<PixelCoordinates>();

            //int blob_value = type == BlobTypes.Black ? 0 : 255;

            //for (int j = 0; j < Height; j++)
            //{
            //    for (int i = Width - 1; i > 0; i--)
            //    {
            //        if (Data[j * Width + i] == blob_value)
            //        {
            //            PixelCoordinates pc = new PixelCoordinates(i, j);
            //            pos.Add(pc);
            //            break;
            //        }
            //    }
            //}

            //return pos;
        }

        public List<PixelCoordinates> RightSearchEdgeArray(int L, int T, int R, int B, BlobTypes type)
        {
            if (L < 0 || R > Width || T < 0 || B > Height)
                return null;

            List<PixelCoordinates> pos = new List<PixelCoordinates>();

            int blob_value = type == BlobTypes.Black ? 0 : 255;

            for (int j = T; j < B; j++)
            {
                for (int i = R; i > L; i--)
                {
                    if (Data[j * Width + i] == blob_value)
                    {
                        PixelCoordinates pc = new PixelCoordinates(i, j);
                        pos.Add(pc);
                        break;
                    }
                }
            }

            return pos;
        }
                
        public List<PixelCoordinates> TopSearchEdgeArray(BlobTypes type)
        {
            return TopSearchEdgeArray(0, 0, Width, Height, type);
        }

        public List<PixelCoordinates> TopSearchEdgeArray(int L, int T, int R, int B, BlobTypes type)
        {
            if (L < 0 || R > Width || T < 0 || B > Height)
                return null;

            List<PixelCoordinates> pos = new List<PixelCoordinates>();

            int blob_value = type == BlobTypes.Black ? 0 : 255;

            for (int i = L; i < R; i++)
            {
                for (int j = T; j < B; j++)
                {
                    if (Data[j * Width + i] == blob_value)
                    {
                        PixelCoordinates pc = new PixelCoordinates(i, j);
                        pos.Add(pc);
                        break;
                    }
                }
            }

            return pos;
        }

        public List<PixelCoordinates> BottomSearchEdgeArray(BlobTypes type)
        {
            return BottomSearchEdgeArray(0, 0, Width, Height, type);
        }

        public List<PixelCoordinates> BottomSearchEdgeArray(int L, int T, int R, int B, BlobTypes type)
        {
            if (L < 0 || R > Width || T < 0 || B > Height)
                return null;

            List<PixelCoordinates> pos = new List<PixelCoordinates>();

            int blob_value = type == BlobTypes.Black ? 0 : 255;

            for (int i = L; i < R; i++)
            {
                for (int j = B; j > T; j--)
                {
                    if (Data[j * Width + i] == blob_value)
                    {
                        PixelCoordinates pc = new PixelCoordinates(i, j);
                        pos.Add(pc);
                        break;
                    }
                }
            }

            return pos;
        }




        public ByteImage ErosionHorizontal()
        {
            var source = Data;
            var copy = Copy();
            var destination = copy.Data;

            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (source[j * Width + i] != 0)
                    {
                        if (source[j * Width + (i - 1)] == 0 || source[j * Width + (i + 1)] == 0)
                            destination[j * Width + i] = 0;
                    }

                }
            }
            return copy;
        }

        public ByteImage ErosionVertical()
        {
            var source = Data;
            var copy = Copy();
            var destination = copy.Data;

            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (source[j * Width + i] != 0)
                    {
                        if (source[(j - 1) * Width + i] == 0 || source[(j + 1) * Width + i] == 0)
                            destination[j * Width + i] = 0;
                    }
                }
            }
            return copy;
        }

        public ByteImage DilationHorizontal1()
        {
            var source = Data;
            var copy = Copy();
            var destination = copy.Data;

            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (source[j * Width + i] == 0)
                    {
                        if (source[j * Width + (i - 1)] != 0 || source[j * Width + (i + 1)] != 0)
                            destination[j * Width + i] = 255;
                    }

                }
            }

            return copy;
        }

        public ByteImage DilationHorizontal()
        {
            var source = Data;
            var copy = Copy();
            var destination = copy.Data;

            //Parallel.For(1, Height - 1, j =>
            //{
            //    Parallel.For(1, Width - 1, i =>
            //    {
            //        if (source[j * Width + i] == 0)
            //        {
            //            if (source[j * Width + (i - 1)] != 0 || source[j * Width + (i + 1)] != 0)
            //                destination[j * Width + i] = 255;
            //        }
            //    });
            //});

            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (source[j * Width + i] == 0)
                    {
                        if (source[j * Width + (i - 1)] != 0 || source[j * Width + (i + 1)] != 0)
                            destination[j * Width + i] = 255;
                    }

                }
            }

            return copy;
        }
        public ByteImage DilationVertical()
        {
            var source = Data;
            var copy = Copy();
            var destination = copy.Data;

            //Parallel.For(1, Height - 1, j =>
            //{
            //    Parallel.For(1, Width - 1, i =>
            //    {
            //        if (source[j * Width + i] == 0)
            //        {
            //            if (source[(j - 1) * Width + i] != 0 || source[(j + 1) * Width + i] != 0)
            //                destination[j * Width + i] = 255;
            //        }
            //    });
            //});


            for (int j = 1; j < Height - 1; j++)
            {
                for (int i = 1; i < Width - 1; i++)
                {
                    if (source[j * Width + i] == 0)
                    {
                        if (source[(j - 1) * Width + i] != 0 || source[(j + 1) * Width + i] != 0)
                            destination[j * Width + i] = 255;
                    }
                }
            }


            return copy;
        }

        public ByteImage Sub(ByteImage image)
        {
            if (image.Data == null || Width != image.Width || Height != image.Height)
                return null;

            ByteImage copy = new ByteImage(Width, Height);

            Parallel.For(0, Data.Length, i =>
            {
                copy.Data[i] = (byte)MathExtensions.limit(Data[i] - image.Data[i], 0, 255);
            });

            return copy;
        }
        public ByteImage Sub1(ByteImage image)
        {
            if (image.Data == null || Width != image.Width || Height != image.Height)
                return null;

            ByteImage copy = new ByteImage(Width, Height);

            //Parallel.For(0, Data.Length, i =>
            //{
            //    copy.Data[i] = (byte)MathExtensions.limit(Data[i] - image.Data[i], 0, 255);
            //});

            for (int i = 0; i < Data.Length; i++)
            {
                copy.Data[i] = (byte)MathExtensions.limit(Data[i] - image.Data[i], 0, 255);
            }

            return copy;
        }
        public ByteImage Add(ByteImage image)
        {
            if (image.Data == null || Width != image.Width || Height != image.Height)
                return null;

            ByteImage copy = new ByteImage(Width, Height);

            Parallel.For(0, Data.Length, i =>
            {
                copy.Data[i] = (byte)MathExtensions.limit(Data[i] + image.Data[i], 0, 255);
            });

            return copy;
        }
        public double Average()
        {
            return data.Average(s => s);
        }
        public int Max()
        {
            return (int)data.Max();
        }
        public int Min()
        {
            return (int)data.Min();
        }

        public int Sum(RoiRectangle roi = null)
        {
            if (roi == null)
            {
                return Sum();
            }

            ByteImage crop_image = Crop(roi) as ByteImage;

            return crop_image.Sum();
        }

        protected int Sum()
        {
            int sum = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    sum += Data[j * Width + i];
                }
            }

            return sum;
        }

        public ByteImage SobelEdge(RoiRectangle roi = null)
        {
            if (roi == null)
            {
                roi = new RoiRectangle(0, 0, dimension.Height, dimension.Width);
            }
            int roix = roi.Left;
            int roiy = roi.Top;
            int width = roi.Width;
            int heigh = roi.Height;

            ByteImage sobel_image = new ByteImage(width, heigh);

            var sobel_data = sobel_image.Data;// new byte[width * heigh];

            double h1 = 0.0;
            double h2 = 0.0;
            double hval = 0.0;

            int pitch = dimension.Pitch;
            for (int y = 1; y < heigh - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    h1 = -data[((y - 1 + roiy) * pitch) + (x - 1 + roix)] - 2 * data[((y - 1 + roiy) * pitch) + (x + roix)] - data[((y - 1 + roiy) * pitch) + (x + 1 + roix)]
                        + data[((y + 1 + roiy) * pitch) + (x - 1 + roix)] + 2 * data[((y + 1 + roiy) * pitch) + (x + roix)] + data[((y + 1 + roiy) * pitch) + (x + 1 + roix)];

                    h2 = -data[((y - 1 + roiy) * pitch) + (x - 1 + roix)] - 2 * data[((y + roiy) * pitch) + (x - 1 + roix)] - data[((y + 1 + roiy) * pitch) + (x - 1 + roix)]
                        + data[((y - 1 + roiy) * pitch) + (x + 1 + roix)] + 2 * data[((y + roiy) * pitch) + (x + 1 + roix)] + data[((y + 1 + roiy) * pitch) + (x + 1 + roix)];

                    hval = Math.Sqrt((double)h1 * h1 + h2 * h2);

                    var val = Math.Max(0, Math.Min(255, hval));

                    sobel_data[(y * width) + x] = (byte)val;
                }
            }

            return sobel_image;// new ByteImage(new ImageDimension(width, heigh, width), sobel_data, 0);
        }
        public ByteImage SobelEdge(ROIDirections direction, RoiRectangle roi = null)
        {
            if (roi == null)
            {
                roi = new RoiRectangle(0, 0, dimension.Height, dimension.Width);
            }
            int roix = roi.Left;
            int roiy = roi.Top;
            int width = roi.Width;
            int heigh = roi.Height;

            ByteImage sobel_image = new ByteImage(width, heigh);

            var sobel_data = sobel_image.Data;// new byte[width * heigh];
            Array.Clear(sobel_data, 0, width * heigh);

            double h1 = 0.0;
            double h2 = 0.0;
            double hval = 0.0;

            int pitch = dimension.Pitch;

            if (direction == ROIDirections.Vertical)
            {
                for (int y = 1; y < heigh - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        // x direction
                        h2 = -data[((y - 1 + roiy) * pitch) + (x - 1 + roix)] - 2 * data[((y + roiy) * pitch) + (x - 1 + roix)] - data[((y + 1 + roiy) * pitch) + (x - 1 + roix)]
                            + data[((y - 1 + roiy) * pitch) + (x + 1 + roix)] + 2 * data[((y + roiy) * pitch) + (x + 1 + roix)] + data[((y + 1 + roiy) * pitch) + (x + 1 + roix)];

                        hval = Math.Sqrt((double)h2 * h2);

                        var val = Math.Max(0, Math.Min(255, hval));

                        sobel_data[(y * width) + x] = (byte)val;
                    }
                }
            }
            else
            {
                for (int y = 1; y < heigh - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        // y direction
                        h1 = -data[((y - 1 + roiy) * pitch) + (x - 1 + roix)] - 2 * data[((y - 1 + roiy) * pitch) + (x + roix)] - data[((y - 1 + roiy) * pitch) + (x + 1 + roix)]
                            + data[((y + 1 + roiy) * pitch) + (x - 1 + roix)] + 2 * data[((y + 1 + roiy) * pitch) + (x + roix)] + data[((y + 1 + roiy) * pitch) + (x + 1 + roix)];

                        hval = Math.Sqrt((double)h1 * h1);

                        var val = Math.Max(0, Math.Min(255, hval));

                        sobel_data[(y * width) + x] = (byte)val;
                    }
                }
            }
            return sobel_image;
        }

        public int GetAutoThresold()
        {
            double[] hist = new double[256];
            int[] lTmp = new int[256];

            //Array.Clear(hist, 0, hist.Length);
            //Array.Clear(lTmp, 0, lTmp.Length);

            //for (int i = 0; i < Height; i++)
            //{
            //    for (int j = 0; j < Width; j++)
            //    {
            //        var value = data[i * Pitch + j];
            //        lTmp[value]++;
            //    }
            //}
            Parallel.For(0, Height, i =>
            {
                Parallel.For(0, Width, j =>
                {
                    var value = data[i * Pitch + j];
                    lTmp[value]++;
                });
            });



            int nSize = Width * Height;

            for (int i = 0; i < 256; i++)
                hist[i] = (double)lTmp[i] / nSize;

            int T, Told;

            double sum = 0.0;

            for (int i = 0; i < 256; i++)
                sum += (i * hist[i]);

            T = (int)sum;
            ////-------------------------------------------------------------------------
            //// 반복에 의한 임계값 결정
            ////-------------------------------------------------------------------------

            double a1, b1, u1, a2, b2, u2;
            do
            {
                Told = T;

                a1 = b1 = 0;
                for (int i = 0; i <= Told; i++)
                {
                    a1 += (i * hist[i]);
                    b1 += hist[i];
                }
                u1 = a1 / b1;

                a2 = b2 = 0;
                for (int i = Told + 1; i < 256; i++)
                {
                    a2 += (i * hist[i]);
                    b2 += hist[i];
                }
                u2 = a2 / b2;

                if (b1 == 0) b1 = 1.0;
                if (b2 == 0) b2 = 1.0;

                T = (int)((u1 + u2) / 2);
            } while (T != Told);

            return T;
        }
        public ByteImage Binarization(int min, int max, BinTypes type)
        {
            var bin_image = new ByteImage(Width, Height);

            var bin_data = bin_image.Data;// new byte[width * heigh];

            //for(int i = 0; i < data.Length; i++)
            Parallel.For(0, data.Length, i =>
            {
                var value = data[i];

                if (type == BinTypes.OutSide)
                {
                    if (value >= max || value <= min)
                        bin_data[i] = 255;
                    else
                        bin_data[i] = 0;
                }
                else
                {
                    if (value <= max && value >= min)
                        bin_data[i] = 255;
                    else
                        bin_data[i] = 0;
                }
            });

            return bin_image;
        }

        public ByteImage FillHole(BlobTypes blob_type, int area)
        {
            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;
            var erase_value = blob_type == BlobTypes.Black ? 255 : 0;

            int labelCount = 0;
            byte[] new_data = new byte[Width * Height];
            byte[] label_data = new byte[Width * Height];

            Array.Copy(data, new_data, data.Length);

            int[,] visited = new int[Height, Width];

            int currentLabel = 0;
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            Dictionary<int, int> dic_area = new Dictionary<int, int>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (data[y * Width + x] == compare_value && visited[y, x] == 0)
                    {
                        currentLabel++;
                        Queue<(int, int)> q = new Queue<(int, int)>();
                        q.Enqueue((x, y));
                        visited[y, x] = currentLabel;

                        while (q.Count > 0)
                        {
                            var (cx, cy) = q.Dequeue();
                            label_data[cy * Width + cx] = (byte)currentLabel;

                            if (!dic_area.ContainsKey(currentLabel))
                                dic_area[currentLabel] = 1;
                            else
                                dic_area[currentLabel]++;

                            for (int dir = 0; dir < 4; dir++)
                            {
                                int nx = cx + dx[dir];
                                int ny = cy + dy[dir];
                                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                                {
                                    if (data[ny * Width + nx] == compare_value && visited[ny, nx] == 0)
                                    {
                                        visited[ny, nx] = currentLabel;
                                        q.Enqueue((nx, ny));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<int> erase_label = new List<int>();
            foreach (var d in dic_area)
            {
                if (d.Value <= area)
                {
                    erase_label.Add(d.Key);
                }
            }




            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    foreach (var label in erase_label)
                    {
                        if (label_data[j * Width + i] == label)
                        {
                            new_data[j * Width + i] = (byte)erase_value;
                        }
                    }
                }
            }


            ByteImage save_test = new ByteImage(Width, Height, Width, label_data, 0);

            labelCount = currentLabel;

            return new ByteImage(Width, Height, Width, new_data, 0);
        }

        private void EnqueueIfZero(Queue<(int x, int y)> queue, bool[,] visited, byte[] data, int x, int y)
        {
            if (data[y * Width + x] == 0 && !visited[y, x])
            {
                visited[y, x] = true;
                queue.Enqueue((x, y));
            }
        }

        public static List<BlobData> Blob(in ByteImage bin_image, int min_area, int max_area, BlobTypes blob_type)
        {
            //Stopwatch sw = new Stopwatch();

            //sw.Restart();
            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;

            var bin_data = bin_image.Data;

            int[] map = new int[bin_image.Width * bin_image.Height];

            Dictionary<int, int[]> eq_tbl = new Dictionary<int, int[]>();

            int label = 0, maxl, minl, min_eq, max_eq;

            //AppLogger.Info()("1 time : {0}", sw.ElapsedMilliseconds);

            //sw.Restart();

            for (int j = 1; j < bin_image.Height; j++)
            {
                for (int i = 1; i < bin_image.Width; i++)
                {
                    int current = j * bin_image.Width + i;
                    if (bin_data[current] == compare_value)
                    {
                        int top = (j - 1) * bin_image.Width + i; // 위쪽
                        int left = j * bin_image.Width + (i - 1); // 좌측

                        // 바로 위 픽셀과 왼쪽 픽셀 모두에 레이블이 존재하는 경우
                        if ((map[top] != 0) && (map[left] != 0))
                        {
                            if (map[top] == map[left])
                            {
                                // 두 레이블이 서로 같은 경우
                                map[current] = map[top];
                            }
                            else
                            {
                                // 두 레이블이 서로 다른 경우, 작은 레이블을 부여
                                maxl = Math.Max(map[top], map[left]);
                                minl = Math.Min(map[top], map[left]);

                                map[current] = minl;

                                // 등가 테이블 조정
                                min_eq = Math.Min(eq_tbl[maxl][1], eq_tbl[minl][1]);
                                max_eq = Math.Max(eq_tbl[maxl][1], eq_tbl[minl][1]);

                                eq_tbl[eq_tbl[max_eq][1]][1] = min_eq;
                            }
                        }
                        else if (map[top] != 0)
                        {
                            // 바로 위 픽셀에만 레이블이 존재할 경우
                            map[current] = map[top];
                        }
                        else if (map[left] != 0)
                        {
                            // 바로 왼쪽 픽셀에만 레이블이 존재할 경우
                            map[current] = map[left];
                        }
                        else
                        {
                            // 이웃에 레이블이 존재하지 않으면 새로운 레이블을 부여
                            label++;
                            map[current] = label;

                            eq_tbl[label] = new int[2];
                            eq_tbl[label][0] = label;
                            eq_tbl[label][1] = label;
                        }

                    }
                }
            }
            //AppLogger.Info()("2 time : {0}", sw.ElapsedMilliseconds);
            //-------------------------------------------------------------------------
            // 등가 테이블 정리
            //-------------------------------------------------------------------------
            //sw.Restart();
            int temp;
            for (int i = 1; i <= label; i++)
            {
                temp = eq_tbl[i][1];
                if (temp != eq_tbl[i][0])
                    eq_tbl[i][1] = eq_tbl[temp][1];
            }
            //AppLogger.Info()("3 time : {0}", sw.ElapsedMilliseconds);
            //sw.Restart();
            // 등가 테이블의 레이블을 1부터 차례대로 증가시키기

            int[] hash = new int[label + 1];

            for (int i = 1; i <= label; i++)
            {
                hash[eq_tbl[i][1]] = eq_tbl[i][1];
            }

            int cnt = 1;
            for (int i = 1; i <= label; i++)
                if (hash[i] != 0)
                    hash[i] = cnt++;

            for (int i = 1; i <= label; i++)
                eq_tbl[i][1] = hash[eq_tbl[i][1]];

            //AppLogger.Info()("4 time : {0}", sw.ElapsedMilliseconds);
            //sw.Restart();

            int width = bin_image.Width;
            int height = bin_image.Height;

            bool[] bFirstFlag = new bool[cnt];
            BlobData[] blobs = new BlobData[cnt];

            for (int i = 0; i < cnt; i++)
            {
                blobs[i] = new BlobData();
                bFirstFlag[i] = true;
            }

            int label_idx = 0;

            for (int j = 1; j < height; j++)
            {
                for (int i = 1; i < width; i++)
                {
                    int current = j * width + i;

                    if (map[current] != 0)
                    {
                        temp = map[current];

                        if (temp != 0)
                        {
                            label_idx = eq_tbl[temp][1];

                            if (bFirstFlag[label_idx])
                            {
                                blobs[label_idx].Area++;

                                blobs[label_idx].Left = i;
                                blobs[label_idx].Top = j;
                                blobs[label_idx].Width = 0;
                                blobs[label_idx].Height = 0;

                                bFirstFlag[label_idx] = false;
                            }
                            else
                            {
                                double left = blobs[label_idx].Left;
                                double right = left + blobs[label_idx].Width;
                                double top = blobs[label_idx].Top;
                                double bottom = top + blobs[label_idx].Height;

                                if (left >= i) left = i;
                                if (right <= i) right = i;
                                if (top >= j) top = j;
                                if (bottom <= j) bottom = j;

                                blobs[label_idx].Left = (int)left;
                                blobs[label_idx].Top = (int)top;
                                blobs[label_idx].Width = (int)(right - left);
                                blobs[label_idx].Height = (int)(bottom - top);
                                blobs[label_idx].Area++;
                            }
                        }
                    }
                }
            }

            //AppLogger.Info()("5 time : {0}", sw.ElapsedMilliseconds);
            //sw.Restart();

            return blobs.Where(s => s != null && s.Area > min_area && s.Area < max_area).ToList();
        }

        public static List<BlobData> Blob(in ByteImage bin_image, int min_area, int max_area, int min_width, int max_width, int min_height, int max_height, BlobTypes blob_type)
        {

            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;

            var bin_data = bin_image.Data;

            int[] map = new int[bin_image.Width * bin_image.Height];

            Dictionary<int, int[]> eq_tbl = new Dictionary<int, int[]>();

            int label = 0, maxl, minl, min_eq, max_eq;

            for (int j = 1; j < bin_image.Height; j++)
            {
                for (int i = 1; i < bin_image.Width; i++)
                {
                    int current = j * bin_image.Width + i;
                    if (bin_data[current] == compare_value)
                    {
                        int top = (j - 1) * bin_image.Width + i; // 위쪽
                        int left = j * bin_image.Width + (i - 1); // 좌측

                        // 바로 위 픽셀과 왼쪽 픽셀 모두에 레이블이 존재하는 경우
                        if ((map[top] != 0) && (map[left] != 0))
                        {
                            if (map[top] == map[left])
                            {
                                // 두 레이블이 서로 같은 경우
                                map[current] = map[top];
                            }
                            else
                            {
                                // 두 레이블이 서로 다른 경우, 작은 레이블을 부여
                                maxl = Math.Max(map[top], map[left]);
                                minl = Math.Min(map[top], map[left]);

                                map[current] = minl;

                                // 등가 테이블 조정
                                min_eq = Math.Min(eq_tbl[maxl][1], eq_tbl[minl][1]);
                                max_eq = Math.Max(eq_tbl[maxl][1], eq_tbl[minl][1]);

                                eq_tbl[eq_tbl[max_eq][1]][1] = min_eq;
                            }
                        }
                        else if (map[top] != 0)
                        {
                            // 바로 위 픽셀에만 레이블이 존재할 경우
                            map[current] = map[top];
                        }
                        else if (map[left] != 0)
                        {
                            // 바로 왼쪽 픽셀에만 레이블이 존재할 경우
                            map[current] = map[left];
                        }
                        else
                        {
                            // 이웃에 레이블이 존재하지 않으면 새로운 레이블을 부여
                            label++;
                            map[current] = label;

                            eq_tbl[label] = new int[2];
                            eq_tbl[label][0] = label;
                            eq_tbl[label][1] = label;
                        }

                    }
                }
            }

            //-------------------------------------------------------------------------
            // 등가 테이블 정리
            //-------------------------------------------------------------------------
            //sw.Restart();
            int temp;
            for (int i = 1; i <= label; i++)
            {
                temp = eq_tbl[i][1];
                if (temp != eq_tbl[i][0])
                    eq_tbl[i][1] = eq_tbl[temp][1];
            }

            // 등가 테이블의 레이블을 1부터 차례대로 증가시키기

            int[] hash = new int[label + 1];

            for (int i = 1; i <= label; i++)
            {
                hash[eq_tbl[i][1]] = eq_tbl[i][1];
            }

            int cnt = 1;
            for (int i = 1; i <= label; i++)
                if (hash[i] != 0)
                    hash[i] = cnt++;

            for (int i = 1; i <= label; i++)
                eq_tbl[i][1] = hash[eq_tbl[i][1]];

            int width = bin_image.Width;
            int height = bin_image.Height;

            bool[] bFirstFlag = new bool[cnt];
            BlobData[] blobs = new BlobData[cnt];

            for (int i = 0; i < cnt; i++)
            {
                blobs[i] = new BlobData();
                bFirstFlag[i] = true;
            }

            int label_idx = 0;

            for (int j = 1; j < height; j++)
            {
                for (int i = 1; i < width; i++)
                {
                    int current = j * width + i;

                    if (map[current] != 0)
                    {
                        temp = map[current];

                        if (temp != 0)
                        {
                            label_idx = eq_tbl[temp][1];

                            if (bFirstFlag[label_idx])
                            {
                                blobs[label_idx].Area = 1;

                                blobs[label_idx].Left = i;
                                blobs[label_idx].Top = j;
                                blobs[label_idx].Width = 1;
                                blobs[label_idx].Height = 1;

                                bFirstFlag[label_idx] = false;
                            }
                            else
                            {
                                double left = blobs[label_idx].Left;
                                double right = left + blobs[label_idx].Width;
                                double top = blobs[label_idx].Top;
                                double bottom = top + blobs[label_idx].Height;

                                if (left >= i) left = i;
                                if (right <= i) right = i + 1;
                                if (top >= j) top = j;
                                if (bottom <= j) bottom = j + 1;

                                blobs[label_idx].Left = (int)left;
                                blobs[label_idx].Top = (int)top;
                                blobs[label_idx].Width = (int)(right - left);
                                blobs[label_idx].Height = (int)(bottom - top);
                                blobs[label_idx].Area++;
                            }
                        }
                    }
                }
            }
            return blobs.Where(s => s != null && s.Area > min_area && s.Area < max_area && s.Width > min_width && s.Width < max_width && s.Height > min_height && s.Height < max_height).ToList();
        }

        public static List<BlobData> Blob2(in ByteImage bin_image, int min_area, int max_area, int min_width, int max_width, int min_height, int max_height, BlobTypes blob_type)
        {
            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;

            var bin_data = bin_image.Data;

            int[] map = new int[bin_image.Width * bin_image.Height];

            Dictionary<int, int[]> eq_tbl = new Dictionary<int, int[]>();

            int label = 0, maxl, minl, min_eq, max_eq;

            for (int j = 1; j < bin_image.Height; j++)
            {
                for (int i = 1; i < bin_image.Width; i++)
                {
                    int current = j * bin_image.Width + i;
                    if (bin_data[current] == compare_value)
                    {
                        int top = (j - 1) * bin_image.Width + i; // 위쪽
                        int left = j * bin_image.Width + (i - 1); // 좌측

                        // 바로 위 픽셀과 왼쪽 픽셀 모두에 레이블이 존재하는 경우
                        if ((map[top] != 0) && (map[left] != 0))
                        {
                            if (map[top] == map[left])
                            {
                                // 두 레이블이 서로 같은 경우
                                map[current] = map[top];
                            }
                            else
                            {
                                // 두 레이블이 서로 다른 경우, 작은 레이블을 부여
                                maxl = Math.Max(map[top], map[left]);
                                minl = Math.Min(map[top], map[left]);

                                map[current] = minl;

                                // 등가 테이블 조정
                                min_eq = Math.Min(eq_tbl[maxl][1], eq_tbl[minl][1]);
                                max_eq = Math.Max(eq_tbl[maxl][1], eq_tbl[minl][1]);

                                eq_tbl[eq_tbl[max_eq][1]][1] = min_eq;
                            }
                        }
                        else if (map[top] != 0)
                        {
                            // 바로 위 픽셀에만 레이블이 존재할 경우
                            map[current] = map[top];
                        }
                        else if (map[left] != 0)
                        {
                            // 바로 왼쪽 픽셀에만 레이블이 존재할 경우
                            map[current] = map[left];
                        }
                        else
                        {
                            // 이웃에 레이블이 존재하지 않으면 새로운 레이블을 부여
                            label++;
                            map[current] = label;

                            eq_tbl[label] = new int[2];
                            eq_tbl[label][0] = label;
                            eq_tbl[label][1] = label;
                        }

                    }
                }
            }

            //-------------------------------------------------------------------------
            // 등가 테이블 정리
            //-------------------------------------------------------------------------
            //sw.Restart();
            int temp;
            for (int i = 1; i <= label; i++)
            {
                temp = eq_tbl[i][1];
                if (temp != eq_tbl[i][0])
                    eq_tbl[i][1] = eq_tbl[temp][1];
            }

            // 등가 테이블의 레이블을 1부터 차례대로 증가시키기

            int[] hash = new int[label + 1];

            for (int i = 1; i <= label; i++)
            {
                hash[eq_tbl[i][1]] = eq_tbl[i][1];
            }

            int cnt = 1;
            for (int i = 1; i <= label; i++)
                if (hash[i] != 0)
                    hash[i] = cnt++;

            for (int i = 1; i <= label; i++)
                eq_tbl[i][1] = hash[eq_tbl[i][1]];

            int width = bin_image.Width;
            int height = bin_image.Height;

            bool[] bFirstFlag = new bool[cnt];
            BlobData[] blobs = new BlobData[cnt];

            for (int i = 0; i < cnt; i++)
            {
                blobs[i] = new BlobData();
                bFirstFlag[i] = true;
            }

            int label_idx = 0;

            for (int j = 1; j < height; j++)
            {
                for (int i = 1; i < width; i++)
                {
                    int current = j * width + i;

                    if (map[current] != 0)
                    {
                        temp = map[current];

                        if (temp != 0)
                        {
                            label_idx = eq_tbl[temp][1];

                            if (bFirstFlag[label_idx])
                            {
                                blobs[label_idx].Area++;

                                blobs[label_idx].Left = i;
                                blobs[label_idx].Top = j;
                                blobs[label_idx].Width = 0;
                                blobs[label_idx].Height = 0;

                                bFirstFlag[label_idx] = false;
                            }
                            else
                            {
                                double left = blobs[label_idx].Left;
                                double right = left + blobs[label_idx].Width;
                                double top = blobs[label_idx].Top;
                                double bottom = top + blobs[label_idx].Height;

                                if (left >= i) left = i;
                                if (right <= i) right = i;
                                if (top >= j) top = j;
                                if (bottom <= j) bottom = j;

                                blobs[label_idx].Left = (int)left;
                                blobs[label_idx].Top = (int)top;
                                blobs[label_idx].Width = (int)(right - left);
                                blobs[label_idx].Height = (int)(bottom - top);
                                blobs[label_idx].Area++;
                            }
                        }
                    }
                }
            }
            return blobs.Where(s => s != null && s.Area > min_area && s.Area < max_area && s.Width > min_width && s.Width < max_width && s.Height > min_height && s.Height < max_height).ToList();
        }

        public List<BlobData> BlobProcessing(int min_thre, int max_thre, BinTypes bin_type, int min_area, int max_area, BlobTypes blob_type)
        {
            // Binarization
            var bin_image = Binarization(min_thre, max_thre, bin_type);

            // Blob
            return Blob(in bin_image, min_area, max_area, blob_type);
        }

        // Test 중
        public List<BlobData> Blob(int min_thre, int max_thre, int min_area, int max_area, BlobTypes blob_type)
        {
            // Labeling
            var bin_image = Binarization(min_thre, max_thre, BinTypes.InSide);

            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;

            int num = 0;
            int nX, nY, k, l;
            int StartX, StartY, EndX, EndY;

            var bin_data = bin_image.Data;

            //sobel_data[(y * width) + x]

            // Find connected components
            for (nY = 0; nY < Height; nY++)
            {
                for (nX = 0; nX < Width; nX++)
                {
                    if (bin_data[nY * Width + nX] == 255)       // Is this a new component?, 255 == Object
                    {
                        num++;

                        bin_data[nY * Width + nX] = (byte)num;

                        StartX = nX; StartY = nY;
                        EndX = nX; EndY = nY;

                        __NRFIndNeighbor(bin_data, Width, Height, nX, nY, ref StartX, ref StartY, ref EndX, ref EndY);

                        var area = __Area(bin_data, StartX, StartY, EndX, EndY, Width, num);

                        if (area > min_area && area < max_area)
                        {
                            for (k = StartY; k <= EndY; k++)
                            {
                                for (l = StartX; l <= EndX; l++)
                                {
                                    if (bin_data[k * Width + l] == num)
                                        bin_data[k * Width + l] = 0;
                                }
                            }
                            --num;

                            //if (num > 250)
                            //    return 0;
                        }
                    }
                }
            }


            // DetectLableingRegion

            int nLabelIndex;

            //BOOL bFirstFlag[255] = { FALSE, };
            bool[] bFirstFlag = new bool[255];
            Array.Clear(bFirstFlag, 0, 255);

            for (nY = 1; nY < Height - 1; nY++)
            {
                for (nX = 1; nX < Width - 1; nX++)
                {
                    nLabelIndex = bin_data[nY * Width + nX];

                    if (nLabelIndex != 0)   // Is this a new component?, 255 == Object
                    {
                        //if (bFirstFlag[nLabelIndex] == false)
                        //{
                        //    m_recBlobs[nLabelIndex - 1].x = nX;
                        //    m_recBlobs[nLabelIndex - 1].y = nY;
                        //    m_recBlobs[nLabelIndex - 1].width = 0;
                        //    m_recBlobs[nLabelIndex - 1].height = 0;

                        //    bFirstFlag[nLabelIndex] = true;
                        //}
                        //else
                        //{
                        //    int left = m_recBlobs[nLabelIndex - 1].x;
                        //    int right = left + m_recBlobs[nLabelIndex - 1].width;
                        //    int top = m_recBlobs[nLabelIndex - 1].y;
                        //    int bottom = top + m_recBlobs[nLabelIndex - 1].height;

                        //    if (left >= nX) left = nX;
                        //    if (right <= nX) right = nX;
                        //    if (top >= nY) top = nY;
                        //    if (bottom <= nY) bottom = nY;

                        //    m_recBlobs[nLabelIndex - 1].x = left;
                        //    m_recBlobs[nLabelIndex - 1].y = top;
                        //    m_recBlobs[nLabelIndex - 1].width = right - left;
                        //    m_recBlobs[nLabelIndex - 1].height = bottom - top;

                        //}
                    }
                }
            }
            var blobs = new List<BlobData>();

            return blobs;
        }
        protected int __NRFIndNeighbor(in byte[] DataBuf, int nWidth, int nHeight, int nPosX, int nPosY, ref int StartX, ref int StartY, ref int EndX, ref int EndY)
        {
            // m_vPoint Initialize
            var m_vPoint = new Visited[Width * Height];
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    m_vPoint[i * Width + j].bVisitedFlag = false;
                    m_vPoint[i * Width + j].ptReturnPoint.X = j;
                    m_vPoint[i * Width + j].ptReturnPoint.Y = i;
                }
            }

            PointCoordinates CurrentPoint = new PointCoordinates();

            CurrentPoint.X = nPosX;
            CurrentPoint.Y = nPosY;

            m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].bVisitedFlag = true;
            m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].ptReturnPoint.X = nPosX;
            m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].ptReturnPoint.Y = nPosY;

            while (true)
            {
                if ((CurrentPoint.X != 0) && (DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X - 1] == 255))   // -X 방향
                {
                    if (m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].bVisitedFlag == false)
                    {
                        DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X - 1] = DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];  // If so, mark it
                        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].bVisitedFlag = true;
                        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].ptReturnPoint = CurrentPoint;
                        CurrentPoint.X--;

                        if (CurrentPoint.X <= 0)
                            CurrentPoint.X = 0;

                        if (StartX >= CurrentPoint.X)
                            StartX = CurrentPoint.X;

                        continue;
                    }
                }

                if ((CurrentPoint.X != nWidth - 1) && (DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X + 1] == 255))   // -X 방향
                {
                    if (m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].bVisitedFlag == false)
                    {
                        DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X + 1] = DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];  // If so, mark it
                        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].bVisitedFlag = true;
                        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].ptReturnPoint = CurrentPoint;
                        CurrentPoint.X++;

                        if (CurrentPoint.X >= nWidth - 1)
                            CurrentPoint.X = nWidth - 1;

                        if (EndX <= CurrentPoint.X)
                            EndX = CurrentPoint.X;

                        continue;
                    }
                }

                if ((CurrentPoint.Y != 0) && (DataBuf[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X] == 255))   // -X 방향
                {
                    if (m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].bVisitedFlag == false)
                    {
                        DataBuf[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X] = DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];    // If so, mark it
                        m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].bVisitedFlag = true;
                        m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].ptReturnPoint = CurrentPoint;
                        CurrentPoint.Y--;

                        if (CurrentPoint.Y <= 0)
                            CurrentPoint.Y = 0;

                        if (StartY >= CurrentPoint.Y)
                            StartY = CurrentPoint.Y;

                        continue;
                    }
                }

                if ((CurrentPoint.Y != nHeight - 1) && (DataBuf[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X] == 255))   // -X 방향
                {
                    if (m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].bVisitedFlag == false)
                    {
                        DataBuf[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X] = DataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];    // If so, mark it
                        m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].bVisitedFlag = true;
                        m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].ptReturnPoint = CurrentPoint;
                        CurrentPoint.Y++;

                        if (CurrentPoint.Y >= nHeight - 1)
                            CurrentPoint.Y = nHeight - 1;

                        if (EndY <= CurrentPoint.Y)
                            EndY = CurrentPoint.Y;

                        continue;
                    }
                }

                if ((CurrentPoint.X == m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].ptReturnPoint.X)
                    && (CurrentPoint.Y == m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].ptReturnPoint.Y))
                {
                    break;
                }
                else
                {
                    CurrentPoint = m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].ptReturnPoint;
                }
            }

            return 0;
        }
        protected int __Area(in byte[] DataBuf, int StartX, int StartY, int EndX, int EndY, int nWidth, int nLevel)
        {
            int nArea = 0;
            int nX, nY;

            for (nY = StartY; nY < EndY; nY++)
                for (nX = StartX; nX < EndX; nX++)
                    if (DataBuf[nY * nWidth + nX] == nLevel)
                        ++nArea;

            return nArea;
        }



        public ByteImage Labeling(BlobTypes blob_type)
        {

            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;

            int labelCount = 0;
            //byte[] new_data = new byte[Width * Height];
            byte[] label_data = new byte[Width * Height];

            //Array.Copy(data, label_data, data.Length);

            int[,] visited = new int[Height, Width];

            int currentLabel = 0;
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            Dictionary<int, int> dic_area = new Dictionary<int, int>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (data[y * Width + x] == compare_value && visited[y, x] == 0)
                    {
                        currentLabel++;
                        Queue<(int, int)> q = new Queue<(int, int)>();
                        q.Enqueue((x, y));
                        visited[y, x] = currentLabel;

                        while (q.Count > 0)
                        {
                            var (cx, cy) = q.Dequeue();
                            label_data[cy * Width + cx] = (byte)currentLabel;


                            if (!dic_area.ContainsKey(currentLabel))
                                dic_area[currentLabel] = 1;
                            else
                                dic_area[currentLabel]++;

                            for (int dir = 0; dir < 4; dir++)
                            {
                                int nx = cx + dx[dir];
                                int ny = cy + dy[dir];
                                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                                {
                                    if (data[ny * Width + nx] == compare_value && visited[ny, nx] == 0)
                                    {
                                        visited[ny, nx] = currentLabel;
                                        q.Enqueue((nx, ny));
                                    }
                                }
                            }
                        }
                    }
                }
            }


            labelCount = currentLabel;

            return new ByteImage(Width, Height, Width, label_data, 0);
        }

        public ByteImage FillHole2(BlobTypes blob_type, int area)
        {
            var compare_value = blob_type == BlobTypes.Black ? 0 : 255;
            var erase_value = blob_type == BlobTypes.Black ? 255 : 0;

            int labelCount = 0;
            byte[] new_data = new byte[Width * Height];
            byte[] label_data = new byte[Width * Height];

            Array.Copy(data, new_data, data.Length);

            int[,] visited = new int[Height, Width];

            int currentLabel = 0;
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            Dictionary<int, int> dic_area = new Dictionary<int, int>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (data[y * Width + x] == compare_value && visited[y, x] == 0)
                    {
                        currentLabel++;
                        Queue<(int, int)> q = new Queue<(int, int)>();
                        q.Enqueue((x, y));
                        visited[y, x] = currentLabel;

                        while (q.Count > 0)
                        {
                            var (cx, cy) = q.Dequeue();
                            label_data[cy * Width + cx] = (byte)currentLabel;

                            if (!dic_area.ContainsKey(currentLabel))
                                dic_area[currentLabel] = 1;
                            else
                                dic_area[currentLabel]++;

                            for (int dir = 0; dir < 4; dir++)
                            {
                                int nx = cx + dx[dir];
                                int ny = cy + dy[dir];
                                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                                {
                                    if (data[ny * Width + nx] == compare_value && visited[ny, nx] == 0)
                                    {
                                        visited[ny, nx] = currentLabel;
                                        q.Enqueue((nx, ny));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<int> erase_label = new List<int>();
            foreach (var d in dic_area)
            {
                if (d.Value <= area)
                {
                    erase_label.Add(d.Key);
                }
            }




            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    foreach (var label in erase_label)
                    {
                        if (label_data[j * Width + i] == label)
                        {
                            new_data[j * Width + i] = (byte)erase_value;
                        }
                    }
                }
            }


            labelCount = currentLabel;

            return new ByteImage(Width, Height, Width, new_data, 0);
        }


        // Minimum Threshold Algorithm
        public ByteImage MinimumBinarization(int smoothCount)
        {
            // 1. 히스토그램 계산
            int[] hist = new int[256];
            for (int i = 0; i < data.Length; i++)
                hist[data[i]]++;

            // 2. 스무딩 반복
            for (int i = 0; i < smoothCount; i++)
                hist = SmoothHistogram(hist);

            // 3. 두 피크 사이 valley 찾기
            int threshold = FindMinimumValley(hist);

            // 4. 이진화
            byte[] binary = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                binary[i] = (byte)(data[i] > threshold ? 255 : 0);

            ByteImage copy = new ByteImage(this.Width, this.Height, this.Width, binary, 0);

            return copy;
        }
        private int[] SmoothHistogram(int[] hist)
        {
            int[] smooth = new int[256];
            smooth[0] = (hist[0] + hist[1]) / 2;
            for (int i = 1; i < 255; i++)
                smooth[i] = (hist[i - 1] + hist[i] + hist[i + 1]) / 3;
            smooth[255] = (hist[254] + hist[255]) / 2;
            return smooth;
        }
        private int FindMinimumValley(int[] hist)
        {
            // 두 개의 피크(봉우리) 찾기
            int peak1 = 0, peak2 = 0;
            int max1 = 0, max2 = 0;

            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i] > max1)
                {
                    max2 = max1; peak2 = peak1;
                    max1 = hist[i]; peak1 = i;
                }
                else if (hist[i] > max2)
                {
                    max2 = hist[i]; peak2 = i;
                }
            }

            // 두 피크 사이의 최소점 찾기
            int start = Math.Min(peak1, peak2);
            int end = Math.Max(peak1, peak2);
            int minVal = int.MaxValue;
            int minIdx = (start + end) / 2;

            for (int i = start; i <= end; i++)
            {
                if (hist[i] < minVal)
                {
                    minVal = hist[i];
                    minIdx = i;
                }
            }

            return minIdx;
        }

        public ByteImage MinimumBinarization2(int radius, int iterations)
        {
            // 1. 히스토그램 계산
            int[] hist = new int[256];
            for (int i = 0; i < data.Length; i++)
                hist[data[i]]++;

            // 2. 히스토그램 스무딩
            double[] smoothHist = SmoothHistogram(hist, radius, iterations);

            // 3. 두 개의 피크 찾기
            FindTwoPeaks(smoothHist, out int peak1, out int peak2);
            if (peak1 > peak2) (peak1, peak2) = (peak2, peak1);

            // 4. 두 피크 사이의 최솟값 위치를 임계값으로 설정
            int threshold = peak1;
            double minValue = smoothHist[peak1];
            for (int i = peak1; i <= peak2; i++)
            {
                if (smoothHist[i] < minValue)
                {
                    minValue = smoothHist[i];
                    threshold = i;
                }
            }

            // 5. 이진화 수행
            byte[] binary = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                binary[i] = (byte)(data[i] > threshold ? 255 : 0);

            return new ByteImage(Width, Height, Width, binary, 0);

        }
        private double[] SmoothHistogram(int[] hist, int radius, int iterations)
        {
            double[] smoothed = Array.ConvertAll(hist, v => (double)v);
            double[] temp = new double[256];

            for (int it = 0; it < iterations; it++)
            {
                for (int i = 0; i < 256; i++)
                {
                    double sum = 0;
                    int count = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int idx = i + k;
                        if (idx >= 0 && idx < 256)
                        {
                            sum += smoothed[idx];
                            count++;
                        }
                    }
                    temp[i] = sum / count;
                }
                Array.Copy(temp, smoothed, 256);
            }
            return smoothed;
        }

        private void FindTwoPeaks(double[] hist, out int peak1, out int peak2)
        {
            peak1 = 0; peak2 = 0;
            double max1 = double.MinValue, max2 = double.MinValue;

            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i] > max1)
                {
                    max2 = max1;
                    peak2 = peak1;
                    max1 = hist[i];
                    peak1 = i;
                }
                else if (hist[i] > max2)
                {
                    max2 = hist[i];
                    peak2 = i;
                }
            }
        }
        // Gaussian Adaptive Thresholding
        public ByteImage GaussionAdaptiveBinarization(int blockSize = 15, double C = 5.0)
        {
            if (blockSize % 2 == 0)
            {
                //throw new ArgumentException("blockSize must be odd.");
                return null;
            }
            int radius = blockSize / 2;

            byte[] dst = new byte[data.Length];

            // --- 1️⃣ Gaussian Kernel 생성 (한 번만 계산)
            double[,] kernel = CreateGaussianKernel(blockSize, blockSize, blockSize / 6.0);

            // --- 2️⃣ 병렬 처리 (멀티코어)
            Parallel.For(0, this.Height, y =>
            {
                int yOffset = y * this.Width;
                for (int x = 0; x < this.Width; x++)
                {
                    double weightedSum = 0;
                    double weightTotal = 0;

                    // --- 3️⃣ 블록 윈도우 계산 ---
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int yy = y + ky;
                        if (yy < 0 || yy >= this.Height) continue;

                        int kyy = ky + radius;
                        int yyOffset = yy * this.Width;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int xx = x + kx;
                            if (xx < 0 || xx >= this.Width) continue;

                            double weight = kernel[kyy, kx + radius];
                            weightedSum += data[yyOffset + xx] * weight;
                            weightTotal += weight;
                        }
                    }

                    // --- 4️⃣ 임계값 계산 및 이진화 ---
                    double threshold = weightedSum / weightTotal - C;
                    dst[yOffset + x] = (byte)(data[yOffset + x] > threshold ? 255 : 0);
                }
            });

            ByteImage copy = new ByteImage(this.Width, this.Height, this.Width, dst, 0);

            return copy;
        }

        private double[,] CreateGaussianKernel(int width, int height, double sigma)
        {
            double[,] kernel = new double[height, width];
            double sum = 0.0;
            int cx = width / 2;
            int cy = height / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - cx;
                    double dy = y - cy;
                    double value = Math.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                    kernel[y, x] = value;
                    sum += value;
                }
            }

            // 정규화
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    kernel[y, x] /= sum;

            return kernel;
        }

        // Otsu
        public ByteImage OtsuBinarization(out int otsu_level, int offset = 0)
        {
            int[] hist = new int[256];
            int total = Width * Height;

            // 1. 히스토그램 계산
            for (int i = 0; i < total; i++)
            {
                hist[data[i]]++;
            }

            // 2. 전체 밝기 합계
            double sumAll = 0;
            for (int i = 0; i < 256; i++)
                sumAll += i * hist[i];

            double sumB = 0;
            int weightB = 0;
            int weightF = 0;

            double maxVariance = 0;
            int threshold = 0;

            // 3. Otsu 임계값 탐색
            for (int t = 0; t < 256; t++)
            {
                weightB += hist[t];
                if (weightB == 0)
                    continue;

                weightF = total - weightB;
                if (weightF == 0)
                    break;

                sumB += (double)(t * hist[t]);

                double meanB = sumB / weightB;
                double meanF = (sumAll - sumB) / weightF;

                double varBetween = (double)weightB * (double)weightF * Math.Pow(meanB - meanF, 2);

                if (varBetween > maxVariance)
                {
                    maxVariance = varBetween;
                    threshold = t;
                }
            }

            otsu_level = threshold;

            //// 4. 임계값으로 이진화
            //byte[] dst = new byte[total];
            //for (int i = 0; i < total; i++)
            //{
            //    dst[i] = (data[i] > threshold + offset) ? (byte)255 : (byte)0;
            //}

            return Binarization(1, threshold + offset, BinTypes.InSide);

        }
        #endregion

        #region methods - util

        public BitmapImage ToBitmapImage()
        {

            if (data == null || data.Length != Width * Height)
                return null;

            // 1. byte[] → BitmapSource (Gray8)
            BitmapSource bitmapSource = BitmapSource.Create(
                Width,
                Height,
                96, 96,                            // DPI
                PixelFormats.Gray8,
                null,
                data,
                Width); // stride = width * bytesPerPixel = width * 1

            // 2. BitmapSource → PNG로 인코딩 후 MemoryStream에 저장
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                // 3. MemoryStream → BitmapImage 생성
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // UI 스레드에서 안전하게 사용 가능

                return bitmapImage;
            }
        }

        public void ToJpg(Stream str)
        {
            using (MemoryStream imageStreamSource = new MemoryStream())
            {
                var pixels = this.Data;
                BitmapPalette myPalette = BitmapPalettes.Gray256;
                BitmapSource image = BitmapSource.Create(dimension.Width, dimension.Height, 96, 96, System.Windows.Media.PixelFormats.Gray8, myPalette, pixels, dimension.Pitch);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(str);
            }
        }
        public override void Clear()
        {
            _Offset = 0;
        }
        public override string ToString()
        {
            return IsEmpty ?
                       "{ByteImage: Empty}" :
                       string.Format("{{ByteImage: Size={0}; }}",
                                     dimension == null ? "0" : dimension.ToString());
        }
        public virtual ByteImage Copy()
        {
            ByteImage copyData;
            if (dimension == null)
                copyData = new ByteImage();
            else
            {
                copyData = new ByteImage(dimension);
                base.Copy(copyData, _Offset);

                copyData.Offset = _Offset;
            }

            return copyData;
        }
        public virtual ByteImage Duplicate()
        {
            return (ByteImage)this.Copy();
        }

        public override ByteImageBase Crop(RoiRectangle roi)
        {
            if (roi == null)
                return null;

            if (roi.Left == roi.Right || roi.Top == roi.Bottom)
                return null;

            return GetChildBuffer(roi);
        }
        public ByteImage GetChildBuffer(RoiRectangle ROI)
        {
            int offsetToChildPtr = ROI.TopLeft.X + (ROI.TopLeft.Y * dimension.Width);

            return GetChildBuffer(offsetToChildPtr, ROI.Size);
        }
        public ByteImage GetChildBuffer(int offsetToChildPtr, Dimension childSize)
        {

            if (offsetToChildPtr > dimension.Width * dimension.Height || offsetToChildPtr < 0)
            {
                return null;
            }

            int srcX = offsetToChildPtr % dimension.Width;
            int srcY = offsetToChildPtr / dimension.Width;

            if (srcX + childSize.Width > Dimension.Width || srcY + childSize.Height > Dimension.Height)
            {
                return null;
            }

            ByteImage childBuffer = new ByteImage(childSize.Width, childSize.Height);

            for (int i = 0; i < childBuffer.Dimension.Height; i++)
            {
                Buffer.BlockCopy(Data, _Offset + offsetToChildPtr + (i * Dimension.Width), childBuffer.Data, i * childBuffer.Dimension.Width, childBuffer.Dimension.Width);
            }

            return childBuffer;
        }
        public bool InsertChildBuffer(RoiRectangle rect, byte[] childBuffer)
        {
            int offsetToChildPtr = rect.TopLeft.X + (rect.TopLeft.Y * Dimension.Width);
            if (offsetToChildPtr > dimension.Width * dimension.Height || offsetToChildPtr < 0)
            {
                return false;
            }

            int srcX = offsetToChildPtr % dimension.Width;
            int srcY = offsetToChildPtr / dimension.Width;

            if (srcX + rect.Size.Width > Dimension.Width || srcY + rect.Size.Height > Dimension.Height)
            {
                return false;
            }

            for (int i = 0; i < rect.Size.Height; i++)
            {
                Buffer.BlockCopy(childBuffer, i * rect.Size.Width, Data, _Offset + offsetToChildPtr + i * dimension.Width, rect.Size.Width);
            }

            return true;
        }
        public bool InsertChildBuffer(int offsetToChildPtr, ByteImage childBuffer)
        {

            if (offsetToChildPtr > dimension.Width * dimension.Height || offsetToChildPtr < 0)
            {
                return false;
            }

            int srcX = offsetToChildPtr % dimension.Width;
            int srcY = offsetToChildPtr / dimension.Width;

            if (srcX + childBuffer.Dimension.Width > Dimension.Width || srcY + childBuffer.Dimension.Height > Dimension.Height)
            {
                return false;
            }

            for (int i = 0; i < childBuffer.Dimension.Height; i++)
            {
                Buffer.BlockCopy(childBuffer.Data, i * childBuffer.Dimension.Width, Data, _Offset + offsetToChildPtr + i * dimension.Width, childBuffer.Dimension.Width);
            }

            return true;
        }
        public bool InsertChildBuffer(int offsetToChildPtr, ByteImage childBuffer, RoiRectangle Roi)
        {

            if (offsetToChildPtr > dimension.Width * dimension.Height || offsetToChildPtr < 0)
            {
                throw new Exception(String.Format("Offset to child buffer is invalid {0}", offsetToChildPtr));
            }

            int srcX = offsetToChildPtr % dimension.Width;
            int srcY = offsetToChildPtr / dimension.Width;

            if (srcX + Roi.Size.Width > Dimension.Width || srcY + Roi.Size.Height > Dimension.Height)
            {
                throw new Exception(String.Format("Incorrect ROI or Offset to child buffer. {0} {1}", Roi.ToString(), offsetToChildPtr));
            }

            for (int i = 0; i < Roi.Size.Height; i++)
            {
                if (((i + Roi.Top) * childBuffer.Dimension.Width) + Roi.Left + Roi.Size.Width > childBuffer.Length)
                {
                    throw new Exception(String.Format("Failed copy - Row {0} exceeds src buffer ", i));
                }

                if (_Offset + offsetToChildPtr + i * dimension.Width + Roi.Size.Width > Length)
                {
                    throw new Exception(String.Format("Failed copy - Row {0} exceeds dst buffer ", i));
                }

                Buffer.BlockCopy(childBuffer.Data, ((i + Roi.Top) * childBuffer.Dimension.Width) + Roi.Left, Data, _Offset + offsetToChildPtr + i * dimension.Width, Roi.Size.Width);
            }

            return true;
        }

        public bool Load(string filepath)
        {
            try
            {
                FileInfo fi = new FileInfo(filepath);
                using (Stream imageStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapDecoder decoder = null;

                    switch (fi.Extension.ToLowerInvariant())
                    {
                        case ".bmp":
                            decoder = new BmpBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".png":
                            decoder = new PngBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".jpg":
                        case ".jpeg":
                            decoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        // 추가 가능
                        default:
                            throw new NotSupportedException("Unsupported image format.");
                    }

                    if (decoder != null && decoder.Frames.Count > 0)
                    {
                        bmpSource = decoder.Frames[0];

                        if (bmpSource.Format != PixelFormats.Gray8)
                        {
                            bmpSource = new FormatConvertedBitmap(bmpSource, PixelFormats.Gray8, null, 0);
                        }

                        dimension = new ImageDimension(bmpSource.PixelWidth, bmpSource.PixelHeight, bmpSource.PixelWidth);

                        int stride = bmpSource.PixelWidth; // Gray8 기준
                        data = new byte[bmpSource.PixelHeight * stride];
                        bmpSource.CopyPixels(data, stride, 0);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in ByteImage.Load. See innerException for more details", ex);
            }

            return false;
        }
        public void Save(string filepath)
        {
            BitmapEncoder encoder = new BmpBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bmpSource));
            encoder.Frames.Add(BitmapFrame.Create(BitmapSource));

            using (var fileStream = new FileStream(filepath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        public async void SaveAsync(string filePath)
        {
            await Task.Run(() =>
            {
                Save(filePath);
            });
        }

        public ByteImage Resize(int new_width, int new_height, ResizeTypes type = ResizeTypes.Gray)
        {
            double wRatio = (double)new_width / (double)Width;
            double hRatio = (double)new_height / (double)Height;

            return Resize(wRatio, hRatio, type);
        }

        public ByteImage Resize(double wRatio, double hRatio, ResizeTypes type = ResizeTypes.Gray)
        {
            PixelFormat Format = PixelFormats.Gray8;

            int newWidth = (int)Math.Round(Width * wRatio, 1);
            int newHeight = (int)Math.Round(Height * hRatio, 1);
            int newStride = (newWidth * Format.BitsPerPixel + 7) / 8;

            int this_stride = (Width * Format.BitsPerPixel + 7) / 8;
            var this_bitmap = BitmapSource.Create(Width, Height, 96, 96, Format, null, Data, this_stride);
            var scaled = new TransformedBitmap(this_bitmap, new ScaleTransform(wRatio, hRatio));

            byte[] resizedData = new byte[newHeight * newStride];
            scaled.CopyPixels(resizedData, newStride, 0);

            if (type == ResizeTypes.Bin)
            {
                for (int i = 0; i < resizedData.Length; i++)
                {
                    if (resizedData[i] != 0)
                        resizedData[i] = 255;
                }
            }


            var resize_image = new ByteImage(newWidth, newHeight, newWidth, resizedData, 0);

            return resize_image;
        }

        public ByteImage Resize(double Ratio, ResizeTypes type = ResizeTypes.Gray)
        {
            return Resize(Ratio, Ratio, type);
        }

        public ByteImage Rotate(double rx, double ry, double angle/* degree */, RotateTypes type = RotateTypes.Expand)
        {
            ByteImage image = null;
            if (type == RotateTypes.Expand)
            {

                if (angle == 90.0)
                {

                }
                else if (angle == 270.0)
                {

                }
                else
                {
                    image = Rotate_Expand(rx, ry, angle);
                }

            }
            else
            {
                image = Rotate_Crop(rx, ry, angle);
            }

            return image;
        }

        private byte BilinearSample(byte[] data, int width, int height, double x, double y, byte defaultValue = 0)
        {
            //int x0 = Math.Max(0, (int)Math.Floor(x));
            int x0 = (int)Math.Floor(x);
            int x1 = x0 + 1;
            int y0 = (int)Math.Floor(y);
            int y1 = y0 + 1;

            if (x0 < 0 || x1 >= width || y0 < 0 || y1 >= height)
                return defaultValue;

            double dx = x - x0;
            double dy = y - y0;

            byte p00 = data[y0 * width + x0];
            byte p10 = data[y0 * width + x1];
            byte p01 = data[y1 * width + x0];
            byte p11 = data[y1 * width + x1];

            double top = p00 * (1 - dx) + p10 * dx;
            double bottom = p01 * (1 - dx) + p11 * dx;
            double value = top * (1 - dy) + bottom * dy;

            return ClampToByte((int)(value + 0.5));
        }

        public ByteImage Rotate(double angle, RotateTypes type = RotateTypes.Expand)
        {
            ByteImage rotate_image = null;
            if (angle == 90.0 || angle == -270)
            {
                rotate_image = Rotate_90();
            }
            else if (angle == 180.0 || angle == -180)
            {
                rotate_image = Rotate_180();
            }
            else if (angle == 270.0 || angle == -90)
            {
                rotate_image = Rotate_270();
            }
            else
            {
                var cx = Width / 2.0;
                var cy = Height / 2.0;

                rotate_image = Rotate(cx, cy, angle, type);
            }

            return rotate_image;
        }

        private DPointCoordinates RotatePoint(double x, double y, double angleRad)
        {
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            return new DPointCoordinates(
                x * cos - y * sin,
                x * sin + y * cos
            );
        }

        public ByteImage Rotate_Expand(double rx, double ry, double angle/* degree */, byte defaultValue = 0)
        {
            double angleRad = angle * Math.PI / 180.0;

            // 원본 좌표계 기준 꼭짓점 회전
            DPointCoordinates[] corners = new DPointCoordinates[]
            {
                RotatePoint(-rx, -ry, angleRad),
                RotatePoint(Width - rx, -ry, angleRad),
                RotatePoint(-rx, Height - ry, angleRad),
                RotatePoint(Width - rx, Height - ry, angleRad)
            };

            double minX = corners.Min(p => p.X);
            double maxX = corners.Max(p => p.X);
            double minY = corners.Min(p => p.Y);
            double maxY = corners.Max(p => p.Y);

            //int newWidth = (int)Math.Ceiling(maxX - minX);
            //int newHeight = (int)Math.Ceiling(maxY - minY);


            int newWidth = (int)(maxX - minX);
            int newHeight = (int)(maxY - minY);


            byte[] output = new byte[newWidth * newHeight];

            // 🔹 새 중심 좌표계 보정
            double newCenterX = -minX;
            double newCenterY = -minY;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // 새 이미지 기준 좌표
                    double dx = x - newCenterX;
                    double dy = y - newCenterY;

                    // 역회전 변환
                    double srcX = dx * Math.Cos(-angleRad) - dy * Math.Sin(-angleRad) + rx;
                    double srcY = dx * Math.Sin(-angleRad) + dy * Math.Cos(-angleRad) + ry;

                    byte pixel = BilinearSample(Data, Width, Height, srcX, srcY, defaultValue);
                    output[y * newWidth + x] = pixel;
                }
            }

            return new ByteImage(newWidth, newHeight, newWidth, output, 0);
        }

        public ByteImage Rotate_90()
        {
            //angle = ((angle % 360) + 360) % 360; // 음수 각도 방지
            var newWidth = Height;
            var newHeight = Width;

            var dst = new byte[data.Length];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int srcIndex = y * Width + x;
                    int dstIndex = x * newWidth + (newWidth - y - 1);
                    dst[dstIndex] = data[srcIndex];
                }
            }
            return new ByteImage(newWidth, newHeight, newWidth, dst, 0);
        }

        public ByteImage Rotate_180()
        {
            var newWidth = Width;
            var newHeight = Height;
            var dst = new byte[newWidth * newHeight];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int srcIndex = y * Width + x;
                    int dstIndex = (newHeight - y - 1) * newWidth + (newWidth - x - 1);
                    dst[dstIndex] = data[srcIndex];
                }
            }


            return new ByteImage(newWidth, newHeight, newWidth, dst, 0);
        }

        public ByteImage Rotate_270()
        {
            var newWidth = Height;
            var newHeight = Width;
            var dst = new byte[newWidth * newHeight];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int srcIndex = y * Width + x;
                    int dstIndex = (newHeight - x - 1) * newWidth + y;
                    dst[dstIndex] = data[srcIndex];
                }
            }

            return new ByteImage(newWidth, newHeight, newWidth, dst, 0);
        }
        //public ByteImage Rotate_Expand(double rx, double ry, double angle/* degree */, byte defaultValue = 0)
        //{
        //    double angleRad = angle * Math.PI / 180.0;

        //    // 각 꼭짓점 회전
        //    DPointCoordinates[] corners = new DPointCoordinates[]
        //    {
        //        RotatePoint(-rx, -ry, angleRad),  // (0,0)
        //        RotatePoint(Width - rx, -ry, angleRad), // (W,0)
        //        RotatePoint(-rx, Height - ry, angleRad), // (0,H)
        //        RotatePoint(Width - rx, Height - ry, angleRad)  // (W,H)
        //    };

        //    double minX = corners.Min(p => p.X);
        //    double maxX = corners.Max(p => p.X);
        //    double minY = corners.Min(p => p.Y);
        //    double maxY = corners.Max(p => p.Y);

        //    var newWidth = (int)Math.Ceiling(maxX - minX);
        //    var newHeight = (int)Math.Ceiling(maxY - minY);
        //    //var newWidth = (int)(maxX - minX);
        //    //var newHeight = (int)(maxY - minY);


        //    byte[] output = new byte[newWidth * newHeight];

        //    // 🔹 보정된 회전 중심 (원본 중심이 새 이미지 중심으로 이동되도록)
        //    double newCenterX = -minX;
        //    double newCenterY = -minY;

        //    for (int y = 0; y < newHeight; y++)
        //    {
        //        for (int x = 0; x < newWidth; x++)
        //        {
        //            double dx = x - newCenterX;
        //            double dy = y - newCenterY;

        //            double srcX = dx * Math.Cos(-angleRad) - dy * Math.Sin(-angleRad) + rx;
        //            double srcY = dx * Math.Sin(-angleRad) + dy * Math.Cos(-angleRad) + ry;

        //            byte pixel = BilinearSample(Data, Width, Height, srcX, srcY, defaultValue);
        //            output[y * newWidth + x] = pixel;
        //        }
        //    }

        //    //double offsetX = newWidth / 2.0;
        //    //double offsetY = newHeight / 2.0;

        //    //for (int y = 0; y < newHeight; y++)
        //    //{
        //    //    for (int x = 0; x < newWidth; x++)
        //    //    {
        //    //        double dx = x - offsetX;
        //    //        double dy = y - offsetY;

        //    //        double srcX = dx * Math.Cos(-angleRad) - dy * Math.Sin(-angleRad) + rx;
        //    //        double srcY = dx * Math.Sin(-angleRad) + dy * Math.Cos(-angleRad) + ry;

        //    //        byte pixel = BilinearSample(Data, Width, Height, srcX, srcY, defaultValue);
        //    //        output[y * newWidth + x] = pixel;
        //    //    }
        //    //}

        //    return new ByteImage(newWidth, newHeight, newWidth, output, 0);
        //}

        public ByteImage Rotate_Crop(double rx, double ry, double angle/* degree */, byte defaultValue = 0)
        {
            double angleRad = angle * Math.PI / 180.0;
            byte[] newData = new byte[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double dx = x - rx;
                    double dy = y - ry;

                    double srcX = dx * Math.Cos(-angleRad) - dy * Math.Sin(-angleRad) + rx;
                    double srcY = dx * Math.Sin(-angleRad) + dy * Math.Cos(-angleRad) + ry;

                    byte pixel = BilinearSample(Data, Width, Height, srcX, srcY, defaultValue);
                    newData[y * Width + x] = pixel;
                }
            }

            return new ByteImage(Width, Height, Width, newData, 0);
            //return output;
        }


        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private byte ClampToByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }
        #endregion        

        #region Skeleton(ZhangSuen)

        public ByteImage Skeleton()
        {
            int width = Width;
            int height = Height;

            byte[] copy_data = new byte[width * height];
            Array.Copy(data, copy_data, width * height);

            bool changed;
            do
            {
                changed = false;
                List<int> toRemove = new List<int>();

                // 1단계
                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        if (Get(copy_data, x, y, width) != 255) continue;

                        int[] p = GetNeighbors(copy_data, x, y, width);
                        int A = Count01Transitions(p);
                        int B = SumNeighbors(p);

                        if (B >= 2 && B <= 6 && A == 1 &&
                            p[0] * p[2] * p[4] == 0 &&
                            p[2] * p[4] * p[6] == 0)
                        {
                            toRemove.Add(y * width + x);
                        }
                    }
                }

                if (toRemove.Count > 0)
                {
                    changed = true;
                    foreach (var index in toRemove)
                        copy_data[index] = 0;
                }

                toRemove.Clear();

                // 2단계
                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        if (Get(copy_data, x, y, width) != 255) continue;

                        int[] p = GetNeighbors(copy_data, x, y, width);
                        int A = Count01Transitions(p);
                        int B = SumNeighbors(p);

                        if (B >= 2 && B <= 6 && A == 1 &&
                            p[0] * p[2] * p[6] == 0 &&
                            p[0] * p[4] * p[6] == 0)
                        {
                            toRemove.Add(y * width + x);
                        }
                    }
                }

                if (toRemove.Count > 0)
                {
                    changed = true;
                    foreach (var index in toRemove)
                        copy_data[index] = 0;
                }

            } while (changed);

            return new ByteImage(width, height, width, copy_data, 0);
        }

        private byte Get(byte[] img, int x, int y, int width)
        {
            return img[y * width + x];
        }

        private int[] GetNeighbors(byte[] img, int x, int y, int width)
        {
            //var p2 = data[y * width + (x - 1)];
            //var p3 = data[(y + 1) * width + (x - 1)];
            //var p4 = data[(y + 1) * width + x];
            //var p5 = data[(y + 1) * width + (x + 1)];
            //var p6 = data[y * width + (x + 1)];
            //var p7 = data[(y - 1) * width + (x + 1)];
            //var p8 = data[(y - 1) * width + x];
            //var p9 = data[(y - 1) * width + (x - 1)];


            return new int[]
            {
                Get(img, x, y-1, width),
                Get(img, x+1, y-1, width),
                Get(img, x+1, y, width),
                Get(img, x+1, y+1, width),
                Get(img, x, y+1, width),
                Get(img, x-1, y+1, width),
                Get(img, x-1, y, width),
                Get(img, x-1, y+1, width),
            //img[x-1, y],   // p2
            //img[x-1, y+1], // p3
            //img[x,   y+1], // p4
            //img[x+1, y+1], // p5
            //img[x+1, y],   // p6
            //img[x+1, y-1], // p7
            //img[x,   y-1], // p8
            //img[x-1, y-1]  // p9

            //p2,
            //p3,
            //p4,
            //p5,
            //p6,
            //p7,
            //p8,
            //p9

                //data[y * Width + (x-1)], // p2
                //data[(y+1) * Width + (x-1)], // p3
                //data[(y+1) * Width + x], // p4
                //data[(y+1) * Width + (x+1)], // p5
                //data[y * Width + (x+1)], // p6
                //data[(y-1) * Width + (x+1)], // p7
                //data[(y-1) * Width + x], // p8
                //data[(y-1) * Width + (x-1)]  // p9
            };
        }

        private int SumNeighbors(int[] p)
        {
            int sum = 0;
            for (int i = 0; i < 8; i++) sum += p[i] / 255;
            return sum;
        }

        private int Count01Transitions(int[] p)
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
                if (p[i] == 0 && p[i + 1] == 255) count++;
            if (p[7] == 0 && p[0] == 255) count++;
            return count;
        }

        #endregion

        #region Canny
        public ByteImage Canny(double? lowThresh = null, double? highThresh = null)
        {
            var src = data;
            int width = Width;
            int height = Height;
            // 1. Gaussian Blur
            double[,] gray = ToDouble(src, width, height);
            double[,] blurred = GaussianBlur(gray, width, height);

            // 2. Sobel Gradient
            double[,] mag, dir;
            ComputeGradient(blurred, width, height, out mag, out dir);

            // 🔹 자동 임계값 설정
            if (lowThresh == null || highThresh == null)
            {
                AutoThreshold(mag, width, height, out double low, out double high, 1);
                lowThresh = low;
                highThresh = high;
            }

            // 3. Non-Maximum Suppression
            double[,] nms = NonMaximumSuppression(mag, dir, width, height);

            // 4. Double Threshold
            int[,] dt = DoubleThreshold(nms, width, height, lowThresh.Value, highThresh.Value);

            // 5. Edge Tracking by Hysteresis
            int[,] result = Hysteresis(dt, width, height);

            // 6. Convert back to byte[]
            var dst = ToByte(result, width, height);

            return new ByteImage(width, height, width, dst, 0);
        }

        public void AutoThreshold(double[,] gradient, int width, int height,
                                 out double low, out double high, double k = 0.33)
        {
            // 1D 배열로 변환
            double[] vals = new double[width * height];
            int idx = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    vals[idx++] = gradient[y, x];

            double mean = vals.Average();
            double std = Math.Sqrt(vals.Select(v => (v - mean) * (v - mean)).Average());

            high = mean + k * std;
            low = high * 0.5;
        }

        public double[,] ToDouble(byte[] src, int w, int h)
        {
            double[,] dst = new double[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    dst[y, x] = src[y * w + x];
            }
            return dst;
        }

        public byte[] ToByte(int[,] src, int w, int h)
        {
            byte[] dst = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    dst[y * w + x] = (byte)(src[y, x] > 0 ? 255 : 0);
            }
            return dst;
        }

        // ① Gaussian Blur (5x5)
        public double[,] GaussianBlur(double[,] src, int w, int h)
        {
            double[,] dst = new double[h, w];
            double[,] kernel = {
                {2, 4, 5, 4, 2},
                {4, 9,12, 9, 4},
                {5,12,15,12, 5},
                {4, 9,12, 9, 4},
                {2, 4, 5, 4, 2}
            };
            double sum = 159.0; // kernel 합

            for (int y = 2; y < h - 2; y++)
            {
                for (int x = 2; x < w - 2; x++)
                {
                    double val = 0;
                    for (int ky = -2; ky <= 2; ky++)
                        for (int kx = -2; kx <= 2; kx++)
                            val += src[y + ky, x + kx] * kernel[ky + 2, kx + 2];
                    dst[y, x] = val / sum;
                }
            }
            return dst;
        }

        // ② Gradient 계산 (Sobel)
        public void ComputeGradient(double[,] src, int w, int h, out double[,] mag, out double[,] dir)
        {
            double[,] gxKernel = {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            };

            double[,] gyKernel = {
                { 1, 2, 1},
                { 0, 0, 0},
                {-1,-2,-1}
            };

            mag = new double[h, w];
            dir = new double[h, w];

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    double gx = 0, gy = 0;
                    for (int ky = -1; ky <= 1; ky++)
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            gx += src[y + ky, x + kx] * gxKernel[ky + 1, kx + 1];
                            gy += src[y + ky, x + kx] * gyKernel[ky + 1, kx + 1];
                        }
                    mag[y, x] = Math.Sqrt(gx * gx + gy * gy);
                    dir[y, x] = Math.Atan2(gy, gx);
                }
            }
        }

        // ③ Non-Maximum Suppression
        public double[,] NonMaximumSuppression(double[,] mag, double[,] dir, int w, int h)
        {
            double[,] dst = new double[h, w];

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    double angle = dir[y, x] * (180.0 / Math.PI);
                    if (angle < 0) angle += 180;

                    double q = 255, r = 255;

                    // 방향별 이웃 픽셀 비교
                    if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180))
                    {
                        q = mag[y, x + 1];
                        r = mag[y, x - 1];
                    }
                    else if (angle >= 22.5 && angle < 67.5)
                    {
                        q = mag[y + 1, x - 1];
                        r = mag[y - 1, x + 1];
                    }
                    else if (angle >= 67.5 && angle < 112.5)
                    {
                        q = mag[y + 1, x];
                        r = mag[y - 1, x];
                    }
                    else if (angle >= 112.5 && angle < 157.5)
                    {
                        q = mag[y - 1, x - 1];
                        r = mag[y + 1, x + 1];
                    }

                    dst[y, x] = (mag[y, x] >= q && mag[y, x] >= r) ? mag[y, x] : 0;
                }
            }

            return dst;
        }

        // ④ Double Threshold
        public int[,] DoubleThreshold(double[,] src, int w, int h, double low, double high)
        {
            int[,] dst = new int[h, w];
            const int STRONG = 2;
            const int WEAK = 1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double val = src[y, x];
                    if (val >= high) dst[y, x] = STRONG;
                    else if (val >= low) dst[y, x] = WEAK;
                    else dst[y, x] = 0;
                }
            }
            return dst;
        }

        // ⑤ Edge Tracking by Hysteresis
        public int[,] Hysteresis(int[,] src, int w, int h)
        {
            const int STRONG = 2;
            const int WEAK = 1;

            int[,] dst = (int[,])src.Clone();

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (dst[y, x] == WEAK)
                    {
                        bool connected = false;
                        for (int ky = -1; ky <= 1; ky++)
                            for (int kx = -1; kx <= 1; kx++)
                                if (dst[y + ky, x + kx] == STRONG)
                                    connected = true;

                        dst[y, x] = connected ? STRONG : 0;
                    }
                }
            }

            return dst;
        }
        #endregion
    }
}
