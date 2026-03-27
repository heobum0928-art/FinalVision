#include "stdafx.h"
#include "VwGigE.Demo.SingleCam.Window.Advance.h"
#include "VwGigE.Demo.SingleCam.Window.AdvanceDlg.h"

#include "VwGigE.Api.h"
#include <list>
#include "VwResourceType.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define  COUNT_FPS 30


#define PIXEL_FORMAT_COUNT	49
TCHAR* STR_PIXEL_FORMAT[PIXEL_FORMAT_COUNT] = {
	L"Mono8",
	L"Mono8signed",
	L"Mono10",
	L"Mono10p",
	L"Mono10Packed",
	L"Mono12",
	L"Mono12p",
	L"Mono12Packed",
	L"Mono14",
	L"Mono16",
	L"BayerGR8",
	L"BayerRG8",
	L"BayerGB8",
	L"BayerBG8",
	L"BayerGR10",
	L"BayerRG10",
	L"BayerGB10",
	L"BayerBG10",
	L"BayerGR10Packed",
	L"BayerRG10Packed",
	L"BayerGR12",
	L"BayerRG12",
	L"BayerGB12",
	L"BayerBG12",
	L"BayerRG12Packed",
	L"BayerGR12Packed",
	L"RGB8",
	L"BGR8",
	L"RGB10",
	L"BGR10",
	L"RGB12",
	L"BGR12",
	L"YUV422_8_UYVY",
	L"YUV422_8",
	L"YUV42210Packed",
	L"YUV42212Packed",
	L"YUV411_8_UYYVYY",
	L"YUV41110Packed",
	L"YUV41112Packed",
	L"BGR10V1Packed",
	L"BGR10V2Packed",
	L"RGB12Packed",
	L"BGR12Packed",
	L"YUV444",
	L"PALInterlaced",
	L"NTSCInterlaced",
	L"YCbCr8",
	L"YCbCr8_CbYCr",
	L"YCbCr411_8"
};


VWSDK::PIXEL_FORMAT ARR_PIXEL_FORMAT[PIXEL_FORMAT_COUNT] = {
	VWSDK::PIXEL_FORMAT_MONO8,
	VWSDK::PIXEL_FORMAT_MONO8_SIGNED,
	VWSDK::PIXEL_FORMAT_MONO10,
	VWSDK::PIXEL_FORMAT_MONO10_P,
	VWSDK::PIXEL_FORMAT_MONO10_PACKED,
	VWSDK::PIXEL_FORMAT_MONO12,
	VWSDK::PIXEL_FORMAT_MONO12_P,
	VWSDK::PIXEL_FORMAT_MONO12_PACKED,
	VWSDK::PIXEL_FORMAT_MONO14,
	VWSDK::PIXEL_FORMAT_MONO16,
	VWSDK::PIXEL_FORMAT_BAYGR8,
	VWSDK::PIXEL_FORMAT_BAYRG8,
	VWSDK::PIXEL_FORMAT_BAYGB8,
	VWSDK::PIXEL_FORMAT_BAYBG8,
	VWSDK::PIXEL_FORMAT_BAYGR10,
	VWSDK::PIXEL_FORMAT_BAYRG10,
	VWSDK::PIXEL_FORMAT_BAYGB10,
	VWSDK::PIXEL_FORMAT_BAYBG10,
	VWSDK::PIXEL_FORMAT_BAYGR10_PACKED,
	VWSDK::PIXEL_FORMAT_BAYRG10_PACKED,
	VWSDK::PIXEL_FORMAT_BAYGR12,
	VWSDK::PIXEL_FORMAT_BAYRG12,
	VWSDK::PIXEL_FORMAT_BAYGB12,
	VWSDK::PIXEL_FORMAT_BAYBG12,
	VWSDK::PIXEL_FORMAT_BAYRG12_PACKED,
	VWSDK::PIXEL_FORMAT_BAYGR12_PACKED,
	VWSDK::PIXEL_FORMAT_RGB8,
	VWSDK::PIXEL_FORMAT_BGR8,
	VWSDK::PIXEL_FORMAT_RGB10,
	VWSDK::PIXEL_FORMAT_BGR10,
	VWSDK::PIXEL_FORMAT_RGB12,
	VWSDK::PIXEL_FORMAT_BGR12,
	VWSDK::PIXEL_FORMAT_YUV422_UYVY,
	VWSDK::PIXEL_FORMAT_YUV422_YUYV,
	VWSDK::PIXEL_FORMAT_YUV422_10_PACKED,
	VWSDK::PIXEL_FORMAT_YUV422_12_PACKED,
	VWSDK::PIXEL_FORMAT_YUV411,
	VWSDK::PIXEL_FORMAT_YUV411_10_PACKED,
	VWSDK::PIXEL_FORMAT_YUV411_12_PACKED,
	VWSDK::PIXEL_FORMAT_BGR10V1_PACKED,
	VWSDK::PIXEL_FORMAT_BGR10V2_PACKED,
	VWSDK::PIXEL_FORMAT_RGB12_PACKED,
	VWSDK::PIXEL_FORMAT_BGR12_PACKED,
	VWSDK::PIXEL_FORMAT_YUV444,
	VWSDK::PIXEL_FORMAT_PAL_INTERLACED,
	VWSDK::PIXEL_FORMAT_NTSC_INTERLACED,
	VWSDK::PIXEL_FORMAT_YCBCR8,
	VWSDK::PIXEL_FORMAT_YCBCR8_CBYCR,
	VWSDK::PIXEL_FORMAT_YCBCR411_8
};

