@Echo off
del .\questor\bin\debug\*.* /Q
del .\questor.modules\bin\debug\*.* /Q
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\BuyLPI\BuyLPI.csproj
Echo Done building BUYLPI - see above for any errors - 1 of 7 builds
pause
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\questor\Questor.csproj
Echo Done building Questor - see above for any errors - 2 of 7 builds
pause
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\Questor.Modules\Questor.Modules.csproj
Echo Done building Questor.Modules - see above for any errors - 3 of 7 builds
pause
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\updateinvtypes\UpdateInvTypes.csproj
Echo Done building UpdateInvTypes - see above for any errors - 4 of 7 builds
pause
c:\Windows\Microsoft.NET\Framework\v3.5\msbuild .\valuedump\ValueDump.csproj
Echo Done building Valuedump - see above for any errors - 5 of 7 builds
pause
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild .\QuestorManager\QuestorManager.csproj
Echo Done building QuestorManager - see above for any errors - 6 of 7 builds
pause
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild .\questorstatistics\QuestorStatistics.csproj
Echo Done building QuestorStatistics - see above for any errors - 7 of 7 builds
pause
::C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild .\QuestorSettings\QuestorSettings.csproj
::Echo Done building QuestorSettings - see above for any errors - 8 of 8 builds
::pause
if not exist output mkdir output >>nul 2>>nul
:: Echo deleting old build from the output directory
del .\output\*.exe /Q >>nul 2>>nul
del .\output\*.dll /Q >>nul 2>>nul
:: Echo Adding new build to the output directory
copy .\questor\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\questor\bin\debug\*.dll .\output\*.dll >>nul 2>>nul
copy .\gotobm\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\questorstatistics\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\updateinvtypes\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\valuedump\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\BuyLPI\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\Traveler\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
copy .\QuestorManager\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
::copy .\QuestorSettings\bin\debug\*.exe .\output\*.exe >>nul 2>>nul
Echo.
Echo use #TransferToLiveCopy#.bat to move the new build into place for testing 
Echo.
pause
