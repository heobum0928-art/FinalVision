using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Project.BaseLib.DataStructures
{
    public class ImageConverter
    {

        static public TiffImage ToTiffImage(ByteImage byteImage, TiffCompressOption compression = TiffCompressOption.None)
        {
            return new TiffImage(byteImage.Data, byteImage.Dimension, compression);
        }

        static public TiffImage ToTiffImage(PtrImage ptrImage, TiffCompressOption compression = TiffCompressOption.None)
        {
            return new TiffImage(ptrImage.Ptr, ptrImage.Dimension, compression);
        }

        static public ByteImage ToByteImage(TiffImage tiffImage)
        {
            return new ByteImage(tiffImage.Dimension, tiffImage.ToByteArray(), 0);
        }

        static public ByteImage ToByteImage(PtrImage ptrImage)
        {
            ByteImage byteImage = new ByteImage(ptrImage.Dimension);
            System.Runtime.InteropServices.Marshal.Copy(
                ptrImage.Ptr,
                byteImage.Data,
                0,
                ptrImage.Length);
            return byteImage;
        }

        static public PtrImage ToPtrImage(ByteImage byteImage)
        {
            return new PtrImage(byteImage.Data, byteImage.Dimension);
        }

        static public PtrImage ToPtrImage(TiffImage tiffImage)
        {
            return new PtrImage(tiffImage.ToByteArray(), tiffImage.Dimension);
        }

    }
}