using namespace std;

CVwGigEDemoSingleCamWindowAdvanceDlgC::CVwGigEDemoSingleCamWindowAdvanceDlgC(CWnd* pParent /*=NULL*/)
: CDialog(CVwGigEDemoSingleCamWindowAdvanceDlgC::IDD, pParent)
, m_pCamera ( NULL )
, m_curFPS( 0 )
, m_grabbedimagecount( 0 )
, m_pvwGigE ( NULL )
, m_pUnpackedImage ( NULL )
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

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

	m_pobjectInfo = new VWSDK::OBJECT_INFO;
}

CVwGigEDemoSingleCamWindowAdvanceDlgC::~CVwGigEDemoSingleCamWindowAdvanceDlgC()
{
	//Delete bitmap object
	if (m_pBmpInfo1)
	{
		delete m_pBmpInfo1;
		m_pBmpInfo1 = NULL;
	}

	if ( m_pvwGigE )
	{
		VWSDK::CloseVwGigE( m_pvwGigE );
		m_pvwGigE = NULL;
	}

	if( m_pUnpackedImage )
	{
		delete [] m_pUnpackedImage;
		m_pUnpackedImage = NULL;
	}

	if (m_pobjectInfo )
	{
		delete m_pobjectInfo;
		m_pobjectInfo = NULL;
	}
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMBO_PIXEL_FORMAT, m_cbxPixelFormat);
	DDX_Control(pDX, IDC_COMBO_PIXEL_SIZE, m_cbxPixelSize);
}

BEGIN_MESSAGE_MAP(CVwGigEDemoSingleCamWindowAdvanceDlgC, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_BUTTON_OPEN1, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonOpen1)
	ON_BN_CLICKED(IDC_BUTTON_PLAY1, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonPlay1)
	ON_BN_CLICKED(IDC_BUTTON_STOP1, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonStop1)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE1, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonClose1)
	ON_EN_CHANGE(IDC_EDIT_IMAGE_BUFFER, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnEnChangeEditImageBuffer)
	ON_BN_CLICKED(IDOK, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedExit)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BTN_SNAP, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedBtnSnap)
	ON_CBN_SELCHANGE(IDC_COMBO_PIXEL_FORMAT, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelFormat)
	ON_CBN_SELCHANGE(IDC_COMBO_PIXEL_SIZE, &CVwGigEDemoSingleCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelSize)
	ON_WM_CLOSE()
END_MESSAGE_MAP()


BOOL CVwGigEDemoSingleCamWindowAdvanceDlgC::OnInitDialog()
{
	CDialog::OnInitDialog();

	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	
	SetIcon(m_hIcon, TRUE);
	SetIcon(m_hIcon, FALSE);

	//Get display handle : 1
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

	//Set Image buffer number
	GetDlgItem(IDC_EDIT_IMAGE_BUFFER)->SetWindowText(_T("2"));
	m_imagebuffernumber = 2;

	// Set default frames for snap.
	GetDlgItem( IDC_EDIT_FRAMES )->SetWindowText( _T("10") );

	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_OPEN1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_STOP1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( FALSE );

	if ( VWSDK::RESULT_SUCCESS != VWSDK::OpenVwGigE( &m_pvwGigE ) )
	{
		AfxMessageBox( _T("Open failed.") );
	}

	VWSDK::VwUserLogging(m_pvwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	return TRUE;
}

VWSDK::RESULT CVwGigEDemoSingleCamWindowAdvanceDlgC::GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
{
	VWSDK::RESULT eRet = VWSDK::RESULT_ERROR;

	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	eRet = CameraGetCustomCommand(hCamera, cpFeatureName, chResult, &szResult, eCmdType);
	if (eRet == VWSDK::RESULT_SUCCESS){
		if (0 == strcmp(cpFeatureName, "PixelSize")
			&& VWSDK::GET_CUSTOM_COMMAND_VALUE == eCmdType){
			// Bpp8 Bpp10 Bpp12 ...
			CString strTmp(chResult);
			*unValue = _ttoi(strTmp.Mid(3));
		}
		else
			*unValue = atoi(chResult);
	}

	return eRet;
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnSysCommand(UINT nID, LPARAM lParam)
{
	CDialog::OnSysCommand(nID, lParam);
}


void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this);

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

	
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}


