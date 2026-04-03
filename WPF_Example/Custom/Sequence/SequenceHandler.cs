using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Utility;

namespace FinalVisionProject.Sequence {
    public sealed partial class SequenceHandler {

        public const string SEQ_INSPECTION = "SEQ_INSPECTION";             //260326 hbk

        public const string ACT_BOLT_ONE   = "Bolt_One_Inspect";           //260326 hbk
        public const string ACT_BOLT_TWO   = "Bolt_Two_Inspect";           //260326 hbk
        public const string ACT_BOLT_THREE = "Bolt_Three_Inspect";         //260326 hbk
        public const string ACT_ASSY_ONE   = "Assy_Rail_One_Inspect";      //260326 hbk
        public const string ACT_ASSY_TWO   = "Assy_Rail_Two_Inspect";      //260326 hbk

        public const int SHOT_INDEX_BOLT_ONE   = 0;                        //260326 hbk
        public const int SHOT_INDEX_BOLT_TWO   = 1;                        //260326 hbk
        public const int SHOT_INDEX_BOLT_THREE = 2;                        //260326 hbk
        public const int SHOT_INDEX_ASSY_ONE   = 3;                        //260326 hbk
        public const int SHOT_INDEX_ASSY_TWO   = 4;                        //260326 hbk

        /// <summary>
        /// Sequence를 정의합니다.
        /// </summary>
        private void RegisterSequences() {   //260326 hbk
            SequenceBuilder.RegisterSequence(
                new Sequence_Inspection(ESequence.Inspection, SEQ_INSPECTION,
                    DeviceHandler.INSPECTION_CAMERA, LightHandler.LIGHT_DEFAULT)   //260326 hbk
            );
        }

        private void RegisterActions() {   //260326 hbk
            SequenceBuilder.RegisterAction(
                new Action_Inspection(EAction.Bolt_One_Inspection,       ACT_BOLT_ONE,   SHOT_INDEX_BOLT_ONE),    //260326 hbk
                new Action_Inspection(EAction.Bolt_Two_Inspection,       ACT_BOLT_TWO,   SHOT_INDEX_BOLT_TWO),    //260326 hbk
                new Action_Inspection(EAction.Bolt_Three_Inspection,     ACT_BOLT_THREE, SHOT_INDEX_BOLT_THREE),  //260326 hbk
                new Action_Inspection(EAction.Assy_Rail_One_Inspection,  ACT_ASSY_ONE,   SHOT_INDEX_ASSY_ONE),    //260326 hbk
                new Action_Inspection(EAction.Assy_Rail_Two_Inspection,  ACT_ASSY_TWO,   SHOT_INDEX_ASSY_TWO)     //260326 hbk
            );
        }

        /// <summary>
        /// Sequence와 Action의 관계를 정의합니다. (Sequence -> Action 관계입니다.)
        /// </summary>
        private void InitializeSequences() {   //260326 hbk
            SequenceBuilder seq;
            seq = SequenceBuilder.CreateSequence(ESequence.Inspection);   //260326 hbk
            seq.AddAction(
                EAction.Bolt_One_Inspection,      //260326 hbk
                EAction.Bolt_Two_Inspection,      //260326 hbk
                EAction.Bolt_Three_Inspection,    //260326 hbk
                EAction.Assy_Rail_One_Inspection, //260326 hbk
                EAction.Assy_Rail_Two_Inspection  //260326 hbk
            );
            RegisterSequence(seq);
        }

    }
}
