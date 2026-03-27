// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"

char* Dispatch(char *cmdline, int cmdSize, VWSDK::VwCamera* phCamera);

int _tmain(int argc, _TCHAR* argv[])
{
	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VwGigE	vwGigE;
	vwGigE.UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	VWSDK::RESULT ret = vwGigE.Open();
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		return -1;
	}
	_tprintf(_T("OK\n"));

	// 1. Open device
	_tprintf(_T("Step 1. Open Device....."));
	VWSDK::VwCamera*		pvwCamera		= NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo			= new VWSDK::OBJECT_INFO;

	ret = vwGigE.OpenCamera( (UINT)0/*Device Index*/,
								&pvwCamera,
								2/*The number of buffer*/,
								0/*Auto*/,
								0/*Auto*/,
								0/*Auto*/,
								pObjectInfo/*User Pointer*/,
								NULL/*Skip in this sample*/ );
	if ( VWSDK::RESULT_SUCCESS != ret )
	{
		// Error handling
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		// Terminate
		if ( pObjectInfo )
		{
			delete pObjectInfo;
			pObjectInfo = NULL;
		}
		vwGigE.Close();
		return -1;
	}
	_tprintf(_T("OK\n"));

MAIN_MENU:
	char chDone = 0;
	char chCmdline[80] = {0,};

	while(1)
	{
		printf("\n\tSelect menu\n");
		printf("\t1.Get All Feature List\n");
		printf("\t2.Get Feature Information\n");
		printf("\t3.Set Feature Data\n");
		printf("\t4.Get Feature Data\n");
		printf("\t5.Get Feature Min/Max\n");
		printf("\t6.Get list of Enumeration Feature\n");

		printf("\t7.Quit\n");
		printf(">");

		gets_s(chCmdline, sizeof(chCmdline));
		if(0 == strlen(chCmdline))
		{
			/* do nothing */
		}
		else if(!_strnicmp("7", chCmdline, 2))
		{
			break;
		}
		else
		{
			Dispatch((char *)chCmdline, sizeof(chCmdline), pvwCamera);
		}
	}

	do 
	{
		printf("** Goto Select menu?\n");
		printf("** Y\n");
		printf("** N\n");	
		printf(">");	
		gets_s(chCmdline, sizeof(chCmdline));
	} while ( ( _strnicmp("Y", chCmdline, 1) ) &&
			  ( _strnicmp("N", chCmdline, 1) ) );

	if( !(_strnicmp("Y", chCmdline, 1))) 
	{
		goto MAIN_MENU;
	}

	// 3. Terminate
	_tprintf(_T("Step 3. Terminate.....\n"));
	if( pObjectInfo )
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if ( pvwCamera )
	{
		pvwCamera->CloseCamera();
		pvwCamera = NULL;
	}

	vwGigE.Close();

	return 0;
}


