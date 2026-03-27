using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class Die : FrameworkElement
    {
        protected int dieNumber;

        protected const int rectFlat = 2;

        // Dependency properties.
        public static readonly DependencyProperty RectSizeProperty;
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty IsHighlightedProperty;

        static Die()
        {
            IsSelectedProperty =
                DependencyProperty.Register("IsSelected", typeof(bool),
                        typeof(Die), new FrameworkPropertyMetadata(false,
                                FrameworkPropertyMetadataOptions.AffectsRender));

            IsHighlightedProperty =
                DependencyProperty.Register("IsHighlighted", typeof(bool),
                        typeof(Die), new FrameworkPropertyMetadata(false,
                                FrameworkPropertyMetadataOptions.AffectsRender));

            RectSizeProperty =
                DependencyProperty.Register("RectSize", typeof(Size),
                        typeof(Die), new FrameworkPropertyMetadata(new Size(0, 0),
                                FrameworkPropertyMetadataOptions.AffectsMeasure));
        }
        // Properties.
        public bool IsSelected
        {
            set { SetValue(IsSelectedProperty, value); }
            get { return (bool)GetValue(IsSelectedProperty); }
        }
        public bool IsHighlighted
        {
            set { SetValue(IsHighlightedProperty, value); }
            get { return (bool)GetValue(IsHighlightedProperty); }
        }
        public Size RectSize
        {
            set { SetValue(RectSizeProperty, value); }
            get { return (Size)GetValue(RectSizeProperty); }
        }

        public int DieNumber { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int ArrayX { get; set; }
        public int ArrayY { get; set; }

        public Die()
        {
            dieNumber = 0;
        }
        public Die(Size size)
        {
            dieNumber = 0;
            RectSize = size;
        }

        protected override Size MeasureOverride(Size sizeAvailable)
        {
            return RectSize;
        }
        protected override void OnRender(DrawingContext dc)
        {
            Rect rect = new Rect(new Point(0, 0), RenderSize);
            Pen pen;
            int size = rectFlat * -1;
            if (IsHighlighted)
            {
                rect.Inflate(size + 1, size + 1);
                pen = new Pen(SystemColors.HighlightBrush, rectFlat);
            }
            else if (IsSelected)
            {
                rect.Inflate(size, size);
                pen = new Pen(Brushes.White, rectFlat);
            }
            else
            {
                pen = new Pen(Brushes.White, 1);
            }
            dc.DrawRectangle(Brushes.DimGray, pen, rect);
            //Pen pen2 = new Pen(Brushes.Black, 3);
            //dc.DrawRectangle(Brushes.Transparent, pen2, rect);
        }

        public Point GetDieCornerLT()
        {
            return new Point(RectSize.Width * ArrayX, RectSize.Height * ArrayY);
        }
    }
}
