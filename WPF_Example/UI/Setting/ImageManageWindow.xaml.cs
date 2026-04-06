//260406 hbk -- IMG-04: 이미지 관리 창 코드비하인드 (D-09, D-10, D-11, D-12)
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using FinalVisionProject.Setting;
using FinalVisionProject.Utility;

namespace FinalVisionProject.UI {
    /// <summary>
    /// ImageManageWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageManageWindow : Window {

        public ImageManageWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LoadDateFolders();
        }

        //260406 hbk -- 폴더 목록 로드
        private void LoadDateFolders() {
            var folders = SystemSetting.Handle.GetImageDateFolders();
            var items = new ObservableCollection<DateFolderItem>();
            foreach (var f in folders) {
                items.Add(new DateFolderItem { FullPath = f, Name = Path.GetFileName(f), IsChecked = false });
            }
            lb_dateFolders.ItemsSource = items;
        }

        private void Btn_RefreshFolders_Click(object sender, RoutedEventArgs e) {
            LoadDateFolders();
        }

        //260406 hbk -- D-11, D-12: 선택된 폴더 삭제
        private void Btn_DeleteFolders_Click(object sender, RoutedEventArgs e) {
            var items = lb_dateFolders.ItemsSource as ObservableCollection<DateFolderItem>;
            if (items == null) return;
            var selected = items.Where(x => x.IsChecked).ToList();
            if (selected.Count == 0) {
                CustomMessageBox.Show("알림", "삭제할 폴더를 선택하세요.", MessageBoxImage.Information);
                return;
            }

            //260406 hbk -- D-11: 삭제 전 확인 다이얼로그
            string names = string.Join(", ", selected.Select(x => x.Name));
            var result = CustomMessageBox.ShowConfirmation(
                "폴더 삭제 확인",
                string.Format("선택된 {0}개 폴더를 삭제합니까?\n({1})\n\n하위 파일 모두 삭제됩니다.", selected.Count, names),
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            int deleted = 0;
            foreach (var item in selected) {
                try {
                    Directory.Delete(item.FullPath, true);   //260406 hbk -- D-12: recursive 삭제
                    deleted++;
                } catch (Exception ex) {
                    CustomMessageBox.Show("삭제 실패", string.Format("{0}: {1}", item.Name, ex.Message), MessageBoxImage.Error);
                }
            }

            if (deleted > 0) {
                Logging.PrintLog((int)ELogType.Trace, "[IMG] 폴더 {0}개 삭제 완료", deleted);
                LoadDateFolders();   //260406 hbk -- 목록 새로고침
            }
        }
    }

    //260406 hbk -- IMG-04: 날짜 폴더 목록용 뷰모델
    public class DateFolderItem : System.ComponentModel.INotifyPropertyChanged {
        public string FullPath { get; set; }
        public string Name { get; set; }
        private bool _isChecked;
        public bool IsChecked {
            get => _isChecked;
            set {
                _isChecked = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsChecked)));
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
