
#include "stdafx.h"
#include "VwUSB.Demo.USB.Discovery.h"
#include "VwUSB.Demo.USB.DiscoveryCameraDlg.h"
//#include "VwResourceType.h"
#include "VwUSB.h"
#include "VwUSBCamera.h"
#include "VwImageProcess.h"

#define  COUNT_FPS 30

using namespace VWSDK;

IMPLEMENT_DYNAMIC(CVwUSBDemoDiscoveryCameraDlg, CDialog)

CVwUSBDemoDiscoveryCameraDlg::CVwUSBDemoDiscoveryCameraDlg( VwUSB* pUSB, char* pName, CWnd* pParent /*=NULL*/)
	: CDialog(CVwUSBDemoDiscoveryCameraDlg::IDD, pParent)
	, m_pCamera ( NULL )
	, m_pUSB ( pUSB )
	,m_curFPS ( 0 )
	,m_nMinInterFrameTime ( 0 )
	,m_pBmpInfo1 ( NULL )
	,m_hdc1 ( NULL )
	,m_imagecontrolHeight ( 0 )
	,m_imagecontrolWidth ( 0 )
	,m_grabbedimagecount ( 0 )
	,m_imagebuffernumber ( 0 )
	, m_pUnpackedImage( NULL )
{
	strcpy_s( m_szName, 128/*hardcoding*/, pName );

	//Camera 1 Bitmap structure
	m_pBmpInfo1 = (BITMAPINFO*)new BYTE[(sizeof(BITMAPINFOHEADER)+256*sizeof(RGBQUAD))];
	ZeroMemory(m_pBmpInfo1, sizeof(BITMAPINFOHEADER));

	m_pBmpInfo1->bmiHeader.biSize		    = sizeof( BITMAPINFOHEADER );
	m_pBmpInfo1->bmiHeader.biPlanes		    = 1;	m_pBmpInfo1->bmiHeader.biCompression	= BI_RGB;
	m_pBmpInfo1->bmiHeader.biClrImportant   = 0;
	m_pBmpInfo1->bmiHeader.biBitCount	    = 8;
	m_pBmpInfo1->bmiHeader.biClrUsed		= 256;


	for( UINT i = 0; i < m_pBmpInfo1->bmiHeader.biClrUsed; i++ )
	{
		m_pBmpInfo1->bmiColors[i].rgbBlue     = i;
		m_pBmpInfo1->bmiColors[i].rgbGreen    = i;
		m_pBmpInfo1->bmiColors[i].rgbRed      = i;
		m_pBmpInfo1->bmiColors[i].rgbReserved = 0;
	}

	m_liLastDisplayTime.QuadPart = 0;
	m_imageTimeStamps.clear();
	QueryPerformanceFrequency( &m_liFreq );
	m_nMinInterFrameTime         = DWORD(m_liFreq.QuadPart / 30);

	m_pObjectInfo = new OBJECT_INFO;
}

CVwUSBDemoDiscoveryCameraDlg::~CVwUSBDemoDiscoveryCameraDlg()
{
	if ( m_pBmpInfo1 )
	{
		delete[] m_pBmpInfo1;
		m_pBmpInfo1 = NULL;
	}

	if ( m_pObjectInfo )
	{
		delete m_pObjectInfo;
	}

	if( m_pUnpackedImage )
	{
		delete [] m_pUnpackedImage;
		m_pUnpackedImage = NULL;
	}
}

void CVwUSBDemoDiscoveryCameraDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CVwUSBDemoDiscoveryCameraDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_GRAB, &CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonGrab)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BUTTON_SNAP, &CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonSnap)
	ON_BN_CLICKED(IDC_BUTTON_ABORT, &CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonAbort)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE_CAMERA, &CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonCloseCamera)
ON_WM_CLOSE()
END_MESSAGE_MAP()


void CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonGrab()
{
	if ( NULL == m_pCamera )
	{
		return;
	}

	BOOL bGrabbing = FALSE;
	m_pCamera->GetGrabCondition( bGrabbing );

	if ( bGrabbing )
	{
		AfxMessageBox( _T("Now grabbing... Please 'Abort' first.") );
		return;
	}

	if( m_pCamera->Grab() == RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Start - Success"));	
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Start - Fail"));		
	}

	SetTimer(0,1000,NULL);

	// Disable buttons
	//GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_GRAB )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_SNAP )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_CLOSE_CAMERA )->EnableWindow( FALSE );
}


