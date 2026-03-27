#include <afxwin.h>         // MFC core and standard components
#include "FPS.h"


CFPS::CFPS(void)
	: FPS_LIMINT(30)
	, m_periodMs(0)

{
	Init();
}


CFPS::~CFPS(void)
{
}

void CFPS::Init()
{
 	ZeroMemory(&m_n64Dur,sizeof(LARGE_INTEGER));

	QueryPerformanceFrequency(&m_n64Freq);

	m_imageTimeStamps.clear();

}

void CFPS::vBegin()
{
	QueryPerformanceCounter(&m_n64Start);	
}

float CFPS::fEnd()
{
	/// Comment::20120830, Dong-Uk Lee, Calculating an average value of several frames.
	QueryPerformanceCounter(&m_n64End);

	m_n64Dur.QuadPart		= m_n64End.QuadPart - m_n64Start.QuadPart;
	m_n64Start.QuadPart		= m_n64End.QuadPart;

	/// Update::20120830, Dong-Uk Lee, The time it takes to acquire a single frame.
	m_periodMs = ( m_n64Dur.QuadPart * ((double)1000) ) / m_n64Freq.QuadPart;

	
	if( m_imageTimeStamps.size() > FPS_LIMINT)
		m_imageTimeStamps.pop_front();

	m_imageTimeStamps.push_back(m_n64End);

	float fCurrentFPS = 0.0;

	__int64 diff = m_n64End.QuadPart - m_imageTimeStamps.begin()->QuadPart;

	if( diff > 0 )
	{
		/// Update::20120830, Dong-Uk Lee, The number of frames that are obtained in 1 second.
		fCurrentFPS = (m_n64Freq.QuadPart * double(m_imageTimeStamps.size()-1 ) ) / diff;
	}

	return fCurrentFPS;
}
