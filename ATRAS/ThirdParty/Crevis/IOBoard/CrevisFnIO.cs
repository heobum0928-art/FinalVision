using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CrevisFnIoLib
{
    class CrevisFnIO
    {

        /***************************************************************************************/
        /* CrevisFnio.H                                                                       **/
        /***************************************************************************************/
        //
        // Default value define
        //
        public const int	FNIO_DEFAULT_VALUE					= 0;
        public const int	FNIO_ENABLE							= 1;
        public const int	FNIO_DISENABLE						= 0;
        public const int	FNIO_EXECUTE						= 1;
        public const int	FNIO_ABORT							= 0;
        public const int	FNIO_DEFAULT_RESPONSE_TIMEOUT		= 200;
        public const int	FNIO_DEFAULT_DEVICE_UPDATE_TIMEOUT	= 5000;
        public const int	FNIO_DEFAULT_CONNECT_TIMEOUT		= 5000;
        public const int	FNIO_DEFAULT_UPDATE_FREQUENCE		= 50;
        public const int	FNIO_DEFAULT_SAFETY_TIMEOUT		    = 10000;
        public const int	FNIO_DEFAULT_RETRY_NUMBER		    = 5;


	
        //
        // Device Type Define
        //
        public const int    MODBUS_TCP				            = 0;
        public const int    MODBUS_232				            = 1;
        public const int    MODBUS_485				            = 2;
        public const int    MODBUS_PORT_NUMBER	    	        = 502;



        //
        // IoUpdate Mode Define
        //
        public const int    IO_UPDATE_PERIODIC	    	        = 0;
        public const int    IO_UPDATE_EVENT	    	            = 1;

        
        //
        // Device infomation structure
        //

        //MODBUS TCP
        [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
        public struct DEVICEINFOMODBUSTCP2
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[]   IpAddress;
        };

        //MODBUS RS232, 485
        [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
        public struct DEVICEINFOMODBUSSERIAL
        {
            //구조체 내부 문자열변수가있을때 마샬링
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public String strComportName;
            public UInt32 BaudRate;
            public Byte DataBit;
            public Byte StopBit;
            public Byte ParityBit;
            public Byte NodeAddr;
        }

        //
        // Commands
        //

        // FNIO_SYS_CMD														
        public const int    SYS_VERSION						    = 0x00000001;			
        public const int    SYS_VENDOR_NAME					    = 0x00000002;	
        public const int    SYS_CONNECTION_TIMEOUT			    = 0x00000005;			
        public const int    SYS_DEVICE_NUM					    = 0x00000006;	
        


        // FNIO_DEV_CMD							
        // Device Identification
        public const int    DEV_PRODUCT_CODE			    	= 0x00001002;
        public const int    DEV_FIRMWARE_VERSION		    	= 0x00001003;
        public const int    DEV_PROCDUCT_NAME			    	= 0x00001005;
        public const int    DEV_FIRMWARE_RELEASE_DATE	    	= 0x00001007;
        // Device Time Controls
        public const int    DEV_WATCHDOC_TIME			    	= 0x00002000;
        public const int    DEV_IO_UPDATE_TIME			    	= 0x00002004;
        public const int    DEV_MAIN_LOOP_TIME			    	= 0x00002005;
        // Device Translayer Controls
        public const int    DEV_CONNECTION_TIMEOUT_TIME			= 0x00003001;
        public const int    DEV_CONNECTED_NUMBER				= 0x00003002;
        public const int    DEV_PORT_NUMBER						= 0x00003003;
        public const int    DEV_INTERFACE_SPEED					= 0x00003004;
        public const int    DEV_BOOTP_ENABLE					= 0x00003005;
        public const int    DEV_ARP_ENABLE						= 0x00003006;
        public const int    DEV_IP_ADDRESS				    	= 0x00003007;
        public const int    DEV_SUBNET_MASK				    	= 0x00003008;
        public const int    DEV_GATEWAY					    	= 0x00003009;
        public const int    DEV_MAC_ID					    	= 0x0000300A;
        // Device Informations
        public const int    DEV_EXPANSION_SLOT_NUMBER	    	= 0x00004009;
        public const int    DEV_ACTIVE_SLOT_NUMBER		    	= 0x0000400A;
        public const int    DEV_INACTIVE_SLOT_NUMBER	    	= 0x0000400B;
        public const int    DEV_INPUT_PROCESS_IMAGE_MODE    	= 0x0000400D;
        public const int    DEV_OUTPUT_PROCESS_IMAGE_MODE	    = 0x0000400E;
        public const int    DEV_INACTIVE_SLOT_LIST		    	= 0x0000400F;
        public const int    DEV_ACTIVE_SLOT_LIST		    	= 0x00004010;
        public const int    DEV_ALARM_SLOT_LIST			    	= 0x00004011;
        public const int    DEV_DEVICE_BUS_STATUS		    	= 0x00004012;
        //
        public const int    DEV_DEVICE_RESET		    	    = 0x00005001;
        //
        public const int    DEV_INPUT_IMAGE_SIZE		    	= 0x00007000;
        public const int    DEV_OUTPUT_IMAGE_SIZE		    	= 0x00007001;	
        // Device Configurations
        public const int    DEV_RECONNECT_TIMEOUT		    	= 0x00009000;
        public const int    DEV_RESPONSE_TIMEOUT		    	= 0x00009001;
        public const int    DEV_UPDATE_FREQUENCY		    	= 0x00009002;
        public const int    DEV_AUTO_CONNECTION			    	= 0x00009003;
        public const int    DEV_SAFETY_MODE				    	= 0x00009004;
        public const int    DEV_SAFETY_TIMEOUT			    	= 0x00009005;
        public const int    DEV_RETRY_NUMBER			    	= 0x00009006;

        
        // FNIO_IO_CMD							
        public const int    IO_MODULE_ID			        	= 0x0000A000;
        public const int    IO_INPUT_BIT_SIZE			    	= 0x0000A008;
        public const int    IO_OUTPUT_BIT_SIZE			    	= 0x0000A009;
        public const int    IO_ST_NUMBER				    	= 0x0000A00E;
        public const int    IO_MODULE_DESCRIPTION			    = 0x0000A00F;
        public const int    IO_CONFIGURATION_PARAMETER_SIZE     = 0x0000A010;
        public const int    IO_CONFIGURATION_PARAMETER		    = 0x0000A011;
        public const int    IO_PROCDUCT_CODE		            = 0x0000A015;
        public const int    IO_CATALOG_NUNBER		            = 0x0000A016;
        public const int    IO_FIRMWARE_VERSION				    = 0x0000A017;
        public const int    IO_EXPANSION_CLASS				    = 0x0000A01A;
        

        // FNIO_DATA_INFO
        public const int    DATA_ACCESS_MODE			    	= 0x00012000;
        public const int    DATA_TYPE					    	= 0x00012001;
        public const int    DATA_MIN					    	= 0x00012003;
        public const int    DATA_MAX					    	= 0x00012004;
        public const int    DATA_INC					    	= 0x00012005;
        public const int    DATA_VALUE					    	= 0x00012006;
        

        // FNIO_ACCESS_MODE								
        public const int    READ_ONLY					    	= 0x00013000;
        public const int    WRITE_ONLY					    	= 0x00013001;
        public const int    READ_WRITE					    	= 0x00013002;
        public const int    NOT_IMPLEMENT				    	= 0x00013003;
        				

        //
        // Deta types
        //
        // FNIO_DATATYPE											
        public const int    DATATYPE_UNKNOWN			    	= 0x00021000;			
        public const int    DATATYPE_STRING 			    	= 0x00021001;			
        public const int    DATATYPE_INT8				    	= 0x00021002;			
        public const int    DATATYPE_UINT8  			    	= 0x00021003;			
        public const int    DATATYPE_INT16  			    	= 0x00021004;			
        public const int    DATATYPE_UINT16   			    	= 0x00021005;			
        public const int    DATATYPE_INT32      		    	= 0x00021006;			
        public const int    DATATYPE_UINT32  			    	= 0x00021007;			
        public const int    DATATYPE_INT64       		    	= 0x00021008;			
        public const int    DATATYPE_UINT64				    	= 0x00021009;			
        public const int    DATATYPE_FLOAT32         	    	= 0x0002100A;			
        public const int    DATATYPE_FLOAT64    		    	= 0x0002100B;			
        public const int    DATATYPE_HANDLE    			    	= 0x0002100C;			
        	

        //
        // Error types
        //
        // FNIO_ERROR_TYPE							
        public const int    FNIO_ERROR_SUCCESS        				= 0;
        public const int    FNIO_ERROR_DEVICE_CONNECT_FAIL   		= -1;
        public const int    FNIO_ERROR_MAX_CONNECTION_EXCEEDED		= -2;
        public const int    FNIO_ERROR_ILLEGAL_DEVICE_TYPE			= -3;
        public const int    FNIO_ERROR_SYSTEM_ALREADY_INIT  		= -4;
        public const int    FNIO_ERROR_SYSTEM_ALLOC_FAIL   			= -5;
        public const int    FNIO_ERROR_SYSTEM_NOT_EXIST   			= -6;
        public const int    FNIO_ERROR_SYSTEM_CHECK_FAIL    		= -7;
        public const int    FNIO_ERROR_WRITE_ONLY_COMMAND   		= -8;
        public const int    FNIO_ERROR_READ_ONLY_COMMAND    		= -9;
        public const int    FNIO_ERROR_NOT_DEFINE_COMMAND   		= -10;
        public const int    FNIO_ERROR_NOT_SUPPORT_COMMAND   		= -11;
        public const int    FNIO_ERROR_DUPLICATE_CONNECT   			= -12;
        public const int    FNIO_ERROR_DEVICEINFO_ALLOC_FAIL  		= -13;
        public const int    FNIO_ERROR_DEVICE_ALLOC_FAIL   			= -14;
        public const int    FNIO_ERROR_UNKNOWN_MODEL		 		= -15;
        public const int    FNIO_ERROR_BUFFERSIZE_SMALL    			= -16;
        public const int    FNIO_ERROR_DEVICE_INDEX_EXCESS   		= -17;
        public const int    FNIO_ERROR_NOT_EXECUTE     				= -18;
        public const int    FNIO_ERROR_DEVICE_CHECK_FAIL    		= -19;
        public const int    FNIO_ERROR_PORT_ALLOC_FAIL 				= -20;
        public const int    FNIO_ERROR_IO_MODULE_ALLOC_FAIL  		= -21;
        public const int    FNIO_ERROR_BUFFER_ALLOC_FAIL   			= -22;
        public const int    FNIO_ERROR_NULL_BUFFER     				= -23;
        public const int    FNIO_ERROR_LIST_INDEX_EXCESS    		= -24;
        public const int    FNIO_ERROR_SLOT_CHECK_FAIL    			= -25;
        public const int    FNIO_ERROR_NOT_DEFINE_DATAINFO   		= -26;
        public const int    FNIO_ERROR_NOT_SUPPORT_DATAINFO   		= -27;
        public const int    FNIO_ERROR_NOT_DEFINE_EVENT				= -28;
        public const int    FNIO_ERROR_NOT_AVAILABLE_PORT			= -29;
        public const int    FNIO_ERROR_INVALID_IMAGE_ADDRESS  		= -30;
        public const int    FNIO_ERROR_INVALID_IMAGE_LENGHT  		= -31;
        public const int    FNIO_ERROR_INVALID_BIT_INDEX  			= -32;
        public const int    FNIO_ERROR_COMPORT_ALLOC_FAIL		  	= -33;
        public const int    FNIO_ERROR_COMPORT_OPEN_FAIL		  	= -34;
        public const int    FNIO_ERROR_SERPORT_ALREADY_IN_USE  	  	= -35;
        public const int    FNIO_ERROR_NODEADDR_ALREADY_IN_USE    	= -36;
        public const int    FNIO_ERROR_DIFFERENT_SERIAL_INFO	  	= -37;
        public const int    FNIO_ERROR_NOT_SEND_MESSAGE			  	= -38;
        public const int    FNIO_ERROR_DIFFERNT_PORT_TYPE		  	= -39;
        public const int    FNIO_ERROR_NOT_WORK_IO_UPDATE		  	= -40;
        

        // Device protocol errors 

        //Modbus TCP
        public const int    FNIO_ERROR_ILLEGAL_FUNCTION				= 1;
        public const int    FNIO_ERROR_ILLEGAL_DATA_ADDRESS			= 2;
        public const int    FNIO_ERROR_ILLEGAL_DATA_VALUE			= 3;
        public const int    FNIO_ERROR_SLAVE_DEVICE_FAILURE			= 4;
        public const int    FNIO_ERROR_ACKNOWLEDGE					= 5;
        public const int    FNIO_ERROR_SLAVE_DEVICE_BUSY			= 6;
        public const int    FNIO_ERROR_MEMORY_PARITY_ERROR			= 8;
        public const int    FNIO_ERROR_GATEWAY_PATH_UNAVAILABLE		= 10;
        public const int    FNIO_ERROR_GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND = 11;
        public const int    FNIO_ERROR_UNKNOW						= 14;
        public const int    FNIO_ERROR_ILLEGAL_TRANSACTION_ID		= 15;
        public const int    FNIO_ERROR_TRANSACTION_TIMEOUT			= 16;
        public const int    FNIO_ERROR_ILLEGAL_PROTOCOL_TYPE		= 19;			
        public const int    FNIO_ERROR_NETWORK_IS_BUSY				= 20;	
        public const int    FNIO_ERROR_NETWORK_ERROR				= 21;
        public const int    FNIO_ERROR_ILLEGAL_PROTOCAL_ID			= 22;
        public const int    FNIO_ERROR_ILLEGAL_UNIT_ID              = 23;
        
        //Modbus Serial
        public const int    FNIO_ERROR_ILLEGAL_NODE_ADDRESS         = 30;
        public const int    FNIO_ERROR_CRC_CHECK_FAIL               = 31;

        //
        // System functions
        //
        [DllImport("CrevisFnIO20.dll")]
        public static extern int LibInitSystem(ref IntPtr phSystem);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int LibFreeSystem(IntPtr hSystem);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int LibGetLastError(ref int pErrorCode, ref byte pErrText, ref int pSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int SysGetParam(IntPtr hSystem, int Cmd, int Info, ref byte pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int SysGetParam(IntPtr hSystem, int Cmd, int Info, ref int pData, ref int pDataSize); 
        [DllImport("CrevisFnIO20.dll")]
        public static extern int SysSetParam(IntPtr hSystem, int Cmd, ref int pData, ref int pDataSize);

        //
        // Device functions
        //
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevOpenDevice(IntPtr hSystem, ref DEVICEINFOMODBUSTCP2 pDeviceInfo, int DeviceType, ref IntPtr phDevice);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevOpenDevice(IntPtr hSystem, ref DEVICEINFOMODBUSSERIAL pDeviceInfo, int DeviceType, ref IntPtr phDevice);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevCloseDevice(IntPtr hDevice);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevGetParam(IntPtr hDevice, int Cmd, int Info, ref byte pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevGetParam(IntPtr hDevice, int Cmd, int Info, ref int pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevSetParam(IntPtr hDevice, int Cmd, ref int pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevIoUpdateStart(IntPtr hDevice, int IoUpdateType);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevIoUpdateStop(IntPtr hDevice);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevReadInputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevReadOutputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevWriteOutputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevReadInputImageBit(IntPtr hDevice, int Addr, int BitIndex, ref int pBitData);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevReadOutputImageBit(IntPtr hDevice, int Addr, int BitIndex, ref int pBitData);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int DevWriteOutputImageBit(IntPtr hDevice, int Addr, int BitIndex, int BitData);

        //
        // IoModule functions
        //
        [DllImport("CrevisFnIO20.dll")]
        public static extern int IoGetIoModule(IntPtr hDevice, int SlotInedx, ref IntPtr phSlot);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int IoGetParam(IntPtr hSlot, int Cmd, int Info, ref byte pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int IoGetParam(IntPtr hSlot, int Cmd, int Info, ref int pData, ref int pDataSize);
        [DllImport("CrevisFnIO20.dll")]
        public static extern int IoSetParam(IntPtr hSlot, int Cmd, ref int pData, ref int pDataSize);


        public int FNIO_LibInitSystem(ref IntPtr hSystem)
        {
            return LibInitSystem(ref hSystem);
        }

        public int FNIO_LibFreeSystem(IntPtr hSystem)
        {
            return LibFreeSystem(hSystem);
        }

        public int FNIO_LibGetLastError(ref Int32 pErrorCode, ref String strErrText)
        {
            int i = 0;
            int err = 0;
            int Size = 1024;
            byte[] ErrText = new byte[1024];
            err = LibGetLastError(ref pErrorCode, ref ErrText[0], ref Size);

            if(err != FNIO_ERROR_SUCCESS)
                return err;

            for(i=0; i<1024; i++)
            {
                if (ErrText[i] != 0)
                {
                    strErrText += (Char)ErrText[i];
                }
                else
                {
                    break;
                }
            }

            return err;
        }

        public int FNIO_SysGetParam(IntPtr hSystem, int Cmd, ref String strText)
        {
            int i = 0;
            int err = 0;
            int Size = 1024;
            int Info = DATA_VALUE;
            byte[] szText = new byte[1024];

            err = SysGetParam(hSystem, Cmd, Info, ref szText[0], ref Size);
           
            if(err != FNIO_ERROR_SUCCESS)
                return err;

            for(i=0; i<1024; i++)
            {
                if (szText[i] != 0)
                {
                    strText += (Char)szText[i];
                }
                else
                {
                    break;
                }
            }

            return err;
        }

        public int FNIO_SysGetParam(IntPtr hSystem, int Cmd, ref int pData)
        {
            int Size = sizeof(int);
            int Info = DATA_VALUE;
            return SysGetParam(hSystem, Cmd, Info, ref pData, ref Size);
        }

        public int FNIO_SysSetParam(IntPtr hSystem, int Cmd, int Data)
        {
            int Size = sizeof(int);
            return SysSetParam(hSystem, Cmd, ref Data, ref Size);
        }

        //
        // Device functions
        //
        public int FNIO_DevOpenDevice(IntPtr hSystem, ref DEVICEINFOMODBUSTCP2 pDeviceInfo, int DeviceType, ref IntPtr phDevice)
        {
            return DevOpenDevice(hSystem, ref pDeviceInfo, DeviceType, ref phDevice);
        }

        public int FNIO_DevOpenDevice(IntPtr hSystem, ref DEVICEINFOMODBUSSERIAL pDeviceInfo, int DeviceType, ref IntPtr phDevice)
        {
            return DevOpenDevice(hSystem, ref pDeviceInfo, DeviceType, ref phDevice);
        }

        public int FNIO_DevCloseDevice(IntPtr hDevice)
        {
            return DevCloseDevice(hDevice);
        }

        public int FNIO_DevGetParam(IntPtr hDevice, int Cmd, ref String strText)
        {
            int i = 0;
            int err = 0;
            int Size = 1024;
            int Info = DATA_VALUE;
            byte[] szText = new byte[1024];

            err = DevGetParam(hDevice, Cmd, Info, ref szText[0], ref Size);
            if(err != FNIO_ERROR_SUCCESS)
                return err;

            for(i=0; i<1024; i++)
            {
                if (szText[i] != 0)
                {
                    strText += (Char)szText[i];
                }
                else
                {
                    break;
                }
            }

            return err;
        }

        public int FNIO_DevGetParam(IntPtr hDevice, int Cmd, ref int pData)
        {
            int Size = sizeof(int);
            int Info = DATA_VALUE;            
            return DevGetParam(hDevice, Cmd, Info, ref pData, ref Size);
        }

        public int FNIO_DevSetParam(IntPtr hDevice, int Cmd, int Data)
        {
            int Size = sizeof(int);
            return DevSetParam(hDevice, Cmd, ref Data, ref Size);
        }

        public int FNIO_DevIoUpdateStart(IntPtr hDevice, int IoUpdateType)
        {
            return DevIoUpdateStart(hDevice, IoUpdateType);
        }

        public int FNIO_DevIoUpdateStop(IntPtr hDevice)
        {
            return DevIoUpdateStop(hDevice);
        }

        public int FNIO_DevReadInputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len)
        {
            return DevReadInputImage(hDevice, Addr, ref pBuffer, Len);
        }

        public int FNIO_DevReadOutputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len)
        {
            return DevReadOutputImage(hDevice, Addr, ref pBuffer, Len);
        }

        public int FNIO_DevWriteOutputImage(IntPtr hDevice, int Addr, ref byte pBuffer, int Len)
        {
            return DevWriteOutputImage(hDevice, Addr, ref pBuffer, Len);
        }

        public int FNIO_DevReadInputImageBit(IntPtr hDevice, int Addr, int BitIndex, ref int pBitData)
        {
            return DevReadInputImageBit(hDevice, Addr, BitIndex, ref pBitData);
        }

        public int FNIO_DevReadOutputImageBit(IntPtr hDevice, int Addr, int BitIndex, ref int pBitData)
        {
            return DevReadOutputImageBit(hDevice, Addr, BitIndex, ref pBitData);
        }

        public int FNIO_DevWriteOutputImageBit(IntPtr hDevice, int Addr, int BitIndex, int BitData)
        {
            return DevWriteOutputImageBit(hDevice, Addr, BitIndex, BitData);
        }

        public int FNIO_IoGetIoModule(IntPtr hDevice, int SlotInedx, ref IntPtr phSlot)
        {
            return IoGetIoModule(hDevice, SlotInedx, ref phSlot);
        }

        public int FNIO_IoGetParam(IntPtr hSlot, int Cmd, ref String strText)
        {
            int i = 0;
            int err = 0;
            int Size = 1024;
            int Info = DATA_VALUE;
            byte[] szText = new byte[1024];

            err = IoGetParam(hSlot, Cmd, Info, ref szText[0], ref Size);

            if(err != FNIO_ERROR_SUCCESS)
                return err;

            for(i=0; i<1024; i++)
            {
                if (szText[i] != 0)
                {
                    strText += (Char)szText[i];
                }
                else
                {
                    break;
                }
            }

            return err;
        }

        public int FNIO_IoGetParam(IntPtr hSlot, int Cmd, ref int pData)
        {
            int Size = sizeof(int);
            int Info = DATA_VALUE; 
            return IoGetParam(hSlot, Cmd, Info, ref pData, ref Size);
        }

        public int FNIO_IoSetParam(IntPtr hSlot, int Cmd, int Data)
        {
            int Size = sizeof(int);
            return IoSetParam(hSlot, Cmd, ref Data, ref Size);
        }


    }
}