BOOL CVwUSBDemoDiscoveryCameraDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	CWnd* pWnd = NULL;
	pWnd	   = GetDlgItem(IDC_CAMERA1);
	if (pWnd)
	{	
		m_hdc1  = pWnd->GetDC()->GetSafeHdc();
		::SetStretchBltMode(m_hdc1, COLORONCOLOR);
	}
	//Get picture control size
	CRect cr;
	pWnd->GetClientRect(&cr);
	m_imagecontrolWidth  = cr.Width();
	m_imagecontrolHeight = cr.Height();

	BOOL bRet = OpenCamera();

	if ( NULL == bRet )
	{
		// ERROR
	}

	return TRUE;  // return TRUE unless you set the focus to a control
}

VWSDK::RESULT CVwUSBDemoDiscoveryCameraDlg::GetCustomCommand(VWSDK::VwUSBCamera* phCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
{
	VWSDK::RESULT eRet = VWSDK::RESULT_ERROR;

	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	eRet = phCamera->GetCustomCommand(cpFeatureName, chResult, &szResult, eCmdType);
	if (eRet == VWSDK::RESULT_SUCCESS)
		*unValue = atoi(chResult);
	
	return eRet;
}

void CVwUSBDemoDiscoveryCameraDlg::SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN PIXEL_FORMAT ePixelFormat )
{
	CString strtemp;
	strtemp.Format(_T("%d"),tempwidth);
	GetDlgItem(IDC_STATIC_WIDTH)->SetWindowText(strtemp);

	strtemp.Format(_T("%d"),tempheight);
	GetDlgItem(IDC_STATIC_HEIGHT)->SetWindowText(strtemp);

	CString strPixelFormat;
	
	strPixelFormat = GetPixelFormatName( ePixelFormat );	
	
	GetDlgItem(IDC_STATIC_PIXEL_FORMAT)->SetWindowText(strPixelFormat);

}

void CVwUSBDemoDiscoveryCameraDlg::OnTimer(UINT_PTR nIDEvent)
{
	CString tempstr;

	if( nIDEvent == 0 )
	{
		if (m_curFPS)
		{
			tempstr.Format(_T("%3.2f"), m_curFPS);

			GetDlgItem(IDC_STATIC_FPS)->SetWindowText(tempstr);
		}
	}	

	CDialog::OnTimer(nIDEvent);
}

BOOL CVwUSBDemoDiscoveryCameraDlg::OpenCamera()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pUSB )
	{
		// ERROR
		ASSERT( m_pUSB );
		return FALSE;
	}


	RESULT ret = RESULT_ERROR;

	const int BUFFER_SIZE = 10;

	ret = m_pUSB->OpenCamera( m_szName, &m_pCamera, BUFFER_SIZE, 0, 0, 0, m_pObjectInfo, DrawImage, Disconnect );
	
	if(ret != RESULT_SUCCESS)
	{
		switch(ret)
		{
		default:
			{
				AfxMessageBox(_T("ERROR : Default error code returned"));
			}
			break;
		case RESULT_ERROR_DEVCREATEDATASTREAM:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned"));
			}
			break;
		case RESULT_ERROR_NO_CAMERAS:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_NO_CAMERAS was returned"));
				AfxMessageBox(_T("CHECK : NIC properties"));
			}
			break;
		case RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned"));
				AfxMessageBox(_T("CHECK : Zero-based camera index"));
			}
			break;
		case RESULT_ERROR_DATASTREAM_MTU:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_STREAM_MTU was returned"));
				AfxMessageBox(_T("CHECK : Check NIC MTU"));
			}
			break;
		case RESULT_ERROR_INSUFFICIENT_RESOURCES:
			{
				AfxMessageBox( _T("ERROR : RESULT_ERROR_BUFFER_TOO_SMALL") );
				AfxMessageBox(_T("CHECK : Check system resources"));
			}
			break;
		}
		return FALSE;
	}

	m_pObjectInfo->pUserPointer = this;
	m_pObjectInfo->pVwCamera = m_pCamera;

	MakeUnPackedBuffer();

	// Update resolution info.
	UINT nWidth = 0;
	UINT nHeight = 0;
	PIXEL_FORMAT pixelFormat = PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	m_pCamera->GetPixelFormat( &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );

	return TRUE;
}
void CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonSnap()
{
	if ( NULL == m_pCamera )
	{
		return;
	}

	BOOL bGrabbing = FALSE;
	m_pCamera->GetGrabCondition( bGrabbing );

	if ( bGrabbing )
	{
		AfxMessageBox( _T("Now grabbing... Please 'Abort' first.") );
		return;
	}

	const int nFrame = 1;

	//Exception
	if ( nFrame < 0 )
	{
		AfxMessageBox( _T("Must be greater than 0.") );
	}
	const BOOL* pbIsRunning = NULL;
	if ( m_pCamera->Snap( nFrame ) == RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Snap - Success"));	
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Snap - Fail"));		
	}

	// Update resolution info.
	UINT nWidth = 0;
	UINT nHeight = 0;
	PIXEL_FORMAT pixelFormat = PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	m_pCamera->GetPixelFormat( &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );
}

void CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonAbort()
{
	CWaitCursor oWaitCursor;

	if( NULL == m_pCamera )
	{
		return;
	}

	if( m_pCamera->Abort() == RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Stop - Success"));		
		m_grabbedimagecount = 0;
		m_imageTimeStamps.clear();
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Stop - Fail"));		
	}

	KillTimer(0);

	GetDlgItem( IDC_BUTTON_GRAB )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_SNAP )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_CLOSE_CAMERA )->EnableWindow( TRUE );
	
}

void CVwUSBDemoDiscoveryCameraDlg::OnBnClickedButtonCloseCamera()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pCamera )
	{
		// ERROR
		return;
	}

	if ( m_pCamera->CloseCamera() == RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	OnOK();
}

void CVwUSBDemoDiscoveryCameraDlg::MakeUnPackedBuffer()
{
	if ( NULL == m_pCamera )
	{
		// ERROR
		return;
	}

	if ( m_pUnpackedImage )
	{
		delete [] m_pUnpackedImage;
		m_pUnpackedImage = NULL;
	}

	//Get image width,height 
	UINT nWidth = 0;
	UINT nHeight = 0;
	PIXEL_FORMAT pixelFormat = PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	m_pCamera->GetPixelFormat( &pixelFormat );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	m_pUnpackedImage = new BYTE[(size_t)(nWidth) * (size_t)(nHeight) * 3];
}

