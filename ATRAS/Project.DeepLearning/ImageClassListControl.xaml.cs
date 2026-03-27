using Microsoft.Win32;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.DeepLearning.UI
{
    /// <summary>
    /// ImageClassListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageClassListControl : UserControl, INotifyPropertyChanged
    {
        #region fields

        protected ClassInfo _ClassInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        protected int _Width = 0;
        protected int _Height = 0;
        #endregion

        #region propertise
        public DelegateCommand UploadCommand { get; set; }

        public ClassInfo ClassInfo
        {
            get
            {
                return _ClassInfo;
            }

            set
            {
                _ClassInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public bool SaveImage(string root_folder)
        {
            if (_ClassInfo == null)
                return false;

            return _ClassInfo.SaveImage(root_folder);
        }


        protected BitmapImage CenterImage(BitmapImage sourceImage, int targetWidth, int targetHeight)
        {
            int sourceWidth = sourceImage.PixelWidth;
            int sourceHeight = sourceImage.PixelHeight;

            // 픽셀 포맷: 흑백 (8비트 그레이스케일)
            PixelFormat pixelFormat = PixelFormats.Gray8;
            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

            // WriteableBitmap 생성 (배경 자동 0 = 검정)
            WriteableBitmap targetBitmap = new WriteableBitmap(
                targetWidth, targetHeight,
                sourceImage.DpiX, sourceImage.DpiY,
                pixelFormat, null
            );

            // 원본을 흑백으로 변환
            BitmapSource graySource = new FormatConvertedBitmap(sourceImage, pixelFormat, null, 0);

            // 픽셀 데이터 복사
            int stride = sourceWidth * bytesPerPixel;
            byte[] pixels = new byte[sourceHeight * stride];
            graySource.CopyPixels(pixels, stride, 0);

            // 중앙 위치 계산
            int offsetX = (targetWidth - sourceWidth) / 2;
            int offsetY = (targetHeight - sourceHeight) / 2;

            // 중심에 복사
            targetBitmap.WritePixels(
                new Int32Rect(offsetX, offsetY, sourceWidth, sourceHeight),
                pixels, stride, 0
            );

            // WriteableBitmap → BitmapImage 변환
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                encoder.Save(stream);
                stream.Position = 0;

                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                return result;
            }
        }


        protected void OnUploadCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*";
            openFileDialog.Title = "파일 선택";

            if (openFileDialog.ShowDialog() == true)
            {
                string[] selectedFiles = openFileDialog.FileNames;

                ClassInfo.ImageItems.Clear();

                var targetWidth = DeepLearningManager.Instance.DataSetWidth;
                var targetHeight = DeepLearningManager.Instance.DataSetHeight;
                // 선택한 파일 경로 처리
                foreach (string filePath in selectedFiles)
                {
                    //// 파일 경로를 사용하여 원하는 작업 수행
                    //var fileName = System.IO.Path.GetFileName(filePath);
                    //var org_image = new BitmapImage(new Uri(filePath, UriKind.Absolute));

                    ////AppLogger.Info()(fileName);
                    //ClassInfo.ImageItems.Add(
                    //    new ImageItem
                    //    {
                    //        ImageName = fileName,
                    //        ImageSource = org_image
                    //    });    

                    var fileName = System.IO.Path.GetFileName(filePath);

                    BitmapImage org_image = new BitmapImage();

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        org_image.BeginInit();
                        org_image.CacheOption = BitmapCacheOption.OnLoad; // 파일 로딩 후 핸들 해제
                        org_image.UriSource = null;                       // Uri 대신 스트림 사용
                        org_image.StreamSource = stream;
                        org_image.EndInit();
                        org_image.Freeze(); // UI 스레드에서 안전하게 사용
                    }

                    ClassInfo.ImageItems.Add(
                        new ImageItem
                        {
                            ImageName = fileName,
                            ImageSource = org_image
                        });
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region constructors

        protected ImageClassListControl()
        {
            InitializeComponent();
            DataContext = this;
            ClassInfo = new ClassInfo();
            UploadCommand = new DelegateCommand(OnUploadCommand);
        }


        public ImageClassListControl(int Width, int Height)
        {
            _Width = Width;
            _Height = Height;
            InitializeComponent();
            DataContext = this;
            ClassInfo = new ClassInfo();
            UploadCommand = new DelegateCommand(OnUploadCommand);
        }

        #endregion



        //public static readonly DependencyProperty ClassInfoProperty =
        //    DependencyProperty.Register("ClassInfo", typeof(ClassInfo), typeof(ImageClassListControl), new PropertyMetadata(null));

        //public ClassInfo ClassInfo
        //{
        //    get
        //    {
        //        return (ClassInfo)GetValue(ClassInfoProperty);
        //    }

        //    set
        //    {
        //        SetValue(ClassInfoProperty, value);
        //    }
        //}


    }
}
