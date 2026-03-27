// VwCXP.Demo.MultiCam.Window.AdvanceDlg.cpp : implementation file
//

#include "stdafx.h"
#include "VwCXP.Demo.MultiCam.Window.Advance.h"
#include "VwCXP.Demo.MultiCam.Window.AdvanceDlg.h"

#include "VwCXP.h"
#include <VwCXPGrabberBoard.h>
#include <VwCXPCamera.h>
#include <VwCXPStream.h>

#include "VwImageProcess.h"
#include "FPS.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif


#define	PROPERTY_WIDTH			"Width"
#define	PROPERTY_HEIGHT			"Height"
#define PROPERTY_PIXEL_FORMAT	"PixelFormat"
#define PROPERTY_LINK_CONFIG	"CxpLinkConfigurationPreferredSwitch"
#define PROPERTY_TEST_PATTERN	"TestPattern"
#define LINK_CHANGE_TIMEOUT		15000

#define COUNT_FPS 30


#define WM_USER_UPDATE_FPS		WM_USER+100


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CVwCXPDemoMultiCamWindowAdvanceDlg dialog




CVwCXPDemoMultiCamWindowAdvanceDlg::CVwCXPDemoMultiCamWindowAdvanceDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CVwCXPDemoMultiCamWindowAdvanceDlg::IDD, pParent),
	m_pCXP(NULL),
	m_pSelectedGrabber(NULL),
	m_nSelectedIdx(-1),
	m_bGrabbing(FALSE),
	m_hImgRcv(NULL),
	m_bUseEventCallback(FALSE),
	m_pSrcBuf(NULL)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_hImgRcv = CreateEvent(NULL, FALSE, FALSE, NULL);
}

CVwCXPDemoMultiCamWindowAdvanceDlg::~CVwCXPDemoMultiCamWindowAdvanceDlg()
{
	CloseHandle(m_hImgRcv);
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_DEVICE_LIST, m_listctrlDevice);
	DDX_Control(pDX, IDC_COMBO_PIXELFORMAT, m_comboPixelformat);
	DDX_Control(pDX, IDC_EDIT_WIDTH, m_editboxWidth);
	DDX_Control(pDX, IDC_EDIT_HEIGHT, m_editboxHeight);
	DDX_Control(pDX, IDC_COMBO_LINKCONFIG, m_comboLinkconfig);
	DDX_Control(pDX, IDC_COMBO_TEST_PATTERN, m_comboTestPattern);
}

BEGIN_MESSAGE_MAP(CVwCXPDemoMultiCamWindowAdvanceDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_DISCOVERY, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonDiscovery)
	ON_WM_DESTROY()
	ON_LBN_SELCHANGE(IDC_LIST_DEVICE_LIST, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnLbnSelchangeListDeviceList)
	ON_BN_CLICKED(IDC_BUTTON_OPEN, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonOpen)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonClose)
	ON_BN_CLICKED(IDC_BUTTON_START, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonStart)
	ON_BN_CLICKED(IDC_BUTTON_STOP, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonStop)
	ON_CBN_SELCHANGE(IDC_COMBO_PIXELFORMAT, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnCbnSelchangeComboPixelformat)
	ON_EN_CHANGE(IDC_EDIT_WIDTH, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnEnChangeEditWidth)
	ON_EN_CHANGE(IDC_EDIT_HEIGHT, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnEnChangeEditHeight)
	ON_CBN_SELCHANGE(IDC_COMBO_LINKCONFIG, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnCbnSelchangeComboLinkconfig)
	ON_CBN_SELCHANGE(IDC_COMBO_TEST_PATTERN, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnSelchangeComboTestPattern)
	ON_MESSAGE(WM_USER_UPDATE_FPS, &CVwCXPDemoMultiCamWindowAdvanceDlg::OnUpdateFPS)
END_MESSAGE_MAP()


// CVwCXPDemoMultiCamWindowAdvanceDlg message handlers

BOOL CVwCXPDemoMultiCamWindowAdvanceDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	m_pCXP = new VWSDK::VwCXP;

	m_pCXP->UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	InitialState(TRUE);

	CWnd* pWnd = GetDlgItem( IDC_STATIC_PICTURE );
	if( pWnd )
	{
		m_pDC = pWnd->GetDC();
		m_hDC = m_pDC->GetSafeHdc();
		::SetStretchBltMode(m_hDC, COLORONCOLOR );

		CRect pictualRt;
		pWnd->GetWindowRect(&pictualRt);

		m_nPictualWidth		= pictualRt.Width() / 2;
		m_nPictualHeight	= pictualRt.Height() / 2;

	}

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CVwCXPDemoMultiCamWindowAdvanceDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CVwCXPDemoMultiCamWindowAdvanceDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}





void CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonDiscovery()
{

	Discovery();
	InitialState(TRUE);
}


void CVwCXPDemoMultiCamWindowAdvanceDlg::Discovery()
{
	CWaitCursor oWaitCursor;

	
	VWSDK::RESULT ret = m_pCXP->Discovery();

	if( ret != VWSDK::RESULT_SUCCESS)
	{
		CString strTmp;
		strTmp.Format(_T("Discovery error %d\n"), ret);
		MessageBox(strTmp);
		return;
	}


	m_listctrlDevice.ResetContent();

	CString deviceName;

	UINT unGrabberCount = 0;
	ret = m_pCXP->GetGrabberBoardCount(unGrabberCount);

	if (ret != VWSDK::RESULT_SUCCESS)
	{
		CString strTmp;
		strTmp.Format(_T("Discovery error %d\n"), ret);
		MessageBox(strTmp);
		return;
	}

	for (UINT i = 0; i < unGrabberCount; i++)
	{
		VWSDK::VwCXPGrabberBoard* pGrabber;
		ret = m_pCXP->GetGrabberBoard(&pGrabber, i);

		if (ret != VWSDK::RESULT_SUCCESS || pGrabber == NULL)
		{
			CString strTmp;
			strTmp.Format(_T("Discovery error %d\n"), ret);
			MessageBox(strTmp);
			return;
		}


		deviceName = (pGrabber)->DeviceID();

		m_listctrlDevice.InsertString(i, deviceName);
		m_listctrlDevice.SetItemDataPtr(i, pGrabber);
	}


}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnDestroy()
{
	CDialog::OnDestroy();

	if (m_pDC)
		ReleaseDC(m_pDC);
	
	SAFE_DELETE(m_pCXP)
}




void CVwCXPDemoMultiCamWindowAdvanceDlg::OnLbnSelchangeListDeviceList()
{
	m_nSelectedIdx = m_listctrlDevice.GetCurSel();

	if( m_nSelectedIdx + 1 > MAX_CAMERA_COUNT || m_nSelectedIdx < 0)
		return;

	m_pSelectedGrabber = (VWSDK::VwCXPGrabberBoard*)m_listctrlDevice.GetItemDataPtr(m_nSelectedIdx);

	if( m_pSelectedGrabber != NULL)
	{
		CString strTmp;

		strTmp = m_pSelectedGrabber->Displayname();
		GetDlgItem( IDC_STATIC_DISPLAYNAME )->SetWindowText( strTmp );

		strTmp = m_pSelectedGrabber->Model();
		GetDlgItem( IDC_STATIC_MODEL )->SetWindowText( strTmp );

		strTmp = m_pSelectedGrabber->Vendor();
		GetDlgItem( IDC_STATIC_VENDOR )->SetWindowText( strTmp );	

		strTmp = m_pSelectedGrabber->DeviceID();
		GetDlgItem( IDC_STATIC_DEVICEID )->SetWindowText( strTmp );


		InitialState(FALSE);
	}
	else
	{
		InitialState(TRUE);
	}

	

}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonOpen()
{
	CWaitCursor oWaitCursor;

	if( NULL == m_pSelectedGrabber )
		return;

	m_pSelectedGrabber->OpenCamera();

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();


	if( pCamera != NULL)
	{
		SetUIWidth(pCamera);
		SetUIHeight(pCamera);
		SetUIPixelFormat(pCamera);
		SetUILinkconfig(pCamera);
		SetUITestPattern(pCamera);
	}

	VWSDK::VwCXPStream* pStream = pCamera->GetStream();
	pStream->SetImageCallback( GetImageEvent1, this);


	OpenState(TRUE);
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonClose()
{
	if( NULL == m_pSelectedGrabber )
		return;

	m_pSelectedGrabber->CloseCamera();

	OpenState(FALSE);
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::InitialState( BOOL bState )
{
	if ( TRUE == bState)
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(FALSE);
		
// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(FALSE);
	}
	else
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(FALSE);

// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(FALSE);

	}

}


void CVwCXPDemoMultiCamWindowAdvanceDlg::PlayState( BOOL bState )
{
	
	if ( TRUE == bState)
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(TRUE);

// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(FALSE);
	}
	else
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(FALSE);

// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(TRUE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(TRUE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(TRUE);
	}

}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OpenState( BOOL bState )
{
	if ( TRUE == bState)
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(FALSE);

// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(TRUE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(TRUE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(TRUE);
	}
	else
	{
		GetDlgItem(IDC_BUTTON_DISCOVERY)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_OPEN)->EnableWindow(TRUE);
		GetDlgItem(IDC_BUTTON_CLOSE)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_START)->EnableWindow(FALSE);
		GetDlgItem(IDC_BUTTON_STOP)->EnableWindow(FALSE);

// 		GetDlgItem(IDC_COMBO_PIXELFORMAT)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_WIDTH)->EnableWindow(FALSE);
// 		GetDlgItem(IDC_EDIT_HEIGHT)->EnableWindow(FALSE);
	}
	
}



void CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonStart()
{
	CWaitCursor oWaitCursor;

	if(m_nSelectedIdx < 0 || m_nSelectedIdx >= MAX_CAMERA_COUNT)
		return;

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	if(NULL == pCamera )
		return;

	m_bGrabbing = TRUE;

	pCamera->AcquisitionStart();

	m_hThreadGrab[m_nSelectedIdx] = (HANDLE)CreateThread(NULL, 0, GrabContinuousThread, this, 0, NULL);

	PlayState(TRUE);
}




void CVwCXPDemoMultiCamWindowAdvanceDlg::OnBnClickedButtonStop()
{
	CWaitCursor oWaitcursor;

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();
	
	m_bGrabbing = FALSE;
	WaitForSingleObject(m_hThreadGrab[m_nSelectedIdx], 5000);
	CloseHandle(m_hThreadGrab[m_nSelectedIdx]);

	pCamera->AcquisitionStop();
	

	PlayState(FALSE);
}



DWORD CVwCXPDemoMultiCamWindowAdvanceDlg::GrabContinuousThread (LPVOID _pOwner)
{

	CVwCXPDemoMultiCamWindowAdvanceDlg * self = (CVwCXPDemoMultiCamWindowAdvanceDlg *)_pOwner;

	self->ThrGrabContinuous();



	return 0;
}

