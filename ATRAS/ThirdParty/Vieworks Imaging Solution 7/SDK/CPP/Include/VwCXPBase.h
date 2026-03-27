#pragma once


#include "VwCXP.Global.h"

#include "GenTL_v1_3.h"

#ifdef USE_GENICAM_3_0_NAMESPACE
#include "GenApi/GenApiNamespace.h"
#else
#define GENAPI_NAMESPACE GenApi
#endif

#define PURE                    = 0

#ifdef _USRDLL
#define CLASS_IMPORTEXPORT __declspec(dllexport)
#else if
#define CLASS_IMPORTEXPORT __declspec(dllimport)
#endif

namespace GENAPI_NAMESPACE
{
	class CNodeMapRef;
	struct INode;
}

enum VwEAccessMode
{
	Vw_NI,        //!< Not implemented
	Vw_NA,        //!< Not available
	Vw_WO,        //!< Write Only
	Vw_RO,        //!< Read Only
	Vw_RW,        //!< Read and Write
	Vw_UndefinedAccesMode,    //!< Object is not yetinitialized
	Vw_CycleDetectAccesMode   //!< used internally for AccessMode cycle detection
};

using namespace std;
using namespace GenICam;
using namespace GenICam::Client;
using namespace GENAPI_NAMESPACE;
using namespace std;


namespace VWSDK
{

	class VwCXPPort;


	class CLASS_IMPORTEXPORT VwCXPBase
	{
	public:


	private:
		PROPERTY_LIST			m_listProperty;
		VwCXPPort*				m_pPortImpl;
		PORT_HANDLE				m_hPort;

	public:
		VwCXPBase(void);
		virtual ~VwCXPBase(void);

		virtual CNodeMapRef*			GetNodeMapRef(void)const PURE;

		/**
		* @Method	VwCXPBase::SetCustomCommand
		* @brief	Sets a specific value to a specific property.
		* @return	RESULT
		* @param	IN char * _pszCommand : XML node name to be modified.
		* @param	IN char * _pszArg : Desired value. IInteger: ex) char* cpArg = ¡°1000¡±; / IFloat: ex) char* cpArg = ¡°50.45¡±; / IEnumeration: ex) char* cpArg = "Mono8¡±; / IBoolean: ex) char* cpArg = ¡°1¡±; // TRUE / IString: ex) char* cpArg = ¡°Test¡±; / ICommand: Do not use cpArg argument. You can use only two arguments.
		* @param	IN size_t _cbArgSize : The size of _pszArg variable.
		* @param	IN BOOL _bSyncCMD : Default value is TRUE. If TRUE, this function will wait for the camera to finish the processing. If FALSE, this function will be finished without waiting for the camera to finish the processing after sending a command to the camera.
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			SetCustomCommandEx(IN char* _pszCommand, IN char* _pszArg, IN size_t _cbArgSize, IN BOOL _bSyncCMD = TRUE) PURE;

		/**
		* @Method	VwCXPBase::SetCustomCommand
		* @brief	Sets a specific value to a specific property.
		* @return	RESULT
		* @param	IN char * _pszCommand : XML node name to be modified.
		* @param	IN char * _pszArg : Desired value. IInteger: ex) char* cpArg = ¡°1000¡±; / IFloat: ex) char* cpArg = ¡°50.45¡±; / IEnumeration: ex) char* cpArg = "Mono8¡±; / IBoolean: ex) char* cpArg = ¡°1¡±; // TRUE / IString: ex) char* cpArg = ¡°Test¡±; / ICommand: Do not use cpArg argument. You can use only two arguments.
		* @param	IN BOOL _bSyncCMD : Default value is TRUE. If TRUE, this function will wait for the camera to finish the processing. If FALSE, this function will be finished without waiting for the camera to finish the processing after sending a command to the camera.
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			SetCustomCommand(IN char* _pszCommand, IN char* _pszArg, IN BOOL _bSyncCMD = TRUE) PURE;
		/**
		* @Method	VwCXPBase::GetCustomCommand
		* @brief	Retrieves the value of a specific property.
		* @return	RESULT
		* @param	IN char * _pszCommand : XML node name to retrieve the value.
		* @param	OUT char * _pszArg : Char. type pointer to transmit the acquired value. Refer to Parameters of CameraSetCustomCommand function for more information about the type of acquired values.
		* @param	INOUT size_t * _pArgSize : Size of the cpArg buffer.
		* @param	IN int _nCmdType : Acquired values as cpArg may vary according to nCmdType. If the last argument is not entered, GET_CUSTOM_COMMAND_VALUE will be selected by default. / GET_CUSTOM_COMMAND_VALUE Returns the current setting value of the XML node corresponding to cpCommand. / GET_CUSTOM_COMMAND_NUM Returns the number of items available if the XML node is IEnumeration type. / GET_CUSTOM_COMMAND_MIN Returns the minimum possible value if the XML node corresponding to cpCommand is IInteger or IFloat type. / GET_CUSTOM_COMMAND_MAX Returns the maximum possible value if the XML node corresponding to cpCommand is IInteger or IFloat type. / GET_CUSTOM_COMMAND_INC Returns incremental value at once if the XML node corresponding to cpCommand is IInteger or IFloat type. / GET_CUSTOM_COMMAND_INDEX Returns an item corresponding to GET_CUSTOM_COMMAND_INDEX from the available items if the XML node corresponding to cpCommand is IEnumeration type.
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			GetCustomCommand(IN char* _pszCommand,
			OUT char* _pszArg,
			INOUT size_t* _pArgSize,
			IN int _nCmdType = GET_CUSTOM_COMMAND_VALUE)PURE;
		/**
		* @Method	VwCXPBase::GetPropertyInfo
		* @brief	Retrieves attribute information about a specific property.
		* @return	RESULT
		* @param	IN const char * _pszCommand : Name of a specific property.
		* @param	OUT PROPERTY * _pPropInfo : Attribute information. See chapter 9 Structure.
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			GetPropertyInfo(IN const char* _pszCommand, OUT PROPERTY* _pPropInfo)PURE;
		/**
		* @Method	VwCXPBase::GetPropertyInfo
		* @brief	Retrieves attribute information about a specific property.
		* @return	RESULT
		* @param	IN int _nIndex : Index of a specific property.
		* @param	OUT PROPERTY * _pPropInfo : Attribute information. See chapter 9 Structure.
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			GetPropertyInfo(IN int _nIndex, OUT PROPERTY* _pPropInfo)PURE;
		/**
		* @Method	VwCXPBase::GetPropertyCount
		* @brief	Retrieves the total number of all properties.
		* @return	RESULT
		* @param	OUT int * _pCount : Index of a specific property
		* @remark
		* @SeeAlso
		**/
		virtual RESULT			GetPropertyCount(OUT int* _pCount)PURE;

