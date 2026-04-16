
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PropertyTools.Wpf;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Sequence;
using FinalVisionProject.Setting;
using FinalVisionProject.Utility;

namespace FinalVisionProject.UI {
    /// <summary>
    /// InspectionListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InspectionListView : UserControl {
        private MainWindow mParentWindow;
        private InspectionListViewModel ViewModel;
        public ParamBase SelectedParam { get; private set; } = null;
        private ParamBase CopiedParam = null;
        
        public InspectionListView() {
            InitializeComponent();
            ViewModel = new InspectionListViewModel();
            this.DataContext = ViewModel;
        }

        private bool _IsEditable = false;
        public bool IsEditable {
            get {
                return _IsEditable;
            }
            set {
                if (value) {
                    //treelistview의 seuqnce 항목을 펼친다. or 자식 항목 표시
                    ViewModel.RootModel.ExpandAll();
                    grid_editor.Visibility = Visibility.Visible;
                    gridSplitter_editor.Visibility = Visibility.Visible;
                    colDefinition_editor.Width = new GridLength(6, GridUnitType.Star);
                }
                else {
                    grid_editor.Visibility = Visibility.Collapsed;
                    gridSplitter_editor.Visibility = Visibility.Collapsed;
                    colDefinition_editor.Width = new GridLength(0, GridUnitType.Star);
                    CopiedParam = null;
                    button_paste.IsEnabled = false;
                }

                ParamEditor.IsEnabled = value;
                btn_RecipeSelect.IsEnabled = value;
                _IsEditable = value;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e) {
            mParentWindow = (MainWindow)Window.GetWindow(this);
        }

        private void Btn_RecipeSelect_Click(object sender, RoutedEventArgs e) {
            ContextMenu cm = this.FindResource("menu_control") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e) {
            mParentWindow.SaveRecipe(tb_RecipeName.Text);
        }

        private void MenuItem_Save_As_Click(object sender, RoutedEventArgs e) {
            string curRecipe = SystemHandler.Handle.Setting.CurrentRecipeName;
            bool dlgResult = TextInputBox.Show("Enter the name of new recipe to copy.", curRecipe, out string inputText);
            if (dlgResult == false) {
                return;
            }
            string newName = inputText;
            if (newName == curRecipe) {
                CustomMessageBox.Show("Error", "Recipe name to be copied must be different.", MessageBoxImage.Error);
                return;
            }
            //260403 hbk — D-06: RecipeSavePath 루트 기준 복사, D-08: 덮어쓰기 시 forceCopy=true
            if (RecipeFiles.Handle.HasRecipe(newName)) {
                if (CustomMessageBox.ShowConfirmation(newName + " Has Already Exists.", "Are you sure you want to overwrite the existing directory?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                    return;
                }
                RecipeFiles.Handle.Copy(curRecipe, newName, forceCopy: true);
            }
            else {
                RecipeFiles.Handle.Copy(curRecipe, newName);
            }
            SystemHandler.Handle.Recipes.CollectRecipe();   //260403 hbk — D-11: RecipeSavePath 루트 기준
        }

        private void MenuItem_open_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Recipe);
        }

        public void OnLoadRecipe(string name) {
            ViewModel.CurrentRecipe = name;
            NodeViewModel root = treeListBox_sequence.Items[0] as NodeViewModel;
            root.Name = name;
        }

