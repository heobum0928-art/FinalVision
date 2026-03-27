using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Vieworks;

using VWCL_HANDLE = System.IntPtr;
using HCAMERA = System.IntPtr;

namespace VwCL.Demo.FeatureControl.Console.CS
{
    class Program
    {
        static void Main(string[] args)
        {
            // 0. Initialize
            System.Console.WriteLine("Step 0. Initialize....");
            VWCL_HANDLE vwCL = IntPtr.Zero;
            Vieworks.RESULT ret = Vieworks.VwCL.OpenVwCL( ref vwCL );

            if ( Vieworks.RESULT.RESULT_SUCCESS != ret )
            {
                // Error handling
                System.Console.WriteLine("Failed(err:{0}))", ret);
                return;
            }
            System.Console.WriteLine("OK");

            // 1. Open Device
            System.Console.WriteLine("Step 1. Open Device.....");

            HCAMERA hCamera = IntPtr.Zero;
            OBJECT_INFO hObjectInfo = new OBJECT_INFO();
            GCHandle gchobjectInfo = GCHandle.Alloc(hObjectInfo);

            IntPtr pObjectInfo = Marshal.AllocHGlobal(Marshal.SizeOf(hObjectInfo));

            int nDeviceIndex = 0;
            int nImageBufferNumber = 20;

            ret = Vieworks.VwCL.VwOpenCameraByIndex(vwCL,
                                                    nDeviceIndex,   // Device Index
                                                    ref hCamera,    // Camera handle
                                                    nImageBufferNumber, // The number of buffer
                                                    0,              // Auto
                                                    0,              // Auto
                                                    0,              // Auto
                                                    pObjectInfo,    // User Pointer
                                                    IntPtr.Zero,    // Skip in this sample
                                                    IntPtr.Zero);   // Skip in this sample
            if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
            {
                switch (ret)
                {
                    default:
                        {
                            System.Console.WriteLine("ERROR : Default error code returned");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_DEVCREATEDATASTREAM:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_NO_CAMERAS:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_NO_CAMERAS was returned");
                            System.Console.WriteLine("CHECK : Camera connection");
                        }
                        break;
                    case Vieworks.RESULT.RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER:
                        {
                            System.Console.WriteLine("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned");
                            System.Console.WriteLine("CHECK : Zero-based camera index");
                        }
                        break;
                }

                Marshal.FreeHGlobal(pObjectInfo);
                if (IntPtr.Zero != hCamera)
                    Vieworks.VwCL.CameraClose(hCamera);
                if (IntPtr.Zero != vwCL)
                    Vieworks.VwCL.CloseVwCL(ref vwCL);
                return;
            }

MAIN_MENU:
            
            while(true)
            {
                System.Console.WriteLine("\n\tSelect menu");
                System.Console.WriteLine("\t1.Get All Feature List");
                System.Console.WriteLine("\t2.Get Feature Information");
                System.Console.WriteLine("\t3.Set Feature Data");
                System.Console.WriteLine("\t4.Get Feature Data");
                System.Console.WriteLine("\t5.Get Feature Min/Max");
                System.Console.WriteLine("\t6.Get list of Enumeration Feature");

                System.Console.WriteLine("\t7.Quit");
                System.Console.WriteLine(">");

                ConsoleKeyInfo tKeyInfo = System.Console.ReadKey(false);

                if ( (tKeyInfo.Key >= ConsoleKey.NumPad1   &&
                     tKeyInfo.Key <= ConsoleKey.NumPad6) ||
                    (tKeyInfo.Key >= ConsoleKey.D1 &&
                    tKeyInfo.Key <= ConsoleKey.D6))
                {
                    Dispatch(tKeyInfo.Key, hCamera);
                }
                else if ( tKeyInfo.Key == ConsoleKey.D7 ||
                          tKeyInfo.Key == ConsoleKey.NumPad7)
                {
                    break;
                }
                else
                {
                    // Unknown key
                }
            }

            ConsoleKeyInfo tKeyExit;
            do
            {
                System.Console.WriteLine("** Goto Select menu?");
                System.Console.WriteLine("** Y");
                System.Console.WriteLine("** N");
                System.Console.WriteLine(">");
                tKeyExit= System.Console.ReadKey(false);
            } while (tKeyExit.Key != ConsoleKey.Y &&
                    tKeyExit.Key != ConsoleKey.N);

            if ( tKeyExit.Key == ConsoleKey.Y)
            {
                goto MAIN_MENU;
            }

            // 3. Terminate
            System.Console.WriteLine("Step 3. Terminate.....");
                 
            Marshal.FreeHGlobal(pObjectInfo);
            if (IntPtr.Zero != hCamera)
                Vieworks.VwCL.CameraClose(hCamera);
            if (IntPtr.Zero != vwCL)
                Vieworks.VwCL.CloseVwCL(ref vwCL);


            System.Console.ReadLine();
        }

