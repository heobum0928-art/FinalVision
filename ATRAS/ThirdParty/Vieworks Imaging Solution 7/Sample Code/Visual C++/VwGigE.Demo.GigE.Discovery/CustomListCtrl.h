#pragma once


// CCustomListCtrl

class CCustomListCtrl : public CListCtrl
{
	DECLARE_DYNAMIC(CCustomListCtrl)

public:
	CCustomListCtrl();
	virtual ~CCustomListCtrl();

protected:
	DECLARE_MESSAGE_MAP()
	virtual void PreSubclassWindow();
public:
	afx_msg void OnNMCustomdraw(NMHDR *pNMHDR, LRESULT *pResult);
};


