using Project.BaseLib.DataStructures;
using Project.BaseLib.Extension;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Project.OCR.UI
{
    public delegate void SingleCompareCharacterFinished(ByteImage TestImage, ByteImage RefImage, int compare_idx, string character, double score);

    public delegate void MultiCompareCharacterFinished(ByteImage [] TestImages, ProcessInfo [] info_list);

    public class OCRManager : NotifySingleton<OCRManager>
    {
        #region fields
        private event SingleCompareCharacterFinished SingleCharacterCompareFinishedEvent = null;

        private event MultiCompareCharacterFinished MultiCharacterCompareFinishedEvent = null;

        private const string FONTS = "Fonts";

        private List<CharacterInfo> _CharacterInfoList;
        #endregion

        #region propertise
        public List<CharacterInfo> CharacterInfoList
        {
            get
            {
                return _CharacterInfoList;
            }
        }

        public Dictionary<string, Dictionary<string, List<CharacterInfo>>> _dic_font_infos;

        #endregion

        #region methods
        public void RemoveCompareCharacterEvent()
        {
            SingleCharacterCompareFinishedEvent = null;
            MultiCharacterCompareFinishedEvent = null;
        }

        public void RegisterSingleCompareCharacterEvent(SingleCompareCharacterFinished SingleCharacterCompareFinishedEvent)
        {
            this.SingleCharacterCompareFinishedEvent += SingleCharacterCompareFinishedEvent;
        }

        private void OnSingleCompareCharacterEvent(ByteImage TestImage, ByteImage RefImage, int compare_index, string character, double score)
        {
            if (SingleCharacterCompareFinishedEvent != null)
                SingleCharacterCompareFinishedEvent(TestImage, RefImage, compare_index, character, score);
        }

        public void RegisterMultiCompareCharacterEvent(MultiCompareCharacterFinished MultiCharacterCompareFinishedEvent)
        {
            this.MultiCharacterCompareFinishedEvent += MultiCharacterCompareFinishedEvent;
        }

        private void OnMultiCompareCharacterEvent(ByteImage [] TestImages, ProcessInfo [] infos)
        {
            if (MultiCharacterCompareFinishedEvent != null)
                MultiCharacterCompareFinishedEvent(TestImages, infos);
        }
        
        public bool SetFontList(string font_name, List<CharacterInfo> charlist)
        {
            if (charlist == null || charlist.Count == 0)
                return false;

            if (_dic_font_infos == null)
                _dic_font_infos = new Dictionary<string, Dictionary<string, List<CharacterInfo>>>();

            _dic_font_infos[font_name] = new Dictionary<string, List<CharacterInfo>>();

            foreach(var c in charlist)
            {
                var charnames = c.ImageName.Split(',');
                if (charnames.Length != 2)
                    continue;

                var character = charnames[0];
                var idx = charnames[1];

                CharacterInfo info = new CharacterInfo
                {
                    ImageName = idx,
                    ImageSource = c.ImageSource
                };

                if (!_dic_font_infos[font_name].ContainsKey(character))
                    _dic_font_infos[font_name][character] = new List<CharacterInfo>();

                _dic_font_infos[font_name][character].Add(info);
            }

            return true;
        }

        public bool LoadAllFont(string path = "")
        {
            if(path == "")
            {
                var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
                path = config.RecipePath;
            }

            var folderPath = path + "\\Recipe";// + font_name + "\\" + FONTS;

            if (!Directory.Exists(folderPath))
                return false;

            string[] directories = Directory.GetDirectories(folderPath);


            if (_dic_font_infos == null)
                _dic_font_infos = new Dictionary<string, Dictionary<string, List<CharacterInfo>>>();

            foreach (string recipe_dir in directories)
            {
                string recipe_name = Path.GetFileName(recipe_dir.TrimEnd(Path.DirectorySeparatorChar));

                _dic_font_infos[recipe_name] = new Dictionary<string, List<CharacterInfo>>(); 

                string font_path = recipe_dir + "\\" + FONTS;

                if (!Directory.Exists(font_path))
                    continue;

                string[] chars_path = Directory.GetDirectories(font_path);

                foreach (string char_path in chars_path)
                {
                    string char_name = Path.GetFileName(char_path.TrimEnd(Path.DirectorySeparatorChar));

                    string[] temp_names = char_name.Split('_');

                    if (temp_names.Length != 2)
                        continue;

                    char_name = temp_names[1];

                    //int char_result = 0;
                    //int[] array_temp = new int[1];
                    //var bool_result = int.TryParse(char_name, out char_result);

                    //if(bool_result == true)
                    //{
                    //    array_temp[0] = char_result;
                    //    char_name = new string(array_temp.Select(code => (char)code).ToArray());
                    //}

                    _dic_font_infos[recipe_name][char_name] = new List<CharacterInfo>();

                    var file_names = Directory.GetFiles(char_path);

                    foreach (var name in file_names)
                    {
                        BitmapImage org_image = new BitmapImage();

                        using (var stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            org_image.BeginInit();
                            org_image.CacheOption = BitmapCacheOption.OnLoad; // 파일 로딩 후 핸들 해제
                            org_image.UriSource = null;                       // Uri 대신 스트림 사용
                            org_image.StreamSource = stream;
                            org_image.EndInit();
                            org_image.Freeze(); // UI 스레드에서 안전하게 사용
                        }

                        CharacterInfo info = new CharacterInfo
                        {
                            ImageName = Path.GetFileNameWithoutExtension(name),

                            ImageSource = org_image
                        };

                        _dic_font_infos[recipe_name][char_name].Add(info);
                    }
                }
            }

            return true;
        }

        public bool SaveAllFont(string path)
        {
            return true;
        }

        public bool LoadFont(string path, string font_name)
        {
            var folderPath = path + "\\Recipe\\" + font_name + "\\" + FONTS;

            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            var dirinfo = Directory.GetDirectories(folderPath);

            return true;
        }

        public bool SaveFont(string path, string font_name)
        {
            if (!_dic_font_infos.ContainsKey(font_name))
                return false;

            var folderPath = path + "\\Recipe\\" + font_name + "\\" + FONTS;

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, recursive: true);
            }

            foreach (var chars in _dic_font_infos[font_name])
            {
                var char_name = chars.Key;

                var type = GetCharacterType(char_name[0]);

                var char_path = folderPath + "\\" + (int)type + "_" + char_name;

                if (!Directory.Exists(char_path))
                {
                    Directory.CreateDirectory(char_path);
                }

                foreach (var char_img in chars.Value)
                {
                    var fullpath = char_path + "\\" + char_img.ImageName + ".bmp";
                    char_img.ImageSource.Save(fullpath);
                }
            }

            return true;
        }

        public List<CharacterInfo> GetCharacterInfoList(string font_name)
        {
            List<CharacterInfo> list = new List<CharacterInfo>();

            if(_dic_font_infos != null)
            {
                if(_dic_font_infos.ContainsKey(font_name))
                {
                    foreach(var fonts in _dic_font_infos[font_name])
                    {
                        foreach(var c in fonts.Value)
                        {
                            c.ImageName = fonts.Key;
                            list.Add(c);
                        }
                    }
                }
            }

            return list;
        }


        public OCRConfigruation GetOCRConfiguration(string font_name)
        {
            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
            if(config != null)
            {
                var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();
                if(recipe != null)
                {
                    recipe.Load(config.RecipePath, font_name);
                    return recipe;
                }
            }
            return null;
        }

        #endregion
        
        #region Processing 1
        public ProcessInfo BinProcessing(ByteImage TestImage, string font_name, CharacterPolarity polarity, double score, CharacterTypes char_type)
        {
            ProcessInfo proc_info = new ProcessInfo();

            if (!_dic_font_infos.ContainsKey(font_name))
                return null;

            List<ProcessInfo> score_list = new List<ProcessInfo>();

            AppLogger.Info()("BinProcessing() - CharacterType : [{0}].", char_type);

            foreach (var char_info in _dic_font_infos[font_name])
            {
                bool flag = false;
                string char_name = char_info.Key;

                if (char_type == CharacterTypes.Number)
                    flag = char.IsNumber(char_name[0]);
                else if (char_type == CharacterTypes.UpperCase)
                    flag = char.IsUpper(char_name[0]);
                else if (char_type == CharacterTypes.LowerCase)
                    flag = char.IsLower(char_name[0]);
                else if (char_type == CharacterTypes.SpecialCase)
                    flag = true;

                if (flag != true)
                    continue;

                AppLogger.Info()("===== Reference Character Name : [{0}] ==========", char_name);
                int i = 0;

                foreach (var info in char_info.Value)
                {
                    double tscore = 0.0;
                    var data = info.ImageSource.GetPixelBytes();

                    ByteImage RefImage = new ByteImage((int)info.ImageSource.Width, (int)info.ImageSource.Height, (int)info.ImageSource.Width, data, 0);


                    //RefImage.Save("D:\\RefImage.bmp");
                    //TestImage.Save("D:\\TestImage.bmp");


                    //tscore = CompareCharacter3(RefImage, TestImage, polarity);
                    tscore = CompareCharacter3(TestImage, RefImage, polarity);
                    //tscore = CompareCharacter(TestImage, RefImage);
                    //tscore_list.Add(tscore);

                    //CharacterInfo score_info = new CharacterInfo();
                    //score_info.ImageSource = info.ImageSource;
                    //score_info.ImageName = string.Format($"{tscore}");
                    //score_list.Add(score_info);

                    ProcessInfo score_info = new ProcessInfo();
                    score_info.Image = RefImage;
                    score_info.Score = tscore;
                    score_info.Character = char_name;
                    score_list.Add(score_info);

                    OnSingleCompareCharacterEvent(TestImage, RefImage, i, char_name, tscore);

                    AppLogger.Info()("[{0}] image score : [{1}]", i++, tscore);
                }
                var item = score_list.FirstOrDefault(f => f.Character == char_name);

                if (item != null)
                {
                    AppLogger.Info()("Score - Average : {0}, Max : {1}, Min : {2}",
                    score_list.Where(w => w.Character == char_name).Average(a => a.Score),
                    score_list.Where(w => w.Character == char_name).Max(a => a.Score),
                    score_list.Where(w => w.Character == char_name).Min(a => a.Score));
                }
            }
                        
            if(score_list.Count == 0)
            {
                AppLogger.Error()("========= Score list is empty.");
                
                return proc_info;
            }

            proc_info = score_list.OrderByDescending(p => p.Score).FirstOrDefault();
            AppLogger.Info()("======= Best Score : [{0}], Character : [{1}].", proc_info.Score * 100.0, proc_info.Character);
            return proc_info;
        }

        public ProcessInfo GrayProcessing(ByteImage TestImage, string font_name, int threshold, CharacterPolarity polarity, double score, CharacterTypes char_type)
        {
            AppLogger.Info()("GrayProcessing() - CharacterType : [{0}].", char_type);

            ProcessInfo proc_info = new ProcessInfo();

            return proc_info;
        }

        public double CompareCharacter(ByteImage RefImage, ByteImage TestImage)
        {
            double score = 0.0;
            int ref_width = RefImage.Width;
            int ref_height = RefImage.Height;
            
            var resize_image = TestImage.Resize(ref_width, ref_height);

            var ref_data = RefImage.Data;

            var test_data = resize_image.Data;

            ulong ulTotalCount = 0;
            ulong ulCount = 0;

            int nMinWeightValue = 1;
            int nMaxWeightValue = 3;

            for (int i = 0; i < ref_height; i++)
            {
                for (int j = 0; j < ref_width; j++)
                {
                    int ref_compare_pixel = 0;
                    int test_compare_pixel = 0;

                    if (ref_data[i * ref_width + j] > 1)
                        ref_compare_pixel = 255;
                    else
                        ref_compare_pixel = 0;

                    if (test_data[i * ref_width + j] > 1)
                        test_compare_pixel = 255;
                    else
                        test_compare_pixel = 0;

                    if (ref_compare_pixel == test_compare_pixel)
                    {
                        ulCount += (ulong)nMaxWeightValue;
                    }
                    else
                    {
                        int x1, x2;
                        int y1, y2;

                        x1 = j - 1; x2 = j + 1;

                        y1 = i - 1; y2 = i + 1;

                        if (x1 < 0)
                            x1 = 0;

                        if (x2 >= ref_width)
                            x2 = ref_width;

                        if (y1 < 0)
                            y1 = 0;

                        if (y2 >= ref_height)
                            y2 = ref_height;

                        var flag = false;

                        for (int k = y1; y1 < y2; y1++)
                        {
                            for (int p = x1; x1 < x2; x1++)
                            {
                                if (test_data[k * ref_width + p] > 1)
                                    test_compare_pixel = 255;
                                else
                                    test_compare_pixel = 0;

                                if (ref_compare_pixel == test_compare_pixel)
                                {
                                    ulCount += (ulong)nMinWeightValue;
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag == true)
                                break;
                        }
                    }
                }
            }

            ulTotalCount = (ulong)(ref_width * ref_height * nMaxWeightValue);

            score = (double)ulCount / ulTotalCount;

            return score;
        }

        public double CompareCharacter2(ByteImage RefImage, ByteImage TestImage)
        {
            double score = 0.0;

            int ref_width = RefImage.Width;
            int ref_height = RefImage.Height;


            var resize_image = TestImage.Resize(ref_width, ref_height);

            var ref_data = RefImage.Data;

            var test_data = resize_image.Data;

            ulong ulTotalCount = 0;
            ulong ulCount = 0;

            int nMaxWeightValue = 9;

            for (int i = 0; i < ref_height; i++)
            {
                for (int j = 0; j < ref_width; j++)
                {
                    int ref_compare_pixel = 0;
                    int test_compare_pixel = 0;

                    if (ref_data[i * ref_width + j] > 1)
                        ref_compare_pixel = 255;
                    else
                        ref_compare_pixel = 0;

                    if (test_data[i * ref_width + j] > 1)
                        test_compare_pixel = 255;
                    else
                        test_compare_pixel = 0;

                    if (ref_compare_pixel == test_compare_pixel)
                    {
                        ulCount += 9;
                    }
                    else
                    {
                        int x1, x2;
                        int y1, y2;

                        x1 = j - 1; x2 = j + 1;

                        y1 = i - 1; y2 = i + 1;


                        if (x1 < 0)
                            x1 = 0;

                        if (x2 >= ref_width)
                            x2 = ref_width;

                        if (y1 < 0)
                            y1 = 0;

                        if (y2 >= ref_height)
                            y2 = ref_height;
                        
                        int match_count = 0;

                        for (int k = y1; y1 < y2; y1++)
                        {
                            for (int p = x1; x1 < x2; x1++)
                            {

                                if (i == k && j == p)
                                    continue;

                                if (test_data[k * ref_width + p] > 1)
                                    test_compare_pixel = 255;
                                else
                                    test_compare_pixel = 0;

                                if (ref_compare_pixel == test_compare_pixel)
                                {
                                    match_count++;
                                }
                            }
                        }
                        ulCount += (ulong)match_count;
                    }
                }
            }

            ulTotalCount = (ulong)(ref_width * ref_height * nMaxWeightValue);

            if (ulTotalCount == 0)
                return 0.0;

            score = (double)ulCount / ulTotalCount;

            return score;
        }

        public double CompareCharacter3(ByteImage RefImage, ByteImage TestImage, CharacterPolarity polarity)
        {
            int resize_threshold = 125;

            int ref_compare_pixel = 0;
            int test_compare_pixel = 0;

            double score = 0.0;

            int ref_width = RefImage.Width;
            int ref_height = RefImage.Height;

            var resize_image = TestImage.Resize(ref_width, ref_height);

            int bin_value = polarity == CharacterPolarity.DarkOnLight ? 255 : 0;

            var ref_data = RefImage.Data;

            var test_data = resize_image.Data;

            int ref_total_count = 0;
            int test_total_count = 0;

            for (int i = 0; i < ref_height; i++)
            {
                for (int j = 0; j < ref_width; j++)
                {
                    ref_compare_pixel = 0;
                    test_compare_pixel = 0;

                    if (ref_data[i * ref_width + j] > resize_threshold)
                        ref_compare_pixel = 255;
                    else
                        ref_compare_pixel = 0;

                    if (ref_compare_pixel == bin_value)
                        ref_total_count++;

                    ref_compare_pixel = 0;
                    test_compare_pixel = 0;

                    if (test_data[i * ref_width + j] > resize_threshold)
                        test_compare_pixel = 255;
                    else
                        test_compare_pixel = 0;

                    if (test_compare_pixel == bin_value)
                        test_total_count++;

                }
            }

            int ulTotalCount = 0;
            int ulCount = 0;

            int nMaxWeightValue = 9;

            for (int i = 0; i < ref_height; i++)
            {
                for (int j = 0; j < ref_width; j++)
                {
                    if (ref_data[i * ref_width + j] > resize_threshold)
                        ref_compare_pixel = 255;
                    else
                        ref_compare_pixel = 0;

                    if (bin_value != ref_compare_pixel)
                        continue;

                    if (test_data[i * ref_width + j] > resize_threshold)
                        test_compare_pixel = 255;
                    else
                        test_compare_pixel = 0;

                    if (ref_compare_pixel == test_compare_pixel)
                    {
                        ulCount += nMaxWeightValue;
                    }
                    else
                    {
                        int x1, x2;
                        int y1, y2;

                        x1 = j - 1; x2 = j + 1;

                        y1 = i - 1; y2 = i + 1;


                        if (x1 < 0)
                            x1 = 0;

                        if (x2 >= ref_width)
                            x2 = ref_width;

                        if (y1 < 0)
                            y1 = 0;

                        if (y2 >= ref_height)
                            y2 = ref_height;

                        int match_count = 0;

                        for (int k = y1; y1 < y2; y1++)
                        {
                            for (int p = x1; x1 < x2; x1++)
                            {
                                if (i == k && j == p)
                                    continue;

                                if (test_data[k * ref_width + p] > resize_threshold)
                                    test_compare_pixel = 255;
                                else
                                    test_compare_pixel = 0;

                                if (ref_compare_pixel == test_compare_pixel)
                                {
                                    match_count++;
                                }
                            }
                        }

                        if (match_count == 0)
                            ulCount -= nMaxWeightValue;
                        else
                            ulCount += match_count;
                    }

                    ulTotalCount += nMaxWeightValue;
                }
            }

            ulTotalCount = Math.Max(ref_total_count, test_total_count) * nMaxWeightValue;

            if (ulTotalCount == 0)
                return 0.0;

            score = (double)ulCount / ulTotalCount;

            return score;
        }

        public double CompareCharacter4(ByteImage RefImage, ByteImage TestImage, CharacterPolarity polarity)
        {
            double score = 0.0;

            int ref_width = RefImage.Width;
            int ref_height = RefImage.Height;

            var resize_image = TestImage.Resize(ref_width, ref_height);

            var ref_data = RefImage.Data;
            var test_data = resize_image.Data;

            double total_score = 0.0;

            for (int i = 0; i < ref_height; i++)
            {
                for (int j = 0; j < ref_width; j++)
                {
                    var ref_value = ref_data[i * ref_width + j];
                    var test_value = test_data[i * ref_width + j];

                    var temp_score = ref_value == 0.0 ? 0.0 : (double)test_value / ref_value;

                    var test_score = temp_score < 1.0 ? temp_score : 2.0 - temp_score;

                    total_score += test_score;

                }

            }


            return score;
        }
        #endregion

        #region Processing 2
        public CharacterTypes GetCharacterType(char c)
        {
            if (char.IsNumber(c) == true)
                return CharacterTypes.Number;
            else if (char.IsUpper(c) == true)
                return CharacterTypes.UpperCase;
            else if (char.IsLower(c) == true)
                return CharacterTypes.LowerCase;

            return CharacterTypes.SpecialCase;
        }
        public List<CharacterTypes> GetCharacterTypes(string str)
        {
            List<CharacterTypes> list = new List<CharacterTypes>();

            if (str == "" || str == string.Empty)
                return list;

            foreach (char c in str)
            {
                list.Add(GetCharacterType(c));
            }
            return list;
        }
        public List<ProcessInfo> MultiCharProcessing(ByteImage TestImage, string font_name, int threshold, CharacterPolarity polarity, double score, List<CharacterTypes> char_types, TestImageTypes test_image_type, bool debug_image = false)
        {
            List<ProcessInfo> list = new List<ProcessInfo>();

            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
            if (config == null)
                return list;

            var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();

            if (recipe == null)
                return list;

            recipe.Load(config.RecipePath, font_name);

            int char_max_width = recipe.CharacterMaxWidth;
            int char_max_height = recipe.CharacterMaxHeight;

            int char_min_width = recipe.CharacterMinWidth;
            int char_min_height = recipe.CharacterMinHeight;

            int min_area = char_min_width * char_min_height;
            int max_area = char_max_width * char_max_height;

            var bin_image = TestImage.Binarization(1, threshold, BaseLib.Enums.BinTypes.InSide);

            if (debug_image == true)
                bin_image.Save("D:\\test_bin_image.bmp");

            var blobs = ByteImage.Blob(bin_image, min_area, max_area, char_min_width, char_max_width,
                        char_min_height, char_max_height, BaseLib.Enums.BlobTypes.White);

            int char_count = char_types.Count;

            if (blobs.Count != char_count)
            {
                AppLogger.Error()("Blob and Character count is not match. Blob : [{0}], CharCount : [{1}]", blobs.Count, char_count);
                return list;
            }

            blobs = blobs.OrderBy(p => p.Left).ToList();

            List<ByteImage> test_image_list = new List<ByteImage>();

            foreach (var blob in blobs)
            {
                var roi = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

                ByteImage blob_image = null;

                if (test_image_type == TestImageTypes.Gray)
                {
                    blob_image = TestImage.Crop(roi) as ByteImage;
                }
                else
                {
                    blob_image = bin_image.Crop(roi) as ByteImage;
                }
                var extand_image = new ByteImage(blob_image.Width + (recipe.CharGap * 2), blob_image.Height + (recipe.CharGap * 2));

                RoiRectangle rect = new RoiRectangle(recipe.CharGap, recipe.CharGap, blob.Height + recipe.CharGap, blob.Width + recipe.CharGap);

                extand_image.InsertChildBuffer(rect, blob_image.Data);
                var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, recipe.FillHoleArea);
                test_image_list.Add(fillhole_image);
            }
            int i = 0;
            if (debug_image == true)
            {
                foreach (var image in test_image_list)
                {
                    image.Save(string.Format("D:\\Test_Dimage_{0}.bmp", i));
                    i++;
                }
            }

            i = 0;
            foreach (var image in test_image_list)
            {
                var char_type = char_types[i];

                ProcessInfo info = null;

                if (test_image_type == TestImageTypes.Binary)
                    info = BinProcessing(image, font_name, polarity, score, char_type);
                else
                    info = GrayProcessing(image, font_name, threshold, polarity, score, char_type);

                if (info != null)
                    list.Add(info);

                i++;
            }

            var ProcessInfoArr = list.ToArray();

            OnMultiCompareCharacterEvent(test_image_list.ToArray(), ProcessInfoArr);

            return list;
        }
               
        public List<ProcessInfo> MultiCharProcessing(ByteImage TestImage, string font_name, int threshold, int otsu_offset, CharacterPolarity polarity, double score, string TestString, TestImageTypes test_image_type, bool debug_image = false)
        {
            List<ProcessInfo> list = new List<ProcessInfo>();

            var config = ConfigManager.Instance.GetConfiguration<OCRSystemConfiguration>();
            if (config == null)
                return list;

            var recipe = ConfigManager.Instance.GetConfiguration<OCRConfigruation>();

            if (recipe == null)
                return list;

            recipe.Load(config.RecipePath, font_name);

            int char_max_width = recipe.CharacterMaxWidth;
            int char_max_height = recipe.CharacterMaxHeight;

            int char_min_width = recipe.CharacterMinWidth;
            int char_min_height = recipe.CharacterMinHeight;

            int min_area = char_min_width * char_min_height;
            int max_area = char_max_width * char_max_height;


            ByteImage bin_image = null;

            int otsu_level = 0;

            if (otsu_offset == 0)
                bin_image = TestImage.Binarization(1, threshold, BaseLib.Enums.BinTypes.InSide);
            else
                bin_image = TestImage.OtsuBinarization(out otsu_level, otsu_offset);

            threshold = otsu_level + otsu_offset;

            int i = 0;
            int dilation_erosion = recipe.DilationErosion;

            for (i = 0; i < dilation_erosion; i++)
            {
                bin_image = bin_image.DilationVertical();
            }

            //bin_image.Save("D:\\DilationVertical.bmp");

            for (i = 0; i < dilation_erosion; i++)
            {
                bin_image = bin_image.ErosionVertical();
            }
            //bin_image.Save("D:\\ErosionVertical.bmp");


            //for (i = 0; i < dilation_erosion; i++)
            //{
            //    bin_image = bin_image.DilationHorizontal();
            //}

            ////bin_image.Save("D:\\DilationHorizontal.bmp");
            //for (i = 0; i < dilation_erosion; i++)
            //{
            //    bin_image = bin_image.ErosionHorizontal();
            //}

            if(debug_image == true)
                bin_image.Save("D:\\ErosionDilation.bmp");



            var blobs = ByteImage.Blob(bin_image, min_area, max_area, char_min_width, char_max_width,
                        char_min_height, char_max_height, BaseLib.Enums.BlobTypes.White);

            if (threshold != 0)
                bin_image = TestImage.Binarization(1, threshold, BaseLib.Enums.BinTypes.InSide);

            if (debug_image == true)
                bin_image.Save("D:\\bin_image.bmp");

            List<CharacterTypes> char_types = GetCharacterTypes(TestString);

            int char_count = char_types.Count;

            if (blobs.Count != char_count)
            {
                AppLogger.Error()("TestString : [{0}], Blob and Character count is not match. Blob : [{1}], CharCount : [{2}]", TestString, blobs.Count, char_count);

                return list;
            }

            blobs = blobs.OrderBy(p => p.Left).ToList();

            List<ByteImage> test_image_list = new List<ByteImage>();

            foreach (var blob in blobs)
            {
                var roi = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

                ByteImage blob_image = null;

                if (test_image_type == TestImageTypes.Gray)
                {
                    blob_image = TestImage.Crop(roi) as ByteImage;
                }
                else
                {
                    blob_image = bin_image.Crop(roi) as ByteImage;
                }
                var extand_image = new ByteImage(blob_image.Width + (recipe.CharGap * 2), blob_image.Height + (recipe.CharGap * 2));

                RoiRectangle rect = new RoiRectangle(recipe.CharGap, recipe.CharGap, blob.Height + recipe.CharGap, blob.Width + recipe.CharGap);

                extand_image.InsertChildBuffer(rect, blob_image.Data);


                if (recipe.FillHoleArea != 0)
                {
                    extand_image.Save(string.Format("D:\\extand_image.bmp"));


                    var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, recipe.FillHoleArea);

                    fillhole_image.Save(string.Format("D:\\fillhole_image.bmp"));

                    test_image_list.Add(fillhole_image);
                }
                else
                {
                    test_image_list.Add(extand_image);
                }

            }
            i = 0;
            if (debug_image == true)
            {
                foreach (var image in test_image_list)
                {
                    image.Save(string.Format("D:\\Test_Dimage_{0}.bmp", i));
                    i++;
                }
            }

            i = 0;
            foreach (var image in test_image_list)
            {
                var char_type = char_types[i];

                ProcessInfo info = null;

                if(char_type == CharacterTypes.SpecialCase)
                {
                    var special_case = TestString[i];

                    info = new ProcessInfo();
                    info.Character = special_case.ToString();
                    info.Image = image;
                    info.Score = 1;

                }
                else
                {
                    if (test_image_type == TestImageTypes.Binary)
                        info = BinProcessing(image, font_name, polarity, score, char_type);
                    else
                        info = GrayProcessing(image, font_name, threshold, polarity, score, char_type);


                }
                if (info != null)
                    list.Add(info);

                i++;
            }

            var ProcessInfoArr = list.ToArray();

            OnMultiCompareCharacterEvent(test_image_list.ToArray(), ProcessInfoArr);

            return list;
        }

        public List<ProcessInfo> MultiCharProcessing(ByteImage TestImage, string font_name, double score, string TestString, TestImageTypes test_image_type, ThresholdTypes threshold_type, bool debug_image = false)
        {
            List<ProcessInfo> list = new List<ProcessInfo>();

            var recipe = GetOCRConfiguration(font_name);

            int char_max_width = recipe.CharacterMaxWidth;
            int char_max_height = recipe.CharacterMaxHeight;

            int char_min_width = recipe.CharacterMinWidth;
            int char_min_height = recipe.CharacterMinHeight;

            int min_area = char_min_width * char_min_height;
            int max_area = char_max_width * char_max_height;

            var threshold = recipe.OtsuLevel;

            var polarity = recipe.CharacterPolarity;

            int dilation_erosion = recipe.DilationErosion;

            var bin_image = OCR_Binarization(TestImage, threshold, threshold_type, debug_image);

            var mopology_bin_image = Mopology_Vertical(bin_image, dilation_erosion, debug_image);


            var blobs = ByteImage.Blob(mopology_bin_image, min_area, max_area, char_min_width, char_max_width,
                        char_min_height, char_max_height, BaseLib.Enums.BlobTypes.White);

            //if (threshold != 0)
            //    bin_image = TestImage.Binarization(1, threshold, BaseLib.Enums.BinTypes.InSide);

            //if (debug_image == true)
            //    bin_image.Save("D:\\bin_image.bmp");

            List<CharacterTypes> char_types = GetCharacterTypes(TestString);

            int char_count = char_types.Count;

            if (blobs.Count != char_count)
            {
                AppLogger.Error()("TestString : [{0}], Blob and Character count is not match. Blob : [{1}], CharCount : [{2}]", TestString, blobs.Count, char_count);

                return list;
            }

            blobs = blobs.OrderBy(p => p.Left).ToList();

            List<ByteImage> test_image_list = new List<ByteImage>();

            foreach (var blob in blobs)
            {
                var roi = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

                var blob_image = bin_image.Crop(roi) as ByteImage;

                var extand_image = new ByteImage(blob_image.Width + (recipe.CharGap * 2), blob_image.Height + (recipe.CharGap * 2));

                RoiRectangle rect = new RoiRectangle(recipe.CharGap, recipe.CharGap, blob.Height + recipe.CharGap, blob.Width + recipe.CharGap);

                extand_image.InsertChildBuffer(rect, blob_image.Data);


                if (recipe.FillHoleArea != 0)
                {
                    var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, recipe.FillHoleArea);
                    test_image_list.Add(fillhole_image);
                }
                else
                {
                    test_image_list.Add(extand_image);
                }

            }
            int i = 0;
            if (debug_image == true)
            {
                foreach (var image in test_image_list)
                {
                    image.Save(string.Format("D:\\Test_Dimage_{0}.bmp", i));
                    i++;
                }
            }

            i = 0;
            foreach (var image in test_image_list)
            {
                var char_type = char_types[i];

                ProcessInfo info = null;

                if (char_type == CharacterTypes.SpecialCase)
                {
                    var special_case = TestString[i];

                    info = new ProcessInfo();
                    info.Character = special_case.ToString();
                    info.Image = image;
                    info.Score = 1;

                }
                else
                {
                    info = BinProcessing(image, font_name, polarity, score, char_type);
                }
                if (info != null)
                    list.Add(info);

                i++;
            }

            var ProcessInfoArr = list.ToArray();

            OnMultiCompareCharacterEvent(test_image_list.ToArray(), ProcessInfoArr);

            return list;
        }

        #endregion

        #region Training 1

        public ByteImage Mopology_Vertical(ByteImage image, int number, bool debug)
        {
            int i = 0;
            for (i = 0; i < number; i++)
            {
                image = image.DilationVertical();
            }

            if (debug == true && number != 0)
                image.Save(string.Format("D:\\DilationVertical_[{0}].bmp", number));

            for (i = 0; i < number; i++)
            {
                image = image.ErosionVertical();
            }

            if (debug == true && number != 0)
                image.Save(string.Format("D:\\ErosionVertical_[{0}].bmp", number));

            return image;
        }
        public ByteImage OCR_Binarization(ByteImage image, int threshold, ThresholdTypes threshold_type, bool debug)
        {
            int otsu_level = 0;
            ByteImage bin_image = null;

            if (threshold_type == ThresholdTypes.Absolute)
            {
                bin_image = image.Binarization(0, threshold, BaseLib.Enums.BinTypes.InSide);

                if (debug == true)
                    bin_image.Save(string.Format("D:\\Training_bin_Absolute_[0]_[{0}].bmp", threshold));
            }
            else
            {
                bin_image = image.OtsuBinarization(out otsu_level, threshold);

                if (debug == true)
                    bin_image.Save(string.Format("D:\\Training_bin_Relative_[{0}]_[{1}].bmp", otsu_level, threshold));
            }

            return bin_image;
        }
 
        public bool Training(ByteImage train_image, int threshold, CharacterPolarity char_plarity, int min_width, int max_width, int min_height, int max_height, int dilation_erosion, int char_gap, int fill_hole_area, TestImageTypes test_image_type, ThresholdTypes threshold_type, bool debug_image = false)
        {
            _CharacterInfoList = new List<CharacterInfo>();
            
            var bin_image = OCR_Binarization(train_image, threshold, threshold_type, debug_image);

            var mopology_bin_image = Mopology_Vertical(bin_image, dilation_erosion, debug_image);

            var min_area = min_width * min_height;
            var max_area = max_width * max_height;
            
            var blobs = ByteImage.Blob(mopology_bin_image, min_area, max_area, min_width, max_width, min_height, max_height, BaseLib.Enums.BlobTypes.White);

            foreach (var blob in blobs)
            {
                CharacterInfo ii = new CharacterInfo();

                ii.Rectangle = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

                ByteImage blob_image = null;
                if (test_image_type == TestImageTypes.Binary)
                {
                    blob_image = bin_image.Crop(ii.Rectangle) as ByteImage;
                }
                else
                {
                    blob_image = train_image.Crop(ii.Rectangle) as ByteImage;
                }

                var extand_image = new ByteImage(blob_image.Width + (char_gap * 2), blob_image.Height + (char_gap * 2));

                RoiRectangle rect = new RoiRectangle(char_gap, char_gap, blob.Height + char_gap, blob.Width + char_gap);
                extand_image.InsertChildBuffer(rect, blob_image.Data);

                if(fill_hole_area == 0)
                {
                    ii.ImageSource = extand_image.ToBitmapImage();
                }
                else
                {
                    var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, fill_hole_area);
                    ii.ImageSource = fillhole_image.ToBitmapImage();
                }

                _CharacterInfoList.Add(ii);
            }

            return true;
        }
        
        public bool Training(ByteImage train_image, string font_name, bool debug)
        {
            _CharacterInfoList = new List<CharacterInfo>();

            var font_config = GetOCRConfiguration(font_name);
            if (font_config == null)
            {
                AppLogger.Error()("Training() - [{0} font-name] is null.", font_name);
                return false;
            }

            var threshold = font_config.OtsuLevel;
            var dilation_erosion = font_config.DilationErosion;

            var min_width = font_config.CharacterMinWidth;
            var max_width = font_config.CharacterMaxWidth;

            var min_height = font_config.CharacterMinHeight;
            var max_height = font_config.CharacterMaxHeight;

            var char_gap = font_config.CharGap;
            var gray_image = font_config.GrayImage;

            var fill_hole_area = font_config.FillHoleArea;

            TestImageTypes test_image_type = gray_image == true ? TestImageTypes.Gray : TestImageTypes.Binary;

            var threshold_type = ThresholdTypes.Relative;

            var bin_image = OCR_Binarization(train_image, threshold, threshold_type, debug);

            var mopology_bin_image = Mopology_Vertical(bin_image, dilation_erosion, debug);

            //var min_area = min_width * min_height;
            //var max_area = max_width * max_height;

            var min_area = font_config.CharMinArea;
            var max_area = font_config.CharMaxArea;


            var blobs = ByteImage.Blob(mopology_bin_image, min_area, max_area, min_width, max_width, min_height, max_height, BaseLib.Enums.BlobTypes.White);

            foreach (var blob in blobs)
            {
                CharacterInfo ii = new CharacterInfo();

                ii.Rectangle = new RoiRectangle(blob.Top, blob.Left, blob.Bottom, blob.Right);

                ByteImage blob_image = null;
                if (test_image_type == TestImageTypes.Binary)
                {
                    blob_image = bin_image.Crop(ii.Rectangle) as ByteImage;
                }
                else
                {
                    blob_image = train_image.Crop(ii.Rectangle) as ByteImage;
                }

                var extand_image = new ByteImage(blob_image.Width + (char_gap * 2), blob_image.Height + (char_gap * 2));

                RoiRectangle rect = new RoiRectangle(char_gap, char_gap, blob.Height + char_gap, blob.Width + char_gap);
                extand_image.InsertChildBuffer(rect, blob_image.Data);

                if (fill_hole_area == 0)
                {
                    ii.ImageSource = extand_image.ToBitmapImage();
                }
                else
                {
                    var fillhole_image = extand_image.FillHole(BaseLib.Enums.BlobTypes.Black, fill_hole_area);
                    ii.ImageSource = fillhole_image.ToBitmapImage();
                }

                _CharacterInfoList.Add(ii);
            }

            return true;
        }

        #endregion


        #region constructors
        protected OCRManager()
        {

        }
        #endregion
    }
}
