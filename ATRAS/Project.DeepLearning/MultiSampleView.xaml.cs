using Microsoft.Win32;
using Project.BaseLib.DataStructures;
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
    /// MultiSampleView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MultiSampleView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected string _MultiPredictResult = string.Empty;

        public DelegateCommand MultiSampleLoadCommand { get; set; }
        
        public string MultiPredictResult
        {
            get
            {
                return _MultiPredictResult;
            }
            set
            {
                _MultiPredictResult = value;
                OnPropertyChanged();
            }
        }
        public MultiSampleView()
        {
            InitializeComponent();

            DataContext = this;

            MultiSampleLoadCommand = new DelegateCommand(OnMultiSampleLoadCommand);

            DeepLearningManager.Instance.SetMultiSampleView(this);            
        }


        public void OnPredictFinished(List<PredictResult> results)
        {

            DeepLearningManager.Instance.SetClassConfidenceBarListClear();
            AppLogger.Info()("====== Multi-Predict Finished =============");
            DeepLearningManager.Instance.SetClassConfidenceBarList(results);

            MultiPredictResult = string.Empty;

            foreach (var result in results)
            {
                MultiPredictResult += result.ClassName;
                AppLogger.Info()(result.ToString());
            }
        }

        protected void OnMultiSampleLoadCommand()
        {
            //string path = "D:\\2_Projects\\Python_Projects\\teacherableMachine\\2.bmp";

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "파일 선택",
                Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*",

                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                string[] selectedFiles = openFileDialog.FileNames;

                //if(selectedFiles.Length == 1)
                {
                    //AppLogger.Info()("Predict Sample Image : [{0}].", selectedFile);

                    //LoadImage(selectedFile);


                    List<ByteImage> images = new List<ByteImage>();

                    foreach (var file in selectedFiles)
                    {
                        ByteImage image = new ByteImage(file);
                        images.Add(image);
                    }



                    DeepLearningManager.Instance.MultiPredict(images.ToArray());
                    //DeepLearningManager.Instance.MultiPredict(selectedFiles);
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DeepLearningManager.Instance.SetMultiSampleView(this);

            var registered = DeepLearningManager.Instance.IsDPMultiPredictFinishedEventRegistered(OnPredictFinished);


            if(registered != true)
                DeepLearningManager.Instance.RegisterMultiPredictFinishedEvent(OnPredictFinished);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DeepLearningManager.Instance.ClearMultiPredictFinishedEvent(OnPredictFinished);
        }
    }
}
