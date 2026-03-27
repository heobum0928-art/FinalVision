using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrevisFnIoLib;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;

namespace Project.HWC
{

    public class DeviceIO_Crevis : DeviceBase, IDeviceIO
    {

        #region fields

        protected object _lock_obj = new object();


        protected CrevisFnIO hCrevisIO;

        protected IntPtr hSystem;
        protected IntPtr hDevice;

        protected CrevisFnIO.DEVICEINFOMODBUSTCP2 DeviceInfo;

        protected Dictionary<int, string> errorMsg;

        protected int input_image_size;
        protected byte[] InputImageData = null;


        protected int output_image_size;
        protected byte[] OutputImageData = null;

        protected bool update_frequence_start = false;

        #endregion

        #region propertise
        public string IpAddress { get; set; } = string.Empty;
        #endregion

        #region methods

        public override bool Connect()
        {
            var res = InitSystem();
            if (res != true)
            {
                AppLogger.Error()($"[{DeviceType}] device InitSystem() is failed.");
                return false;
            }

            res = OpenDevice();
            if (res != true)
            {
                AppLogger.Error()($"[{DeviceType}] device OpenDevice() is failed.");
                return false;
            }

            res = SetUpdateFrequency();
            if (res != true)
            {
                AppLogger.Error()($"[{DeviceType}] device SetUpdateFrequency() is failed.");
                return false;
            }





            return true;
        }

        public override bool Disconnect()
        {
            var Err = hCrevisIO.FNIO_DevCloseDevice(hDevice);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                Console.Write("Failed to close the device.\n ");
                hCrevisIO.FNIO_LibFreeSystem(hSystem);
                return false;
            }
            hCrevisIO.FNIO_LibFreeSystem(hSystem);

            return true;
        }

        public bool InitSystem()
        {
            hSystem = new IntPtr();
            var Err = hCrevisIO.FNIO_LibInitSystem(ref hSystem);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                hCrevisIO.FNIO_LibFreeSystem(hSystem);
                AppLogger.Error()($"[{DeviceType}] device failed to Initialize the system. [{GetErrorMessage(Err)}]");
                return false;
            }

