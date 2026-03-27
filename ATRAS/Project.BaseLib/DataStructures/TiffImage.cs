using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [Serializable]
    public class TiffImage : ByteImageBase
    {
        public TiffImage()
        {
        }

        public TiffImage(string fileName)
        {
            FromFile(fileName);
        }

        public TiffImage(Stream stream)
        {
            FromStream(stream);
        }

        public TiffImage(byte[] imageData, ImageDimension dimension, TiffCompressOption compression = TiffCompressOption.None, int offset = 0)
        {
            FromByteArray(imageData, dimension, compression, offset);
        }

        public TiffImage(IntPtr imagePtr, ImageDimension dimension, TiffCompressOption compression = TiffCompressOption.None)
        {
            FromBytePtr(imagePtr, dimension, compression);
        }

        public TiffImage(BitmapSource image, TiffCompressOption compression = TiffCompressOption.None)
        {
            Encode(image, compression);
        }

        public override void Clear()
        {
            base.Clear();
        }

        public TiffImage Copy()
        {
            TiffImage copyData = new TiffImage();
            base.Copy(copyData);

            return copyData;
        }

        public new TiffImage Duplicate()
        {
            return (TiffImage)this.Copy();
        }

        public TiffImage GetMinimizedTiffImage(float minValue, TiffCompressOption compression = TiffCompressOption.None)
        {
            float resizeFactor = 1.0f;

            if (minValue <= 0)
                return null;

            if (this.Dimension.Width >= minValue && this.Dimension.Height >= minValue)
            {
                int minDimension = Math.Min(Dimension.Width, Dimension.Height);

                resizeFactor = minValue / minDimension;
            }

            return GetScaledTiffImage(compression, resizeFactor);
        }

        public TiffImage GetScaledTiffImage(TiffCompressOption compression = TiffCompressOption.None, float resizeFactor = 1.0f)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(Data);
            bitmapImage.EndInit();

            var bitmapScale = new TransformedBitmap(bitmapImage, new ScaleTransform(resizeFactor, resizeFactor));
            TiffImage t = new TiffImage(bitmapScale, compression);

            return t;
        }

        public void ToFile(string name, TiffCompressOption compression = TiffCompressOption.None, float resizeFactor = 1.0f)
        {
            if (resizeFactor == 1.0f && Compression == compression)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(name, System.IO.FileMode.Create))
                {
                    ToStream(fs);
                    fs.Close();
                }

                return;
            }

            TiffImage t = GetScaledTiffImage(compression, resizeFactor);

            using (System.IO.FileStream fs = new System.IO.FileStream(name, System.IO.FileMode.Create))
            {
                if (t.Data != null)
                    fs.Write(t.Data, 0, t.Data.Length);
                fs.Close();
            }
        }

        public byte[] ToByteArray()
        {
            byte[] data = null;
            try
            {

                using (Stream ms = new MemoryStream(Data))
                {
                    TiffBitmapDecoder decoder = new TiffBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    if (decoder.Frames.Count > 0)
                    {
                        BitmapSource source = decoder.Frames[0];

                        data = new byte[source.PixelWidth * source.PixelHeight];
                        source.CopyPixels(data, source.PixelWidth, 0);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in TiffImage.ToByteArray. see innerException for more details", ex);
            }

            return data;
        }

        private void FromStream(Stream imageStream)
        {
            Data = new Byte[imageStream.Length];
            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.Read(Data, 0, Data.Length);

            if (dimension.Width == 0 && dimension.Height == 0)
            {
                using (Image tif = Image.FromStream(imageStream, false, false))
                {
                    dimension.Width = tif.Width;
                    dimension.Height = tif.Height;
                    dimension.Pitch = tif.Width;
                }
            }
            else
            {
                dimension.Pitch = dimension.Width;
            }
        }

        public bool FromByteArray(byte[] imageData, ImageDimension dimension, TiffCompressOption compression, int offset)
        {
            try
            {

                if (((dimension.Width * dimension.Height) + offset) > imageData.Length)
                {
                    throw new ArgumentOutOfRangeException(String.Format("ImageDimension+offset {0}+{1} is larger than Byte Data length{2}", dimension.ToString(), offset, imageData.Length));
                }

                GCHandle pch = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                IntPtr ptr = pch.AddrOfPinnedObject();

                FromBytePtr(ptr + offset, dimension, compression);

                pch.Free();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in TiffImage.FromByteArray. see innerException for more details", ex);
            }

            return true;
        }
        public bool FromBytePtr(IntPtr imageData, ImageDimension dimension, TiffCompressOption compression)
        {
            this.dimension = dimension.Duplicate();
            this.dimension.Pitch = this.dimension.Pitch == 0 ? dimension.Width : this.dimension.Pitch;
            int bufferSize = dimension.Pitch * dimension.Height;

            BitmapPalette myPalette = BitmapPalettes.Gray256;
            BitmapSource image = BitmapSource.Create(dimension.Width, dimension.Height, 96, 96, System.Windows.Media.PixelFormats.Gray8, myPalette, imageData, bufferSize, dimension.Pitch);

            Encode(image, compression);

            return true;
        }

        private void Encode(BitmapSource image, TiffCompressOption compression = TiffCompressOption.None)
        {
            using (Stream imageStreamSource = new MemoryStream())
            {
                TiffBitmapEncoder encoder = new TiffBitmapEncoder();

                BitmapMetadata metadata = null;
                if (image.Metadata == null)
                    metadata = new BitmapMetadata("tiff");
                else
                    metadata = image.Metadata.Clone() as BitmapMetadata;

                metadata.SetQuery("/ifd/{ushort=274}", (ushort)1);
                encoder.Compression = compression;
                encoder.Frames.Add(BitmapFrame.Create(image, null, metadata, null));
                encoder.Save(imageStreamSource);

                FromStream(imageStreamSource);
            }

        }

        public bool FromFile(string imageFileNamePath)
        {
            try
            {
                dimension.Width = 0;
                dimension.Height = 0;
                dimension.Pitch = 0;

                using (Stream imageStreamSource = new FileStream(imageFileNamePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    FromStream(imageStreamSource);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in TiffImage.FromFile. see innerException for more details", ex);
            }

            return true;
        }

        static public byte[] CreateMultiTiff(List<TiffImage> images)
        {
            return CreateMultiTiff(images.Select(im => im.ToStream()).ToList());
        }

        static public byte[] CreateMultiTiff(List<Stream> imageStreams)
        {
            using (MemoryStream imageStreamSource = new MemoryStream())
            {
                var decoders = imageStreams.Select(imageStream => (new TiffBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default))).ToList();
                var frames = decoders.Select(d => d.Frames).ToList();

                TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                encoder.Frames = frames.SelectMany(fl => fl).ToList();
                encoder.Save(imageStreamSource);
                return imageStreamSource.ToArray();
            }
        }

        static public List<TiffImage> SplitMultiTiff(Stream image)
        {
            var decoder = new TiffBitmapDecoder(image, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            var frames = decoder.Frames;
            return frames.Select(f => new TiffImage(f)).ToList();
        }

        public byte[] ToJpg()
        {
            using (MemoryStream imageStreamSource = new MemoryStream())
            {
                var pixels = this.ToByteArray();
                BitmapPalette myPalette = BitmapPalettes.Gray256;
                BitmapSource image = BitmapSource.Create(dimension.Width, dimension.Height, 96, 96, System.Windows.Media.PixelFormats.Gray8, myPalette, pixels, dimension.Pitch);

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(imageStreamSource);

                return imageStreamSource.ToArray();
            }
        }

        private T GetTagValue<T>(int tag)
        {
            object Temp;

            using (MemoryStream ms = new MemoryStream(Data))
            {
                using (Image tif = Image.FromStream(ms, false, false))
                {
                    int tagIndex = Array.IndexOf(tif.PropertyIdList, tag);
                    if (tagIndex == -1)
                        throw new Exception(String.Format("Unsupported tag code {0}", tag));

                    PropertyItem tagItem = tif.PropertyItems[tagIndex];

                    T _Value = default(T);

                    Temp = _Value; // temp to avoid casting

                    if (_Value is Boolean) Temp =
                        BitConverter.ToBoolean(tagItem.Value, 0);
                    else if (_Value is UInt32) Temp =
                        BitConverter.ToUInt32(tagItem.Value, 0);
                    else if (_Value is UInt16) Temp =
                        BitConverter.ToUInt16(tagItem.Value, 0);
                    else if (_Value is Byte)
                        Temp = tagItem.Value[0];
                    else if (_Value is Double) Temp =
                        BitConverter.ToDouble(tagItem.Value, 0);
                    else if (_Value is String) Temp =
                        BitConverter.ToString(tagItem.Value);
                    else
                        throw new Exception(String.Format("Unsupported tag type {0}", typeof(T).ToString()));

                }
            }
            return (T)Temp;
        }

        public override long[] CalculateGrayscaleHistogram(out double average, out int peakGL, out int histogramPeak)
        {
            return ImageConverter.ToByteImage(this).CalculateGrayscaleHistogram(out average, out peakGL, out histogramPeak);
        }

        public override ByteImageBase Crop(RoiRectangle roi)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(Data);
            byte[] buffer = new byte[roi.Size.Width * roi.Size.Height];
            bitmapImage.EndInit();
            bitmapImage.CopyPixels(new System.Windows.Int32Rect(roi.Left, roi.Top, roi.Size.Width, roi.Size.Height), buffer, roi.Size.Width, 0);

            return new TiffImage(buffer, new ImageDimension(roi.Size.Width, roi.Size.Height, roi.Size.Width));
        }

        public TiffCompressOption Compression
        {
            get
            {
                ushort compression = GetTagValue<ushort>(259);
                switch (compression)
                {
                    case 1:
                        return TiffCompressOption.None;
                    case 2:
                        return TiffCompressOption.Rle;
                    case 3:
                        return TiffCompressOption.Ccitt3;
                    case 4:
                        return TiffCompressOption.Ccitt4;
                    case 5:
                        return TiffCompressOption.Lzw;
                    case 32946:
                        return TiffCompressOption.Zip;
                    default:
                        throw new Exception(String.Format("Unknown compression type {0}", compression));
                }
            }
        }

        public BitmapImage ToBitmapImage()
        {
            if (Data == null)
                return null;

            var image = new BitmapImage();

            using (var mem = new MemoryStream(Data))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }

            image.Freeze();

            return image;
        }

    }
}