        private void Btn_start_Click(object sender, RoutedEventArgs e) {
            if (treeListBox_sequence.SelectedIndex < 0) return;

            //260410 hbk -- SimulImagePath가 설정된 Shot: 카메라 촬상 없이 RunBlobOnLastGrab 검사
            if (treeListBox_sequence.SelectedItem is NodeViewModel selNode
                && selNode.SequenceID == ESequence.Inspection) {

                SequenceBase inspSeq = SystemHandler.Handle.Sequences[ESequence.Inspection];
                if (inspSeq != null) {

                    //260410 hbk -- Action 선택: 해당 Shot 1개만 검사
                    if (selNode.NodeType == ENodeType.Action) {
                        int actIndex = -1;
                        for (int i = 0; i < inspSeq.ActionCount; i++) {
                            if (inspSeq[i].ID == selNode.ActionID) { actIndex = i; break; }
                        }
                        if (actIndex >= 0 && inspSeq[actIndex] is Action_Inspection act) {
                            InspectionParam p = act.Param as InspectionParam;
                            if (p != null && !string.IsNullOrEmpty(p.SimulImagePath)
                                && File.Exists(p.SimulImagePath)) {
                                if (!SystemHandler.Handle.Sequences.IsIdle) return;
                                act.RunBlobOnLastGrab();
                                mParentWindow.mainView.RefreshShotImage(actIndex);
                                return;
                            }
                        }
                    }

                    //260410 hbk -- Sequence 선택: SimulImagePath가 있는 Shot 전부 검사
                    if (selNode.NodeType == ENodeType.Sequence) {
                        int ranCount = 0;
                        for (int i = 0; i < inspSeq.ActionCount; i++) {
                            if (!(inspSeq[i] is Action_Inspection act)) continue;
                            InspectionParam p = act.Param as InspectionParam;
                            if (p == null || string.IsNullOrEmpty(p.SimulImagePath)
                                || !File.Exists(p.SimulImagePath)) continue;
                            ranCount++;
                        }
                        if (ranCount > 0) {
                            if (!SystemHandler.Handle.Sequences.IsIdle) return;
                            for (int i = 0; i < inspSeq.ActionCount; i++) {
                                if (!(inspSeq[i] is Action_Inspection act)) continue;
                                InspectionParam p = act.Param as InspectionParam;
                                if (p == null || string.IsNullOrEmpty(p.SimulImagePath)
                                    || !File.Exists(p.SimulImagePath)) continue;
                                act.RunBlobOnLastGrab();
                            }
                            mParentWindow.mainView.RefreshAllShotImages();
                            return;
                        }
                    }
                }
            }

            //260410 hbk -- 로드된 이미지 없으면 기존 시퀀스 실행 (카메라 촬상 + 검사)
            if (treeListBox_sequence.SelectedItem is NodeViewModel node) {
                ESequence seqID;
                EAction actID;
                if (node.NodeType == ENodeType.Action) {
                    seqID = node.SequenceID;
                    actID = node.ActionID;
                    mParentWindow.StartSequence(seqID, actID);
                    return;
                }
                else if(node.NodeType == ENodeType.Sequence) {
                    seqID = node.SequenceID;
                    //first action 수행
                    SequenceBase seq = SystemHandler.Handle.Sequences[seqID];
                    if(seq != null) {
                        if (seq.ActionCount > 0) {
                            actID = seq[0].ID;
                            mParentWindow.StartSequence(seqID, actID);
                            return;
                        }
                    }
                }
                //show error msg
                CustomMessageBox.Show("Error", "There is no action to run.\nSelect the sequence or action you want to perform.", MessageBoxImage.Error);
            }
        }