void CVwUSBDemoDiscoveryCameraDlg::OnClose()
{
	if ( m_pCamera->CloseCamera() == RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	CDialog::OnClose();
}

void CVwUSBDemoDiscoveryCameraDlg::Disconnect( OBJECT_INFO* pObjectInfo, DISCONNECT_INFO tDisconnect )
{
	CVwUSBDemoDiscoveryCameraDlg* pDlg = (CVwUSBDemoDiscoveryCameraDlg*)pObjectInfo->pUserPointer;

	CString strMSG;
	strMSG.Format( _T("Device connection has been lost.\nHeartBeatTimeOut : %d, TimeOutTryCount : %d"),
					tDisconnect.nCurrHeartBeatTimeOut,
					tDisconnect.nTimeOutTryCount );

	AfxMessageBox( strMSG );

	// Do not call the CloseCamera or OpenCamera function in this callback.
}

void CVwUSBDemoDiscoveryCameraDlg::DrawImage( OBJECT_INFO* pObjectInfo, IMAGE_INFO* pImageInfo )
{

	CVwUSBDemoDiscoveryCameraDlg* pDlg = (CVwUSBDemoDiscoveryCameraDlg*)pObjectInfo->pUserPointer;

	// FPS
	LARGE_INTEGER liTime;
	QueryPerformanceCounter( &liTime ); 

	while( pDlg->m_imageTimeStamps.size() > COUNT_FPS )
		pDlg->m_imageTimeStamps.pop_front();

	pDlg->m_imageTimeStamps.push_back( liTime );

	__int64 diff = liTime.QuadPart-pDlg->m_imageTimeStamps.begin()->QuadPart;

	if( diff > 0 )
		pDlg->m_curFPS = (pDlg->m_liFreq.QuadPart * double(pDlg->m_imageTimeStamps.size()-1)) / diff;
	else
		pDlg->m_curFPS = 0;

	if( liTime.QuadPart - pDlg->m_liLastDisplayTime.QuadPart > pDlg->m_nMinInterFrameTime )
	{
		UINT unWidth		= pImageInfo->width;
		UINT unHeight		= pImageInfo->height;
		UINT unBufIdx		= pImageInfo->bufferIndex;
		UINT unBitCount		= 0;
		UINT unbiClrUsed	= 0;
		int nCalcHeight		= 0;
		void* vpBuffer		= pImageInfo->pImage;
		void* pBuf			= NULL;
		PIXEL_FORMAT ePixelFormat = pImageInfo->pixelFormat;
		CString strTmp = _T("");


		pDlg->GetCustomCommand(((VwUSBCamera*)pObjectInfo->pVwCamera), "Width", &unWidth);
		pDlg->GetCustomCommand(((VwUSBCamera*)pObjectInfo->pVwCamera), "Height", &unHeight);			
		((VwUSBCamera*)pObjectInfo->pVwCamera)->GetPixelFormat(&ePixelFormat);

		if ( VWSDK::PIXEL_FORMAT_MONO8 == ePixelFormat )
		{
			nCalcHeight = (-1 * unHeight);
			unBitCount	= 8;
			unbiClrUsed	= 256;
		}
		else
		{
			nCalcHeight = (-1 * unHeight);
			unBitCount	= 24;
			unbiClrUsed	= 0;
		}

		unBitCount = pDlg->GetPixelBitCount( ePixelFormat );

		pDlg->m_pBmpInfo1->bmiHeader.biWidth       = unWidth;
		pDlg->m_pBmpInfo1->bmiHeader.biHeight      = nCalcHeight;
		pDlg->m_pBmpInfo1->bmiHeader.biBitCount    = unBitCount;
		pDlg->m_pBmpInfo1->bmiHeader.biCompression = BI_RGB;
		pDlg->m_pBmpInfo1->bmiHeader.biClrUsed     = unbiClrUsed;

		BOOL bRet = pDlg->ConvertPixelFormat( ePixelFormat, pDlg->m_pUnpackedImage, (BYTE*)vpBuffer, unWidth, unHeight );
		
		if ( FALSE == bRet )
		{
			AfxMessageBox( _T("Do not support current pixel format.") );
			return;
		}

		pBuf = pDlg->m_pUnpackedImage;

		// ERROR
		if( pBuf == NULL )
			return;

		pDlg->m_liLastDisplayTime = liTime;

		UINT unWidthPos		= 0;
		UINT unHeightPos	= 0;

		::SetStretchBltMode( pDlg->m_hdc1, COLORONCOLOR );
		StretchDIBits( pDlg->m_hdc1,
			unWidthPos,
			unHeightPos,
			pDlg->m_imagecontrolWidth,
			pDlg->m_imagecontrolHeight,
			0,
			0,
			unWidth,
			unHeight,
			pBuf,
			pDlg->m_pBmpInfo1,
			DIB_RGB_COLORS,
			SRCCOPY );	
	}

	pDlg->m_grabbedimagecount++;
}


CString CVwUSBDemoDiscoveryCameraDlg::GetPixelFormatName( VWSDK::PIXEL_FORMAT ePixelFormat )
{
	CString strTmp;

	switch( ePixelFormat )
	{
	case VWSDK::PIXEL_FORMAT_MONO8:
		strTmp = _T("Mono8");
		break;
	case VWSDK::PIXEL_FORMAT_MONO8_SIGNED:
		strTmp = _T("Mono8signed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
		strTmp = _T("Mono10");
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
		strTmp = _T("Mono10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12:
		strTmp = _T("Mono12");
		break;
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		strTmp = _T("Mono12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_MONO14:
		strTmp = _T("Mono14");
		break;
	case VWSDK::PIXEL_FORMAT_MONO16:
		strTmp = _T("Mono16");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR8:
		strTmp = _T("BayerGR8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG8:
		strTmp = _T("BayerRG8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB8:
		strTmp = _T("BayerGB8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG8:
		strTmp = _T("BayerBG8");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10:
		strTmp = _T("BayerGR10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10:
		strTmp = _T("BayerRG10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB10:
		strTmp = _T("BayerGB10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG10:
		strTmp = _T("BayerBG10");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
		strTmp = _T("BayerGR10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
		strTmp = _T("BayerRG10Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12:
		strTmp = _T("BayerGR12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12:
		strTmp = _T("BayerRG12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGB12:
		strTmp = _T("BayerGB12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYBG12:
		strTmp = _T("BayerBG12");
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		strTmp = _T("BayerRG12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
		strTmp = _T("BayerGR12Packed");
		break;	
	case VWSDK::PIXEL_FORMAT_RGB8:
		strTmp = _T("RGB8");
		break;
	case VWSDK::PIXEL_FORMAT_BGR8:
		strTmp = _T("BGR8");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
		strTmp = _T("YUV422Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
		strTmp = _T("YUV422YUVYPacked");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
		strTmp = _T("YUV42210Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		strTmp = _T("YUV42212Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
		strTmp = _T("YUV411");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
		strTmp = _T("YUV41110Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
		strTmp = _T("YUV41112Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		strTmp = _T("BGR10V1Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V2_PACKED:
		strTmp = _T("BGR10V2Packed");
		break;
	case VWSDK::PIXEL_FORMAT_RGB12_PACKED:
		strTmp = _T("RGB12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_BGR12_PACKED:
		strTmp = _T("BGR12Packed");
		break;
	case VWSDK::PIXEL_FORMAT_YUV444:
		strTmp = _T("YUV444");
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
		strTmp = _T("PALInterlaced");
		break;
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		strTmp = _T("NTSCInterlaced");
		break;
	default:
		strTmp = _T("Unrecognized pixel format\n");
	}

	return strTmp;
}


int CVwUSBDemoDiscoveryCameraDlg::GetPixelBitCount( VWSDK::PIXEL_FORMAT ePixelFormat )
{
	int nRet = 0;
	switch( ePixelFormat )
	{
		//-----------------------------------------------------------------
		// about MONO Pixel Format Series ---------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_MONO8:
		nRet	=	8;
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
	case VWSDK::PIXEL_FORMAT_MONO12:
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO14:
	case VWSDK::PIXEL_FORMAT_MONO16:
		nRet	=	24;
		break;
		//-----------------------------------------------------------------
		// about BAYER Pixel Format Series --------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_BAYGR8:
	case VWSDK::PIXEL_FORMAT_BAYRG8:
	case VWSDK::PIXEL_FORMAT_BAYGR10:
	case VWSDK::PIXEL_FORMAT_BAYRG10:
	case VWSDK::PIXEL_FORMAT_BAYGR12:
	case VWSDK::PIXEL_FORMAT_BAYRG12:
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_RGB8:
	case VWSDK::PIXEL_FORMAT_BGR8:
	case VWSDK::PIXEL_FORMAT_RGB12_PACKED:
	case VWSDK::PIXEL_FORMAT_BGR12_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V2_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
	case VWSDK::PIXEL_FORMAT_YUV444:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		nRet	=	32;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		nRet	=	24;
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		nRet	=	24;
		break;
	default:
		{
		}
	}

	return nRet;
}


BOOL CVwUSBDemoDiscoveryCameraDlg::ConvertPixelFormat( VWSDK::PIXEL_FORMAT ePixelFormat, BYTE* pDest, BYTE* pSource, int nWidth, int nHeight )
{
	if ( NULL == pDest ||
		NULL == pSource )
	{
		return FALSE;
	}

	if ( 0 == nWidth || 0 == nHeight )
	{
		return FALSE;
	}

	BOOL bRet = TRUE;
	BYTE* bpConvertPixelFormat =	new BYTE[ nWidth * nHeight * 2 ];

	switch( ePixelFormat )
	{
		//-----------------------------------------------------------------
		// about MONO Pixel Format Series ---------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_MONO8:
		memcpy( pDest, pSource, nWidth * nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_MONO10:
		VwImageProcess::ConvertMono10ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break;
	case VWSDK::PIXEL_FORMAT_MONO12:
		VwImageProcess::ConvertMono12ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break;
	case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
	case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		VwImageProcess::ConvertMonoPackedToBGR8( pSource,
			UINT(1.5*nWidth*nHeight),
			pDest );
		break;

	case VWSDK::PIXEL_FORMAT_MONO14:
		VwImageProcess::ConvertMono14ToBGR8(PBYTE(pSource), nWidth*nHeight*2, pDest);
		break;

	case VWSDK::PIXEL_FORMAT_MONO16:
		VwImageProcess::ConvertMono16PackedToBGR8( pSource,
			UINT(2*nWidth*nHeight),
			pDest );
		break;
		//-----------------------------------------------------------------
		// about BAYER Pixel Format Series --------------------------------
		//-----------------------------------------------------------------
	case VWSDK::PIXEL_FORMAT_BAYGR8:
		VwImageProcess::ConvertBAYGR8ToBGR8( pSource,
			pDest,
			nWidth,
			nHeight );
		break;

		/// Delete::20150629, Dong-Uk Lee, Basler USB 카메라 테스트용
		// 	case VWSDK::PIXEL_FORMAT_BAYBG8:
		// 		ConvertBAYBG8ToBGR8( pSource,
		// 											pDest,
		// 											nWidth,
		// 											nHeight );
		// 		break;
		/// Delete::20150629, Dong-Uk Lee, 
		/// 
	case VWSDK::PIXEL_FORMAT_BAYRG8:
		VwImageProcess::ConvertBAYRG8ToBGR8( pSource,
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10:
		VwImageProcess::ConvertBAYGR10ToBGR8( (WORD*)(pSource),
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10:
		VwImageProcess::ConvertBAYRG10ToBGR8( (WORD*)(pSource),
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12:
		VwImageProcess::ConvertBAYGR12ToBGR8( (WORD*)(pSource),
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12:
		VwImageProcess::ConvertBAYRG12ToBGR8( (WORD*)(pSource),
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
		VwImageProcess::ConvertMono10PackedToMono16bit( (PBYTE)pSource,
			nWidth,
			nHeight,
			bpConvertPixelFormat );
		VwImageProcess::ConvertBAYGR10ToBGR8( (WORD*)bpConvertPixelFormat,
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG10_PACKED:
		VwImageProcess::ConvertMono10PackedToMono16bit( (PBYTE)pSource,
			nWidth,
			nHeight,
			bpConvertPixelFormat );
		VwImageProcess::ConvertBAYRG10ToBGR8( (WORD*)bpConvertPixelFormat,
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYGR12_PACKED:
		VwImageProcess::ConvertMono12PackedToMono16bit( (PBYTE)pSource,
			nWidth,
			nHeight,
			bpConvertPixelFormat );
		VwImageProcess::ConvertBAYGR12ToBGR8( (WORD*)bpConvertPixelFormat,
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_BAYRG12_PACKED:
		VwImageProcess::ConvertMono12PackedToMono16bit( (PBYTE)pSource,
			nWidth,
			nHeight,
			bpConvertPixelFormat );
		VwImageProcess::ConvertBAYRG12ToBGR8( (WORD*)bpConvertPixelFormat,
			pDest,
			nWidth,
			nHeight );
		break;
	case VWSDK::PIXEL_FORMAT_RGB8:
		VwImageProcess::ConvertRGB8ToBGR8( (PBYTE)pSource,
			UINT(3*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_BGR8:
		bRet = FALSE;
		break;
	case VWSDK::PIXEL_FORMAT_RGB12_PACKED:
		VwImageProcess::ConvertRGB12PackedToBGR8( (PBYTE)pSource,
			UINT(6*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_BGR12_PACKED:
		bRet = FALSE;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411:
		VwImageProcess::ConvertYUV411ToBGR8( (PBYTE)pSource,
			UINT(1.5*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_UYVY:
		VwImageProcess::ConvertYUV422_UYVYToBGR8( (PBYTE)pSource,
			nWidth,
			nHeight,
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_YUYV:
		VwImageProcess::ConvertYUV422_YUYVToBGR8( (PBYTE)pSource,
			nWidth,
			nHeight,
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV444:
		VwImageProcess::ConvertYUV444ToBGR8( (PBYTE)pSource,
			UINT(1.5*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_BGR10V1_PACKED:
		bRet = FALSE;
		break;
	case VWSDK::PIXEL_FORMAT_YUV411_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV411_12_PACKED:
		VwImageProcess::ConvertYUV411PackedToBGR8( (PBYTE)pSource,
			UINT(2.25*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_YUV422_10_PACKED:
	case VWSDK::PIXEL_FORMAT_YUV422_12_PACKED:
		VwImageProcess::ConvertYUV422PackedToBGR8( (PBYTE)pSource,
			UINT(3*nWidth*nHeight),
			pDest );
		break;
	case VWSDK::PIXEL_FORMAT_PAL_INTERLACED:
	case VWSDK::PIXEL_FORMAT_NTSC_INTERLACED:
		break;
	default:
		{
			bRet = FALSE;
		}
	}

	if ( NULL != bpConvertPixelFormat )
	{
		delete [] bpConvertPixelFormat;
	}

	return bRet;
}

