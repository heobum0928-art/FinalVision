using Project.BaseLib.DataStructures;
using Project.BaseLib.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Project.BaseLib.DataStructures
{
    public class ByteImage2
    {
        #region Members
        private int width;
        private int height;
        private byte[] data;
        private BitmapSource bmpSource;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Size { get { return width * height; } }
        public byte[] Data { get { return data; } }
        #endregion
        public ByteImage2(int _width, int _height)
        {
            width = _width;
            height = _height;
            data = new byte[width * height];
            Reset();
        }

        public ByteImage2(Stream imageStream)
        {
            FromStream(imageStream);
        }

        public ByteImage2(string fileName)
        {
            using (Stream imageStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                FromStream(imageStream);
        }

        private void FromStream(Stream imageStream)
        {
            try
            {


                //                _decoder = new BmpBitmapDecoder(_imageDataStream, BitmapCreateOptions.PreservePixelFormat,
                //    BitmapCacheOption.OnDemand);
                //else if (extension.Contains("TIF"))
                //                    _decoder = new TiffBitmapDecoder(_imageDataStream, BitmapCreateOptions.PreservePixelFormat,
                //                        BitmapCacheOption.OnDemand);




                BmpBitmapDecoder decoder = new BmpBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnDemand);

                //TiffBitmapDecoder decoder = new TiffBitmapDecoder(imageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                if (decoder.Frames.Count > 0)
                {
                    BitmapSource source = decoder.Frames[0];

                    width = source.PixelWidth;
                    height = source.PixelHeight;

                    data = new byte[source.PixelWidth * source.PixelHeight];
                    source.CopyPixels(data, source.PixelWidth, 0);

                    bmpSource = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, BitmapPalettes.Gray256, data, width);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in ByteImage.Constructor. see innerException for more details", ex);
            }
        }

        public void Save(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Create);

            TiffBitmapEncoder encoder = new TiffBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmpSource));
            encoder.Save(stream);
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmpSource));
                enc.Save(ms);
                return new System.Drawing.Bitmap(ms);
            }
        }

        public void Reset()
        {
            data.Initialize();
        }

        public void Fill(byte value)
        {
            System.Array.ForEach(data, d => d = value);
        }

        public ByteImage2 CreateChildImage(int offsetX, int offsetY, int sizeX, int sizeY)
        {
            ByteImage2 childImage = new ByteImage2(sizeX, sizeY);

            // copies picture to child
            int childIndex = 0;
            int index = offsetY * width + offsetX;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    childImage.data[childIndex] = data[index + x];
                    childIndex++;
                }
                index += width;
            }

            return childImage;
        }


    }
}
