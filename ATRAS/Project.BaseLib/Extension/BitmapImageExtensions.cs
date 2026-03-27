using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project.BaseLib.Extension
{
    public static class BitmapImageExtensions
    {
        public static byte[] GetPixelBytes(this BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
            {
                throw new ArgumentNullException(nameof(bitmapImage));
            }

            // 렌더링된 이미지의 포맷을 가져옵니다.
            PixelFormat format = bitmapImage.Format;

            // WriteableBitmap을 사용하여 픽셀 데이터에 접근합니다.
            // BitmapImage를 WriteableBitmap으로 변환합니다.
            WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);

            // 픽셀 데이터를 저장할 바이트 배열을 생성합니다.
            int width = writableBitmap.PixelWidth;
            int height = writableBitmap.PixelHeight;
            int bytesPerPixel = (format.BitsPerPixel + 7) / 8; // 각 픽셀당 바이트 수 계산
            int stride = width * bytesPerPixel; // 각 행의 바이트 수

            byte[] pixels = new byte[height * stride];

            // 픽셀 데이터를 배열에 복사합니다.
            writableBitmap.CopyPixels(pixels, stride, 0);

            return pixels;
        }
        
        public static void Save(this BitmapImage bitmapImage, string filePath)
        {
            if (bitmapImage == null)
            {
                throw new ArgumentNullException(nameof(bitmapImage));
            }

            BitmapEncoder encoder = null;
            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                case ".wmp": // Windows Media Photo (HD Photo)
                    encoder = new WmpBitmapEncoder();
                    break;
                default:
                    throw new ArgumentException($"지원하지 않는 파일 형식입니다: {extension}");
            }

            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                Console.WriteLine($"이미지가 {filePath}에 성공적으로 저장되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 저장 오류: {ex.Message}");
            }
        }

        public static BitmapImage Duplicate(this BitmapImage source)
        {
            if (source == null)
                return null;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                var clone = new BitmapImage();
                clone.BeginInit();
                clone.CacheOption = BitmapCacheOption.OnLoad;
                clone.StreamSource = memoryStream;
                clone.EndInit();
                clone.Freeze(); // 선택: 스레드 안정성을 위해

                return clone;
            }
        }
    }
}
