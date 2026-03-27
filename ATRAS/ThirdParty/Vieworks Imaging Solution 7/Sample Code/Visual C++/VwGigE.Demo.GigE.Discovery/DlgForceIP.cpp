#include "stdafx.h"
#include "VwGigE.Demo.GigE.Discovery.h"
#include "DlgForceIP.h"


IMPLEMENT_DYNAMIC(CDlgForceIP, CDialog)

CDlgForceIP::CDlgForceIP(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgForceIP::IDD, pParent)
{
}

CDlgForceIP::~CDlgForceIP()
{
}

void CDlgForceIP::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_IPADDRESS, m_IPAddress);
	DDX_Control(pDX, IDC_SUBNET, m_Subnet);
	DDX_Control(pDX, IDC_GATEWAY, m_Gateway);
}


BEGIN_MESSAGE_MAP(CDlgForceIP, CDialog)
	ON_BN_CLICKED(IDOK, &CDlgForceIP::OnBtnOK)
	ON_BN_CLICKED(IDCANCEL, &CDlgForceIP::OnBtnCancel)
END_MESSAGE_MAP()


BOOL CDlgForceIP::OnInitDialog()
{
	CDialog::OnInitDialog();

	m_IPAddress.SetAddress( 169, 254, 0, 1 );
	m_Subnet.SetAddress( 255, 255, 0, 0 );
	m_Gateway.SetAddress( 0, 0, 0, 0 );

	return TRUE;  // return TRUE unless you set the focus to a control
}


void CDlgForceIP::OnBtnOK()
{
	UpdateData();
	DWORD dwIP = 0;
	DWORD dwSubnet = 0;
	DWORD dwGateway = 0;

	m_IPAddress.GetAddress( dwIP );
	m_Subnet.GetAddress( dwSubnet );
	m_Gateway.GetAddress( dwGateway );

	*m_pdwIP = dwIP;
	*m_pdwSubnet = dwSubnet;
	*m_pdwGateway = dwGateway;

	OnOK();
}

void CDlgForceIP::OnBtnCancel()
{
	OnCancel();
}

void CDlgForceIP::GetIP( DWORD* pdwIP )
{
	if ( NULL != pdwIP )
	{
		m_pdwIP = pdwIP;
	}
}

void CDlgForceIP::GetSubnet( DWORD* pdwSubnet )
{
	if ( NULL != pdwSubnet )
	{
		m_pdwSubnet = pdwSubnet;
	}
}

void CDlgForceIP::GetGateway( DWORD* pdwGateway )
{
	if ( NULL != pdwGateway )
	{
		m_pdwGateway = pdwGateway;
	}
}