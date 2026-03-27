// VwGigE.Demo.GigE.DiscoveryDlg.cpp : implementation file
//

#include "stdafx.h"
#include "VwGigE.Demo.GigE.Discovery.h"
#include "VwGigE.Demo.GigE.DiscoveryDlg.h"
#include "VwGigE.Demo.GigE.DiscoveryCameraDlg.h"

// for VwGigE
#include "VwGigE.h"

#include "DlgForceIP.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


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


// CVwGigEDemoGigEDiscoveryDlg dialog




CVwGigEDemoGigEDiscoveryDlg::CVwGigEDemoGigEDiscoveryDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CVwGigEDemoGigEDiscoveryDlg::IDD, pParent)
	, m_pvwGigE ( NULL )
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	// Init. list
	m_plistCameraDlg.RemoveAll();
}

CVwGigEDemoGigEDiscoveryDlg::~CVwGigEDemoGigEDiscoveryDlg()
{

}

void CVwGigEDemoGigEDiscoveryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST_CAMERA, m_lstCamera);
}

BEGIN_MESSAGE_MAP(CVwGigEDemoGigEDiscoveryDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_BUTTON_DISCOVERY, &CVwGigEDemoGigEDiscoveryDlg::OnBnClickedButtonDiscovery)
	ON_BN_CLICKED(IDC_BUTTON_OPEN, &CVwGigEDemoGigEDiscoveryDlg::OnBnClickedButtonOpen)
	ON_WM_DESTROY()
	ON_BN_CLICKED(IDCANCEL, &CVwGigEDemoGigEDiscoveryDlg::OnBnClickedCancel)
	ON_BN_CLICKED(IDC_BTN_FORCEIP, &CVwGigEDemoGigEDiscoveryDlg::OnBnClickedBtnForceip)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_CAMERA, &CVwGigEDemoGigEDiscoveryDlg::OnLvnItemchangedListCamera)
END_MESSAGE_MAP()


// CVwGigEDemoGigEDiscoveryDlg message handlers

BOOL CVwGigEDemoGigEDiscoveryDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
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

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// Enable window
	GetDlgItem( IDC_BTN_FORCEIP )->EnableWindow( FALSE );

	// Init list ctrl
	m_lstCamera.ModifyStyle( 0, LVS_SHOWSELALWAYS );
	m_lstCamera.SetExtendedStyle( m_lstCamera.GetExtendedStyle() | LVS_EX_FULLROWSELECT );

	CStringArray arrColumnName;
	arrColumnName.Add( _T("Name") );
	arrColumnName.Add( _T("Vendor") );
	arrColumnName.Add( _T("Model") );
	arrColumnName.Add( _T("IP Address") );
	arrColumnName.Add( _T("MAC Address") );
	arrColumnName.Add( _T("IP Status") );

	CRect rtList;
	GetClientRect( &rtList );

	int nItemWidth = rtList.Width() / (int)arrColumnName.GetSize();
	for ( int i = 0; i < arrColumnName.GetSize(); i ++ )
	{
		m_lstCamera.InsertColumn( i, arrColumnName.GetAt( i ), LVCFMT_LEFT, nItemWidth );
	}

	// Create VwGigE
	m_pvwGigE = new VWSDK::VwGigE;
	m_pvwGigE->UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	if ( NULL == m_pvwGigE )
	{
		// ERROR
				
		ASSERT( m_pvwGigE );
	}

	VWSDK::RESULT ret = m_pvwGigE->Open();

	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// ERROR
	}

	GetDlgItem( IDC_BUTTON_OPEN )->EnableWindow( FALSE );
	GetDlgItem( IDC_BTN_FORCEIP )->EnableWindow( FALSE );
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CVwGigEDemoGigEDiscoveryDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CVwGigEDemoGigEDiscoveryDlg::OnPaint()
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
HCURSOR CVwGigEDemoGigEDiscoveryDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CVwGigEDemoGigEDiscoveryDlg::OnBnClickedButtonDiscovery()
{
	CWaitCursor oWaitCursor;

	if ( NULL == m_pvwGigE )
	{
		ASSERT( m_pvwGigE );

		return;
	}

	VWSDK::RESULT ret = m_pvwGigE->Discovery();

	//RESULT ret = vwGigE.Discovery();

	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// ERROR
		return;
	}

	UINT nCameraNum = 0;
	VWSDK::RESULT Ret = m_pvwGigE->GetNumCameras( &nCameraNum );

	if ( VWSDK::RESULT_ERROR == Ret )
	{
		// Message
		return;
	}

	// Init. list
	m_lstCamera.DeleteAllItems();

	for ( int i = 0; i < (int)nCameraNum; i ++ )
	{
		VWSDK::VWCAMERA_INFO cameraInfo;
		m_pvwGigE->GetCameraInfo( i, &cameraInfo );

		//
		// Add list
		CString strCameraName;
		strCameraName = cameraInfo.name;
		int nInsertedIndex = m_lstCamera.InsertItem( i, strCameraName );
		CString strVendor;
		strVendor = cameraInfo.vendor;
		m_lstCamera.SetItem( nInsertedIndex, 1, LVIF_TEXT, strVendor, 0, 0, 0, 0 );
		CString strModel;
		strModel = cameraInfo.model;
		m_lstCamera.SetItem( nInsertedIndex, 2, LVIF_TEXT, strModel, 0, 0, 0, 0 );
		CString strIPAddress;
		strIPAddress = cameraInfo.ip;
		m_lstCamera.SetItem( nInsertedIndex, 3, LVIF_TEXT, strIPAddress, 0, 0, 0, 0 );
		CString strMACAddress;
		strMACAddress = cameraInfo.mac;
		m_lstCamera.SetItem( nInsertedIndex, 4, LVIF_TEXT, strMACAddress, 0, 0, 0, 0 );
	}
}


