@Echo off
::set releasetype=Release
set releasetype=Debug
::
:: path to msbuild compiler - do not include trailing slash
::
set msbuild35=%systemroot%\Microsoft.Net\FrameWork\v3.5\msbuild.exe
set msbuild4=%systemroot%\Microsoft.Net\FrameWork\v4.0.30319\msbuild.exe
::

::
:: clear existing DLLs and EVEs from the previous build
::
del .\questor\bin\debug\*.* /Q
del .\questor.modules\bin\debug\*.* /Q
::
:: Build Project 1
::
set nameofproject=BUYLPI
set csproj=.\BuyLPI\BuyLPI.csproj
"%msbuild35%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 1 of 7 builds
pause
::
:: Build Project 2
::
set nameofproject=Questor
set csproj=.\questor\Questor.csproj
"%msbuild35%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 2 of 7 builds
pause
::
:: Build Project 3
::
set nameofproject=Questor.Modules
set csproj=.\Questor.Modules\Questor.Modules.csproj
"%msbuild35%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 3 of 7 builds
pause
::
:: Build Project 4
::
set nameofproject=updateinvtypes
set csproj=.\updateinvtypes\UpdateInvTypes.csproj
"%msbuild35%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 4 of 7 builds
pause
::
:: Build Project 5
::
set nameofproject=valuedump
set csproj=.\valuedump\ValueDump.csproj
"%msbuild35%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 5 of 7 builds
pause
::
:: Build Project 6
::
set nameofproject=QuestorManager
set csproj=.\QuestorManager\QuestorManager.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 6 of 7 builds
pause
::
:: Build Project 7
::
set nameofproject=QuestorStatistics
set csproj=.\questorstatistics\QuestorStatistics.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 7 of 7 builds
pause

::
:: Build Project 8
::
::set nameofproject=QuestorSettings
::set csproj=.\QuestorSettings\QuestorSettings.csproj
::%pathtomsbuild4%\msbuild %%csproj% /p:configuration="%releasetype%" /target:Clean;Build
::Echo Done building %nameofproject% - see above for any errors - 7 of 7 builds
::pause
if not exist output mkdir output >>nul 2>>nul
:: Echo deleting old build from the output directory
del .\output\*.exe /Q >>nul 2>>nul
del .\output\*.dll /Q >>nul 2>>nul
del .\output\*.xml /Q >>nul 2>>nul

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
Echo Copying mostly static files...
copy .\questor\invtypes.xml .\output\
copy .\questor\ShipTargetValues.xml .\output\
copy .\questor\factions.xml .\output\
copy .\questor\settings.xml .\output\settings-template-rename-to-charactername.xml
Echo.
Echo use #TransferToLiveCopy#.bat to move the new build into place for testing 
Echo.
pause
