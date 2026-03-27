using Microsoft.Win32;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// SingleSampleView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SingleSampleView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected string _ClassName;
        protected double _PredictConfidence;

        public DelegateCommand SampleLoadCommand { get; set; }


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

        public double PredictConfidence
        {
            get
            {
                return _PredictConfidence;
            }

            set
            {
                _PredictConfidence = value;
                OnPropertyChanged();
            }
        }

        public SingleSampleView()
        {
            InitializeComponent();

            DataContext = this;

            DeepLearningManager.Instance.SetSingleSampleView(this);

            SampleLoadCommand = new DelegateCommand(OnSampleLoadCommand);

            
        }
        private void LoadImage(string filePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // UI 쓰레드에서 안전하게 사용 가능하게 만듦

                MyImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 로드 실패: {ex.Message}");
            }
        }
        protected void OnSampleLoadCommand()
        {
            //string path = "D:\\2_Projects\\Python_Projects\\teacherableMachine\\2.bmp";

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "파일 선택",
                Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*",

                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                string selectedFile = openFileDialog.FileName;

                //if(selectedFiles.Length == 1)
                {
                    AppLogger.Info()("Predict Sample Image : [{0}].", selectedFile);

                    LoadImage(selectedFile);

                    DeepLearningManager.Instance.Predict(selectedFile);
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

        public void OnSinglePredictFinished(List<PredictResult> results)
        {
            DeepLearningManager.Instance.SetClassConfidenceBarListClear();
            AppLogger.Info()("====== Single Predict Finished =============");
            DeepLearningManager.Instance.SetClassConfidenceBarList(results);




            PredictResult ui_result = null;

            double max_confidence = 0.0;


            foreach (var result in results)
            {
                if (result.Confidence > max_confidence)
                {
                    max_confidence = result.Confidence;
                    ui_result = result;
                }

                AppLogger.Info()(result.ToString());
            }

            if(ui_result != null)
            {
                ClassName = ui_result.ClassName;
                PredictConfidence = ui_result.Confidence;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DeepLearningManager.Instance.SetSingleSampleView(this);
            DeepLearningManager.Instance.RegisterSinglePredictFinishedEvent(OnSinglePredictFinished);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DeepLearningManager.Instance.ClearSinglePredictFinishedEvent();
        }
    }
}
