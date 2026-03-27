using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public interface IGrabber
    {
        string Name();
        bool Initialize();

        bool Open();

        bool Close();

        bool Reset();

        bool Grab(int idx);
    }
}