HCURSOR CVwGigEDemoSingleCamWindowAdvanceDlgC::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonOpen1()
{
	CWaitCursor oWaitCursor;

	//VwCamera*	m_pCamera;
	VWSDK::RESULT	result = VWSDK::RESULT_ERROR;

	result = VWSDK::VwOpenCameraByIndex( m_pvwGigE, 0, &m_pCamera, m_imagebuffernumber, 0, 0, 0, m_pobjectInfo, DrawImage );
	if(result != VWSDK::RESULT_SUCCESS)
	{
		switch(result)
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
				AfxMessageBox( _T("ERROR : RESULT_ERROR_BUFFER_TOO_SMALL was returned") );
				AfxMessageBox(_T("CHECK : Check system resources"));
			}
			break;
		case VWSDK::RESULT_ERROR_MEMORY_ALLOCATION:
			{
				AfxMessageBox( _T("ERROR : RESULT_ERROR_MEMORY_ALLOCATION was returned") );
				AfxMessageBox(_T("CHECK : Check system resources"));
			}
			break;
		}
		return ;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	m_pobjectInfo->pUserPointer = this;
	m_pobjectInfo->pVwCamera = m_pCamera;

	//Get image width,height 
	UINT nWidth = 0;
	UINT nHeight = 0;
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	
	list<VWSDK::PIXEL_FORMAT> plstPixelFormat;
	// BOTH_POSSIBLE
#if 1
	UINT nLineupNum = 0;
	GetCustomCommand(m_pCamera, "PixelFormat", &nLineupNum, VWSDK::GET_CUSTOM_COMMAND_NUM);
	
	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	for (int i = 0; i<nLineupNum; i++){
		szResult = sizeof(chResult);
		ZeroMemory(chResult, szResult);
		VWSDK::CameraGetCustomCommand(m_pCamera, "PixelFormat", chResult, &szResult, VWSDK::GET_CUSTOM_COMMAND_INDEX + i);
		CString strPixelType(chResult);
		VWSDK::PIXEL_FORMAT pixelFormatItem = VWSDK::PIXEL_FORMAT_MONO8;
		for (int j = 0; j < PIXEL_FORMAT_COUNT; j++)
		{
			if (STR_PIXEL_FORMAT[j] == strPixelType)
			{
				pixelFormatItem = ARR_PIXEL_FORMAT[j];
				break;
			}
		}
		if (VWSDK::PIXEL_FORMAT_MONO16 == pixelFormatItem)
			continue;

		plstPixelFormat.push_back(pixelFormatItem);
	}
#else
	// CameraGetPixelFormatLine
	{
		char chResult[512] = { 0, };
		size_t szResult = sizeof(chResult);
		VWSDK::CameraGetEnumPropertyItems(m_pCamera, "PixelFormat", chResult, &szResult);

		CString strResult(chResult);
		while (strResult.Find(_T("(")) > -1){
			int nStart = strResult.Find(_T("(")) + 1;
			int nCount = strResult.Find(_T(")")) - nStart;

			CString strTmp = strResult.Mid(nStart, nCount);
			VWSDK::PIXEL_FORMAT ePixelFormat = static_cast<VWSDK::PIXEL_FORMAT>(_ttoi(strTmp));
			plstPixelFormat.push_back(ePixelFormat);

			if (VWSDK::PIXEL_FORMAT_MONO16 == ePixelFormat)
				plstPixelFormat.pop_back();

			if (strResult.Find(_T(",")) == -1)
				break;

			strResult = strResult.Mid(strResult.Find(_T(",")) + 1);
		}
	}
