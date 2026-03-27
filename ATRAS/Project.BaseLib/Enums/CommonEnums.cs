using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Enums
{
    public enum CommunicationTypes
    {
        NotUsed = 0,
        Tcp = 1,
        Serial = 2,
    }
    public enum DeviceSerialPort
    {
        NotUsed = 0,
        COM0,
        COM1,
        COM2,
        COM3,
        COM4,
        COM5,
        COM6,
        COM7,
        COM8,
        COM9,
        COM10,
        COM11,
        COM12,
        COM13,
        COM14,
        COM15,
        COM16,
        COM17,
        COM18,
        COM19,
    }
    public enum ParityBits
    {
        NOPARITY,
        EVEN,
        ODD
    }
    public enum HWModes
    {
        None, // NotUsed
        Real, // Device
        Simulator, // Simulator
    }
    public enum SwitchStates
    {
        Unknown,
        On,
        Off,
    }
    public enum ROIDirections
    {
        Horizontal,
        Vertical
    }

    public enum ROITypes
    {
        Unknown,
        Measure_Horz,
        Measure_Vert,
        Detect_Black,
        Detect_White,
        
    }

    public enum Axis
    {
        Horizontal,
        Vertical
    }

    public enum DistanceDirections
    {
        Width,
        Height,
        Diag
    }

    public enum BlobTypes
    {
        White,
        Black
    }

    public enum BinTypes
    {
        OutSide,
        InSide
    }
    public enum Bits
    {
        b0  = 0,
        b1  = 1,
        b2  = 1 << 1,
        b3  = 1 << 2,
        b4  = 1 << 3,
        b5  = 1 << 4,
        b6  = 1 << 5,
        b7  = 1 << 6,
        b8  = 1 << 7,
        b9  = 1 << 8,
        b10 = 1 << 9,
        b11 = 1 << 10,
        b12 = 1 << 11,
        b13 = 1 << 12,
        b14 = 1 << 13,
        b15 = 1 << 14,
        b16 = 1 << 15,
        b17 = 1 << 16,
        b18 = 1 << 17,
        b19 = 1 << 18,
        b20 = 1 << 19,
        b21 = 1 << 20,
        b22 = 1 << 21,
        b23 = 1 << 22,
        b24 = 1 << 23,
        b25 = 1 << 24,
        b26 = 1 << 25,
        b27 = 1 << 26,
        b28 = 1 << 27,
        b29 = 1 << 28,
        b30 = 1 << 29,
        b31 = 1 << 30,
    }

    public enum RotateTypes
    {
        Crop,
        Expand
    }

    public enum ResizeTypes
    {
        Bin,
        Gray,
    }
}
