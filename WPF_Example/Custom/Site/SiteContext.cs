using System.Collections.Generic;
using System.ComponentModel;

namespace FinalVisionProject.Site {
    public class SiteContext : INotifyPropertyChanged {
        public int SiteNumber { get; private set; }
        public string SiteName { get { return "Site" + SiteNumber; } }

        private string _currentRecipeName = "Default";
        public string CurrentRecipeName {
            get { return _currentRecipeName; }
            set {
                _currentRecipeName = value;
                RaisePropertyChanged("CurrentRecipeName");
            }
        }

        public SiteStatistics Statistics { get; private set; }

        private readonly Queue<bool> _resultHistory = new Queue<bool>();
        public const int MAX_HISTORY = 100;

        public SiteContext(int siteNumber) {
            SiteNumber = siteNumber;
            Statistics = new SiteStatistics();
        }

        public void AddResult(bool isOk) {
            Statistics.Add(isOk);
            if (_resultHistory.Count >= MAX_HISTORY)
                _resultHistory.Dequeue();
            _resultHistory.Enqueue(isOk);
        }

        public IEnumerable<bool> GetRecentResults() {
            return _resultHistory;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}
