// This is the main DLL file.

#include "stdafx.h"
#include <vcclr.h>
#include <msclr/marshal_cppstd.h>
#include "InjectorLib.h"
#include <sstream>

namespace Injector
{
	std::wstring ManagedToNativeString(System::String^ str)
	{
		msclr::interop::marshal_context context;
		std::wstring result = context.marshal_as<std::wstring, System::String^>(str);
		return result;
	}

	System::String^ NativeToManagedString(std::wstring& str)
	{
		msclr::interop::marshal_context context;
		System::String^ result = context.marshal_as<System::String^>(str);
		return result;
	}

	InjectorLib::InjectorLib()
	{
	}

	bool InjectorLib::InjectAndRun(System::UInt32 Pid, System::String^ DLLPath, System::String^ FunctionName, System::String^ Arguments, System::UInt32 %ReturnCode)
	{
		std::wstring dll = ManagedToNativeString(DLLPath);
		std::wstring func = ManagedToNativeString(FunctionName);
		std::wstring args = ManagedToNativeString(Arguments);

		DWORD retCode;
		bool b = ::InjectAndRun(Pid, dll, func, args, retCode);
		ReturnCode = retCode;
		return b;
	}

	bool InjectorLib::LaunchAndInject(System::String^ EXEPath, System::String^ DLLPath, System::String^ FunctionName, System::String^ Arguments, System::UInt32 %ReturnCode)
	{
		std::wstring exe = ManagedToNativeString(EXEPath);
		std::wstring dll = ManagedToNativeString(DLLPath);
		std::wstring func = ManagedToNativeString(FunctionName);
		std::wstring args = ManagedToNativeString(Arguments);

		DWORD retCode;
		bool b = ::LaunchAndInject(exe, dll, func, args, retCode);
		ReturnCode = retCode;
		return b;
	}

	int InjectorLib::IsDllManaged(System::String^ DLLPath)
	{
		std::wstring dll = ManagedToNativeString(DLLPath);
		return ::IsDllManaged(dll);
	}
}
