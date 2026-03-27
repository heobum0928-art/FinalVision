using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class TISLoadportInputs : TISSensors
    {
        public TISLoadportInputs(string ioValues, int nbyte)
            : base(ioValues, nbyte)
        {
        }

        #region ch0
        public IOStates CDASensor { get { return (bits[0][0] ? IOStates.Off : IOStates.On); } }
        public IOStates VACSensor { get { return (bits[0][1] ? IOStates.Off : IOStates.On); } }
        public IOStates AutoModeSensor { get { return (bits[0][2] ? IOStates.Off : IOStates.On); } }
        public IOStates ClampUpSensor { get { return (bits[0][3] ? IOStates.Off : IOStates.On); } }
        public IOStates ClampDetectSensor { get { return (bits[0][4] ? IOStates.Off : IOStates.On); } }
        public IOStates ClampDownSensor { get { return (bits[0][5] ? IOStates.Off : IOStates.On); } }
        public IOStates DockSensor { get { return (bits[0][6] ? IOStates.Off : IOStates.On); } }
        public IOStates UndockSensor { get { return (bits[0][7] ? IOStates.Off : IOStates.On); } }
        #endregion
        #region ch1
        public IOStates PresentSensor { get { return (bits[1][0] ? IOStates.Off : IOStates.On); } }
        public IOStates Place1Sensor { get { return (bits[1][1] ? IOStates.Off : IOStates.On); } }
        public IOStates Place2Sensor { get { return (bits[1][2] ? IOStates.Off : IOStates.On); } }
        public IOStates Place3Sensor { get { return (bits[1][3] ? IOStates.Off : IOStates.On); } }
        public IOStates SW1Sensor { get { return (bits[1][4] ? IOStates.Off : IOStates.On); } }
        public IOStates SW2Sensor { get { return (bits[1][5] ? IOStates.Off : IOStates.On); } }
        public IOStates LatchSensor { get { return (bits[1][6] ? IOStates.Off : IOStates.On); } }
        public IOStates UnlatchSensor { get { return (bits[1][7] ? IOStates.Off : IOStates.On); } }
        #endregion
        #region ch2
        public IOStates Mapping1FWSensore { get { return (bits[2][0] ? IOStates.Off : IOStates.On); } }
        public IOStates Mapping1BWSensore { get { return (bits[2][1] ? IOStates.Off : IOStates.On); } }
        public IOStates Mapping2FWSensore { get { return (bits[2][2] ? IOStates.Off : IOStates.On); } }
        public IOStates Mapping2BWSensore { get { return (bits[2][3] ? IOStates.Off : IOStates.On); } }
        public IOStates MappingSensor { get { return (bits[2][4] ? IOStates.Off : IOStates.On); } }
        public IOStates DoorOpenSensor { get { return (bits[2][5] ? IOStates.Off : IOStates.On); } }
        public IOStates DoorCloseSensor { get { return (bits[2][6] ? IOStates.Off : IOStates.On); } }
        public IOStates WaferProtrusionSensor { get { return (bits[2][7] ? IOStates.Off : IOStates.On); } }
        #endregion
        #region ch3
        public IOStates PinchSensor { get { return (bits[3][0] ? IOStates.Off : IOStates.On); } }
        public IOStates ZLimitPlusSensor { get { return (bits[3][1] ? IOStates.Off : IOStates.On); } }
        public IOStates ZUnloadSensore { get { return (bits[3][2] ? IOStates.Off : IOStates.On); } }
        public IOStates ZLoadSensor { get { return (bits[3][3] ? IOStates.Off : IOStates.On); } }
        public IOStates ZLimitMinusSensor { get { return (bits[3][4] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP1Sensor { get { return (bits[3][5] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP2Sensor { get { return (bits[3][6] ? IOStates.Off : IOStates.On); } }

        #endregion
        #region ch4
        public IOStates ISP3Sensor { get { return (bits[5][0] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP4Sensor { get { return (bits[5][1] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP5Sensor { get { return (bits[5][2] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP6Sensor { get { return (bits[5][3] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP7Sensor { get { return (bits[5][4] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP8Sensor { get { return (bits[5][5] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP9Sensor { get { return (bits[5][6] ? IOStates.Off : IOStates.On); } }
        public IOStates ISP10Sensor { get { return (bits[5][7] ? IOStates.Off : IOStates.On); } }

        #endregion













        //#region ch0
        //public IOStates CDASensor { get { return (bits[0][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates VACSensor { get { return (bits[0][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates AutoModeSensor { get { return (bits[0][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates ClampUpSensor { get { return (bits[0][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates ClampDetectSensor { get { return (bits[0][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates ClampDownSensor { get { return (bits[0][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates DockSensor { get { return (bits[0][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates UndockSensor { get { return (bits[0][7] ? IOStates.On : IOStates.Off); } }
        //#endregion
        //#region ch1
        //public IOStates PresentSensor { get { return (bits[1][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates Place1Sensor { get { return (bits[1][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates Place2Sensor { get { return (bits[1][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates Place3Sensor { get { return (bits[1][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates SW1Sensor { get { return (bits[1][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates SW2Sensor { get { return (bits[1][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates LatchSensor { get { return (bits[1][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates UnlatchSensor { get { return (bits[1][7] ? IOStates.On : IOStates.Off); } }
        //#endregion
        //#region ch2
        //public IOStates Mapping1FWSensore { get { return (bits[2][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates Mapping1BWSensore { get { return (bits[2][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates Mapping2FWSensore { get { return (bits[2][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates Mapping2BWSensore { get { return (bits[2][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates MappingSensor { get { return (bits[2][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates DoorOpenSensor { get { return (bits[2][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates DoorCloseSensor { get { return (bits[2][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates WaferProtrusionSensor { get { return (bits[2][7] ? IOStates.On : IOStates.Off); } }
        //#endregion
        //#region ch3
        //public IOStates PinchSensor { get { return (bits[3][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates ZLimitPlusSensor { get { return (bits[3][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates ZUnloadSensore { get { return (bits[3][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates ZLoadSensor { get { return (bits[3][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates ZLimitMinusSensor { get { return (bits[3][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP1Sensor { get { return (bits[3][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP2Sensor { get { return (bits[3][6] ? IOStates.On : IOStates.Off); } }

        //#endregion
        //#region ch4
        //public IOStates ISP3Sensor { get { return (bits[5][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP4Sensor { get { return (bits[5][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP5Sensor { get { return (bits[5][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP6Sensor { get { return (bits[5][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP7Sensor { get { return (bits[5][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP8Sensor { get { return (bits[5][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP9Sensor { get { return (bits[5][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates ISP10Sensor { get { return (bits[5][7] ? IOStates.On : IOStates.Off); } }

        //#endregion
            
        public string ToSensorString()
        {

            //public IOStates PinchSensor { get { return (bits[3][0] ? IOStates.On : IOStates.Off); } }
            //public IOStates ZLimitPlusSensor { get { return (bits[3][1] ? IOStates.On : IOStates.Off); } }
            //public IOStates ZUnloadSensore { get { return (bits[3][2] ? IOStates.On : IOStates.Off); } }
            //public IOStates ZLoadSensor { get { return (bits[3][3] ? IOStates.On : IOStates.Off); } }
            //public IOStates ZLimitMinusSensor { get { return (bits[3][4] ? IOStates.On : IOStates.Off); } }
            //public IOStates ISP1Sensor { get { return (bits[3][5] ? IOStates.On : IOStates.Off); } }
            //public IOStates ISP2Sensor { get { return (bits[3][6] ? IOStates.On : IOStates.Off); } }

            string ch_string = string.Format($"CDASensor : {CDASensor}, VACSensor : {VACSensor}, AutoModeSensor : {AutoModeSensor}, ClampUpSensor : {ClampUpSensor}, ClampDetectSensor : {ClampDetectSensor}," +
                        $"ClampDownSensor : {ClampDownSensor}, DockSensor : {DockSensor}, UndockSensor : {UndockSensor}," +

                        $"PresentSensor : {PresentSensor}, Place1Sensor : {Place1Sensor}, Place2Sensor : {Place2Sensor}, Place3Sensor : {Place3Sensor}, SW1Sensor : {SW1Sensor}, SW2Sensor : {SW2Sensor}," +
                        $"LatchSensor : {LatchSensor}, UnlatchSensor : {UnlatchSensor}" +

                        $"Mapping1FWSensore : {Mapping1FWSensore}, Mapping1BWSensore : {Mapping1BWSensore}, Mapping2FWSensore : {Mapping2FWSensore}, Mapping2BWSensore : {Mapping2BWSensore}," +
                        $"MappingSensor : {MappingSensor}, DoorOpenSensor : {DoorOpenSensor}, DoorCloseSensor : {DoorCloseSensor}, WaferProtrusionSensor : {WaferProtrusionSensor}," +

                        $"PinchSensor : {PinchSensor}, ZLimitPlusSensor : {ZLimitPlusSensor}, ZUnloadSensore : {ZUnloadSensore}, ZLoadSensor : {ZLoadSensor}, ZLimitMinusSensor : {ZLimitMinusSensor}");



            return base.ToString() + ch_string;
        }
    }

    public class TISLoadportOutputs : TISSensors
    {
        public TISLoadportOutputs(string ioValues, int nbyte)
            : base(ioValues, nbyte)
        {
        }
        #region ch0
        public IOStates SolClampUpSensor { get { return (bits[0][0] ? IOStates.On : IOStates.Off); } }
        public IOStates SolClampDownSensor { get { return (bits[0][1] ? IOStates.On : IOStates.Off); } }
        public IOStates SolDockSensor { get { return (bits[0][2] ? IOStates.On : IOStates.Off); } }
        public IOStates SolUndockSensor { get { return (bits[0][3] ? IOStates.On : IOStates.Off); } }
        public IOStates SolLatchSensor { get { return (bits[0][4] ? IOStates.On : IOStates.Off); } }
        public IOStates SolUnlatchSensor { get { return (bits[0][5] ? IOStates.On : IOStates.Off); } }
        public IOStates SolOpenSensor { get { return (bits[0][6] ? IOStates.On : IOStates.Off); } }
        public IOStates SolCloseSensor { get { return (bits[0][7] ? IOStates.On : IOStates.Off); } }
        #endregion

        #region ch1
        public IOStates SolMappingArmSensor { get { return (bits[1][0] ? IOStates.On : IOStates.Off); } }
        public IOStates SolSuctionSensor { get { return (bits[1][1] ? IOStates.On : IOStates.Off); } }
        public IOStates BREAKSensor { get { return (bits[1][2] ? IOStates.On : IOStates.Off); } }
        public IOStates MappingSensorOnSensor { get { return (bits[1][3] ? IOStates.On : IOStates.Off); } }
        public IOStates LampSW1Sensor { get { return (bits[1][4] ? IOStates.On : IOStates.Off); } }
        public IOStates LampSW2Sensor { get { return (bits[1][5] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP1Sensor { get { return (bits[1][6] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP2Sensor { get { return (bits[1][7] ? IOStates.On : IOStates.Off); } }
        #endregion

        #region ch2
        public IOStates LampManualSensor { get { return (bits[2][0] ? IOStates.On : IOStates.Off); } }
        public IOStates LampAutoSensor { get { return (bits[2][1] ? IOStates.On : IOStates.Off); } }
        public IOStates LampPresentSensor { get { return (bits[2][2] ? IOStates.On : IOStates.Off); } }
        public IOStates LampPlacedSensor { get { return (bits[2][3] ? IOStates.On : IOStates.Off); } }
        public IOStates LampLoadSensor { get { return (bits[2][4] ? IOStates.On : IOStates.Off); } }
        public IOStates LampUnloadSensor { get { return (bits[2][5] ? IOStates.On : IOStates.Off); } }
        public IOStates LampReservedSensor { get { return (bits[2][6] ? IOStates.On : IOStates.Off); } }
        public IOStates LampAlarmSensor { get { return (bits[2][7] ? IOStates.On : IOStates.Off); } }
        #endregion


        #region ch3
        public IOStates OSP3Senor { get { return (bits[3][0] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP4Senor { get { return (bits[3][1] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP5Senor { get { return (bits[3][2] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP6Senor { get { return (bits[3][3] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP7Senor { get { return (bits[3][4] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP8Senor { get { return (bits[3][5] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP9Senor { get { return (bits[3][6] ? IOStates.On : IOStates.Off); } }
        public IOStates OSP10Senor { get { return (bits[3][7] ? IOStates.On : IOStates.Off); } }

        #endregion






        #region 일단
        //#region ch1
        //public IOStates SolClampUpSensor    { get { return (bits[1][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolClampDownSensor  { get { return (bits[1][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolDockSensor       { get { return (bits[1][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolUndockSensor     { get { return (bits[1][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolLatchSensor      { get { return (bits[1][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolUnlatchSensor    { get { return (bits[1][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolOpenSensor       { get { return (bits[1][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolCloseSensor      { get { return (bits[1][7] ? IOStates.On : IOStates.Off); } }
        //#endregion

        //#region ch2
        //public IOStates SolMappingArmSensor     { get { return (bits[2][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates SolSuctionSensor        { get { return (bits[2][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates BREAKSensor             { get { return (bits[2][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates MappingSensorOnSensor   { get { return (bits[2][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampSW1Sensor           { get { return (bits[2][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampSW2Sensor           { get { return (bits[2][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP1Sensor              { get { return (bits[2][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP2Sensor              { get { return (bits[2][7] ? IOStates.On : IOStates.Off); } }
        //#endregion

        //#region ch3
        //public IOStates LampManualSensor    { get { return (bits[3][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampAutoSensor      { get { return (bits[3][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampPresentSensor   { get { return (bits[3][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampPlacedSensor    { get { return (bits[3][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampLoadSensor      { get { return (bits[3][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampUnloadSensor    { get { return (bits[3][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampReservedSensor  { get { return (bits[3][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates LampAlarmSensor     { get { return (bits[3][7] ? IOStates.On : IOStates.Off); } }
        //#endregion


        //#region ch4
        //public IOStates OSP3Senor   { get { return (bits[4][0] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP4Senor   { get { return (bits[4][1] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP5Senor   { get { return (bits[4][2] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP6Senor   { get { return (bits[4][3] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP7Senor   { get { return (bits[4][4] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP8Senor   { get { return (bits[4][5] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP9Senor   { get { return (bits[4][6] ? IOStates.On : IOStates.Off); } }
        //public IOStates OSP10Senor  { get { return (bits[4][7] ? IOStates.On : IOStates.Off); } }

        //#endregion
        #endregion


    }

    public class TISLoadportSendMessage
    {
        public string command;
        public string parameters;
        public int waitTime;

        public string response;
        private AutoResetEvent finishedEvent;

        public TISLoadportSendMessage(string command, string parameters, int waitTime)
        {
            this.command = command;
            this.parameters = parameters;
            this.waitTime = waitTime;

            response = null;
            finishedEvent = new AutoResetEvent(false);
        }

        public string Wait()
        {
            finishedEvent.WaitOne();
            finishedEvent.Dispose();
            finishedEvent = null;

            return response;
        }

        public void Set(string response)
        {
            this.response = response;
            finishedEvent.Set();
        }
    }
    public class DeviceLoadPortTIS : DeviceTIS, IDeviceLoadPort
    {
        #region fields
        private List<TISErrors> loadportErrorList;
        private TISLoadportInputs inputPorts = new TISLoadportInputs(null, 8);

        int slotCount = 0;
        bool isOpenAndMapped = false;

        //
        private BlockingCollection<TISLoadportSendMessage> sendMessageQueue;
        private AutoResetEvent sendMessageEvent = null;      // for polling mode

        //
        private System.Threading.Tasks.Task pollingTask = null;
        private const int pollingTime = 100;    // 100ms
        bool pollingShutdown = false;
        bool isPollingMode = false;

        //
        private IDeviceMapper mapperDevice = null;
        //


        protected LoadPortInfo _LoadPortInfo;
        #endregion

        #region propertise
        public int PortNumber
        {
            get
            {
                if(Config is LoadPortConfig)
                {
                    var loadport_config = Config as LoadPortConfig;

                    return loadport_config.LoadPortIndex;
                }
                return -1;
            }
        }
        #endregion

        #region methods
        //
        public bool ParseInputPorts(string ioString)
        {
            // check input sensor..
            if (inputPorts.ToString() == ioString) return false;

            //

            TISLoadportInputs newInput = new TISLoadportInputs(ioString, 8);

            if(parse_log == true)
                AppLogger.Debug()("<* {0}", newInput.ToSensorString());


            //// Need to task.. 
            //if (newInput.PresenceSensor != inputPorts.PresenceSensor)
            //{
            //    if (newInput.PresenceSensor == IOStates.On)
            //    {
            //        RaiseDeviceEvent(PortNumber, EventCodes.CarrierPresent);

            //        if (newInput.PlacementSensor != inputPorts.PlacementSensor && newInput.PlacementSensor == IOStates.On)
            //        {
            //            RaiseDeviceEvent(PortNumber, EventCodes.CarrierPlaced);
            //        }
            //    }
            //    else
            //    {
            //        if (newInput.PlacementSensor != inputPorts.PlacementSensor && newInput.PlacementSensor == IOStates.Off)
            //        {
            //            RaiseDeviceEvent(PortNumber, EventCodes.CarrierRemoved);
            //        }

            //        RaiseDeviceEvent(PortNumber, EventCodes.CarrierNotPresent);
            //    }
            //}
            //else if (newInput.PlacementSensor != inputPorts.PlacementSensor)
            //{
            //    RaiseDeviceEvent(PortNumber, (newInput.PlacementSensor == IOStates.On) ? EventCodes.CarrierPlaced : EventCodes.CarrierRemoved);
            //}

            ////
            //if (newInput.LoadSWSensor != inputPorts.LoadSWSensor)
            //{
            //    RaiseDeviceEvent(PortNumber, (newInput.LoadSWSensor == IOStates.On) ? EventCodes.Button2Pressed : EventCodes.Button2Released);
            //}

            ////
            //if (newInput.UnloadSWSensor != inputPorts.UnloadSWSensor)
            //{
            //    RaiseDeviceEvent(PortNumber, (newInput.UnloadSWSensor == IOStates.On) ? EventCodes.Button1Pressed : EventCodes.Button1Released);
            //}

            //inputPorts.SetIOValues(ioString, 8);

            return true;
        }

        private void CreateErrorList()
        {
            loadportErrorList = new List<TISErrors>();

            loadportErrorList.Add(new TISErrors("002", "Unknown Error"));
            loadportErrorList.Add(new TISErrors("004", "Emergency"));
            loadportErrorList.Add(new TISErrors("008", "Over-Run"));
            loadportErrorList.Add(new TISErrors("009", "Barrier Error"));
            loadportErrorList.Add(new TISErrors("012", "Motor Error"));

            loadportErrorList.Add(new TISErrors("201", "Foup Clamp Up Error"));
            loadportErrorList.Add(new TISErrors("202", "Foup Clamp Down Error"));
            loadportErrorList.Add(new TISErrors("203", "Foup Clamp Lock Error"));
            loadportErrorList.Add(new TISErrors("204", "Foup Clamp Forward Error"));
            loadportErrorList.Add(new TISErrors("205", "Foup Clamp Backward Error"));
            loadportErrorList.Add(new TISErrors("206", "Foup Docking Error"));
            loadportErrorList.Add(new TISErrors("207", "Foup Undocking Error"));
            loadportErrorList.Add(new TISErrors("208", "Door Latch Error"));
            loadportErrorList.Add(new TISErrors("209", "Door unlatch Error"));
            loadportErrorList.Add(new TISErrors("210", "Door suction On Error"));
            loadportErrorList.Add(new TISErrors("211", "Door Suction Off Error"));
            loadportErrorList.Add(new TISErrors("212", "Door Open Error"));
            loadportErrorList.Add(new TISErrors("213", "Door Close Error"));
            loadportErrorList.Add(new TISErrors("214", "Mapping Arm Home Error"));
            loadportErrorList.Add(new TISErrors("215", "Mapping Arm Map Error"));
            loadportErrorList.Add(new TISErrors("216", "InterLock-Wafer protrusion from Foup"));
            loadportErrorList.Add(new TISErrors("217", "InterLock-Not sticked with Door of Foup"));
            loadportErrorList.Add(new TISErrors("221", "InterLock-Should be Foup clamper is UP"));
            loadportErrorList.Add(new TISErrors("222", "InterLock-Should be Foup Clamper is DOWN"));
            loadportErrorList.Add(new TISErrors("223", "InterLock-Should be Foup Clamper is LOCK", "RFLK/RFUK"));
            loadportErrorList.Add(new TISErrors("224", "InterLock-Should be Foup Clamper is Forward"));
            loadportErrorList.Add(new TISErrors("225", "InterLock-Should be Foup Clamper is Backward"));
            loadportErrorList.Add(new TISErrors("226", "InterLock-Should be Foup is Docking", "FBDK"));
            loadportErrorList.Add(new TISErrors("227", "InterLock-Should be Foup is Undocking", "FBUD"));
            loadportErrorList.Add(new TISErrors("228", "InterLock-Should be Door latch key is Latch", "DRLH"));
            loadportErrorList.Add(new TISErrors("229", "InterLock-Should be Door latch key is Unlatch", "DRUH"));
            loadportErrorList.Add(new TISErrors("230", "InterLock-Should be Door Suction is ON", "STON"));
            loadportErrorList.Add(new TISErrors("231", "InterLock-Should be Door Suction is OFF", "STOF"));
            loadportErrorList.Add(new TISErrors("232", "InterLock-Should be Door is Open", "DROP"));
            loadportErrorList.Add(new TISErrors("233", "InterLock-Should be Door is Close", "DRCL"));
            loadportErrorList.Add(new TISErrors("234", "InterLock-Should be Mapping arm is home.", "MAHP"));
            loadportErrorList.Add(new TISErrors("235", "InterLock-Should be Mapping arm is map position", "MAMP"));
            loadportErrorList.Add(new TISErrors("236", "InterLock-Should be DOOR is up"));
            loadportErrorList.Add(new TISErrors("237", "InterLock-Should be DOOR is down"));
            loadportErrorList.Add(new TISErrors("238", "InterLock-Should be Mapping start position"));
            loadportErrorList.Add(new TISErrors("239", "InterLock-Should be Mapping end position"));
            loadportErrorList.Add(new TISErrors("240", "InterLock-Wafer detection is error"));
            loadportErrorList.Add(new TISErrors("241", "InterLock-Main Air is error"));
            loadportErrorList.Add(new TISErrors("242", "InterLock-Foup is abnomal position"));
            loadportErrorList.Add(new TISErrors("243", "InterLock-Foup is empty"));
            loadportErrorList.Add(new TISErrors("244", "InterLock-OBSTACLE DETECTION error"));
            loadportErrorList.Add(new TISErrors("245", "InterLock-Should be mapping position"));
            loadportErrorList.Add(new TISErrors("246", "Loading condition is error"));
            loadportErrorList.Add(new TISErrors("247", "Unloading condition is error"));
        }

        private void ClosePollingTask()
        {
            if (pollingTask != null)
            {
                pollingShutdown = true;

                pollingTask.Wait();
                pollingTask.Dispose();

                pollingTask = null;
                isPollingMode = false;
                pollingShutdown = false;
            }
        }

        public override bool Shutdown()
        {
            ClosePollingTask();

            return base.Shutdown();
        }

        public void SetExternalMapper(IDeviceMapper device)
        {
            mapperDevice = device;
        }

        //
        private bool MessageSendOnPolling(string command, string parameters, int waitTime)
        {
            if (pollingTask == null)      // no wait..
            {
                return base.MessageSend(command, parameters, waitTime);
            }

            TISLoadportSendMessage message = new TISLoadportSendMessage(command, parameters, waitTime);
            sendMessageQueue.Add(message);
            sendMessageEvent.Set();
            responseParameters = message.Wait();

            return true;
        }

        public TISLoadportInputs GetInputStatus(int ch)
        {
            string parameters = string.Format($"{ch}");
            var oss = MessageSendOnPolling("IDI", parameters, REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetInputStatus({ch}) failed.");
                return null;
                //return new OperationStatus<TISLoadportInputs>(null, oss.ErrorInfo);
            }



            return new TISLoadportInputs(ResponseParameters, 8);
        }
        public TISLoadportOutputs GetOutputStatus(int ch)
        {
            string parameters = string.Format($"{ch}");
            var oss = MessageSendOnPolling("IDO", parameters, REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetOutputStatus({ch}) failed.");
                return null;
                //return new OperationStatus<TISLoadportInputs>(null, oss.ErrorInfo);
            }



            return new TISLoadportOutputs(ResponseParameters, 8);
        }

        public TISLoadportInputs GetInputPorts()
        {
            //string parameters = string.Format("0,3");
            //var oss = MessageSendOnPolling("IDI", parameters, REQUEST_TIMEOUT);
            //if (oss == false)
            //{
            //    AppLogger.Error()($"{Name} device GetInputPorts() failed.");
            //    return null;
            //    //return new OperationStatus<TISLoadportInputs>(null, oss.ErrorInfo);
            //}



            //return new TISLoadportInputs(ResponseParameters, 8);

            TISLoadportInputs i1 = GetInputStatus(1);
            string r1 = ResponseParameters;
            TISLoadportInputs i2 = GetInputStatus(2);
            string r2 = ResponseParameters;
            TISLoadportInputs i3 = GetInputStatus(3);
            string r3 = ResponseParameters;

            TISLoadportInputs i4 = GetInputStatus(4);
            string r4 = ResponseParameters;

            if (i1 != null && i2 != null && i3 != null)
            {
                var t1 = r1.Split(',');
                var t2 = r2.Split(',');
                var t3 = r3.Split(',');
                var t4 = r4.Split(',');

                string data = string.Format($"{t1[1]},{t2[1]},{t3[1]},{t4[1]}");

                var inputs = new TISLoadportInputs(data, 8);
                //AppLogger.Debug()("Loadport Inputs : [{0}]", inputs.ToSensorString());
                return inputs;                
            }

            return null;
        }

        public TISLoadportOutputs GetOutputPort()
        {
            //int nbyte = 8;
            //string parameters = string.Format("0,{0}", nbyte);
            //var oss = MessageSendOnPolling("IDO", parameters, REQUEST_TIMEOUT);
            //if (oss == false)
            //{
            //    AppLogger.Error()($"{Name} device GetOutputPort() failed.");
            //    return null;
            //}

            //return new TISLoadportOutputs(ResponseParameters, nbyte);




            TISLoadportOutputs i1 = GetOutputStatus(0);
            string r1 = ResponseParameters;
            TISLoadportOutputs i2 = GetOutputStatus(1);
            string r2 = ResponseParameters;
            TISLoadportOutputs i3 = GetOutputStatus(2);
            string r3 = ResponseParameters;

            if (i1 != null && i2 != null && i3 != null)
            {
                var t1 = r1.Split(',');
                var t2 = r2.Split(',');
                var t3 = r3.Split(',');


                string data = string.Format($"{t1[1]},{t2[1]},{t3[1]}");
                return new TISLoadportOutputs(data, 8);

            }

            return null;
        }

        public LoadPortInfo GetPortInfo()
        {
            //TISLoadportInputs osi = GetInputPorts();
            //if (osi == null)
            //{
            //    return null;
            //}

            return _LoadPortInfo;
        }

        public bool InitPort()
        {

            //TISLoadportInputs i1 = GetInputStatus(0);
            //TISLoadportInputs i2 = GetInputStatus(1);
            //TISLoadportInputs i3 = GetInputStatus(2);
            



            TISLoadportInputs osi = GetInputPorts();
            if (osi == null)
            {
                return false;
            }

            // saved port states..
            inputPorts = osi;

            //
            TISLoadportOutputs oso = GetOutputPort();
            if (oso == null)
            {
                return false;
                //return new OperationStatus(oso.ErrorInfo);
            }

            //need to origin..
            //if (oso.OriginCompleteSensor != IOStates.On)
            //{
            //    var os = Homing();
            //    if (os.IsError)
            //    {
            //        return os;
            //    }
            //}

            //
            //if (osi.DoorOpenSensor == IOStates.On)
            //{
            //    return true;
            //}

            //
            //if (
            //    (
            //        osi.Place1Sensor== IOStates.Off ||
            //        osi.Place2Sensor == IOStates.Off ||
            //        osi.Place3Sensor == IOStates.Off
            //    )
            //    &&
            //    osi.DockSensor == IOStates.Off)
            {
                return Homing();
            }

            return true;
        }

        public override bool HWInitialize()
        {
            ClosePollingTask();


            var os = base.HWInitialize();
            if (os == false)
            {
                return false;
            }

            //
            os = InitPort();
            if (os == false)
            {
                return false;
            }


            //os = Homing();
            //if(os == false)
            //{
            //    return false;
            //}

            pollingTask = new System.Threading.Tasks.Task((e) =>
            {
                string response = string.Empty;
                TISLoadportSendMessage message = null;

                while (!pollingShutdown)
                {
                    if (sendMessageQueue.TryTake(out message))
                    {
                        var r = MessageSend(message.command, message.parameters, message.waitTime);
                        message.Set(ResponseParameters);
                    }
                    //else
                    //{
                    //    isPollingMode = true;

                    //    parse_log = false;
                    //    var b1 = MessageSend("IDI", "0", 1000, false);
                    //    string r1 = ResponseParameters;

                    //    var b2 = MessageSend("IDI", "1", 1000, false);
                    //    string r2 = ResponseParameters;

                    //    var b3 = MessageSend("IDI", "2", 1000, false);
                    //    string r3 = ResponseParameters;



                    //    var t1 = r1.Split(',');
                    //    var t2 = r2.Split(',');
                    //    var t3 = r3.Split(',');

                    //    if(t1.Length > 1 || t2.Length > 1 || t3.Length > 1)
                    //    {
                    //        string data = string.Format($"{t1[1]},{t2[1]},{t3[1]}");
                                               
                    //        if (b1 != false && b2 != false && b3 != false)
                    //        {
                    //            ParseInputPorts(data);
                    //        }
                    //        parse_log = true;
                    //        isPollingMode = false;
                    //        sendMessageEvent.WaitOne(100);
                    //    }
                    //}
                }
            }, null, TaskCreationOptions.LongRunning);

            pollingTask.Start();

            return true;
        }

        //
        public bool ClampCarrier()
        {
            //var os = MessageSendOnPolling("CLCL", null, ACTION_TIMEOUT);
            var os = MessageSendOnPolling("CPDN", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device ClampCarrier(CPDN) failed.");
                return false;
            }

            RaiseDeviceEvent(PortNumber, EventCodes.CarrierClamped);

            return true;
        }
        public bool UnclampCarrier()
        {
            //var os = MessageSendOnPolling("CLOP", null, ACTION_TIMEOUT);
            var os = MessageSendOnPolling("CPUP", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device UnclampCarrier(CPUP) failed.");

                return false;
            }

            RaiseDeviceEvent(PortNumber, EventCodes.CarrierUnclamped);

            return true;
        }

        //
        public bool OpenDoor()
        {
            //on the dock and Door unlatch
            //OperationStatus os;

            //os = MessageSendOnPolling("RLDL", null, ACTION_TIMEOUT);       // FBDK -> STON -> DRUH
            //if (os.IsError)
            //{
            //    return os;
            //}

            //var os = MessageSendOnPolling("VCON", null, ACTION_TIMEOUT);       // Door Suction ON
            var os = MessageSendOnPolling("STON", null, ACTION_TIMEOUT);
            if (os == false)
            {

                AppLogger.Error()($"{Name}-{Device_Index} device OpenDoor(STON) failed.");

                return false;
            }

            os = MessageSendOnPolling("DRLH", null, ACTION_TIMEOUT);       // Door Up
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device OpenDoor(DRLH) failed.");
                return false;
            }

            // open and mapping..
            isOpenAndMapped = false;
            os = MessageSendOnPolling("RLDM", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device OpenDoor(RLDM) failed.");
                return false;
            }

            //
            isOpenAndMapped = true;
            RaiseDeviceEvent(PortNumber, EventCodes.DoorOpened);

            return true;

            AppLogger.Info()($"OpenDoor() - [{Name}-{Device_Index} device is OpenDoor");
        }
        public bool CloseDoor()
        {
            //Close door and goto home mapping arm 
            var os = MessageSendOnPolling("CULDK", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device CloseDoor(CULDK) failed.");
                return false;
            }

            //Door on the Latch
            os = MessageSendOnPolling("DRCL", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device CloseDoor(DRCL) failed.");
                return false;
            }

            //Door Suction OFF
            os = MessageSendOnPolling("STOF", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device CloseDoor(STOF) failed.");
                return false;
            }


            //Door Latch OFF
            os = MessageSendOnPolling("DRUH", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device CloseDoor(STOF) failed.");
                return false;
            }

            RaiseDeviceEvent(PortNumber, EventCodes.DoorClosed);

            return true;
        }

        //
        public bool DockCarrier()
        {
            //var os = MessageSendOnPolling("SLFW", null, ACTION_TIMEOUT);
            var os = MessageSendOnPolling("FBDK", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device DockCarrier(FBDK) failed.");

                return false;
            }

            RaiseDeviceEvent(PortNumber, EventCodes.CarrierUnclamped);

            return true;
            AppLogger.Info()($"DockCarrier() - [{Name}-{Device_Index} device is DockCarrier");
        }

        public bool UndockCarrier()
        {
            //var os = MessageSendOnPolling("SLBW", null, ACTION_TIMEOUT);
            var os = MessageSendOnPolling("FBUD", null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device UndockCarrier(FBUD) failed.");

                return false;
            }

            RaiseDeviceEvent(PortNumber, EventCodes.CarrierUnclamped);

            return true;
        }

        public ClampStates GetClampState()
        {
            AppLogger.Info()($"GetClampState() - [{Name}-{Device_Index} device ClampState is {ClampStates.Undefined}");

            return ClampStates.Undefined;
        }

        public DockStates GetDockState()
        {
            AppLogger.Info()($"GetDockState() - [{Name}-{Device_Index} device DockState is {DockStates.Undefined}");

            return DockStates.Undefined;
        }

        public DoorStates GetDoorState()
        {
            AppLogger.Info()($"GetDoorState() - [{Name}-{Device_Index} device DoorState is {DoorStates.Undefined}");

            return DoorStates.Undefined;
        }

        public bool GetPlacement()
        {
            LoadPortInfo osl = GetPortInfo();
            if (osl == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetPlacement() failed.");

                return false;
                //return new OperationStatus<bool>(false, osl.ErrorInfo);
            }

            return osl.CarrierPlacement;
        }

        public bool GetPresent()
        {
            LoadPortInfo osl = GetPortInfo();
            if (osl == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetPresent() failed.");

                return false;
                //return new OperationStatus<bool>(false, osl.ErrorInfo);
            }

            return osl.CarrierPresence;
        }

        public SlotMapResult ParsingMapResult(string rdata)
        {
            var data_array = rdata.Split(',');
            SlotMapResult mapResult = SlotMapResult.CreateNew();

            SlotStates ss = SlotStates.Empty;

            for (int slotId = 0; slotId < rdata.Length ; slotId++)
            {
                char c = rdata[slotId];
                switch (c)
                {
                    case '1': ss = SlotStates.Correct; break;
                    case 'D': ss = SlotStates.DoubleSlotted; break;
                    case 'C': ss = SlotStates.CrossSlotted; break;
                    case '?': ss = SlotStates.Undefined; break;
                    default: ss = SlotStates.Empty; break;
                }

                mapResult.SlotMap.Add(new CarrierSlot(slotId + 1, ss));
            }
            return mapResult;
        }

        public SlotMapResult ReadMapData()
        {
            //MLD 1
            //MLD 111111111111CC11111DD00100 
            //MLD EOD
            var oss = MessageSendOnPolling("MLD1", null, ACTION_TIMEOUT);
            if (oss == false)
            {
                var slotMap = SlotMapResult.CreateNew();
                slotMap.Status = ReadStatus.ReadFailed;
                return slotMap;
            }



            //mapData = "0000000000000000111111111";

            var slot_data = mapData.Split(',');
            slotCount = slot_data.Length;

            var slot_mapdata = string.Empty;

            for (int i = slot_data.Length - 1; i >= 0; i--)
            {
                slot_mapdata += slot_data[i];
            }


            //for (int i = 0; i < slot_data.Length; i++)
            //{
            //    slot_mapdata += slot_data[i];
            //}







            //if(mapData != string.Empty)
            //{
            //    var slot_data = mapData.Split(',');
            //    slotCount = slot_data.Length;
            //}
            //else
            //{
            //    slotCount = 0;
            //}


            //// last char is dummy '0'
            //if ((ResponseParameters.Length - 1) != slotCount)
            //{
            //    AppLogger.Debug()("** {0} (Not match slot count {1})", ResponseParameters, ResponseParameters.Length - 1);

            //    var slotMap = SlotMapResult.CreateNew();
            //    slotMap.Status = ReadStatus.ReadFailed;

            //    return slotMap;
            //}
            AppLogger.Debug()("Carrier Map Data : [{0}].", mapData);

            SlotMapResult mapResult = ParsingMapResult(slot_mapdata);
            mapResult.Status = ReadStatus.ReadOK;

            return mapResult;
        }


        public SlotMapResult MapCarrier()
        {
            if (mapperDevice != null)
            {
                return mapperDevice.MapCarrier(PortNumber);
            }

            if (isOpenAndMapped == false)
            {
                var os = MessageSendOnPolling("RMAP", null, ACTION_TIMEOUT);
                if (os == false)
                {
                    var slotMap = SlotMapResult.CreateNew();
                    slotMap.Status = ReadStatus.ReadFailed;

                    return slotMap;
                }
            }

            isOpenAndMapped = false;
            SlotMapResult oss = ReadMapData();
            if (oss.Status == ReadStatus.ReadFailed)
            {
                return oss;
            }

            //
            RaiseDeviceEvent(PortNumber, EventCodes.CarrierMapped, null);

            return oss;
        }

        public void SetLightStates(List<LoadPortLightState> loadPortLightsMapping)
        {
            throw new NotImplementedException();
        }

        public void SetSlotCount(int slotCount)
        {
            AppLogger.Info()($"{Name}-{Device_Index} device set slot count : {slotCount}");
        }

        public override TISErrors ParseErrorMessage(string command, string parameter)
        {
            // if query of error ? true is ignore
            if (command == "ERR")
            {
                return null;
            }

            var error = loadportErrorList.Find(s => s.ErrorCode == parameter);
            if (error != null)
            {
                return error;
            }

            return base.ParseErrorMessage(command, parameter);
        }


        public override bool UserCommand(string command)
        {
            string cmd = string.Format("{0}", command);
            var os = MessageSendOnPolling(cmd, null, ACTION_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device UserCommand({cmd}) failed.");
                return false;
            }
            return true;
        }

        public void SetLoadPortInfo(LoadPortInfo info)
        {
            _LoadPortInfo = info.Duplicate();

            foreach(var slot in info.SlotMap)
            {
                slot.SubstrateInfo.StartTime = DateTime.Now;
            }
        }
        #endregion

        #region Events
        public event NotifyDeviceEvent notifyDeviceEvent;
        //
        public virtual void RegisterEvent(NotifyDeviceEvent notifyDeviceEvent)
        {
            this.notifyDeviceEvent += notifyDeviceEvent;
        }

        //
        public virtual void ClearResiterEvents()
        {
            this.notifyDeviceEvent = null;

            //this.deviceConnected = null;
            //this.deviceDisconnected = null;
        }

        public virtual void RaiseDeviceEvent(int portId, EventCodes code, object data = null)
        {
            if (this.notifyDeviceEvent != null)
            {
                this.notifyDeviceEvent(portId, code, data);
            }
        }
        #endregion

        #region constructors
        public DeviceLoadPortTIS(NetworkConfig config)
            : base(DeviceTypes.LoadPort_TIS, config)
        {
            CreateErrorList();

            _LoadPortInfo = new LoadPortInfo(PortNumber, false, false, ClampStates.Undefined, DockStates.Undefined, DoorStates.Undefined);

            sendMessageEvent = new AutoResetEvent(false);
            sendMessageQueue = new BlockingCollection<TISLoadportSendMessage>();
        }
        #endregion


    }
}
