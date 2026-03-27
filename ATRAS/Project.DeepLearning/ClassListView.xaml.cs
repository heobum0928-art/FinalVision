using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

using Project.BaseLib.Extension;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Services.Dialogs;
using Project.UI.Common;
using LiveCharts;
using LiveCharts.Wpf;
using System.Threading;
using Project.BaseLib.DataStructures;

namespace Project.DeepLearning.UI
{
    /// <summary>
    /// ClassListView.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class ClassListView : UserControl, INotifyPropertyChanged
    {
        #region fields
        public event PropertyChangedEventHandler PropertyChanged;

        public string _ClassName;
        public double _PredictConfidence;

        protected string _ModelName;

        protected bool IsChecked = false;


        protected bool use_gpu = true;

        #endregion

        #region propertise

        public bool Use_GPU
        {
            get
            {
                return use_gpu;
            }
        }

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> EpochLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }


        public SeriesCollection SeriesLossCollection { get; set; }
        public Func<double, string> YLossFormatter { get; set; }


        public string ModelName
        {
            get
            {

                return _ModelName;

                //var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                //return config.ModelName;
            }

            set
            {
                //var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                //config.ModelName = value;
                //config.Save();

                _ModelName = value;

                OnPropertyChanged();
            }
        }

        public int Epoch
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();

                
                return config.Epoch;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.Epoch = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public int BatchSize
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.BatchSize;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.BatchSize = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public int Patience
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.Patience;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.Patience = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public double LearningRatio
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.LearningRatio;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.LearningRatio = value;
                config.Save();

