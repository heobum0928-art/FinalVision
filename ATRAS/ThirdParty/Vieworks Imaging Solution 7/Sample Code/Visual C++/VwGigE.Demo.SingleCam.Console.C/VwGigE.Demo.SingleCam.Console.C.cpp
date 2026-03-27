#include "stdafx.h"
#include "VwGigE.Demo.SingleCam.Console.C.h"
#include "VwGigE.Api.h"
#include "VwImageProcess.API.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


#define MAX_CAMS 2

CWinApp theApp;

using namespace std;

CString GetPixelFormatString(VWSDK::PIXEL_FORMAT _ePixelFormat );

char* Dispatch(char *cmdline, int cmdSize, VWSDK::HCAMERA phCamera);

static void	GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event

VWSDK::RESULT GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;

	VWSDK::OBJECT_INFO* pObjectInfo = new VWSDK::OBJECT_INFO;
MAIN_MENU:
	VWSDK::VWGIGE_HANDLE m_pvwGigE	= NULL;
	char cmdline[80];

	printf("\n***************Main menu***************\n");
	printf("** Select the menu\n");
	printf("** 1.Normal Test\n");
	printf("** 2.End");	
	printf("\n->");

	ZeroMemory(cmdline, 80);
	gets_s(cmdline, sizeof(cmdline));

	if(      !(_strnicmp("1", cmdline, 2))) 
	{
	}
	else if( !(_strnicmp("2", cmdline, 2))) 
	{
		if ( pObjectInfo )
		{
			delete pObjectInfo;
			pObjectInfo = NULL;
		}

		return 0;
	}
	else
	{
		printf("** Unrecognized command\n");
		goto MAIN_MENU;
	}

	// Open Network Interface Card.
	if ( VWSDK::RESULT_SUCCESS != VWSDK::OpenVwGigE( &m_pvwGigE ) )
	{
		goto MAIN_MENU;
	}

	if ( m_pvwGigE == NULL )
	{
		goto MAIN_MENU;
	}
	else
	{	
		VWSDK::VwUserLogging(m_pvwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
			_T("You can see this message in a tool called SpiderLogger.exe"));

		UINT NICNum = 0;
		UINT deviceOrder = 0;
		VWSDK::VwGetNumInterfaces( m_pvwGigE, &NICNum );
		for(int i=0;i<(int)NICNum;i++)
		{
			VWSDK::HINTERFACE pInterface = NULL;
			VWSDK::VwOpenInterfaceByIndex( m_pvwGigE, i, &pInterface );

			UINT DeviceNum = 0;
			VWSDK::InterfaceGetNumCameras( pInterface, &DeviceNum );

			VWSDK::INTERFACE_INFO_STRUCT InterfaceInfo;
			VWSDK::VwDiscoveryInterfaceInfo( m_pvwGigE, i, &InterfaceInfo );
			
			for(int j=0;j<(int)DeviceNum;j++)
			{
				printf("** Device %d\n",deviceOrder);
				printf("** %s\n",InterfaceInfo.name );
				char chDevName[1024] = {0,};
				size_t sizeDevName = sizeof( chDevName );
				VWSDK::InterfaceGetCameraName( pInterface, j, chDevName, &sizeDevName );
				printf("** %s\n",chDevName );

				deviceOrder++;
			}
		}
	}