#endif	
	MakeUnPackedBuffer();

	//
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	int nPixelFormatIndex = 0;
	int nCount = 0;
	m_cbxPixelFormat.ResetContent();
	

	for( list<VWSDK::PIXEL_FORMAT>::iterator itList = plstPixelFormat.begin();
		itList != plstPixelFormat.end();
		itList++ )
	{
		CString strTemp;
		strTemp = GetPixelFormatFromEnum( (*itList) );
		m_cbxPixelFormat.AddString( strTemp );
	}

	CameraGetPixelFormat( m_pCamera, &pixelFormat );
	

	m_pBmpInfo1->bmiHeader.biWidth	= nWidth;
	m_pBmpInfo1->bmiHeader.biHeight	= (-1 * nHeight);
	m_pBmpInfo1->bmiHeader.biBitCount	= 24;
	m_pBmpInfo1->bmiHeader.biClrUsed	= 0;

	m_pBmpInfo1->bmiHeader.biBitCount = CVwResourceType::GetPixelBitCount( pixelFormat );
	
	if( m_pBmpInfo1->bmiHeader.biBitCount == 16 || m_pBmpInfo1->bmiHeader.biBitCount == 32 )
	{
		m_pBmpInfo1->bmiHeader.biCompression	= BI_BITFIELDS;
		m_pBmpInfo1->bmiHeader.biClrUsed		= 3;	
	}
	else 
	{
		m_pBmpInfo1->bmiHeader.biCompression	= BI_RGB;

		if( m_pBmpInfo1->bmiHeader.biBitCount == 8 )
			m_pBmpInfo1->bmiHeader.biClrUsed = 256;
	}

	// Get PixelSize
	UINT unPixelSize = 0;
	m_cbxPixelSize.ResetContent();
	// BOTH_POSSIBLE
#if 1
	nLineupNum = 0;
	GetCustomCommand(m_pCamera, "PixelSize", &nLineupNum, VWSDK::GET_CUSTOM_COMMAND_NUM);
	
	//char chResult[100] = { 0, };
	//size_t szResult = sizeof(chResult);

	for (int i = 0; i<nLineupNum; i++){
		szResult = sizeof(chResult);
		ZeroMemory(chResult, szResult);
		VWSDK::CameraGetCustomCommand(m_pCamera, "PixelSize", chResult, &szResult, VWSDK::GET_CUSTOM_COMMAND_INDEX + i);
		CString strTmp(chResult);
		m_cbxPixelSize.AddString(strTmp);
	}
#else
	// CameraGetPixelSizeLine
	char chResult[512] = { 0, };
	size_t szResult = sizeof(chResult);
	VWSDK::CameraGetEnumPropertyItems(m_pCamera, "PixelSize", chResult, &szResult);

	CString strResult(chResult);
	while (strResult.Find(_T("(")) > -1){
		int nStart = strResult.Find(_T("(")) + 1;
		int nCount = strResult.Find(_T(")")) - nStart;

		CString strTmp = strResult.Mid(nStart, nCount);
		strTmp.Format(_T("Bpp%d"), strTmp);
		m_cbxPixelSize.AddString(strTmp);

		if (strResult.Find(_T(",")) == -1)
			break;

		strResult = strResult.Mid(strResult.Find(_T(",")) + 1);
	}
#endif
	
	
	unPixelSize = 0;
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);

	//Get several values from camera using VWGigESerial class
	int time = 0;
	CString strtemp;

	// Set resolution info.
	UINT tempwidth = 0;
	UINT tempheight = 0;
	GetCustomCommand(m_pCamera, "Width", &tempwidth);
	GetCustomCommand(m_pCamera, "Height", &tempheight);

	SetUIResolutionInfo( tempwidth, tempheight, pixelFormat, unPixelSize );

	// Get device information
	CString strVendorName;
	CString strModelName;
	CString strVersion;
	CString strID;

	GetDeviceInfo( 0/*DeviceIndex*/, strVendorName, strModelName, strVersion, strID );

	//
	GetDlgItem( IDC_STATIC_VENDORNAME )->SetWindowText( strVendorName );
	GetDlgItem( IDC_STATIC_MODELNAME )->SetWindowText( strModelName );
	GetDlgItem( IDC_STATIC_VERSION )->SetWindowText( strVersion );
	GetDlgItem( IDC_STATIC_ID )->SetWindowText( strID );

	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_OPEN1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_STOP1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( TRUE );
	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( TRUE );
	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( TRUE );
}


