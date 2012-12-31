#include <Windows.h>
#include <mscoree.h>
#include <metahost.h>
#include <vector>
#include "Injection.h"
#include "ManagedInjection.h"
#include "NativeInjection.h"

#include "EnsureCleanup.h"
using Hades::Windows::EnsureRelease;
using Hades::Windows::EnsureCloseHandle;

EnsureRelease<ICLRMetaHost> MetaHost;

std::wstring FindNetRuntimeVersion()
{
	std::wstring notFound = L"n/a";

	if(FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&MetaHost)))
		return notFound;

	EnsureRelease<IEnumUnknown> Runtimes;
	if(FAILED(MetaHost->EnumerateInstalledRuntimes(&Runtimes)))
		return notFound;

	EnsureRelease<IUnknown> Runtime;
	EnsureRelease<ICLRRuntimeInfo> Latest;
	std::wstring LatestVersion;

	while(Runtimes->Next(1, &Runtime, NULL) == S_OK)
	{
		EnsureRelease<ICLRRuntimeInfo> Current;
		if(SUCCEEDED(Runtime->QueryInterface(IID_PPV_ARGS(&Current))))
		{
			if(!Latest)
			{
				if(SUCCEEDED(Current->QueryInterface(IID_PPV_ARGS(&Latest))))
				{
					DWORD tmp = MAX_PATH * sizeof(WCHAR);
					std::vector<WCHAR> tmpString(MAX_PATH);
					Latest->GetVersionString(tmpString.data(), &tmp);
					LatestVersion = tmpString.data();
				}
			}
			else
			{
				DWORD tmp = MAX_PATH * sizeof(WCHAR);
				std::vector<WCHAR> tmpString(MAX_PATH);
				if(SUCCEEDED(Current->GetVersionString(tmpString.data(), &tmp)))
				{
					std::wstring CurrentVersion = tmpString.data();
					if(CurrentVersion.compare(LatestVersion) > 0)
					{
						LatestVersion = CurrentVersion;
						Latest = NULL; // Force a release
						Current->QueryInterface(IID_PPV_ARGS(&Latest));
					}
				}
			}
		}
	}

	return LatestVersion;
}

int IsDllManaged(std::wstring& filename)
{
	if(!MetaHost)
		// Create our ICLRMetaHost interface
		FindNetRuntimeVersion();
	if(!MetaHost)
		return -1;

	std::vector<WCHAR> buffer(MAX_PATH);
	DWORD size = MAX_PATH * sizeof(WCHAR);
	HRESULT hr = MetaHost->GetVersionFromFile(filename.c_str(), buffer.data(), &size);
	if(hr == S_OK)
		// DLL is managed
		return 1;
	else if(hr == 0x8007000B)
		// "Bad image format", assume DLL is native code
		return 0;
	else
		// other errors
		return -1;
}

bool InjectAndRun(DWORD pid, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode)
{
	EnsureCloseHandle Process = OpenProcess(pid);
	return InjectAndRun(Process, dll, function, args, returnCode);
}

bool InjectAndRun(HANDLE process, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode)
{
	std::wstring runtimeVersion = FindNetRuntimeVersion();
	if(runtimeVersion == L"n/a")
		// No usable net runtime found.. Don't think this can ever happen,
		// but we check anyways.
		return false;

	if(!MetaHost)
		// This shouldn't happen either..
		return false;

	
	int i = IsDllManaged(dll);
	if(i == 1)
		return InjectAndRunManaged(process, runtimeVersion, dll, function, args, returnCode);
	else if(i == 0)
	{
		// "Bad image format", assume DLL is native code
		return InjectAndRunNative(process, dll, function, args, returnCode);
	}

	return false;
}

bool LaunchAndInject(std::wstring& exe, std::wstring& dll, std::wstring& function, std::wstring& args, DWORD& returnCode)
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	memset(&si, 0, sizeof(si));
	si.cb = sizeof(si);
	memset(&pi, 0, sizeof(pi));
	if(CreateProcess(exe.c_str(), NULL, NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, NULL, &si, &pi) == 0)
	{
		returnCode = 1;
		return false;
	}

	EnsureCloseHandle Process = pi.hProcess;
	EnsureCloseHandle Thread = pi.hThread;

	bool b = InjectAndRun(Process, dll, function, args, returnCode);
	ResumeThread(Thread);
	return b;
}
