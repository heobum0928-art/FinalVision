using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Enums
{
    public enum Severity
    {
        None = 0,
        Fatal = 1,
        Error = 2,
        Warning = 3,
        Informational = 4
    }

    public enum ResultStatus
    {
        Success = 0,
        Error = 1,

    }
    public enum Initialized
    {
        True,
        False
    }

    #region Status

    public enum ErrorSeverity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3,
        Restart = 4,
    }

    public enum UnitOnline
    {
        Offline,
        Online,
    }
    public enum OperationStatus
    {
        Failed,
        Succeeded,
    }

    public enum InitializationStates
    {
        NotReady = 0,       // Connect : No,  Initialize : No
        Ready = 1,          // Connect : Yes, Initialize : No
        Initialized = 2     // Connect : Yes, Initialize : Yes

    }
    public enum UnitStatus
    {
        Initialized,
        NotInitialized,
        Initializing,
        Error,
        Ready,
    }

    public enum RunMode
    {
        Run,
        DryRun,
    }

    public enum SequenceStatus
    {
        Run,
        Pause,
        Manual,
        Stop
    }
    #endregion

    public enum SystemStatus
    {
        Virtual,
        Real,
    }

    public enum ImageSaveModes
    {
        NO,
        OK,
        NG, 
        ALL,
    }

    public enum SystemTypes
    {
        Real,
        Simulation,
    }


    public enum ReadStatus
    {
        ReadOK = 0,
        ReadFailed = 1,
        NotAvailable = 2,
    }
}
