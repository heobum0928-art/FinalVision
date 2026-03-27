using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Project.UI.Common
{
    public static class ROIColors
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

        public static Color GetColor(int idx)
        {
            return (Color)typeof(Colors).GetProperty(strColors[idx]).GetValue(null, null);
        }
    }
}
