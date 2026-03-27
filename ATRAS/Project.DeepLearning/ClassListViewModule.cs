using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DeepLearning.UI
{
    public class ClassListViewModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ClassListView", typeof(ClassListView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
