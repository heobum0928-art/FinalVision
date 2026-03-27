#pragma once

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

#include "VwCXP.h"
#include "VwCXPBase.h"


using namespace GenApi;


namespace VWSDK
{


	class VwCXPGrabberBoard;
	class VwCXPStream;

	class CLASS_IMPORTEXPORT VwCXPCamera :public VwCXPBase
	{
	private:
		PORT_HANDLE				m_hCxpCamera;
		DEV_HANDLE				m_hDevice;
		DS_HANDLE				m_hDatastream;

		HANDLE					m_hThreadGrab;


		uint32_t				m_unNumDataStreams;

		CNodeMapRef *			m_pCameraMap;
		BOOL					m_bGrabbing;

		VwCXPStream*			m_pCxpStream;
		VwCXPGrabberBoard*		m_pCxpGrabberBoard;

		int						EvalPixelSize(IN char* pszPixelFormat);
		RESULT					AcquisitionPrepare(size_t nBufferCount);

		RESULT					Open(size_t nBufferCount = 2);
		RESULT					Close();

	public:
		VwCXPCamera(DEV_HANDLE a_hDevice, VwCXPGrabberBoard* a_pGrabber);
		virtual ~VwCXPCamera();



		virtual RESULT			SetCustomCommandEx(IN char* _pszCommand, IN char* _pszArg, IN size_t cbArgSize, IN BOOL _bSyncCMD = TRUE);
		virtual	RESULT			SetCustomCommand(IN char* _pszCommand, IN char* _pszArg, IN BOOL bSyncCMD = TRUE);
		virtual	RESULT			GetCustomCommand(IN char* _pszCommand,
			OUT char* _pszArg,
			INOUT size_t* _pArgSize,
			IN int _nCmdType = GET_CUSTOM_COMMAND_VALUE);

		virtual	RESULT			GetPropertyCount(OUT int* _pCount);
		virtual	RESULT			GetPropertyInfo(IN int _nIndex, OUT PROPERTY* _pPropInfo);
		virtual	RESULT			GetPropertyInfo(IN const char* _pszCommand, OUT PROPERTY* _pPropInfo);
		virtual CNodeMapRef*	GetNodeMapRef()const { return m_pCameraMap; }

		RESULT					AcquisitionStart(size_t nBufferCount = 2);
		RESULT					AcquisitionStop();

		VwCXPStream*			GetStream() const { return m_pCxpStream; }
		VwCXPGrabberBoard*		GetGrabberBoard() const { return m_pCxpGrabberBoard; }
		RESULT					GetHandle(OUT PORT_HANDLE* pHandle) { *pHandle = m_hCxpCamera; return RESULT_SUCCESS; }

	};
}