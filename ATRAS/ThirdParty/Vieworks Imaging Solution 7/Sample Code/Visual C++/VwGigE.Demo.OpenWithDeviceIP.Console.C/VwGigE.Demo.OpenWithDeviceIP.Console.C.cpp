// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.API.h"

#include <list>


int _tmain(int argc, _TCHAR* argv[])
{
 	VWSDK::OBJECT_INFO*	pObjectInfo = NULL;
	VWSDK::CAMERA_INFO_STRUCT** ppCamInfo = NULL;

	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));

	VWSDK::VWGIGE_HANDLE hVwGigE;

	VWSDK::RESULT ret = VWSDK::OpenVwGigE(&hVwGigE);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));

	VWSDK::VwUserLogging(hVwGigE, _T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));


	// 1. Input device IP
	_tprintf(_T("Step 1. Input Device IP.....\n"));
	_tprintf(_T("ex)169.254.10.1 \n"));

	char pchIP[80] = { 0, };
	gets_s(pchIP, sizeof(pchIP));

	
	// 2. Discovery Device
	_tprintf(_T("Step 2. Discovery Device....."));
	
	ret = VWSDK::VwDiscovery(hVwGigE);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	// 3. Get Discovery Information
	_tprintf(_T("Step 3. Get Discovery Information.....\n"));
	
	UINT unCamNum = 0;
	ret = VWSDK::VwGetNumCameras(hVwGigE, &unCamNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	if (unCamNum == 0)
	{
		_tprintf(_T("Not found a Device....\n"));
		goto EXIT;
	}


	
	ppCamInfo = new VWSDK::CAMERA_INFO_STRUCT*[unCamNum];
	UINT unCamIndex = 0;
	BOOL bFind = FALSE;

	for (UINT i = 0; i < unCamNum; i++)
	{
		ppCamInfo[i] = new VWSDK::CAMERA_INFO_STRUCT;
		VWSDK::VwDiscoveryCameraInfo(hVwGigE, i, ppCamInfo[i] );


		printf("\t");
		printf(ppCamInfo[i]->name);
		printf("\n");

		printf("\t");
		printf(ppCamInfo[i]->ip);
		printf("\n");

		printf("\t");
		printf(ppCamInfo[i]->mac);
		printf("\n");


		if (strcmp(ppCamInfo[i]->ip, pchIP) == 0)
		{
			bFind = TRUE;
			unCamIndex = ppCamInfo[i]->index;

			break;
		}


	}
	

	if (bFind == FALSE)
	{
		_tprintf(_T("Not found a Device...."));

		goto EXIT;
	}

	_tprintf(_T("OK\n"));


	// 4. Open device
	_tprintf(_T("Step 4. Open Device....."));
	pObjectInfo = new VWSDK::OBJECT_INFO;

	
	VWSDK::HCAMERA hVwCamera;
	ret = VWSDK::VwOpenCameraByIndex(hVwGigE,
									unCamIndex/*Device Index*/,
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


	if (ppCamInfo != NULL)
	{
		for (UINT i = 0; i < unCamNum; i++)
		{
			delete ppCamInfo[i];
		}

		delete[] ppCamInfo;
	}


	if (pObjectInfo)
	{
		delete pObjectInfo;
		pObjectInfo = NULL;
	}


	return 0;





}


