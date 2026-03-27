using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Enums
{
    public enum DeviceTypes
    {
        NotUsed = 0,
        HWS = 1,
        MXComponent_X64 = 2,
        Matrox = 3,
        Vieworks = 4,
        NVRServer_HIK=5,

        LoadPort_TIS = 6,  
        PreAligner_TIS = 7,
        WTR_TIS = 8,
        Crevis_IO = 9,
    }

    public enum HWDirections
    {
        CW,
        CCW,
        Unknown
    }

    public enum HomeStatues
    {
        Complete,
        Uncomplete,
        Unknown
    }
    public enum LightStates
    {
        Unknown = 0,
        Off = 1,
        On = 2,
        Flash = 3,
        FlashFast = 4,
        FlashSlow = 5,
    }
    public enum BuzzerStates
    {
        Unknown = 0,
        Off = 1,
        Fast = 2,
        Normal = 3,
        Slow = 4,
    }

    public enum LightTowerIDs
    {
        Unknown = 0,
        Light1 = 1,
        Light2 = 2,
        Light3 = 3,
        Light4 = 4,
        All = 5,
    }

    public enum LightCurtainStates
    {
        Unknown = 0,
        Block = 1,
        NoneBlock = 2,
    }

    public enum IOStates
    {
        Unknown = 0,
        On = 1,
        Off = 2,
    }


    public enum SubstrateSizes
    {
        Unknown = 0,
        TwoHundredMM = 1,
        ThreeHundredMM = 2,
        FourHundredFiftyMM = 3,
    }

    public enum RobotArmIDs
    {
        Arm1 = 1,
        Arm2 = 2,
        //Arm3,
        //Arm4,
        //Arm5,
        //Arm6,
        //Arm7,
        //Arm8,
        //Arm9,
        //Arm10,
    }

    public enum ContourType
    {
        Flat = 0,
        Notch = 1,
        Default = 2,
    }

    public enum SubstrateOrientations
    {
        Right = 0,
        Up = 90,
        Left = 180,
        Down = 270,
    }

    public enum SlotStates
    {
        Undefined = 0,
        Empty = 1,
        NotEmpty = 2,
        Correct = 3,
        DoubleSlotted = 4,
        CrossSlotted = 5,
        ProcessDone = 10,
    }

    public enum ClampStates
    {
        Undefined = 0,
        Unclamped = 1,
        Clamped = 2,
    }

    public enum DoorStates
    {
        Undefined = 0,
        Closed = 1,
        Opened = 2,
        Opening = 3,
        Closing = 4,
    }

    public enum DockStates
    {
        Undefined = 0,
        Undocked = 1,
        Docked = 2,
    }

    public enum CarrierStates
    {
        Undefined = 0,
        Empty = 1,
        Removed = 2,
        Presence = 3,
        Placed = 4,
        Clamped = 5,
        Loading = 6,
        Unloading = 7,
        Docked = 8,
        Opening = 9,
        Closing = 10,
        Loaded = 11,
    }

    public enum LoadPortStates
    {
        Undefined = 0,
        CarrierPresent = 1,
        CarrierPlaced = 2,
        AutoMode = 3,
        ManualMode = 4,
        LoadReady = 5,
        UnloadReady = 6,
        Error = 7,
        Reserved = 8,
    }
    public enum SubstratePresences
    {
        Unknown = 0,
        NoPresence = 1,
        Presence = 2,
    }

    public enum RobotSpeeds
    {
        Normal = 0,
        Slow = 1,
    }

    public enum RobotStations
    {
        LoadPort1 = 1,
        LoadPort2 = 2,
        LoadPort3 = 3,
        LoadPort4 = 4,
        LoadPort5 = 5,
        LoadPort6 = 6,
        LoadPort7 = 7,
        LoadPort8 = 8,
        LoadPort9 = 9,
        LoadPort10 = 10,

        Prealigner1 = 11,
        Prealigner2 = 12,
        Prealigner3 = 13,
        Prealigner4 = 14,
        Prealigner5 = 15,
        Prealigner6 = 16,
        Prealigner7 = 17,
        Prealigner8 = 18,
        Prealigner9 = 19,
        Prealigner10 = 20,

        Chamber1 = 21,
        Chamber2 = 22,
        Chamber3 = 23,
        Chamber4 = 24,
        Chamber5 = 25,
        Chamber6 = 26,
        Chamber7 = 27,
        Chamber8 = 28,
        Chamber9 = 29,
        Chamber10 = 30,
    };

    public enum LEDLampColors
    {
        Unused_or_Off,
        Red,
        Green,
        Yellow,
        Blue,
        Magenta,
        Cyan,
        White,
        Orange,
    }

    public enum EventCodes
    {
        None,
        PaddleNoText,
        PaddleDown,

        // Fixload6M Events.. ,
        CarrierPlaced,
        CarrierRemoved,
        CarrierPresent,
        CarrierNotPresent,
        Button1Pressed,
        Button1Released,
        Button2Pressed,
        Button2Released,
        Button3Pressed,
        Button3Released,
        Button4Pressed,
        Button4Released,
        DoorUp1,
        DoorDown1,
        TrayDocked1,

        ReadCarrierID,

        //Level Event Codes
        // E85 PIO Control
        HandshakeActive,
        HandoffActive,
        HandoffCompleted,
        HandshakeReleased,
        //HandshakeCanceled,
        HandshakeError,
        HandshakeErrorCleared,

        //
        AMHSPresentToLoad,
        AMHSPresentToUnload,
        //AMHSPresentCanceled,
        AMHSErrorSet,
        AMHSErrorCleared,
        //AMHSCarrierPlaced,
        //AMHSCarrierRemoved,

        //
        LoadTransferCompleted,
        UnloadTransferCompleted,

        LightCurtainBlocked,
        LightCurtainNoneBlocked,
        //
        LoadButtonPressed,
        UnloadButtonPressed,
        PrepareLoadButtonPressed,
        PrepareUnloadButtonPressed,
        //
        CarrierClamped,
        CarrierUnclamped,
        CarrierDocked,
        CarrierUndocked,
        DoorOpened,
        DoorClosed,
        CarrierMapped,
        SlotStateChanged,

        CarrierDoorOpened,
        CarrierDoorClosed
    }

    public enum InterlockStates
    {
        Unknown = 0,
        Alarm = 1,
        Free = 2,
    }

    public enum InterlockIDs
    {
        Unknown = 0,
        Door = 1,
        FIDoor = 2,
        ImagerCover = 3,
        Vacuum = 4,
        Airline = 5,
        DrawerFan = 6,
        ImagerFan = 7,
        FIFFU = 8,
        MainFFU = 9,
        XYLoadPosition = 10,
        PinsEnable = 11,
        LightCurtain = 12,
        LowWaterFlow = 13,
        WaterLeak = 14,
        IsolatorLevel = 15,
        FPAFan1 = 16,
        FPAFan2 = 17,
        SmokeDetector1 = 18,
        SmokeDetector2 = 19,
        SmokeDetector3 = 20,
        SmokeDetector4 = 21,
        FPALaserAlarm = 22,
        ChillerAlarm = 23,
    }

    public enum RobotArmTypes
    {
        ArmA,
        ArmB,
    }

    public enum ArmActionTypes
    {
        Load,
        Unload,
        Both,
    }

}
