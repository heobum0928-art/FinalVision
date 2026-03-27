using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Project.BaseLib.Extension
{
  public static class ImageExtensions
  {
    static ColorPalette palette = null;

    static System.Drawing.Imaging.ColorPalette Palette
    {
      get
      {
        if(palette == null)
        {
          Bitmap bmpp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

          palette = bmpp.Palette;
          Color[] entries = palette.Entries;
          for (int i = 0; i < 256; i++)
            entries[i] = Color.FromArgb(i, i, i);

        }
        return palette;
      }
    }
    public static Bitmap CreateBitmap(int width, int height)
    {
      Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
      bmp.Palette = Palette;

      return bmp;
    }
        public static byte[] ImageToByte(this Bitmap bmp)
        {
            // 1. 순수 픽셀 데이터만 담을 배열 (Padding 제외)
            byte[] data = new byte[bmp.Width * bmp.Height];

            // 2. 비트맵 락
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            try
            {
                IntPtr curPtr = bmpData.Scan0;
                int stride = bmpData.Stride;
                int width = bmp.Width;

                // 3. 줄 단위로 복사 (Stride의 패딩 바이트를 건너뜀)
                for (int y = 0; y < bmp.Height; y++)
                {
                    // 원본 포인터에서 현재 줄의 위치를 계산
                    IntPtr rowPtr = IntPtr.Add(curPtr, y * stride);

                    // 목적지 배열의 현재 위치 계산
                    int targetOffset = y * width;

                    // 실제 데이터 폭(Width)만큼만 복사
                    Marshal.Copy(rowPtr, data, targetOffset, width);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return data;

            //byte[] data = new byte[bmp.Width * bmp.Height];
            //BitmapData bmpData = bmp.LockBits(new Rectangle(new System.Drawing.Point(0, 0), bmp.Size), ImageLockMode.ReadOnly,
            //  System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //try
            //{
            //  Marshal.Copy(bmpData.Scan0, data, 0, data.Length);
            //}
            //finally
            //{
            //  bmp.UnlockBits(bmpData);
            //}
            //return data;
    }

    public static Bitmap ByteToImage(byte[] image, int width, int height, int offset)
    {
      Bitmap bmp = CreateBitmap(width, height);
      BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
        ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

      try
      {
        Marshal.Copy(image, offset, bmpData.Scan0, width * height);
      }
      finally
      {
        bmp.UnlockBits(bmpData);
      }
      return bmp;
    }
  }
}
