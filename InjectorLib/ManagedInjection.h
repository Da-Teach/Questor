#pragma once

#pragma once

#include <Windows.h>
#include <string>

bool InjectAndRunManaged(HANDLE process, std::wstring& netVersion, std::wstring& netAssemblyPath,
	std::wstring& netAssemblyMethod, std::wstring& netAssemblyArgs, DWORD& returnCode);
