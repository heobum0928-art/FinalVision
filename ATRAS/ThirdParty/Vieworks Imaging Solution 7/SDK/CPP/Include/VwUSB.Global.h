 
//#pragma once

#ifndef VWGIGE_GLOBAL_H
#define VWGIGE_GLOBAL_H

#include "VwSDK.h"

#include "windows.h"

#include <vector>
using namespace std;

#define IN
#define OUT
#define INOUT

// Event types for GeneralEventCallback
#define VWDEF_EVENT_TYPE_FRAME_DROP					0X00000001
#define VWDEF_EVENT_TYPE_TEMP_EXCEED_LIMIT			0x00000002

#define __DLL_NAME__		( _T("VwUSB.V7.DLL") )


namespace VWSDK
{
	enum EventType
	{
		EVENT_TYPE_FRAME_DROP = 0x0000001,
		EVENT_TYPE_SENSOR_MASTER = 0x0000002,
		EVENT_TYPE_SENSOR_SLAVE = 0x0000003,
		EVENT_TYPE_LINK_SPEED = 0x0000004,
		EVENT_TYPE_TEMP_EXCEED_LIMIT = 0x0000005
	};


	enum STREAM_INFO
	{
		STREAM_INFO_NUM_OF_FRAMES_LOST,
		STREAM_INFO_NUM_PACKETS_MISSING
	};

	enum FILTER_TYPE
	{
		FILTER_TYPE_DIRECT = 1,
		FILTER_TYPE_WPCAB
	};


	struct SBRM_DATA
	{
		UINT64		ullVersion;
		UINT64		ullCapability_register;
		UINT64		ullConfiguration_register;
		UINT64		ullCommand_transfer_length;
		UINT64		ullAcknowledge_transfer_length;
		UINT64		ullNumber_of_stream_channels;
		UINT64		ullSirm_offset;
		UINT64		ullSirm_length;
		UINT64		ullEirm_address;
		UINT64		ullEirm_length;
		UINT64		ullIdc2_offset;
		UINT64		ullCurrent_speed;
		UINT64		ullReserved;
	};

#ifdef __cplusplus
	extern "C"
	{
#endif

		struct OBJECT_INFO
		{
			void*		pUserPointer;
			void*		pVwCamera;
			OBJECT_INFO()
			{
				pUserPointer = NULL;
				pVwCamera = NULL;
			}
		};

		struct IMAGE_INFO
		{
			VWSDK::RESULT			callbackResult;
			unsigned int			bufferIndex;
			VWSDK::PIXEL_FORMAT		pixelFormat;
			unsigned int			width;
			unsigned int			height;
			UINT64					unTimeStamp;
			UINT					ImageStatus;
			void*				pImage;
		};

		struct CAMERA_INFO_STRUCT
		{
			BOOL				error;
			VWSDK::RESULT				errorResult;
			unsigned int		index;
			char				name[256];
			char				vendor[128];
			char				model[128];
			char				ip[32];
			char				mac[32];

		public:
			CAMERA_INFO_STRUCT()
			{
				error = FALSE;
				errorResult = VWSDK::RESULT_ERROR;
				index = 0;
				// hCam = NULL;
				memset(name, 0, sizeof(name));
				memset(vendor, 0, sizeof(vendor));
				memset(model, 0, sizeof(model));
				memset(ip, 0, sizeof(ip));
				memset(mac, 0, sizeof(mac));
			}
		};

		struct INTERFACE_INFO_STRUCT
		{
			BOOL				error;
			VWSDK::RESULT				errorCause;
			unsigned int		index;
			char				name[256];
		};

		struct PIXEL_FORMAT_INFO
		{
			char				szName[256];
			VWSDK::PIXEL_FORMAT		pixelFormat;
			unsigned int		nSize;
		};

		struct DISCONNECT_INFO
		{
			int					nCurrHeartBeatTimeOut;	//	Current heartbeat timeout millisecond. 
			int					nTimeOutTryCount;		//	Heartbeat timeout try count.
		};


		typedef enum
		{
			CCP_NOCONTEXT = 0,		// 
			PRIVILEGE_EXCLUSIVE = 1,		//!< Exclusive control privilege (read-write)
			PRIVILEGE_CONTROL = 2,		//!< Control privilege (read-write)
			PRIVILEGE_MONITOR = 3		//!< Monitor privilege (read-only)
		} CCP_PRIVILEGE;


#ifdef __cplusplus
	}
#endif

	namespace VwGenICam
	{
		typedef unsigned __int64 uint64_t;
		typedef unsigned __int32 uint32_t;
		typedef void *      TL_HANDLE;
		typedef void *      IF_HANDLE;
		typedef void *      DEV_HANDLE;
		typedef void *      DS_HANDLE;
		typedef void *      PORT_HANDLE;
		typedef void *      EVENT_HANDLE;
		typedef void *      BUFFER_HANDLE;
		typedef __int32		INFO_DATATYPE;
		typedef __int32		EVENT_TYPE;

		struct EVENT_DATA
		{
			__int64		llnBlockID;
			__int64		llnTimestamp;
			WORD			wEventID;
			WORD			wStreamChannelIndex;
			WORD			wBlockID;
			void*			pDeviceHandle;
		};

		enum INTERFACE_INFO_CMD_LIST
		{
			INTERFACE_INFO_ID = 0,     /* STRING     Unique ID of the interface. */
			INTERFACE_INFO_DISPLAYNAME = 1,     /* STRING     User readable name of the interface. */
			INTERFACE_INFO_TLTYPE = 2,     /* STRING     Transport layer technology that is supported. */
		};
	}
}


typedef void (ImageCallbackFn)(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo);

typedef void (DisconnectCallbackFn)(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnectInfo);

typedef void (EventControlCallbackFn)( char* pcaEventName, VWSDK::VwGenICam::EVENT_DATA* ptEventData);

typedef void (GeneralEventCallbackFn)( VWSDK::EventType eEventType );

#endif
