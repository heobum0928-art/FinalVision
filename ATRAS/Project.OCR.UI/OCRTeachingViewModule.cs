using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.OCR.UI
{
    public class OCRTeachingViewModule : IModule
    {
        #region fields

        #endregion

        #region propertise
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("OCRTeachingView", typeof(OCRTeachingView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
        #endregion

        #region methods

        #endregion

        #region constructors

        #endregion

    }
}
