#pragma once
#include <Windows.h>

enum NotifyMsg
{
	WM_NotifyActivate = WM_APP + 1,
	WM_NotifyFocus,
	WM_NotifyCallWndProc,
	WM_NotifyGetMessage,
};

typedef struct _TRAY_ICON_DATAW_XP
{
	DWORD Signature;              // Explorer 自己的签名
	DWORD dwMessage;              // NIM_ADD / NIM_MODIFY / NIM_DELETE
	DWORD cbSize;                 // 发送方传入的 NOTIFYICONDATA.cbSize
	DWORD hWnd;                   // HWND（被强制当 DWORD 保存）
	UINT  uID;
	UINT  uFlags;
	UINT  uCallbackMessage;
	DWORD uIconID; 
	WCHAR szTip [128];
	DWORD dwState;
	DWORD dwStateMask;
	WCHAR szInfo [256];
	union
	{
		UINT uTimeout;
		UINT uVersion;
	};
	WCHAR szInfoTitle [64];
	DWORD dwInfoFlags;
	GUID guidItem; 
} TRAY_ICON_DATAW_XP;
typedef struct _TRAY_ICON_DATAW_VISTA
{
	DWORD Signature;
	DWORD dwMessage;
	DWORD cbSize;
	DWORD hWnd;
	UINT  uID;
	UINT  uFlags;
	UINT  uCallbackMessage;
	DWORD uIconID;
	WCHAR szTip [128];
	DWORD dwState;
	DWORD dwStateMask;
	WCHAR szInfo [256];
	union
	{
		UINT uTimeout;
		UINT uVersion;
	};
	WCHAR szInfoTitle [64];
	DWORD dwInfoFlags;
	GUID guidItem;
	HICON hBalloonIcon;

} TRAY_ICON_DATAW_VISTA;


typedef struct __SHELLWND_MAG
{
	LPARAM  lParam;
	WPARAM  wParam;
	UINT    message;
	HWND    hMsgWnd;
}SHELLWND_MAG, PSHELLWND_MAG;

typedef LONG (WINAPI* RtlGetVersionPtr)(PRTL_OSVERSIONINFOW);

bool IsNT6OrLater ()
{
	HMODULE hNtDll = GetModuleHandleW (L"ntdll.dll");
	if (!hNtDll) return false;
	RtlGetVersionPtr fnRtlGetVersion =
		(RtlGetVersionPtr)GetProcAddress (hNtDll, "RtlGetVersion");
	if (!fnRtlGetVersion)
		return false;
	RTL_OSVERSIONINFOW rovi = { 0 };
	rovi.dwOSVersionInfoSize = sizeof (rovi);
	if (fnRtlGetVersion (&rovi) != 0) return false;
	return rovi.dwMajorVersion >= 6;
}

EXTERN_C bool InstallCBTHook (HWND hNotifyWnd);
EXTERN_C bool UninstallCBTHook ();

EXTERN_C bool InstallCallWndProcHook (HWND hNotifyWnd, HWND hCaptureWnd);
EXTERN_C bool UninstallCallWndProcHook ();

EXTERN_C bool InstallGetMessageHook (HWND hNotifyWnd, HWND hCaptureWnd);
EXTERN_C bool UninstallGetMessageHook ();
