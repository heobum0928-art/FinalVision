using Project.BaseLib.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [Serializable]
    public abstract class ImageBase
    {
        #region fields

        protected ImageDimension dimension;

        #endregion

        #region propertise
        [DataMember]
        [XmlElement]
        public ImageDimension Dimension
        {
            get { return dimension; }

            set { dimension = value; }
        }

        public virtual int Length
        {
            get { return dimension.Width * dimension.Height; }
        }
        #endregion

        #region constructors

        public ImageBase()
            : this(0, 0, 0)
        {
        }

        public ImageBase(int width, int height)
            : this(width, height, width)
        {
        }

        public ImageBase(int width, int height, int pitch)
        {
            this.dimension = new ImageDimension(width, height, pitch);
        }

        public ImageBase(ImageDimension dimension)
        {
            this.dimension = dimension.Duplicate();
            if (this.dimension.Pitch == 0)
                this.dimension.Pitch = this.dimension.Width;
        }

        #endregion

        #region methods
        public virtual void Clear()
        {
            //base.Clear();
            if (dimension != null)
                dimension.Clear();
        }
        public override bool Equals(object obj)
        {
            return Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public abstract long[] CalculateGrayscaleHistogram(out double average, out int maxGL, out int histogramPeak);

        #endregion
    }

    [DataContract]
    [Serializable]
    public abstract class ByteImageBase : ImageBase
    {
        #region fields
        protected byte[] data;
        #endregion

        #region propertise
        public byte[] Data
        {
            get { return data; }

            set
            {
                data = value;
            }
        }

        public override int Length
        {
            get { return data.Length; }
        }

        public bool IsEmpty
        {
            get
            {
                return (data == null || data.Length == 0);
            }
        }

        #endregion

        #region constructors
        public ByteImageBase()
        {
        }

        public ByteImageBase(int length)
        {
            data = new byte[length];
            Array.Clear(data, 0, data.Length);
        }
        public ByteImageBase(byte[] data)
        {
            this.data = data;
        }
        protected ByteImageBase(int width, int height) : this(width, height, width, new byte[width * height])
        {
        }
        protected ByteImageBase(ImageDimension dimension)
            : this(dimension, new byte[dimension.Width * dimension.Height])
        {
            this.dimension = dimension.Duplicate();
            if (this.dimension.Pitch == 0)
                this.dimension.Pitch = this.dimension.Width;
        }
        protected ByteImageBase(int width, int height, int pitch, byte[] data)
            : this(data)
        {
            this.dimension = new ImageDimension(width, height, width);
        }
        protected ByteImageBase(ImageDimension dimension, byte[] data)
            : base(dimension)
        {
            this.Data = data;
        }
        #endregion

        #region methods
        public static bool IsNullOrEmpty(ByteImageBase blob)
        {
            return blob == null || blob.IsEmpty;
        }

        protected virtual void Copy(ByteImageBase copyData, int offset = 0)
        {
            if (data != null)
            {
                Array.Copy(data, offset, copyData.Data, 0, copyData.Data.Length);
            }
        }

        public override void Clear()
        {
            if (data != null)
                Array.Clear(data, 0, data.Length);
        }
        public Stream ToStream()
        {
            Stream stream = new MemoryStream();
            ToStream(stream);
            return stream;
        }

        public void ToStream(Stream stream)
        {
            if (Data != null)
                stream.Write(Data, 0, Data.Length);
        }

        public override long[] CalculateGrayscaleHistogram(out double average, out int maxGL, out int histogramPeak)
        {
            average = 0;
            maxGL = 0;
            histogramPeak = 0;
            long[] histogram = new long[256];
            for (int i = 0; i < Length; i++)
            {
                var value = Data[i];
                histogram[value]++;

                average += value;
            }
            average /= Length;

            long max = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > 0)
                    maxGL = i;

                if (histogram[i] > max)
                {
                    max = histogram[i];
                    histogramPeak = i;
                }
            }

            return histogram;
        }

        public abstract ByteImageBase Crop(RoiRectangle roi);

        public virtual ByteImageBase CropImageAroundPoint(PixelCoordinates centerLocation, ImageDimension dimensions)
        {
            var roi = new RoiRectangle(new PointCoordinates(centerLocation.X - dimensions.Width / 2, centerLocation.Y - dimensions.Height / 2),
                                    new Dimension(dimensions.Width, dimensions.Height));

            if (roi.Top.Between(0, dimension.Height) && roi.Bottom.Between(0, dimension.Height) &&
               roi.Left.Between(0, dimension.Width) && roi.Right.Between(0, dimension.Width))
            {
                return Crop(roi);
            }

            return null;
        }
        #endregion
    }
}
