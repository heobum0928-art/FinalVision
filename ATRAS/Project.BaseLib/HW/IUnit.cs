using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public interface IUnit
    {
        Task<bool> SWInitialize(HWConfigBase hwConfigBase);
        Task<bool> HWInitialize(bool forceInitialization);

        //
        void Reset();
        void Abort();
        void Shutdown();
    }
}
