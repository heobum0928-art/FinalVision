#pragma once

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

#include "VwCXP.h"
#include "VwCXPBase.h"


namespace VWSDK
{

		class VwCXPCamera;

		class CLASS_IMPORTEXPORT VwCXPGrabberBoard :public VwCXPBase
		{
		private:
			IF_HANDLE					m_hInterface;
			CNodeMapRef *				m_pGrabberMap;
			DEV_HANDLE					m_hDevice;

			VwCXPCamera*				m_pCamera;

		public:
			char*						m_pszDeviceID;


			DEVICE_ACCESS_STATUS		m_eDeviceInfo_AccessStatus;
			char*						m_pszDeviceInfo_Displayname;
			char*						m_pszDeviceInfo_Tltype;
			char*						m_pszDeviceInfo_Model;
			char*						m_pszDeviceInfo_Vendor;
			char*						m_pszDeviceInfo_ID;



		public:
			VwCXPGrabberBoard(IF_HANDLE hInterface);

			virtual ~VwCXPGrabberBoard(void);

			RESULT						OpenCamera();
			RESULT						CloseCamera();

			virtual RESULT				SetCustomCommandEx(IN char* _pszCommand, IN char* _pszArg, IN size_t _cbArgSize, IN BOOL _bSyncCMD = TRUE);
			virtual RESULT				SetCustomCommand(IN char* _pszCommand, IN char* _pszArg, IN BOOL _bSyncCMD = TRUE);
			virtual RESULT				GetCustomCommand(IN char* _pszCommand,
				OUT char* _pszArg,
				INOUT size_t* _pArgSize,
				IN int _nCmdType = GET_CUSTOM_COMMAND_VALUE);

			virtual RESULT				GetPropertyInfo(IN const char* _pszCommand, OUT PROPERTY* _pPropInfo);
			virtual RESULT				GetPropertyInfo(IN int _nIndex, OUT PROPERTY* _pPropInfo);
			virtual RESULT				GetPropertyCount(OUT int* _pCount);
			virtual CNodeMapRef*		GetNodeMapRef()const { return m_pGrabberMap; }

			/**
			* @Method	VwCXPGrabberBoard::DeviceID
			* @brief	Returns the ID of the Camera that is discovered.
			* @return	char*
			* @remark	Return value - ID of the discovered Camera.
			* @SeeAlso
			**/
			char*						DeviceID() const { return m_pszDeviceInfo_ID; }


			/**
			* @Method	VwCXPGrabberBoard::Displayname
			* @brief	Returns the model name of the camera that is discovered.
			* @return	char*
			* @remark	Return value - char* : Display name of the discovered Camera.
			* @SeeAlso
			**/
			char*						Displayname() const { return m_pszDeviceInfo_Displayname; }

			/**
			* @Method	VwCXPGrabberBoard::Model
			* @brief	Returns the model name of the camera that is discovered.
			* @return	char*
			* @remark	Return value - char* : Model name of the discovered Camera
			* @SeeAlso
			**/
			char*						Model() const { return m_pszDeviceInfo_Model; }

			/**
			* @Method	VwCXPGrabberBoard::Vendor
			* @brief	Returns the vendor name of the Camera that is discovered.
			* @return	char*
			* @remark	Return value - char* : Vendor name of the discovered Camera
			* @SeeAlso
			**/
			char*						Vendor() const { return m_pszDeviceInfo_Vendor; }
			/**
			* @Method	VwCXPGrabberBoard::GetCamera
			* @brief	Returns the VwCXPCamera object of the opened Camera.
			* @return	VwCXPCamera*
			* @remark	Return value - VwCXPCamera* : Address of VwCXPCamera object
			* @SeeAlso
			**/
			VwCXPCamera*				GetCamera() const { return m_pCamera; }

			RESULT						GetHandle(OUT DEV_HANDLE* pHandle);
			RESULT						GetInterfaceHandle(OUT IF_HANDLE* pHandle);

		};

}//namespace VWSDK