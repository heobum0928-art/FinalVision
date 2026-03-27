using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using Project.BaseLib.Extension;
using System.IO;

namespace Project.DeepLearning.UI
{
    public class ImageItem : NotifyPropertyChanged
    {
        #region fields

        protected string _ImageName;

        protected BitmapImage _ImageSource;

        #endregion

        #region propertise
        public string ImageName
        {
            get
            {
                return _ImageName;
            }

            set
            {
                _ImageName = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage ImageSource
        {
            get
            {
                return _ImageSource;
            }

            set
            {
                _ImageSource = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors

        public ImageItem()
        {
            _ImageName = string.Empty;
            _ImageSource = null;
        }
        #endregion
    }

    public class ClassInfo : NotifyPropertyChanged
    {
        #region fields
        static int CLASS_COUNT = 1;

        protected string _ClassName;

        public ObservableCollection<ImageItem> ImageItems { get; set; }

        #endregion

        #region propertise
        public string ClassName
        {
            get
            {
                return _ClassName;
            }

            set
            {
                _ClassName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public bool SaveImage(string root_folder)
        {
            if (ImageItems == null || ImageItems.Count == 0)
                return false;

            var save_folder = root_folder + "\\" + ClassName;

            if (!Directory.Exists(save_folder))
                Directory.CreateDirectory(save_folder);

            foreach(var image in ImageItems)
            {
                string path = save_folder + "\\" + image.ImageName;

                image.ImageSource.Save(path);                
            }

            return true;
        }


        //public byte[] GetPixelBytes(BitmapImage bitmapImage)
        //{
        //    if (bitmapImage == null)
        //    {
        //        throw new ArgumentNullException(nameof(bitmapImage));
        //    }

        //    // 렌더링된 이미지의 포맷을 가져옵니다.
        //    PixelFormat format = bitmapImage.Format;

        //    // WriteableBitmap을 사용하여 픽셀 데이터에 접근합니다.
        //    // BitmapImage를 WriteableBitmap으로 변환합니다.
        //    WriteableBitmap writableBitmap = new WriteableBitmap(bitmapImage);

        //    // 픽셀 데이터를 저장할 바이트 배열을 생성합니다.
        //    int width = writableBitmap.PixelWidth;
        //    int height = writableBitmap.PixelHeight;
        //    int bytesPerPixel = (format.BitsPerPixel + 7) / 8; // 각 픽셀당 바이트 수 계산
        //    int stride = width * bytesPerPixel; // 각 행의 바이트 수

        //    byte[] pixels = new byte[height * stride];

        //    // 픽셀 데이터를 배열에 복사합니다.
        //    writableBitmap.CopyPixels(pixels, stride, 0);

        //    return pixels;
        //}
        public byte[][] GetPixelBytesArray()
        {
            if (ImageItems == null || ImageItems.Count == 0)
                return null;

            byte[][] pixelsArray = new byte[ImageItems.Count][];

            int i = 0;
            foreach(var image in ImageItems)
            {
                pixelsArray[i] = image.ImageSource.GetPixelBytes();
                i++;
            }

            return pixelsArray;
        }
        #endregion

        #region constructors
        public ClassInfo()
        {
            ImageItems = new ObservableCollection<ImageItem>();

            ClassName = string.Format("Class_{0}", CLASS_COUNT);

            CLASS_COUNT++;
        }
        #endregion

    }
}
