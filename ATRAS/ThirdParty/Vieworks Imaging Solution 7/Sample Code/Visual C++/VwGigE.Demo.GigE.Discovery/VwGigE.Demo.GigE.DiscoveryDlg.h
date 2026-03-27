// VwGigE.Demo.GigE.DiscoveryDlg.h : header file
//

#pragma once
#include "afxcmn.h"


class CVwGigEDemoGigEDiscoveryCameraDlg;

namespace VWSDK
{
	class VwGigE;
}

// CVwGigEDemoGigEDiscoveryDlg dialog
class CVwGigEDemoGigEDiscoveryDlg : public CDialog
{
// Construction
public:
	CVwGigEDemoGigEDiscoveryDlg(CWnd* pParent = NULL);	// standard constructor
	virtual ~CVwGigEDemoGigEDiscoveryDlg();

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

	DECLARE_MESSAGE_MAP()
public:
	void AddListCameraInfo();

protected:
	VWSDK::VwGigE* m_pvwGigE;
public:
	CListCtrl m_lstCamera;
	CList<CVwGigEDemoGigEDiscoveryCameraDlg*> m_plistCameraDlg;
	
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedCancel();
	afx_msg void OnBnClickedBtnForceip();

	afx_msg void OnLvnItemchangedListCamera(NMHDR *pNMHDR, LRESULT *pResult);
};
