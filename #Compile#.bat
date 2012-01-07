del .\questor\bin\debug\*.* /Q
del .\questor.modules\bin\debug\*.* /Q
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\gotobm\GoToBM.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\BuyLPI\BuyLPI.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\QuestorManager\QuestorManager.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\questor\Questor.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\Questor.Modules\Questor.Modules.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\updateinvtypes\UpdateInvTypes.csproj
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\valuedump\ValueDump.csproj
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild .\questorstatistics\QuestorStatistics.csproj
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild .\QuestorSettings\QuestorSettings.csproj
mkdir output
del .\output\*.exe /Q
del .\output\*.dll /Q
copy .\questor\bin\debug\*.exe .\output\*.exe
copy .\questor\bin\debug\*.dll .\output\*.dll
copy .\gotobm\bin\debug\*.exe .\output\*.exe
copy .\questorstatistics\bin\debug\*.exe .\output\*.exe
copy .\updateinvtypes\bin\debug\*.exe .\output\*.exe
copy .\valuedump\bin\debug\*.exe .\output\*.exe
copy .\BuyLPI\bin\debug\*.exe .\output\*.exe
copy .\QuestorManager\bin\debug\*.exe .\output\*.exe
copy .\QuestorSettings\bin\debug\*.exe .\output\*.exe
pause