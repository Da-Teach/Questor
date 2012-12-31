// InjectorLib.h

#pragma once

using namespace System;

namespace Injector {

	public ref class InjectorLib
	{
	public:
		InjectorLib();

		bool InjectAndRun(System::UInt32 Pid, System::String^ DLLPath, System::String^ FunctionName, System::String^ Arguments, System::UInt32 %ReturnCode);
		bool LaunchAndInject(System::String^ EXEPath, System::String^ DLLPath, System::String^ FunctionName, System::String^ Arguments, System::UInt32 %ReturnCode);
		int IsDllManaged(System::String^ DLLPath);
	};
}
