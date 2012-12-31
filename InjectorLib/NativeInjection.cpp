#include "Stdafx.h"

#include "EnsureCleanup.h"
using Hades::Windows::EnsureCloseHandle;
using Hades::Windows::EnsureFreeLibrary;
using Hades::Windows::EnsureVirtualFree;


bool InjectAndRunNative(HANDLE process, std::wstring& nativeDllPath,
	std::wstring& nativeFunction, std::wstring& nativeArgs, DWORD& returnCode)
{
	if(!process)
	{
		returnCode = 1;
		return false;
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
