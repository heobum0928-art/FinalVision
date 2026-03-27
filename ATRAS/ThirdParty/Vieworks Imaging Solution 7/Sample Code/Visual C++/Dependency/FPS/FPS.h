/********************************************************************
	created:	2013/11/07
	created:	7:11:2013   15:21
	filename: 	D:\Source\MV_VIS\Dependency\FPS\FPS.h
	file path:	D:\Source\MV_VIS\Dependency\FPS
	file base:	FPS
	file ext:	h
	author:		dong-uk, Lee
	
	purpose:	Calcurate FPS.
*********************************************************************/

#pragma once


#include <list>


class CFPS
{
public:
	CFPS(void);
	~CFPS(void);

	
	void vBegin();
	float fEnd();


private:
	void Init();

private:
	/// Add::20120830, Dong-Uk Lee, List that stores an end time.
	std::list<LARGE_INTEGER>		m_imageTimeStamps;

	LARGE_INTEGER	m_n64Freq;
	LARGE_INTEGER	m_n64Dur;
	LARGE_INTEGER	m_n64Start;
	LARGE_INTEGER	m_n64End;
	double			m_periodMs;

	const UINT		FPS_LIMINT;
};

