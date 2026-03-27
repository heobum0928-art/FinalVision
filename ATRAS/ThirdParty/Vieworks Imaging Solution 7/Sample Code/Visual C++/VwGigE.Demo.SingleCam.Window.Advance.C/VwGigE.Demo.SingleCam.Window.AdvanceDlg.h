#pragma once
#include <list>
#include "afxwin.h"
#include "VwGigE.API.h"

namespace VWSDK
{
	enum PIXEL_FORMAT;
}



class CVwGigEDemoSingleCamWindowAdvanceDlgC : public CDialog
{

public:
	CVwGigEDemoSingleCamWindowAdvanceDlgC(CWnd* pParent = NULL);
	~CVwGigEDemoSingleCamWindowAdvanceDlgC();


	enum { IDD = IDD_VWGIGEDEMOSINGLECAMWINDOWADVANCE_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);


	
protected:
	HICON m_hIcon;

	
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonOpen1();
	afx_msg void OnBnClickedButtonPlay1();
	//afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedButtonSearchDevice();
	afx_msg void OnBnClickedButtonStop1();
	afx_msg void OnBnClickedButtonClose1();

	afx_msg void OnEnChangeEditImageBuffer();
	afx_msg void OnBnClickedExit();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedBtnSnap();

	afx_msg void OnCbnSelchangeComboPixelFormat();
	afx_msg void OnCbnSelchangeComboPixelSize();

	afx_msg void OnClose();

public:
	
	static void		DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo );		 //Image capture event
	void GetDeviceInfo( IN int nIndex, OUT CString& strVenderName, OUT CString& strModelName, OUT CString& strDeviceVersion, OUT CString& strDeviceID );

	void SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat, IN UINT unPixelSize );
	int GetPixelTypeIndex( IN CString strType );
	CString GetPixelTypeStr( IN int nIndex );
	void MakeUnPackedBuffer();
	CString GetPixelFormatFromEnum( VWSDK::PIXEL_FORMAT pixelFormat );

	BOOL SetWidthCamera( int nWidth );
	BOOL SetHeightCamera( int nHeight );
	int GetPixelSizeIndex( int nPixelSize );
	
	VWSDK::RESULT GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

public:
	VWSDK::VWGIGE_HANDLE	 m_pvwGigE;
	BITMAPINFO*				 m_pBmpInfo1;									  //Bitmap object : 1
	CTreeCtrl				 m_deviceListTree;
	HDC						 m_hdc1;

	UINT					 m_imagecontrolHeight;
	UINT					 m_imagecontrolWidth;
	UINT					 m_grabbedimagecount;
	UINT					 m_imagebuffernumber;

	VWSDK::OBJECT_INFO*		m_pobjectInfo;

	VWSDK::HCAMERA			 m_pCamera;
	CImageList				 m_imlDevices;

	BYTE*					   m_pUnpackedImage;

	std::list<LARGE_INTEGER>		 m_imageTimeStamps;
	double					 m_curFPS;
	LARGE_INTEGER			 m_liLastDisplayTime;
	LARGE_INTEGER			 m_liFreq;
	DWORD					 m_nMinInterFrameTime;

	CComboBox				m_cbxPixelFormat;
	CComboBox				m_cbxPixelSize;

	BYTE*					 m_bpConvertPixelFormat;
	virtual BOOL PreTranslateMessage(MSG* pMsg);
};