void CVwGigEDemoSingleCamWindowAdvanceDlgC::DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo )
{

	CVwGigEDemoSingleCamWindowAdvanceDlgC* pDlg = (CVwGigEDemoSingleCamWindowAdvanceDlgC*)pObjectInfo->pUserPointer;


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


void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonPlay1()
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

	UINT nWidth = 0;
	UINT nHeight = 0;
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);

	// Set Width, Height
	CString strWidth;
	GetDlgItem( IDC_EDIT_WIDTH )->GetWindowText( strWidth );
	UINT nInputWidth = _ttoi( strWidth );
	if ( FALSE == SetWidthCamera( nInputWidth ) )
	{
		// Rollback
		UINT nWidth = 0;
		GetCustomCommand(m_pCamera, "Width", &nWidth);
		CString strWidth;
		strWidth.Format( _T("%d"), nWidth );
		GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText( strWidth );
	}

	// Set Width, Height
	CString strHeight;
	GetDlgItem( IDC_EDIT_HEIGHT )->GetWindowText( strHeight );
	UINT nInputHeight = _ttoi( strHeight );
	if ( FALSE == SetHeightCamera( nInputHeight ) )
	{
		// Rollback
		UINT nHeight = 0;
		GetCustomCommand(m_pCamera, "Height", &nHeight);
		CString strHeight;
		strHeight.Format( _T("%d"), nHeight );
		GetDlgItem( IDC_EDIT_HEIGHT )->SetWindowText( strHeight );
	}

	GetCustomCommand(m_pCamera, "Width", &nInputWidth);
	GetCustomCommand(m_pCamera, "Height", &nInputHeight);
	{
		// CameraSetReadoutMode
		char chValue[10] = { 0, };
		// This property values may be different for each GigE camera.
		// READOUT_NORMAL			= 0
		// READOUT_AOI				= 1
		// READOUT_BINNING			= 2
		// READOUT_HORIZONTALSTART	= 3
		// READOUT_HORIZONTALEND	= 4
		// READOUT_VERTICALSTART	= 5
		// READOUT_VERTICALEND		= 6
		// READOUT_BINNINGFATOR		= 7
		sprintf(chValue, "%d", 1); // READOUT_AOI = 1
		VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "ReadoutMode", chValue);
	}
	{
		// CameraSetHorizontalStart/End
		VWSDK::CameraSetCustomCommand(m_pCamera, "HorizontalStart", "0");
		char chValue[100] = { 0, };
		sprintf(chValue, "%d", nInputWidth - 1);
		VWSDK::CameraSetCustomCommand(m_pCamera, "HorizontalEnd", chValue);
		// CameraSetVerticalStart/End
		VWSDK::CameraSetCustomCommand(m_pCamera, "VerticalStart", "0");
		ZeroMemory(chValue, sizeof(chValue));
		sprintf(chValue, "%d", nInputHeight - 1);
		VWSDK::CameraSetCustomCommand(m_pCamera, "VerticalEnd", chValue);
	}
	

	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );

	if ( nInputWidth != nWidth ||
		 nInputHeight != nHeight )
	{
		if ( VWSDK::RESULT_SUCCESS != VWSDK::CameraChangeBufferFormat( m_pCamera, m_imagebuffernumber, nInputWidth, nInputHeight, pixelFormat ) )
		{
			AfxMessageBox( _T("Can't change the camera buffer.") );
			return;
		}

		MakeUnPackedBuffer();
	}

	

	if( VWSDK::CameraGrab( m_pCamera ) == VWSDK::RESULT_SUCCESS )
	{
		
	}
	else
	{
		return;
	}

	SetTimer(0,1000,NULL);

	// Disable buttons
	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( FALSE );

	UINT unPixelSize = 0;
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat(m_pCamera, &pixelFormat);
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat, unPixelSize );

}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonSearchDevice()
{

}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonStop1()
{
	CWaitCursor oWaitCursor;

	if( NULL == m_pCamera )
	{
		return;
	}

	VWSDK::CameraAbort( m_pCamera );

	m_grabbedimagecount = 0;
	m_imageTimeStamps.clear();

	KillTimer(0);

	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( TRUE );
	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( TRUE );
	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( TRUE );

	
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedButtonClose1()
{
	CWaitCursor oWaitCursor;

	if ( NULL != m_pCamera )
	{		
		if ( VWSDK::CameraClose( m_pCamera ) == VWSDK::RESULT_SUCCESS )
		{
			// Success
		}
		else
		{
			// Fail
		}
		m_pCamera = NULL;
	}
	
	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_OPEN1 )->EnableWindow( TRUE );
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->EnableWindow( TRUE );
	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( FALSE );
	GetDlgItem( IDC_BUTTON_STOP1 )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( FALSE );
	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( FALSE );
	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( FALSE );

	if( m_pUnpackedImage != NULL )
	{
		delete [] m_pUnpackedImage;
		m_pUnpackedImage = NULL;
	}

}
void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnEnChangeEditImageBuffer()
{
	CString strImageBuffer;
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->GetWindowText( strImageBuffer );

	int nImageBuffer = _ttoi( strImageBuffer );

	if ( nImageBuffer < 1 )
	{
		AfxMessageBox( _T("Must be greater than 0.") );
		return;
	}


	m_imagebuffernumber = nImageBuffer;
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedExit()
{
	if( NULL != m_pCamera )
	{
		AfxMessageBox(_T("First, Close device\n"));
		return;
	}

	if ( NULL != m_pvwGigE )
	{
		VWSDK::CloseVwGigE( m_pvwGigE );
		m_pvwGigE = NULL;
	}
	OnOK();
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnTimer(UINT_PTR nIDEvent)
{
	CString tempstr;

	if( nIDEvent == 0 )
	{
		if (m_curFPS)
		{
			tempstr.Format(_T("FPS : %3.2f"), m_curFPS);

			GetDlgItem(IDC_STATIC_FPS)->SetWindowText(tempstr);
		}
	}	


	CDialog::OnTimer(nIDEvent);
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnBnClickedBtnSnap()
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

	// Set Width, Height
	CString strWidth;
	GetDlgItem( IDC_EDIT_WIDTH )->GetWindowText( strWidth );
	int nInputWidth = _ttoi( strWidth );
	SetWidthCamera( nInputWidth );

	// Set Width, Height
	CString strHeight;
	GetDlgItem( IDC_EDIT_HEIGHT )->GetWindowText( strHeight );
	int nInputHeight = _ttoi( strHeight );
	SetHeightCamera( nInputHeight );

	CString strFrame;
	GetDlgItem( IDC_EDIT_FRAMES )->GetWindowText( strFrame );
	int nFrame = _ttoi( strFrame );

	//Exception
	if ( nFrame < 1 )
	{
		AfxMessageBox( _T("Must be greater than 0.") );
		nFrame = 1;
	}
	else if ( nFrame > 255 )
	{
		AfxMessageBox( _T("Must be less than 256.") );
		nFrame = 255;
	}

	strFrame.Format( _T("%d"), nFrame );
	GetDlgItem( IDC_EDIT_FRAMES )->SetWindowText( strFrame );

	if ( VWSDK::CameraSnap( m_pCamera, nFrame ) == VWSDK::RESULT_SUCCESS )
	{
		
	}
	else
	{
		
	}

	// Update resolution info.
	UINT unPixelSize = 0;
	UINT nWidth = 0;
	UINT nHeight = 0;
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);

	SetUIResolutionInfo( nWidth, nHeight, pixelFormat, unPixelSize );

}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::GetDeviceInfo( int nIndex, CString& strVenderName, CString& strModelName, CString& strDeviceVersion, CString& strDeviceID )
{
	if ( NULL == m_pCamera )
	{
		return;
	}

	const int STR_SIZE	=	256;

	char szVendorName[ STR_SIZE ];
	size_t cbVendor = sizeof( szVendorName );

	char szModelName[ STR_SIZE ];
	size_t cbModel = sizeof( szModelName );

	char szVersion[ STR_SIZE ];
	size_t cbVersion = sizeof ( szVersion );

	char szID[ STR_SIZE ];
	size_t cbID = sizeof ( szID );

	
	if ( VWSDK::CameraGetDeviceVendorName( m_pCamera, nIndex, szVendorName, &cbVendor ) == VWSDK::RESULT_SUCCESS )
	{
		strVenderName = szVendorName;
	}

	if ( VWSDK::CameraGetDeviceModelName( m_pCamera, nIndex, szModelName, &cbModel ) == VWSDK::RESULT_SUCCESS )
	{
		strModelName = szModelName;
	}

	if ( VWSDK::CameraGetDeviceVersion( m_pCamera, nIndex, szVersion, &cbVersion ) == VWSDK::RESULT_SUCCESS )
	{
		strDeviceVersion = szVersion;
	}

	if ( VWSDK::CameraGetDeviceID( m_pCamera, nIndex, szID, &cbID ) == VWSDK::RESULT_SUCCESS )
	{
		strDeviceID = szID;
	}
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelFormat()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pCamera)	
	{
		return;
	}

	int nCurSel = m_cbxPixelFormat.GetCurSel();

	CString strPixelType = GetPixelTypeStr( nCurSel );
#if 0
	VWSDK::PIXEL_FORMAT pixelFormatItem = VWSDK::PIXEL_FORMAT_MONO8;
	for ( int i = 0; i < PIXEL_FORMAT_COUNT; i ++ )
	{
		if ( STR_PIXEL_FORMAT[ i ] == strPixelType )
		{
			pixelFormatItem = ARR_PIXEL_FORMAT[ i ];
			break;
		}
	}
#endif
	// CameraSetPixelFormat
	char chValue[100] = { 0, };
	sprintf(chValue, "%S", strPixelType);
	VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "PixelFormat", chValue);
	
	switch ( ret )
	{
	case VWSDK::RESULT_SUCCESS:
		break;
	case VWSDK::RESULT_ERROR_INVALID_PARAMETER:
		AfxMessageBox( _T("Invalid pixelformat.") );
		return;
		break;

	default:
		AfxMessageBox( _T("Can't change the pixelformat.") );
		return;
		break;
	}

	//
	UINT nWidth = 0;
	UINT nHeight = 0;
	VWSDK::PIXEL_FORMAT pixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	GetCustomCommand(m_pCamera, "Width", &nWidth);
	GetCustomCommand(m_pCamera, "Height", &nHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &pixelFormat );

	if ( VWSDK::RESULT_SUCCESS != VWSDK::CameraChangeBufferFormat( m_pCamera, m_imagebuffernumber, nWidth, nHeight, pixelFormat ) )
	{
		AfxMessageBox( _T("Can't change the camera buffer.") );
		return;
	}

	MakeUnPackedBuffer();

	// Update Pixel Size
	UINT nPixelSize = 0;
	GetCustomCommand(m_pCamera, "PixelSize", &nPixelSize);
	int nIndex = GetPixelSizeIndex( nPixelSize );

	m_cbxPixelSize.SetCurSel( nIndex );

}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelSize()
{
	if( m_pCamera == NULL )	
		return;

	CString strPixelSize;
	CString strTmp;

	int nCurSel = m_cbxPixelSize.GetCurSel();
	m_cbxPixelSize.GetLBText( nCurSel, strPixelSize );
	strTmp = strPixelSize.Mid( (int)strlen("Bpp"), strPixelSize.GetLength() );

	nCurSel = atoi( CT2A(strTmp) );

	// CameraSetPixelSize
	char chValue[100] = { 0, };
	sprintf(chValue, "%d", nCurSel);
	VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "PixelSize", chValue);

	UINT nIndex = 0;
	switch( ret )
	{
	case VWSDK::RESULT_SUCCESS:
		break;
	case VWSDK::RESULT_ERROR_INVALID_PARAMETER:
		AfxMessageBox( _T("Invalid pixelsize.") );
		GetCustomCommand(m_pCamera, "PixelSize", &nIndex);
		m_cbxPixelSize.SetCurSel( GetPixelSizeIndex( nIndex ) );
		return;
	default:
		AfxMessageBox( _T("Can't change the pixelsize.") );
		GetCustomCommand(m_pCamera, "PixelSize", &nIndex);
		m_cbxPixelSize.SetCurSel( GetPixelSizeIndex( nIndex ) );
		return;
	}
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat, IN UINT unPixelSize )
{
	CString strTmp;
	strTmp.Format(_T("Width : %d"),tempwidth);
	GetDlgItem(IDC_STATIC_WIDTH)->SetWindowText(strTmp);

	CString strWidth;
	strWidth.Format( _T("%d"), tempwidth );
	GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText( strWidth );



	strTmp.Format(_T("Height : %d"),tempheight);
	GetDlgItem(IDC_STATIC_HEIGHT)->SetWindowText(strTmp);

	CString strHeight;
	strHeight.Format( _T("%d"), tempheight );
	GetDlgItem( IDC_EDIT_HEIGHT )->SetWindowText( strHeight );

	int nTmp;
	strTmp = CVwResourceType::GetPixelFormatName( ePixelFormat );
	strTmp.Remove( '\n' );
	nTmp = GetPixelTypeIndex( strTmp );
	strTmp = _T("Pixel Format : ") + strTmp + _T("\n");

	if( nTmp != -1 )
		m_cbxPixelFormat.SetCurSel( nTmp );

	GetDlgItem(IDC_STATIC_PIXEL_DEPTH)->SetWindowText(strTmp);

	int nIndex = GetPixelSizeIndex( unPixelSize );

	m_cbxPixelSize.SetCurSel( nIndex );
}

