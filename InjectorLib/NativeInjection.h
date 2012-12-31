#pragma once

#include <Windows.h>
#include <string>

bool InjectAndRunNative(HANDLE process, std::wstring& nativeDllPath,
	std::wstring& nativeFunction, std::wstring& nativeArgs, DWORD& returnCode);
