#include "stdafx.h"
#include "VwGigE.Demo.MultiCam.Window.Advance.h"
#include "VwGigE.Demo.MultiCam.Window.AdvanceDlg.h"

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

CVwGigEDemoMultiCamWindowAdvanceDlgC::CVwGigEDemoMultiCamWindowAdvanceDlgC(CWnd* pParent /*=NULL*/)
: CDialog(CVwGigEDemoMultiCamWindowAdvanceDlgC::IDD, pParent)
, m_pCamera ( NULL )
, m_grabbedimagecount( 0 )
, m_pvwGigE ( NULL )
, m_nWidth ( 0 )
, m_nHeight ( 0 )
, m_radioDevice( 0 )
, m_radioDeviceOld( 0 )
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);



	for ( int i = 0; i < DF_DEVICE_COUNT; i ++ )
	{
		m_curFPS[ i ] = 0;

		m_pobjectInfo[ i ] = NULL;

		m_pUnpackedImage[ i ] = NULL;

		m_lstCamera[ i ] = NULL;

		//Camera 1 Bitmap structure
		m_pBmpInfo[ i ] = (BITMAPINFO*)new BYTE[(sizeof(BITMAPINFOHEADER)+256*sizeof(RGBQUAD))];
		ZeroMemory(m_pBmpInfo[ i ], sizeof(BITMAPINFOHEADER));

		m_pBmpInfo[i]->bmiHeader.biSize		 = sizeof( BITMAPINFOHEADER );
		m_pBmpInfo[i]->bmiHeader.biPlanes		 = 1;	
		m_pBmpInfo[i]->bmiHeader.biCompression	 = BI_RGB;
		m_pBmpInfo[i]->bmiHeader.biClrImportant = 0;
		m_pBmpInfo[i]->bmiHeader.biBitCount	 = 8;
		m_pBmpInfo[i]->bmiHeader.biClrUsed		 = 256;

		for( UINT j = 0; j < m_pBmpInfo[i]->bmiHeader.biClrUsed; j++ )
		{
			m_pBmpInfo[i]->bmiColors[j].rgbBlue     = j;
			m_pBmpInfo[i]->bmiColors[j].rgbGreen    = j;
			m_pBmpInfo[i]->bmiColors[j].rgbRed      = j;
			m_pBmpInfo[i]->bmiColors[j].rgbReserved = 0;
		}

		m_liLastDisplayTime[ i ].QuadPart = 0;
		m_imageTimeStamps[ i ].clear();
		QueryPerformanceFrequency( &m_liFreq );
		m_nMinInterFrameTime = DWORD(m_liFreq.QuadPart / 30);

		m_pobjectInfo[ i ] = new VWSDK::OBJECT_INFO;

	}
}

CVwGigEDemoMultiCamWindowAdvanceDlgC::~CVwGigEDemoMultiCamWindowAdvanceDlgC()
{
	for ( int i = 0 ; i < DF_DEVICE_COUNT; i ++ )
	{
		if( m_pobjectInfo[ i ] )
		{
			delete m_pobjectInfo[ i ];
			m_pobjectInfo[ i ] = NULL;
		}
	
		if( m_pUnpackedImage[i] )
		{
			delete [] m_pUnpackedImage[i];
			m_pUnpackedImage[i] = NULL;
		}

		if( m_pBmpInfo[ i ]  )
		{
			delete [] m_pBmpInfo[ i ];
			m_pBmpInfo[ i ] = NULL;
		}
	}

	if( m_pvwGigE )
	{
		VWSDK::CloseVwGigE( m_pvwGigE );
		m_pvwGigE = NULL;
	}
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_COMBO_PIXEL_FORMAT, m_cbxPixelFormat);
	DDX_Control(pDX, IDC_COMBO_PIXEL_SIZE, m_cbxPixelSize);
	DDX_Radio(pDX, IDC_RADIO_DEVICE0, (int&)m_radioDevice);
}

BEGIN_MESSAGE_MAP(CVwGigEDemoMultiCamWindowAdvanceDlgC, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()

	ON_BN_CLICKED(IDC_BUTTON_OPEN1, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonOpen1)
	ON_BN_CLICKED(IDC_BUTTON_PLAY1, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonPlay1)
	ON_BN_CLICKED(IDC_BUTTON_STOP1, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonStop1)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE1, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonClose1)
	ON_BN_CLICKED(IDOK, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedExit)
	ON_BN_CLICKED(IDC_BTN_SNAP, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedBtnSnap)

	ON_EN_CHANGE(IDC_EDIT_IMAGE_BUFFER, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnEnChangeEditImageBuffer)

	ON_CBN_SELCHANGE(IDC_COMBO_PIXEL_FORMAT, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelFormat)
	ON_CBN_SELCHANGE(IDC_COMBO_PIXEL_SIZE, &CVwGigEDemoMultiCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelSize)
	ON_CONTROL_RANGE(BN_CLICKED, IDC_RADIO_DEVICE0, IDC_RADIO_DEVICE3, RadioCtrl)
	ON_WM_CLOSE()