int CVwGigEDemoSingleCamWindowAdvanceDlgC::GetPixelTypeIndex( IN CString strType )
{
	for ( int i = 0; i < m_cbxPixelFormat.GetCount(); i ++ ) 
	{
		CString strTemp;
		m_cbxPixelFormat.GetLBText( i, strTemp );

		if ( strTemp.Compare( strType ) == 0 )
		{
			return i;
		}
	}

	return -1;
}


CString CVwGigEDemoSingleCamWindowAdvanceDlgC::GetPixelTypeStr( IN int nIndex )
{
	CString strTemp;
	m_cbxPixelFormat.GetLBText( nIndex, strTemp );
	
	return strTemp;
}

void CVwGigEDemoSingleCamWindowAdvanceDlgC::MakeUnPackedBuffer()
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


	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	switch( pixelFormat )
	{
		case VWSDK::PIXEL_FORMAT_MONO10_PACKED:
		case VWSDK::PIXEL_FORMAT_MONO12_PACKED:
		case VWSDK::PIXEL_FORMAT_BAYGR10_PACKED:
			m_pUnpackedImage = new BYTE[nWidth*nHeight*6];
			break;
		case VWSDK::PIXEL_FORMAT_BAYGR8:
		case VWSDK::PIXEL_FORMAT_BAYRG8:
		case VWSDK::PIXEL_FORMAT_MONO16:
			m_pUnpackedImage = new BYTE[nWidth*nHeight*3];
			break;
		default:
			m_pUnpackedImage = new BYTE[nWidth*nHeight*3];
			break;
	}
}

