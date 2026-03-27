namespace FinalVisionProject.Site {
    public class SiteManager {
        public const int SITE_COUNT = 5;

        public static SiteManager Handle { get; } = new SiteManager();

        private readonly SiteContext[] _sites = new SiteContext[SITE_COUNT];
        private int _currentSiteIndex = 0;  // 0-based (Site1=0 ... Site5=4)

        public int CurrentSiteIndex { get { return _currentSiteIndex; } }
        public SiteContext CurrentSite { get { return _sites[_currentSiteIndex]; } }
        public SiteContext this[int siteIndex] { get { return _sites[siteIndex]; } }

        private SiteManager() {
            for (int i = 0; i < SITE_COUNT; i++) {
                _sites[i] = new SiteContext(i + 1);  // siteNumber = 1~5
            }
        }

        /// <summary>
        /// 현재 Site를 전환한다. siteNumber는 1~5.
        /// 범위 밖이면 false 반환.
        /// </summary>
        public bool SwitchSite(int siteNumber) {
            int idx = siteNumber - 1;
            if (idx < 0 || idx >= SITE_COUNT) return false;
            _currentSiteIndex = idx;
            return true;
        }
    }
}