UINT CVwCXPDemoMultiCamWindowAdvanceDlg::ThrGrabContinuous()
{
	CFPS oFPS;

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();
	if(NULL == pCamera)
		goto EXIT;

	VWSDK::VwCXPStream* pStream = pCamera->GetStream();
	if(NULL == pStream)
		goto EXIT;

	int nCameraIdx = m_nSelectedIdx;
	int pictual_baseX, pictual_baseY, pictual_width, pictual_height;

	switch (nCameraIdx)
	{
	case 0:
		pictual_baseX	= 0;
		pictual_baseY	= 0;
		pictual_width	= m_nPictualWidth;
		pictual_height	= m_nPictualHeight;
		break;
	case 1:
		pictual_baseX	= m_nPictualWidth;
		pictual_baseY	= 0;
		pictual_width	= m_nPictualWidth;
		pictual_height	= m_nPictualHeight;
		break;
	case 2:
		pictual_baseX	= 0;
		pictual_baseY	= m_nPictualHeight;
		pictual_width	= m_nPictualWidth;
		pictual_height	= m_nPictualHeight;
		break;
	case 3:
		pictual_baseX	= m_nPictualWidth;
		pictual_baseY	= m_nPictualHeight;
		pictual_width	= m_nPictualWidth;
		pictual_height	= m_nPictualHeight;
		break;
	}

	int nBitCount	= 8;
	int nClrUsed	= 0;
	UINT unUsage	= DIB_RGB_COLORS;

	const int TMP_SIZE				= 256;
	size_t nPixelFormatSize			= TMP_SIZE;
	char pszPixelFormat[TMP_SIZE]	= {0,};
		
	VWSDK::RESULT ret = pCamera->GetCustomCommand(PROPERTY_PIXEL_FORMAT, pszPixelFormat, &nPixelFormatSize);

	PLOGPALETTE pLogPallete = NULL;
	HPALETTE hPalette		= NULL;
	HPALETTE hOldPalette		= NULL;
	int	nColorCount = 0;


	if( strcmp(pszPixelFormat, "Mono8") == 0 || 
		strcmp(pszPixelFormat, "Mono10") == 0 ||
		strcmp(pszPixelFormat, "Mono12") == 0)
	{
		nBitCount	= 8;
		nColorCount = 256;		
		nClrUsed	= nColorCount;
		unUsage	= DIB_PAL_COLORS;

		pLogPallete = (PLOGPALETTE)new BYTE[ sizeof(WORD) + sizeof(WORD) + nColorCount * sizeof(PALETTEENTRY) ];
		ZeroMemory(pLogPallete, sizeof(WORD) + sizeof(WORD) + nColorCount * sizeof(PALETTEENTRY) );

		pLogPallete->palNumEntries	= nColorCount;
		pLogPallete->palVersion		= 0x300;

		for(int i = 0; i < nColorCount; i++)
		{
			pLogPallete->palPalEntry[i].peRed	= (BYTE)(i*2);
			pLogPallete->palPalEntry[i].peGreen	= (BYTE)(i*2);
			pLogPallete->palPalEntry[i].peBlue	= (BYTE)(i*2);
			pLogPallete->palPalEntry[i].peFlags	= PC_RESERVED;
		}
			

		hPalette	= ::CreatePalette(pLogPallete);
		hOldPalette = ::SelectPalette(m_hDC, hPalette, FALSE);

		RealizePalette(m_hDC);
	}
	else if(strcmp(pszPixelFormat, "BayerRG8")	== 0	||
			strcmp(pszPixelFormat, "BayerRG10")	== 0	||
			strcmp(pszPixelFormat, "BayerGB8")	== 0	||
			strcmp(pszPixelFormat, "BayerGB10")	== 0	||
			strcmp(pszPixelFormat, "BayerGR8")	== 0	||
			strcmp(pszPixelFormat, "BayerGR10") == 0	||
			strcmp(pszPixelFormat, "RGB8") == 0 ||
			strcmp(pszPixelFormat, "RGB10") == 0 ||
			strcmp(pszPixelFormat, "RGB12") == 0
			)
	{
		nBitCount	= 24;
		nClrUsed	= 256;

		unUsage	= DIB_RGB_COLORS;
	}
	

	BITMAPINFO* pBmpInfo = (BITMAPINFO*)new BYTE[(sizeof(BITMAPINFOHEADER)+ 256 * sizeof(RGBQUAD))];
	ZeroMemory(pBmpInfo, sizeof(BITMAPINFOHEADER));

	pBmpInfo->bmiHeader.biSize			= sizeof( BITMAPINFOHEADER );
	pBmpInfo->bmiHeader.biPlanes		= 1;	
	pBmpInfo->bmiHeader.biBitCount		= nBitCount;
	pBmpInfo->bmiHeader.biCompression	= BI_RGB;
	pBmpInfo->bmiHeader.biClrUsed		= nClrUsed;
	pBmpInfo->bmiHeader.biClrImportant	= nClrUsed;
	

	for( UINT j = 0; j < 256; j++ )
	{
		pBmpInfo->bmiColors[j].rgbBlue     = (BYTE)(j);
		pBmpInfo->bmiColors[j].rgbGreen    = (BYTE)(j);
		pBmpInfo->bmiColors[j].rgbRed      = (BYTE)(j);
		pBmpInfo->bmiColors[j].rgbReserved = 0;
	}


	

	size_t bufferWidth = pStream->BufferWidth();
	size_t bufferHeight = pStream->BufferHeight();

	unsigned char* pGrabImage = NULL;
	if (NULL == pGrabImage && bufferWidth != 0 && bufferHeight != 0)
	{
		SAFE_DELETE_ARRAY(pGrabImage);
		pGrabImage = new unsigned char[bufferWidth * bufferHeight * 3];
	}

	if (m_bUseEventCallback)
	{
		m_pSrcBuf = new unsigned char[bufferWidth * bufferHeight * 3];
		ZeroMemory(m_pSrcBuf, bufferWidth*bufferHeight * 3);
	}

	while(m_bGrabbing)
	{
		oFPS.vBegin();

		if (m_bUseEventCallback)
		{
			WaitForSingleObject(m_hImgRcv, INFINITE);
		}
		else
		{
			m_pSrcBuf = (unsigned char*)pStream->GrabImage();
		}

		if (NULL == m_pSrcBuf)
			continue;
		
		float fCurrentFPS =	oFPS.fEnd();		

		m_fCurrentFPS = fCurrentFPS;
		PostMessage(WM_USER_UPDATE_FPS, NULL, NULL);

		
		

		ConvertPixelFormat(pszPixelFormat, pGrabImage, m_pSrcBuf, bufferWidth, bufferHeight);

		if (FALSE == m_bUseEventCallback)
		{
			pStream->QueueBuffer();
		}
		
				

		pBmpInfo->bmiHeader.biWidth		= bufferWidth;
		pBmpInfo->bmiHeader.biHeight	= (-1) * bufferHeight;
		pBmpInfo->bmiHeader.biSizeImage = bufferWidth * bufferHeight;
		

		int nScanline = StretchDIBits(m_hDC,
										pictual_baseX,
										pictual_baseY,
										pictual_width,
										pictual_height,
										0,
										0,
										bufferWidth,
										bufferHeight,
										pGrabImage,
										pBmpInfo,
										unUsage,
										SRCCOPY );


	}

	if (m_bUseEventCallback)
	{
		SAFE_DELETE_ARRAY(m_pSrcBuf);
	}

	SAFE_DELETE_ARRAY(pLogPallete);
	SAFE_DELETE_ARRAY(pBmpInfo);
	SAFE_DELETE_ARRAY(pGrabImage);

	m_bGrabbing = FALSE;

	if( hPalette != NULL)
	{
		::SelectPalette(m_hDC, hOldPalette, FALSE);

		DeleteObject(hPalette);
	}
EXIT:

	return 0;

}

