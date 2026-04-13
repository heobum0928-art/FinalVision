using FinalVisionProject.Setting;
using FinalVisionProject.Network;
using FinalVisionProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FinalVisionProject.Sequence;
using FinalVisionProject.Site;
using System.Diagnostics;

namespace FinalVisionProject {
    public sealed partial class SystemHandler {
        //260413 hbk — v8 터미널 모드 상태 필드
        private volatile bool _dryRunMode = false;         //260413 hbk — DryRun ON/OFF
        private DateTime _syncedTime = DateTime.MinValue;  //260413 hbk — TIME 동기화 값 (Windows 시계 미변경)
        private string _palletId = "";                     //260413 hbk — TRACE Pallet ID (다음 $TRACE까지 유지)
        private string _materialId = "";                   //260413 hbk — TRACE Material ID (다음 $TRACE까지 유지)

        //project 별, sequence 정의
        private void MainRun() {
            //send test response message
            for (int i = 0; i < Sequences.Count; i++) {
                TestResultPacket response = Sequences[i].PopResponse();
                if (response == null) continue;
                Logging.PrintLog((int)ELogType.TcpConnection, "[TCP][SEND] TEST Result To:{0} Site:{1} Type:{2} Result:{3}",   //260330 hbk
                    response.Target, response.Site, response.InspectionType, response.Result);
                if (!Server.SendPacket(response.Target, response)) {
                    //occurs error
                }
            }

            //recv message
            for(int i = 0; i < Server.GetConnectedClientCount(); i++) {
                if(Server.GetRecvPacket(i, out VisionRequestPacket packet) == false) {
                    //no received message
                    continue;
                }
                if(packet == null) {
                    continue;
                }
                Logging.PrintLog((int)ELogType.TcpConnection, "[TCP][RECV] From:{0} Type:{1}",   //260330 hbk
                    packet.Sender, packet.RequestType);
                //메시지를 받음 (operator 모드로 변경함)
                VisionResponsePacket responsePacket = null;
                switch (packet.RequestType) {
                    case VisionRequestType.Light:
                        responsePacket = ProcessLightSet(packet.AsLight());
                        break;
                    case VisionRequestType.RecipeChange:
                        responsePacket = ProcessRecipeChange(packet.AsRecipeChange());
                        break;
                    case VisionRequestType.RecipeGet:
                        responsePacket = ProcessRecipeGet(packet.AsRecipeGet());
                        break;
                    case VisionRequestType.SiteStatus:
                        responsePacket = ProcessSiteStatus(packet.AsSiteStatus());
                        break;
                    case VisionRequestType.Test:
                        if (Setting.AutoLogoutWhenRecvTest && Login.IsLogin) { Login.LogOut(); }

                        if (!ProcessTest(packet.AsTest())) {
                            Logging.PrintLog((int)ELogType.Error, "Client {0} : Fail to Start Sequence. sender:{1}, identifier:{2}", i, packet.Sender, packet.Identifier);
                            //send fail message
                            responsePacket = SendTestError(packet.AsTest());
                        }
                        break;
                    case VisionRequestType.DryRun:  //260413 hbk
                        responsePacket = ProcessDryRun(packet.AsDryRun());
                        break;
                    case VisionRequestType.Time:  //260413 hbk
                        responsePacket = ProcessTime(packet.AsTime());
                        break;
                    case VisionRequestType.Trace:  //260413 hbk
                        responsePacket = ProcessTrace(packet.AsTrace());
                        break;

                    case VisionRequestType.Unknown:
                        //occurs error
                        break;
                }

                //send response
                if (responsePacket == null) {
                    //test 메시지는 곧바로 response를 하지 않음
                }
                else if (!Server.SendPacket(i, responsePacket)) {
                    Logging.PrintLog((int)ELogType.Error, "Client {0} : Fail to Send packet. packetType :{1}", i, responsePacket.ResponseType.ToString());
                }
            }
        }

        private LightResultPacket ProcessLightSet(LightPacket packet) {
            LightResultPacket resultPacket = new LightResultPacket();

            resultPacket.Target = packet.Sender;
            resultPacket.Site = packet.Site;
            Debug.WriteLine($"Packet.TestType:{packet}");
            resultPacket.TestType = packet.TestType;

            if (Sequences[packet.Identifier] != null) {
                SequenceBase seq = Sequences[packet.Identifier];
                if (seq != null) {
                    if(packet.TestType == 0) //off
                    {
                        if (Lights.SetOnOff(packet.Identifier, packet.On) == false) {
                            Thread.Sleep(50);
                            resultPacket.On = !packet.On;
                        }
                        else {
                            Thread.Sleep(50);
                            resultPacket.On = packet.On;
                        }
                        return resultPacket;
                    }
                    int actIndex = seq.GetIndexOf(packet.Identifier2);
                    ActionBase act = seq.GetAction(actIndex);
                    if (act != null) {
                        if(act.Param is CameraSlaveParam) {
                            CameraSlaveParam camParam = act.Param as CameraSlaveParam;

                            if (camParam.LightGroupName == "WAFER")
                            {
                                // Unused
                            }
                            else
                            {
                                if (Lights.SetLevel(camParam.LightGroupName, camParam.LightLevel) == false)
                                {
                                    //Thread.Sleep(50);
                                    resultPacket.On = !packet.On;
                                }
                                else
                                {
                                    resultPacket.On = packet.On;
                                }

                                Thread.Sleep(50);   // 11.12 Insert

                                if (Lights.SetOnOff(camParam.LightGroupName, packet.On) == false)
                                {
                                    //Thread.Sleep(50);
                                    resultPacket.On = !packet.On;
                                }
                                else
                                {
                                    //Thread.Sleep(50);
                                    resultPacket.On = packet.On;
                                }
                            }
                        }
                    }
                }
                return resultPacket;
            }

            //sequence not have identifier
            if(Lights.SetOnOff(packet.Identifier, packet.On) == false) {
                resultPacket.On = !packet.On;
            }
            else {
                resultPacket.On = packet.On;
            }

            return resultPacket;
        }

