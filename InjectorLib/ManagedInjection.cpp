#include "Stdafx.h"
#include <mscoree.h>
#include <metahost.h>
#include <vector>

#include "AsmJit/AsmJit.h"
#include "AsmJit/Assembler.h"
using namespace AsmJit; // Global namespace polution! Yarr!

#include "EnsureCleanup.h"
using Hades::Windows::EnsureRelease;
using Hades::Windows::EnsureCloseHandle;
using Hades::Windows::EnsureFreeLibrary;
using Hades::Windows::EnsureVirtualFree;

bool InjectAndRunManaged(HANDLE process, std::wstring& netVersion, std::wstring& netAssemblyPath,
	std::wstring& netAssemblyMethod, std::wstring& netAssemblyArgs, DWORD& returnCode)
{
	if(!process)
	{
		returnCode = 1;
		return false;
	}

	// split netAssemblyMethod string into class and method names
	unsigned int idx = netAssemblyMethod.find_last_of('.');
	std::wstring MethodName = netAssemblyMethod.substr(idx+1);
	std::wstring tmp = netAssemblyMethod;
	tmp.erase(idx);
	std::wstring ClassName = tmp;

	EnsureVirtualFree address(VirtualAllocEx(process, NULL, 0x10000, MEM_COMMIT, PAGE_EXECUTE_READWRITE), process);
	DWORD offset = 4;
	if(!address)
	{
		returnCode = 2;
		return false;
	}

	std::wstring RuntimeDllName = L"mscoree.dll";
	std::string CLRCreateInstanceName = "CLRCreateInstance";

	LPVOID address_LoadLibraryW = GetProcAddress(GetModuleHandle(L"kernel32"), "LoadLibraryW");
	LPVOID address_GetProcAddress = GetProcAddress(GetModuleHandle(L"kernel32"), "GetProcAddress");

	// Offset of CLRCreateInstance from dll base address
	EnsureFreeLibrary Runtime = LoadLibrary(RuntimeDllName.c_str());
	if(!Runtime)
	{
		returnCode = 3;
		return false;
	}
	DWORD CreateInstanceAddress = reinterpret_cast<DWORD>(GetProcAddress(Runtime, CLRCreateInstanceName.c_str())) - (DWORD)(HMODULE)Runtime;

	// filename of the net runtime dll (mscoree.dll)
	BYTE* address_DLLName = (BYTE*)address + offset;
	DWORD written = 0;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, RuntimeDllName.c_str(), RuntimeDllName.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 4;
		return false;
	}
	offset = DWAlign(offset + written);

	// runtime version to use
	if(!WriteProcessMemory(process, (BYTE*)address+offset, netVersion.c_str(), netVersion.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 5;
		return false;
	}
	BYTE* address_VersionString = (BYTE*)address+offset;
	offset = DWAlign(offset + written);
	offset+= 4;

	BYTE* address_netAssemblyDll = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, netAssemblyPath.c_str(), netAssemblyPath.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 6;
		return false;
	}
	offset = DWAlign(offset + written);
	
	BYTE* address_netAssemblyClass = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, ClassName.c_str(), ClassName.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 7;
		return false;
	}
	offset = DWAlign(offset + written);
	
	BYTE* address_netAssemblyMethod = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, MethodName.c_str(), MethodName.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 8;
		return false;
	}
	offset = DWAlign(offset + written);
	
	BYTE* address_netAssemblyArgs = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, netAssemblyArgs.c_str(), netAssemblyArgs.length() * sizeof(WCHAR) + 1, &written))
	{
		returnCode = 9;
		return false;
	}
	offset = DWAlign(offset + written);

	// COM object GUIDs (should batch these into a single write)
	BYTE* address_CLSID_CLRMetaHost = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, &CLSID_CLRMetaHost, 16, NULL))
	{
		returnCode = 10;
		return false;
	}
	offset+= 16;
	BYTE* address_IID_ICLRMetaHost = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, &IID_ICLRMetaHost, 16, NULL))
	{
		returnCode = 11;
		return false;
	}
	offset+= 16;
	BYTE* address_IID_ICLRRuntimeInfo = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, &IID_ICLRRuntimeInfo, 16, NULL))
	{
		returnCode = 12;
		return false;
	}
	offset+= 16;
	BYTE* address_CLSID_CLRRuntimeHost = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, &CLSID_CLRRuntimeHost, 16, NULL))
	{
		returnCode = 13;
		return false;
	}
	offset+= 16;
	BYTE* address_IID_ICLRRuntimeHost = (BYTE*)address+offset;
	if(!WriteProcessMemory(process, (BYTE*)address+offset, &IID_ICLRRuntimeHost, 16, NULL))
	{
		returnCode = 14;
		return false;
	}
	offset+= 16;

	// Remote LoadLibrary("mscoree.dll")
	EnsureCloseHandle Thread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)address_LoadLibraryW, address_DLLName, NULL, NULL);
	if(!Thread)
	{
		returnCode = 15;
		return false;
	}
	
	WaitForSingleObject(Thread, INFINITE);
	GetExitCodeThread(Thread, &returnCode);
	if(!returnCode)
	{
		returnCode = 16;
		return false;
	}

	// Add dll base address to CLRCreateInstance offset
	CreateInstanceAddress+= returnCode;
	
	// stack variables for the injected code
	DWORD stack_MetaHost = -0x4, stack_RuntimeInfo = -0x8, stack_RuntimeHost = -0xC, stack_IsStarted = -0x10, stack_StartupFlags = -0x14;
	DWORD stack_returnCode = -0x18;

	// Scary assembler code incoming!
	Assembler a;
	Label L_Exit = a.newLabel();
	Label L_Error1 = a.newLabel();
	Label L_Error2 = a.newLabel();
	Label L_Error3 = a.newLabel();
	Label L_Error4 = a.newLabel();
	Label L_Error5 = a.newLabel();
	Label L_Error6 = a.newLabel();
	Label L_SkipStart = a.newLabel();
	Label L_ReleaseInterface = a.newLabel();

	// function prolog
	a.push(ebp);
	a.mov(ebp, esp);
	a.sub(esp, 0x1C);
	a.xor_(esi, esi);

	// CLRCreateInstance()
	a.lea(edx, dword_ptr(ebp, stack_MetaHost));
	a.mov(dword_ptr(ebp, stack_MetaHost), esi);
	a.push(edx);
	a.push(reinterpret_cast<DWORD>(address_IID_ICLRMetaHost));
	a.push(reinterpret_cast<DWORD>(address_CLSID_CLRMetaHost));
	a.mov(eax, CreateInstanceAddress);
	a.call(eax);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error1);

	// pMetaHost->GetRuntime()
	a.mov(eax, dword_ptr(ebp, stack_MetaHost));
	a.lea(edx, dword_ptr(ebp, stack_RuntimeInfo));
	a.mov(dword_ptr(ebp, stack_RuntimeInfo), esi);
	a.push(edx);
	a.push(reinterpret_cast<DWORD>(address_IID_ICLRRuntimeInfo));
	a.mov(ecx, dword_ptr(eax));
	a.push(reinterpret_cast<DWORD>(address_VersionString));
	a.push(eax);
	a.mov(eax, dword_ptr(ecx, 0xC));
	a.call(eax);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error2);

	// pRuntimeInterface->IsStarted()
	a.mov(eax, dword_ptr(ebp, stack_RuntimeInfo));
	a.mov(ecx, dword_ptr(eax));
	a.lea(edx, dword_ptr(ebp, stack_StartupFlags));
	a.push(edx);
	a.lea(edx, dword_ptr(ebp, stack_IsStarted));
	a.push(edx);
	a.push(eax);
	a.mov(eax, dword_ptr(ecx, 0x38));
	a.call(eax);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error3);

	// pRuntimeTime->GetInterface()
	a.mov(eax, dword_ptr(ebp, stack_RuntimeInfo));
	a.lea(edx, dword_ptr(ebp, stack_RuntimeHost));
	a.mov(dword_ptr(ebp, stack_RuntimeHost), esi);
	a.push(edx);
	a.push(reinterpret_cast<DWORD>(address_IID_ICLRRuntimeHost));
	a.push(reinterpret_cast<DWORD>(address_CLSID_CLRRuntimeHost));
	a.mov(ecx, dword_ptr(eax));
	a.push(eax);
	a.mov(eax, dword_ptr(ecx, 0x24));
	a.call(eax);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error4);

	// pRuntimeHost->Start()
	a.cmp(dword_ptr(ebp, stack_IsStarted), esi);
	a.jne(L_SkipStart); // jump if already started
	a.mov(eax, dword_ptr(ebp, stack_RuntimeHost));
	a.mov(ecx, dword_ptr(eax));
	a.mov(edx, dword_ptr(ecx, 0xC));
	a.push(eax);
	a.call(edx);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error5);

	// pRuntimeHost->ExecuteInDefaultAppDomain()
	a.bind(L_SkipStart);
	a.lea(edi, dword_ptr(ebp, stack_returnCode));
	a.push(edi);
	a.push(reinterpret_cast<DWORD>(address_netAssemblyArgs));
	a.push(reinterpret_cast<DWORD>(address_netAssemblyMethod));
	a.push(reinterpret_cast<DWORD>(address_netAssemblyClass));
	a.push(reinterpret_cast<DWORD>(address_netAssemblyDll));
	a.mov(eax, dword_ptr(ebp, stack_RuntimeHost));
	a.mov(edx, dword_ptr(eax));
	a.push(eax);
	a.mov(eax, dword_ptr(edx, 44));
	a.call(eax);
	// success?
	a.test(eax, eax);
	a.jnz(L_Error6);

	// Release unneeded interfaces
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeHost));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeInfo));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);

	// Write the managed code's return value to the first DWORD
	// in the allocated buffer
	a.mov(eax, dword_ptr(ebp, stack_returnCode));
	a.mov(edx, reinterpret_cast<DWORD>((LPVOID)address));
	a.mov(dword_ptr(edx), eax);
	a.mov(eax, 0);

	// stack restoration
	a.bind(L_Exit);
	a.mov(esp, ebp);
	a.pop(ebp);
	a.ret();

	// CLRCreateInstance() failed
	a.bind(L_Error1);
	a.mov(eax, 1);
	a.jmp(L_Exit);

	// pMetaHost->GetRuntime() failed
	a.bind(L_Error2);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);
	a.mov(eax, 2);
	a.jmp(L_Exit);

	// pRuntimeInterface->IsStarted() failed
	a.bind(L_Error3);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeInfo));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);
	a.mov(eax, 3);
	a.jmp(L_Exit);

	// pRuntimeTime->GetInterface() failed
	a.bind(L_Error4);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeInfo));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);
	a.mov(eax, 4);
	a.jmp(L_Exit);

	// pRuntimeHost->Start() failed
	a.bind(L_Error5);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeHost));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeInfo));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);
	a.mov(eax, 5);
	a.jmp(L_Exit);

	// pRuntimeHost->ExecuteInDefaultAppDomain() failed
	a.bind(L_Error6);
	a.push(eax);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeHost));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_RuntimeInfo));
	a.call(L_ReleaseInterface);
	a.mov(ecx, dword_ptr(ebp, stack_MetaHost));
	a.call(L_ReleaseInterface);
	a.mov(eax, 6);
	a.pop(eax);
	a.jmp(L_Exit);

	// void __fastcall ReleaseInterface(IUnknown* pInterface)
	a.bind(L_ReleaseInterface);
	a.mov(eax, ecx);
	a.mov(ecx, dword_ptr(eax));
	a.mov(edx, dword_ptr(ecx, 8));
	a.push(eax);
	a.call(edx);
	a.ret();

	// write JIT code to target
	DWORD codeSize = a.getCodeSize();
	BYTE* codeAddress = (BYTE*)address + offset;
	std::vector<BYTE> codeBuffer(codeSize);
	a.relocCode(codeBuffer.data(), (DWORD)codeAddress);
	if(!WriteProcessMemory(process, codeAddress, codeBuffer.data(), codeSize, NULL))
	{
		returnCode = 17;
		return false;
	}

	// run ze codez
	Thread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)codeAddress, NULL, NULL, NULL);
	if(!Thread)
	{
		returnCode = 18;
		return false;
	}
	WaitForSingleObject(Thread, INFINITE);

	// success = 0, everything else is a failure.
	GetExitCodeThread(Thread, &returnCode);

	if(returnCode != 0)
	{
		returnCode += 18;
		return false;
	}

	// Get the managed return value
	ReadProcessMemory(process, address, &returnCode, 4, NULL);

	return true;
}
