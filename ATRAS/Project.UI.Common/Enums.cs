using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public enum HitLocation
    {
        None,
        Body,

        Left,
        Top,
        Right,
        Bottom,

        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    }

    public enum ArrowDirect
    {
        None,

        Start,

        End,
    }

    public enum ImageSaveTypes
    {
        OnlyImage,
        WithShapes
    }
}
