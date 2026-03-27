// CustomListCtrl.cpp : implementation file
//

#include "stdafx.h"
#include "CustomListCtrl.h"


// CCustomListCtrl

IMPLEMENT_DYNAMIC(CCustomListCtrl, CListCtrl)

CCustomListCtrl::CCustomListCtrl()
{

}

CCustomListCtrl::~CCustomListCtrl()
{
}


BEGIN_MESSAGE_MAP(CCustomListCtrl, CListCtrl)
END_MESSAGE_MAP()



// CCustomListCtrl message handlers

void CCustomListCtrl::PreSubclassWindow()
{
	DWORD	dwStyle;
	dwStyle = GetStyle();
	dwStyle != LVS_SHOWSELALWAYS;

	ModifyStyle( LVS_TYPEMASK, dwStyle );

	CListCtrl::PreSubclassWindow();
}