END_MESSAGE_MAP()


BOOL CVwGigEDemoMultiCamWindowAdvanceDlgC::OnInitDialog()
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
	m_imagecontrolWidth  = cr.Width() / 2;
	m_imagecontrolHeight = cr.Height() / 2;

	//Set Image buffer number
	m_imagebuffernumber = 2;
	GetDlgItem(IDC_EDIT_IMAGE_BUFFER)->SetWindowText(_T("2"));

	for( int i = 0; i < DF_DEVICE_COUNT; i ++ )
	{
		m_deviceState[i].m_nBuffer = m_imagebuffernumber;
		m_deviceState[i].m_bOpen = TRUE;
		m_deviceState[i].m_bClose = FALSE;
		m_deviceState[i].m_bImageBuffer = TRUE;
		m_deviceState[i].m_nSnapBuffer = 10;
		m_deviceState[i].m_bSnapBuffer = FALSE;
		m_deviceState[i].m_bGrab = FALSE;
		m_deviceState[i].m_bSnap = FALSE;
		m_deviceState[i].m_bAbort = FALSE;
		m_deviceState[i].m_bPixelFormat= FALSE;
		m_deviceState[i].m_bPixelSize = FALSE;
		m_deviceState[i].m_bWidth = FALSE;
		m_deviceState[i].m_bHeight = FALSE;

	}

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

	VWSDK::OpenVwGigE( &m_pvwGigE );

	VWSDK::VwUserLogging(m_pvwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	return TRUE; 
}

