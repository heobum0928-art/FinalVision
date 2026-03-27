using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Extension;
using Project.BaseLib.Utils;
using Project.UI.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Project.OCR.UI
{
    /// <summary>
    /// OCRTeachingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OCRTeachingView : UserControl, INotifyPropertyChanged
    {
        #region fields
        public event PropertyChangedEventHandler PropertyChanged;

        protected ByteImage _ByteImage;

        protected ByteImage _RoiImage;

        protected RoiRectangle _ROI;

        protected string _ModelName;

        protected int _CharCount;

        protected string _ImageInformation;


        protected CharacterInfo _SelectedImageItem;


        protected CharacterInfos _CharacterInfos;

        //protected CharacterInfos _FontInfos;

        protected FontInfos _FontInfos;

        protected string _TestString;

        protected string _StringTest1;

        protected string _StringTest2;

        protected string _StringTest3;


        protected CharacterInfos _TestInfos;

        protected bool _DebugImage;
        #endregion

        #region propertise
        public CharacterInfos TestInfos
        {
            get
            {
                return _TestInfos;
            }

            set
            {
                _TestInfos = value;
                OnPropertyChanged();
            }
        }


        public CharacterInfos CharacterInfos
        {
            get
            {
                return _CharacterInfos;
            }

            set
            {
                _CharacterInfos = value;
                OnPropertyChanged();
            }
        }

        public FontInfos FontInfos
        {
            get
            {
                return _FontInfos;
            }

            set
            {
                _FontInfos = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand FontTestImageLoadCommand { get; set; }

        public DelegateCommand ImageLoadCommand { get; set; }

        public DelegateCommand FontLoadCommand { get; set; }
        public DelegateCommand FontSaveCommand { get; set; }
        public DelegateCommand FontSaveAsCommand { get; set; }

        public DelegateCommand CharacterRemoveCommand { get; set; }
        public DelegateCommand CharacterAllRemoveCommand { get; set; }

        public DelegateCommand SetROICommand { get; set; }

        public DelegateCommand FontAddCommand { get; set; }

        public DelegateCommand OCRTrainingCommand { get; set; }
        public DelegateCommand OCRTestCommand { get; set; }


        public DelegateCommand AddROICommand { get; set; }

        public DelegateCommand TrainImageSaveCommand { get; set; }

        public DelegateCommand Test1Command { get; set; }
        public DelegateCommand Test2Command { get; set; }
        public DelegateCommand Test3Command { get; set; }

        public string ModelName
        {
            get
            {
                return _ModelName;
            }

            set
            {
                _ModelName = value;

                OnPropertyChanged();
            }
        }

        public CharacterPolarity CharacterPolarity
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharacterPolarity;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterPolarity = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public int CharacterMinWidth
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharacterMinWidth;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterMinWidth = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public int CharacterMaxWidth
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharacterMaxWidth;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterMaxWidth = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public int CharacterMinHeight
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharacterMinHeight;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterMinHeight = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public int CharacterMaxHeight
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharacterMaxHeight;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterMaxHeight = value;
                config.Save();

                OnPropertyChanged();
            }
        }



        public int CharMinArea
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharMinArea;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharMinArea = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public int CharMaxArea
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();


                return config.CharMaxArea;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharMaxArea = value;
                config.Save();

                OnPropertyChanged();
            }
        }




        public int CharacterThreshold
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.CharacterThreshold;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharacterThreshold = value;
                config.Save();

                OnPropertyChanged();
            }
        }


        public int OtsuLevel
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.OtsuLevel;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.OtsuLevel = value;
                config.Save();

                OnPropertyChanged();
            }
        }


        public CharacterInfo SelectedImageItem
        {
            get
            {
                return _SelectedImageItem;
            }

            set
            {
                _SelectedImageItem = value;
                OnPropertyChanged();
            }
        }



        public bool GrayImage
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.GrayImage;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.GrayImage = value;
                config.Save();

                OnPropertyChanged();

                OnPropertyChanged("CTLEnable");
            }
        }


        public bool DebugImage
        {
            get
            {
                return _DebugImage;
            }

            set
            {
                _DebugImage = value;
                OnPropertyChanged();
            }
        }

        public bool AutoProcessing
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.AutoProcessing;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.AutoProcessing = value;
                config.Save();

                OnPropertyChanged();

                OnPropertyChanged("CTLEnable");
            }
        }

        public int DilationErosion
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.DilationErosion;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.DilationErosion = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public int CharGap
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.CharGap;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.CharGap = value;
                config.Save();

                OnPropertyChanged();
            }
        }


        public int FillHoleArea
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                return config.FillHoleArea;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                config.FillHoleArea = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public string ImageInformation
        {
            get
            {
                return _ImageInformation;
            }

            set
            {
                if(value != null)
                {
                    _ImageInformation = value;
                    OnPropertyChanged();
                }

            }
        }

        public string TestString
        {
            get
            {
                return _TestString;
            }

            set
            {
                _TestString = value;
                OnPropertyChanged();
            }
        }

        public string StringTest1
        {
            get
            {
                return _StringTest1;
            }

            set
            {
                _StringTest1 = value;
                OnPropertyChanged();
            }
        }

        public string StringTest2
        {
            get
            {
                return _StringTest2;
            }

            set
            {
                _StringTest2 = value;
                OnPropertyChanged();
            }
        }

        public string StringTest3
        {
            get
            {
                return _StringTest3;
            }

            set
            {
                    _StringTest3 = value;
                    OnPropertyChanged();
            }
        }

        #endregion

        #region methods
        
        protected void OnTest1Command()
        {
            var test_image = img_view.GetByteImage();

            //test_image.Save("C:\\test_image.bmp");

            var rois = img_view.GetAllRoiRectangles();

            TestImageTypes test_image_type = GrayImage == true ? TestImageTypes.Gray : TestImageTypes.Binary;

            if (rois.Length == 0)
            {
                MessageBox.Show("ROI is Empty.");
                AppLogger.Info()("Roi is empty.");
                return;
            }

            test_image = test_image.Crop(rois[0]) as ByteImage;

            //test_image.Save("C:\\test_image__.bmp");
            if(test_image == null)
            {
                MessageBox.Show("Crop Image is Null.");
                AppLogger.Info()("Crop Image is Null.");
                return;
            }

            //var bin_image = test_image.Binarization(0, CharacterThreshold, BaseLib.Enums.BinTypes.InSide);

            //test_image.Save("D:\\test_image.bmp");

            //var min_area = CharacterMaxWidth + CharacterMaxHeight;
            //var max_area = CharacterMaxWidth * CharacterMaxHeight;

            //var blobs = ByteImage.Blob(bin_image, min_area, max_area, CharacterMinWidth, CharacterMaxWidth,
            //                        CharacterMinHeight, CharacterMaxHeight, BaseLib.Enums.BlobTypes.White);

            //if (blobs.Count == 0)
            //{
            //    MessageBox.Show("Blob is Empty.");
            //    AppLogger.Info()("Blob is empty.");
            //    return;
            //}


            //blobs = blobs.OrderBy(p => p.Left).ToList();

            //List<ByteImage> list = new List<ByteImage>();

            //foreach (var blob in blobs)
            //{
            //    var roi = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

            //    ByteImage blob_image = null;

            //    if (test_image_type == TestImageTypes.Gray)
            //    {
            //        blob_image = test_image.Crop(roi) as ByteImage;
            //    }
            //    else
            //    {
            //        blob_image = bin_image.Crop(roi) as ByteImage;
            //    }

            //    var extand_image = new ByteImage(blob_image.Width + (CharGap * 2), blob_image.Height + (CharGap * 2));

            //    RoiRectangle rect = new RoiRectangle(CharGap, CharGap, blob.Height + CharGap, blob.Width + CharGap);

            //    extand_image.InsertChildBuffer(rect, blob_image.Data);
            //    var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, FillHoleArea);
            //    list.Add(fillhole_image);
            //}

            //int i = 0;
            //if (DebugImage == true)
            //{
            //    foreach (var image in list)
            //    {
            //        image.Save(string.Format("D:\\test_image_{0}.bmp", i));
            //        i++;
            //    }
            //}

            List<CharacterTypes> CharTypes = OCRManager.Instance.GetCharacterTypes(TestString);

            //var infos = OCRManager.Instance.MultiCharProcessing(test_image, ModelName, CharacterThreshold, OtsuLevel, CharacterPolarity.DarkOnLight, 80, TestString, test_image_type, DebugImage);
            //var infos = OCRManager.Instance.MultiCharProcessing(test_image, ModelName, CharacterPolarity.DarkOnLight, 80, TestString, test_image_type, ThresholdTypes.Relative, DebugImage);
            var infos = OCRManager.Instance.MultiCharProcessing(test_image, ModelName, 80, TestString, test_image_type, ThresholdTypes.Relative, DebugImage);

            if (infos.Count == 0)
            {
                MessageBox.Show("Character is Empty.");
                AppLogger.Info()("Character is empty.");
                return;
            }

            StringTest1 = "";
            foreach (var info in infos)
            {
                StringTest1 += info.Character;
            }
        }

        protected void OnTest2Command()
        {

        }

        protected void OnTest3Command()
        {

        }

        protected void RecipeLoad()
        {
            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
            if (config != null)
            {
                ModelName = config.RecipeName;


                var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();

                if (recipe != null)
                {
                    recipe.Load(config.RecipePath, ModelName);

                    FontInfos = new FontInfos();

                    var result = OCRManager.Instance.LoadAllFont(config.RecipePath);
                    if (result == true)
                    {
                        var list = OCRManager.Instance.GetCharacterInfoList(ModelName);

                        FontInfos.SetCharacter(list);
                    }
                }

                OnPropertyChanged("FontInfo");

                OnPropertyChanged("CharacterPolarity");

                OnPropertyChanged("CharacterMinWidth");
                OnPropertyChanged("CharacterMaxWidth");
                OnPropertyChanged("CharacterMinHeight");
                OnPropertyChanged("CharacterMaxHeight");
                OnPropertyChanged("CharacterThreshold");
                OnPropertyChanged("OtsuLevel");
                OnPropertyChanged("CharCount");
            }
        }

        protected void RecipeLoad(string recipe_name)
        {

            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
            if (config != null)
            {
                ModelName = recipe_name;


                var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();

                if(recipe != null)
                {
                    recipe.Load(config.RecipePath, ModelName);
                }

                //OnPropertyChanged("CharacterPolarity");

                //OnPropertyChanged("CharacterMinWidth");
                //OnPropertyChanged("CharacterMaxWidth");
                //OnPropertyChanged("CharacterMinHeight");
                //OnPropertyChanged("CharacterMaxHeight");
                //OnPropertyChanged("CharacterThreshold");
            }
        }
        
        public void OnSetROICommand()
        {
            var rois = img_view.GetAllRoiRectangles();

            var count = rois.Count();
            if (count == 1)
            {
                _ROI = rois[0];

                _RoiImage = _ByteImage.Crop(_ROI) as ByteImage;

                roi_img_view.SetImage(_RoiImage);
            }
        }

        public void OnFontLoadCommand()
        {
            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();

            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Font Load";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = config.RecipePath + "\\Recipe";

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dlg.FileName;
                AppLogger.Info()("Font Load Path : [{0}].", selectedPath);

                ModelName = System.IO.Path.GetFileName(selectedPath);

                config.RecipeName = ModelName;
                config.Save();

                RecipeLoad();
            }
        }

        public void OnFontSaveCommand()
        {
            var result = MessageBox.Show(
                    "Save 작업을 진행하시겠습니까?",
                    "확인 요청", MessageBoxButton.OKCancel
                );

            if (result == MessageBoxResult.OK)
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
                var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                if (recipe != null && config != null)
                {
                    recipe.Save(config.RecipePath, config.RecipeName);
                }

                if (config != null)
                {
                    var r = OCRManager.Instance.SetFontList(config.RecipeName, FontInfos.Items);
                    r = OCRManager.Instance.SaveFont(config.RecipePath, config.RecipeName);
                }
            }
        }

        public void OnFontSaveAsCommand()
        {
            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();

            var fr_dlg = new FolderRecipeWindow(config.RecipePath + "\\Recipe");
            var result = fr_dlg.ShowDialog();

            if (fr_dlg.Result == MessageBoxResult.OK)
            {
                string selectedPath = fr_dlg.FolderName;
                AppLogger.Info()("Model Export Path : [{0}].", selectedPath);

                ModelName = System.IO.Path.GetFileName(selectedPath);

                config.RecipeName = ModelName;

                var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                recipe.Save(config.RecipePath, ModelName);
                config.Save();

                var r = OCRManager.Instance.SetFontList(config.RecipeName, FontInfos.Items);
                r = OCRManager.Instance.SaveFont(config.RecipePath, config.RecipeName);

            }
        }

        public void OnImageLoadCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*";
            openFileDialog.Title = "파일 선택";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                
                _ByteImage = new ByteImage(selectedFile);

                //img_view.SetByteImage(_ByteImage);
                img_view.SetImage(_ByteImage);


                img_view.RemoveAllShape();
                var size = img_view.ImageSize;

                var pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 2);

                img_view.DrawRectangle(pen, System.Drawing.Brushes.Transparent, 10, 10, size.Width - 10, size.Height - 10);

            }
        }
                
        public void OnCharacterRemoveCommand()
        {
            if(SelectedImageItem != null)
            {
                AppLogger.Info()(SelectedImageItem.ToString());
                FontInfos.RemoveCharacter(SelectedImageItem.ImageName);

            }
        }

        public void OnCharacterAllRemoveCommand()
        {
            var result = MessageBox.Show(
                    "모든 Font를 삭제 하시겠습니까?",
                    "확인 요청", MessageBoxButton.OKCancel
                );

            if(result == MessageBoxResult.OK)
            {
                FontInfos.AllRemoveCharacter();
            }
            
        }

        public void OnFontAddCommand()
        {
            if (CharacterInfos == null)
                return;

            if (FontInfos == null)
                FontInfos = new FontInfos();

            FontInfos.SetCharacter(CharacterInfos.Items.ToList());
        }

        public void OnFontTestImageLoadCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*";
            openFileDialog.Title = "파일 선택";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;

                _ByteImage = new ByteImage(selectedFile);

                roi_img_view.SetImage(_ByteImage);
            }
        }

        public void OnOCRTestCommand()
        {

        }

        public void OnAddROICommand()
        {
            img_view.RemoveAllShape();
            var size = img_view.ImageSize;

            var pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 2);

            img_view.DrawRectangle(pen, System.Drawing.Brushes.Transparent, 5, 5, size.Width - 5, size.Height - 5);
        }

        public void OnOCRTrainingCommand()
        {
            TestImageTypes test_image_type = GrayImage == true ? TestImageTypes.Gray : TestImageTypes.Binary;
            //var result = OCRManager.Instance.Training(roi_img_view.GetByteImage(), 0, CharacterThreshold, OtsuLevel, CharacterPolarity, CharacterMinWidth, CharacterMaxWidth, CharacterMinHeight, CharacterMaxHeight, DilationErosion, CharGap, FillHoleArea, test_image_type, DebugImage, AutoProcessing);



            //var result = OCRManager.Instance.Training(roi_img_view.GetByteImage(), OtsuLevel, CharacterPolarity, CharacterMinWidth, CharacterMaxWidth, CharacterMinHeight, CharacterMaxHeight, DilationErosion, CharGap, FillHoleArea, test_image_type, ThresholdTypes.Relative, DebugImage);

            var result = OCRManager.Instance.Training(roi_img_view.GetByteImage(), ModelName, DebugImage);
            //(roi_img_view.GetByteImage(), OtsuLevel, CharacterPolarity, CharacterMinWidth, CharacterMaxWidth, CharacterMinHeight, CharacterMaxHeight, DilationErosion, CharGap, FillHoleArea, test_image_type, test_image_type, ThresholdTypes.Relative, DebugImage);
            CharacterInfos = new CharacterInfos();

            CharacterInfos.Items.AddRange(OCRManager.Instance.CharacterInfoList);

            roi_img_view.RemoveAllShape();
            int i = 0;
            foreach (var item in CharacterInfos.Items)
            {
                //roi_img_view.SetRectangle(i, item.Rectangle.Left, item.Rectangle.Top, item.Rectangle.Width, item.Rectangle.Height, Colors.Red, BaseLib.Enums.ROITypes.Detect_Black, i);

                var pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 1);
                roi_img_view.DrawRectangle(pen, System.Drawing.Brushes.Transparent, item.Rectangle.Left, item.Rectangle.Top, item.Rectangle.Right, item.Rectangle.Bottom);
                i++;
            }
        }

        public void OnTrainImageSaveCommand()
        {
            if (_CharacterInfos == null)
            {
                return;
            }

            foreach(var item in _CharacterInfos.Items)
            {
                if (item.ImageName == "" || item.ImageName == "?")
                    continue;

                string name = string.Format("{0}", item.ImageName);
                item.ImageSource.Save(string.Format("D:\\{0}.bmp", name));
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
        
        public void OnMouseMoveImageInformation(int x, int y, int gray)
        {
            ImageInformation = string.Format("X = [{0}], Y = [{1}], Gray = [{2}]", x, y, gray);
        }
        #endregion

        #region constructors
        public OCRTeachingView()
        {
            InitializeComponent();

            DataContext = this;

            Test1Command = new DelegateCommand(OnTest1Command);
            Test2Command = new DelegateCommand(OnTest2Command);
            Test3Command = new DelegateCommand(OnTest3Command);

            ImageLoadCommand = new DelegateCommand(OnImageLoadCommand);
            FontLoadCommand = new DelegateCommand(OnFontLoadCommand);
            FontSaveCommand = new DelegateCommand(OnFontSaveCommand);
            FontSaveAsCommand = new DelegateCommand(OnFontSaveAsCommand);
            SetROICommand = new DelegateCommand(OnSetROICommand);

            CharacterRemoveCommand = new DelegateCommand(OnCharacterRemoveCommand);

            CharacterAllRemoveCommand = new DelegateCommand(OnCharacterAllRemoveCommand);


            FontAddCommand = new DelegateCommand(OnFontAddCommand);
            FontTestImageLoadCommand = new DelegateCommand(OnFontTestImageLoadCommand);
            OCRTestCommand = new DelegateCommand(OnOCRTestCommand);

            AddROICommand = new DelegateCommand(OnAddROICommand);

            OCRTrainingCommand = new DelegateCommand(OnOCRTrainingCommand);

            TrainImageSaveCommand = new DelegateCommand(OnTrainImageSaveCommand);

            //img_view.SetImage(new ByteImage(500, 100));
            roi_img_view.SetImage(new ByteImage(7314, 1900));
            
            //roi_img_view.RegisterImageInformationEvent(OnMouseMoveImageInformation);
            RecipeLoad();

            _StringTest1 = "String_Test1";

            _StringTest2 = "String_Test2";

            _StringTest3 = "String_Test3";

            _DebugImage = false;

            _TestString = "";
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //img_view.AddROI();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*";
            openFileDialog.Title = "파일 선택";

            if (openFileDialog.ShowDialog() == true)
            {

                CharacterInfos = new CharacterInfos();

                string[] selectedFiles = openFileDialog.FileNames;


                // 선택한 파일 경로 처리
                foreach (string filePath in selectedFiles)
                {
                    // 파일 경로를 사용하여 원하는 작업 수행
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var org_image = new BitmapImage(new Uri(filePath, UriKind.Absolute));

                    //AppLogger.Info()(fileName);
                    CharacterInfos.Items.Add(
                        new CharacterInfo
                        {
                            ImageName = fileName,
                            ImageSource = org_image
                        });
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ByteImage ref_image  = new ByteImage("D:\\test2\\17.bmp");
            ByteImage test_image = new ByteImage("D:\\test2\\17.bmp");
            
            var score = OCRManager.Instance.CompareCharacter(ref_image, test_image);

            AppLogger.Info()("5-5Score : [{0}]", score);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var test_image = new ByteImage("D:\\S.bmp");
            var result = OCRManager.Instance.BinProcessing(test_image, ModelName, CharacterPolarity.DarkOnLight, 80.0, CharacterTypes.Number);

            AppLogger.Info()(result.ToString());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.StateChanged -= Window_StateChanged;

            OCRManager.Instance.RemoveCompareCharacterEvent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.StateChanged += Window_StateChanged;

            OCRManager.Instance.RegisterSingleCompareCharacterEvent(OnSingleCompareCharacterFinished);
            OCRManager.Instance.RegisterMultiCompareCharacterEvent(OnMultiCompareCharacterFinished);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window.WindowState == WindowState.Minimized)
            {
                Console.WriteLine("부모 윈도우 최소화됨 → UserControl도 사실상 안보임");
            }
            else
            {
                Console.WriteLine("부모 윈도우 복원/최대화 → UserControl 다시 보임");
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                Console.WriteLine("UserControl 보이기 시작!");
                // 다시 보일 때 실행할 코드
            }
            else
            {
                Console.WriteLine("UserControl 숨김!");
                // 숨겨질 때 실행할 코드
            }
        }
        
        public void OnSingleCompareCharacterFinished(ByteImage TestImage, ByteImage RefImage, int compare_idx, string character, double score)
        {

        }

        public void OnMultiCompareCharacterFinished(ByteImage [] TestImages, ProcessInfo [] info_list)
        {
            TestInfos = new CharacterInfos();

            int i = 0;
            foreach (var info in info_list)
            {
                CharacterInfo charinfo = new CharacterInfo();

                charinfo.ImageName = string.Format("{0},{1:F2}", info.Character, info.Score * 100.0);
                charinfo.ImageSource = TestImages[i].ToBitmapImage();
                TestInfos.Items.Add(charinfo);

                i++;
            }
        }
    }
}