        //260406 hbk -- 시간폴더 일괄 로드: Action 이름으로 5-Shot 이미지 자동 매칭
        private void Btn_LoadFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "시간 폴더 선택 (이미지 일괄 로드)",
                UseDescriptionForTitle = true,
                SelectedPath = SystemSetting.Handle.ImageSavePath,
            };
            if (dlg.ShowDialog() != true) return;

            SequenceBase seq = SystemHandler.Handle.Sequences[ESequence.Inspection];
            if (seq == null) return;

            //260406 hbk -- 폴더 내 BMP 파일 목록
            string[] files = Directory.GetFiles(dlg.SelectedPath, "*.bmp");
            if (files.Length == 0)
            {
                CustomMessageBox.Show("알림", "폴더에 BMP 파일이 없습니다.", MessageBoxImage.Information);
                return;
            }

            int loaded = 0;
            for (int i = 0; i < seq.ActionCount; i++)
            {
                if (!(seq[i] is Action_Inspection act)) continue;
                string actionName = act.Name;   //260406 hbk -- e.g. "Bolt_One_Inspect"

                //260406 hbk -- Action 이름으로 시작하는 파일 매칭
                string matched = files.FirstOrDefault(f => Path.GetFileName(f).StartsWith(actionName));
                if (matched == null) continue;

                var param = act.Param as InspectionParam;
                if (param == null) continue;

                param.SimulImagePath = matched;   //260406 hbk -- 경로 저장
                OpenCvSharp.Mat mat = OpenCvSharp.Cv2.ImRead(matched, OpenCvSharp.ImreadModes.Color);   //260406 hbk -- 이미지 로드
                param.SetOriginalImage(mat);
                mat?.Dispose();
                loaded++;
            }

            if (loaded == 0)
            {
                CustomMessageBox.Show("알림",
                    "Action 이름과 매칭되는 이미지 파일을 찾지 못했습니다.\n" +
                    "파일명이 Action 이름으로 시작해야 합니다.\n" +
                    "(예: Bolt_One_Inspect_OK_23_456.bmp)",
                    MessageBoxImage.Information);
                return;
            }

            //260406 hbk -- 5개 ShotTabView 일괄 갱신
            mParentWindow.mainView.RefreshAllShotImages();

            Logging.PrintLog((int)ELogType.Trace,
                "[IMG] 폴더 일괄 로드: {0} ({1}/{2} Shot 매칭)",
                dlg.SelectedPath, loaded, seq.ActionCount);
        }

        //260407 hbk — Shot 탭 클릭 시 해당 Action 자동 선택
        public void SelectActionByShotIndex(int shotIndex) {
            SequenceBase seq = SystemHandler.Handle.Sequences[ESequence.Inspection];
            if (seq == null || shotIndex < 0 || shotIndex >= seq.ActionCount) return;

            EAction targetActionID = seq[shotIndex].ID;
            for (int i = 0; i < treeListBox_sequence.Items.Count; i++) {
                NodeViewModel node = treeListBox_sequence.Items[i] as NodeViewModel;
                if (node == null) continue;
                if (node.NodeType == ENodeType.Action && node.ActionID == targetActionID) {
                    treeListBox_sequence.SelectedIndex = i;
                    node.IsSelected = true;
                    treeListBox_sequence.ScrollIntoView(node);
                    return;
                }
            }
        }

        public void SetSelectionChange(string seqName) {
            NodeViewModel root = treeListBox_sequence.Items[0] as NodeViewModel;
            root.IsExpanded = true;
            for (int i = 0; i < treeListBox_sequence.Items.Count; i++) {
                NodeViewModel item = treeListBox_sequence.Items[i] as NodeViewModel;
                if((item.NodeType == ENodeType.Sequence) && (item.Name == seqName)) {
                    item.IsSelected = true;
                    treeListBox_sequence.ScrollIntoView(item);
                }
                else {
                    item.IsSelected = false;
                }
            }
        }

        private void InspectionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (mParentWindow == null) mParentWindow = (MainWindow)Window.GetWindow(this);
            button_light.IsEnabled = false;
            button_grab.IsEnabled = false;
            //button_showConfig.IsEnabled = false;

            object source = e.Source;
            if(source is TreeListBox) {
                TreeListBox list = source as TreeListBox;
                if(list.SelectedItem is NodeViewModel) {
                    NodeViewModel item = list.SelectedItem as NodeViewModel;
                    object itemParam = item.Param;

                    //param 
                    if (itemParam is ParamBase) { //action
                        ParamBase param = itemParam as ParamBase;
                        if (itemParam is ICameraParam) {
                            button_grab.IsEnabled = true;
                            button_light.IsEnabled = true;
                        }
                        mParentWindow.mainView.SetParam(item.SequenceID, param);
                        SelectedParam = param;
                    }
                    else { //recipe
                        mParentWindow.mainView.SetParam(item.SequenceID, null);
                        SelectedParam = null;
                    }
                }
            }
        }

        private void button_copy_Click(object sender, RoutedEventArgs e) {
            if(SelectedParam != null) {
                CopiedParam = SelectedParam;
                mParentWindow.statusBar.Model.SetText("Copied : " + CopiedParam.ToString());
                button_paste.IsEnabled = true;
            }
        }

        private void button_paste_Click(object sender, RoutedEventArgs e) {
            if((SelectedParam != null) && (CopiedParam != null)) {
                //confirm message
                if (SelectedParam == CopiedParam) return;

                MessageBoxResult res = CustomMessageBox.ShowConfirmation("Confirmation", string.Format("Would you like to paste {0} into {1}?", CopiedParam.ToString(), SelectedParam.ToString()), MessageBoxButton.OKCancel);
                if(res != MessageBoxResult.OK) {
                    return;
                }
                //paste
                if(CopiedParam.CopyTo(SelectedParam) == false) {
                    //fail
                    CustomMessageBox.Show("Fail to Copy", string.Format("Copy Failed From {0} into {1}", CopiedParam.ToString(), SelectedParam.ToString()), MessageBoxImage.Error);
                    return;
                }
                //success
                mParentWindow.statusBar.Model.SetText(string.Format("Pasted : {0} to {1}",CopiedParam.ToString(), SelectedParam.ToString()));
                int index = treeListBox_sequence.SelectedIndex;

                //reselect (update)
                treeListBox_sequence.UnselectAll();
                treeListBox_sequence.SelectedIndex = index;

                // Paste 후 ShotTabView 강제 갱신 — SelectionChanged가 발생하지 않을 경우 대비   //260330 hbk
                if (SelectedParam is InspectionParam ip)   //260330 hbk
                    mParentWindow.mainView.SetParam(ESequence.Inspection, ip);   //260330 hbk
            }
        }


        private void button_reset_Click(object sender, RoutedEventArgs e)   //260407 hbk — D-02, D-03: 선택 Shot 파라미터 Reset
        {
            NodeViewModel node = treeListBox_sequence.SelectedItem as NodeViewModel;
            if (node == null || node.NodeType != ENodeType.Action) return;
            if (node.SequenceID != ESequence.Inspection) return;

            //260407 hbk — shotIndex 결정 (Btn_start_Click 기존 패턴 재사용)
            SequenceBase inspSeq = SystemHandler.Handle.Sequences[ESequence.Inspection];
            if (inspSeq == null) return;
            int shotIndex = -1;
            for (int i = 0; i < inspSeq.ActionCount; i++)
            {
                if (inspSeq[i].ID == node.ActionID) { shotIndex = i; break; }
            }
            if (shotIndex < 0) return;

            //260407 hbk — Sequence_Inspection 캐스팅 + RestoreShot 호출
            Sequence_Inspection inspectionSeq = inspSeq as Sequence_Inspection;
            if (inspectionSeq == null || !inspectionSeq.HasBackup)
            {
                CustomMessageBox.Show("Reset", "백업 데이터가 없습니다. 레시피를 먼저 로드하세요.", MessageBoxImage.Warning);
                return;
            }

            //260407 hbk — 확인 다이얼로그 (Paste 패턴 준수)
            MessageBoxResult res = CustomMessageBox.ShowConfirmation("Reset",
                string.Format("Shot_{0} 파라미터를 레시피 로드 시점으로 되돌리시겠습니까?", shotIndex + 1),
                MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;

            bool ok = inspectionSeq.RestoreShot(shotIndex);
            if (!ok)
            {
                CustomMessageBox.Show("Reset", "복원에 실패했습니다.", MessageBoxImage.Error);
                return;
            }

            //260407 hbk — PropertyGrid 강제 갱신 (Paste 패턴 재사용)
            int index = treeListBox_sequence.SelectedIndex;
            treeListBox_sequence.UnselectAll();
            treeListBox_sequence.SelectedIndex = index;

            //260407 hbk — ShotTabView 강제 갱신
            if (SelectedParam is InspectionParam ip)
                mParentWindow.mainView.SetParam(ESequence.Inspection, ip);

            mParentWindow.statusBar.Model.SetText(string.Format("Reset Shot_{0} 완료", shotIndex + 1));
        }

        private void button_grab_Click(object sender, RoutedEventArgs e) {
            if (SelectedParam == null) return;
            if (!(SelectedParam is ICameraParam)) return;
            if (SystemHandler.Handle.Sequences.IsIdle == false) {
                //show message
                return;
            }
            //Debug.WriteLine($"217-InspectionListView.xaml.cs SelectedParam:{SelectedParam.ToString()}");
            //list에서 선택된 node의 param을 가져옴
            //param으로 grab 수행하여 결과 drawing
            ICameraParam camParam = SelectedParam as ICameraParam;
            //260326 hbk // SIMUL_MODE: Grab 완료 후 다음 Action 자동 선택 (B방식)
#if SIMUL_MODE
            mParentWindow.mainView.GrabAndDisplay(camParam, onComplete: () => AdvanceToNextAction());   //260326 hbk
#else
            mParentWindow.mainView.GrabAndDisplay(camParam);   //260326 hbk
#endif
        }

#if SIMUL_MODE
        // SIMUL_MODE B방식: Grab 완료 후 다음 Action으로 포커스 이동   //260326 hbk
        private void AdvanceToNextAction()   //260326 hbk
        {
            // onComplete는 GrabAndDisplay의 Dispatcher.BeginInvoke 내부(UI 스레드)에서 호출됨   //260326 hbk
            // 현재 선택된 Action 노드 인덱스 파악   //260326 hbk
            int currentIndex = -1;   //260326 hbk
            int nextIndex = -1;      //260326 hbk

            NodeViewModel currentNode = treeListBox_sequence.SelectedItem as NodeViewModel;   //260326 hbk
            if (currentNode == null || currentNode.NodeType != ENodeType.Action) return;   //260326 hbk

            for (int i = 0; i < treeListBox_sequence.Items.Count; i++)   //260326 hbk
            {
                NodeViewModel node = treeListBox_sequence.Items[i] as NodeViewModel;   //260326 hbk
                if (node == null) continue;   //260326 hbk
                if (node == currentNode)   //260326 hbk
                {
                    currentIndex = i;   //260326 hbk
                    break;             //260326 hbk
                }
            }

            if (currentIndex < 0) return;   //260326 hbk // 선택된 Action 없음

            // 다음 Action 노드 탐색 (currentIndex+1 이후에서 ENodeType.Action 찾기)   //260326 hbk
            for (int i = currentIndex + 1; i < treeListBox_sequence.Items.Count; i++)   //260326 hbk
            {
                NodeViewModel node = treeListBox_sequence.Items[i] as NodeViewModel;   //260326 hbk
                if (node == null) continue;   //260326 hbk
                if (node.NodeType == ENodeType.Action && node.SequenceID == ESequence.Inspection)   //260326 hbk
                {
                    nextIndex = i;   //260326 hbk
                    break;           //260326 hbk
                }
            }

            if (nextIndex < 0) return;   //260326 hbk // Shot 5 이후 더 이상 진행 없음

            // 다음 Action 선택   //260326 hbk
            treeListBox_sequence.UnselectAll();   //260326 hbk
            treeListBox_sequence.SelectedIndex = nextIndex;   //260326 hbk
            NodeViewModel nextNode = treeListBox_sequence.Items[nextIndex] as NodeViewModel;   //260326 hbk
            if (nextNode != null)   //260326 hbk
            {
                nextNode.IsSelected = true;   //260326 hbk
                treeListBox_sequence.ScrollIntoView(nextNode);   //260326 hbk // 스크롤하여 보이게 함
            }
        }
#endif

        private void button_light_Click(object sender, RoutedEventArgs e) {
            //light
            if (SelectedParam == null) return;
            if (!(SelectedParam is ICameraParam)) return;
            if(SystemHandler.Handle.Sequences.IsIdle == false) {
                return;
            }
            ICameraParam camParam = SelectedParam as ICameraParam;
            SystemHandler.Handle.Lights.SetLevel(camParam.LightGroupName, camParam.LightLevel);
            SystemHandler.Handle.Lights.SetOnOff(camParam.LightGroupName, true);
        }
    }
}