BOOL CVwCXPDemoMultiCamWindowAdvanceDlg::PreTranslateMessage(MSG* pMsg)
{

	if(pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
	{
		return TRUE;
	}
	else if(pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_ESCAPE)
	{
		return TRUE;
	}
	else if(pMsg->message == WM_SYSKEYDOWN && pMsg->wParam == VK_F4)
	{
		return TRUE;
	}
	else
	{
		return CDialog::PreTranslateMessage(pMsg);
	}
}









void CVwCXPDemoMultiCamWindowAdvanceDlg::OnCbnSelchangeComboPixelformat()
{
	if( NULL == m_pSelectedGrabber )
		return;

	CString strSelected;
	int nSelected = m_comboPixelformat.GetCurSel();
	m_comboPixelformat.GetLBText(nSelected,strSelected);

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	char pszTmp[256] = {0,};

	wcstombs(pszTmp, strSelected.GetBuffer(), 256);

	if( pCamera != NULL)
		VWSDK::RESULT ret = pCamera->SetCustomCommand(PROPERTY_PIXEL_FORMAT, pszTmp);


}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnEnChangeEditWidth()
{
	if( NULL == m_pSelectedGrabber )
		return;

	CString strWidth;
	GetDlgItem(IDC_EDIT_WIDTH)->GetWindowTextW(strWidth);

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	char pszTmp[256] = {0,};

	wcstombs(pszTmp, strWidth.GetBuffer(), 256);

	if( pCamera != NULL)
		VWSDK::RESULT ret = pCamera->SetCustomCommand(PROPERTY_WIDTH, pszTmp);



}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnEnChangeEditHeight()
{
	if( NULL == m_pSelectedGrabber )
		return;

	CString strHeight;
	GetDlgItem(IDC_EDIT_HEIGHT)->GetWindowTextW(strHeight);

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	char pszTmp[256] = {0,};

	wcstombs(pszTmp, strHeight.GetBuffer(), 256);

	if( pCamera != NULL)
		VWSDK::RESULT ret = pCamera->SetCustomCommand(PROPERTY_HEIGHT, pszTmp);


}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnCbnSelchangeComboLinkconfig()
{
	CWaitCursor oWaitCursor;

	if( NULL == m_pSelectedGrabber )
		return;

	CString strSelected;
	int nSelected = m_comboLinkconfig.GetCurSel();
	m_comboLinkconfig.GetLBText(nSelected,strSelected);

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	char pszTmp[256] = {0,};

	wcstombs(pszTmp, strSelected.GetBuffer(), 256);

	if( pCamera != NULL)
		VWSDK::RESULT ret = pCamera->SetCustomCommand(PROPERTY_LINK_CONFIG, pszTmp);


	Sleep(LINK_CHANGE_TIMEOUT);
}


void CVwCXPDemoMultiCamWindowAdvanceDlg::OnSelchangeComboTestPattern()
{
	if( NULL == m_pSelectedGrabber )
		return;

	CString strSelected;
	int nSelected = m_comboTestPattern.GetCurSel();
	m_comboTestPattern.GetLBText(nSelected,strSelected);

	VWSDK::VwCXPCamera* pCamera = m_pSelectedGrabber->GetCamera();

	char pszTmp[256] = {0,};

	wcstombs(pszTmp, strSelected.GetBuffer(), 256);

	if( pCamera != NULL)
		VWSDK::RESULT ret = pCamera->SetCustomCommand(PROPERTY_TEST_PATTERN, pszTmp);

}

LRESULT CVwCXPDemoMultiCamWindowAdvanceDlg::OnUpdateFPS(WPARAM wParam, LPARAM lParam)
{
	CString strFPS = _T("");
	strFPS.Format(_T("FPS - %.1f"), m_fCurrentFPS);


	GetDlgItem(IDC_STATIC_FPS)->SetWindowText(strFPS);

	return 0;
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::SetUIWidth(VWSDK::VwCXPCamera* pCamera)
{
	if(NULL == pCamera)
		return;

	CString		strTmp;
	char		pszWidth[256]	= {0,};
	size_t		nWidthSize		= sizeof(pszWidth) / sizeof(char);
	VWSDK::PROPERTY	propertyInfo;

	//width
	pCamera->GetCustomCommand(PROPERTY_WIDTH, pszWidth, &nWidthSize);
	strTmp = pszWidth;
	m_editboxWidth.SetWindowTextW(strTmp);

	pCamera->GetPropertyInfo(PROPERTY_WIDTH, &propertyInfo );
	if( propertyInfo.eAccessMode == VWSDK::WRITE_ONLY || propertyInfo.eAccessMode == VWSDK::READ_WRITE)
		m_editboxWidth.EnableWindow(TRUE);
	else
		m_editboxWidth.EnableWindow(FALSE);
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::SetUIHeight(VWSDK::VwCXPCamera* pCamera)
{
	if(NULL == pCamera)
		return;

	CString			strTmp;
	char			pszHeight[256]	= {0,};
	size_t			nHeightSize		= sizeof(pszHeight) / sizeof(char);
	VWSDK::PROPERTY	propertyInfo;
	
	//height
	pCamera->GetCustomCommand(PROPERTY_HEIGHT, pszHeight, &nHeightSize);
	strTmp = pszHeight;
	m_editboxHeight.SetWindowTextW(strTmp);
	pCamera->GetPropertyInfo(PROPERTY_HEIGHT, &propertyInfo );
	if( propertyInfo.eAccessMode == VWSDK::WRITE_ONLY || propertyInfo.eAccessMode == VWSDK::READ_WRITE)
		m_editboxHeight.EnableWindow(TRUE);
	else
		m_editboxHeight.EnableWindow(FALSE);

}

void CVwCXPDemoMultiCamWindowAdvanceDlg::SetUIPixelFormat(VWSDK::VwCXPCamera* pCamera)
{
	if(NULL == pCamera)
		return;

	m_comboPixelformat.ResetContent();

	CString			strTmp;
	char			pszPixelformat[256] = {0,};
	size_t			nPixelformatSize	= sizeof(pszPixelformat) / sizeof(char);
	VWSDK::PROPERTY	propertyInfo;
		
	//pixel format
	// Get Entry Num
	pCamera->GetCustomCommand( PROPERTY_PIXEL_FORMAT, pszPixelformat, &nPixelformatSize, VWSDK::GET_CUSTOM_COMMAND_NUM );

	int nPixelFormatNum = atoi(pszPixelformat);
	CString strPixelFormat;

	for( int i = 0; i < nPixelFormatNum; i++ )
	{

		// Get Entry Each Value
		nPixelformatSize = sizeof(pszPixelformat) / sizeof(char);
		pCamera->GetCustomCommand( PROPERTY_PIXEL_FORMAT, pszPixelformat, &nPixelformatSize, VWSDK::GET_CUSTOM_COMMAND_INDEX+i );

		strPixelFormat = pszPixelformat;
		// Put Entry Each Value
		m_comboPixelformat.AddString( strPixelFormat );
	}

	// Get Current Set Value
	nPixelformatSize = sizeof(pszPixelformat) / sizeof(char);
	pCamera->GetCustomCommand( PROPERTY_PIXEL_FORMAT, pszPixelformat, &nPixelformatSize );
	strPixelFormat = pszPixelformat;

	for( int i = 0; i < m_comboPixelformat.GetCount(); i++ )
	{
		m_comboPixelformat.GetLBText( i, strTmp );

		if( !strTmp.Compare( strPixelFormat ) )
		{
			m_comboPixelformat.SetCurSel(i);
			break;
		}
	}

	pCamera->GetPropertyInfo(PROPERTY_PIXEL_FORMAT, &propertyInfo );
	if( propertyInfo.eAccessMode == VWSDK::WRITE_ONLY || propertyInfo.eAccessMode == VWSDK::READ_WRITE)
		m_comboPixelformat.EnableWindow(TRUE);
	else
		m_comboPixelformat.EnableWindow(FALSE);
}

void CVwCXPDemoMultiCamWindowAdvanceDlg::SetUILinkconfig(VWSDK::VwCXPCamera* pCamera)
{
	if(NULL == pCamera)
		return;

	// Initialize comboBox.
	m_comboLinkconfig.ResetContent();

	// linkconfig
	CString			strTmp;
	char			pszLinkconfig[256]	= {0,};
	size_t			nLinkconfigSize		= sizeof(pszLinkconfig) / sizeof(char);
	VWSDK::PROPERTY	propertyInfo;

	// Get Entry Num
	pCamera->GetCustomCommand( PROPERTY_LINK_CONFIG, pszLinkconfig, &nLinkconfigSize, VWSDK::GET_CUSTOM_COMMAND_NUM );

	int nLinkconfigNum = atoi(pszLinkconfig);
	CString strLinkconfig;

	for( int i = 0; i < nLinkconfigNum; i++ )
	{

		// Get Entry Each Value
		nLinkconfigSize = sizeof(pszLinkconfig) / sizeof(char);
		pCamera->GetCustomCommand( PROPERTY_LINK_CONFIG, pszLinkconfig, &nLinkconfigSize, VWSDK::GET_CUSTOM_COMMAND_INDEX+i );

		strLinkconfig = pszLinkconfig;
		// Put Entry Each Value
		m_comboLinkconfig.AddString( strLinkconfig );
	}

	// Get Current Set Value
	nLinkconfigSize = sizeof(pszLinkconfig) / sizeof(char);
	pCamera->GetCustomCommand( PROPERTY_LINK_CONFIG, pszLinkconfig, &nLinkconfigSize );
	strLinkconfig = pszLinkconfig;

	for( int i = 0; i < m_comboLinkconfig.GetCount(); i++ )
	{
		m_comboLinkconfig.GetLBText( i, strTmp );

		if( !strTmp.Compare( strLinkconfig ) )
		{
			m_comboLinkconfig.SetCurSel(i);
			break;
		}
	}

	pCamera->GetPropertyInfo(PROPERTY_LINK_CONFIG, &propertyInfo );
	if( propertyInfo.eAccessMode == VWSDK::WRITE_ONLY || propertyInfo.eAccessMode == VWSDK::READ_WRITE)
		m_comboLinkconfig.EnableWindow(TRUE);
	else
		m_comboLinkconfig.EnableWindow(FALSE);
}


void CVwCXPDemoMultiCamWindowAdvanceDlg::SetUITestPattern(VWSDK::VwCXPCamera* pCamera)
{
	if(NULL == pCamera)
		return;

	// Initialize comboBox.
	m_comboTestPattern.ResetContent();

	// linkconfig
	CString		strTmp;
	char		pszTestPattern[256]	= {0,};
	size_t		nTestPatternSize		= sizeof(pszTestPattern) / sizeof(char);
	VWSDK::PROPERTY	propertyInfo;

	// Get Entry Num
	pCamera->GetCustomCommand( PROPERTY_TEST_PATTERN, pszTestPattern, &nTestPatternSize, VWSDK::GET_CUSTOM_COMMAND_NUM );

	int nTestPatternNum = atoi(pszTestPattern);
	CString strTestPattern;

	for( int i = 0; i < nTestPatternNum; i++ )
	{

		// Get Entry Each Value
		nTestPatternSize = sizeof(pszTestPattern) / sizeof(char);
		pCamera->GetCustomCommand( PROPERTY_TEST_PATTERN, pszTestPattern, &nTestPatternSize, VWSDK::GET_CUSTOM_COMMAND_INDEX+i );

		strTestPattern = pszTestPattern;
		// Put Entry Each Value
		m_comboTestPattern.AddString( strTestPattern );
	}

	// Get Current Set Value
	nTestPatternSize = sizeof(pszTestPattern) / sizeof(char);
	pCamera->GetCustomCommand( PROPERTY_TEST_PATTERN, pszTestPattern, &nTestPatternSize );
	strTestPattern = pszTestPattern;

	for( int i = 0; i < m_comboTestPattern.GetCount(); i++ )
	{
		m_comboTestPattern.GetLBText( i, strTmp );

		if( !strTmp.Compare( strTestPattern ) )
		{
			m_comboTestPattern.SetCurSel(i);
			break;
		}
	}

	pCamera->GetPropertyInfo(PROPERTY_TEST_PATTERN, &propertyInfo );
	if( propertyInfo.eAccessMode == VWSDK::WRITE_ONLY || propertyInfo.eAccessMode == VWSDK::READ_WRITE)
		m_comboTestPattern.EnableWindow(TRUE);
	else
		m_comboTestPattern.EnableWindow(FALSE);

}

void CVwCXPDemoMultiCamWindowAdvanceDlg::GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo)
{
	CVwCXPDemoMultiCamWindowAdvanceDlg* dlg = (CVwCXPDemoMultiCamWindowAdvanceDlg*)pObjectInfo->pUserPointer;

	UINT unDepth = 1;
	if (pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_MONO8 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGB8 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYBG8 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGR8 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYRG8)
	{
		unDepth = 1;
	}
	else if (pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_MONO10 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_MONO12 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_MONO16 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGB10 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYBG10 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGR10 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYRG10 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGB12 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYBG12 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYGR12 ||
		pImageInfo->pixelFormat == VWSDK::PIXEL_FORMAT_BAYRG12 )
	{
		unDepth = 2;
	}
	else
	{
		unDepth = 3;
	}

	if( dlg->m_pSrcBuf )
		memcpy(dlg->m_pSrcBuf, pImageInfo->pImage, (pImageInfo->width*pImageInfo->height*unDepth) );

	SetEvent(dlg->m_hImgRcv);
}

void ConvertPixelFormat( char* a_pszPixelFormat, BYTE* a_pDest, BYTE* a_pSource, int a_nWidth, int a_nHeight )
{
	if( strcmp(a_pszPixelFormat, "Mono8") == 0)
	{
		memcpy( a_pDest, a_pSource, a_nWidth * a_nHeight );
	}
	else if( strcmp(a_pszPixelFormat, "Mono10") == 0)
	{
		VWSDK::VwImageProcess::ConvertMono10ToMono8(PBYTE(a_pSource), a_nWidth*a_nHeight * 2, a_pDest);
	}
	else if (strcmp(a_pszPixelFormat, "Mono12") == 0)
	{
		VWSDK::VwImageProcess::ConvertMono12ToMono8(PBYTE(a_pSource), a_nWidth*a_nHeight * 2, a_pDest);
	}
	else if( strcmp(a_pszPixelFormat, "BayerRG8") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYRG8ToBGR8( a_pSource,
							a_pDest,
							a_nWidth,
							a_nHeight );
	}
	else if( strcmp(a_pszPixelFormat, "BayerRG10") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYRG10ToBGR8( (WORD*)(a_pSource),
								a_pDest,
								a_nWidth,
								a_nHeight );

	}
	else if( strcmp(a_pszPixelFormat, "BayerGB8") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYGB8ToBGR8( a_pSource,
			a_pDest,
			a_nWidth,
			a_nHeight );
	}
	else if( strcmp(a_pszPixelFormat, "BayerGB10") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYGB10ToBGR8( (WORD*)(a_pSource),
			a_pDest,
			a_nWidth,
			a_nHeight );
	}
	else if( strcmp(a_pszPixelFormat, "BayerGR8") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYGR8ToBGR8( a_pSource,
			a_pDest,
			a_nWidth,
			a_nHeight );
	}
	else if( strcmp(a_pszPixelFormat, "BayerGR10") == 0)
	{
		VWSDK::VwImageProcess::ConvertBAYGR10ToBGR8( (WORD*)(a_pSource),
			a_pDest,
			a_nWidth,
			a_nHeight );
	}
	else if (strcmp(a_pszPixelFormat, "RGB8") == 0)
	{
		VWSDK::VwImageProcess::ConvertRGB8ToBGR8(PBYTE(a_pSource), UINT(3 * a_nWidth*a_nHeight), a_pDest);
	}
	else if (strcmp(a_pszPixelFormat, "RGB10") == 0)
	{
		VWSDK::VwImageProcess::ConvertRGB10ToBGR8((PBYTE)a_pSource, UINT(6 * a_nWidth*a_nHeight), a_pDest);
	}
	else if (strcmp(a_pszPixelFormat, "RGB12") == 0)
	{
		VWSDK::VwImageProcess::ConvertRGB12ToBGR8((PBYTE)a_pSource, UINT(6 * a_nWidth*a_nHeight), a_pDest);
	}
}
