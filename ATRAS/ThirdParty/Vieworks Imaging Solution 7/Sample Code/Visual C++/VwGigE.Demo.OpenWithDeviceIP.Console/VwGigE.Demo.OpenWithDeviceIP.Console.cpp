// Descriptions
//

#include "stdafx.h"

// for VwGigE
#include "VwGigE.h"
#include "VwCamera.h"




int _tmain(int argc, _TCHAR* argv[])
{
	VWSDK::DISCOVERY_INFO discoveryInfo;
	VWSDK::VwCamera*		pvwCamera = NULL;
	VWSDK::OBJECT_INFO*	pObjectInfo = NULL;

	// 0. Initialize
	_tprintf(_T("Step 0. Initialize....."));
	VWSDK::VwGigE	vwGigE;
	vwGigE.UserLogging(_T("Sample Code"), __VWFILE__, __VWFUNCTION__, __VWLINE__,
		_T("You can see this message in a tool called SpiderLogger.exe"));

	VWSDK::RESULT ret = vwGigE.Open();
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	// 1. Input device IP
	_tprintf(_T("Step 1. Input Device IP.....\n"));
	_tprintf(_T("ex)169.254.10.1 \n"));

	char pchIP[80] = { 0, };
	gets_s(pchIP, sizeof(pchIP));

	
	// 2. Discovery Device
	_tprintf(_T("Step 2. Discovery Device....."));
	ret = vwGigE.Discovery();
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

	_tprintf(_T("OK\n"));


	// 3. Get Discovery Information
	_tprintf(_T("Step 3. Get Discovery Information.....\n"));


	UINT unCamIndex = 0;
	BOOL bFind = FALSE;
	UINT unCamNum = 0;

	ret = vwGigE.GetNumCameras(&unCamNum);
	if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;


	for (UINT i = 0; i < unCamNum; i++)
	{
		VWSDK::VWCAMERA_INFO camInfo;
		ret = vwGigE.GetCameraInfo(i, &camInfo);
		if (ret != VWSDK::RESULT_SUCCESS)	goto EXIT;

		printf("\t");
		printf(camInfo.name);
		printf("\n");

		printf("\t");
		printf(camInfo.ip);
		printf("\n");

		printf("\t");
		printf(camInfo.mac);
		printf("\n");

		if (strcmp(camInfo.ip, pchIP) == 0)
		{
			bFind = TRUE;
			unCamIndex = camInfo.index;

			break;
		}

	}


	if (bFind == FALSE)
	{
		_tprintf(_T("Not found a Device....\n"));

		goto EXIT;
	}

	_tprintf(_T("OK\n"));
	

	// 4. Open device
	_tprintf(_T("Step 4. Open Device....."));
	pObjectInfo = new VWSDK::OBJECT_INFO;


	ret = vwGigE.OpenCamera(unCamIndex/*Device Index*/,
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


	return 0;

	
}