CString CVwGigEDemoSingleCamWindowAdvanceDlgC::GetPixelFormatFromEnum( VWSDK::PIXEL_FORMAT pixelFormat )
{
	for ( int i = 0; i < PIXEL_FORMAT_COUNT; i ++ )
	{
		if ( ARR_PIXEL_FORMAT[ i ] == pixelFormat )
		{
			return CString(STR_PIXEL_FORMAT[ i ]);
		}
	}

	return _T("");
}

BOOL CVwGigEDemoSingleCamWindowAdvanceDlgC::SetWidthCamera( int nWidth )
{
	UINT nCurrWidth = 0;
	if (VWSDK::RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Width", &nCurrWidth))
	{
		// ERROR
		return FALSE;
	}

	if ( nWidth != nCurrWidth )
	{
		{
			// CameraSetWidth
			char chValue[100] = { 0, };
			sprintf(chValue, "%d", nWidth);
			VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "Width", chValue);

			if (VWSDK::RESULT_ERROR_VWCAMERA_IMAGE_NOT4DIVIDE == ret)
			{
				AfxMessageBox(_T("Width must be a multiple of 4!"));

				return FALSE;
			}
		}
	}	

	return TRUE;
}

BOOL CVwGigEDemoSingleCamWindowAdvanceDlgC::SetHeightCamera( int nHeight )
{
	UINT nCurrHeight = 0;
	if (VWSDK::RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Height", &nCurrHeight))
	{
		// ERROR
		return FALSE;
	}

	if ( nHeight != nCurrHeight )
	{
		// CameraSetHeight
		char chValue[100] = { 0, };
		sprintf(chValue, "%d", nHeight);
		VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "Height", chValue);
		
		if ( VWSDK::RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE == ret )
		{
			AfxMessageBox( _T("Height must be a multiple of 2!"));
			return FALSE;
		}
	}	

	return TRUE;
}
void CVwGigEDemoSingleCamWindowAdvanceDlgC::OnClose()
{
	if( NULL != m_pCamera )
	{
		AfxMessageBox(_T("First, Close device\n"));
		return;
	}

	if ( NULL != m_pvwGigE )
	{
		VWSDK::CloseVwGigE( m_pvwGigE );
		m_pvwGigE = NULL;		
	}

	CDialog::OnClose();
}

