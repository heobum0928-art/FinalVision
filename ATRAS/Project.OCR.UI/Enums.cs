using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.OCR.UI
{
    public enum ThresholdTypes
    {
        Absolute,
        Relative
    }

    public enum CharacterPolarity
    {
        DarkOnLight,
        LightOnDark
    }


    public enum CharacterTypes
    {
        Number = 0,
        UpperCase = 1,
        LowerCase = 2,
        SpecialCase = 3,
    }

    public enum TestImageTypes
    {
        Binary,
        Gray,
    }
}
