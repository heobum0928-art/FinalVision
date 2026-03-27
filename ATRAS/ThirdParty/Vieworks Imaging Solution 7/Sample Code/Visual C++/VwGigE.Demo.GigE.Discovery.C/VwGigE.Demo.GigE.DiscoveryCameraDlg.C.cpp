#include "stdafx.h"
#include "VwGigE.Demo.GigE.Discovery.C.h"
#include "VwGigE.Demo.GigE.DiscoveryCameraDlg.C.h"
#include "VwResourceType.h"

#define  COUNT_FPS 30

IMPLEMENT_DYNAMIC(CVwGigEDemoGigEDiscoveryCameraCDlg, CDialog)

CVwGigEDemoGigEDiscoveryCameraCDlg::CVwGigEDemoGigEDiscoveryCameraCDlg( VWSDK::VWGIGE_HANDLE pvwGigE, char* pName, CWnd* pParent /*=NULL*/)
	: CDialog(CVwGigEDemoGigEDiscoveryCameraCDlg::IDD, pParent)
	, m_pCamera ( NULL )
	, m_pvwGigE ( pvwGigE )
	,m_curFPS ( 0 )
	,m_nMinInterFrameTime ( 0 )
	,m_pBmpInfo1 ( NULL )
	,m_hdc1 ( NULL )
	,m_imagecontrolHeight ( 0 )
	,m_imagecontrolWidth ( 0 )
	,m_grabbedimagecount ( 0 )
	,m_imagebuffernumber ( 0 )
	, m_pUnpackedImage ( NULL )
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

	m_pObjectInfo = new VWSDK::OBJECT_INFO;
}

CVwGigEDemoGigEDiscoveryCameraCDlg::~CVwGigEDemoGigEDiscoveryCameraCDlg()
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

void CVwGigEDemoGigEDiscoveryCameraCDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CVwGigEDemoGigEDiscoveryCameraCDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_GRAB, &CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonGrab)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BUTTON_SNAP, &CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonSnap)
	ON_BN_CLICKED(IDC_BUTTON_ABORT, &CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonAbort)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE_CAMERA, &CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonCloseCamera)
	ON_WM_CLOSE()
END_MESSAGE_MAP()


void CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonGrab()
{
	if ( NULL == m_pCamera )
	{
		return;
	}

	BOOL bGrabbing = FALSE;
	VWSDK::CameraGetGrabCondition( m_pCamera, bGrabbing );

	if ( bGrabbing )
	{
		AfxMessageBox( _T("Now grabbing... Please 'Abort' first.") );
		return;
	}

	if( VWSDK::CameraGrab( m_pCamera ) == VWSDK::RESULT_SUCCESS )
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

BOOL CVwGigEDemoGigEDiscoveryCameraCDlg::OnInitDialog()
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

VWSDK::RESULT CVwGigEDemoGigEDiscoveryCameraCDlg::GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
{
	VWSDK::RESULT eRet = VWSDK::RESULT_ERROR;

	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	eRet = CameraGetCustomCommand(hCamera, cpFeatureName, chResult, &szResult, eCmdType);
	if (eRet == VWSDK::RESULT_SUCCESS)
		*unValue = atoi(chResult);

	return eRet;
}

void CVwGigEDemoGigEDiscoveryCameraCDlg::GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwGigEDemoGigEDiscoveryCameraCDlg *dlg = (CVwGigEDemoGigEDiscoveryCameraCDlg *)pObjectInfo->pUserPointer;
	
	// FPS
	LARGE_INTEGER liTime;
	QueryPerformanceCounter( &liTime ); 

	while( dlg->m_imageTimeStamps.size() > COUNT_FPS )
		dlg->m_imageTimeStamps.pop_front();

	dlg->m_imageTimeStamps.push_back( liTime );

	__int64 diff = liTime.QuadPart-dlg->m_imageTimeStamps.begin()->QuadPart;

	if( diff > 0 )
		dlg->m_curFPS = (dlg->m_liFreq.QuadPart * double(dlg->m_imageTimeStamps.size()-1)) / diff;
	else
		dlg->m_curFPS = 0;

	if( liTime.QuadPart - dlg->m_liLastDisplayTime.QuadPart > dlg->m_nMinInterFrameTime )
	{
		UINT nWidth = pImageInfo->width;
		UINT nHeight = pImageInfo->height;
		VWSDK::PIXEL_FORMAT pixelFormat = pImageInfo->pixelFormat;
		UINT nBufferIndex = pImageInfo->bufferIndex;
		void* pBuffer = pImageInfo->pImage;

		dlg->GetCustomCommand(dlg->m_pCamera, "Width", &nWidth);
		dlg->GetCustomCommand(dlg->m_pCamera, "Height", &nHeight);
		VWSDK::CameraGetPixelFormat( dlg->m_pCamera, &pixelFormat );

		dlg->m_pBmpInfo1->bmiHeader.biWidth	       =  nWidth;

		if ( VWSDK::PIXEL_FORMAT_MONO8 == pixelFormat )
		{
			dlg->m_pBmpInfo1->bmiHeader.biHeight   = -1 * nHeight;
			dlg->m_pBmpInfo1->bmiHeader.biBitCount = 8;	
		}
		else
		{
			dlg->m_pBmpInfo1->bmiHeader.biHeight   = -1 * nHeight;
			dlg->m_pBmpInfo1->bmiHeader.biBitCount = 24;
		}

		CString strTemp;
		strTemp = CVwResourceType::GetPixelFormatName( pixelFormat );
		
		strTemp = _T("Pixel Format : ") + strTemp;

		dlg->m_liLastDisplayTime = liTime;

		void* pBuf = NULL;

		BOOL bRet = CVwResourceType::ConvertPixelFormat( pixelFormat, dlg->m_pUnpackedImage, (BYTE*)pBuffer, nWidth, nHeight );

		if ( FALSE == bRet )
		{
			AfxMessageBox( _T("Do not support current pixel format.") );
			return;
		}

		if ( NULL == pBuf )
		{
			// ERROR
			return;
		}

		::SetStretchBltMode(dlg->m_hdc1,COLORONCOLOR);

		StretchDIBits(
			dlg->m_hdc1,
			0,
			0,
			dlg->m_imagecontrolWidth,
			dlg->m_imagecontrolHeight,
			0,
			0,
			nWidth,
			nHeight,
			pBuf,
			dlg->m_pBmpInfo1,
			DIB_RGB_COLORS,
			SRCCOPY
			);	


	}

	dlg->m_grabbedimagecount++;
	CString tempvalue = _T("");
	tempvalue.Format(_T("%d"),dlg->m_grabbedimagecount);
	dlg->GetDlgItem(IDC_STATIC_LOG)->SetWindowText(tempvalue);	
}


