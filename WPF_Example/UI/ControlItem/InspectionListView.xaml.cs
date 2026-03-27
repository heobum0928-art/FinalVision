
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PropertyTools.Wpf;
using FinalVisionProject.Define;
using FinalVisionProject.Sequence;
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
            if (RecipeFiles.Handle.HasRecipe(newName)) {
                if (CustomMessageBox.ShowConfirmation(newName + " Has Already Exists.", "Are you sure you want to overwrite the existing directory?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                    return;
                }
            }
            RecipeFiles.Handle.Copy(curRecipe, newName);
            SystemHandler.Handle.Recipes.CollectRecipe();
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
            //get selected sequence
            if (treeListBox_sequence.SelectedItem is NodeViewModel) {
                NodeViewModel node = treeListBox_sequence.SelectedItem as NodeViewModel;

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
            }
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

            if (nextIndex < 0) return;   //260326 hbk // Shot 5 이후 → 더 이상 진행 없음

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
