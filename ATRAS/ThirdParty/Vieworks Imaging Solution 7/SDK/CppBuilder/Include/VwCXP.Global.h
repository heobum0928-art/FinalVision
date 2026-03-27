#pragma once

#include "VwSDK.h"


#include "windows.h"

#include "GenTL_v1_3.h"


#include <vector>
#include <memory.h>

#include <string>
#include <iostream>
#include <sstream>

#include <tchar.h>



#define __DLL_NAME__		( _T("VwCXP.V7.DLL") )

using namespace std;
using namespace GenICam;
using namespace GenICam::Client;

#define IN
#define OUT
#define INOUT



typedef unsigned int UINT;

extern HANDLE _hEventVwCXPLog;

#ifdef __cplusplus
extern "C"
{
#endif
	
namespace VWSDK
{

	typedef enum
	{
		Raw							= 0x0000,

		Mono8						= 0x0101,
		Mono10pmsb					= 0x0102,
		Mono12pmsb					= 0x0103,
		Mono14pmsb					= 0x0104,
		Mono16						= 0x0105,

		BayerGR8					= 0x0311,
		BayerGR10pmsb				= 0x0312,
		BayerGR12pmsb				= 0x0313,
		BayerGR14pmsb				= 0x0314,
		BayerGR16					= 0x0315,

		BayerRG8					= 0x0321,
		BayerRG10pmsb				= 0x0322,
		BayerRG12pmsb				= 0x0323,
		BayerRG14pmsb				= 0x0324,
		BayerRG16					= 0x0325,

		BayerGB8					= 0x0331,
		BayerGB10pmsb				= 0x0332,
		BayerGB12pmsb				= 0x0333,
		BayerGB14pmsb				= 0x0334,
		BayerGB16					= 0x0335,

		BayerBG8					= 0x0341,
		BayerBG10pmsb				= 0x0342,
		BayerBG12pmsb				= 0x0343,
		BayerBG14pmsb				= 0x0344,
		BayerBG16					= 0x0345,

		RGB8						= 0x0401,
		RGB10pmsb					= 0x0402,
		RGB12pmsb					= 0x0403,
		RGB14pmsb					= 0x0404,
		RGB16						= 0x0405,

		RGBa8						= 0x0501,
		RGBa10pmsb					= 0x0502,
		RGBa12pmsb					= 0x0503,
		RGBa14pmsb					= 0x0504,
		RGBa16						= 0x0505,

		YUV411_8					= 0x0611,
		YUV411_10pmsb				= 0x0612,
		YUV411_12pmsb				= 0x0613,
		YUV411_14pmsb				= 0x0614,
		YUV411_16					= 0x0615,

		YUV422_8					= 0x0621,
		YUV422_10pmsb				= 0x0622,
		YUV422_12pmsb				= 0x0623,
		YUV422_14pmsb				= 0x0624,
		YUV422_16					= 0x0625,

		YUV8						= 0x0631,
		YUV10pmsb					= 0x0632,
		YUV12pmsb					= 0x0633,
		YUV14pmsb					= 0x0634,
		YUV16						= 0x0635,

		YCbCr601_411_8				= 0x0711,
		YCbCr601_411_10pmsb			= 0x0712,
		YCbCr601_411_12pmsb			= 0x0713,
		YCbCr601_411_14pmsb			= 0x0714,
		YCbCr601_411_16				= 0x0715,

		YCbCr601_422_8				= 0x0721,
		YCbCr601_422_10pmsb			= 0x0722,
		YCbCr601_422_12pmsb			= 0x0723,
		YCbCr601_422_14pmsb			= 0x0724,
		YCbCr601_422_16				= 0x0725,

		YCbCr601_8					= 0x0731,
		YCbCr601_10pmsb				= 0x0732,
		YCbCr601_12pmsb				= 0x0733,
		YCbCr601_14pmsb				= 0x0734,
		YCbCr601_16					= 0x0735,

		YCbCr709_411_8				= 0x0811,
		YCbCr709_411_10pmsb			= 0x0812,
		YCbCr709_411_12pmsb			= 0x0813,
		YCbCr709_411_14pmsb			= 0x0814,

		YCbCr709_422_8				= 0x0821,
		YCbCr709_422_10pmsb			= 0x0822,
		YCbCr709_422_12pmsb			= 0x0823,
		YCbCr709_422_14pmsb			= 0x0824,
		YCbCr709_422_16				= 0x0825,

		YCbCr709_8					= 0x0831,
		YCbCr709_10pmsb				= 0x0832,
		YCbCr709_12pmsb				= 0x0833,
		YCbCr709_14pmsb				= 0x0834,
		YCbCr709_16					= 0x0835

	}CXP_PIXEL_TYPE;

	enum EventType
	{
		EVENT_TYPE_FRAME_DROP = 0x0000001,
		EVENT_TYPE_SENSOR_MASTER = 0x0000002,
		EVENT_TYPE_SENSOR_SLAVE = 0x0000003,
		EVENT_TYPE_LINK_SPEED = 0x0000004,
		EVENT_TYPE_TEMP_EXCEED_LIMIT = 0x0000005
	};

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

	struct INTERFACE_INFO_STRUCT
	{
		BOOL				error;
		VWSDK::RESULT				errorCause;
		unsigned int		index;
		char				name[256];
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

	enum STREAM_INFO
	{
		STREAM_INFO_NUM_OF_FRAMES_LOST,
		STREAM_INFO_NUM_PACKETS_MISSING
	};



	std::string static makeSentence(const char *fmt, ...)
	{
		char c[4096];
		va_list argp;
		va_start(argp, fmt);
#if __STDC_WANT_SECURE_LIB__
		vsprintf_s(c, fmt, argp);
#else
		vsprintf(c, fmt, argp);
#endif
		va_end(argp);
		return std::string (c);
	}

	inline void ErrorToString(char* pc, size_t len, GC_ERROR gcErr)
	{
		switch(gcErr){
		case GC_ERR_ERROR:
			sprintf_s(pc, len, "%s\n", "GC_ERR_ERROR");
			break;
		case GC_ERR_NOT_INITIALIZED:
			sprintf_s(pc, len, "%s\n", "GC_ERR_NOT_INITIALIZED");
			break;
		case GC_ERR_NOT_IMPLEMENTED:
			sprintf_s(pc, len, "%s\n", "GC_ERR_NOT_IMPLEMENTED");
			break;
		case GC_ERR_RESOURCE_IN_USE:
			sprintf_s(pc, len, "%s\n", "GC_ERR_RESOURCE_IN_USE");
			break;
		case GC_ERR_ACCESS_DENIED:
			sprintf_s(pc, len, "%s\n", "GC_ERR_ACCESS_DENIED");
			break;
		case GC_ERR_INVALID_HANDLE:
			sprintf_s(pc, len, "%s\n", "GC_ERR_INVALID_HANDLE");
			break;
		case GC_ERR_INVALID_ID:
			sprintf_s(pc, len, "%s\n", "GC_ERR_INVALID_ID");
			break;
		case GC_ERR_NO_DATA:
			sprintf_s(pc, len, "%s\n", "GC_ERR_NO_DATA");
			break;
		case GC_ERR_INVALID_PARAMETER:
			sprintf_s(pc, len, "%s\n", "GC_ERR_INVALID_PARAMETER");
			break;
		case GC_ERR_IO:
			sprintf_s(pc, len, "%s\n", "GC_ERR_IO");
			break;
		case GC_ERR_TIMEOUT:
			sprintf_s(pc, len, "%s\n", "GC_ERR_TIMEOUT");
			break;
		case GC_ERR_ABORT:
			sprintf_s(pc, len, "%s\n", "GC_ERR_ABORT");
			break;
		case GC_ERR_INVALID_BUFFER:
			sprintf_s(pc, len, "%s\n", "GC_ERR_INVALID_BUFFER");
			break;
		case GC_ERR_NOT_AVAILABLE:
			sprintf_s(pc, len, "%s\n", "GC_ERR_NOT_AVAILABLE");
			break;
		case GC_ERR_INVALID_ADDRESS:
			sprintf_s(pc, len, "%s\n", "GC_ERR_INVALID_ADDRESS");
			break;
		};
	}

#define CHECK_GENTL(message, function) \
	{\
	GenICam::Client::GC_ERROR gcErr = function;\
	if (gcErr != GenICam::Client::GC_ERR_SUCCESS) {\
	size_t iSize = 0;\
	char * pc = new char[1024];\
	std::stringstream ss;\
	try {\
	ErrorToString(pc, 1024, gcErr);\
	std::string s(#function);\
	s = s.substr(0, s.find_first_of("("));\
	ss << std::string(message) \
	<< std::string(" Error calling ") << s << std::string("() ") \
	<< std::string(pc);\
	} catch(std::exception &e) {\
	THROW_GENTL(makeSentence("std::exception caught while processing:\n %s\n%s", #function, e.what()));\
	if (pc) delete [] pc;\
	} catch(...) {\
	THROW_GENTL(makeSentence("Unknown exception caught while processing:\n %s", #function));\
	if (pc) delete [] pc;\
	}\
	if (pc) delete [] pc;\
	THROW_GENTL(ss.str().c_str());\
	}\
	}

#define CHECK_ERR_GENTL(gcErr, message, function) \
{\
	gcErr = function;\
	if (gcErr != GenICam::Client::GC_ERR_SUCCESS) {\
		size_t iSize = 0;\
		char * pc = new char[1024];\
		std::stringstream ss;\
		try {\
			ErrorToString(pc, 1024, gcErr);\
			std::string s(#function);\
			s = s.substr(0, s.find_first_of("("));\
			ss << std::string(message)\
			<< std::string(" Error calling ") << s << std::string("() ") \
			<< std::string(pc);\
		} catch(std::exception &e) {\
			THROW_GENTL(makeSentence("std::exception caught while processing:\n %s\n%s", #function, e.what()));\
			if (pc) delete [] pc;\
		} catch(...) {\
			THROW_GENTL(makeSentence("Unknown exception caught while processing:\n %s", #function));\
			if (pc) delete [] pc;\
		}\
		if (pc) delete [] pc;\
		THROW_GENTL(ss.str().c_str());\
	}\
}

#define THROW_GENTL(message) throw CGenTLException((message), __FUNCTION__, __FILE__, __LINE__)

	class CGenTLException : public std::exception
	{
	public:
		CGenTLException(std::string description, std::string functionName, std::string sourceFileName, unsigned int sourceLine) :
		  std::exception()
			  , m_FunctionName(functionName)
			  , m_SourceLine(sourceLine)
			  , m_SourceFileName(sourceFileName)
			  , m_Description(description)
		  {
			  makeMessage();
		  }

		  virtual const char *what() const throw()
		  {
			  return m_What.c_str();
		  }

	private:
		std::string m_FunctionName;
		unsigned int m_SourceLine;
		std::string m_SourceFileName;
		std::string m_Description;
		std::string m_What; //Complete error message, including file name and line number

		void makeMessage()
		{
			// extract the file name from the path
			std::string Buffer( m_SourceFileName );
			size_t found = Buffer.find_last_of("/\\");
			std::string ShortFileName = Buffer.substr(found+1);

			// assemble the error message
			std::stringstream Message;

			LARGE_INTEGER n64PastTime;
			QueryPerformanceCounter(&n64PastTime);

			Message << m_Description;
			//Message << "\n\r" << __DLL_NAME__ << "\n\r" << ShortFileName << "\n\r" << m_FunctionName << "\n\r" << m_SourceLine << "\n\r" << n64PastTime.QuadPart;


			m_What = Message.str().c_str();

		}
	};



#if 0

#define WIDEN2(x) L ## x
#define WIDEN(x) WIDEN2(x)
#define __WFILE__ WIDEN(__FILE__)
#define __WFUNCTION__ WIDEN(__FUNCTION__)


	// Define for the simple value.
#define DBG_PRINT_SWITCH		TRUE					// Turn on/off the OutputDebugString function.
#define DBG_PRINT_BUFFER_SIZE	2048					// The temporary buffer size.

	enum EVENTLOG_TYPE
	{
		INFO = 0,
		ERR,
		NONE
	};

	static void VwReportEvent(	HANDLE			a_hEventLog,
		EVENTLOG_TYPE	a_eInfo,
		TCHAR*			a_tchFileName,  
		TCHAR*			a_tchFuncName, 
		UINT			a_unLine, 
		TCHAR*			a_tchFormat, ... )
	{

#ifdef USE_EDIT_EVENT_LOG

		static TCHAR chBuf[ DBG_PRINT_BUFFER_SIZE ] = {0,};
		va_list		marker;
		va_start( marker, a_tchFormat );

#if __STDC_WANT_SECURE_LIB__
		_vstprintf_s( chBuf, a_tchFormat, marker );
#else
		_vstprintf( chBuf, a_tchFormat, marker );
#endif
		va_end( marker );

		static TCHAR chReport[4096] = {0,};

#if __STDC_WANT_SECURE_LIB__
		swprintf_s( chReport, _T("%s\t%s\t%s\t%d \n"), chBuf, a_tchFileName, a_tchFuncName, a_unLine );
#else
		wsprintf( chReport, _T("%s\t%s\t%s\t%d \n"), chBuf, a_tchFileName, a_tchFuncName, a_unLine );
#endif
		


		LPCWSTR strReport = (LPCWSTR)chReport;

		switch(a_eInfo)
		{
		case INFO:
			ReportEvent( a_hEventLog, EVENTLOG_INFORMATION_TYPE, NULL, MSG_TLI_INFO_VALUE, NULL, 1, 0, &strReport, NULL );
			break;
		case ERR:
			ReportEvent( a_hEventLog, EVENTLOG_ERROR_TYPE, NULL, MSG_ERROR_RETURN_VALUE, NULL, 1, 0, &strReport, NULL );
			break;
		}


#endif

	}

#endif


#define __func__ __FUNCTION__
#define DEBUG_CONFIDENCE
#ifdef DEBUG_CONFIDENCE
#define debugPrint	printf
#define printEnter	printf("[%s]//++\n", __func__)
#define printExit	printf("[%s]//--\n", __func__)
#define printError	printf
#else
#define debugPrint	
#define printEnter
#define printExit
#define printError	printf
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

}//namespace VWSDK

typedef void (ImageCallbackFn)(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::IMAGE_INFO* pImageInfo);

typedef void (DisconnectCallbackFn)(VWSDK::OBJECT_INFO* pObjectInfo, VWSDK::DISCONNECT_INFO tDisconnectInfo);

typedef void (EventControlCallbackFn)(char* pcaEventName, VWSDK::VwGenICam::EVENT_DATA* ptEventData);

typedef void (GeneralEventCallbackFn)(VWSDK::EventType eEventType);

#ifdef __cplusplus

}
#endif	//__cplusplus 
