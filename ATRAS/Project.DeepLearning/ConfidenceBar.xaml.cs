using System;
using System.Collections.Generic;
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

namespace Project.DeepLearning.UI
{
    public partial class ConfidenceBar : UserControl
    {
        #region fields
        public static readonly DependencyProperty ColorIndexProperty =
            DependencyProperty.Register("ColorIndex", typeof(int), typeof(ConfidenceBar), new PropertyMetadata(null));


        public static readonly DependencyProperty ClassNameProperty =
            DependencyProperty.Register("ClassName", typeof(string), typeof(ConfidenceBar), new PropertyMetadata(null));


        public static readonly DependencyProperty ConfidenceProperty =
            DependencyProperty.Register("ConfidenceProperty", typeof(double), typeof(ConfidenceBar), new PropertyMetadata(null));
        #endregion

        #region propertise
        public int ColorIndex
        {
            get
            {
                return (int)GetValue(ColorIndexProperty);
            }

            set
            {
                SetValue(ColorIndexProperty, value);
            }
        }
        public string ClassName
        {
            get
            {
                return (string)GetValue(ClassNameProperty);
            }

            set
            {
                SetValue(ClassNameProperty, value);
            }
        }
        public double Confidence
        {
            get
            {
                return (double)GetValue(ConfidenceProperty);
            }

            set
            {
                SetValue(ConfidenceProperty, value);
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public ConfidenceBar()
        {
            InitializeComponent();
        }
        #endregion

    }
}