void CVwGigEDemoGigEDiscoveryCameraCDlg::SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat )
{
	CString strtemp;
	strtemp.Format(_T("%d"),tempwidth);
	GetDlgItem(IDC_STATIC_WIDTH)->SetWindowText(strtemp);

	strtemp.Format(_T("%d"),tempheight);
	GetDlgItem(IDC_STATIC_HEIGHT)->SetWindowText(strtemp);

	CString strPixelFormat;

	strPixelFormat = CVwResourceType::GetPixelFormatName( ePixelFormat );

	GetDlgItem(IDC_STATIC_PIXEL_FORMAT)->SetWindowText(strPixelFormat);
}

void CVwGigEDemoGigEDiscoveryCameraCDlg::OnTimer(UINT_PTR nIDEvent)
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

BOOL CVwGigEDemoGigEDiscoveryCameraCDlg::OpenCamera()
{
	if ( NULL == m_pvwGigE )
	{
		ASSERT( m_pvwGigE );
		return FALSE;
	}


	VWSDK::RESULT ret = VWSDK::RESULT_ERROR;

	const int BUFFER_SIZE = 2;
	ret = VwOpenCameraByName( m_pvwGigE, m_szName, &m_pCamera, BUFFER_SIZE, 0, 0, 0, m_pObjectInfo, DrawImage, Disconnect );
	if(ret != VWSDK::RESULT_SUCCESS)
	{
		switch(ret)
		{
		default:
			{
				AfxMessageBox(_T("ERROR : Default error code returned"));
			}
			break;
		case VWSDK::RESULT_ERROR_DEVCREATEDATASTREAM:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_DEVCREATESTREAM was returned"));
			}
			break;
		case VWSDK::RESULT_ERROR_NO_CAMERAS:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_NO_CAMERAS was returned"));
				AfxMessageBox(_T("CHECK : NIC properties"));
			}
			break;
		case VWSDK::RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_VWCAMERA_CAMERAINDEX_OVER was returned"));
				AfxMessageBox(_T("CHECK : Zero-based camera index"));
			}
			break;
		case VWSDK::RESULT_ERROR_DATASTREAM_MTU:
			{
				AfxMessageBox(_T("ERROR : RESULT_ERROR_STREAM_MTU was returned"));
				AfxMessageBox(_T("CHECK : Check NIC MTU"));
			}
			break;
		case VWSDK::RESULT_ERROR_INSUFFICIENT_RESOURCES:
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
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );

	return TRUE;
}
void CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonSnap()
{
	if ( NULL == m_pCamera )
	{
		return;
	}

	BOOL bGrabbing = FALSE;
	VWSDK::CameraGetGrabCondition( m_pCamera, bGrabbing );

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

	if ( VWSDK::CameraSnap( m_pCamera, nFrame ) == VWSDK::RESULT_SUCCESS )
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
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );

}

void CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonAbort()
{
	if( NULL == m_pCamera )
	{
		return;
	}

	if( VWSDK::CameraAbort( m_pCamera ) == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Stop - Success"));		
		m_grabbedimagecount = 0;
		m_imageTimeStamps.clear();
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Stop - Fail"));		
		//return;
	}

	KillTimer(0);

	GetDlgItem( IDC_BUTTON_GRAB )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_CLOSE_CAMERA )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_SNAP )->EnableWindow( TRUE );

}

void CVwGigEDemoGigEDiscoveryCameraCDlg::OnBnClickedButtonCloseCamera()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pCamera )
	{
		// ERROR
		return;
	}

	if ( VWSDK::CameraClose( m_pCamera ) == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	m_pCamera = NULL;

	OnOK();
}


void CVwGigEDemoGigEDiscoveryCameraCDlg::MakeUnPackedBuffer()
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
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );


	m_pUnpackedImage = new BYTE[(size_t)(nWidth) * (size_t)(nHeight) * 6];
}
void CVwGigEDemoGigEDiscoveryCameraCDlg::OnClose()
{
	if ( VWSDK::CameraClose( m_pCamera ) == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	CDialog::OnClose();
}


void CVwGigEDemoGigEDiscoveryCameraCDlg::DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo )
{

	CVwGigEDemoGigEDiscoveryCameraCDlg* pDlg = (CVwGigEDemoGigEDiscoveryCameraCDlg*)pObjectInfo->pUserPointer;


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
		VWSDK::PIXEL_FORMAT ePixelFormat = pImageInfo->pixelFormat;
		CString strTmp = _T("");

		
		pDlg->GetCustomCommand(pObjectInfo->pVwCamera, "Width", &unWidth);
		pDlg->GetCustomCommand(pObjectInfo->pVwCamera, "Height", &unHeight);
		VWSDK::CameraGetPixelFormat( pObjectInfo->pVwCamera, &ePixelFormat);

		if ( VWSDK::PIXEL_FORMAT_MONO8 == ePixelFormat )
		{
			nCalcHeight = (-1 * unHeight);
			unBitCount	= 8;
			unbiClrUsed	= 256;
		}
		else
		{
			nCalcHeight = (-1 * unHeight);
			unBitCount  = 24;
			unbiClrUsed	= 0;
		}

		strTmp = CVwResourceType::GetPixelFormatName( ePixelFormat );
		strTmp = _T("Pixel Format : ") + strTmp;
		
		pDlg->m_pBmpInfo1->bmiHeader.biWidth       = unWidth;
		pDlg->m_pBmpInfo1->bmiHeader.biHeight      = nCalcHeight;
		pDlg->m_pBmpInfo1->bmiHeader.biBitCount    = unBitCount;
		pDlg->m_pBmpInfo1->bmiHeader.biCompression = BI_RGB;
		pDlg->m_pBmpInfo1->bmiHeader.biClrUsed     = unbiClrUsed;

		//pDlg->GetDlgItem(IDC_STATIC_PIXEL_DEPTH)->SetWindowText(strTmp);

		BOOL bRet = CVwResourceType::ConvertPixelFormat( ePixelFormat, pDlg->m_pUnpackedImage, (BYTE*)vpBuffer, unWidth, unHeight );
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

void CVwGigEDemoGigEDiscoveryCameraCDlg::Disconnect( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnect )
{
	CVwGigEDemoGigEDiscoveryCameraCDlg* pDlg = (CVwGigEDemoGigEDiscoveryCameraCDlg*)pObjectInfo->pUserPointer;

	CString strMSG;
	strMSG.Format( _T("Device connection has been lost.\nHeartBeatTimeOut : %d, TimeOutTryCount : %d"),
		tDisconnect.nCurrHeartBeatTimeOut,
		tDisconnect.nTimeOutTryCount );

	AfxMessageBox( strMSG );

	// Do not call the CloseCamera or OpenCamera function in this callback.
}