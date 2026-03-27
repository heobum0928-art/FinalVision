using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class DieMap : Control
    {
        protected const int maxRectSize = 300;
        protected ScrollViewer scViewer;
        public UniformGrid unigrid;
        Border border;

        protected bool IsCtrlKeyDown = false;
        protected Die DieSelected;
        protected Die DieHighlighted;

        protected double zoomFactor = 1;

        protected double scViewerLeft = 0;
        protected double scViewerTop = 0;

        protected Size BackgroundSize = Size.Empty;

        public int DieWidthCount
        {
            get { return (WaferSize == WaferSize._300mm ? 300000 : 200000) / (int)DieSize.Width; }
        }

        public Point CurrentPositionInDie;

        public int DieHeightCount
        {
            get { return (WaferSize == WaferSize._300mm ? 300000 : 200000) / (int)DieSize.Height; }
        }

        protected int DieAllCount
        {
            get { return DieWidthCount * DieHeightCount; }
        }

        protected bool IsMaxZoom
        {
            get
            {
                if(unigrid.Children.Count > 0)
                {
                    Die die = unigrid.Children[0] as Die;
                    if(die != null)
                    {
                        if (die.RectSize.Width > 200)
                            return true;
                        else
                            return false;
                    }
                    return false;
                }
                return false;
            }
        }

        public DieMap()
        {
            //Background = Brushes.DimGray;
            // Create a Border for the control.
            scViewer = new ScrollViewer();
            scViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scViewer.Background = Brushes.Transparent;
            AddVisualChild(scViewer);           // necessary for event routing.
            AddLogicalChild(scViewer);
            // Create a UniformGrid as a child of the Border.
            
            //
            border = new Border();
            scViewer.Content = border;
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = Brushes.Black;
            border.Background = Brushes.DimGray;         
            //
            unigrid = new UniformGrid();
            //unigrid.Background = Brushes.DimGray;

            //
            border.Child = unigrid;
            //Margin = new Thickness(5);

            KeyDown += DieMapOnKeyDown;
            KeyUp += DieMap_KeyUp;

            MouseWheel += Die_MouseWheel;
        }

        protected Size GetRectSize(Size sizeAvailable)
        {
            double length = Math.Min(sizeAvailable.Width, sizeAvailable.Height) * zoomFactor;
            return new Size(length / DieWidthCount, length / DieHeightCount);
        }
        protected void CreateDie()
        {
            if (unigrid.Children.Count != 0)
                unigrid.Children.Clear();

            int dieNumber = 0;

            unigrid.Columns = DieWidthCount;
            unigrid.Rows = DieHeightCount;
            for (int j = 0; j < DieHeightCount; j++)
            {
                for (int i = 0; i < DieWidthCount; i++)
                {
                    Die die = new Die();
                    unigrid.Children.Add(die);

                    die.ArrayX = i;
                    die.ArrayY = j;
                    if (DieEnableCheck(i, j) == true)
                    {
                        die.DieNumber = dieNumber++;
                        die.X = i; die.Y = j;
                        die.MouseWheel += Die_MouseWheel;
                        die.MouseMove += Die_MouseMove;

                        ToolTip tip = new ToolTip();
                        tip.Content = string.Format("Number = {0}, X = {1}, Y = {2}", die.DieNumber, die.X, die.Y);
                        die.ToolTip = tip;

                        die.RectSize = new Size(0, 0);
                    }
                    else
                        die.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Die_MouseMove(object sender, MouseEventArgs e)
        {
            Die die = sender as Die;
            if(die != null)
            {
                CurrentPositionInDie = e.GetPosition(die);
            }

        }

        protected bool DieEnableCheck(int dieX, int dieY)
        {
            double dieCX = WaferSize == WaferSize._300mm ? 150000 : 100000;
            double dieCY = WaferSize == WaferSize._300mm ? 150000 : 100000;

            double dL = dieX * DieSize.Width;
            double dT = dieY * DieSize.Height;
            double dR = dL + DieSize.Width;
            double dB = dT + DieSize.Height;

            double dLength = (300000 / 2.0) - WaferEdgeLoss;

            double dL1 = Math.Sqrt(Math.Pow(dieCX - dL, 2) + Math.Pow(dieCY - dT, 2)); // LT
            double dL2 = Math.Sqrt(Math.Pow(dieCX - dL, 2) + Math.Pow(dieCY - dB, 2)); // LB
            double dL3 = Math.Sqrt(Math.Pow(dieCX - dR, 2) + Math.Pow(dieCY - dB, 2)); // RB
            double dL4 = Math.Sqrt(Math.Pow(dieCX - dR, 2) + Math.Pow(dieCY - dT, 2)); // RT

            if (dL1 < dLength && dL2 < dLength && dL3 < dLength && dL4 < dLength)
                return true;

            return false;
        }
        
        protected void SetScrollCenter()
        {
            if(DieSelected != null)
            {
                Point center = DieSelected.GetDieCornerLT();

                center.X += CurrentPositionInDie.X;
                center.Y += CurrentPositionInDie.Y;

                scViewer.ScrollToHorizontalOffset(Math.Max(0, center.X - scViewer.ViewportWidth/2));
                scViewer.ScrollToVerticalOffset(Math.Max(0, center.Y - scViewer.ViewportHeight / 2));
                scViewer.InvalidateScrollInfo();
            }
        }

        public bool ZoomIn()
        {
            if (unigrid.Children.Count > 0)
            {
                Die die = unigrid.Children[0] as Die;
                if (die != null)
                {
                    double rectSize = die.ActualWidth;
                    if (rectSize < maxRectSize)
                    {                        
                        double firstSize = rectSize * (1 / zoomFactor);
                        if (firstSize == 0)
                            return false;

                        double dValue = maxRectSize / firstSize / 25.0;// firstSize;
                        ZoomFactor += dValue;
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }
        public bool ZoomOut()
        {
            Die die = unigrid.Children[0] as Die;
            if (die != null)
            {
                double firstSize = die.ActualWidth * (1 / zoomFactor);

                double rectSize = die.ActualWidth;
                var val = Math.Round(rectSize, 10);

                if (firstSize < val)
                {
                    double dValue = maxRectSize / firstSize / 25.0;
                    ZoomFactor -= dValue;
                    return true;
                }
                return false;
            }
            return false; 
        }

        #region ## DependencyProperty

        public static DependencyProperty DieSizeProperty;
        public static DependencyProperty WaferSizeProperty;
        public static DependencyProperty WaferTypeProperty;
        public static DependencyProperty ZoomFactorProperty;
        public static DependencyProperty WaferEdgeLossProperty;

        static DieMap()
        {
            DieSizeProperty =
                DependencyProperty.Register("DieSize", typeof(Size),
                    typeof(DieMap), new FrameworkPropertyMetadata(new Size(10000, 10000),
                            FrameworkPropertyMetadataOptions.AffectsRender, OnDieSizeChanged));
            
            WaferSizeProperty =
                DependencyProperty.Register("WaferSize", typeof(WaferSize),
                    typeof(DieMap), new FrameworkPropertyMetadata(WaferSize._300mm,
                            FrameworkPropertyMetadataOptions.AffectsRender));

            WaferTypeProperty =
                DependencyProperty.Register("WaferType", typeof(WaferType),
                    typeof(DieMap), new FrameworkPropertyMetadata(WaferType.Notch,
                            FrameworkPropertyMetadataOptions.AffectsRender));

            ZoomFactorProperty =
                DependencyProperty.Register("ZoomFactor", typeof(double),
                    typeof(DieMap), new FrameworkPropertyMetadata(1.0,
                            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

            WaferEdgeLossProperty =
                DependencyProperty.Register("WaferEdgeLoss", typeof(int),
                    typeof(DieMap), new FrameworkPropertyMetadata(5000,
                            FrameworkPropertyMetadataOptions.AffectsRender, OnDieSizeChanged));
        }

        static void OnDieSizeChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            DieMap diemap = obj as DieMap;
            diemap.CreateDie();
        }

        public Size DieSize
        {
            set { SetValue(DieSizeProperty, value); }
            get { return (Size)GetValue(DieSizeProperty); }
        }

        public WaferSize WaferSize
        {
            set { SetValue(WaferSizeProperty, value); }
            get { return (WaferSize)GetValue(WaferSizeProperty); }
        }

        public WaferType WaferType
        {
            set { SetValue(WaferTypeProperty, value); }
            get { return (WaferType)GetValue(WaferTypeProperty); }
        }

        public double ZoomFactor
        {
            set
            {
                double val = Math.Max(1, value);
                if (zoomFactor != val)
                {
                    zoomFactor = val;
                    SetValue(ZoomFactorProperty, val);
                }

            }
            get { return (double)GetValue(ZoomFactorProperty); }
        }

        public int WaferEdgeLoss
        {
            set { SetValue(WaferEdgeLossProperty, value); }
            get { return (int)GetValue(WaferEdgeLossProperty); }
        }
        #endregion

        #region ## UI Override
        // Override of VisualChildrenCount.
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        // Override of GetVisualChild.
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0)
                throw new ArgumentOutOfRangeException("index");

            return scViewer;
        }

        protected override Size MeasureOverride(Size sizeAvailable)
        {
            //scViewer.Measure(sizeAvailable);

            Size sizeViewer = new Size(sizeAvailable.Width * zoomFactor, sizeAvailable.Height * zoomFactor);

            double length = Math.Min(sizeViewer.Width, sizeViewer.Height);// * zoomFactor;
            Size rectSize = new Size(length / DieWidthCount, length / DieHeightCount);
            
            //
            foreach (var die in unigrid.Children)
            {
                Die dd = (die as Die);
                if (dd != null)
                    dd.RectSize = rectSize;
            }

            //
            //if(!Double.IsInfinity(length))
            {
                border.Width = length;
                border.Height = length;
                border.BorderThickness = new Thickness(5);
                border.CornerRadius = new CornerRadius(length / 2);
            }

            scViewer.Measure(sizeAvailable);
            SetScrollCenter();


            return scViewer.DesiredSize;
        }
        protected override Size ArrangeOverride(Size sizeFinal)
        {
            scViewerLeft = (sizeFinal.Width - scViewer.DesiredSize.Width) / 2;
            scViewerTop = (sizeFinal.Height - scViewer.DesiredSize.Height) / 2;
            scViewer.Arrange(new Rect(new Point(scViewerLeft, scViewerTop), scViewer.DesiredSize));

            return sizeFinal;
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(Background, new Pen(Brushes.Black, 1), new Rect(RenderSize));

            Size size = RenderSize;
        }


        #endregion

        #region ## Mouse Event
        private void DieMap_KeyUp(object sender, KeyEventArgs e)
        {
            IsCtrlKeyDown = false;
        }

        private void DieMapOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                IsCtrlKeyDown = true;
            }
        }

        private void Die_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsCtrlKeyDown)
            {
                if (e.Delta > 0)
                {
                    //ZoomFactor = IsMaxZoom == true ? ZoomFactor : ZoomFactor += 0.2;
                    //ZoomFactor = Math.Min(5, ZoomFactor += 0.1);
                    ZoomIn();
                }
                else
                    ZoomOut();
                    //ZoomFactor = Math.Max(1, ZoomFactor -= 0.2);
            }
        }

        protected Point currentMousePoint;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Die die = e.Source as Die;
            currentMousePoint = e.GetPosition(scViewer);
            if (die != null)
            {
                if (DieHighlighted != null)
                    DieHighlighted.IsHighlighted = false;

                DieHighlighted = die;
                DieHighlighted.IsHighlighted = true;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Die die = e.Source as Die;
            if (die != null)
                die.CaptureMouse();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            Die die = e.Source as Die;

            if (die != null)
            {
                if (DieSelected != null)
                    DieSelected.IsSelected = false;

                DieSelected = die;
                DieSelected.IsSelected = true;
                DieSelected.Focus();

                ReleaseMouseCapture();
            }
        }
        #endregion

    }
}
