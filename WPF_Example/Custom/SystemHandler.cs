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
        //project 별, sequence 정의
        private void MainRun() {
            //send test response message
            for (int i = 0; i < Sequences.Count; i++) {
                TestResultPacket response = Sequences[i].PopResponse();
                if (response == null) continue; 
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

            // Site별 레시피 목록을 수집하여 HasRecipe 체크
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
            //TestResultPacket resultPacket = new TestResultPacket();

            //resultPacket.Target = packet.Sender;
            //resultPacket.Zone = packet.Zone;
            //resultPacket.Site = packet.Site;

            //Sequences[]

            //return resultPacket;
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


    }
}
