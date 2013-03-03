#include "Stdafx.h"

#include "EnsureCleanup.h"
using Hades::Windows::EnsureCloseHandle;
using Hades::Windows::EnsureFreeLibrary;
using Hades::Windows::EnsureVirtualFree;


BOOL SetPrivilege( HANDLE hToken, LPCTSTR lpszPrivilege, BOOL bEnablePrivilege ) 
{
    TOKEN_PRIVILEGES tp;
    LUID luid;

    if ( !LookupPrivilegeValue( NULL, lpszPrivilege, &luid ) )
    {
        return FALSE; 
    }

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    if (bEnablePrivilege)
	{
        tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	}
    else
	{
        tp.Privileges[0].Attributes = 0;
	}

    if( !AdjustTokenPrivileges( hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), (PTOKEN_PRIVILEGES) NULL, (PDWORD) NULL) )
    { 
          return FALSE; 
    } 

    if( GetLastError() == ERROR_NOT_ALL_ASSIGNED )
    {
          return FALSE;
    } 

    return TRUE;
}


bool InjectAndRunNative(HANDLE process, std::wstring& nativeDllPath,
	std::wstring& nativeFunction, std::wstring& nativeArgs, DWORD& returnCode)
{
    LPVOID lpMsgBuf;

	if(!process)
	{
		returnCode = 1;
		return false;
	}

	HANDLE hToken;
	if( OpenProcessToken( GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken ) == false )
	{
	    FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) &lpMsgBuf, 0, NULL );
		MessageBox(NULL,(LPCWSTR)lpMsgBuf,L"OpenProcessToken() error",MB_OK);
	}
	else
	{
		if( SetPrivilege( hToken, SE_DEBUG_NAME, TRUE ) == false )
		{
			FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) &lpMsgBuf, 0, NULL );
			MessageBox(NULL,(LPCWSTR)lpMsgBuf,L"SetPrivilege() error",MB_OK);
		}
		CloseHandle(hToken);
	}

	EnsureFreeLibrary Dll = LoadLibrary(nativeDllPath.c_str());
	if(!Dll)
	{
		returnCode = 2;
		return false;
	}
	
	LPVOID address_LoadLibraryW = GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
	LPVOID address_NativeFunction = GetProcAddress(Dll, std::string(nativeFunction.begin(), nativeFunction.end()).c_str());
	if(address_LoadLibraryW == 0 || (address_NativeFunction == 0 && nativeFunction.length() > 0))
	{
		returnCode = 3;
		return false;
	}
	address_NativeFunction = (LPVOID)((DWORD)address_NativeFunction - (DWORD)(HMODULE)Dll);
	
	DWORD allocSize = nativeDllPath.length() * sizeof(WCHAR) + 1;
	EnsureVirtualFree remoteBuffer(VirtualAllocEx(process, NULL, allocSize, MEM_COMMIT, PAGE_READWRITE), process);
	if(!remoteBuffer)
	{
		returnCode = 4;
		return false;
	}
	
	if(!WriteProcessMemory(process, remoteBuffer, nativeDllPath.c_str(), allocSize, NULL))
	{
		returnCode = 5;
		return false;
	}
	
	EnsureCloseHandle Thread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)address_LoadLibraryW, remoteBuffer, NULL, NULL);
	if(!Thread)
	{
	    FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) &lpMsgBuf, 0, NULL );
		MessageBox(NULL,(LPCWSTR)lpMsgBuf,L"CreateRemoteThread() error",MB_OK);
		returnCode = 6;
		return false;
	}

	WaitForSingleObject(Thread, INFINITE);
	GetExitCodeThread(Thread, &returnCode);
	if(returnCode == 0)
	{
		returnCode = 7;
		return false;
	}
	address_NativeFunction = (BYTE*)address_NativeFunction + returnCode;

	if(nativeFunction.length() > 0)
	{
		if(nativeArgs.length() > 0)
		{
			allocSize = nativeArgs.length() * sizeof(WCHAR) + 1;
			EnsureVirtualFree argumentBuffer(VirtualAllocEx(process, NULL, allocSize, MEM_COMMIT, PAGE_READWRITE), process);
			if(!argumentBuffer)
			{
				returnCode = 8;
				return false;
			}
			if(!WriteProcessMemory(process, argumentBuffer, nativeArgs.c_str(), allocSize, NULL))
			{
				returnCode = 9;
				return false;
			}
			Thread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)address_NativeFunction, argumentBuffer, NULL, NULL);
			if(!Thread)
			{
				returnCode = 10;
				return false;
			}
			WaitForSingleObject(Thread, INFINITE);
			GetExitCodeThread(Thread, &returnCode);
		}
		else
		{
			Thread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)address_NativeFunction, NULL, NULL, NULL);
			if(!Thread)
			{
				returnCode = 11;
				return false;
			}
			WaitForSingleObject(Thread, INFINITE);
			GetExitCodeThread(Thread, &returnCode);
		}
	}

	return true;
}
