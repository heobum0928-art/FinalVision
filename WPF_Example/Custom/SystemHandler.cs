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
using System.Runtime.InteropServices;  //260413 hbk — SetLocalTime P/Invoke

namespace FinalVisionProject {
    public sealed partial class SystemHandler {
        //260413 hbk — v8 터미널 모드 상태 필드
        private volatile bool _dryRunMode = false;         //260413 hbk — DryRun ON/OFF
        private DateTime _syncedTime = DateTime.MinValue;  //260413 hbk — TIME 동기화 값

        //260413 hbk — SetLocalTime P/Invoke (PLC 시간동기화)
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME {
            public ushort wYear, wMonth, wDayOfWeek, wDay;
            public ushort wHour, wMinute, wSecond, wMilliseconds;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetLocalTime(ref SYSTEMTIME st);
        private string _palletId = "";                     //260413 hbk — TRACE Pallet ID (다음 $TRACE까지 유지)
        private string _materialId = "";                   //260413 hbk — TRACE Material ID (다음 $TRACE까지 유지)
        private const int ALIVE_SEND_INTERVAL_MS = 1000;   //260413 hbk — 1초 주기 송신
        private const int ALIVE_DOWN_TIMEOUT_MS = 5000;    //260414 hbk — PLC ALIVE 5초 미수신 시 down 판정
        private readonly Stopwatch _lastAliveRecvTimer = new Stopwatch();  //260414 hbk — PLC ALIVE 마지막 수신 경과시간
        public event Action AliveHeartbeatReceived;  //260413 hbk — Phase 16 UI flash 트리거 (Phase 15 로직 미수정)
        public event Action AliveTimeout;            //260413 hbk — Phase 16 UI 빨강 트리거 (Phase 15 로직 미수정)

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
                    case VisionRequestType.Alive:  //260413 hbk — PLC ALIVE 수신
                        _lastAliveRecvTimer.Restart();  //260414 hbk — 마지막 수신시각 갱신 (down 판정용)
                        AliveHeartbeatReceived?.Invoke();  //260413 hbk — Phase 16 ALIVE flash event
                        responsePacket = ProcessAlive(packet.AsAlive());
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
            //260413 hbk — PLC 시간동기화: Windows 로컬 시계 변경 (미확정 — 주석처리)
            //SYSTEMTIME st = new SYSTEMTIME {
            //    wYear          = (ushort)_syncedTime.Year,
            //    wMonth         = (ushort)_syncedTime.Month,
            //    wDay           = (ushort)_syncedTime.Day,
            //    wHour          = (ushort)_syncedTime.Hour,
            //    wMinute        = (ushort)_syncedTime.Minute,
            //    wSecond        = (ushort)_syncedTime.Second,
            //    wMilliseconds  = 0,
            //    wDayOfWeek     = (ushort)_syncedTime.DayOfWeek,
            //};
            //bool ok = SetLocalTime(ref st);
            Logging.PrintLog((int)ELogType.Trace, "[TIME] Synced={0:yyyy-MM-dd HH:mm:ss}", _syncedTime);
            TimeResultPacket result = new TimeResultPacket();
            result.Target = packet.Sender;
            result.Site = packet.Site;
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

        private AliveResultPacket ProcessAlive(AlivePacket packet) {  //260413 hbk
            AliveResultPacket result = new AliveResultPacket();
            result.Target = packet.Sender;
            result.Site = packet.Site;
            return result;  //260413 hbk — $ALIVE:1,OK@
        }

        //260414 hbk — ALIVE 하트비트: 1초마다 V→PLC 송신, PLC 패킷 5초 미수신 시 down 판정
        private void AliveProcess() {
            bool wasDown = false;
            while (!IsTerminated) {
                if (!Server.IsConnected()) {
                    _lastAliveRecvTimer.Reset();   //260414 hbk — 연결 없으면 타이머 정지 (재연결 후 새로 시작)
                    wasDown = false;
                    Thread.Sleep(500);
                    continue;
                }

                SendAlivePacket();   //260414 hbk — 1초마다 V→PLC 송신 (응답 대기 안 함)

                //260414 hbk — PLC가 보낸 ALIVE 마지막 수신시각 기준 5초 경과 시 down
                //타이머가 멈춰 있으면(=수신 이력 없음) 첫 수신을 기다리는 단계라 down 판정 보류
                if (_lastAliveRecvTimer.IsRunning &&
                    _lastAliveRecvTimer.ElapsedMilliseconds >= ALIVE_DOWN_TIMEOUT_MS) {
                    if (!wasDown) {
                        PerformAliveTimeout();
                        wasDown = true;
                    }
                }
                else if (_lastAliveRecvTimer.IsRunning) {
                    wasDown = false;
                }

                Thread.Sleep(ALIVE_SEND_INTERVAL_MS);
            }
        }

        private void SendAlivePacket() {  //260413 hbk
            string msg = "$ALIVE:1@";
            Server.SendMessage(0, msg);  //260413 hbk — 첫 번째 Client에 송신 (MAX_CONNECTION_COUNT=1)
            Logging.PrintLog((int)ELogType.TcpConnection, "[TCP][SEND] ALIVE heartbeat");
        }

        private void PerformAliveTimeout() {  //260414 hbk — PLC ALIVE 5초 미수신 시 호출
            Logging.PrintLog((int)ELogType.Error, "[ALIVE] No PLC ALIVE for {0}ms. Disconnecting client.", ALIVE_DOWN_TIMEOUT_MS);
            //260413 hbk — 기존 Client 소켓 강제 종료 → TcpServer가 Accept 대기로 복귀
            if (Server.GetConnectedClientCount() > 0) {
                try {
                    Server.GetClient(0).Disconnect();
                } catch (Exception ex) {
                    Logging.PrintLog((int)ELogType.Error, "[ALIVE] Disconnect error: {0}", ex.Message);
                }
            }
            AliveTimeout?.Invoke();  //260413 hbk — Phase 16 UI 빨강 latch 트리거
        }

    }
}
