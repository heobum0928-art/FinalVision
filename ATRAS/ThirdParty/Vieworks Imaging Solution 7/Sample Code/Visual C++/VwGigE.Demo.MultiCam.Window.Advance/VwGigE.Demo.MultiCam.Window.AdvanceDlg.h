
#pragma once
#include <list>
#include "afxwin.h"
#include "VwSDK.h"
#define DF_DEVICE_COUNT 4


namespace VWSDK
{
	class VwGigE;
	class VwCamera;
	struct OBJECT_INFO;
	struct IMAGE_INFO;
	enum PIXEL_FORMAT;
	struct DISCONNECT_INFO;
	enum RESULT;
	enum GET_CUSTOM_COMMAND;
}

class CDeviceState
{
public:
	CDeviceState()
	{
		m_bOpen = FALSE;
		m_bClose = FALSE;
		m_bImageBuffer = FALSE;
		m_nBuffer = 0;
		m_bGrab = FALSE;
		m_nSnapBuffer = 0;
		m_bSnapBuffer = FALSE;
		m_bSnap = FALSE;
		m_bAbort = FALSE;
		m_bPixelFormat = FALSE;
		m_bPixelSize = FALSE;
		m_nWidth = 0;
		m_bWidth = 0;
		m_nHeight = 0;
		m_bHeight = 0;
	};

public:
	BOOL m_bOpen;
	BOOL m_bClose;
	BOOL m_bImageBuffer;
	int	m_nBuffer;
	CString m_strVendorName;
	CString m_strModelName;
	CString m_strDeviceVersion;
	CString m_strDeviceID;
	BOOL m_bGrab;
	int m_nSnapBuffer;
	BOOL m_bSnapBuffer;
	BOOL m_bSnap;
	BOOL m_bAbort;
	CString m_strPixelFormat;
	BOOL m_bPixelFormat;
	CString m_strPixelSize;
	BOOL m_bPixelSize;
	int m_nWidth;
	BOOL m_bWidth;
	int m_nHeight;
	BOOL m_bHeight;

};

class CVwGigEDemoMultiCamWindowAdvanceDlg : public CDialog
{
public:
	CVwGigEDemoMultiCamWindowAdvanceDlg(CWnd* pParent = NULL);
	~CVwGigEDemoMultiCamWindowAdvanceDlg();

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
		
	afx_msg void OnBnClickedButtonStop1();
	afx_msg void OnBnClickedButtonClose1();

	afx_msg void OnEnChangeEditImageBuffer();
	afx_msg void OnBnClickedOk();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedBtnSnap();

	afx_msg void OnCbnSelchangeComboPixelFormat();
	afx_msg void OnCbnSelchangeComboPixelSize();
	afx_msg void OnClose();
	afx_msg void RadioCtrl( UINT ID );

public:
	static void	GetImageEvent1(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event
	static void	GetImageEvent2(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event
	static void	GetImageEvent3(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event
	static void	GetImageEvent4(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo); //Image capture event

	void DrawImage( VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo, int nIndex );
	void GetDeviceInfo( IN int nIndex, OUT CString& strVenderName, OUT CString& strModelName, OUT CString& strDeviceVersion, OUT CString& strDeviceID );

	void SetUIResolutionInfo( IN UINT tempwidth, IN UINT tempheight, IN VWSDK::PIXEL_FORMAT ePixelFormat, IN UINT unPixelSize );
	int GetPixelTypeIndex( IN CString strType );
	CString GetPixelTypeStr( IN int nIndex );
	void MakeUnPackedBuffer();
	CString GetPixelFormatFromEnum( VWSDK::PIXEL_FORMAT pixelFormat );

	BOOL SetWidthCamera( int nWidth );
	BOOL SetHeightCamera( int nHeight );

	void SaveDeviceState( int nDeviceNum );
	void UpdateDeviceState( int nDeviceNum );

	int GetPixelSizeIndex( int nPixelSize );
	void UpdatePixelFormat();

	virtual BOOL PreTranslateMessage(MSG* pMsg);
	
	VWSDK::RESULT GetCustomCommand(VWSDK::VwCamera* hCamera, char* cpFeatureName, UINT* unValue, VWSDK::GET_CUSTOM_COMMAND eCmdType = VWSDK::GET_CUSTOM_COMMAND_VALUE);

public:
	VWSDK::VwGigE*			 m_pvwGigE;
	BITMAPINFO*				 m_pBmpInfo[DF_DEVICE_COUNT];									  //Bitmap object : 1
	CTreeCtrl				 m_deviceListTree;
	HDC						 m_hdc1;

	UINT					 m_imagecontrolHeight;
	UINT					 m_imagecontrolWidth;
	UINT					 m_grabbedimagecount;
	UINT					 m_imagebuffernumber;

	VWSDK::VwCamera*		 m_pCamera;
	VWSDK::VwCamera*		 m_lstCamera[DF_DEVICE_COUNT];
	CImageList				 m_imlDevices;
	VWSDK::OBJECT_INFO*		 m_pobjectInfo[DF_DEVICE_COUNT];

	BYTE*					 m_pUnpackedImage[DF_DEVICE_COUNT];

	std::list<LARGE_INTEGER>		 m_imageTimeStamps[DF_DEVICE_COUNT];
	double					 m_curFPS[ DF_DEVICE_COUNT ];
	LARGE_INTEGER			 m_liLastDisplayTime[DF_DEVICE_COUNT];
	LARGE_INTEGER			 m_liFreq;
	DWORD					 m_nMinInterFrameTime;

	CComboBox				m_cbxPixelFormat;
	CComboBox				m_cbxPixelSize;
	int						m_nWidth;
	int						m_nHeight;

	int						m_radioDeviceOld;
	UINT					m_radioDevice;
	CDeviceState			m_deviceState[DF_DEVICE_COUNT];




};
