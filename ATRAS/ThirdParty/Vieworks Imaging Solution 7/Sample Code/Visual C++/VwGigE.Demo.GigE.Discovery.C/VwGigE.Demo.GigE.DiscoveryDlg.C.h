// VwGigE.Demo.GigE.DiscoveryDlg.h : header file
//

#pragma once
#include "afxcmn.h"
#include "VwGigE.API.h"

class CVwGigEDemoGigEDiscoveryCameraCDlg;

// CVwGigEDemoGigEDiscoveryCDlg dialog
class CVwGigEDemoGigEDiscoveryCDlg : public CDialog
{
// Construction
public:
	CVwGigEDemoGigEDiscoveryCDlg(CWnd* pParent = NULL);	// standard constructor
	virtual ~CVwGigEDemoGigEDiscoveryCDlg();

// Dialog Data
	enum { IDD = IDD_VWGIGEDEMOGIGEDISCOVERY_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL OnInitDialog();

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBnClickedButtonDiscovery();
	afx_msg void OnBnClickedButtonOpen();
	afx_msg void OnCustomDraw( NMHDR* pNMHDR, LRESULT* pResult );
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedCancel();
	afx_msg void OnBnClickedBtnForceip();

	DECLARE_MESSAGE_MAP()
public:
	void AddListCameraInfo();

protected:
	VWSDK::VWGIGE_HANDLE m_pvwGigE;
public:
	CListCtrl m_lstCamera;
	CList<CVwGigEDemoGigEDiscoveryCameraCDlg*> m_plistCameraDlg;
	

	afx_msg void OnLvnItemchangedListCamera(NMHDR *pNMHDR, LRESULT *pResult);

};