        private RecipeChangeResultPacket ProcessRecipeChange(RecipeChangePacket packet) {
            RecipeChangeResultPacket resultPacket = new RecipeChangeResultPacket();

            resultPacket.Target = packet.Sender;
            resultPacket.Site = packet.Site;

            int siteNumber = packet.Site;       // 1~5 (패킷에서 직접 Site 번호 사용)
            string recipeName = packet.RecipeName;

            //260403 hbk — D-10: TCP쪽은 siteNumber 기반 경로로 CollectRecipe
            Recipes.CollectRecipe(siteNumber);

            if (Recipes.HasRecipe(recipeName) == false) {
                resultPacket.Result = EVisionResultType.NG;
            }
            else if (Sequences.LoadRecipe(siteNumber, recipeName)) {
                resultPacket.Result = EVisionResultType.OK;
            }
            else {
                resultPacket.Result = EVisionResultType.NG;
            }

            return resultPacket;
        }

        private RecipeListResultPacket ProcessRecipeGet(RecipeGetPacket packet) {
            RecipeListResultPacket resultPacket = new RecipeListResultPacket();

            resultPacket.Target = packet.Sender;
            resultPacket.Site = packet.Site;
            resultPacket.MaxCount = packet.MaxCount;
            
            //sorting
            if (packet.Option == 1) {
                Recipes.SortingByCreateDate();
            }
            else if(packet.Option == 2) {
                Recipes.SortingByLastAccessDate();
            }

            //listing
            resultPacket.RecipeList.Clear();
            for (int i = 0; i< Recipes.List.Count; i++) {
                if (i >= packet.MaxCount) break;
                resultPacket.RecipeList.Add(Recipes[i].Name);
            }
            return resultPacket;
        }
        //sequence의 상태를 반환
        private SiteStatusResultPacket ProcessSiteStatus(SiteStatusPacket packet) {
            SiteStatusResultPacket resultPacket = new SiteStatusResultPacket();

            resultPacket.Target = packet.Sender;
            resultPacket.Site = packet.Site;

            EContextState state =  Sequences.GetSequenceState(packet.Identifier);
            switch (state) {
                case EContextState.Idle:
                    resultPacket.Result = EVisionSiteStatusType.Ready;
                    break;
                case EContextState.Error:
                    resultPacket.Result = EVisionSiteStatusType.Error;
                    break;
                case EContextState.Paused:
                case EContextState.Running:
                case EContextState.Finish:
                    resultPacket.Result = EVisionSiteStatusType.Busy;
                    break;
            }
            return resultPacket;
        }

        //검사 시작 명령 후, 검사 완료까지 대기,
        private bool ProcessTest(TestPacket packet) {
            if (_dryRunMode) {  //260413 hbk — DryRun ON: Sequence 실행 없이 즉시 OK 응답
                TestResultPacket dryResult = new TestResultPacket();
                dryResult.Target = packet.Sender;
                dryResult.Site = packet.Site;
                dryResult.InspectionType = packet.TestType;
                dryResult.Result = EVisionResultType.OK;
                SequenceBase seq = Sequences[packet.Identifier];
                if (seq != null) {
                    seq.ResponseQueue.Enqueue(dryResult);  //260413 hbk — MainRun 상단 PopResponse 경로로 송신
                }
                Logging.PrintLog((int)ELogType.Trace, "[DRYRUN] TEST skipped — immediate OK. Site:{0} Type:{1}", packet.Site, packet.TestType);
                return true;
            }
            return Sequences.Start(packet);
        }

        private TestResultPacket SendTestError(TestPacket packet) {
            TestResultPacket resultPacket = new TestResultPacket();
            TestPacket sendPacket = packet.AsTest();

            resultPacket.Target = sendPacket.Sender;
            resultPacket.Site = sendPacket.Site;
            resultPacket.InspectionType = sendPacket.TestType;
            resultPacket.Result = EVisionResultType.NG;

            return resultPacket;
        }

        private DryRunResultPacket ProcessDryRun(DryRunPacket packet) {  //260413 hbk
            _dryRunMode = packet.Enable;
            DryRunResultPacket result = new DryRunResultPacket();
            result.Target = packet.Sender;
            result.Site = packet.Site;
            Logging.PrintLog((int)ELogType.Trace, "[DRYRUN] Mode={0}", _dryRunMode ? "ON" : "OFF");
            return result;
        }

        private TimeResultPacket ProcessTime(TimePacket packet) {  //260413 hbk
            _syncedTime = packet.SyncedTime;
            TimeResultPacket result = new TimeResultPacket();
            result.Target = packet.Sender;
            result.Site = packet.Site;
            Logging.PrintLog((int)ELogType.Trace, "[TIME] Synced={0:yyyy-MM-dd HH:mm:ss}", _syncedTime);
            return result;
        }

        private TraceResultPacket ProcessTrace(TracePacket packet) {  //260413 hbk
            _palletId = packet.PalletId;
            _materialId = packet.MaterialId;
            TraceResultPacket result = new TraceResultPacket();
            result.Target = packet.Sender;
            result.Site = packet.Site;
            Logging.PrintLog((int)ELogType.Trace, "[TRACE] PalletId={0} MaterialId={1}", _palletId, _materialId);
            return result;
        }


    }
}
