
#include "stdafx.h"
#include "VwGigE.Demo.GigE.Discovery.h"
#include "VwGigE.Demo.GigE.DiscoveryCameraDlg.h"
#include "VwResourceType.h"
#include "VwGigE.h"
#include "VwCamera.h"
#include "VwImageProcess.h"


#define  COUNT_FPS 30

IMPLEMENT_DYNAMIC(CVwGigEDemoGigEDiscoveryCameraDlg, CDialog)

CVwGigEDemoGigEDiscoveryCameraDlg::CVwGigEDemoGigEDiscoveryCameraDlg( VWSDK::VwGigE* pvwGigE, char* pName, CWnd* pParent /*=NULL*/)
	: CDialog(CVwGigEDemoGigEDiscoveryCameraDlg::IDD, pParent)
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

	m_pObjectInfo = new VWSDK::OBJECT_INFO;
}

CVwGigEDemoGigEDiscoveryCameraDlg::~CVwGigEDemoGigEDiscoveryCameraDlg()
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

void CVwGigEDemoGigEDiscoveryCameraDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CVwGigEDemoGigEDiscoveryCameraDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_GRAB, &CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonGrab)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BUTTON_SNAP, &CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonSnap)
	ON_BN_CLICKED(IDC_BUTTON_ABORT, &CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonAbort)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE_CAMERA, &CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonCloseCamera)
ON_WM_CLOSE()
END_MESSAGE_MAP()


void CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonGrab()
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

	if( m_pCamera->Grab() == VWSDK::RESULT_SUCCESS )
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


BOOL CVwGigEDemoGigEDiscoveryCameraDlg::OnInitDialog()
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
VWSDK::RESULT CVwGigEDemoGigEDiscoveryCameraDlg::GetCustomCommand(VWSDK::VwCamera* phCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
{
	VWSDK::RESULT eRet = VWSDK::RESULT_ERROR;

	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	eRet = phCamera->GetCustomCommand(cpFeatureName, chResult, &szResult, (int)eCmdType);
	if (eRet == VWSDK::RESULT_SUCCESS)
		*unValue = atoi(chResult);

	return eRet;
}

void CVwGigEDemoGigEDiscoveryCameraDlg::SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat )
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

void CVwGigEDemoGigEDiscoveryCameraDlg::OnTimer(UINT_PTR nIDEvent)
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

BOOL CVwGigEDemoGigEDiscoveryCameraDlg::OpenCamera()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pvwGigE )
	{
		// ERROR
		ASSERT( m_pvwGigE );
		return FALSE;
	}


	VWSDK::RESULT ret = VWSDK::RESULT_ERROR;

	const int BUFFER_SIZE = 2;

	ret = m_pvwGigE->OpenCamera( m_szName, &m_pCamera, BUFFER_SIZE, 0, 0, 0, m_pObjectInfo, DrawImage, Disconnect );
	
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
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	UINT nHeight = 0;
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	m_pCamera->GetPixelFormat( &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );

	return TRUE;
}
void CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonSnap()
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
	if ( m_pCamera->Snap( nFrame ) == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Snap - Success"));	
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Snap - Fail"));		
	}

	// Update resolution info.
	UINT nWidth = 0;
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	UINT nHeight = 0;
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	
	m_pCamera->GetPixelFormat( &pixelFormat );

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat );
}

void CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonAbort()
{
	CWaitCursor oWaitCursor;

	if( NULL == m_pCamera )
	{
		return;
	}

	if( m_pCamera->Abort() == VWSDK::RESULT_SUCCESS )
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

void CVwGigEDemoGigEDiscoveryCameraDlg::OnBnClickedButtonCloseCamera()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pCamera )
	{
		// ERROR
		return;
	}

	if ( m_pCamera->CloseCamera() == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	OnOK();
}

void CVwGigEDemoGigEDiscoveryCameraDlg::MakeUnPackedBuffer()
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
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	UINT nHeight = 0;
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	
	m_pCamera->GetPixelFormat( &pixelFormat );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	m_pUnpackedImage = new BYTE[(size_t)(nWidth) * (size_t)(nHeight) * 3];
}

void CVwGigEDemoGigEDiscoveryCameraDlg::OnClose()
{
	if ( m_pCamera->CloseCamera() == VWSDK::RESULT_SUCCESS )
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close - Success"));		
	}
	else
	{
		GetDlgItem(IDC_STATIC_LOG)->SetWindowText(_T("Camera 1 : Close"));		
	}

	CDialog::OnClose();
}

void CVwGigEDemoGigEDiscoveryCameraDlg::Disconnect( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnect )
{
	CVwGigEDemoGigEDiscoveryCameraDlg* pDlg = (CVwGigEDemoGigEDiscoveryCameraDlg*)pObjectInfo->pUserPointer;

	CString strMSG;
	strMSG.Format( _T("Device connection has been lost.\nHeartBeatTimeOut : %d, TimeOutTryCount : %d"),
					tDisconnect.nCurrHeartBeatTimeOut,
					tDisconnect.nTimeOutTryCount );

	AfxMessageBox( strMSG );

	// Do not call the CloseCamera or OpenCamera function in this callback.
}

void CVwGigEDemoGigEDiscoveryCameraDlg::DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo )
{

	CVwGigEDemoGigEDiscoveryCameraDlg* pDlg = (CVwGigEDemoGigEDiscoveryCameraDlg*)pObjectInfo->pUserPointer;

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

		pDlg->GetCustomCommand(((VWSDK::VwCamera*)pObjectInfo->pVwCamera), "Width", &unWidth);
		pDlg->GetCustomCommand(((VWSDK::VwCamera*)pObjectInfo->pVwCamera), "Height", &unHeight);

		((VWSDK::VwCamera*)pObjectInfo->pVwCamera)->GetPixelFormat( &ePixelFormat );

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

		unBitCount = CVwResourceType::GetPixelBitCount( ePixelFormat );

		pDlg->m_pBmpInfo1->bmiHeader.biWidth       = unWidth;
		pDlg->m_pBmpInfo1->bmiHeader.biHeight      = nCalcHeight;
		pDlg->m_pBmpInfo1->bmiHeader.biBitCount    = unBitCount;
		pDlg->m_pBmpInfo1->bmiHeader.biCompression = BI_RGB;
		pDlg->m_pBmpInfo1->bmiHeader.biClrUsed     = unbiClrUsed;

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