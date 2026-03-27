//
//  Values are 32 bit values laid out as follows:
//
//   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
//   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
//  +---+-+-+-----------------------+-------------------------------+
//  |Sev|C|R|     Facility          |               Code            |
//  +---+-+-+-----------------------+-------------------------------+
//
//  where
//
//      Sev - is the severity code
//
//          00 - Success
//          01 - Informational
//          10 - Warning
//          11 - Error
//
//      C - is the Customer code flag
//
//      R - is a reserved bit
//
//      Facility - is the facility code
//
//      Code - is the facility's status code
//
//
// Define the facility codes
//
#define FACILITY_SYSTEM                  0x0
#define FACILITY_STUBS                   0x3
#define FACILITY_RUNTIME                 0x2
#define FACILITY_IO_ERROR_CODE           0x4


//
// Define the severity codes
//
#define STATUS_SEVERITY_WARNING          0x2
#define STATUS_SEVERITY_SUCCESS          0x0
#define STATUS_SEVERITY_INFORMATIONAL    0x1
#define STATUS_SEVERITY_ERROR            0x3


//
// MessageId: MSG_DLL_LOADED
//
// MessageText:
//
// TLI loaded
//
#define MSG_DLL_LOADED                   ((DWORD)0x40020001L)

//
// MessageId: MSG_DLL_UNLOADED
//
// MessageText:
//
// TLI unloaded
//
#define MSG_DLL_UNLOADED                 ((DWORD)0x40020002L)

//
// MessageId: MSG_TLI_REGISTER_VALUES
//
// MessageText:
//
// Register values initialized: %1
//
#define MSG_TLI_REGISTER_VALUES          ((DWORD)0x40020003L)

//
// MessageId: MSG_ERROR_READING_REGISTER
//
// MessageText:
//
// Error reading register at %1: %2
//
#define MSG_ERROR_READING_REGISTER       ((DWORD)0xC0020004L)

//
// MessageId: MSG_ERROR_WRITING_REGISTER
//
// MessageText:
//
// Error writing register at %1: %2
//
#define MSG_ERROR_WRITING_REGISTER       ((DWORD)0xC0020005L)

//
// MessageId: MSG_ERROR_READING_MEMORY
//
// MessageText:
//
// Error reading memory at %1: %2
//
#define MSG_ERROR_READING_MEMORY         ((DWORD)0xC0020006L)

//
// MessageId: MSG_ERROR_WRITING_MEMORY
//
// MessageText:
//
// Error writing memory at %1: %2
//
#define MSG_ERROR_WRITING_MEMORY         ((DWORD)0xC0020007L)

//
// MessageId: MSG_ERROR_OPENING_FILTER_STREAM
//
// MessageText:
//
// Error while opening filter stream: %1
//
#define MSG_ERROR_OPENING_FILTER_STREAM  ((DWORD)0xC0020008L)

//
// MessageId: MSG_ERROR_CLOSING_FILTER_STREAM
//
// MessageText:
//
// Error while closing filter stream: %1
//
#define MSG_ERROR_CLOSING_FILTER_STREAM  ((DWORD)0xC0020009L)

//
// MessageId: MSG_ERROR_GETTING_FILTER_IMAGE
//
// MessageText:
//
// Error while getting image from filter: %1
//
#define MSG_ERROR_GETTING_FILTER_IMAGE   ((DWORD)0xC002000AL)

//
// MessageId: MSG_ERROR_BAD_FILTER_IMAGE
//
// MessageText:
//
// Bad image from filter: %1
//
#define MSG_ERROR_BAD_FILTER_IMAGE       ((DWORD)0xC002000BL)

//
// MessageId: MSG_ERROR_RETURN_VALUE
//
// MessageText:
//
// Error while processing the function: %1
//
#define MSG_ERROR_RETURN_VALUE           ((DWORD)0xC002000CL)

//
// MessageId: MSG_TLI_INFO_VALUE
//
// MessageText:
//
// TLI Information : %1
//
#define MSG_TLI_INFO_VALUE               ((DWORD)0x4002000DL)