                OnPropertyChanged();
            }
        }

        public int ItemWidth
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.ItemWidth;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.ItemWidth = value;
                config.Save();

                OnPropertyChanged();
            }
        }
        public int ItemHeight
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.ItemHeight;
            }

            set
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                config.ItemHeight = value;
                config.Save();
                OnPropertyChanged();
            }
        }

        //public string ClassName
        //{
        //    get
        //    {
        //        return _ClassName;
        //    }

        //    set
        //    {
        //        _ClassName = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public double PredictConfidence
        //{
        //    get
        //    {
        //        return _PredictConfidence;
        //    }

        //    set
        //    {
        //        _PredictConfidence = value;
        //        OnPropertyChanged();
        //    }
        //}

        public DelegateCommand ModelSaveCommand { get; set; }

        public DelegateCommand ModelSaveAsCommand { get; set; }

        public DelegateCommand ModelLoadCommand { get; set; }

        public DelegateCommand ItemsSaveCommand { get; set; }

        public DelegateCommand AddClassCommand { get; set; }

        public DelegateCommand ModelTrainningCommand { get; set; }

        public DelegateCommand NextTrainningCommand { get; set; }


        public DelegateCommand MultiSampleLoadCommand { get; set; }



        public DelegateCommand<ImageClassListControl> ClassDeleteCommand { get; set; }
        public ObservableCollection<ImageClassListControl> ImageClassList
        {
            get
            {
                return DeepLearningManager.Instance.ImageClassList;
            }

            set
            {
                DeepLearningManager.Instance.ImageClassList = value;
                OnPropertyChanged();
            }
        }

        public string SelectedRecipe
        {
            get
            {
                var dl_config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                if(dl_config != null)
                {
                    return dl_config.RecipeName;
                }

                return "none";
            }
        }
        #endregion

        #region methods

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnModelSaveCommand()
        {
            var result = MessageBox.Show(
                "작업을 진행하시겠습니까?",
                "확인 요청", MessageBoxButton.OKCancel
            );

            if (result == MessageBoxResult.OK)
            {
                var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                if(recipe != null && config != null)
                {
                    recipe.ClassList.ClassName = DeepLearningManager.Instance.GetClassNames();
                    recipe.Save(config.RecipePath, config.RecipeName);
                }

                if(config != null)
                {
                    string model_name = config.RecipePath + "\\Recipe\\" + config.RecipeName + "\\" + config.RecipeName + ".keras";

                    DeepLearningManager.Instance.SaveModel(model_name);
                }
                
            }
        }

        protected void OnModelSaveAsCommand()
        {
            var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();

            var fr_dlg = new FolderRecipeWindow(config.RecipePath + "\\Recipe");
            var result = fr_dlg.ShowDialog();

            if(fr_dlg.Result == MessageBoxResult.OK)
            {
                string selectedPath = fr_dlg.FolderName;
                AppLogger.Info()("Model Export Path : [{0}].", selectedPath);

                ModelName = System.IO.Path.GetFileName(selectedPath);

                config.RecipeName = ModelName;

                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                recipe.ClassList.ClassName = DeepLearningManager.Instance.GetClassNames();
                recipe.Save(config.RecipePath, ModelName);
                config.Save();

                string model_name = config.RecipePath + "\\Recipe\\" + config.RecipeName + "\\" + config.RecipeName + ".keras";

                DeepLearningManager.Instance.SaveModel(model_name);
            }
        }

        protected void OnModelLoadCommand()
        {
            var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();

            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Model Load";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = config.RecipePath + "\\Recipe";

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dlg.FileName;
                AppLogger.Info()("Model Load Path : [{0}].", selectedPath);

                ModelName = System.IO.Path.GetFileName(selectedPath);
                                
                config.RecipeName = ModelName;
                config.Save();

                RemoveClassUIAll();

                RecipeLoad();
            }
        }

        protected async void OnModelTrainningCommand()
        {
            var dlg = new LoadingWindow(5 * 60);
            dlg.Show();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            var result = await DeepLearningManager.Instance.ModelTrainingAsync(true);

            dlg.Dispatcher.Invoke(() =>
            {
                dlg.Close();
            });

            AppLogger.Info()("New Training Time : [{0} ms]", sw.ElapsedMilliseconds);
        }
        
        protected async void OnNextTrainningCommand()
        {
            var dlg = new LoadingWindow(5 * 60);
            dlg.Show();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var result = await DeepLearningManager.Instance.ModelTrainingAsync(false);

            dlg.Dispatcher.Invoke(() =>
            {
                dlg.Close();
            });

            AppLogger.Info()("Next Training Time : [{0} ms]", sw.ElapsedMilliseconds);
        }

        protected void OnItemsSaveCommand()
        {
            if(ImageClassList == null || ImageClassList.Count == 0)
            {
                System.Windows.MessageBox.Show($"Image Class List은 하나 이상 있어야 합니다.");
                return;
            }

            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "폴더를 선택하세요.";
            dialog.UseDescriptionForTitle = true; // 설명을 제목에 표시
            dialog.ShowNewFolderButton = true;    // 새 폴더 만들기 버튼 보이기
            dialog.SelectedPath = @"D:\";

            if (dialog.ShowDialog() == true)
            {
                string folderPath = dialog.SelectedPath;

                var result = DeepLearningManager.Instance.ClassListSave(folderPath);
                //System.Windows.MessageBox.Show($"선택한 폴더: {folderPath}");
            }
        }
        
        //protected void OnSampleLoadCommand()
        //{
        //    //string path = "D:\\2_Projects\\Python_Projects\\teacherableMachine\\2.bmp";

        //    OpenFileDialog openFileDialog = new OpenFileDialog
        //    {
        //        Title = "파일 선택",
        //        Filter = "이미지 파일 (*.bmp;*.png)|*.bmp;*.png|모든 파일 (*.*)|*.*",




        //        Multiselect = false
        //    };

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        Stopwatch sw = new Stopwatch();
        //        sw.Start();

        //        string  selectedFile = openFileDialog.FileName;

        //        //if(selectedFiles.Length == 1)
        //        {
        //            AppLogger.Info()("Predict Sample Image : [{0}].", selectedFile);

        //            LoadImage(selectedFile);

        //            DeepLearningManager.Instance.Predict(selectedFile);
        //        }



                               
        //    }
        //}

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

                string [] selectedFiles = openFileDialog.FileNames;

                //if(selectedFiles.Length == 1)
                {
                    //AppLogger.Info()("Predict Sample Image : [{0}].", selectedFile);

                    //LoadImage(selectedFile);


                    List<ByteImage> images = new List<ByteImage>();

                    foreach(var file in selectedFiles)
                    {
                        ByteImage image = new ByteImage(file);
                        images.Add(image);
                    }



                    DeepLearningManager.Instance.MultiPredict(images.ToArray());
                    //DeepLearningManager.Instance.MultiPredict(selectedFiles);
                }




            }
        }

        protected void RemoveClassUIAll()
        {
            ImageClassList.Clear();
            class_pn.Children.Clear();
        }

        protected void OnClassDeleteCommand(ImageClassListControl control)
        {
            int k = 0;
            var item = ImageClassList.FirstOrDefault(f => f.ClassInfo.ClassName == control.ClassInfo.ClassName);
            if(item != null)
            {
                ImageClassList.Remove(item);
            }
            class_pn.Children.Remove(control);
        }

        protected void OnAddClassCommand()
        {
            var image_class = new ImageClassListControl(ItemWidth, ItemHeight);

            image_class.btn_class_delete.CommandParameter = image_class;
            image_class.btn_class_delete.Command = ClassDeleteCommand;

            ImageClassList.Add(image_class);
            class_pn.Children.Add(image_class);
        }

        private void ImageClassList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var image_class = new ImageClassListControl();

            //class_pn.Children.Add()
        }   

        public void SetClassConfidenceBarListClear()
        {
            ClassConfidenceBarList.Dispatcher.Invoke(() =>
            {
                ClassConfidenceBarList.Children.Clear();
            });
        }

        public void SetClassConfidenceBarList(List<PredictResult> list)
        {
            ClassConfidenceBarList.Dispatcher.Invoke(() =>
            {
                foreach(var item in list)
                {
                    var bar = new ConfidenceBar();
                    bar.ClassName = item.ClassName;
                    bar.Confidence = item.Confidence;
                    bar.ColorIndex = item.ClassIndex;

                    ClassConfidenceBarList.Children.Add(bar);
                }
            });
        }
        #endregion

        #region constructors
        public ClassListView()
        {
            InitializeComponent();

            DataContext = this;

            ModelSaveCommand = new DelegateCommand(OnModelSaveCommand);

            ModelSaveAsCommand = new DelegateCommand(OnModelSaveAsCommand);

            ModelLoadCommand = new DelegateCommand(OnModelLoadCommand);

            ModelTrainningCommand = new DelegateCommand(OnModelTrainningCommand);

            NextTrainningCommand = new DelegateCommand(OnNextTrainningCommand);

            AddClassCommand = new DelegateCommand(OnAddClassCommand);

            ItemsSaveCommand = new DelegateCommand(OnItemsSaveCommand);

            MultiSampleLoadCommand = new DelegateCommand(OnMultiSampleLoadCommand);

            ClassDeleteCommand = new DelegateCommand<ImageClassListControl>(OnClassDeleteCommand);

            ImageClassList = new ObservableCollection<ImageClassListControl>();

            ImageClassList.CollectionChanged += ImageClassList_CollectionChanged;

            _ClassName = "none";
            _PredictConfidence = 0.0;

            DeepLearningManager.Instance.Initialize();

            DeepLearningManager.Instance.SetClassListView(this);

            RecipeLoad();
        }

        public void SetAccuracyGraph(TrainingHistory history)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "acc",
                        Values = new ChartValues<double>(history.accuracy.ToArray()),
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 6
                    },
                    new LineSeries
                    {
                        Title = "val",
                        Values = new ChartValues<double>(history.val_accuracy.ToArray()),
                        PointGeometry = DefaultGeometries.Square,
                        PointGeometrySize = 6
                    }
                };

                var bestEpoch = history.best_epoch;

                // ✅ Best Epoch 점 강조 (빨간 점)
                if (bestEpoch >= 0 && bestEpoch < history.val_accuracy.Count)
                {
                    SeriesCollection.Add(new ScatterSeries
                    {
                        Title = "Best Epoch",
                        Values = new ChartValues<LiveCharts.Defaults.ObservablePoint>
                        {
                            new LiveCharts.Defaults.ObservablePoint(bestEpoch, history.val_accuracy[bestEpoch])
                        },
                        Fill = Brushes.Red,
                        MinPointShapeDiameter = 12,
                        MaxPointShapeDiameter = 12,
                        StrokeThickness = 0
                    });

                    // ✅ 수직선 강조
                    if (accuracyChart.AxisX.Count > 0)
                    {
                        accuracyChart.AxisX[0].Sections?.Clear();
                        accuracyChart.AxisX[0].Sections = new SectionsCollection
                        {
                            new AxisSection
                            {
                                Value = bestEpoch,
                                Stroke = Brushes.Red,
                                StrokeThickness = 2,
                                SectionWidth = 0.01
                            }
                        };
                    }
                }

                // Epoch 라벨 만들기
                EpochLabels = new List<string>();
                for (int i = 1; i <= history.accuracy.Count; i++)
                {
                    EpochLabels.Add(i.ToString());
                }

                YFormatter = value => value.ToString("P0");  // 0%, 50% 이런 식

                //DataContext = this;
                accuracyChart.Series = SeriesCollection;
            });


        }


        public void SetLossGraph(TrainingHistory history)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                SeriesLossCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "loss",
                        Values = new ChartValues<double>(history.loss.ToArray()),
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 6
                    },
                    new LineSeries
                    {
                        Title = "val loss",
                        Values = new ChartValues<double>(history.val_loss.ToArray()),
                        PointGeometry = DefaultGeometries.Square,
                        PointGeometrySize = 6
                    }
                };





                var bestEpoch = history.best_epoch;

                // ✅ Best Epoch 점 강조 (빨간 점)
                if (bestEpoch >= 0 && bestEpoch < history.val_loss.Count)
                {
                    SeriesLossCollection.Add(new ScatterSeries
                    {
                        Title = "Best Epoch",
                        Values = new ChartValues<LiveCharts.Defaults.ObservablePoint>
                        {
                            new LiveCharts.Defaults.ObservablePoint(bestEpoch, history.val_loss[bestEpoch])
                        },
                        Fill = Brushes.Red,
                        MinPointShapeDiameter = 12,
                        MaxPointShapeDiameter = 12,
                        StrokeThickness = 0
                    });

                    // ✅ 수직선 강조
                    if (lossChart.AxisX.Count > 0)
                    {
                        lossChart.AxisX[0].Sections?.Clear();
                        lossChart.AxisX[0].Sections = new SectionsCollection
                        {
                            new AxisSection
                            {
                                Value = bestEpoch,
                                Stroke = Brushes.Red,
                                StrokeThickness = 2,
                                SectionWidth = 0.01
                            }
                        };
                    }
                }



                // Epoch 라벨 만들기
                EpochLabels = new List<string>();
                for (int i = 1; i <= history.loss.Count; i++)
                {
                    EpochLabels.Add(i.ToString());
                }

                YLossFormatter = value => value.ToString("P0");  // 0%, 50% 이런 식

                //DataContext = this;
                lossChart.Series = SeriesLossCollection;
            });

        }

        protected void RecipeLoad()
        {
            var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
            if (config != null)
            {
                ModelName = config.RecipeName;

                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();


                //recipe.ClassList.ClassName.Add("Class1");
                //recipe.ClassList.ClassName.Add("Class2");
                //recipe.ClassList.ClassName.Add("Class3");
                //recipe.ClassList.ClassName.Add("Class4");



                //recipe.Save(config.RecipePath, ModelName);


                if (recipe != null)
                {
                    recipe.Load(config.RecipePath, ModelName);

                    foreach (var class_name in recipe.ClassList.ClassName)
                    {
                        var image_class = new ImageClassListControl(ItemWidth, ItemHeight);
                        image_class.ClassInfo.ClassName = class_name;
                        image_class.btn_class_delete.CommandParameter = image_class;
                        image_class.btn_class_delete.Command = ClassDeleteCommand;

                        ImageClassList.Add(image_class);
                        class_pn.Children.Add(image_class);
                    }

                    DeepLearningManager.Instance.LoadModel(config.RecipePath + "\\Recipe\\" + ModelName + "\\" + ModelName + ".keras");

                    OnPropertyChanged("Epoch");
                    OnPropertyChanged("BatchSize");
                    OnPropertyChanged("Patience");
                    OnPropertyChanged("LearningRatio");
                    OnPropertyChanged("ItemWidth");
                    OnPropertyChanged("ItemHeight");
                }
            }
        }




        #endregion

        private void Button_Extend_Click(object sender, RoutedEventArgs e)
        {
            if (IsChecked == true)
            {
                this.IsChecked = false;
                extend_btn.Content = "Extend Diabled";

                extend_panel.Width = new GridLength(1);
            }
            else
            {
                this.IsChecked = true;
                extend_btn.Content = "Extend Enabled";

                extend_panel.Width = new GridLength(400);
            }
        }

        private void RadioButton_CPU_Click(object sender, RoutedEventArgs e)
        {
            use_gpu = false;
        }

        private void RadioButton_GPU_Click(object sender, RoutedEventArgs e)
        {
            use_gpu = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
