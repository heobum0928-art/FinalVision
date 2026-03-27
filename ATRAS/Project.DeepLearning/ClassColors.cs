using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Project.DeepLearning.UI
{

    public static class ClassColors
    {
        public static string[] strColors = new string[]
        {
            "Brown", "DarkGreen", "MidnightBlue", "DimGray", "Olive",
            "Teal", "Blue", "SlateGray",
            "Red", "Orange", "YellowGreen",
            "Aqua", "LightBlue", "Violet",
            "Pink", "Gold", "Yellow", "Lime",
            "SkyBlue", "Plum", "LightGray",
            "LightPink", "Tan", "LightYellow", "LightGreen",
            "LightCyan", "LightSkyBlue", "Lavender", "White"
        };

        public static string[] strTexts = new string[]
        {
            "Red", "Orange", "Brown", "Green", "MediumBlue", "Purple", "DimGray", "Gray", "Black", "Orchid", "Turquoise", "Cyan"
        };

        public static string[] strTextBackground = new string[]
        {
            "Pink", "Tomato", "Brown", "LimeGreen", "Blue", "Lavender", "DimGray", "Gray", "Black", "Orchid", "Turquoise", "Cyan"
        };

        public static string[] strBrushs = new string[]
        {
            "Brown", "DarkGreen", "MidnightBlue", "DimGray", "Olive",
            "Teal", "Blue", "SlateGray",
            "Red", "Orange", "YellowGreen",
            "Aqua", "LightBlue", "Violet",
            "Pink", "Gold", "Yellow", "Lime",
            "SkyBlue", "Plum", "LightGray",
            "LightPink", "Tan", "LightYellow", "LightGreen",
            "LightCyan", "LightSkyBlue", "Lavender", "White"
        };

        public static Color GetColor(int idx)
        {
            return (Color)typeof(Colors).GetProperty(strColors[idx]).GetValue(null, null);
        }

        public static Color GetColor()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int idx = rand.Next(0, strColors.Length - 1);

            return (Color)typeof(Colors).GetProperty(strColors[idx]).GetValue(null, null);
        }

        public static Brush GetTextBrush(int idx)
        {
            //Random rand = new Random();
            //int 
            return (Brush)typeof(Brushes).GetProperty(strTexts[idx]).GetValue(null, null);
        }
        public static Brush GetTextBrush()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int idx = rand.Next(0, strTexts.Length - 1);
            return (Brush)typeof(Brushes).GetProperty(strTexts[idx]).GetValue(null, null);
        }


        public static Brush GetTextBackgroundBrush(int idx)
        {
            return (Brush)typeof(Brushes).GetProperty(strTextBackground[idx]).GetValue(null, null);
        }

        public static Brush GetTextBackgroundBrush()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int idx = rand.Next(0, strTexts.Length - 1);
            return (Brush)typeof(Brushes).GetProperty(strTextBackground[idx]).GetValue(null, null);
        }
    }
}