VWSDK::RESULT CVwGigEDemoMultiCamWindowAdvanceDlgC::GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType)
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnSysCommand(UINT nID, LPARAM lParam)
{
	CDialog::OnSysCommand(nID, lParam);
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnPaint()
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

HCURSOR CVwGigEDemoMultiCamWindowAdvanceDlgC::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonOpen1()
{
	CWaitCursor oWaitCursor;

	VWSDK::RESULT	result = VWSDK::RESULT_ERROR;

	ImageCallbackFn* pImageCallbackFn = NULL;
	VWSDK::HCAMERA pCamera = NULL;
	switch ( m_radioDevice )
	{
	case 0:
		{
			pImageCallbackFn = GetImageEvent1;
		}
		break;
	case 1:
		{
			pImageCallbackFn = GetImageEvent2;
		}
		break;
	case 2:
		{
			pImageCallbackFn = GetImageEvent3;
		}
		break;
	case 3:
		{
			pImageCallbackFn = GetImageEvent4;
		}
		break;
	default:
		return;

	}
	result = VwOpenCameraByIndex( m_pvwGigE, m_radioDevice, &pCamera, m_imagebuffernumber, 0, 0, 0, m_pobjectInfo[ m_radioDevice ], pImageCallbackFn );

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
		return;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	m_pobjectInfo[ m_radioDevice ]->pUserPointer = this;
	m_pobjectInfo[ m_radioDevice ]->pVwCamera    = pCamera;

	m_lstCamera[ m_radioDevice ] = pCamera;
	m_pCamera = pCamera;

	//Get image width,height 
	UINT unWidth = 0;
	UINT unHeight = 0;
	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(pCamera, "Width", &unWidth);
	GetCustomCommand(pCamera, "Height", &unHeight);
	
	list<VWSDK::PIXEL_FORMAT> plstPixelFormat;
	// BOTH_POSSIBLE
#if 1
	UINT nLineupNum = 0;
	GetCustomCommand(pCamera, "PixelFormat",& nLineupNum, VWSDK::GET_CUSTOM_COMMAND_NUM);
	
	char chResult[100] = { 0, };
	size_t szResult = sizeof(chResult);

	for (int i = 0; i<nLineupNum; i++){
		szResult = sizeof(chResult);
		ZeroMemory(chResult, szResult);
		VWSDK::CameraGetCustomCommand(pCamera, "PixelFormat", chResult, &szResult, VWSDK::GET_CUSTOM_COMMAND_INDEX + i);
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
	char chResult[512] = { 0, };
	size_t szResult = sizeof(chResult);
	VWSDK::CameraGetEnumPropertyItems(pCamera, "PixelFormat", chResult, &szResult);

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
#endif
	MakeUnPackedBuffer();

	//
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	m_cbxPixelFormat.ResetContent();
	
	for( list<VWSDK::PIXEL_FORMAT>::iterator itList = plstPixelFormat.begin(); itList != plstPixelFormat.end(); itList++ )
	{
		CString strTmp;
		strTmp = GetPixelFormatFromEnum( (*itList) );
		m_cbxPixelFormat.AddString( strTmp );
	}

	CameraGetPixelFormat( pCamera, &ePixelFormat );

	m_pBmpInfo[m_radioDevice]->bmiHeader.biWidth	= unWidth;
	m_pBmpInfo[m_radioDevice]->bmiHeader.biHeight	= (-1 * unHeight);
	m_pBmpInfo[m_radioDevice]->bmiHeader.biBitCount = 24;
	m_pBmpInfo[m_radioDevice]->bmiHeader.biClrUsed	 = 0;

	m_pBmpInfo[m_radioDevice]->bmiHeader.biBitCount = CVwResourceType::GetPixelBitCount( ePixelFormat );
	
	if( m_pBmpInfo[m_radioDevice]->bmiHeader.biBitCount == 16 || m_pBmpInfo[m_radioDevice]->bmiHeader.biBitCount == 32 )
	{
		m_pBmpInfo[m_radioDevice]->bmiHeader.biCompression	= BI_BITFIELDS;
		m_pBmpInfo[m_radioDevice]->bmiHeader.biClrUsed		= 3;	
	}
	else 
	{
		m_pBmpInfo[m_radioDevice]->bmiHeader.biCompression	= BI_RGB;

		if( m_pBmpInfo[m_radioDevice]->bmiHeader.biBitCount == 8 )
			m_pBmpInfo[m_radioDevice]->bmiHeader.biClrUsed = 256;
	}

	// Get PixelSize
	UINT unPixelSize = 0;
	m_cbxPixelSize.ResetContent();
	// BOTH_POSSIBLE
#if 1	
	nLineupNum = 0;
	GetCustomCommand(pCamera, "PixelSize", &nLineupNum, VWSDK::GET_CUSTOM_COMMAND_NUM);
	
//	char chResult[100] = { 0, };
//	size_t szResult = sizeof(chResult);

	for (int i = 0; i<nLineupNum; i++){
		szResult = sizeof(chResult);
		ZeroMemory(chResult, szResult);
		VWSDK::CameraGetCustomCommand(pCamera, "PixelSize", chResult, &szResult, VWSDK::GET_CUSTOM_COMMAND_INDEX + i);
		CString strTmp(chResult);
		m_cbxPixelSize.AddString(strTmp);		
	}
#else
	// CameraGetPixelSizeLine
	char chResult[512] = { 0, };
	size_t szResult = sizeof(chResult);
	VWSDK::CameraGetEnumPropertyItems(pCamera, "PixelSize", chResult, &szResult);

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
	GetCustomCommand(pCamera, "PixelSize", &unPixelSize);

	// Set resolution info.
	SetUIResolutionInfo( unWidth, unHeight, ePixelFormat, unPixelSize );

	// Get device information
	CString strVendorName;
	CString strModelName;
	CString strVersion;
	CString strID;

	GetDeviceInfo( m_radioDevice, strVendorName, strModelName, strVersion, strID );

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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwGigEDemoMultiCamWindowAdvanceDlgC *dlg = (CVwGigEDemoMultiCamWindowAdvanceDlgC *)pObjectInfo->pUserPointer;

	dlg->DrawImage(pObjectInfo, pImageInfo, 1);

	return;
}


void CVwGigEDemoMultiCamWindowAdvanceDlgC::GetImageEvent2(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwGigEDemoMultiCamWindowAdvanceDlgC *dlg = (CVwGigEDemoMultiCamWindowAdvanceDlgC *)pObjectInfo->pUserPointer;

	dlg->DrawImage(pObjectInfo, pImageInfo, 2);

	return;
}


void CVwGigEDemoMultiCamWindowAdvanceDlgC::GetImageEvent3(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwGigEDemoMultiCamWindowAdvanceDlgC *dlg = (CVwGigEDemoMultiCamWindowAdvanceDlgC *)pObjectInfo->pUserPointer;

	dlg->DrawImage(pObjectInfo, pImageInfo, 3);

	return;
}


void CVwGigEDemoMultiCamWindowAdvanceDlgC::GetImageEvent4(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwGigEDemoMultiCamWindowAdvanceDlgC *dlg = (CVwGigEDemoMultiCamWindowAdvanceDlgC *)pObjectInfo->pUserPointer;

	dlg->DrawImage(pObjectInfo, pImageInfo, 4);

	return;
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonPlay1()
{
	if( NULL == m_pCamera )
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

	UpdateData();

	BOOL bRet = FALSE;

	UINT nInputWidth = 0;
	UINT nInputHeight = 0;

	UINT unWidth = 0;
	UINT unHeight = 0;
	UINT unPixelSize = 0;

	CString strWidth;
	CString strHeight;

	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);

	// Set Width, Height
	GetDlgItem( IDC_EDIT_WIDTH )->GetWindowText( strWidth );

	nInputWidth = _ttoi( strWidth );
	bRet = SetWidthCamera( nInputWidth );
	if( bRet )
		strWidth.Format( _T("%d"), nInputWidth );
	else
	{
		GetCustomCommand(m_pCamera, "Width", &unWidth);
		strWidth.Format( _T("%d"), unWidth );
	}
	GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText( strWidth );

	// Set Height
	GetDlgItem( IDC_EDIT_HEIGHT )->GetWindowText( strHeight );
	
	nInputHeight = _ttoi( strHeight );
	bRet = SetHeightCamera( nInputHeight );
	if( bRet )
		strWidth.Format( _T("%d"), nInputHeight );
	else
	{
		GetCustomCommand(m_pCamera, "Height", &unHeight);
		strWidth.Format( _T("%d"), unHeight );
	}
	GetDlgItem( IDC_EDIT_HEIGHT )->SetWindowText( strHeight );

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
	

	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );

	if ( nInputWidth != unWidth ||
		 nInputHeight != unHeight )
	{
		if ( VWSDK::RESULT_SUCCESS != CameraChangeBufferFormat( m_pCamera, m_imagebuffernumber, nInputWidth, nInputHeight, ePixelFormat ) )
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
		return;

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

	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);
	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );

	SetUIResolutionInfo( unWidth, unHeight, ePixelFormat, unPixelSize );

}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonSearchDevice()
{

}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonStop1()
{
	if( NULL == m_pCamera )
	{
		return;
	}

	VWSDK::CameraAbort( m_pCamera );
	
	m_grabbedimagecount = 0;
	m_imageTimeStamps[m_radioDevice].clear();
	
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedButtonClose1()
{
	CWaitCursor oWaitCursor;

	if( m_pCamera != NULL )
	{		
		if( VWSDK::CameraClose( m_pCamera ) == VWSDK::RESULT_SUCCESS )
		{
			// Success
		}
		else
		{
			// Fail
		}
		m_pCamera = NULL;
		m_lstCamera[ m_radioDevice ] = NULL;
	}

	if( m_pUnpackedImage[m_radioDevice] != NULL )
	{
		delete [] m_pUnpackedImage[m_radioDevice];
		m_pUnpackedImage[m_radioDevice] = NULL;
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
}
void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnEnChangeEditImageBuffer()
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedExit()
{
	BOOL bEmpty = TRUE;
	for ( int i = 0; i < DF_DEVICE_COUNT; i ++ )
	{
		if ( NULL != m_lstCamera[ i ] )
		{
			bEmpty = FALSE;
			break;
		}
	}
	
	if( FALSE == bEmpty )
	{
		AfxMessageBox(_T("First, Close device\n"));
		return;
	}

	OnClose();

	CWnd* pWnd = NULL;
	pWnd	   = GetDlgItem(IDC_CAMERA1);
	::ReleaseDC( pWnd->GetSafeHwnd(), m_hdc1 );


	OnOK();
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnTimer(UINT_PTR nIDEvent)
{
	CString tempstr;

	if( nIDEvent == 0 )
	{
		if ( m_curFPS[ m_radioDevice] )
		{
			tempstr.Format(_T("FPS : %3.2f"), m_curFPS[ m_radioDevice]);

			GetDlgItem(IDC_STATIC_FPS)->SetWindowText(tempstr);
		}
	}	

	CDialog::OnTimer(nIDEvent);
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnBnClickedBtnSnap()
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
	if ( FALSE == SetWidthCamera( nInputWidth ) )
	{
		// Rollback
		UINT unWidth = 0;
		GetCustomCommand(m_pCamera, "Width", &unWidth);
		CString strWidth;
		strWidth.Format( _T("%d"), unWidth );
		GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText( strWidth );
	}

	// Set Width, Height
	CString strHeight;
	GetDlgItem(IDC_EDIT_HEIGHT)->GetWindowText(strHeight);
	int nInputHeight = _ttoi(strHeight);
	if (FALSE == SetHeightCamera(nInputHeight))
	{
		// Rollback
		UINT unHeight = 0;
		GetCustomCommand(m_pCamera, "Height", &unHeight);
		CString strHeight;
		strHeight.Format(_T("%d"), unHeight);
		GetDlgItem(IDC_EDIT_HEIGHT)->SetWindowText(strHeight);
	}

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
	UINT unWidth = 0;
	UINT unHeight = 0;
	UINT unPixelSize = 0;
	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);
	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );

	SetUIResolutionInfo( unWidth, unHeight, ePixelFormat, unPixelSize );
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::GetDeviceInfo( int nIndex, CString& strVenderName, CString& strModelName, CString& strDeviceVersion, CString& strDeviceID )
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelFormat()
{
	CWaitCursor oWaitCursor;

	if( m_pCamera == NULL )	
		return;

	int nCurSel = m_cbxPixelFormat.GetCurSel();

	CString strPixelType = GetPixelTypeStr( nCurSel );
#if 0
	VWSDK::PIXEL_FORMAT pixelFormatItem = VWSDK::PIXEL_FORMAT_MONO8;
	for( int i = 0; i < PIXEL_FORMAT_COUNT; i ++ )
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

	switch( ret )
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

	UINT unWidth = 0;
	UINT unHeight = 0;
	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;
	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );
	
	if( VWSDK::RESULT_SUCCESS != VWSDK::CameraChangeBufferFormat( m_pCamera, m_imagebuffernumber, unWidth, unHeight, ePixelFormat ) )
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnCbnSelchangeComboPixelSize()
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
			break;
		default:
			AfxMessageBox(_T("Can't change the pixelsize."));
			GetCustomCommand(m_pCamera, "PixelSize", &nIndex);
			m_cbxPixelSize.SetCurSel( GetPixelSizeIndex( nIndex ) );
			break;
	}
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat, IN UINT unPixelSize )
{
	int nTmp;

	CString strTmp;
	CString strWidth;
	CString strHeight;

	// Width
	strTmp.Format(_T("Width : %d"),tempwidth);
	GetDlgItem(IDC_STATIC_WIDTH)->SetWindowText(strTmp);
	strWidth.Format( _T("%d"), tempwidth );
	GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText(strWidth);

	// Height
	strTmp.Format(_T("Height : %d"),tempheight);
	GetDlgItem(IDC_STATIC_HEIGHT)->SetWindowText(strTmp);
	strHeight.Format( _T("%d"), tempheight );
	GetDlgItem( IDC_EDIT_HEIGHT )->SetWindowText( strHeight );

		
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

int CVwGigEDemoMultiCamWindowAdvanceDlgC::GetPixelTypeIndex( IN CString strType )
{
	for ( int i = 0; i < m_cbxPixelFormat.GetCount(); i ++ ) 
	{
		CString strTmp;
		m_cbxPixelFormat.GetLBText( i, strTmp );

		if ( strTmp.Compare( strType ) == 0 )
		{
			return i;
		}
	}

	return -1;
}


CString CVwGigEDemoMultiCamWindowAdvanceDlgC::GetPixelTypeStr( IN int nIndex )
{
	CString strTmp;
	m_cbxPixelFormat.GetLBText( nIndex, strTmp );
	
	return strTmp;
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::MakeUnPackedBuffer()
{
	// ERROR
	if( m_pCamera == NULL )
		return;

	if( m_pUnpackedImage[m_radioDevice] != NULL )
	{
		delete [] m_pUnpackedImage[m_radioDevice];
		m_pUnpackedImage[m_radioDevice] = NULL;
	}

	//Get image width,height 
	UINT unWidth = 0;
	UINT unHeight = 0;
	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);
	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );


	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//
	if( ePixelFormat == VWSDK::PIXEL_FORMAT_MONO10_PACKED ||
		ePixelFormat == VWSDK::PIXEL_FORMAT_MONO12_PACKED )
	{
		m_pUnpackedImage[m_radioDevice] = new BYTE[unWidth*unHeight*6];
	}
	else if( ePixelFormat == VWSDK::PIXEL_FORMAT_BAYGR8 ||
		     ePixelFormat == VWSDK::PIXEL_FORMAT_BAYRG8 ||
			 ePixelFormat == VWSDK::PIXEL_FORMAT_MONO16 )
	{
		m_pUnpackedImage[m_radioDevice] = new BYTE[unWidth*unHeight*3];
	}
	else
	{
		m_pUnpackedImage[m_radioDevice] = new BYTE[unWidth*unHeight*3];
	}
}

