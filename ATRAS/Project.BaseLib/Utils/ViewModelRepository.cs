using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class ViewModelRepository : Singleton<ViewModelRepository>
    {
        #region fields

        protected ILogger logger;

        protected Dictionary<string, object> _viewModels = null;
        #endregion

        #region propertiese
        public Dictionary<string, object> ViewModels
        {
            get { return _viewModels; }
        }
        #endregion

        #region methods

        public void SetViewModel<M>(object instance)
        {
            string name = typeof(M).Name;

            if (instance == null)
                throw new ArgumentNullException(name);

            if (!_viewModels.ContainsKey(name))
            {
                _viewModels[name] = instance;
            }
        }


        public M GetViewModel<M>() where M : INotifyPropertyChanged, new()
        {
            string name = typeof(M).Name;
            if (!_viewModels.ContainsKey(name))
            {
                _viewModels[name] = new M();
                //return (M)_viewModels[name];
            }
            //return default(M);

            return (M)_viewModels[name];
        }


        public NotifyPropertyChanged GetViewModel(string modelname)
        {
            foreach (var viewModel in _viewModels.Values)
            {
                string name = viewModel.GetType().Name;

                if (modelname == name)
                    return (NotifyPropertyChanged)viewModel;

            }
            return null;
        }

        #endregion

        #region constructor
        protected ViewModelRepository()
        {
            string loggerName = this.GetType().Name;
            LogManager.InitializeLogger(false);
            this.logger = LogManager.GetLogger(loggerName);
            //CreateModelView();

            _viewModels = new Dictionary<string, object>();
        }


        #endregion

    }
}
