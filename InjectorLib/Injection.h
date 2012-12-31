#pragma once

#include <Windows.h>
#include <string>

// DWORD alignment
_inline DWORD DWAlign(DWORD offset)
{
	return offset % 4 == 0 ? offset : offset + (4 - offset % 4);
}

// OpenProcess helper function
_inline HANDLE OpenProcess(DWORD pid)
{
	return OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
}

bool InjectAndRun(DWORD pid, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode);
bool InjectAndRun(HANDLE process, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode);
bool LaunchAndInject(std::wstring& exe, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode);
int IsDllManaged(std::wstring& filename);