DEVICE_NUMBER:
	printf("** Select Device number to open:");
	printf("\n->");
	
	ZeroMemory(cmdline, 80);
	gets_s(cmdline, sizeof(cmdline));

	bool isNum = false;
	int strLength = strlen(cmdline);

	for(int i=0; i<strLength; i++)
	{
		if( '0' <= cmdline[i] && '9' >=cmdline[i])
		{
			isNum = true;
		}
		else
		{
			isNum = false;
			break;
		}
	}

	if( isNum == false)
	{
		goto DEVICE_NUMBER;
	}


	UINT selectednum = atoi(cmdline);

	for( UINT i=0 ; i<1 ; i++)
	{	
		/**************************************************************************
		* attempt to open a camera up on index 0
		***************************************************************************
		*/
		WORD camNdx = 0;

		
		VWSDK::HCAMERA pCamera = NULL;
		if ( VWSDK::RESULT_SUCCESS != VWSDK::VwOpenCameraByIndex( m_pvwGigE, selectednum, &pCamera, 2, 0, 0, 0, pObjectInfo, GetImageEvent1  ) )
		{
			printf("** Failed to open camera %d. Exiting...\n", selectednum);

			VWSDK::CloseVwGigE(m_pvwGigE);
			goto MAIN_MENU;
		}
		else
		{
			pObjectInfo->pVwCamera    = pCamera;

			// Print device info.

			char chVendorName[256] = {0,};
			size_t sizeVendorName = sizeof( chVendorName );
			VWSDK::CameraGetDeviceVendorName( pCamera, 0, chVendorName, &sizeVendorName );
			printf("VendorName : %s\n", chVendorName);
			char chModelName[256] = {0,};
			size_t sizeModelName = sizeof( chModelName );
			VWSDK::CameraGetDeviceModelName( pCamera, 0, chModelName, &sizeModelName );
			printf("ModelName : %s\n", chVendorName);
			char chVersion[256] = {0,};
			size_t sizeVersion = sizeof( chVersion );
			VWSDK::CameraGetDeviceVersion( pCamera, 0, chVersion, &sizeVersion );
			printf("Version : %s\n", chVersion);
			UINT nWidth = 0;
			UINT nHeight = 0;
			GetCustomCommand(pCamera, "Width", &nWidth);
			GetCustomCommand(pCamera, "Height", &nHeight);
			printf("Width : %d, Height : %d\n", nWidth, nHeight);
			VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
			VWSDK::CameraGetPixelFormat( pCamera, &pixelFormat );
			CStringA strPixel( GetPixelFormatString(pixelFormat) );
			printf("%s", strPixel );
			// Camera->GigEGetCurrentMACAddress
			char chMACAddress[256] = {0,};
			size_t sizeMAC = sizeof( chMACAddress );
			VWSDK::CameraGetCustomCommand(pCamera, "GevMACAddress", chMACAddress, &sizeMAC );
			printf("MACAddress : %s\n", chMACAddress);
		}

		// Put device handle into serial class.
		/****************************************************************************
		* program does not return until the user sends a quit command
		*****************************************************************************
		*/
		{		
			
			char done = 0;
			char cmdline[80];

			printf("*************************************\n");

			while(1)
			{
				printf("\nSelect menu\n");
				printf("1.Start grabbing\n");
				printf("2.Stop  grabbing\n");
				printf("3.Set Gain\n");
				printf("4.Get Gain\n");
				printf("5.Set Exposure time\n");
				printf("6.Get Exposure time\n");
				printf("7.Get Property info\n");
				printf("8.Get All Property List\n");
				printf("9.Quit\n");
				printf(">");

				gets_s(cmdline, sizeof(cmdline));

				if(0 == strlen(cmdline))
					;/* do nothing */
				else if(!_strnicmp("9", cmdline, 2))
				{
					break;
				}
				else
				{
					Dispatch((char *)cmdline, sizeof(cmdline), pCamera);
				}
			}
		}
	}

	// Close VwGigE(including closing of interface cards and cameras).
	if( VWSDK::CloseVwGigE( m_pvwGigE ) != VWSDK::RESULT_SUCCESS )
	{

	} 

	do 
	{
		printf("** Goto Main menu?\n");
		printf("** Y\n");
		printf("** N\n");	
		printf(">");	
		gets_s(cmdline, sizeof(cmdline));
	} while ( ( _strnicmp("Y", cmdline, 1) ) &&
		( _strnicmp("N", cmdline, 1) ) );

	if( !(_strnicmp("Y", cmdline, 1))) 
	{
		goto MAIN_MENU;
	}
	
	if(pObjectInfo)
		delete pObjectInfo;
	
	return nRetCode;
}



