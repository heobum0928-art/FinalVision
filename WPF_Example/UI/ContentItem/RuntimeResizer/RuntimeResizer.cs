using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using FinalVisionProject.Sequence;
using FinalVisionProject.Device;
using System.ComponentModel;

namespace FinalVisionProject.UI {
    public class SelectionChangedCallbackArg : EventArgs {
        public IDrawableItem SelectedItem { get; private set; }

        public SelectionChangedCallbackArg(IDrawableItem selected) {
            SelectedItem = selected;
        }
    }

    public delegate void OnDrawingItemSelectionChanged(object sender, SelectionChangedCallbackArg args);

    public class RuntimeResizer : Canvas, INotifyPropertyChanged {
        //operation
        private EPickerPosition CurrentActionMode = EPickerPosition.None;
        
        public ScaleTransform _ScaleTransform { get; set; }

        //translateTransform
        //public TranslateTransform _TranslateTransform { get; set; }
        public ScrollViewer ParentScrollViewer { get; set; }

        //spec
        private ParamBase pParam;

        //drawable
        public IDrawableItem SelectedItem { get; private set; }
        private List<IDrawableItem> DrawableList = new List<IDrawableItem>();
        private object mDrawInterlock = new object();
        //event
        public event OnDrawingItemSelectionChanged OnSelectionItemChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private SequenceContext SequenceResult;

        private bool _IsEditable = false;
        public bool IsEditable {
            get { return _IsEditable; }
            set {
                if (_IsEditable != value) {
                    _IsEditable = value;
                    SelectedItem = null;
                    InvalidateVisual();
                }
            }
        }

        private Point DownPosition;
        private Point ScrollStartPos;
        private Point ScrollEndPos;
        private Point CurrentPos;
        private bool _isDrawingNew = false;   //260326 hbk // 신규 ROI 드래그 중 플래그
        private Point _drawStartPoint;        //260326 hbk // 드래그 시작 이미지 좌표 (ScaleTransform 역변환)

        private string _CurrentPosDisplay;
        public string CurrentPosDisplay {
            get {
                return _CurrentPosDisplay;
            }
            set {
                _CurrentPosDisplay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentPosDisplay"));
            }
        }
            
        
        public RuntimeResizer() : base() {
            this.MouseDown += this.OnMouseDown;
            this.MouseMove += this.OnMouseMove;
            this.MouseUp += this.OnMouseUp;
        }

        public void SetParam(ICameraParam param) {
            if(param is ParamBase) {
                ParamBase pb = param as ParamBase;
                SetParam(pb);
            }
        }

        public void SetParam(ParamBase param) {
            lock (mDrawInterlock) {
                
                pParam = param;
                SequenceResult = null;

                //ROI attribute를 검색해서 drawable list를 구축한다.
                DrawableList.Clear();
                if (pParam != null) {
                    // ROIShape 필터링: InspectionParam이면 선택된 도형만 표시   //260327 hbk 그리기
                    bool showRect   = true;   //260327 hbk 그리기
                    bool showCircle = true;   //260327 hbk 그리기
                    if (pParam is InspectionParam ip) {   //260327 hbk 그리기
                        showRect   = (ip.ROIShape == ERoiShape.Rectangle);   //260327 hbk 그리기
                        showCircle = (ip.ROIShape == ERoiShape.Circle);      //260327 hbk 그리기
                    }

                    if (showRect) {   //260327 hbk 그리기
                        for (int i = 0; i < pParam.GetRectCount(); i++) {
                            pParam.GetRectName(i, out string name);
                            pParam.GetRectOwner(i, out object owner);
                            DrawableList.Add(new DrawableRectangle(pParam, owner, name));
                        }
                    }
                    for(int i = 0; i < pParam.GetLineCount(); i++) {
                        pParam.GetLineName(i, out string name);
                        pParam.GetLineOwner(i, out object owner);
                        DrawableList.Add(new DrawableLine(pParam, owner, name));
                    }
                    if (showCircle) {   //260327 hbk 그리기
                        for(int i = 0; i < pParam.GetCircleCount(); i++) {
                            pParam.GetCircleName(i, out string name);
                            pParam.GetCircleOwner(i, out object owner);
                            DrawableList.Add(new DrawableCircle(pParam, owner, name));
                        }
                    }
                }
            }
            this.InvalidateVisual();
        }
        /*
        public void OnKeyUp(object sender, KeyEventArgs e) {
            
            if (IsEditable == false) return;
            if (SelectedItem == null) {
                e.Handled = true;
                return;
            }
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            switch(e.Key) {
                case Key.Up:
                    if (shift) { //size up
                        SelectedItem.ExecResize(0, (int)-(10 / _ScaleTransform.ScaleY), 0, (int)(10 / _ScaleTransform.ScaleY));
                    }
                    else if(ctrl) { //move
                        SelectedItem.ExecMove(0, (int)(-10 / _ScaleTransform.ScaleY));
                    }
                    break;
                case Key.Down:
                    if(shift) { //size up
                        SelectedItem.ExecResize(0, (int)(10 / _ScaleTransform.ScaleY), 0, (int)-(10 / _ScaleTransform.ScaleY));
                    }
                    else if (ctrl) { //move
                        SelectedItem.ExecMove(0, (int)(10 / _ScaleTransform.ScaleY));
                    }
                    break;
                case Key.Left:
                    break;
                case Key.Right:
                    break;
            }
            SelectedItem.CheckAvailable(CurrentActionMode);
            this.InvalidateVisual();
        }
        */

