using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public delegate void UpdateInputIOData(int ch, byte[] data);
    public delegate void UpdateOutputIOData(int ch, byte[] data);

    public class UnitEFEMIO : UnitTyped
    {

        #region fields
        protected event UpdateInputIOData eventUpdateInputIOData = null;
        protected event UpdateOutputIOData eventUpdateOutputIOData = null;

        private System.Threading.Tasks.Task task = null;
        bool io_search_shutdown = false;
        #endregion

        #region propertise
        public IDeviceIO Device
        {
            get { return base.BaseDevice as IDeviceIO; }

            set
            {
                base.BaseDevice = value;
            }
        }


        new public EFEMIOConfig Config
        {
            get
            {
                return (base.Config as EFEMIOConfig);
            }
        }
        #endregion

        #region methods
        protected byte[] BitArrayToByteArray(BitArray bitArray)
        {
            // BitArray의 크기(비트 수)를 가져옴
            int byteCount = (bitArray.Count + 7) / 8; // 8비트로 나누고 올림 처리

            byte[] byteArray = new byte[byteCount];

            // BitArray의 각 비트를 byte로 변환하여 배열에 저장
            bitArray.CopyTo(byteArray, 0);

            return byteArray;
        }


        public void RegisterIOEvent(UpdateInputIOData eventUpdateInputIOData, UpdateOutputIOData eventUpdateOutputIOData)
        {
            this.eventUpdateInputIOData += eventUpdateInputIOData;
            this.eventUpdateOutputIOData += eventUpdateOutputIOData;
        }

        private void CloseIOSearchTask()
        {
            if (task != null)
            {
                io_search_shutdown = true;

                task.Wait();
                task.Dispose();

                task = null;
                io_search_shutdown = false;
            }
            AppLogger.Debug()($"[{UnitName}] unit closeIOSearchTask shut down success.");
        }

        public override IDevice CreateDevice()
        {
            IDevice device = null;
            if(DeviceType == DeviceTypes.Crevis_IO)
            {
                //device = new DeviceEFEMIOCrevis(Config.IpAddress, Config.Port);
                device = new DeviceIO_Crevis(Config.IpAddress);
            }
            else
            {
                device = new DeviceEFEMIOSim(Config.IpAddress, Config.Port);
            }

            return device;
        }

        protected void RaiseInputIO(byte [] input_data)
        {
            if(eventUpdateInputIOData != null)
            {
                CrevisSensors sensor = new CrevisSensors(input_data);

                for(int i = 0; i < sensor.Bits.Length; i++)
                {
                    var datas = BitArrayToByteArray(sensor.Bits[i]);

                    eventUpdateInputIOData(i, datas);
                }
            }
        }

        protected void RaiseOutputIO(byte[] output_data)
        {
            if(eventUpdateOutputIOData != null)
            {
                CrevisSensors sensor = new CrevisSensors(output_data);

                for (int i = 0; i < sensor.Bits.Length; i++)
                {
                    var datas = BitArrayToByteArray(sensor.Bits[i]);

                    eventUpdateOutputIOData(i, datas);
                }
            }
        }

        public override bool Initialize()
        {
            AppLogger.Info()($"[{UnitName}] unit initialize start");


            try
            {
                task = new Task((e) =>
                {
                    while (!io_search_shutdown)
                    {

                        var input_data = Device.GetAllDigitalInput();
                        RaiseInputIO(input_data);

                        var output_data = Device.GetAllDigitalOutput();
                        RaiseOutputIO(output_data);




                        Thread.Sleep(100);

                        //AppLogger.Debug()($"[{UnitName}] unit io search thread running.");
                    }


                }, null, TaskCreationOptions.LongRunning);

                task.Start();
                AppLogger.Info()($"[{UnitName}] unit io search thread start.");

                //Thread.Sleep(1000);

                AppLogger.Info()($"[{UnitName}] unit initialize success.");
                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()($"[{UnitName}] unit initialize failed.");
                return false;
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();


            CloseIOSearchTask();

            if(Device != null)
                Device.Disconnect();

        }


        public bool SetOutputBits(int ch, int no, IOStates state)
        {
            var low_ch = ch * 2;
            var high_ch = low_ch + 1;


            var tch = low_ch;
            var tno = no;

            if(Device != null)
            {
                if(no > 7)
                {
                    tch = high_ch;
                    tno = no - 8;
                }



                return Device.SetDigitalOut(tch, tno, state);
            }

            AppLogger.Error()($"[{UnitName}] unit is not initialized. SetOutputBits({ch}, {no}, {state}) is failed.");

            return false;
        }
        #endregion

        #region constructors
        public UnitEFEMIO(UnitIDs unitID)
            : base(unitID)
        {
        }
        #endregion




    }
}
