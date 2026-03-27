#pragma once

class CDlgForceIP : public CDialog
{
	DECLARE_DYNAMIC(CDlgForceIP)

public:
	CDlgForceIP(CWnd* pParent = NULL);
	virtual ~CDlgForceIP();

	enum { IDD = IDD_DLG_FORCEIP };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);

	DECLARE_MESSAGE_MAP()
	afx_msg void OnBtnOK();
	afx_msg void OnBtnCancel();

private:
	CIPAddressCtrl	m_IPAddress;
	CIPAddressCtrl	m_Subnet;
	CIPAddressCtrl	m_Gateway;

	DWORD*			m_pdwIP;
	DWORD*			m_pdwSubnet;
	DWORD*			m_pdwGateway;

public:
	virtual BOOL OnInitDialog();
	void GetIP( DWORD* pdwIP );
	void GetSubnet( DWORD* pdwSubnet );
	void GetGateway( DWORD* pdwGateway );
};