        public void OnMouseDown(object sender, MouseEventArgs e) {
            if (IsEditable == false) {
                this.CaptureMouse();
                ScrollStartPos = e.GetPosition(ParentScrollViewer);
                return;
            }

            bool IsSelected = false;

            //down 위치를 저장함.
            DownPosition.X = e.GetPosition(this).X;
            DownPosition.Y = e.GetPosition(this).Y;

            //이미 선택되어 있다면
            if (SelectedItem != null) {
                CurrentActionMode = SelectedItem.GetMouseIsEnter((int)(e.GetPosition(this).X / _ScaleTransform.ScaleX), (int)(e.GetPosition(this).Y / _ScaleTransform.ScaleY));
                if (CurrentActionMode == EPickerPosition.None) {
                    SelectedItem = null;
                    OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                }
                else if(e.RightButton == MouseButtonState.Pressed) {
                    //다음 item 선택 (선택 전환)
                    int curIndex = DrawableList.IndexOf(SelectedItem);
                    foreach (IDrawableItem item in DrawableList) {
                        if (item == SelectedItem) continue;
                        if (item.GetMouseIsEnter((int)(e.GetPosition(this).X / _ScaleTransform.ScaleX), (int)(e.GetPosition(this).Y / _ScaleTransform.ScaleY)) == EPickerPosition.Body) {
                            SelectedItem = item;
                            OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                            return;
                        }
                    }
                    return;
                }
                else return;
            }

            //선택 작업
            foreach (IDrawableItem item in DrawableList) {
                //roi의 자식 item이 선택되었는지를 확인
                switch (item.GetMouseIsEnter((int)(e.GetPosition(this).X / _ScaleTransform.ScaleX), (int)(e.GetPosition(this).Y / _ScaleTransform.ScaleY))) {
                    case EPickerPosition.Body:
                        SelectedItem = item;
                        IsSelected = true;
                        OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                        break;
                    case EPickerPosition.LeftBottom:
                    case EPickerPosition.LeftTop:
                    case EPickerPosition.RightBottom:
                    case EPickerPosition.RightTop:
                    case EPickerPosition.Left:
                    case EPickerPosition.Right:
                    case EPickerPosition.Top:
                    case EPickerPosition.Bottom:
                        if (SelectedItem != null) IsSelected = true;
                        break;
                    case EPickerPosition.Point1:
                    case EPickerPosition.Point2:
                        if (SelectedItem != null) IsSelected = true;
                        break;
                }

                //아무 자식도 선택되지 않았으면.. ROI 자신을 선택
                if (!IsSelected) {
                    if (item.GetMouseIsEnter((int)(e.GetPosition(this).X / _ScaleTransform.ScaleX), (int)(e.GetPosition(this).Y / _ScaleTransform.ScaleY)) == EPickerPosition.Body) {
                        //이미 추가된 항목을 추가하지 않도록 중복 체크함.
                        SelectedItem = item;
                        IsSelected = true;
                        OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                        break;
                    }
                }
            }
            if (!IsSelected) {   //260326 hbk
                // IsEditable + 좌클릭 + DrawableList에 ROI 존재 → 신규 ROI 드래그 시작   //260326 hbk
                if (IsEditable && e.LeftButton == MouseButtonState.Pressed && DrawableList.Count > 0)   //260326 hbk
                {
                    _isDrawingNew = true;   //260326 hbk // 신규 ROI 드래그 시작
                    // 이미지 좌표계로 변환 (ScaleTransform 역변환)   //260326 hbk
                    _drawStartPoint = new Point(
                        e.GetPosition(this).X / _ScaleTransform.ScaleX,   //260326 hbk
                        e.GetPosition(this).Y / _ScaleTransform.ScaleY);  //260326 hbk
                    this.CaptureMouse();   //260326 hbk // 드래그 중 마우스 캡처
                }
                else   //260326 hbk // 우클릭 or IsEditable=false or ROI 없음 → 기존 스크롤
                {
                    this.CaptureMouse();
                    ScrollStartPos = e.GetPosition(ParentScrollViewer);
                }
            }
            this.InvalidateVisual();
        }