        static public void Dispatch(System.ConsoleKey Key, HCAMERA hCamera)
        {
            if (IntPtr.Zero == hCamera)
                return;

            switch( Key )
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    {
                        int nPropCount = 0;
                        RESULT ret = Vieworks.VwCL.CameraGetPropertyCount(hCamera, ref nPropCount);
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }
                        else
                        {
                            PROPERTY tPropertyInfo = new PROPERTY();
                            for ( int nIdx = 0; nIdx < nPropCount; nIdx ++)
                            {
                                ret = Vieworks.VwCL.CameraGetPropertyInfoUsingIndex(hCamera, nIdx, ref tPropertyInfo);
                                if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                                {
                                    System.Console.WriteLine("Failed(err:{0}))", ret);
                                    break;
                                }
                                else
                                {
                                    System.Console.WriteLine(tPropertyInfo.caName);
                                }
                            }
                        }
                    }
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    {
                        System.Console.WriteLine("\nInput a name of Feature");
                        System.Console.WriteLine(">");

                        string sFeatureName = System.Console.ReadLine();

                        PROPERTY tPropertyInfo = new PROPERTY();
                        RESULT ret = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), ref tPropertyInfo);
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }
                        else
                        {
                            if( Vieworks.PROPERTY_ACCESS_MODE.READ_ONLY == tPropertyInfo.eAccessMode)
				                System.Console.WriteLine("Access Mode : Read Only");
			                else if( PROPERTY_ACCESS_MODE.WRITE_ONLY == tPropertyInfo.eAccessMode)
				                System.Console.WriteLine("Access Mode : Write Only");
			                else if( PROPERTY_ACCESS_MODE.READ_WRITE == tPropertyInfo.eAccessMode)
				                System.Console.WriteLine("Access Mode : Read Write");
			                else if( PROPERTY_ACCESS_MODE.NOT_AVAILABLE == tPropertyInfo.eAccessMode)
				                System.Console.WriteLine("Access Mode : Not Available");
			                else 
				                System.Console.WriteLine("Access Mode : Not Implement");

			                if( PROPERTY_TYPE.ATTR_BOOLEAN == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Boolean");
			                else if( PROPERTY_TYPE.ATTR_CATEGORY == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Category");
			                else if( PROPERTY_TYPE.ATTR_COMMAND == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Command");
			                else if( PROPERTY_TYPE.ATTR_ENUM == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Enumeration");
			                else if( PROPERTY_TYPE.ATTR_FLOAT == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Float");
			                else if( PROPERTY_TYPE.ATTR_STRING == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : String");
			                else if( PROPERTY_TYPE.ATTR_UINT == tPropertyInfo.ePropType)
				                System.Console.WriteLine("Property Type : Integer");
			                else
				                System.Console.WriteLine("Property Type : Unknown");

			                if( PROPERTY_VISIBILITY.BEGINNER == tPropertyInfo.eVisibility)
				                System.Console.WriteLine("Property Visibility : Beginner");
			                else if( PROPERTY_VISIBILITY.EXPERT == tPropertyInfo.eVisibility)
				                System.Console.WriteLine("Property Visibility : Expert");
			                else if( PROPERTY_VISIBILITY.GURU == tPropertyInfo.eVisibility)
				                System.Console.WriteLine("Property Visibility : Guru");
                            else if (PROPERTY_VISIBILITY.INVISIBLE == tPropertyInfo.eVisibility)
				                System.Console.WriteLine("Property Visibility : Invisible");
			                else
				                System.Console.WriteLine("Property Visibility : Undefined");
                        }
                    }
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    {
                        System.Console.WriteLine("\nInput a name of Feature");
                        System.Console.WriteLine(">");
                        string sFeatureName = System.Console.ReadLine();

                        System.Console.WriteLine("\nInput a value");
                        System.Console.WriteLine(">");
                        string sArgument = System.Console.ReadLine();

                        RESULT ret = Vieworks.VwCL.CameraSetCustomCommand(hCamera, sFeatureName.ToCharArray(), sArgument.ToCharArray());
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }
                    }
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    {
                        System.Console.WriteLine("\nInput a name of Feature");
                        System.Console.WriteLine(">");
                        string sFeatureName = System.Console.ReadLine();

                        PROPERTY tPropInfo = new PROPERTY();
                        RESULT ret = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), ref tPropInfo);
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }

                        const int STR_SIZE = 256;
                        Byte[] btArg = new Byte[STR_SIZE];
                        int nSize = STR_SIZE;
                        ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, ref nSize, (int)Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE);
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }
                        else
                        {
                            char[] acHashedData = new char[nSize];
                            int nValidCount = 0;
                            for (int i = 0; i < nSize; i++)
                            {
                                if (btArg[i] == 0)
                                {
                                    break;
                                }
                                nValidCount++;
                            }
                            acHashedData = Encoding.Default.GetChars(btArg, 0, nValidCount);

                            string strArg = new string(acHashedData);

                            switch( tPropInfo.ePropType )
			                    {
			                    case PROPERTY_TYPE.ATTR_BOOLEAN:
				                    {
                                        int nValue = Convert.ToInt32(strArg);
					                    if ( nValue == 0 )
					                    {
                                            System.Console.WriteLine("Value : FALSE");
					                    }
					                    else
					                    {
                                            System.Console.WriteLine("Value : TRUE");
					                    }

					                    break;
				                    }
			                    case PROPERTY_TYPE.ATTR_UINT:
			                    case PROPERTY_TYPE.ATTR_FLOAT:
				                    {
					                    switch( tPropInfo.eRepresentation )
					                    {
					                    case PROPERTY_REPRESENTATION.REP_HEXNUMBER:
						                    {
                                                int nIP = Convert.ToInt32(strArg);
                                                System.Console.WriteLine("Value : {0}", nIP);
							                    break;
						                    }
					                    case PROPERTY_REPRESENTATION.REP_IPV4ADDRESS:
						                    {
                                                Int64 ulIP = Convert.ToInt64(strArg);
                                                System.Console.WriteLine("Value : {0}.{0}.{0}.{0}",
                                                    ( ulIP & 0xFF000000 ) >> 24,
								                    ( ulIP & 0x00FF0000 ) >> 16,
								                    ( ulIP & 0x0000FF00 ) >> 8,
								                    ( ulIP & 0x000000FF ));

							                    break;
						                    }
					                    default:
						                    {
                                                System.Console.WriteLine("Value : {0}", strArg);
						                    }
                                            break;
					                    }
					                    break;
				                    }
			                    case PROPERTY_TYPE.ATTR_ENUM:
			                    case PROPERTY_TYPE.ATTR_STRING:
				                    {
                                        System.Console.WriteLine("Value : {0}", strArg);
					                    break;
				                    }
			                    case PROPERTY_TYPE.ATTR_CATEGORY:
				                    {
                                        System.Console.WriteLine("Value : This is Category type feature.");
					                    break;
				                    }
			                    case PROPERTY_TYPE.ATTR_COMMAND:
				                    {
                                        System.Console.WriteLine("Value : This is Command type feature.");
                                        break;
				                    }
			                    default:
				                    {
                                        System.Console.WriteLine("Value : {0}", strArg);
				                    }
                                    break;
			                    }

                        }
                    }
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    {
                        System.Console.WriteLine("\nInput a name of Feature");
                        System.Console.WriteLine(">");
                        string sFeatureName = System.Console.ReadLine();

                        {
                            const int STR_SIZE = 256;
                            Byte[] btArg = new Byte[STR_SIZE];
                            int nSize = STR_SIZE;
                            RESULT ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, ref nSize, (int)Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_MAX);
                            if (ret != Vieworks.RESULT.RESULT_SUCCESS)
                            {
                                System.Console.WriteLine("Failed(err:{0}))", ret);
                            }
                            else
                            {
                                char[] acHashedData = new char[nSize];
                                int nValidCount = 0;
                                for (int i = 0; i < nSize; i++)
                                {
                                    if (btArg[i] == 0)
                                    {
                                        break;
                                    }
                                    nValidCount++;
                                }
                                acHashedData = Encoding.Default.GetChars(btArg, 0, nValidCount);

                                string strArg = new string(acHashedData);
                                System.Console.WriteLine("Max value : {0}", strArg);
                            }
                        }

                        {
                            const int STR_SIZE = 256;
                            Byte[] btArg = new Byte[STR_SIZE];
                            int nSize = STR_SIZE;
                            RESULT ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, ref nSize, (int)Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_MIN);
                            if (ret != Vieworks.RESULT.RESULT_SUCCESS)
                            {
                                System.Console.WriteLine("Failed(err:{0}))", ret);
                            }
                            else
                            {
                                char[] acHashedData = new char[nSize];
                                int nValidCount = 0;
                                for (int i = 0; i < nSize; i++)
                                {
                                    if (btArg[i] == 0)
                                    {
                                        break;
                                    }
                                    nValidCount++;
                                }
                                acHashedData = Encoding.Default.GetChars(btArg, 0, nValidCount);

                                string strArg = new string(acHashedData);
                                System.Console.WriteLine("Min value : {0}", strArg);
                            }
                        }
                    }
                    break;
                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    {
                        System.Console.WriteLine("\nInput a name of Feature");
                        System.Console.WriteLine(">");
                        string sFeatureName = System.Console.ReadLine();

                        PROPERTY tPropInfo = new PROPERTY();
                        RESULT ret = Vieworks.VwCL.CameraGetPropertyInfo(hCamera, sFeatureName.ToCharArray(), ref tPropInfo);
                        if ( ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }

                        int nCount = 0;
                        
                        const int STR_SIZE = 256;
                        Byte[] btArg = new Byte[STR_SIZE];
                        int nSize = STR_SIZE;
                        ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, ref nSize, (int)Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_NUM);
                        if (ret != Vieworks.RESULT.RESULT_SUCCESS)
                        {
                            System.Console.WriteLine("Failed(err:{0}))", ret);
                        }
                        else
                        {
                            char[] acHashedData = new char[nSize];
                            int nValidCount = 0;
                            for (int i = 0; i < nSize; i++)
                            {
                                if (btArg[i] == 0)
                                {
                                    break;
                                }
                                nValidCount++;
                            }
                            acHashedData = Encoding.Default.GetChars(btArg, 0, nValidCount);

                            string strArg = new string(acHashedData);
                            System.Console.WriteLine("The number of item : {0}", strArg);
                            nCount = Convert.ToInt32(strArg);
                        }
                        
                        for ( int i = 0; i < nCount; i ++ )
                        {
                            System.Array.Clear(btArg, 0, STR_SIZE);

                            nSize = STR_SIZE;
                            ret = Vieworks.VwCL.CameraGetCustomCommand(hCamera, sFeatureName.ToCharArray(), btArg, ref nSize, (int)Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_INDEX + i);
                            if (ret != Vieworks.RESULT.RESULT_SUCCESS)
                            {
                                System.Console.WriteLine("Failed(err:{0}))", ret);
                            }
                            else
                            {
                                char[] acHashedData = new char[nSize];
                                int nValidCount = 0;
                                for (int nIdx = 0; nIdx < nSize; nIdx++)
                                {
                                    if (btArg[nIdx] == 0)
                                    {
                                        break;
                                    }
                                    nValidCount++;
                                }
                                acHashedData = Encoding.Default.GetChars(btArg, 0, nValidCount);

                                string strArg = new string(acHashedData);
                                System.Console.WriteLine("\t{0}", strArg);
                            }
                        }
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Unrecognized command");
                    }
                    break;
            }
        }
    }
}