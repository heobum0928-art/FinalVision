using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class RadioButtonStackPanelEventArgs : EventArgs
    {
        #region fields
        protected MyRadioButton _SelectedButton;

        #endregion

        #region propertise
        public MyRadioButton SelectedButton
        {
            get { return _SelectedButton; }
        }

        #endregion

        #region methods

        #endregion

        #region constructors
        public RadioButtonStackPanelEventArgs(MyRadioButton button)
        {
            _SelectedButton = button;
        }
        #endregion
    }
    public class RadioButtonStackPanel : Control
    {
        #region fields

        public static readonly DependencyProperty ButtonCountProperty;

        public static readonly DependencyProperty ButtonListProperty;

        public static readonly DependencyProperty IsVisiblesProperty;

        //public static readonly DependencyProperty SelectedButtonIndexProperty;

        protected Border bord;

        protected StackPanel stack;

        protected List<MyRadioButton> listBtn = new List<MyRadioButton>();

        MyRadioButton buttonSelected;

        MyRadioButton buttonHighlighted;

        protected int length = 20;

        protected int maxCount = 100;

        // Public "Changed" event.
        public event EventHandler SelectedItemChanged;



        #endregion

        #region propertise
        //public int SelectedIndex
        //{
        //    //set
        //    //{
        //    //    listBtn;
        //    //    ButtonList;
        //    //}

        //    get
        //    {
        //        //int index = 0;
        //        //Dispatcher.Invoke(new Action(delegate
        //        //{
        //        //    if (buttonSelected != null)
        //        //    {
        //        //        index = ButtonList.IndexOf(buttonSelected);
        //        //    }
        //        //}));

        //        //return index;


        //        return buttonSelected.ButtonIndex;
        //    }
        //}


        public Orientation Orientation
        {
            set { stack.Orientation = value; }
        }

        public int ButtonCount
        {
            get
            {
                return ButtonList.Count;
            }

            //set
            //{
            //    ButtonList = new ObservableCollection<MyRadioButton>();

            //    for(int i = 0; i < value; i++)
            //    {
            //        var btn = new MyRadioButton();
            //        ButtonList.Add(btn);
            //    }
            //}
        }

        public ObservableCollection<MyRadioButton> ButtonList
        {
            get { return (ObservableCollection<MyRadioButton>)GetValue(ButtonListProperty); }

            set { SetValue(ButtonListProperty, value); }
        }

        public bool[] IsVisibles
        {
            get { return (bool[])GetValue(IsVisiblesProperty); }

            set
            {
                SetValue(IsVisiblesProperty, value);
            }
        }

        public int SelectedButtonIndex
        {

            get 
            {
                if(buttonSelected == null)
                    return 0;

                return buttonSelected.ButtonIndex;
            }



            set
            {
                if(ButtonCount > value)
                {
                    var btn = ButtonList[value];


                    if (btn != null)
                    {
                        if (buttonSelected != null)
                            buttonSelected.IsSelected = false;

                        buttonSelected = btn;
                        buttonSelected.IsSelected = true;


                        foreach (var b in ButtonList)
                        {
                            b.Brush = ButtonBackGroundColor;
                        }


                        buttonSelected.Brush = SelectedColor;
                        OnSelectedItemChanged(new RadioButtonStackPanelEventArgs(btn));

                    }
                }
            }

            //get { return (int)GetValue(SelectedButtonIndexProperty); }

            //set { SetValue(SelectedButtonIndexProperty, value); }
        }

        public int Length
        {
            set
            {
                length = value;
                InvalidateMeasure();
            }

            get { return length; }
        }

        public Brush ButtonBackGroundColor { get; set; }

        public Brush SelectedColor { get; set; }
        #endregion

        #region methods
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        // Override of GetVisualChild.
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0)
                throw new ArgumentOutOfRangeException("index");

            return bord;

        }
        // Override of MeasureOverride.
        protected override Size MeasureOverride(Size constraint)
        {
            stack.Children.Clear();
            foreach (var item in ButtonList)
            {
                item.Length = Length;
                item.FontSize = FontSize;
                item.FontWeight = FontWeight;
                item.FontStyle = FontStyle;
                item.FontFamily = FontFamily;
                item.FontStretch = FontStretch;
                item.Foreground = Foreground;
                item.FlowDirection = FlowDirection;

                stack.Children.Add(item);
            }
            bord.Measure(constraint);
            return bord.DesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            bord.Arrange(new Rect(new Point(0, 0), arrangeBounds));
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (buttonHighlighted != null)
            {
                buttonHighlighted.IsHighlighted = false;
                buttonHighlighted = null;

            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MyRadioButton btn = e.Source as MyRadioButton;

            if (btn != null)
            {
                if (buttonHighlighted != null)
                    buttonHighlighted.IsHighlighted = false;

                buttonHighlighted = btn;
                buttonHighlighted.IsHighlighted = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (buttonHighlighted != null)
            {
                buttonHighlighted.IsHighlighted = false;
                buttonHighlighted = null;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            MyRadioButton btn = e.Source as MyRadioButton;
            MyRadioButton btn2 = e.OriginalSource as MyRadioButton;
        }

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {
            base.OnMouseDown(args);

            args.Handled = false;

            MyRadioButton btn = args.Source as MyRadioButton;
            MyRadioButton btn2 = args.OriginalSource as MyRadioButton;

            if (btn != null)
            {
                if (buttonHighlighted != null)
                    buttonHighlighted.IsSelected = false;

                buttonHighlighted = btn;
                buttonHighlighted.IsSelected = true;
            }
            Focus();
        }

        protected override void OnMouseUp(MouseButtonEventArgs args)
        {
            base.OnMouseUp(args);
            MyRadioButton btn = args.Source as MyRadioButton;

            if (btn != null)
            {
                if (buttonSelected != null)
                    buttonSelected.IsSelected = false;

                buttonSelected = btn;
                buttonSelected.IsSelected = true;


                foreach(var b in ButtonList)
                {
                    b.Brush = ButtonBackGroundColor;
                }


                btn.Brush = SelectedColor;

                OnSelectedItemChanged(new RadioButtonStackPanelEventArgs(btn));
            }
        }

        private static void OnButtonListChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonStackPanel sp = o as RadioButtonStackPanel;

            var old = (ObservableCollection<MyRadioButton>)e.OldValue;
            if (old != null)
            {
                old.CollectionChanged -= sp.BackupItems_CollectionChanged;
            }
            ((ObservableCollection<MyRadioButton>)e.NewValue).CollectionChanged += sp.BackupItems_CollectionChanged;

        }

        private void BackupItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        private static void OnButtonCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonStackPanel sp = o as RadioButtonStackPanel;

            sp.stack.Children.RemoveRange(0, sp.ButtonCount);

            sp.listBtn = new List<MyRadioButton>();

            for (int i = 0; i < (int)e.NewValue; i++)
            {
                MyRadioButton btn = new MyRadioButton(i);
                btn.Text = (i + 1).ToString();
                btn.Brush = new SolidColorBrush(ROIColors.GetColor(i));
                btn.BorderBrush = Brushes.Black;
                btn.BorderThickness = new Thickness(2);

                sp.listBtn.Add(btn);
                sp.stack.Children.Add(btn);
            }
        }

        private static void OnButtonVisibleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            RadioButtonStackPanel sp = o as RadioButtonStackPanel;

            if (sp.IsVisibles != null)
            {
                for (int i = 0; i < sp.ButtonList.Count; i++)
                {
                    if (i >= sp.IsVisibles.Length)
                        continue;

                    sp.ButtonList[i].IsEnabled = sp.IsVisibles[i];
                }
            }
        }
        private static void OnSelectedButtonIndexChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //RadioButtonStackPanel sp = o as RadioButtonStackPanel;

            //if (sp.buttonSelected != null)
            //    sp.buttonSelected.IsSelected = false;

            //sp.buttonSelected = sp.SelectedButton[sp.SelectedIndex];
            //sp.buttonSelected.IsSelected = true;

        }
        

        // Protected method to fire SelectedColorChanged event.
        protected virtual void OnSelectedItemChanged(EventArgs args)
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, args);
        }


        public void AddButton(MyRadioButton btn)
        {
            var list = new ObservableCollection<MyRadioButton>();

            btn.Brush = ButtonBackGroundColor;

            foreach (var item in ButtonList)
            {
                list.Add(item);
            }
            list.Add(btn);

            ButtonList = list;
        }

        public void ButtonClear()
        {
            ButtonList.Clear();

            buttonSelected = null;
        }


        #endregion

        #region constructor
        public RadioButtonStackPanel()
        {
            bord = new Border();
            bord.BorderBrush = SystemColors.ControlDarkDarkBrush;
            bord.BorderThickness = new Thickness(1);
            AddVisualChild(bord);           // necessary for event routing.
            AddLogicalChild(bord);

            stack = new StackPanel();
            stack.Background = SystemColors.WindowBrush;

            bord.Child = stack;

            ButtonList = new ObservableCollection<MyRadioButton>();
        }

        static RadioButtonStackPanel()
        {
            ButtonCountProperty =
                DependencyProperty.Register("ButtonCount", typeof(int),
                        typeof(RadioButtonStackPanel), new FrameworkPropertyMetadata(0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure,
                                new PropertyChangedCallback(OnButtonCountChanged)));

            ButtonListProperty =
                    DependencyProperty.Register("ButtonList",
                    typeof(ObservableCollection<MyRadioButton>),
                    typeof(RadioButtonStackPanel), new FrameworkPropertyMetadata(new ObservableCollection<MyRadioButton>(),
                        FrameworkPropertyMetadataOptions.AffectsMeasure,
                                new PropertyChangedCallback(OnButtonListChanged)));

            IsVisiblesProperty =
                DependencyProperty.Register("IsVisibles", typeof(bool[]),
                typeof(RadioButtonStackPanel), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(OnButtonVisibleChanged)));


            //SelectedButtonIndexProperty =
            //    DependencyProperty.Register("SelectedButton", typeof(int),
            //    typeof(RadioButtonStackPanel), new FrameworkPropertyMetadata(0,
            //    FrameworkPropertyMetadataOptions.AffectsMeasure,
            //    new PropertyChangedCallback(OnSelectedButtonIndexChanged)));
        }
        #endregion
    }
}