        public void OnMouseMove(object sender, MouseEventArgs e) {
            
            CurrentPos = e.GetPosition(this);
            CurrentPosDisplay = string.Format("X:{0:0.0}, Y:{1:0.0}", CurrentPos.X / _ScaleTransform.ScaleX, CurrentPos.Y / _ScaleTransform.ScaleY);

            if (this.IsMouseCaptured) {
                if (_isDrawingNew)   //260326 hbk // 신규 ROI 드래그 미리보기 — 스크롤 없이 캔버스만 갱신
                {
                    this.InvalidateVisual();   //260326 hbk // OnRender에서 미리보기 사각형 그리기
                    return;                    //260326 hbk
                }
                ScrollEndPos = e.GetPosition(ParentScrollViewer);
                double x = ScrollEndPos.X - ScrollStartPos.X;
                double y = ScrollEndPos.Y - ScrollStartPos.Y;
                ParentScrollViewer.ScrollToHorizontalOffset(ParentScrollViewer.HorizontalOffset - x);
                ParentScrollViewer.ScrollToVerticalOffset(ParentScrollViewer.VerticalOffset - y);
                ScrollStartPos = ScrollEndPos;

                if (IsEditable == false) return;
            }

            else if (SelectedItem != null) {
                if (e.LeftButton == MouseButtonState.Pressed) {
                    switch (CurrentActionMode) {
                        case EPickerPosition.Body:
                            Cursor = Cursors.SizeAll;
                            SelectedItem.ExecMove(
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Left:
                            Cursor = Cursors.SizeWE;
                            SelectedItem.ExecResize(
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                0,
                                (int)-((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                0);

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Right:
                            Cursor = Cursors.SizeWE;
                            SelectedItem.ExecResize(
                                0,
                                0,
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                0);

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Top:
                            Cursor = Cursors.SizeNS;
                            SelectedItem.ExecResize(
                                0,
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY),
                                0,
                                (int)-((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Bottom:
                            Cursor = Cursors.SizeNS;
                            SelectedItem.ExecResize(
                                0,
                                0,
                                0,
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.LeftBottom:
                            Cursor = Cursors.SizeNESW;
                            SelectedItem.ExecResize(
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                0,
                                (int)-((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.LeftTop:
                            Cursor = Cursors.SizeNWSE;
                            SelectedItem.ExecResize(
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY),
                                (int)-((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)-((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.RightBottom:
                            Cursor = Cursors.SizeNWSE;
                            SelectedItem.ExecResize(
                                0,
                                0,
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));
                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.RightTop:
                            Cursor = Cursors.SizeNESW;
                            SelectedItem.ExecResize(
                                0,
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY),
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)-((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));

                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Point1:
                            Cursor = Cursors.SizeWE;
                            SelectedItem.ExecResize(
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY),
                                0,
                                0);
                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.Point2:
                            Cursor = Cursors.SizeWE;
                            SelectedItem.ExecResize(
                                0,
                                0,
                                (int)((CurrentPos.X - DownPosition.X) / _ScaleTransform.ScaleX),
                                (int)((CurrentPos.Y - DownPosition.Y) / _ScaleTransform.ScaleY));
                            DownPosition.X = CurrentPos.X;
                            DownPosition.Y = CurrentPos.Y;
                            break;
                        case EPickerPosition.None:
                            Cursor = Cursors.Arrow;
                            break;

                    }
                    this.InvalidateVisual();
                }
                else {
                    switch (SelectedItem.GetMouseIsEnter((int)(CurrentPos.X / _ScaleTransform.ScaleX), (int)(CurrentPos.Y / _ScaleTransform.ScaleY))) {
                        case EPickerPosition.Body:
                            Cursor = Cursors.SizeAll;
                            break;
                        case EPickerPosition.Left:
                            Cursor = Cursors.SizeWE;
                            break;
                        case EPickerPosition.Right:
                            Cursor = Cursors.SizeWE;
                            break;
                        case EPickerPosition.Top:
                            Cursor = Cursors.SizeNS;
                            break;
                        case EPickerPosition.Bottom:
                            Cursor = Cursors.SizeNS;
                            break;
                        case EPickerPosition.LeftBottom:
                            Cursor = Cursors.SizeNESW;
                            break;
                        case EPickerPosition.LeftTop:
                            Cursor = Cursors.SizeNWSE;
                            break;
                        case EPickerPosition.RightBottom:
                            Cursor = Cursors.SizeNWSE;
                            break;
                        case EPickerPosition.RightTop:
                            Cursor = Cursors.SizeNESW;
                            break;
                        case EPickerPosition.None:
                            Cursor = Cursors.Arrow;
                            break;
                        case EPickerPosition.Point1:
                        case EPickerPosition.Point2:
                            Cursor = Cursors.SizeWE;
                            break;
                    }
                }
            }
            else {
                Cursor = Cursors.Arrow;
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs e) {
            this.ReleaseMouseCapture();

            if (_isDrawingNew)   //260326 hbk // 신규 ROI 드래그 완료 → SelectedItem ROI 업데이트
            {
                _isDrawingNew = false;   //260326 hbk

                // 현재 마우스 이미지 좌표   //260326 hbk
                Point endPt = new Point(
                    CurrentPos.X / _ScaleTransform.ScaleX,
                    CurrentPos.Y / _ScaleTransform.ScaleY);

                // Circle 모드 처리   //260327 hbk 그리기
                bool isCircleMode = (pParam is InspectionParam ipc && ipc.ROIShape == ERoiShape.Circle);   //260327 hbk 그리기
                if (isCircleMode)   //260327 hbk 그리기
                {
                    double cRadius = Math.Sqrt(
                        Math.Pow(endPt.X - _drawStartPoint.X, 2) +
                        Math.Pow(endPt.Y - _drawStartPoint.Y, 2));   //260327 hbk 그리기 — 시작점=중심, 끝점까지=반지름
                    if (cRadius > DeviceHandler.MIN_CIRCLE_RADIUS)   //260327 hbk 그리기
                    {
                        IDrawableItem target = SelectedItem;
                        if (target == null) {
                            foreach (IDrawableItem d in DrawableList)
                                if (d is DrawableCircle) { target = d; break; }   //260327 hbk 그리기
                        }
                        if (target is DrawableCircle dcirc)   //260327 hbk 그리기
                        {
                            dcirc.OriginalCircle = new Circle(_drawStartPoint.X, _drawStartPoint.Y, cRadius);   //260327 hbk 그리기
                            dcirc.UpdatePicker();                                                                  //260327 hbk 그리기
                            dcirc.CheckAvailable(EPickerPosition.None);                                           //260327 hbk 그리기
                            SelectedItem = dcirc;
                            OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                        }
                    }
                }
                else   //260327 hbk 그리기 — Rectangle 모드
                {
                    // 드래그 범위 → Rect 계산 (음수 폭/높이 방지)   //260326 hbk
                    double rx = Math.Min(_drawStartPoint.X, endPt.X);
                    double ry = Math.Min(_drawStartPoint.Y, endPt.Y);
                    double rw = Math.Abs(endPt.X - _drawStartPoint.X);
                    double rh = Math.Abs(endPt.Y - _drawStartPoint.Y);

                    if (rw > DeviceHandler.MIN_ROI_WIDTH && rh > DeviceHandler.MIN_ROI_HEIGHT)   //260326 hbk
                    {
                        IDrawableItem target = SelectedItem;
                        if (target == null) {
                            foreach (IDrawableItem d in DrawableList)
                                if (d is DrawableRectangle) { target = d; break; }
                        }
                        if (target is DrawableRectangle dr)   //260326 hbk
                        {
                            dr.UpdateRect(new System.Windows.Rect(rx, ry, rw, rh));
                            dr.CheckAvailable(EPickerPosition.None);
                            SelectedItem = dr;
                            OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));
                        }
                    }
                }
                this.InvalidateVisual();   //260326 hbk
                return;   //260326 hbk
            }

            if (IsEditable == false) return;
            if (SelectedItem != null) {
                SelectedItem.CheckAvailable(CurrentActionMode);
                this.InvalidateVisual();
            }
        }

        public void SetContext(SequenceContext context) {
            lock (mDrawInterlock) {
                SequenceResult = context;
                DrawableList.Clear();
                if ((SequenceResult != null) && (SequenceResult.ActionParam != null)) {
                    pParam = SequenceResult.ActionParam;
                    
                    for (int i = 0; i < pParam.GetRectCount(); i++) {
                        //pParam.GetROI(i, out Rect item);
                        pParam.GetRectName(i, out string name);
                        pParam.GetRectOwner(i, out object owner);
                        DrawableList.Add(new DrawableRectangle(pParam, owner, name));
                    }
                    for (int i = 0; i < pParam.GetLineCount(); i++) {
                        pParam.GetLineName(i, out string name);
                        pParam.GetLineOwner(i, out object owner);
                        DrawableList.Add(new DrawableLine(pParam, owner, name));
                    }
                    for (int i = 0; i < pParam.GetCircleCount(); i++) {
                        pParam.GetCircleName(i, out string name);
                        pParam.GetCircleOwner(i, out object owner);
                        DrawableList.Add(new DrawableCircle(pParam, owner, name));
                    }
                }
            }
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            lock (mDrawInterlock) {
                dc.PushTransform(this._ScaleTransform);

                //draw center lines
                if (SequenceResult != null) {
                    SequenceResult.RenderResult(dc);
                    if(SequenceResult.Source.Param is CameraMasterParam) {
                        CameraMasterParam camParam = SequenceResult.Source.Param as CameraMasterParam;
                        VirtualCamera cam = SystemHandler.Handle.Devices[camParam.DeviceName];
                        if (cam != null) {
                            cam.RenderCenterLine(dc);
                        }
                    }
                }
                else if (pParam != null) {
                    if(pParam is ICameraParam) {
                        ICameraParam camParam = pParam as ICameraParam;
                        VirtualCamera cam = SystemHandler.Handle.Devices[camParam.DeviceName];
                        if(cam != null) {
                            cam.RenderCenterLine(dc);
                        }
                    }
                }

                // ROI 테두리는 항상 표시, 편집 핸들(Picker)은 Edit 모드에서만 표시   //260330 hbk
                foreach (IDrawableItem item in DrawableList) {
                    item.Render(dc);

                    if (IsEditable && (item == SelectedItem)) {
                        item.RenderPicker(dc);
                    }
                }

                // 신규 ROI 드래그 미리보기   //260326 hbk
                if (_isDrawingNew)   //260326 hbk
                {
                    Point curImgPt = new Point(
                        CurrentPos.X / _ScaleTransform.ScaleX,
                        CurrentPos.Y / _ScaleTransform.ScaleY);

                    Pen dashPen = new Pen(Brushes.Yellow, 1);
                    dashPen.DashStyle = new DashStyle(new double[] { 4, 4 }, 0);

                    // Circle 모드 미리보기   //260327 hbk 그리기
                    bool isCircleMode = (pParam is InspectionParam ipc && ipc.ROIShape == ERoiShape.Circle);   //260327 hbk 그리기
                    if (isCircleMode)   //260327 hbk 그리기
                    {
                        double previewRadius = Math.Sqrt(
                            Math.Pow(curImgPt.X - _drawStartPoint.X, 2) +
                            Math.Pow(curImgPt.Y - _drawStartPoint.Y, 2));   //260327 hbk 그리기 — 시작점=중심, 반지름=거리
                        if (previewRadius > 1)
                            dc.DrawEllipse(null, dashPen, _drawStartPoint, previewRadius, previewRadius);   //260327 hbk 그리기
                    }
                    else
                    {
                        double px = Math.Min(_drawStartPoint.X, curImgPt.X);
                        double py = Math.Min(_drawStartPoint.Y, curImgPt.Y);
                        double pw = Math.Abs(curImgPt.X - _drawStartPoint.X);
                        double ph = Math.Abs(curImgPt.Y - _drawStartPoint.Y);
                        if (pw > 1 && ph > 1)
                            dc.DrawRectangle(null, dashPen, new System.Windows.Rect(px, py, pw, ph));
                    }
                }
            }
        }
    }

}
