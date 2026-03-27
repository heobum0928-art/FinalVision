// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"




int _tmain(int argc, _TCHAR* argv[])
{
	VWSDK::VwInterface** ppVwInf = NULL;
	UINT unInfIdex = 0;
	BOOL bFind = FALSE;

	char** ppchCamName = NULL;
	UINT unCamNum = 0;

	VWSDK::VwCamera*		pvwCamera	= NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo		= NULL;

	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	
	VWSDK::VwGigE	vwGigE;
	vwGigE.UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	VWSDK::RESULT ret = VWSDK::RESULT_ERROR;
		
	ret = vwGigE.Open();
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	// 1. Input NIC IP
	_tprintf(_T("Step 1. Input Interface IP.....\n"));
	_tprintf(_T("ex)169.254.10.1 \n"));
	_tprintf(_T(">"));

	char pchInput[80] = { 0, };
	gets_s(pchInput, sizeof(pchInput));

	
	// 2. Discovery Device
	_tprintf(_T("Step 2. Discovery Device....."));
	ret = vwGigE.Discovery();
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	UINT unInfNum = 0;
	ret = vwGigE.GetNumInterfaces(&unInfNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


		
	ppVwInf = new VWSDK::VwInterface*[unInfNum];
	
	for (UINT i = 0; i < unInfNum; i++)
	{
		ppVwInf[i] = NULL;
	}

	for (UINT i = 0; i < unInfNum; i++)
	{
		ret = vwGigE.OpenInterface(i, &ppVwInf[i] );
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


		size_t nIpSize = 0;
		ret = ppVwInf[i]->GetIPAddress(NULL, &nIpSize);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


		char* pchInfIP = new char[nIpSize];

		ret = ppVwInf[i]->GetIPAddress(pchInfIP, &nIpSize);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


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
		_tprintf(_T("Not found the interface that is connected to the device....\n"));

		goto EXIT;
	}
	
	
	ret = ppVwInf[unInfIdex]->GetNumCameras(&unCamNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;



		
	ppchCamName = new char*[unCamNum];


	for (UINT i = 0; i < unCamNum; i++)
	{
		ppchCamName[i] = new char[MAX_PATH];
		ZeroMemory(ppchCamName[i], MAX_PATH);

		size_t nCamNameSize = MAX_PATH;


		ret = ppVwInf[unInfIdex]->GetCameraID(i, ppchCamName[i], &nCamNameSize);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


		
		printf("%d) ", i+1);
		printf(ppchCamName[i]);
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
			_tprintf(_T("Not found a Device....\n"));

			goto EXIT;
		}
	}

	// 4. Open device
	_tprintf(_T("\nStep 4. Open Device....."));
	
	pObjectInfo = new VWSDK::OBJECT_INFO;


	ret = vwGigE.OpenCamera(ppchCamName[selectednum - 1]/*Device Index*/,
							&pvwCamera,
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

	if (pvwCamera)
	{
		ret = pvwCamera->CloseCamera();
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;
		
		pvwCamera = NULL;
	}

	_tprintf(_T("OK\n"));

EXIT:
	
	vwGigE.Close();

	if (ret != VWSDK::RESULT_SUCCESS)
	{
		_tprintf(_T("Failed(err:%s))\n"), VWSDK::VwErrorReport(ret));
	}


	if (pObjectInfo)
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}

	if (ppchCamName != NULL)
	{
		for (UINT i = 0; i < unCamNum; i++)
		{
			delete[] ppchCamName[i];
		}

		delete [] ppchCamName;
	}

	if (ppVwInf != NULL)
	{
		delete[] ppVwInf;
	}


	return 0;
}


