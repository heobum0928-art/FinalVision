#pragma once

#include <list>
#include "VwGigE.API.h"

class VwCamera;
struct OBJECT_INFO;
struct IMAGE_INFO;
enum PIXEL_FORMAT;

class CVwGigEDemoGigEDiscoveryCameraCDlg : public CDialog
{
	DECLARE_DYNAMIC(CVwGigEDemoGigEDiscoveryCameraCDlg)

public:
	CVwGigEDemoGigEDiscoveryCameraCDlg( VWSDK::VWGIGE_HANDLE pvwGigE, char* pName, CWnd* pParent = NULL);
	virtual ~CVwGigEDemoGigEDiscoveryCameraCDlg();

	enum { IDD = IDD_DLG_CAMERA };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonGrab();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedButtonSnap();
	afx_msg void OnBnClickedButtonAbort();
	afx_msg void OnBnClickedButtonCloseCamera();
	afx_msg void OnClose();

public:
	static void				 GetImageEvent1		   (VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event
	void SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat );
	BOOL OpenCamera();

	void MakeUnPackedBuffer();
	static void		DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo );	//Image capture event
	static void		Disconnect( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnect );
protected:
	char					m_szName[128];
	VWSDK::HCAMERA			m_pCamera;
	VWSDK::VWGIGE_HANDLE	m_pvwGigE;
	VWSDK::OBJECT_INFO*		m_pObjectInfo;

	BYTE*					 m_pUnpackedImage;

	std::list<LARGE_INTEGER>		 m_imageTimeStamps;
	double					 m_curFPS;
	LARGE_INTEGER			 m_liLastDisplayTime;
	LARGE_INTEGER			 m_liFreq;
	DWORD					 m_nMinInterFrameTime;
	BITMAPINFO*				 m_pBmpInfo1;									  //Bitmap object : 1
	HDC						 m_hdc1;
	UINT					 m_imagecontrolHeight;
	UINT					 m_imagecontrolWidth;
	UINT					 m_grabbedimagecount;
	UINT					 m_imagebuffernumber;
	
public:
	VWSDK::RESULT GetCustomCommand(VWSDK::HCAMERA hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);
};
