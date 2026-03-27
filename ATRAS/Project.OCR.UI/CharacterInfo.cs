using Project.BaseLib.DataStructures;
using Project.BaseLib.Extension;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Project.OCR.UI
{
    public class CharacterInfos : NotifyPropertyChanged
    {
        public ObservableCollection<CharacterInfo> Items { get; set; }

        public CharacterInfos()
        {
            Items = new ObservableCollection<CharacterInfo>();
        }

    }
    public class CharacterInfo : NotifyPropertyChanged
    {
        #region fields

        protected string _ImageName;

        protected BitmapImage _ImageSource;

        protected RoiRectangle _Rectangle;

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

        public RoiRectangle Rectangle
        {
            get
            {
                return _Rectangle;
            }

            set
            {
                _Rectangle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format($"ImageItem : Size : {_ImageSource.Width} X {_ImageSource.Height}, Name : {_ImageName}");
        }

        public CharacterInfo Duplicate()
        {
            CharacterInfo info = new CharacterInfo();
            info.ImageName = this.ImageName;
            info.ImageSource = this.ImageSource.Duplicate();
            info.Rectangle = this.Rectangle.Duplicate();

            return info;

        }
        #endregion

        #region constructors

        public CharacterInfo()
        {
            _ImageName = "?";
            _ImageSource = null;
        }
        #endregion
    }


    public class ProcessInfo : NotifyPropertyChanged
    {
        #region fields
        private double _Score;

        private string _Character;

        private ByteImage _Image;
        #endregion

        #region propertise
        public double Score
        {
            get
            {
                return _Score;
            }
            set
            {
                _Score = value;
                OnPropertyChanged();
            }
        }

        public string Character
        {
            get
            {
                return _Character;
            }
            set
            {
                _Character = value;
                OnPropertyChanged();
            }
        }

        public ByteImage Image
        {
            get
            {
                return _Image;
            }
            set
            {
                _Image = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            int width = 0;
            int height = 0;

            if(_Image != null)
            {
                width = _Image.Width;
                height = _Image.Height;
            }
            return string.Format($"Image : [{width} x {height}], Score : [{_Score}], Character : [{_Character}]");
        }
        #endregion

        #region constructors
        public ProcessInfo()
        {
            _Score = 0.0;

            _Character = "";

            _Image = null; ;
        }
        #endregion
    }


    public class FontInfos : NotifyPropertyChanged
    {
        #region fields
        protected string _FontName;

        protected Dictionary<string, List<CharacterInfo>> _Dic_Items;

        #endregion

        #region propertise

        public List<CharacterInfo> Items
        {
            get
            {
                List<CharacterInfo> list = new List<CharacterInfo>();

                foreach(var dic_item in _Dic_Items)
                {
                    string character = dic_item.Key;

                    int i = 0;
                    foreach(var item in dic_item.Value)
                    {
                        CharacterInfo info = new CharacterInfo();

                        string name = string.Format("{0},{1}", character, i);

                        info.ImageName = name;
                        info.ImageSource = item.ImageSource;
                        list.Add(info);
                        i++;
                    }
                }

                return list;
            }
        }

        public string FontName
        {
            get
            {
                return _FontName;
            }

            set
            {
                _FontName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public bool SetCharacter(List<CharacterInfo> chars)
        {
            if (_Dic_Items == null)
                _Dic_Items = new Dictionary<string, List<CharacterInfo>>();

            foreach(var c in chars)
            {
                if (!_Dic_Items.ContainsKey(c.ImageName))
                    _Dic_Items[c.ImageName] = new List<CharacterInfo>();

                var char_type = OCRManager.Instance.GetCharacterType(c.ImageName[0]);
                if (c.ImageName == "?" || char_type == CharacterTypes.SpecialCase)
                    continue;

                //_Dic_Items[c.ImageName].Add(c.Duplicate());
                _Dic_Items[c.ImageName].Add(c);
            }

            OnPropertyChanged("Items");

            return true;
        }

        public bool RemoveCharacter(string name)
        {
            var infos = name.Split(',');
            if (infos.Count() != 2)
            {
                AppLogger.Error()("Name : {0}", name);
                return false;
            }

            if(_Dic_Items.ContainsKey(infos[0]))
            {
                var index = int.Parse(infos[1]);
                _Dic_Items[infos[0]].RemoveAt(index);
            }

            OnPropertyChanged("Items");
            return true;
        }

        public bool AllRemoveCharacter()
        {

            _Dic_Items = new Dictionary<string, List<CharacterInfo>>();
            OnPropertyChanged("Items");
            return true;
        }
        #endregion

        #region constructors
        public FontInfos()
        {
            _Dic_Items = new Dictionary<string, List<CharacterInfo>>();
        }
        #endregion
    }
}
