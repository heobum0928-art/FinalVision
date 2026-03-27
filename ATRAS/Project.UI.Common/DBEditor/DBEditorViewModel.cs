using ICSharpCode.AvalonEdit.Document;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project.UI.Common
{
    public delegate string ConfigurationSelectionChanged(string categoryName, string dbName, string xml);

    public class DBEditorViewModel : ViewModelBase
    {
        public event ConfigurationSelectionChanged configSelectionChanged;

        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }


        private Dictionary<string, Dictionary<string, string>> configPathInfo;
        public Dictionary<string, Dictionary<string, string>> ConfigPathInfo
        {
            get { return configPathInfo; }
        }
        private string selectedCategory;
        public string SelectedCategory
        {
            get { return selectedCategory; }

            set
            {
                if(selectedCategory != value)
                {
                    selectedCategory = value;

                    SelectedDB = configPathInfo[selectedCategory].Keys.First();

                    OnPropertyChanged();
                    OnPropertyChanged("DBList");
                }
            }
        }

        private string selectedDB;
        public string SelectedDB
        {
            get { return selectedDB; }

            set
            {
                selectedDB = value;

                OnPropertyChanged();
            }
        }

        private TextDocument document;

        public TextDocument Document
        {
            get { return document; }
            set
            {
                document = value;
                OnPropertyChanged();
             }
        }

        public List<string> DBList
        {
            get { return configPathInfo[selectedCategory].Keys.ToList(); }
        }

        public List<string> Categories
        {
            get { return configPathInfo.Keys.ToList(); }
        }

        public DBEditorViewModel() : base("DBEditorViewModel")
        {
            document = new TextDocument();
            AddConfigPathInfo();

            SelectedCategory = Categories.First();

            ImportCommand = new DelegateCommand(OnImportCommand, CanExecuteImportExportElementCommand);
            ExportCommand = new DelegateCommand(OnExportCommand, CanExecuteImportExportElementCommand);

        }

        private bool CanExecuteImportExportElementCommand()
        {
            return true;
        }

        private void OnImportCommand()
        {
            logger.Info()("DBEditor - {0} / {1} DB Save.", selectedCategory, selectedDB);

            if (configSelectionChanged != null)
            {

                Document.Text = configSelectionChanged(selectedCategory, selectedDB, Document.Text);
            }
        }

        private void OnExportCommand()
        {
            logger.Info()("OnExportCommand()");
        }

        public void AddConfigPathInfo()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string dataDir = currentDir + "\\..\\Data\\";
            var infos = new DirectoryInfo(dataDir).GetDirectories();

            configPathInfo = new Dictionary<string, Dictionary<string, string>>();

            foreach (var info in infos)
            {
                //logger.Info()(info.Name);
                if(!configPathInfo.ContainsKey(info.Name))
                {
                    configPathInfo[info.Name] = new Dictionary<string, string>();
                }

                foreach (var file in info.GetFiles())
                {
                    string[] split = file.Name.Split('.');
                    configPathInfo[info.Name].Add(split[0], file.FullName);
                }
            }
        }

        public string GetCurrentSelectedPath()
        {
            if(configPathInfo.ContainsKey(selectedCategory))
            {
                if(configPathInfo[selectedCategory].ContainsKey(selectedDB))
                {
                    return configPathInfo[selectedCategory][selectedDB];
                }
            }

            return string.Empty;
        }
    }
}
