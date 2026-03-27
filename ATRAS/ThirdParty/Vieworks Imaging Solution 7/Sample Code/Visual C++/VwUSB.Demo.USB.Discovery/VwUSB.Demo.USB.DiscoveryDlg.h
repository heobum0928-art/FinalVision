// VwUSB.Demo.USB.DiscoveryDlg.h : header file
//

#pragma once
#include "afxcmn.h"

class CVwUSBDemoDiscoveryCameraDlg;
namespace VWSDK
{
	class VwUSB;
}

// CVwUSBDemoUSBDiscoveryDlg dialog
class CUSBDemoUSBDiscoveryDlg : public CDialog
{
// Construction
public:
	CUSBDemoUSBDiscoveryDlg(CWnd* pParent = NULL);	// standard constructor
	virtual ~CUSBDemoUSBDiscoveryDlg();

// Dialog Data
	enum { IDD = IDD_VWUSBDEMOUSBDISCOVERY_DIALOG };

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
	VWSDK::VwUSB* m_pvwUSB;
public:
	CListCtrl m_lstCamera;
	CList<CVwUSBDemoDiscoveryCameraDlg*> m_plistCameraDlg;
	
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedCancel();

	afx_msg void OnLvnItemchangedListCamera(NMHDR *pNMHDR, LRESULT *pResult);
};