		virtual RESULT					ReadData(void* _pBuffer, int64_t _n64Address, int64_t _n64Length);
		virtual RESULT					WriteData(const void* _pBuffer, int64_t _n64Address, int64_t _n64Length);
		virtual VwEAccessMode			GetAccessMode(void)const;

		virtual BOOL					IsBigEndian();
		virtual BOOL					IsLittleEndian();


	protected:
		RESULT							_SetCustomCommand(IN CNodeMapRef* _pNodeMap, IN char* _pszCommand, IN char* _pszArg, IN size_t _cbArgSize, IN BOOL _bSyncCMD = TRUE);
		RESULT							_SetCustomCommand(IN CNodeMapRef* _pNodeMap, IN char* _pszCommand, IN char* _pszArg, IN BOOL _bSyncCMD = TRUE);
		RESULT							_GetCustomCommand(IN CNodeMapRef* _pNodeMap,
			IN char* _pszCommand,
			OUT char* _pszArg,
			INOUT size_t* _pArgSize,
			IN int _nCmdType = GET_CUSTOM_COMMAND_VALUE);

		void							_ftoa(double dInput, char* chRet, int digit);
		RESULT							_LoadPropertyList(IN CNodeMapRef* _pNodeMap);
		void							_RetrieveNode(IN CNodeMapRef* _pNodeMap, INode* _pNode);

		RESULT							_GetPropertyCount(OUT int* _pCount);
		RESULT							_GetPropertyInfo(IN int _nIndex, OUT PROPERTY* _pPropInfo);
		RESULT							_GetPropertyInfo(IN CNodeMapRef* _pNodeMap, IN const char* _pszCommand, OUT PROPERTY* _pPropInfo);

		virtual CNodeMapRef*			_LoadNodeMap(PORT_HANDLE hPort);
		int								_GetPollingTime(IN CNodeMapRef* _pNodeMap, char* strNodeName);
		char*							_GetNodeItemValue(IN CNodeMapRef* _pNodeMap, char* strNodeName, char* strItemName);
	};

}//namespace VWSDK