using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.Extension;
using Python.Runtime;
using Microsoft.Win32;
using System.Windows.Media;
using Project.BaseLib.DataStructures;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using Newtonsoft.Json;
using System.Threading;

namespace Project.DeepLearning.UI
{
    public delegate void PredictFinished(List<PredictResult> results);

    public class TrainingHistory
    {
        public int best_epoch { get; set; }
        public int stopped_epoch { get; set; }
        public double acc_at_best { get; set; }
        public double val_acc_at_best { get; set; }
        public double loss_at_best { get; set; }
        public double val_loss_at_best { get; set; }

        public List<double> accuracy { get; set; }
        public List<double> val_accuracy { get; set; }
        public List<double> loss { get; set; }
        public List<double> val_loss { get; set; }
    }

    public class DeepLearningManager : NotifySingleton<DeepLearningManager>, IDisposable
    {
        #region fields

        protected event PredictFinished SinglePredictFinishedEvent = null;

        protected event PredictFinished MultiPredictFinishedEvent = null;

        public ObservableCollection<ImageClassListControl> ImageClassList { get; set; }

        protected dynamic _DLModule = null;

        protected dynamic _CalculatorClass = null;
        protected dynamic _CalculatorInstance = null;
        protected dynamic _CalculatorInstance2 = null;

        protected dynamic _TISClassificatorClass = null;
        protected dynamic _TISClassificatorInstance = null;


        ClassListView _ClassListView = null;

        SingleSampleView _SingleSampleView;
        MultiSampleView _MultiSampleView;

        protected object _lock_obj;

        protected Task _DeepLearningTask;

        protected DeepLearningSequence _Sequence;

        #endregion

        #region propertise
        public string PYTHON_VER { get; set; } = "3.10";
        
