
// VwCXP.Demo.MultiCam.Window.AdvanceDlg.h : header file
//

#pragma once
#include "afxwin.h"
#include <list>
#define		MAX_CAMERA_COUNT	4


namespace VWSDK
{
	class VwCXP;
	class VwCXPGrabberBoard;
	class VwCXPCamera;
	struct OBJECT_INFO;
	struct IMAGE_INFO;
}


// CVwCXPDemoMultiCamWindowAdvanceDlg dialog
class CVwCXPDemoMultiCamWindowAdvanceDlg : public CDialog
{
// Construction
public:
	CVwCXPDemoMultiCamWindowAdvanceDlg(CWnd* pParent = NULL);	// standard constructor
	~CVwCXPDemoMultiCamWindowAdvanceDlg();

// Dialog Data
	enum { IDD = IDD_VWCXPDEMOMULTICAMWINDOWADVANCE_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonDiscovery();
	afx_msg void OnDestroy();
	afx_msg void OnLbnSelchangeListDeviceList();
	afx_msg void OnBnClickedButtonOpen();
	afx_msg void OnBnClickedButtonClose();
	afx_msg void OnBnClickedButtonStart();
	afx_msg void OnBnClickedButtonStop();

	virtual BOOL PreTranslateMessage(MSG* pMsg);

public:
	
	static void					GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event
	static DWORD WINAPI			GrabContinuousThread (LPVOID _pOwner);
	UINT						ThrGrabContinuous();
	void						Discovery();
	void						InitialState(BOOL bState );
	void						PlayState(BOOL bState);
	void						OpenState(BOOL bState);

	void						SetUIWidth(VWSDK::VwCXPCamera* pCamera);
	void						SetUIHeight(VWSDK::VwCXPCamera* pCamera);
	void						SetUIPixelFormat(VWSDK::VwCXPCamera* pCamera);
	void						SetUILinkconfig(VWSDK::VwCXPCamera* pCamera);
	void						SetUITestPattern(VWSDK::VwCXPCamera* pCamera);

	VWSDK::VwCXP*				m_pCXP;
	CListBox					m_listctrlDevice;
	VWSDK::VwCXPGrabberBoard*	m_pSelectedGrabber;

	HANDLE						m_hThreadGrab[MAX_CAMERA_COUNT];
	int							m_nSelectedIdx;

	float						m_fCurrentFPS;

	HANDLE						m_hImgRcv;

	BOOL						m_bUseEventCallback;
	unsigned char*				m_pSrcBuf;

private:
	BOOL						m_bGrabbing;
	HDC							m_hDC;
	CDC*						m_pDC;
	int							m_nPictualWidth;
	int							m_nPictualHeight;

	
public:
	CComboBox					m_comboTestPattern;
	CComboBox					m_comboPixelformat;
	CComboBox					m_comboLinkconfig;

	CEdit						m_editboxWidth;
	CEdit						m_editboxHeight;
	
	std::list<LARGE_INTEGER>	m_imageTimeStamps;


	


	afx_msg void OnCbnSelchangeComboPixelformat();
	afx_msg void OnEnChangeEditWidth();
	afx_msg void OnEnChangeEditHeight();
	afx_msg void OnCbnSelchangeComboLinkconfig();
	afx_msg void OnSelchangeComboTestPattern();

	afx_msg LRESULT OnUpdateFPS(WPARAM wParam, LPARAM lParam);

	
};

void ConvertPixelFormat( char* a_pszPixelFormat, BYTE* a_pDest, BYTE* a_pSource, int a_nWidth, int a_nHeight );