void CVwGigEDemoGigEDiscoveryDlg::AddListCameraInfo()
{

}
void CVwGigEDemoGigEDiscoveryDlg::OnBnClickedButtonOpen()
{
	CWaitCursor oWaitCursor;

	POSITION pos = m_lstCamera.GetFirstSelectedItemPosition();

	while ( pos )
	{
		int nSelectedIndex = m_lstCamera.GetNextSelectedItem( pos );
		CString strName = m_lstCamera.GetItemText( nSelectedIndex, 0/*Camera name*/ );
		char szName[128];

		int nMultiByteLen = WideCharToMultiByte( CP_ACP, 0, strName, -1, NULL, 0, NULL, NULL );
		WideCharToMultiByte( CP_ACP, 0, strName, -1, szName, nMultiByteLen, NULL, NULL );


		CVwGigEDemoGigEDiscoveryCameraDlg* pCameraDlg = new CVwGigEDemoGigEDiscoveryCameraDlg( m_pvwGigE, szName, this );
		pCameraDlg->Create( IDD_DLG_CAMERA );
		pCameraDlg->ShowWindow( SW_SHOW );

		m_plistCameraDlg.AddTail( pCameraDlg );
		
	}
}

void CVwGigEDemoGigEDiscoveryDlg::OnDestroy()
{
	CDialog::OnDestroy();

	POSITION pos = m_plistCameraDlg.GetHeadPosition();

	while ( pos )
	{
		CVwGigEDemoGigEDiscoveryCameraDlg* pCameraDlg = m_plistCameraDlg.GetNext( pos );

		if ( NULL != pCameraDlg )
		{
			delete pCameraDlg;
		}
	}
	
	if ( m_pvwGigE )
	{
		m_pvwGigE->Close();
		delete m_pvwGigE;
		m_pvwGigE = NULL;
	}
}

void CVwGigEDemoGigEDiscoveryDlg::OnBnClickedCancel()
{
	if ( NULL != m_pvwGigE )
	{
		m_pvwGigE->Close();
		delete m_pvwGigE;
		m_pvwGigE = NULL;
	}

	OnCancel();
}

void CVwGigEDemoGigEDiscoveryDlg::OnBnClickedBtnForceip()
{
	DWORD dwIP = 0;
	DWORD dwSubnet = 0;
	DWORD dwGateway = 0;

	POSITION pos = m_lstCamera.GetFirstSelectedItemPosition();

	if ( NULL == pos )
	{
		return;
	}

	int nSelectedIndex = m_lstCamera.GetNextSelectedItem( pos );
	CString strName = m_lstCamera.GetItemText( nSelectedIndex, 0/*Camera name*/ );
	char szName[128]; // = (char*)((LPCTSTR)strName);

	int nMultiByteLen = WideCharToMultiByte( CP_ACP, 0, strName, -1, NULL, 0, NULL, NULL );
	WideCharToMultiByte( CP_ACP, 0, strName, -1, szName, nMultiByteLen, NULL, NULL );

	VWSDK::VWCAMERA_INFO cameraInfo;
	m_pvwGigE->GetCameraInfo( szName, &cameraInfo );


	CDlgForceIP dlgForceIP;
	dlgForceIP.GetIP( &dwIP );
	dlgForceIP.GetSubnet( &dwSubnet );
	dlgForceIP.GetGateway( &dwGateway );
	if ( IDCANCEL == dlgForceIP.DoModal() )
	{
		return;
	}

	CString strMAC( cameraInfo.mac );
	const int MAC_LEN = 6;
	unsigned __int8 mac[128] = {0,};
	int nToken = 0;
	for ( int i = 0; i < MAC_LEN; i ++ )
	{
		CString strMAC1 = strMAC.Tokenize( _T(":"), nToken );

		_stscanf_s( strMAC1, _T("%x"), &mac[i] );
	}

	m_pvwGigE->SetForceIP( (PBYTE)mac, dwIP, dwSubnet, dwGateway );

	// Call Discovery func
	OnBnClickedButtonDiscovery();
}

void CVwGigEDemoGigEDiscoveryDlg::OnLvnItemchangedListCamera(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

	int nSelectedItem = pNMLV->iItem;

	if ( -1 == nSelectedItem )
	{
		return;
	}

	GetDlgItem( IDC_BUTTON_OPEN )->EnableWindow( TRUE );
	GetDlgItem( IDC_BTN_FORCEIP )->EnableWindow( TRUE );

	*pResult = 0;
}