        public int DataSetWidth
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.ItemWidth;
            }
        }

        public int DataSetHeight
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.ItemHeight;
            }
        }

        public int Epoch
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();


                return config.Epoch;
            }
        }
        public int BatchSize
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.BatchSize;
            }
        }

        public int Patience
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.Patience;
            }
        }




        public double LearningRatio
        {
            get
            {
                var config = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                return config.LearningRatio;
            }
        }

        #endregion

        #region methods

        public void SetClassConfidenceBarListClear()
        {
            if(_ClassListView != null)
                _ClassListView.SetClassConfidenceBarListClear();
        }
        
        public void SetClassConfidenceBarList(List<PredictResult> list)
        {
            if (_ClassListView != null)
                _ClassListView.SetClassConfidenceBarList(list);
        }
        
        // Single Predict Finished Event
        public void RegisterSinglePredictFinishedEvent(PredictFinished SinglePredictFinishedEvent)
        {
            this.SinglePredictFinishedEvent += SinglePredictFinishedEvent;
        }

        public void OnSinglePredictFinshedEvent(List<PredictResult> results)
        {
            if (SinglePredictFinishedEvent != null)
                this.SinglePredictFinishedEvent(results);
        }

        public void ClearSinglePredictFinishedEvent()
        {
            SinglePredictFinishedEvent = null;
            _SingleSampleView = null;
        }
        
        // Multi Predict Finished Event
        public void RegisterMultiPredictFinishedEvent(PredictFinished MultiPredictFinishedEvent)
        {
            this.MultiPredictFinishedEvent += MultiPredictFinishedEvent;
        }

        public void ClearMultiPredictFinishedEvent(PredictFinished MultiPredictFinishedEvent = null)
        {
            if (MultiPredictFinishedEvent == null)
                this.MultiPredictFinishedEvent = null;
            else
                this.MultiPredictFinishedEvent -= MultiPredictFinishedEvent;

            _MultiSampleView = null;
        }

        public bool IsDPMultiPredictFinishedEventRegistered(PredictFinished handler)
        {
            if (MultiPredictFinishedEvent == null)
                return false;

            foreach (Delegate d in MultiPredictFinishedEvent.GetInvocationList())
            {
                if (d == (Delegate)handler)
                    return true;
            }
            return false;
        }

        public void OnMultiPredictFinshedEvent(List<PredictResult> results)
        {
            if (MultiPredictFinishedEvent != null)
                this.MultiPredictFinishedEvent(results);
        }
               
        public bool SetSequence(DeepLearningSequence seq)
        {
            lock(_lock_obj)
            {
                AppLogger.Info()($"Pre-Seq : [{_Sequence}], Current-Seq : [{seq}]");
                _Sequence = seq;

                return true;
            }
        }

        public DeepLearningSequence GetSequence()
        {
            lock(_lock_obj)
            {
                return _Sequence;
            }
        }

        public void SetClassListView(ClassListView view)
        {
            _ClassListView = view;
        }
        
        public void SetSingleSampleView(SingleSampleView view)
        {
            _SingleSampleView = view;
        }

        public void SetMultiSampleView(MultiSampleView view)
        {
            _MultiSampleView = view;
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

        public Dictionary<string, byte[][]> GetClassBytesData()
        {
            Dictionary<string, byte[][]> dic = new Dictionary<string, byte[][]>();

            foreach(var ImageClass in ImageClassList)
            {
                var class_name = ImageClass.ClassInfo.ClassName;
                var array = new byte[ImageClass.ClassInfo.ImageItems.Count][];

                int i = 0; 

                foreach(var item in ImageClass.ClassInfo.ImageItems)
                {
                    array[i] = item.ImageSource.GetPixelBytes();
                    i++;
                }

                dic.Add(class_name, array);
            }
            return dic;
        }

        public Dictionary<string, byte[][]> GetClassResizeBytesData()
        {
            Dictionary<string, byte[][]> dic = new Dictionary<string, byte[][]>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var ImageClass in ImageClassList)
                {
                    var class_name = ImageClass.ClassInfo.ClassName;
                    var array = new byte[ImageClass.ClassInfo.ImageItems.Count][];

                    int i = 0;

                    foreach (var item in ImageClass.ClassInfo.ImageItems)
                    {
                        byte[] resize_data = null;

                        if (item.ImageSource.PixelWidth == DataSetWidth &&
                            item.ImageSource.PixelHeight == DataSetHeight)
                        {
                            resize_data = item.ImageSource.GetPixelBytes();
                        }
                        else
                        {
                            ByteImage byte_image = new ByteImage((int)item.ImageSource.PixelWidth, (int)item.ImageSource.PixelHeight, (int)item.ImageSource.PixelWidth, item.ImageSource.GetPixelBytes(), 0);


                            var resize_image = byte_image.Resize(DataSetWidth, DataSetHeight, BaseLib.Enums.ResizeTypes.Gray);
                            resize_data = resize_image.Data;
                        }

                        array[i] = resize_data;
                        i++;
                    }

                    dic.Add(class_name, array);
                }
            });


            return dic;
        }

        public Dictionary<string, byte[][]> GetClassResizeBytesData2()
        {
            Dictionary<string, byte[][]> dic = new Dictionary<string, byte[][]>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var ImageClass in ImageClassList)
                {
                    var class_name = ImageClass.ClassInfo.ClassName;
                    var array = new byte[ImageClass.ClassInfo.ImageItems.Count][];

                    int i = 0;

                    foreach (var item in ImageClass.ClassInfo.ImageItems)
                    {
                        BitmapImage resize_image = null;
                        if (item.ImageSource.PixelWidth == DataSetWidth &&
                            item.ImageSource.PixelHeight == DataSetHeight)
                        {
                            resize_image = item.ImageSource;
                        }
                        else
                        {
                            resize_image = CenterImage(item.ImageSource, DataSetWidth, DataSetHeight);
                        }

                        array[i] = resize_image.GetPixelBytes();
                        i++;


                        //array[i] = item.ImageSource.GetPixelBytes();
                        //i++;
                    }

                    dic.Add(class_name, array);
                }
            });


            return dic;
        }
               
        public Dictionary<string, List<byte[]>> GetClassListByteData()
        {
            Dictionary<string, List<byte[]>> dic = new Dictionary<string, List<byte[]>>();

            foreach (var ImageClass in ImageClassList)
            {
                var class_name = ImageClass.ClassInfo.ClassName;

                List<byte[]> list = new List<byte[]>();
                foreach (var item in ImageClass.ClassInfo.ImageItems)
                {
                    var datas = item.ImageSource.GetPixelBytes();
                    list.Add(datas);
                }
                dic.Add(class_name, list);
            }
            return dic;
        }

        public Dictionary<string, byte[][]> GetClassTransformBytesData()
        {
            if (ImageClassList == null || ImageClassList.Count == 0)
            {
                AppLogger.Error()($"Image Item List Control is Empty.");
                return null;
            }

            var resize_width = DataSetWidth;
            var resize_height = DataSetHeight;

            Dictionary<string, byte[][]> dic = new Dictionary<string, byte[][]>();

            foreach (var ImageClass in ImageClassList)
            {
                var class_name = ImageClass.ClassInfo.ClassName;
                var array = new byte[ImageClass.ClassInfo.ImageItems.Count][];

                int i = 0;

                foreach (var item in ImageClass.ClassInfo.ImageItems)
                {
                    int img_width = (int)item.ImageSource.PixelWidth;
                    int img_height = (int)item.ImageSource.PixelHeight;
                    int bitsPerPixel = item.ImageSource.Format.BitsPerPixel;
                    int bytesPerPixel = (bitsPerPixel + 7) / 8; // 3
                    int img_pitch = img_width * bytesPerPixel; // 640 * 3 = 1920




                    if (img_width > resize_width || img_height > resize_height)
                    {
                        AppLogger.Debug()($"Item Size is too small. item size : [{img_width}]/[{img_height}], resize image : [{resize_width}]/[{resize_height}]");
                        continue;
                    }

                    int offset_x = (resize_width - img_width) / 2;
                    int offset_y = (resize_height - img_height) / 2;

                    RoiRectangle roi = new RoiRectangle(offset_y, offset_x, offset_y + img_height, offset_x + img_width);

                    ByteImage resize_image = new ByteImage(resize_width, resize_height);
                    
                    resize_image.InsertChildBuffer(roi, item.ImageSource.GetPixelBytes());
                    
                    array[i] = resize_image.Data;// item.ImageSource.GetPixelBytes();
                    i++;
                }

                dic.Add(class_name, array);
            }
            return dic;
        }

        public async Task<bool> ClassListSaveAsync(string folderpath)
        {
            return await Task.Run(() =>
            {
                return ClassListSave(folderpath);
            });
        }

        public bool ClassListSave(string folderpath)
        {
            if (ImageClassList == null || ImageClassList.Count == 0)
            {
                AppLogger.Error()($"Image Item List Control is Empty.");
                return false;
            }
            var trans_bytes = GetClassTransformBytesData();

            if (!Directory.Exists(folderpath))
                Directory.CreateDirectory(folderpath);

            foreach(var class_name in trans_bytes.Keys)
            {
                int count = trans_bytes[class_name].GetLength(0);

                if(count == 0)
                {
                    AppLogger.Error()($"Image List is Empty.");
                    continue;
                }

                var class_path = folderpath + "\\" + class_name;

                if (!Directory.Exists(class_path))
                    Directory.CreateDirectory(class_path);

                for (int i = 0; i < count; i++)
                {
                    ByteImage save_image = new ByteImage(DataSetWidth, DataSetHeight, DataSetWidth, trans_bytes[class_name][i], 0);

                    save_image.Save(class_path + "\\" + i + ".bmp");
                }
            }
            return true;
        }

        public List<string> GetClassNames()
        {
            List<string> list = new List<string>();

            foreach (var ImageClass in ImageClassList)
            {
                var class_name = ImageClass.ClassInfo.ClassName;
                list.Add(class_name);
            }

            return list;
        }

        public static string GetPythonInstallPath(string version = "3.11")
        {
            string registryKeyPath = $@"Software\Python\PythonCore\{version}\InstallPath";

            // 먼저 CurrentUser 확인
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue("");
                    if (value != null)
                        return value.ToString();
                }
            }

            // 다음으로 LocalMachine 확인 (관리자 권한 설치일 경우)
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue("");
                    if (value != null)
                        return value.ToString();
                }
            }

            return null;
        }

        public void Initialize()
        {
            var path = GetPythonInstallPath("3.10");
        }

        public void Dispose()
        {
            if (PythonEngine.IsInitialized)
            {
                PythonEngine.Shutdown();
            }
        }

        public void Test()
        {
            using (Py.GIL())
            {
                if (_CalculatorClass != null)
                {
                    // Calculator 클래스의 인스턴스 생성

                    // add123 함수 호출
                    int num1Add = 10;
                    int num2Add = 5;
                    dynamic resultAdd = _CalculatorInstance.add123(num1Add, num2Add);

                    // multiply123 함수 호출
                    int num1Multiply = 7;
                    int num2Multiply = 3;
                    dynamic resultMultiply = _CalculatorInstance.multiply123(num1Multiply, num2Multiply);
                    
                    byte[] byteArrayToSend = { 0x01, 0x02, 0x03, 0x04, 0xFF };

                    dynamic builtins = Py.Import("builtins");
                    PyObject pyBytes2 = builtins.bytes(byteArrayToSend);

                    using (PyObject pyBytes = builtins.bytes(byteArrayToSend))
                    {
                        dynamic result = _CalculatorInstance.process_bytes(pyBytes);
                    }
                }
                else
                {
                    //MessageBox.Show("Python 클래스 'Calculator'를 찾을 수 없습니다.");
                }

                _CalculatorInstance2 = _CalculatorClass();
                if (_CalculatorInstance2 != null)
                {
                    int num1Add = 10;
                    int num2Add = 5;
                    dynamic resultAdd = _CalculatorInstance2.add123(num1Add, num2Add);
                }
            }
        }

        public string GetName()
        {
            using(Py.GIL())
            {
                if (_TISClassificatorInstance != null)
                {
                    return _TISClassificatorInstance.GetName();
                }

                return "";
            }
        }

        public bool SetDataSet()
        {
            return SetSequence(DeepLearningSequence.SetDataSet);
        }

        public bool PySetDataSet()
        {
            var classbytesdata = GetClassResizeBytesData();

            List<byte[]> input = new List<byte[]>();
            List<byte[]> target = new List<byte[]>();
            int j = 0;
            foreach (var datas in classbytesdata.Values)
            {
                byte[][] byteArray = new byte[datas.Length][];

                // 각 행을 byte[1] 배열로 초기화하고 첫 번째 요소에 1 할당
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i] = new byte[1]; // 각 행에 크기가 1인 byte 배열 할당
                    byteArray[i][0] = (byte)j;       // 해당 배열의 첫 번째 요소에 1 할당
                }

                target.AddRange(byteArray);

                input.AddRange(datas);

                j++;
            }

            //using (Py.GIL())
            if(_TISClassificatorInstance != null)
            {
                dynamic result2 = _TISClassificatorInstance.SetDataSet(DataSetWidth, DataSetHeight, input, target);
                
                string[] header = { "train_scaled", "val_scaled", "test_scaled", "train_target", "val_target", "test_target" };
                
                int index = 0;
                foreach (PyObject shape in result2)
                {
                    var shapeTuple = shape.As<PyTuple>();  // 각 shape는 tuple
                    string data = header[index] + " Shape: (";
                    index++;

                    foreach(var st in shapeTuple)
                    {
                        int dim0 = st.As<int>();
                        data += string.Format($"{dim0},");
                    }
                    data= data.Remove(data.Length - 1, 1);
                    data += ")";
                    AppLogger.Info()(data);

                }
            }

            return true;
        }

        protected bool new_training;

        protected TrainingHistory PyTraining()
        {
            var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
            if (config != null)
            {
                var model_path = config.RecipePath + "\\Recipe\\" + config.RecipeName + "\\" + config.RecipeName + ".keras";
                if (File.Exists(model_path))
                    File.Delete(model_path);

                using (Py.GIL())
                {
                    if (_TISClassificatorInstance != null)
                    {
                        string log_path = string.Format("training_log_[{0}{1}{2}_{3}{4}{5}].csv", 
                            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                        PyObject pyTrue = PyObject.FromManagedObject(true);

                        bool use_gpu = true;
                        if (_ClassListView != null)
                            use_gpu = _ClassListView.Use_GPU;

                        var result = _TISClassificatorInstance.training(Epoch, BatchSize, LearningRatio, Patience, model_path, log_path/*"training_log.csv"*/, new_training, use_gpu);

                        string jsonHistory = result.ToString();

                        var history = JsonConvert.DeserializeObject<TrainingHistory>(jsonHistory);

                        // 결과 출력
                        for (int i = 0; i < history.accuracy.Count; i++)
                        {
                            AppLogger.Info()($"Epoch {i + 1}: " +
                                              $"acc={history.accuracy[i]:F4}, " +
                                              $"val_acc={history.val_accuracy[i]:F4}, " +
                                              $"loss={history.loss[i]:F4}, " +
                                              $"val_loss={history.val_loss[i]:F4}");
                        }
                        var acc = _TISClassificatorInstance.evaluate(use_gpu);
                        AppLogger.Info()("evaluate : [{0}]%", acc);

                        return history;
                    }
                    return null;
                }
            }
            return null;
        }

        public bool Training(bool new_training = true)
        {
            this.new_training = new_training;

            return SetSequence(DeepLearningSequence.Training);
        }

        public async Task<bool> ModelTrainingAsync(bool new_training = true)
        {
            return await Task.Run(() =>
            {
                SetDataSet();

                while(true)
                {
                    if (GetSequence() == DeepLearningSequence.Idle)
                    {
                        AppLogger.Info()("ModelTrainingAsync SetDataSet() Finished.");
                        break;
                    }

                    Thread.Sleep(1);
                }

                Training(new_training);

                while (true)
                {
                    if (GetSequence() == DeepLearningSequence.Idle)
                    {
                        AppLogger.Info()("ModelTrainingAsync Training() Finished.");
                        break;
                    }

                    Thread.Sleep(1);
                }

                return true;
            });
        }

        public async Task<TrainingHistory> RunTrainingAsync()
        {
            TrainingHistory history = null;

            using (Py.GIL())
            {
                await Task.Run(() =>
                {
                    if (_TISClassificatorInstance != null)
                    {
                        var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                        var modelPath = Path.Combine(config.RecipePath, "Recipe", config.RecipeName, config.RecipeName + ".keras");

                        if (File.Exists(modelPath))
                            File.Delete(modelPath);

                        var result = _TISClassificatorInstance.training(Epoch, BatchSize, LearningRatio, Patience, modelPath);
                        string jsonHistory = result.ToString();

                        history = JsonConvert.DeserializeObject<TrainingHistory>(jsonHistory);

                        // 로그 출력
                        for (int i = 0; i < history.accuracy.Count; i++)
                        {
                            AppLogger.Info()($"Epoch {i + 1}: acc={history.accuracy[i]:F4}, val_acc={history.val_accuracy[i]:F4}");
                        }

                        var acc = _TISClassificatorInstance.evaluate();
                        AppLogger.Info()("evaluate : [{0}]%", acc);
                    }
                });
            }




            return history;
        }

        public bool Evaluate()
        {
            using (Py.GIL())
            {
                if (_TISClassificatorInstance != null)
                {
                    var acc = _TISClassificatorInstance.evaluate(_ClassListView.Use_GPU);
                    AppLogger.Info()("evaluate : [{0}]%", acc);
                    
                    return true;
                }

                return false;
            }
        }

        protected string save_model_path;

        public bool PySaveModel()
        {
            if (_TISClassificatorInstance != null)
            {
                var result = _TISClassificatorInstance.save_model(save_model_path);

                AppLogger.Info()("Model Load : [{0}].", save_model_path);
                return result;
            }
            return false;
        }

        public bool SaveModel(string path)
        {
            save_model_path = path;
            return SetSequence(DeepLearningSequence.RecipeSave);
        }
               
        protected string load_model_path;

        public bool PyLoadModel()
        {
            if (_TISClassificatorInstance != null)
            {
                var result = _TISClassificatorInstance.load_model(DataSetWidth, DataSetHeight, load_model_path);

                AppLogger.Info()("Model[{0} x {1}] Load : [{2}].", DataSetWidth, DataSetHeight, load_model_path);
                return result;
            }
            return false;
        }

        public void LoadModel(string path = "")
        {
            if(path == "")
            {
                var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                var model_name = config.RecipeName;

                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                recipe.Load(config.RecipePath, model_name);

                path = config.RecipePath + "\\Recipe\\" + model_name + "\\" + model_name + ".keras";
            }
            load_model_path = path;

            SetSequence(DeepLearningSequence.RecipeLoad);
        }

        protected string    sample_path = null;
        protected ByteImage predict_image = null;

        protected List<PredictResult> PyPredict()
        {
            List<PredictResult> list = new List<PredictResult>();
            try
            {
                int width = DataSetWidth;   // 250
                int height = DataSetHeight; // 250

                ByteImage SampleImage = null;
                ByteImage test_image = null;

                if (sample_path != null)
                {
                    SampleImage = new ByteImage(sample_path);
                    test_image = SampleImage.Resize(width, height, BaseLib.Enums.ResizeTypes.Gray);
                }

                if(predict_image != null)
                {
                    test_image = predict_image.Resize(width, height, BaseLib.Enums.ResizeTypes.Gray);
                }

                var imageBytes = test_image.Data;

                bool use_gpu = true;
                if (_ClassListView != null)
                    use_gpu = _ClassListView.Use_GPU;

                var predict_info = _TISClassificatorInstance.Predict(imageBytes, use_gpu);

                int predictedClass = predict_info[0].As<int>();
                double confidence = predict_info[1].As<double>();

                // 배열 파싱
                // Python에서 numpy를 import
                PyObject npArray = predict_info[2];
                dynamic np = Py.Import("numpy");

                // ndarray를 Python list로 변환
                PyObject listObj = npArray.InvokeMethod("tolist");

                // 2D 리스트라면 PyList 안에 PyList가 들어 있음
                var outerList = new PyList(listObj);

                List<double> softmaxValues = new List<double>();
                foreach (PyObject inner in outerList)
                {
                    var innerList = new PyList(inner);
                    foreach (PyObject val in innerList)
                    {
                        softmaxValues.Add(Math.Round(val.As<double>(), 4));
                    }
                }

                AppLogger.Info()("Class : [{0}], Confidence : [{1}], SoftMaxValues : [{2}].",
                                    predictedClass, confidence, string.Join(" / ", softmaxValues));

                var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                recipe.Load(config.RecipePath, config.RecipeName);
                var class_names = recipe.ClassList.ClassName.ToArray();

                for (int i = 0; i < class_names.Length; i++)
                {
                    list.Add(new PredictResult(class_names[i], i, softmaxValues[i] * 100.0));
                }

                OnSinglePredictFinshedEvent(list);
            }
            catch (Exception ex)
            {
                AppLogger.Error()("Predict Error : [{0}].", ex.Message);
            }

            return list;
        }

        protected string[]      samples_path = null;
        protected ByteImage[]   predict_images = null;

        protected List<PredictResult> PyMultiPredict()
        {
            List<PredictResult> list = new List<PredictResult>();

            List<byte[]> input = new List<byte[]>();

            try
            {
                int width = DataSetWidth;   // 250
                int height = DataSetHeight; // 250
                
                if(samples_path != null)
                {
                    foreach(var sample_path in samples_path)
                    {
                        ByteImage SampleImage = new ByteImage(sample_path);
                        ByteImage test_image = SampleImage.Resize(width, height, BaseLib.Enums.ResizeTypes.Gray);

                        var imageBytes = test_image.Data;

                        input.Add(imageBytes);
                    }
                }

                if(predict_images != null)
                {
                    foreach(var image in predict_images)
                    {
                        ByteImage test_image = image.Resize(width, height, BaseLib.Enums.ResizeTypes.Gray);

                        var imageBytes = test_image.Data;

                        input.Add(imageBytes);
                    }
                }

                dynamic pyList = new PyList();
                dynamic np = Py.Import("numpy");

                foreach (var imgBytes in input)
                {
                    // C# byte[] → Python numpy array (uint8)
                    PyObject npArray = np.frombuffer(imgBytes, dtype: np.uint8);
                    pyList.Append(npArray);
                }

                bool use_gpu = true;
                if (_ClassListView != null)
                    use_gpu = _ClassListView.Use_GPU;

                var predict_info = _TISClassificatorInstance.MultiPredict(pyList, use_gpu);

                var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                recipe.Load(config.RecipePath, config.RecipeName);                
                var class_names = recipe.ClassList.ClassName.ToArray();
                
                int idx = 0;
                foreach (var pred in predict_info)
                {
                    int classId = (int)pred["class"];
                    double confidence = (double)pred["confidence"];                                      

                    var ClassName = class_names[classId];
                    var PredictConfidence = confidence * 100.0;

                    PredictResult result = new PredictResult(ClassName, idx++, PredictConfidence);
                    list.Add(result);

                    AppLogger.Info()($"Class: {ClassName}, Confidence: {PredictConfidence}");
                }

                OnMultiPredictFinshedEvent(list);
            }
            catch (Exception ex)
            {
                AppLogger.Error()("Predict Error : [{0}].", ex.Message);
            }

            return list;
        }

        protected List<PredictResult> PyPredict2()
        {
            List<PredictResult> list = new List<PredictResult>();
            try
            {
                using (var original = new System.Drawing.Bitmap(sample_path))
                {
                    int width = DataSetWidth;   // 250
                    int height = DataSetHeight; // 250

                    using (var padded = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                    {
                        // 1. 배경 검정으로 초기화 + 원본 이미지 중앙 배치
                        using (var g = System.Drawing.Graphics.FromImage(padded))
                        {
                            g.Clear(System.Drawing.Color.Black); // 배경 0 (검정)

                            int offsetX = (width - original.Width) / 2;
                            int offsetY = (height - original.Height) / 2;

                            g.DrawImage(original, offsetX, offsetY, original.Width, original.Height);
                        }

                        // 2. 흑백으로 변환 후 픽셀 추출
                        byte[] imageBytes = new byte[width * height];
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                System.Drawing.Color pixel = padded.GetPixel(x, y);

                                // Grayscale로 변환: 일반적으로 Y = 0.299R + 0.587G + 0.114B
                                byte gray = (byte)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);

                                imageBytes[y * width + x] = gray;
                            }
                        }

                        bool use_gpu = true;
                        if (_ClassListView != null)
                            use_gpu = _ClassListView.Use_GPU;
                            
                        var predict_info = _TISClassificatorInstance.Predict(imageBytes, use_gpu);

                        int predictedClass = predict_info[0].As<int>();
                        double confidence = predict_info[1].As<double>();

                        // 배열 파싱
                        // Python에서 numpy를 import
                        PyObject npArray = predict_info[2];
                        dynamic np = Py.Import("numpy");

                        // ndarray를 Python list로 변환
                        PyObject listObj = npArray.InvokeMethod("tolist");

                        // 2D 리스트라면 PyList 안에 PyList가 들어 있음
                        var outerList = new PyList(listObj);

                        List<double> softmaxValues = new List<double>();
                        foreach (PyObject inner in outerList)
                        {
                            var innerList = new PyList(inner);
                            foreach (PyObject val in innerList)
                            {
                                softmaxValues.Add(Math.Round(val.As<double>(), 4));
                            }
                        }

                        AppLogger.Info()("Class : [{0}], Confidence : [{1}], SoftMaxValues : [{2}].",
                                            predictedClass, confidence, string.Join(" / ", softmaxValues));

                        var config = ConfigManager.Instance.GetConfiguration<DLSystemConfiguration>();
                        var recipe = ConfigManager.Instance.GetConfiguration<DeepLearningConfiguration>();
                        recipe.Load(config.RecipePath, config.RecipeName);
                        var class_names = recipe.ClassList.ClassName.ToArray();

                        if(_SingleSampleView != null)
                        {
                            _SingleSampleView.ClassName = class_names[predictedClass];
                            _SingleSampleView.PredictConfidence = confidence * 100.0;
                        }

                        if (class_names.Length != softmaxValues.Count)
                            return list;

                        for (int i = 0; i < class_names.Length; i++)
                        {
                            list.Add(new PredictResult(class_names[i], i, softmaxValues[i] * 100.0));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error()("Predict Error : [{0}].", ex.Message);
            }

            return list;
        }

        public bool Predict(string sample_path)
        {
            this.sample_path = sample_path;
            this.predict_image = null;
            return SetSequence(DeepLearningSequence.Predict);
        }

        public bool Predict(ByteImage image)
        {
            this.sample_path = null;
            this.predict_image = image;

            return SetSequence(DeepLearningSequence.Predict);
        }
        
        public bool MultiPredict(string [] samples_path)
        {
            this.samples_path = samples_path;
            this.predict_images = null;

            return SetSequence(DeepLearningSequence.MultiPredict);
        }

        public bool MultiPredict(ByteImage [] images)
        {
            this.samples_path = null;
            this.predict_images = images;

            return SetSequence(DeepLearningSequence.MultiPredict);
        }

        #endregion

        #region constructors
        protected DeepLearningManager()
        {
            _lock_obj = new object();

            _Sequence = DeepLearningSequence.Idle;

            _DeepLearningTask = new Task(new Action(DeepLearningFunction));
            _DeepLearningTask.Start();

        }
        #endregion

        protected void DeepLearningFunction()
        {
            var path = GetPythonInstallPath(PYTHON_VER);

            var pythonpath = path + "\\" + string.Format("python{0}.dll", "39");

            pythonpath = @"C:\Users\tech\AppData\Local\Programs\Python\Python39\python39.dll"; // 이 경로의 Python 환경 확인
            
            //pythonpath = @"C:\Users\user\AppData\Local\Programs\Python\Python39\python39.dll"; // 이 경로의 Python 환경 확인

            Runtime.PythonDLL = pythonpath;

            PythonEngine.Initialize();


            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");

                _DLModule = Py.Import("DLModule");

                // ⬇️ 리다이렉션 함수 호출
                _DLModule.redirect_stdout_to(new Action<string>(msg =>
                {
                    if (!string.IsNullOrEmpty(msg) && msg[0] == '\n')
                    {
                        msg = msg.Substring(1);
                    }


                    AppLogger.Info()("[Python Log] : " + msg);
                }));

                if (_DLModule != null)
                {
                    _TISClassificatorClass = _DLModule.TISClassificator;
                    if (_TISClassificatorClass != null)
                    {
                        // TISClassificator 클래스의 인스턴스 생성
                        _TISClassificatorInstance = _TISClassificatorClass("TISClass");
                    }
                    else
                    {
                    }
                }

                AppLogger.Info()("DeepLearning Task Started!");

                while (true)
                {
                    switch(_Sequence)
                    {
                        case DeepLearningSequence.Idle:

                            Thread.Sleep(10);
                            break;

                        case DeepLearningSequence.RecipeLoad:

                            PyLoadModel();

                            SetSequence(DeepLearningSequence.Idle);
                            break;

                        case DeepLearningSequence.RecipeSave:

                            PySaveModel();

                            SetSequence(DeepLearningSequence.Idle);
                            break;

                        case DeepLearningSequence.SetDataSet:
                            PySetDataSet();

                            SetSequence(DeepLearningSequence.Idle);
                            break;

                        case DeepLearningSequence.Training:

                            var history = PyTraining();

                            if (_ClassListView != null)
                                _ClassListView.SetAccuracyGraph(history);

                            Thread.Sleep(100);

                            if (_ClassListView != null)
                                _ClassListView.SetLossGraph(history);


                            SetSequence(DeepLearningSequence.Idle);
                            break;

                        case DeepLearningSequence.Predict:

                            var list = PyPredict();

                            SetSequence(DeepLearningSequence.Idle);
                            break;

                        case DeepLearningSequence.MultiPredict:

                            if(_ClassListView != null)
                                _ClassListView.SetClassConfidenceBarListClear();

                            var multilist = PyMultiPredict();

                            SetSequence(DeepLearningSequence.Idle);
                            break;
                    }
                    Thread.Sleep(10);
                }
            }
        }
    }
}
