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
:: clear existing DLLs and EVEs from the previous build(s)
::
del .\bin\debug\*.* /Q
del .\bin\debug\*.* /Q
del .\bin\release\*.* /Q
del .\bin\release\*.* /Q
::
:: Build Project 1
::
set nameofproject=BUYLPI
set csproj=.\BuyLPI\BuyLPI.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 1 of 7 builds
pause
::
:: Build Project 2
::
set nameofproject=Questor
set csproj=.\questor\Questor.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 2 of 7 builds
pause
::
:: Build Project 3
::
set nameofproject=Questor.Modules
set csproj=.\Questor.Modules\Questor.Modules.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 3 of 7 builds
pause
::
:: Build Project 4
::
set nameofproject=updateinvtypes
set csproj=.\updateinvtypes\UpdateInvTypes.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 4 of 7 builds
pause
::
:: Build Project 5
::
set nameofproject=valuedump
set csproj=.\valuedump\ValueDump.csproj
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
Echo Done building %nameofproject% - see above for any errors - 5 of 7 builds
pause
::
:: Build Project 6
::
set nameofproject=QuestorManager
set csproj=.\QuestorManager\QuestorManager.csproj
::"%msbuild4%" "%csproj%" /p:configuration="%releasetype%" /target:Clean;Build
"%msbuild4%" "%csproj%" /p:configuration="%releasetype%"
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
::
:: DO NOT delete the XMLs as this is the ONLY directory they exist in now. 
::
::del .\output\*.xml /Q >>nul 2>>nul

::
:: Eventually all EXEs and DLLs will be in the following common directory...
::
copy .\bin\%releasetype%\*.exe .\output\ >>nul 2>>nul
copy .\bin\%releasetype%\*.dll .\output\ >>nul 2>>nul
::Echo Copying mostly static files...
::copy .\questor\invtypes.xml .\output\
::copy .\questor\ShipTargetValues.xml .\output\
::copy .\questor\factions.xml .\output\
::copy .\questor\settings.xml .\output\settings-template-rename-to-charactername.xml
Echo.
Echo use #TransferToLiveCopy#.bat to move the new build into place for testing 
Echo.
pause