            AppLogger.Info()($"[{DeviceType}] device system alloc successs.");
            return true;
        }

        public bool OpenDevice()
        {
            if (IpAddress == string.Empty)
            {
                AppLogger.Error()($"[{DeviceType}] device IpAddress is null. IpAddress : [{IpAddress}]");
                return false;
            }

            hDevice = new IntPtr();



            CrevisFnIO.DEVICEINFOMODBUSTCP2 DeviceInfo = new CrevisFnIO.DEVICEINFOMODBUSTCP2();
            DeviceInfo.IpAddress = new byte[4];
            
            int i = 0;
            string[] words = IpAddress.Split('.');
            foreach (string word in words)
            {
                DeviceInfo.IpAddress[i] = (byte)(Int32.Parse(word));
                i++;
            }

            var Err = hCrevisIO.FNIO_DevOpenDevice(hSystem, ref DeviceInfo, CrevisFnIO.MODBUS_TCP, ref hDevice);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device failed to OpenDevice. ErrorCode : [{GetErrorMessage(Err)}]");
                hCrevisIO.FNIO_LibFreeSystem(hSystem);
                return false;
            }

            AppLogger.Info()($"[{DeviceType}] device [{IpAddress}] Connected.");
            return true;
        }

        public bool SetUpdateFrequency()
        {
            update_frequence_start = false;

            int val = 0;
            var Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_INPUT_IMAGE_SIZE, ref val);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device failed to SetUpdateFrequency(DEV_INPUT_IMAGE_SIZE). ErrorCode : [{GetErrorMessage(Err)}]");
                return false;
            }

            input_image_size = val;

            InputImageData = new byte[input_image_size];

            val = 0;
            Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_OUTPUT_IMAGE_SIZE, ref val);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device failed to SetUpdateFrequency(DEV_OUTPUT_IMAGE_SIZE). ErrorCode : [{GetErrorMessage(Err)}]");
                return false;
            }

            output_image_size = val;

            OutputImageData = new byte[output_image_size];

            AppLogger.Debug()($"[{DeviceType}] device Read[{input_image_size}] / Write[{output_image_size}] data alloc success.");

            val = 0;
            Err = hCrevisIO.FNIO_DevSetParam(hDevice, CrevisFnIO.DEV_UPDATE_FREQUENCY, val);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device failed to SetUpdateFrequency(DEV_UPDATE_FREQUENCY). ErrorCode : [{GetErrorMessage(Err)}]");
                return false;
            }



            val = 1000;	//1 s 
            Err = hCrevisIO.FNIO_DevSetParam(hDevice, CrevisFnIO.DEV_RESPONSE_TIMEOUT, val);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {

                AppLogger.Error()($"[{DeviceType}] device failed to SetUpdateFrequency(DEV_RESPONSE_TIMEOUT). ErrorCode : [{GetErrorMessage(Err)}]");
                return false;
            }


            Err = hCrevisIO.FNIO_DevIoUpdateStart(hDevice, CrevisFnIO.IO_UPDATE_PERIODIC);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device failed to SetUpdateFrequency(IO_UPDATE_PERIODIC). ErrorCode : [{GetErrorMessage(Err)}]");
                return false;
            }


            update_frequence_start = true;
            AppLogger.Debug()($"[{DeviceType}] device IO data update start success.");

            return true;
        }

        public bool DeviceInformation()
        {
            string information = string.Empty;

            int val = 0;
            string txt = string.Empty;
            int Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_PRODUCT_CODE, ref val);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device Get Product Code. [{GetErrorMessage(Err)}]");
                return false;
            }

            information = string.Format("Product Code : 0x{0:X}", val);

            var strBuff = "";
            Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_PROCDUCT_NAME, ref strBuff);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device Get Product Name. [{GetErrorMessage(Err)}]");
                return false;
            }


            information += ", " + string.Format($"Name : {strBuff}");

            strBuff = "";
            Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_FIRMWARE_VERSION, ref strBuff);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device Get Firmware Version. [{GetErrorMessage(Err)}]");
                return false;
            }

            information += ", " + string.Format($"Firmware version : {strBuff}");

            strBuff = "";
            Err = hCrevisIO.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_FIRMWARE_RELEASE_DATE, ref strBuff);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] device Get Firmware Release Date. [{GetErrorMessage(Err)}]");
                return false;
            }

            information += ", " + string.Format($"Release date : {strBuff}");

            AppLogger.Info()($"[{DeviceType}] device Information : {information}");
            return true;
        }

        protected byte[] BitArrayToByteArray(BitArray bitArray)
        {
            // BitArray의 크기(비트 수)를 가져옴
            int byteCount = (bitArray.Count + 7) / 8; // 8비트로 나누고 올림 처리

            byte[] byteArray = new byte[byteCount];

            // BitArray의 각 비트를 byte로 변환하여 배열에 저장
            bitArray.CopyTo(byteArray, 0);

            return byteArray;
        }

        public string SetUserCommand(string command)
        {
            throw new NotImplementedException();
        }

        public string GetErrorMessage(int code)
        {
            if (errorMsg.ContainsKey(code))
            {
                return errorMsg[code];
            }

            return string.Format($"[{code}] code is not defined.");
        }


        public byte[] GetDigitalInputs(int ch)
        {
            var Err = hCrevisIO.FNIO_DevReadInputImage(hDevice, 0, ref InputImageData[0], input_image_size);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] GetDigitalInputs({ch}). Message : [{GetErrorMessage(Err)}]");
                return null;
            }

            var CrevisSensor = new CrevisSensors(InputImageData);

            return BitArrayToByteArray(CrevisSensor.Bits[ch]);
        }

        public IOStates GetDigitalInput(int ch, int no)
        {
            var Err = hCrevisIO.FNIO_DevReadInputImage(hDevice, 0, ref InputImageData[0], input_image_size);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] GetDigitalInputs({ch}, {no}). Message : [{GetErrorMessage(Err)}]");
                return IOStates.Unknown;
            }

            var CrevisSensor = new CrevisSensors(InputImageData);

            return CrevisSensor.Bits[ch][no] ? IOStates.On : IOStates.Off;

        }

        public byte[] GetDigitalOutputs(int ch)
        {
            lock(_lock_obj)
            {
                var Err = hCrevisIO.FNIO_DevReadOutputImage(hDevice, 0, ref OutputImageData[0], output_image_size);
                if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                {
                    AppLogger.Error()($"[{DeviceType}] GetDigitalOutputs({ch}). Message : [{GetErrorMessage(Err)}]");
                    return null;
                }
                var CrevisSensor = new CrevisSensors(OutputImageData);
                return BitArrayToByteArray(CrevisSensor.Bits[ch]);
            }
        }

        public IOStates GetDigitalOutput(int ch, int no)
        {
            var Err = hCrevisIO.FNIO_DevReadInputImage(hDevice, 0, ref OutputImageData[0], output_image_size);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] GetDigitalOutput({ch}, {no}). Message : [{GetErrorMessage(Err)}]");
                return IOStates.Unknown;
            }

            var CrevisSensor = new CrevisSensors(OutputImageData);

            return CrevisSensor.Bits[ch][no] ? IOStates.On : IOStates.Off;
        }

        public bool SetDigitalOut(int ch, int no, IOStates state)
        {
            var Err = hCrevisIO.FNIO_DevWriteOutputImageBit(hDevice, ch, no, state == IOStates.On ? 1 : 0);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] SetDigitalOut({ch}, {no}, {state}). Message : [{GetErrorMessage(Err)}]");
                return false;
            }

            AppLogger.Debug()($"[{DeviceType}] SetDigitalOut({ch}, {no}, {state}) success.");
            return false;
        }

        public byte[] GetAllDigitalInput()
        {
            var Err = hCrevisIO.FNIO_DevReadInputImage(hDevice, 0, ref InputImageData[0], input_image_size);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] GetAllDigitalInput(). Message : [{GetErrorMessage(Err)}]");
                return null;
            }

            byte[] return_data = new byte[InputImageData.Length];

            Array.Copy(InputImageData, return_data, InputImageData.Length);

            return return_data;
        }

        public byte[] GetAllDigitalOutput()
        {
            var Err = hCrevisIO.FNIO_DevReadOutputImage(hDevice, 0, ref OutputImageData[0], output_image_size);
            if (Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                AppLogger.Error()($"[{DeviceType}] GetAllDigitalOutput(). Message : [{GetErrorMessage(Err)}]");
                return null;
            }

            byte[] return_data = new byte[OutputImageData.Length];

            Array.Copy(OutputImageData, return_data, OutputImageData.Length);

            return return_data;
        }

        
        #endregion

        #region constructors
        public DeviceIO_Crevis(string IpAddress)
            : base(DeviceTypes.Crevis_IO)
        {
            this.IpAddress = IpAddress;

            hCrevisIO = new CrevisFnIO();

            DeviceInfo = new CrevisFnIO.DEVICEINFOMODBUSTCP2();

            errorMsg = new Dictionary<int, string>()
            {
                {CrevisFnIO.FNIO_ERROR_SUCCESS, "Operation was sucessful, no error occurred." },
                {CrevisFnIO.FNIO_ERROR_DEVICE_CONNECT_FAIL, "Device connection was failed." },
                {CrevisFnIO.FNIO_ERROR_MAX_CONNECTION_EXCEEDED, "Device list is full." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_DEVICE_TYPE, "The provided device type is not supported." },
                {CrevisFnIO.FNIO_ERROR_SYSTEM_ALREADY_INIT, "The System module was open." },
                {CrevisFnIO.FNIO_ERROR_SYSTEM_ALLOC_FAIL, "The System module allocation was failed." },
                {CrevisFnIO.FNIO_ERROR_SYSTEM_NOT_EXIST, "The System module was not initialized. LibInitSystem() function was not called." },
                {CrevisFnIO.FNIO_ERROR_SYSTEM_CHECK_FAIL, "The System handle is the wrong handle or NULL pointer." },
                {CrevisFnIO.FNIO_ERROR_WRITE_ONLY_COMMAND, "The requested command is only allowed to 'Write'." },
                {CrevisFnIO.FNIO_ERROR_READ_ONLY_COMMAND, "The requested command is only allowed to 'Read'." },

                {CrevisFnIO.FNIO_ERROR_NOT_DEFINE_COMMAND, "The requested command is not defined or supported." },
                {CrevisFnIO.FNIO_ERROR_NOT_SUPPORT_COMMAND, "The requested command is not supported." },
                {CrevisFnIO.FNIO_ERROR_DUPLICATE_CONNECT, "The requested device is used." },
                {CrevisFnIO.FNIO_ERROR_DEVICEINFO_ALLOC_FAIL, "Deviceinfo module allocation was failed." },
                {CrevisFnIO.FNIO_ERROR_DEVICE_ALLOC_FAIL, "Device module allocation was failed." },
                {CrevisFnIO.FNIO_ERROR_UNKNOWN_MODEL, "Not defined model." },
                {CrevisFnIO.FNIO_ERROR_BUFFERSIZE_SMALL, "The size of the provided buffer is too small." },
                {CrevisFnIO.FNIO_ERROR_DEVICE_INDEX_EXCESS, "The requested index of device is out of the range." },
                {CrevisFnIO.FNIO_ERROR_NOT_EXECUTE, "Command is not executed." },
                {CrevisFnIO.FNIO_ERROR_DEVICE_CHECK_FAIL, "Device handle is the wrong handle or NULL pointer." },

                {CrevisFnIO.FNIO_ERROR_PORT_ALLOC_FAIL, "Port module allocation was failed." },
                {CrevisFnIO.FNIO_ERROR_IO_MODULE_ALLOC_FAIL, "IO module allocation was failed." },
                {CrevisFnIO.FNIO_ERROR_BUFFER_ALLOC_FAIL, "Buffer module alloction was failed." },
                {CrevisFnIO.FNIO_ERROR_NULL_BUFFER, "The provieded buffer is NULL pointer." },
                {CrevisFnIO.FNIO_ERROR_LIST_INDEX_EXCESS, "The requested index of list is out of the range." },
                {CrevisFnIO.FNIO_ERROR_SLOT_CHECK_FAIL, "IO module handle is the wrong handle or NULL pointer." },
                {CrevisFnIO.FNIO_ERROR_NOT_DEFINE_DATAINFO, "The requested information is not defined." },
                {CrevisFnIO.FNIO_ERROR_NOT_SUPPORT_DATAINFO, "The requested information is not supported." },
                {CrevisFnIO.FNIO_ERROR_NOT_DEFINE_EVENT, "The requested event is not defined." },
                {CrevisFnIO.FNIO_ERROR_NOT_AVAILABLE_PORT, "Device port is not available." },

                {CrevisFnIO.FNIO_ERROR_INVALID_IMAGE_ADDRESS, "The requested address is out of the range or not in the operation." },
                {CrevisFnIO.FNIO_ERROR_INVALID_IMAGE_LENGHT, "The requested lenght is out of the range." },
                {CrevisFnIO.FNIO_ERROR_INVALID_BIT_INDEX, "The requested bit Index is out of the range." },
                {CrevisFnIO.FNIO_ERROR_COMPORT_ALLOC_FAIL, "This function is not supported in this device module." },
                {CrevisFnIO.FNIO_ERROR_COMPORT_OPEN_FAIL, "Time out occurred during updating the IO data using IO_UPDATE_EVENT method." },
                {CrevisFnIO.FNIO_ERROR_SERPORT_ALREADY_IN_USE, "This IO Update Type is not supported." },
                {CrevisFnIO.FNIO_ERROR_NODEADDR_ALREADY_IN_USE, "IO Update is already running." },
                {CrevisFnIO.FNIO_ERROR_DIFFERENT_SERIAL_INFO, "This is not defined IO Access Type." },
                {CrevisFnIO.FNIO_ERROR_NOT_SEND_MESSAGE, "Failed to create safety timer." },
                {CrevisFnIO.FNIO_ERROR_DIFFERNT_PORT_TYPE, "Unspecified runtime error." },
                {CrevisFnIO.FNIO_ERROR_NOT_WORK_IO_UPDATE, "COM port module allocation was failed." },

                {CrevisFnIO.FNIO_ERROR_ILLEGAL_FUNCTION, "The requested function code is not allowed." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_DATA_ADDRESS, "The requested data address is not allowed." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_DATA_VALUE, "The requested data value is not allowed." },
                {CrevisFnIO.FNIO_ERROR_SLAVE_DEVICE_FAILURE, "An unrecover error occurred while the server (or slave) was attempting to perform the requested action." },
                {CrevisFnIO.FNIO_ERROR_ACKNOWLEDGE, "The requested action will take a long duration." },
                {CrevisFnIO.FNIO_ERROR_SLAVE_DEVICE_BUSY, "The Device is engaged in processing a long-duration program command." },
                {CrevisFnIO.FNIO_ERROR_MEMORY_PARITY_ERROR, "The extended file area failed to pass a consistency check." },
                {CrevisFnIO.FNIO_ERROR_GATEWAY_PATH_UNAVAILABLE, "The gateway was unable to allocate an internal communication path from the input port to the output port for processing the request." },
                {CrevisFnIO.FNIO_ERROR_GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND, "No response was obtained from the target device." },
                {CrevisFnIO.FNIO_ERROR_UNKNOW, "Unspecified runtime error." },

                {CrevisFnIO.FNIO_ERROR_ILLEGAL_TRANSACTION_ID, "The received transaction id has caused the error." },
                {CrevisFnIO.FNIO_ERROR_TRANSACTION_TIMEOUT, "Time was expired before completing the transaction." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_PROTOCOL_TYPE, "The provided protocol type is not supported." },
                {CrevisFnIO.FNIO_ERROR_NETWORK_IS_BUSY, "Network resource is temporarily not available." },
                {CrevisFnIO.FNIO_ERROR_NETWORK_ERROR, "Network is not available." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_PROTOCAL_ID, "The received protocol id has caused the error." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_UNIT_ID, "The received unit id has caused the error." },
                {CrevisFnIO.FNIO_ERROR_ILLEGAL_NODE_ADDRESS, "The node address is wrong." },
                {CrevisFnIO.FNIO_ERROR_CRC_CHECK_FAIL, "CRC-16 Check is failed because of the wrong receive data." },
            };
        }
        #endregion

    }
}
