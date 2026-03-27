
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class HIKGrabber : GrabberBase
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        public override bool Close()
        {
            throw new NotImplementedException();
        }

        public override bool GrabOnce(int idx)
        {
            throw new NotImplementedException();
        }

        public override bool GrabStart(int idx)
        {
            throw new NotImplementedException();
        }

        public override bool Open()
        {
            throw new NotImplementedException();
        }

        public override bool Reset()
        {
            throw new NotImplementedException();
        }

        public override bool GrabStop(int idx)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region constructors
        public HIKGrabber()
            : base(GrabberTypes.HIK)
        {

        }
        #endregion

    }
}
