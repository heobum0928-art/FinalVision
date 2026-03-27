#pragma once

#include "VwCXPBase.h"

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

namespace VWSDK
{

	class VwCXPGrabberBoard;
	class VwCXPCamera;

	class CLASS_IMPORTEXPORT VwCXPStream : public VwCXPBase
	{
	private:
		CNodeMapRef *			m_pDataStreamMap;
		DS_HANDLE				m_hDataStream;
		EVENT_HANDLE			m_hEventNewBuffer;
		EVENT_HANDLE			m_hEventError;


		list<BUFFER_HANDLE>		m_listBufferHandle;
		void*					m_NewImageEventData[2];
		void*					m_EventDataTmp[2];


		size_t					m_BufferWidth;
		size_t					m_BufferHeight;
		size_t					m_BufferSize;
		size_t					m_BufferCount;

		bool					m_bGrabbThreadRunning;

		ImageCallbackFn*		m_pImageCallbackFn;
		PVOID					m_pParent;

		HANDLE					m_pThrGrab;

		RESULT					Open(IN int _nDevWidth, IN int _nDevHeight, IN int _pixelSize, IN BOOL _bAreascan = TRUE);
		RESULT					Close();

	public:
		VwCXPStream(DS_HANDLE _hDs, IN int _nDevWidth, IN int _nDevHeight, IN int _pixelSize, IN size_t _nBufferCount = 2, IN BOOL _bAreascan = TRUE);
		virtual ~VwCXPStream();

		virtual RESULT			SetCustomCommandEx(IN char* _pszCommand, IN char* _pszArg, IN size_t _cbArgSize, IN BOOL _bSyncCMD = TRUE);
		virtual RESULT			SetCustomCommand(IN char* _pszCommand, IN char* _pszArg, IN BOOL _bSyncCMD = TRUE);
		virtual RESULT			GetCustomCommand(IN char* _pszCommand,
			OUT char* _pszArg,
			INOUT size_t* _pArgSize,
			IN int _nCmdType = GET_CUSTOM_COMMAND_VALUE);

		virtual RESULT			GetPropertyInfo(IN const char* _pszCommand, OUT PROPERTY* _pPropInfo);
		virtual RESULT			GetPropertyInfo(IN int _nIndex, OUT PROPERTY* _pPropInfo);
		virtual RESULT			GetPropertyCount(OUT int* _pCount);
		virtual CNodeMapRef*	GetNodeMapRef(void)const { return m_pDataStreamMap; }


		unsigned char*			GrabImage();
		void					QueueBuffer();

		void					StreamStart(BOOL bUseImageCallback = TRUE);
		void					StreamStop();
		RESULT					ChangeBuffer(IN int _nDevWidth, IN int _nDevHeight, IN int _pixelSize, IN size_t _nBufferCount = 2, IN BOOL _bAreascan = TRUE);

		void					SetImageCallback(ImageCallbackFn* pImageCallbackFn, void* pParent)
		{	
			m_pImageCallbackFn = pImageCallbackFn;
			m_pParent = pParent;
		};

		/**
		* @Method	VwCXPStream::BufferWidth
		* @brief	Returns the width of the buffer.
		* @return	size_t
		* @remark	Return value - Width value of the buffer.
		* @SeeAlso
		**/
		size_t					BufferWidth() const { return m_BufferWidth; }
		/**
		* @Method	VwCXPStream::BufferHeight
		* @brief	Returns the height of the buffer.
		* @return	size_t
		* @remark	Return value - Height value of the buffer.
		* @SeeAlso
		**/
		size_t					BufferHeight() const { return m_BufferHeight; }
		/**
		* @Method	VwCXPStream::BufferSize
		* @brief	Returns the size of the buffer.
		* @return	size_t
		* @remark	Return value - Size of the buffer.
		* @SeeAlso
		**/
		size_t					BufferSize() const { return m_BufferSize; }
		/**
		* @Method	VwCXPStream::BufferCount
		* @brief	Returns the count of the buffer.
		* @return	size_t
		* @remark	Return value - Count of the buffer.
		* @SeeAlso
		**/
		size_t					BufferCount() const { return m_BufferCount; }

		RESULT					GetHandle(OUT DS_HANDLE* pHandle);

	private:
		RESULT					AllocateMemory(IN int _nWidth, IN int _nHeight, IN int _nPixelPerByte);
		RESULT					FlushQueue();

		bool					BufferReady();
		bool					BufferReady2();

		bool					FIFO();

		static unsigned int WINAPI ThrGrabCallbackThread(LPVOID me);
		UINT					ThrGrabContinuos();

	};
}//namespace VWSDK
