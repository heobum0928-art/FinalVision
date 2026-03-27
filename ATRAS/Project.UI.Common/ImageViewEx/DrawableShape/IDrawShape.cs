using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public interface IDrawableShape
    {
        void Draw(Graphics gh);
    }
}