int CVwGigEDemoSingleCamWindowAdvanceDlgC::GetPixelSizeIndex( int nPixelSize )
{
	// Pixel Size
	CString strTmp;
	strTmp.Format( _T("Bpp%d"), nPixelSize );

	for( int i = 0; i < m_cbxPixelSize.GetCount(); i++ ) 
	{
		CString strPixelSize;

		m_cbxPixelSize.GetLBText( i, strPixelSize );

		if( !strTmp.Compare( strPixelSize ) )
		{
			return i;
		}
	}
}

BOOL CVwGigEDemoSingleCamWindowAdvanceDlgC::PreTranslateMessage(MSG* pMsg)
{
	switch ( pMsg->message )
	{
	case WM_KEYDOWN:
		{
			switch ( pMsg->wParam )
			{
			case VK_RETURN:
				{
					if ( GetDlgItem( IDC_EDIT_WIDTH ) == GetFocus() ||
						GetDlgItem( IDC_EDIT_HEIGHT ) == GetFocus() ||
						GetDlgItem( IDC_EDIT_FRAMES ) == GetFocus() ||
						GetDlgItem( IDC_EDIT_IMAGE_BUFFER ) == GetFocus() )
					{
						GetDlgItem( IDC_BUTTON_PLAY1 )->SetFocus();
						pMsg->wParam = NULL;
					}
				}
				break;
			}
		}
		break;
	}

	return CDialog::PreTranslateMessage(pMsg);
}
