#pragma once

#include <list>
#include "VwSDK.h"

namespace VWSDK
{
	enum PIXEL_FORMAT;
	class VwUSB;
	class VwUSBCamera;
	struct OBJECT_INFO;
	struct IMAGE_INFO;
	struct DISCONNECT_INFO;
	enum RESULT;
};


class CVwUSBDemoDiscoveryCameraDlg : public CDialog
{
	DECLARE_DYNAMIC(CVwUSBDemoDiscoveryCameraDlg)

public:
	CVwUSBDemoDiscoveryCameraDlg( VWSDK::VwUSB* pUSB, char* pName, CWnd* pParent = NULL);
	virtual ~CVwUSBDemoDiscoveryCameraDlg();

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
	static void		DrawImage(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo);		//Image capture event
	static void		Disconnect(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnect);
	void			SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat );
	BOOL			OpenCamera();
	void			MakeUnPackedBuffer();
	CString			GetPixelFormatName( VWSDK::PIXEL_FORMAT ePixelFormat );
	int				GetPixelBitCount( VWSDK::PIXEL_FORMAT ePixelFormat );
	BOOL			ConvertPixelFormat( VWSDK::PIXEL_FORMAT ePixelFormat, BYTE* pDest, BYTE* pSource, int nWidth, int nHeight );
	
	VWSDK::RESULT GetCustomCommand(VWSDK::VwUSBCamera* phCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

protected:
	char						m_szName[128];
	VWSDK::VwUSBCamera*					m_pCamera;
	VWSDK::VwUSB*						m_pUSB;
	VWSDK::OBJECT_INFO*				m_pObjectInfo;

	BYTE*						m_pUnpackedImage;

	std::list<LARGE_INTEGER>	m_imageTimeStamps;
	double						m_curFPS;
	LARGE_INTEGER				m_liLastDisplayTime;
	LARGE_INTEGER				m_liFreq;
	DWORD						m_nMinInterFrameTime;
	BITMAPINFO*					m_pBmpInfo1;									  //Bitmap object : 1
	HDC							m_hdc1;
	UINT						m_imagecontrolHeight;
	UINT						m_imagecontrolWidth;
	UINT						m_grabbedimagecount;
	UINT						m_imagebuffernumber;

	BYTE*						m_bpConvertPixelFormat;
public:

};