CString CVwGigEDemoMultiCamWindowAdvanceDlgC::GetPixelFormatFromEnum( VWSDK::PIXEL_FORMAT ePixelFormat )
{
	for ( int i = 0; i < PIXEL_FORMAT_COUNT; i ++ )
	{
		if( ePixelFormat == ARR_PIXEL_FORMAT[i] )
		{
			return CString(STR_PIXEL_FORMAT[i]);
		}
	}

	return _T("");
}

BOOL CVwGigEDemoMultiCamWindowAdvanceDlgC::SetWidthCamera( int unWidth )
{
	UINT nCurrWidth = 0;	
	
	if (VWSDK::RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Width", &nCurrWidth))
	{
		// ERROR
		return FALSE;
	}
	
	if ( unWidth != nCurrWidth )
	{
		{
			// CameraSetWidth
			char chValue[100] = { 0, };
			sprintf(chValue, "%d", unWidth);
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

BOOL CVwGigEDemoMultiCamWindowAdvanceDlgC::SetHeightCamera( int unHeight )
{
	UINT nCurrHeight = 0;

	if (VWSDK::RESULT_SUCCESS != GetCustomCommand(m_pCamera, "Height", &nCurrHeight))
	{
		// ERROR
		return FALSE;
	}

	if ( unHeight != nCurrHeight )
	{
		// CameraSetHeight
		char chValue[100] = { 0, };
		sprintf(chValue, "%d", unHeight);
		VWSDK::RESULT ret = VWSDK::CameraSetCustomCommand(m_pCamera, "Height", chValue);

		if ( VWSDK::RESULT_ERROR_VWCAMERA_IMAGE_NOT2DIVIDE == ret )
		{
			AfxMessageBox( _T("Height must be a multiple of 2!"));
			return FALSE;
		}
	}

	return TRUE;
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::RadioCtrl( UINT ID )
{
	UpdateData( TRUE );

	if ( m_radioDeviceOld != m_radioDevice )
	{
		m_pCamera = m_lstCamera[ m_radioDevice ];
		SaveDeviceState( m_radioDeviceOld );
		UpdateDeviceState( m_radioDevice );
	}

	m_radioDeviceOld = m_radioDevice;
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::SaveDeviceState( int nDeviceNum )
{
	CDeviceState* pDeviceState = &m_deviceState[ nDeviceNum ];

	if ( pDeviceState == NULL )
	{
		return;
	}

	pDeviceState->m_bOpen = GetDlgItem( IDC_BUTTON_OPEN1 )->IsWindowEnabled();
	pDeviceState->m_bClose = GetDlgItem( IDC_BUTTON_CLOSE1 )->IsWindowEnabled();

	pDeviceState->m_bImageBuffer = GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->IsWindowEnabled();
	CString strImageBuffer;
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->GetWindowText( strImageBuffer );
	pDeviceState->m_nBuffer = _ttoi( strImageBuffer );

	GetDlgItem( IDC_STATIC_VENDORNAME )->GetWindowText( pDeviceState->m_strVendorName );
	GetDlgItem( IDC_STATIC_MODELNAME )->GetWindowText( pDeviceState->m_strModelName );
	GetDlgItem( IDC_STATIC_VERSION )->GetWindowText( pDeviceState->m_strDeviceVersion );
	GetDlgItem( IDC_STATIC_ID )->GetWindowText( pDeviceState->m_strDeviceID );
	CString strFrames;
	GetDlgItem( IDC_EDIT_FRAMES )->GetWindowText( strFrames );
	pDeviceState->m_nSnapBuffer = _ttoi( strFrames );
	pDeviceState->m_bSnapBuffer = GetDlgItem( IDC_EDIT_FRAMES )->IsWindowEnabled();

	pDeviceState->m_bGrab = GetDlgItem( IDC_BUTTON_PLAY1 )->IsWindowEnabled();
	pDeviceState->m_bSnap = GetDlgItem( IDC_BTN_SNAP )->IsWindowEnabled();
	pDeviceState->m_bAbort = GetDlgItem( IDC_BUTTON_STOP1 )->IsWindowEnabled();

	pDeviceState->m_bPixelFormat = GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->IsWindowEnabled();
	if ( m_cbxPixelFormat.GetCurSel() >= 0 )
	{
		m_cbxPixelFormat.GetLBText( m_cbxPixelFormat.GetCurSel(), pDeviceState->m_strPixelFormat );
	}
	pDeviceState->m_bPixelSize = GetDlgItem( IDC_COMBO_PIXEL_SIZE )->IsWindowEnabled();
	if ( m_cbxPixelSize.GetCurSel() >= 0 )
	{
		m_cbxPixelSize.GetLBText( m_cbxPixelSize.GetCurSel(), pDeviceState->m_strPixelSize );
	}
	pDeviceState->m_bWidth = GetDlgItem( IDC_EDIT_WIDTH )->IsWindowEnabled();
	CString strWidth;
	GetDlgItem( IDC_EDIT_WIDTH )->GetWindowText( strWidth );
	pDeviceState->m_nWidth = _ttoi( strWidth );

	pDeviceState->m_bHeight = GetDlgItem( IDC_EDIT_HEIGHT )->IsWindowEnabled();
	CString strHeight;
	GetDlgItem( IDC_EDIT_HEIGHT )->GetWindowText( strHeight );
	pDeviceState->m_nHeight = _ttoi( strHeight );
}

void CVwGigEDemoMultiCamWindowAdvanceDlgC::UpdateDeviceState( int nDeviceNum )
{
	CDeviceState* pDeviceState = &m_deviceState[ nDeviceNum ];

	if ( pDeviceState == NULL )
	{
		return;
	}

	UpdatePixelFormat();

	GetDlgItem( IDC_BUTTON_OPEN1 )->EnableWindow( pDeviceState->m_bOpen );
	GetDlgItem( IDC_BUTTON_CLOSE1 )->EnableWindow( pDeviceState->m_bClose );

	 GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->EnableWindow( pDeviceState->m_bImageBuffer );
	CString strImageBuffer;
	strImageBuffer.Format( _T("%d"), pDeviceState->m_nBuffer );
	GetDlgItem( IDC_EDIT_IMAGE_BUFFER )->SetWindowText( strImageBuffer );
	m_imagebuffernumber = pDeviceState->m_nBuffer;
	GetDlgItem( IDC_STATIC_VENDORNAME )->SetWindowText( pDeviceState->m_strVendorName );
	GetDlgItem( IDC_STATIC_MODELNAME )->SetWindowText( pDeviceState->m_strModelName );
	GetDlgItem( IDC_STATIC_VERSION )->SetWindowText( pDeviceState->m_strDeviceVersion );
	GetDlgItem( IDC_STATIC_ID )->SetWindowText( pDeviceState->m_strDeviceID );

	GetDlgItem( IDC_EDIT_FRAMES )->EnableWindow( pDeviceState->m_bSnapBuffer );

	CString strFrames;
	strFrames.Format( _T("%d"), pDeviceState->m_nSnapBuffer );
	GetDlgItem( IDC_EDIT_FRAMES )->SetWindowText( strFrames );

	GetDlgItem( IDC_BUTTON_PLAY1 )->EnableWindow( pDeviceState->m_bGrab );
	GetDlgItem( IDC_BTN_SNAP )->EnableWindow( pDeviceState->m_bSnap );
	GetDlgItem( IDC_BUTTON_STOP1 )->EnableWindow( pDeviceState->m_bAbort );

	GetDlgItem( IDC_COMBO_PIXEL_FORMAT )->EnableWindow( pDeviceState->m_bPixelFormat );

	int nPixelFormatCount = m_cbxPixelFormat.GetCount();
	for ( int i = 0; i < nPixelFormatCount; i ++ )
	{
		CString strText;
		m_cbxPixelFormat.GetLBText( i, strText );

		if ( strText == pDeviceState->m_strPixelFormat )
		{
			m_cbxPixelFormat.SetCurSel( i );
			break;
		}
	}

	GetDlgItem( IDC_COMBO_PIXEL_SIZE )->EnableWindow( pDeviceState->m_bPixelSize );
	int nPixelSizeCount = m_cbxPixelSize.GetCount();
	for ( int i = 0; i < nPixelSizeCount; i ++ )
	{
		CString strText;
		m_cbxPixelSize.GetLBText( i, strText );

		if ( strText == pDeviceState->m_bPixelSize )
		{
			m_cbxPixelSize.SetCurSel( i );
			break;
		}
	}

	GetDlgItem( IDC_EDIT_WIDTH )->EnableWindow( pDeviceState->m_bWidth );
	CString strWidth;
	strWidth.Format( _T("%d"), pDeviceState->m_nWidth );
	GetDlgItem( IDC_EDIT_WIDTH )->SetWindowText( strWidth );

	GetDlgItem( IDC_EDIT_HEIGHT )->EnableWindow( pDeviceState->m_bHeight );
	CString strHeight;
	strHeight.Format( _T("%d"), pDeviceState->m_nHeight );
	GetDlgItem( IDC_EDIT_HEIGHT )->SetWindowText( strHeight );


	// Update resolution info.
	UINT unWidth = 0;
	UINT unHeight = 0;
	UINT unPixelSize = 0;
	VWSDK::PIXEL_FORMAT ePixelFormat = VWSDK::PIXEL_FORMAT_MONO8;

	GetCustomCommand(m_pCamera, "Width", &unWidth);
	GetCustomCommand(m_pCamera, "Height", &unHeight);
	GetCustomCommand(m_pCamera, "PixelSize", &unPixelSize);
	VWSDK::CameraGetPixelFormat( m_pCamera, &ePixelFormat );

	SetUIResolutionInfo( unWidth, unHeight, ePixelFormat, unPixelSize );
}

int CVwGigEDemoMultiCamWindowAdvanceDlgC::GetPixelSizeIndex( int nPixelSize )
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

void CVwGigEDemoMultiCamWindowAdvanceDlgC::UpdatePixelFormat()
{
	if ( NULL == m_pCamera )
	{
		return;
	}

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
	//
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	m_cbxPixelFormat.ResetContent();

	for( list<VWSDK::PIXEL_FORMAT>::iterator itList = plstPixelFormat.begin();
		itList != plstPixelFormat.end();
		itList++ )
	{
		CString strTemp;
		strTemp = GetPixelFormatFromEnum( (*itList) );
		m_cbxPixelFormat.AddString( strTemp );
	}
}



void CVwGigEDemoMultiCamWindowAdvanceDlgC::DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo, int nIndex )
{
	nIndex--;

	if( !(0 <= nIndex && nIndex < 4) )
	{
		AfxMessageBox( _T("Invalidate Input Parameter.") );
		return;
	}

	CVwGigEDemoMultiCamWindowAdvanceDlgC* pDlg = (CVwGigEDemoMultiCamWindowAdvanceDlgC*)pObjectInfo->pUserPointer;

	// FPS
	LARGE_INTEGER liTime;
	QueryPerformanceCounter( &liTime ); 

	while( pDlg->m_imageTimeStamps[nIndex].size() > COUNT_FPS )
		pDlg->m_imageTimeStamps[nIndex].pop_front();

	pDlg->m_imageTimeStamps[nIndex].push_back( liTime );

	__int64 diff = liTime.QuadPart-pDlg->m_imageTimeStamps[nIndex].begin()->QuadPart;

	if( diff > 0 )
		pDlg->m_curFPS[nIndex] = (pDlg->m_liFreq.QuadPart * double(pDlg->m_imageTimeStamps[nIndex].size()-1)) / diff;
	else
		pDlg->m_curFPS[nIndex] = 0;

	if( liTime.QuadPart - pDlg->m_liLastDisplayTime[nIndex].QuadPart > pDlg->m_nMinInterFrameTime )
	{
		UINT unWidth	 = pImageInfo->width;
		UINT unHeight	 = pImageInfo->height;
		UINT unBufIdx	 = pImageInfo->bufferIndex;
		UINT unBitCount	 = 0;
		UINT unbiClrUsed = 0;
		int nCalcHeight  = 0;
		void* vpBuffer = pImageInfo->pImage;
		void* pBuf = NULL;
		VWSDK::PIXEL_FORMAT ePixelFormat = pImageInfo->pixelFormat;
		CString strTmp = _T("");


		GetCustomCommand(pObjectInfo->pVwCamera, "Width", &unWidth);
		GetCustomCommand(pObjectInfo->pVwCamera, "Height", &unHeight);
		VWSDK::CameraGetPixelFormat( pObjectInfo->pVwCamera, &ePixelFormat);

		nCalcHeight = (-1 * unHeight);
		unBitCount  = 24;
		unbiClrUsed	= 0;

		if ( VWSDK::PIXEL_FORMAT_MONO8 == ePixelFormat )
		{
			unbiClrUsed	= 256;
		}
		unBitCount = CVwResourceType::GetPixelBitCount( ePixelFormat );
		strTmp = CVwResourceType::GetPixelFormatName( ePixelFormat );
		strTmp =  _T("Pixel Format : ") + strTmp;

		pDlg->m_pBmpInfo[nIndex]->bmiHeader.biWidth       = unWidth;
		pDlg->m_pBmpInfo[nIndex]->bmiHeader.biHeight      = nCalcHeight;
		pDlg->m_pBmpInfo[nIndex]->bmiHeader.biBitCount    = unBitCount;
		pDlg->m_pBmpInfo[nIndex]->bmiHeader.biCompression = BI_RGB;
		pDlg->m_pBmpInfo[nIndex]->bmiHeader.biClrUsed     = unbiClrUsed;

		//pDlg->GetDlgItem(IDC_STATIC_PIXEL_DEPTH)->SetWindowText(strTmp);

		BOOL bRet = CVwResourceType::ConvertPixelFormat( ePixelFormat, pDlg->m_pUnpackedImage[nIndex], (BYTE*)vpBuffer, unWidth, unHeight );
		
		if ( FALSE == bRet )
		{
			AfxMessageBox( _T("Do not support current pixel format.") );
			return;
		}

		pBuf = pDlg->m_pUnpackedImage[nIndex];

		// ERROR
		if( pBuf == NULL )
			return;

		pDlg->m_liLastDisplayTime[nIndex] = liTime;

		UINT unWidthPos;
		UINT unHeightPos;

		switch( nIndex )
		{
		case 0:
			unWidthPos	= 0;
			unHeightPos	= 0;
			break;
		case 1:
			unWidthPos	= pDlg->m_imagecontrolWidth+1;
			unHeightPos	= 0;
			break;
		case 2:
			unWidthPos	= 0;
			unHeightPos	= pDlg->m_imagecontrolHeight+1;
			break;
		case 3:
			unWidthPos	= pDlg->m_imagecontrolWidth+1;
			unHeightPos	= pDlg->m_imagecontrolHeight+1;
			break;
		}

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
			pDlg->m_pBmpInfo[nIndex],
			DIB_RGB_COLORS,
			SRCCOPY );	
	}

	pDlg->m_grabbedimagecount++;
}


BOOL CVwGigEDemoMultiCamWindowAdvanceDlgC::PreTranslateMessage(MSG* pMsg)
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


void CVwGigEDemoMultiCamWindowAdvanceDlgC::OnClose()
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
