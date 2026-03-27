using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    
    public enum ExposureModes
    {
        Timed,
        TriggerWidth,
    }
    public enum TriggerModes
    {
        Internal,
        External,
    }
    public enum Imagers
    {
        
        Cam1,
        Cam2,
        Cam3,
        Cam4,
        Cam5,
        Cam6,
        Cam7,
        Cam8,
        Max,
        Unknown,
    }
    public enum GrabberTypes
    {
        Simulator,
        Matrox_PCI,
        Matrox_GigE,
        HIK,
        Vieworks,
    }

    public enum GrabTypes
    {
        Unknown,
        OnceGrab,
        ContinusGrab
    }
}
