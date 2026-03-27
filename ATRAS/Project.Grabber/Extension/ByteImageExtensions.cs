using Matrox.MatroxImagingLibrary;
using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public static class ByteImageExtensions
    {
        //public static ByteImage SetMilImage(this ByteImage byteImage, MIL_ID milImage)
        //{
        //    MIL_INT width = MIL.MbufInquire(milImage, MIL.M_SIZE_X);
        //    MIL_INT height = MIL.MbufInquire(milImage, MIL.M_SIZE_Y);

        //    ByteImage buffer = new ByteImage(((int)width), (int)height);

        //    MIL.MbufGet2d(milImage, 0, 0, width, height, buffer.Data);
        //    return buffer;
        //}

        public static bool SetMilImage(this ByteImage byteImage, MIL_ID milImage)
        {
            MIL_INT width = MIL.MbufInquire(milImage, MIL.M_SIZE_X);
            MIL_INT height = MIL.MbufInquire(milImage, MIL.M_SIZE_Y);

            byteImage = new ByteImage(((int)width), (int)height);

            MIL.MbufGet2d(milImage, 0, 0, width, height, byteImage.Data);
            return true;
        }

        public static void CreateImage(this ByteImage byteImage, ref MIL_ID milImage)
        {

        }
    }
}
