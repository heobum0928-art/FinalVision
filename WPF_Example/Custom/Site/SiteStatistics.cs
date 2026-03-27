using System.ComponentModel;

namespace FinalVisionProject.Site {
    public class SiteStatistics : INotifyPropertyChanged {
        private readonly object _lock = new object();
        private int _totalCount;
        private int _okCount;
        private int _ngCount;

        public int TotalCount { get { lock (_lock) { return _totalCount; } } }
        public int OkCount   { get { lock (_lock) { return _okCount; } } }
        public int NgCount   { get { lock (_lock) { return _ngCount; } } }
        public double Yield  {
            get {
                lock (_lock) {
                    return _totalCount == 0 ? 0.0 : (double)_okCount / _totalCount * 100.0;
                }
            }
        }

        public void Add(bool isOk) {
            lock (_lock) {
                _totalCount++;
                if (isOk) _okCount++;
                else _ngCount++;
            }
            RaisePropertyChanged("TotalCount");
            RaisePropertyChanged("OkCount");
            RaisePropertyChanged("NgCount");
            RaisePropertyChanged("Yield");
        }

        public void Reset() {
            lock (_lock) {
                _totalCount = 0;
                _okCount = 0;
                _ngCount = 0;
            }
            RaisePropertyChanged("TotalCount");
            RaisePropertyChanged("OkCount");
            RaisePropertyChanged("NgCount");
            RaisePropertyChanged("Yield");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