CString GetPixelFormatString(VWSDK::PIXEL_FORMAT _ePixelFormat )
{
	CString strTmp = _T("");

	switch( _ePixelFormat )
	{
	case VWSDK::PIXEL_FORMAT_MONO8:
		strTmp = _T("Pixel Format : Mono 8\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
		strTmp = _T("Pixel Format : Mono 10\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12:
		strTmp = _T("Pixel Format : Mono 12\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO14:
		strTmp = _T("Pixel Format : Mono 14\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
		strTmp = _T("Pixel Format : MONO10_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		strTmp = _T("Pixel Format : MONO12_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_MONO16:
		strTmp = _T("Pixel Format : MONO16_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR8:
		strTmp = _T("Pixel Format : BAYGR8\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG8:
		strTmp = _T("Pixel Format : BAYRG8\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10:
		strTmp = _T("Pixel Format : BAYGR10\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10:
		strTmp = _T("Pixel Format : BAYRG10\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12:
		strTmp = _T("Pixel Format : BAYGR12\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12:
		strTmp = _T("Pixel Format : BAYRG12\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
		strTmp = _T("Pixel Format : BAYGR10_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
		strTmp = _T("Pixel Format : BAYRG10_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
		strTmp = _T("Pixel Format : BAYGR12_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		strTmp = _T("Pixel Format : BAYRG12_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_RGB8:
		strTmp = _T("Pixel Format : RGB8_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_BGR8:
		strTmp = _T("Pixel Format : BGR8\n");
		break;
	case VWSDK::PIXEL_FORMAT_RGB12:
		strTmp = _T("Pixel Format : RGB12\n");
		break;
	case VWSDK::PIXEL_FORMAT_BGR12:
		strTmp = _T("Pixel Format : BGR12\n");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V2_PACKED:
		strTmp = _T("Pixel Format : BGR10_V2_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
		strTmp = _T("Pixel Format : YUV411\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
		strTmp = _T("Pixel Format : YUV422_UYVY\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
		strTmp = _T("Pixel Format : YUV422_YUVY\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV444:
		strTmp = _T("Pixel Format : YUV444\n");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		strTmp = _T("Pixel Format : BGR10_V1_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
		strTmp = _T("Pixel Format : YUV411_10_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
		strTmp = _T("Pixel Format : YUV411_12_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
		strTmp = _T("Pixel Format : YUV422_10_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		strTmp = _T("Pixel Format : YUV422_12_PACKED\n");
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
		strTmp = _T("Pixel Format : PAL_INTERLACED\n");
		break;
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		strTmp = _T("Pixel Format : NTSC_INTERLACED\n");
		break;
	}

	return strTmp;
}


void GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	UINT unWidth  = pImageInfo->width;
	UINT unHeight = pImageInfo->height;
	UINT unBufIdx = pImageInfo->bufferIndex;
	void* vpBuffer = pImageInfo->pImage;
	void* pBuf = NULL;
	VWSDK::PIXEL_FORMAT ePixelFormat = pImageInfo->pixelFormat;

	GetCustomCommand(pObjectInfo->pVwCamera, "Width", &unWidth);
	GetCustomCommand(pObjectInfo->pVwCamera, "Height", &unHeight);
	VWSDK::CameraGetPixelFormat( pObjectInfo->pVwCamera, &ePixelFormat );

	
	BYTE* m_pUnpackedImage = new BYTE[unWidth*unHeight*6];

	BYTE* m_bpConvertPixelFormat =	new BYTE[unWidth*unHeight*2];
	
	switch( ePixelFormat )
	{
		//-----------------------------------------------------------------
		// about MONO Pixel Format Series ---------------------------------
		//-----------------------------------------------------------------
		case VWSDK::PIXEL_FORMAT_MONO8:
			memcpy(m_pUnpackedImage, vpBuffer, unWidth*unHeight);
			break;
		case VWSDK::PIXEL_FORMAT_MONO10:
			VWSDK::ConvertMono10ToBGR8(PBYTE(vpBuffer),
				unWidth*unHeight*2,
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_MONO12:
			VWSDK::ConvertMono12ToBGR8(PBYTE(vpBuffer), 
				unWidth*unHeight*2, 
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_MONO14:
			VWSDK::ConvertMono14ToBGR8(PBYTE(vpBuffer), 
				unWidth*unHeight*2, 
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
		case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
			VWSDK::ConvertMonoPackedToBGR8( PBYTE(vpBuffer),
				UINT(1.5*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_MONO16:
			VWSDK::ConvertMono16PackedToBGR8( PBYTE(vpBuffer),
				UINT(2*unWidth*unHeight),
				m_pUnpackedImage );
			break;

			//-----------------------------------------------------------------
			// about BAYER Pixel Format Series --------------------------------
			//-----------------------------------------------------------------
		case VWSDK::PIXEL_FORMAT_BAYGR8:
			VWSDK::ConvertBAYGR8ToBGR8( PBYTE(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYRG8:
			VWSDK::ConvertBAYRG8ToBGR8( PBYTE(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYGR10:
			VWSDK::ConvertBAYGR10ToBGR8( (WORD*)(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYRG10:
			VWSDK::ConvertBAYRG10ToBGR8( (WORD*)(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYGR12:
			VWSDK::ConvertBAYGR12ToBGR8( (WORD*)(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYRG12:
			VWSDK::ConvertBAYRG12ToBGR8( (WORD*)(vpBuffer),
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
			VWSDK::ConvertMono10PackedToMono16bit( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_bpConvertPixelFormat );
			VWSDK::ConvertBAYGR10ToBGR8( (WORD*)m_bpConvertPixelFormat,
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
			VWSDK::ConvertMono10PackedToMono16bit( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_bpConvertPixelFormat );
			VWSDK::ConvertBAYRG10ToBGR8( (WORD*)m_bpConvertPixelFormat,
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
			VWSDK::ConvertMono12PackedToMono16bit( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_bpConvertPixelFormat );
			VWSDK::ConvertBAYGR12ToBGR8( (WORD*)m_bpConvertPixelFormat,
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
			VWSDK::ConvertMono12PackedToMono16bit( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_bpConvertPixelFormat );
			VWSDK::ConvertBAYRG12ToBGR8( (WORD*)m_bpConvertPixelFormat,
				m_pUnpackedImage,
				unWidth,
				unHeight );
			break;
		case VWSDK::PIXEL_FORMAT_RGB8:
			VWSDK::ConvertRGB8ToBGR8( (PBYTE)vpBuffer,
				UINT(3*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_BGR8:
			goto RETURN_CONVERT_STATUS;
			break;
		case VWSDK::PIXEL_FORMAT_RGB12_PACKED:
			VWSDK::ConvertRGB12PackedToBGR8( (PBYTE)vpBuffer,
				UINT(6*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_BGR12_PACKED:
			goto RETURN_CONVERT_STATUS;
			break;
		case VWSDK::PIXEL_FORMAT_YUV411:
			VWSDK::ConvertYUV411ToBGR8( (PBYTE)vpBuffer,
				UINT(1.5*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
			VWSDK::ConvertYUV422_UYVYToBGR8( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
			VWSDK::ConvertYUV422_YUYVToBGR8( (PBYTE)vpBuffer,
				unWidth,
				unHeight,
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_YUV444:
			VWSDK::ConvertYUV444ToBGR8( (PBYTE)vpBuffer,
				UINT(1.5*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
			goto RETURN_CONVERT_STATUS;
			break;
		case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
		case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
			VWSDK::ConvertYUV411PackedToBGR8( (PBYTE)vpBuffer,
				UINT(2.25*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
		case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
			VWSDK::ConvertYUV422PackedToBGR8( (PBYTE)vpBuffer,
				UINT(3*unWidth*unHeight),
				m_pUnpackedImage );
			break;
		case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
		case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
			goto RETURN_CONVERT_STATUS;
			break;
		default:
			{
		RETURN_CONVERT_STATUS:

				printf( "Do not support current pixel format.\n" );
			}
	}

	pBuf = m_pUnpackedImage;

	// ERROR
	if( pBuf == NULL )
		return;

	// Draw or save routine


	delete [] m_pUnpackedImage;
	delete [] m_bpConvertPixelFormat;

	printf("Frame completed\n");
}


char* Dispatch(char *cmdline, int cmdSize, VWSDK::HCAMERA phCamera)
{
	char hostname   [80] = "Unknown host";
	char subcmdline [80];

	if(0 == strlen(cmdline))
	{

	}
	else if( !(_strnicmp("1", cmdline, 2))) 
	{
		VWSDK::CameraGrab( phCamera );
	}
	else if( !(_strnicmp("2", cmdline, 2))) 
	{
		//sys.StopAcquisition(cmdline);
		VWSDK::CameraAbort( phCamera );
	}
	else if( !(_strnicmp("3", cmdline, 2))) 
	{
		printf("\nInput Gain value\n");
		printf(">");
		
		gets_s(subcmdline, sizeof(subcmdline));

		float gain = atof(subcmdline);

		VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand( phCamera, "Gain", subcmdline );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			printf("Error code : %d\n",ret);
		}
	}
	else if( !(_strnicmp("4", cmdline, 2))) 
	{
		float gain1=0;

		// CameraGetGain
		char chResult[100] = { 0, };
		size_t szResult = sizeof(chResult);
		VWSDK::RESULT ret = VWSDK::CameraGetCustomCommand(phCamera, "GainSelector", chResult, &szResult);
		szResult = sizeof(chResult);
		ZeroMemory(chResult, szResult);
		ret = VWSDK::CameraGetCustomCommand(phCamera, "Gain", chResult, &szResult);
		if (VWSDK::RESULT_SUCCESS == ret){
			gain1 = atof(chResult);
			printf("%f\n", gain1);
		}
		else
			printf("Error code : %d\n", ret);			
		
	}
	else if( !(_strnicmp("5", cmdline, 2))) 
	{
		printf("\nInput Exposure time\n");
		printf(">");
		gets_s(subcmdline, sizeof(subcmdline));

		int exposuretime = atoi(subcmdline);

		VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand( phCamera, "ExposureTime", subcmdline );
		if( ret != VWSDK::RESULT_SUCCESS )
		{
			printf("Error code : %d\n",ret);
		}
	}
	else if( !(_strnicmp("6", cmdline, 2))) 
	{
		char chExposureTime[ 100 ] = {0,};
		size_t szExposureTime = sizeof( chExposureTime );
		VWSDK::RESULT ret = VWSDK::CameraGetCustomCommand( phCamera, "ExposureTime", chExposureTime, &szExposureTime );
		
		if( ret != VWSDK::RESULT_SUCCESS )
		{
			printf("Error code : %d\n",ret);
		}
		else
		{
			printf("%d\n", atoi( chExposureTime ));
		}
	}
	else if( !(_strnicmp("7", cmdline, 2))) 
	{
		printf("\nInput Property name\n");
		printf(">");

		gets_s(subcmdline, sizeof(subcmdline));

		VWSDK::PROPERTY propInfo;
		VWSDK::RESULT ret = CameraGetPropertyInfo(phCamera, subcmdline, &propInfo); 

		if( ret != VWSDK::RESULT_SUCCESS )
		{
			printf("Error code : %d\n",ret);
		}
		else
		{

			if( VWSDK::READ_ONLY == propInfo.eAccessMode)
				printf("Access Mode : Read Only\n");
			else if( VWSDK::WRITE_ONLY == propInfo.eAccessMode)
				printf("Access Mode : Write Only\n");
			else if( VWSDK::READ_WRITE == propInfo.eAccessMode)
				printf("Access Mode : Read Write\n");
			else if( VWSDK::NOT_AVAILABLE == propInfo.eAccessMode)
				printf("Access Mode : Not Available\n");
			else 
				printf("Access Mode : Not Implement\n");

			if( VWSDK::ATTR_BOOLEAN == propInfo.ePropType)
				printf("Property Type : Boolean\n");
			else if( VWSDK::ATTR_CATEGORY == propInfo.ePropType)
				printf("Property Type : Category\n");
			else if( VWSDK::ATTR_COMMAND == propInfo.ePropType)
				printf("Property Type : Command\n");
			else if( VWSDK::ATTR_ENUM == propInfo.ePropType)
				printf("Property Type : Enumeration\n");
			else if( VWSDK::ATTR_FLOAT == propInfo.ePropType)
				printf("Property Type : Float\n");
			else if( VWSDK::ATTR_STRING == propInfo.ePropType)
				printf("Property Type : String\n");
			else if( VWSDK::ATTR_INT == propInfo.ePropType)
				printf("Property Type : Integer\n");
			else
				printf("Property Type : Unknown\n");

			if( VWSDK::BEGINNER == propInfo.eVisibility)
				printf("Property Visibilty : Beginner\n");
			else if( VWSDK::EXPERT == propInfo.eVisibility)
				printf("Property Visibilty : Expert\n");
			else if( VWSDK::GURU == propInfo.eVisibility)
				printf("Property Visibilty : Guru\n");
			else if( VWSDK::INVISIBLE == propInfo.eVisibility)
				printf("Property Visibilty : Invisible\n");
			else
				printf("Property Visibilty : Undefine\n");


		}
	}
	else if ( !(_strnicmp("8", cmdline, 2)) ) 
	{
		
		int nPropCount;

		VWSDK::RESULT ret = VWSDK::CameraGetPropertyCount(phCamera, &nPropCount);

		if( ret != VWSDK::RESULT_SUCCESS )
		{
			printf("%s\n", VwErrorReport(ret) );
			printf("Error code : %d\n",ret);
		}
		else
		{
			VWSDK::PROPERTY propInfo;

			for(int nIdx=0; nIdx<nPropCount; nIdx++ )
			{
				ret = VWSDK::CameraGetPropertyInfoUsingIndex(phCamera, nIdx, &propInfo );

				if( ret != VWSDK::RESULT_SUCCESS )
				{
					printf("%s\n", VwErrorReport(ret) );
					printf("Error code : %d\n",ret);

					break;
				}
				else
				{
					printf("%s \n", propInfo.caName );
				}
			}

		}
		
	}
	else
		strncpy_s(cmdline, cmdSize,  "Unrecognized command\n", cmdSize);

	return cmdline;
}

VWSDK::RESULT GetCustomCommand(VWSDK::HCAMERA phCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
{
	VWSDK::RESULT eRet = VWSDK::RESULT_ERROR;

	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	eRet = CameraGetCustomCommand(phCamera, cpFeatureName, chResult, &szResult, eCmdType);
	if (eRet == VWSDK::RESULT_SUCCESS)
		*unValue = atoi(chResult);

	return eRet;
}

