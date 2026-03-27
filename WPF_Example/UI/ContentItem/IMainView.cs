using OpenCvSharp;
using FinalVisionProject.Define;
using FinalVisionProject.Sequence;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace FinalVisionProject.UI {
    interface IMainView {
        Dictionary<string, SequenceContext> ContextList { get; set; }
        
        bool Display(string name, string result, Brush resultBrush, object param = null);
        
        bool Display(string name, Mat img, string result, Brush resultBrush, object param = null);
    }
}
