// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.API.h"




int _tmain(int argc, _TCHAR* argv[])
{
	UINT unInfIdex = 0;
	BOOL bFind = FALSE;
	VWSDK::INTERFACE_INFO_STRUCT** ppInf_Info = NULL;

	char** ppchCamID = NULL;
	UINT unCamNum = 0;

	VWSDK::OBJECT_INFO*	pObjectInfo		= NULL;

	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	
	VWSDK::RESULT ret = VWSDK::RESULT_ERROR;
	
	VWSDK::VWGIGE_HANDLE hVwGigE;

	ret = VWSDK::OpenVwGigE(&hVwGigE);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));

	VWSDK::VwUserLogging(hVwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	// 1. Input NIC IP
	_tprintf(_T("Step 1. Input Interface IP.....\n"));
	_tprintf(_T("ex)169.254.10.1 \n"));
	_tprintf(_T(">"));

	char pchInput[80] = { 0, };
	gets_s(pchInput, sizeof(pchInput));

	
	// 2. Discovery Device
	_tprintf(_T("Step 2. Discovery Device....."));
	ret = VWSDK::VwDiscovery(hVwGigE);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	UINT unInfNum = 0;

	ret = VWSDK::VwGetNumInterfaces(hVwGigE, &unInfNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;
	

	if (unInfNum == 0)
	{
		_tprintf(_T("Not found a interface....\n"));
		goto EXIT;
	}

#if 0
	
	ppInf_Info = new VWSDK::INTERFACE_INFO_STRUCT*[unInfNum];

	for (UINT i = 0; i < unInfNum; i++)
	{
		ppInf_Info[i] = new VWSDK::INTERFACE_INFO_STRUCT;
		ret = VWSDK::VwDiscoveryInterfaceInfo(hVwGigE, i, ppInf_Info[i] );
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


		_tprintf(_T("Index:%d \n"), ppInf_Info[i]->index );
		_tprintf(_T("Name:%s \n"), ppInf_Info[i]->name );

	}

#endif // 0

	VWSDK::HINTERFACE** pphInf = NULL;
	pphInf = new VWSDK::HINTERFACE*[unInfNum];

	for (UINT i = 0; i < unInfNum; i++)
	{
		pphInf[i] = new VWSDK::HINTERFACE;
	}



	for (UINT i = 0; i < unInfNum; i++)
	{
		ret = VWSDK::VwOpenInterfaceByIndex(hVwGigE, i, pphInf[i] );
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

 		size_t ipSize = 0; 
		ret = VWSDK::InterfaceGetIPAddress(*pphInf[i], NULL, &ipSize );
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

		if (ipSize == 0)
			continue;


		char* pchInfIP = new char[ipSize];
		
		ret = VWSDK::InterfaceGetIPAddress(*pphInf[i], pchInfIP, &ipSize);
		if (ret != VWSDK::RESULT_SUCCESS)
		{
			delete[] pchInfIP;
			goto EXIT;
		}
		

		if (strcmp(pchInfIP, pchInput) == 0)
		{
			bFind = TRUE;
			unInfIdex = i;

			delete[] pchInfIP;

			break;
		}

		delete[] pchInfIP;
	}
		
	

	if (bFind == FALSE)
	{
		_tprintf(_T("Not found the interface that is connected to the device...."));

		goto EXIT;
	}



	ret = VWSDK::InterfaceGetNumCameras(*pphInf[unInfIdex], &unCamNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


	ppchCamID = new char*[unCamNum];


	for (UINT i = 0; i < unCamNum; i++)
	{
		ppchCamID[i] = new char[MAX_PATH];
		ZeroMemory(ppchCamID[i], MAX_PATH);

		size_t nCamIDSize = MAX_PATH;
		
		ret = VWSDK::InterfaceGetCameraID(*pphInf[unInfIdex], i, ppchCamID[i], &nCamIDSize);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;
		
		printf("%d) ", i+1);
		printf(ppchCamID[i]);
	}


	UINT selectednum = 1;

	if (unCamNum > 1)
	{
		printf("\nSelect Device number to open:\n");
		printf(">");

		ZeroMemory(pchInput, 80);

		gets_s(pchInput, sizeof(pchInput));

		bool isNum = false;
		int strLength = strlen(pchInput);

		for (int i = 0; i < strLength; i++)
		{
			if ('0' <= pchInput[i] && '9' >= pchInput[i])
			{
				isNum = true;
			}
			else
			{
				isNum = false;
				break;
			}
		}

		if (isNum == false)
			goto EXIT;

		selectednum = atoi(pchInput);

		if (selectednum > unCamNum)
		{
			_tprintf(_T("Not found a Device...."));

			goto EXIT;
		}
	}

	// 4. Open device
	_tprintf(_T("\nStep 4. Open Device....."));
	
	pObjectInfo = new VWSDK::OBJECT_INFO;

	VWSDK::HCAMERA hVwCamera;
	ret = VwOpenCameraByName(hVwGigE, 
							ppchCamID[selectednum - 1]/*Device Index*/,
							&hVwCamera,
							2/*The number of buffer*/,
							0/*Auto*/,
							0/*Auto*/,
							0/*Auto*/,
							pObjectInfo/*User Pointer*/,
							NULL/*Skip in this sample*/,
							NULL/*Skip in this sample*/);

	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));

	// 5. Terminate
	_tprintf(_T("Step 5. Terminate....."));

	if (hVwCamera)
	{
		ret = VWSDK::CameraClose(hVwCamera);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;
		
		hVwCamera = NULL;
	}

	_tprintf(_T("OK\n"));

EXIT:
	
	VWSDK::CloseVwGigE(hVwGigE);

	if (ret != VWSDK::RESULT_SUCCESS)
	{
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
	}


	if (pObjectInfo)
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if (ppchCamID != NULL)
	{
		for (UINT i = 0; i < unCamNum; i++)
		{
			delete[] ppchCamID[i];
		}

		delete [] ppchCamID;
	}

	if (pphInf != NULL)
	{
		for (UINT i = 0; i < unInfNum; i++)
		{
			delete pphInf[i];

		}
		
		delete[] pphInf;
	}


	return 0;
}