char* Dispatch(char *cmdline, int cmdSize, VWSDK::VwCamera* phCamera)
{
	if ( NULL == phCamera )
	{
		return NULL;
	}

	if(0 == strlen(cmdline))
	{

	}
	else if( !(_strnicmp("1", cmdline, 2))) 
	{
		int nPropCount = 0;

		VWSDK::RESULT ret = phCamera->GetPropertyCount(&nPropCount);

		if( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		}
		else
		{
			VWSDK::PROPERTY propInfo;

			for( int nIdx = 0; nIdx < nPropCount; nIdx ++ )
			{
				ret = phCamera->GetPropertyInfo( nIdx, &propInfo );

				if( ret != VWSDK::RESULT_SUCCESS )
				{
					_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
					break;
				}
				else
				{
					printf("%s \n", propInfo.caName );
				}
			}
		}
	}
	else if( !(_strnicmp("2", cmdline, 2))) 
	{
		char caFeature[80] = {0,};

		// Input the feature name
		printf("\nInput a name of Feature\n");
		printf(">");

		gets_s(caFeature, sizeof(caFeature));

		VWSDK::PROPERTY propInfo;
		VWSDK::RESULT ret = phCamera->GetPropertyInfo(caFeature, &propInfo); 

		if( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		}
		else
		{
			if( VWSDK::READ_ONLY == propInfo.eAccessMode)
				printf("Access Mode : Read Only\n");
			else if( VWSDK::WRITE_ONLY == propInfo.eAccessMode)
				printf("Access Mode : Write Only\n");
			else if( VWSDK::READ_WRITE == propInfo.eAccessMode)
				printf("Access Mode : Read Write\n");
			else if( VWSDK::NOT_AVAILABLE == propInfo.eAccessMode)
				printf("Access Mode : Not Available\n");
			else 
				printf("Access Mode : Not Implement\n");

			if( VWSDK::ATTR_BOOLEAN == propInfo.ePropType)
				printf("Property Type : Boolean\n");
			else if( VWSDK::ATTR_CATEGORY == propInfo.ePropType)
				printf("Property Type : Category\n");
			else if( VWSDK::ATTR_COMMAND == propInfo.ePropType)
				printf("Property Type : Command\n");
			else if( VWSDK::ATTR_ENUM == propInfo.ePropType)
				printf("Property Type : Enumeration\n");
			else if( VWSDK::ATTR_FLOAT == propInfo.ePropType)
				printf("Property Type : Float\n");
			else if( VWSDK::ATTR_STRING == propInfo.ePropType)
				printf("Property Type : String\n");
			else if( VWSDK::ATTR_INT == propInfo.ePropType)
				printf("Property Type : Integer\n");
			else
				printf("Property Type : Unknown\n");

			if( VWSDK::BEGINNER == propInfo.eVisibility)
				printf("Property Visibility : Beginner\n");
			else if( VWSDK::EXPERT == propInfo.eVisibility)
				printf("Property Visibility : Expert\n");
			else if( VWSDK::GURU == propInfo.eVisibility)
				printf("Property Visibility : Guru\n");
			else if( VWSDK::INVISIBLE == propInfo.eVisibility)
				printf("Property Visibility : Invisible\n");
			else
				printf("Property Visibility : Undefined\n");
		}
	}
	else if( !(_strnicmp("3", cmdline, 2))) 
	{
		char caFeature[80]   = {0,};
		char caArgument[80] = {0,};

		// Input the feature name
		printf("\nInput a name of Feature\n");
		printf(">");

		gets_s(caFeature, sizeof(caFeature));

		printf("\nInput a value\n");
		printf(">");

		gets_s(caArgument, sizeof(caArgument));

		// SetCustomCommand
		VWSDK::RESULT ret = phCamera->SetCustomCommand( caFeature, caArgument );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		}
	}
	else if( !(_strnicmp("4", cmdline, 2))) 
	{
		char caFeature[80]  = {0,};
		char caArgument[80] = {0,};

		// Input the feature name
		printf("\nInput a name of Feature\n");
		printf(">");

		gets_s(caFeature, sizeof(caFeature));

		// Get Property Info
		VWSDK::PROPERTY propInfo;
		VWSDK::RESULT ret = phCamera->GetPropertyInfo(caFeature, &propInfo); 
		if( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		}

		size_t unArgumentSize = sizeof( caArgument );
		// GetCustomCommand
		ret = phCamera->GetCustomCommand( caFeature, caArgument, &unArgumentSize );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		}
		else
		{
			switch( propInfo.ePropType )
			{
			case VWSDK::ATTR_BOOLEAN:
				{
					if ( ! strcmp( caArgument, "0") )
					{
						printf("Value : FALSE\n");
					}
					else
					{
						printf("Value : TRUE\n");
					}
					
					break;
				}
			case VWSDK::ATTR_INT:
			case VWSDK::ATTR_FLOAT:
				{
					switch( propInfo.eRepresentation )
					{
					case VWSDK::REP_HEXNUMBER:
						{
							int nData = atoi( caArgument );
							printf("Value : %X\n", nData);
							break;
						}
					case VWSDK::REP_IPV4ADDRESS:
						{
							ULONGLONG ulData		= _atoi64( caArgument );
							BYTE arrbt[4]			= {0,};
							arrbt[0] = ( ulData & 0xFF000000 ) >> 24;
							arrbt[1] = ( ulData & 0x00FF0000 ) >> 16;
							arrbt[2] = ( ulData & 0x0000FF00 ) >> 8;
							arrbt[3] = ( ulData & 0x000000FF );

							printf("Value : %d.%d.%d.%d\n", arrbt[0],
															arrbt[1],
															arrbt[2],
															arrbt[3] );
							break;
						}
					default:
						{
							printf("Value : %s\n", caArgument);
						}
					}
					break;
				}
			case VWSDK::ATTR_ENUM:
			case VWSDK::ATTR_STRING:
				{
					printf("Value : %s\n", caArgument);
					break;
				}
			case VWSDK::ATTR_CATEGORY:
				{
					printf("Value : This is Category type feature.\n");
					break;
				}
			case VWSDK::ATTR_COMMAND:
				{
					printf("Value : This is Command type feature.\n");
				}
			default:
				{
					printf("Value : %s\n", caArgument);
				}
			}

		}
	}
	else if( !(_strnicmp("5", cmdline, 2))) 
	{
		char caFeature[80]  = {0,};
		char caArgument[80] = {0,};

		// Input the feature name
		printf("\nInput a name of Feature\n");
		printf(">");

		gets_s(caFeature, sizeof(caFeature));

		size_t unArgumentSize = sizeof( caArgument );
		// GetCustomCommand
		VWSDK::RESULT ret = phCamera->GetCustomCommand( caFeature, caArgument, &unArgumentSize, VWSDK::GET_CUSTOM_COMMAND_MAX );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
		}
		else
		{
			printf("Max value : %s\n", caArgument);
		}

		unArgumentSize = sizeof( caArgument );
		ZeroMemory( caArgument, unArgumentSize );
		ret = phCamera->GetCustomCommand( caFeature, caArgument, &unArgumentSize, VWSDK::GET_CUSTOM_COMMAND_MIN );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		}
		else
		{
			printf("Min value : %s\n", caArgument);
		}
	}
	else if( !(_strnicmp("6", cmdline, 2))) 
	{
		char caFeature[80]  = {0,};
		char caArgument[80] = {0,};

		// Input the feature name
		printf("\nInput a name of Feature\n");
		printf(">");

		gets_s(caFeature, sizeof(caFeature));

		// Get Property Info
		VWSDK::PROPERTY propInfo;
		VWSDK::RESULT ret = phCamera->GetPropertyInfo(caFeature, &propInfo); 
		if( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		}
		if ( VWSDK::ATTR_ENUM != propInfo.ePropType )
		{
			printf("The name of Feature is not a enumeration type.\n");
			return cmdline;
		}

		size_t unArgumentSize = sizeof( caArgument );
		// GetCustomCommand
		ret = phCamera->GetCustomCommand( caFeature, caArgument, &unArgumentSize, VWSDK::GET_CUSTOM_COMMAND_NUM );
		if ( ret != VWSDK::RESULT_SUCCESS )
		{
			_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
		}
		else
		{
			printf("The number of item : %s\n", caArgument);
		}

		UINT unCount = atoi( caArgument );
		for ( UINT i = 0; i < unCount; i ++ )
		{
			unArgumentSize = sizeof( caArgument );
			ZeroMemory( caArgument, unArgumentSize );
			ret = phCamera->GetCustomCommand( caFeature, caArgument, &unArgumentSize, VWSDK::GET_CUSTOM_COMMAND_INDEX + i );
			if ( ret != VWSDK::RESULT_SUCCESS )
			{
				_tprintf(_T("Failed(err:%s))\n"), VwErrorReport(ret));
			}
			else
			{
				printf("\t%s\n", caArgument);
			}
		}
	}
	else
		strncpy_s(cmdline, cmdSize, "Unrecognized command\n", cmdSize);

	return cmdline;
}
