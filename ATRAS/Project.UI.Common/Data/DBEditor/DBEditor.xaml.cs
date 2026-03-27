using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;

namespace Project.UI.Common
{
    /// <summary>
    /// Interaction logic for DBEditor.xaml
    /// </summary>
    public partial class DBEditor : UserControl
    {
        string currentFileName;

        FoldingManager foldingManager;
        //AbstractFoldingStrategy foldingStrategy;
        XmlFoldingStrategy foldingStrategy;


        public TextDocument Document
        {
            get
            {
                return textEditor.Document;
            }

            set
            {
                if(textEditor.Document.Text.Equals(string.Empty))
                {

                }
                else
                {
                    textEditor.Document = value;
                }
            }
        }


        public DBEditor()
        {
            InitializeComponent();

            foldingStrategy = new XmlFoldingStrategy();
            foldingManager = FoldingManager.Install(this.textEditor.TextArea);
        }

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() ?? false)
            {
                string currentFileName = dlg.FileName;
                if (Path.GetExtension(currentFileName).ToUpper() == ".XML")
                {
                    currentFileName = dlg.FileName;
                    textEditor.Load(currentFileName);

                    return;
                }
                MessageBox.Show("Please, Select the xml config file!");
            }
        }
        private void saveFileClick(object sender, RoutedEventArgs e)
        {

        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            if (textEditor.Document.Text.Equals(string.Empty))
            {
                //((DataContext as DataViewModel).ImportCommand as DelegateCommandBase).RaiseCanExecuteChanged();
            }
            else
            {
                if (foldingStrategy != null)
                {
                    foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
                }

                //if ((DataContext as DataViewModel).ImportCommand.CanExecute(null))
                //    ((DataContext as DataViewModel).ImportCommand as DelegateCommandBase).RaiseCanExecuteChanged();
            }
        }
    }
}
