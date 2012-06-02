@Echo off
cls
:: delims is a TAB followed by a space
(set debug=false)
::Custom Path to innerspace if you have yours in another location
(set InnerSpacePath=..\..\innerspace\)
:: Path for 32bit OSs - likely Windows XP (but could be windows vista or windows 7, but not commonly)
::(set InnerSpacePath=%ProgramFiles%\InnerSpace\InnerSpace.exe)
:: Path for 64bit OSs - likely windows Vista or Windows 7
::(set InnerSpacePath=%ProgramFiles(x86)%\InnerSpace\InnerSpace.exe)
::
:: if you choose not to hard code the path above we will search the registry for your innerSpace directory
:: 
set scripturl=%~dp0
set scripturl=%scripturl:~0,-1%

if exist "%InnerSpacePath%" goto :dequote
if not exist "%InnerSpacePath%" Echo.
if not exist "%InnerSpacePath%" Echo [before registry] Unable to Find Innerspace Path [ %InnerSpacePath% ] attempting to lookup path using the windows registry
if not exist "%InnerSpacePath%" Echo.
if not exist "%InnerSpacePath%" (set InnerSpacePath=)
if not exist "%InnerSpacePath%" if "%debug%"=="true" Echo.
if not exist "%InnerSpacePath%" if "%debug%"=="true" echo [before registry] InnerSpacePath is now [ %InnerSpacePath% ] && pause
if not exist "%InnerSpacePath%" if "%debug%"=="true" Echo.
::
:: if the directory above did not exist then check the registry for where innerspace is loacted and use that
::
:registrysearch
if "%InnerSpacePath%"=="" if "%debug%"=="true" Echo [about to query registry] InnerSpacePath is [ %InnerSpacePath% ] - trying to query known registry keys for innerspace.exe
if "%InnerSpacePath%"=="" FOR /F "tokens=2* delims=	 " %%A IN ('REG QUERY "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\InnerSpace.exe" /v "Path"') DO (SET InnerSpacePath=%%B)
if "%InnerSpacePath%"=="" FOR /F "tokens=2* delims=	 " %%A IN ('REG QUERY "HKCU\Software\Microsoft\IntelliPoint\AppSpecific\InnerSpace.exe" /v "Path"') DO (SET InnerSpacePath=%%B)
if "%InnerSpacePath%"=="" FOR /F "tokens=2* delims=	 " %%A IN ('REG QUERY "HKCU\Software\Microsoft\IntelliType Pro\AppSpecific\InnerSpace.exe" /v "Path"') DO  (SET InnerSpacePath=%%B)
if "%InnerSpacePath%"=="" if exist "%programfiles(x86)%\Innerspace\Innerspace.exe" SET InnerSpacePath = %programfiles(x86)%\Innerspace\
if "%InnerSpacePath%"=="" if exist "%programfiles%\Innerspace\Innerspace.exe" SET InnerSpacePath = %programfiles%\Innerspace\
if "%InnerSpacePath%"=="" if exist "%userprofile%\Documents\Innerspace\Innerspace.exe" SET InnerSpacePath = %systemdrive%\Innerspace\
if "%InnerSpacePath%"=="" if exist "%systemdrive%\My Documents\Innerspace\Innerspace.exe" SET InnerSpacePath = %systemdrive%\Innerspace\
if "%InnerSpacePath%"=="" if exist "%userprofile%\Innerspace\Innerspace.exe" SET InnerSpacePath = %systemdrive%\Innerspace\
if "%InnerSpacePath%"=="" if exist "%systemdrive%\Innerspace\Innerspace.exe" SET InnerSpacePath = %systemdrive%\Innerspace\

if "%InnerSpacePath%"=="" goto :ERROR
if "%debug%"=="true" Echo [registry] After registry queries: InnerSpacePath is [ %InnerSpacePath% ] && pause
if "%debug%"=="true" echo [remove exe] if "%InnerSpacePath:~-15%"=="\InnerSpace.exe" (set InnerSpacePath=%InnerSpacePath:~0,-15%)
if "%InnerSpacePath:~-15%"=="\InnerSpace.exe" (set InnerSpacePath=%InnerSpacePath:~0,-15%)

:dequote
pushd
cd %scripturl%
call dequote.cmd InnerSpacePath
call dequote.cmd InnerSpacePath
popd

:setinnerspacedotnetdirectory
if exist "%InnerSpacePath%" if "%debug%"=="true" Echo (set innerspacedotnetdirectory="%InnerSpacePath%\.Net Programs\")
if exist "%InnerSpacePath%" set innerspacedotnetdirectory=%InnerSpacePath%\.Net Programs\
@echo [finished] Innerspace Path is: [%InnerSpacePath%]
@echo [finished] innerspacedotnetdirectory Path is: [%innerspacedotnetdirectory%]
if "%debug%"=="true" pause && echo ------------------------------------------ && echo ------------------------------------------
if not exist "%Innerspacedotnetdirectory%" goto :error

:dequote2
pushd
cd %scripturl%
call dequote.cmd innerspacedotnetdirectory
call dequote.cmd innerspacedotnetdirectory
popd

:CopyQuestor
@Echo.
@Echo Starting to copy EXE files from [.\output\*.exe] to [%innerspacedotnetdirectory%]
@Echo on
copy /y ".\output\*.exe" "%innerspacedotnetdirectory%"
@Echo off
@Echo.
@Echo Starting to copy DLL files from [.\output\*.dll] to [%innerspacedotnetdirectory%]
@Echo on
copy /y ".\output\*.dll" "%innerspacedotnetdirectory%"
@Echo off
@Echo.
@Echo.
@Echo Starting to copy debug files from [.\output\*.pdb] to [%innerspacedotnetdirectory%]
@Echo on
copy /y ".\output\*.pdb" "%innerspacedotnetdirectory%"
@Echo off
@Echo.

if "%debug%"=="true" pause && echo ------------------------------------------ && echo ------------------------------------------
::
if "%debug%"=="true" Echo on

:CopyXMLConfigFiles
@Echo.
@Echo *** always copy the template settings.xml file to [%innerspacedotnetdirectory%]
@Echo on
copy /y ".\output\settings.xml" "%innerspacedotnetdirectory%"
@Echo off
@Echo.
@Echo *** always copy factions.xml file to [%innerspacedotnetdirectory%]
copy /y ".\output\factions.xml" "%innerspacedotnetdirectory%"
@Echo.
@Echo *** only copy ShipTargetValues.xml if one does not already exist (it contains targeting data)
if not exist "%innerspacedotnetdirectory%ShipTargetValues.xml" copy /y ".\output\ShipTargetValues.xml" "%innerspacedotnetdirectory%"
@Echo.
@Echo *** only copy invtypes.xml if one does not already exist (it contains pricing data)
if not exist "%innerspacedotnetdirectory%invtypes.xml" copy /y ".\output\invtypes.xml" "%innerspacedotnetdirectory%"
@Echo off

goto :done

:error
echo ------------------------------------------ && echo ------------------------------------------
Echo.
Echo unable to find your innerspace directory via the 3 registry keys we tried
Echo InnerSpacePath is [ %InnerSpacePath% ]
Echo innerspacedotnetdirectory is [ %innerspacedotnetdirectory% ]
Echo      you can edit the script and change:
Echo                (set InnerSpacePath=..\..\innerspace\)
Echo      to the full path to your innerspace directory if needed. 
Echo.
Echo if you want to debug this script you can set debug=true near the top of the script
echo ------------------------------------------ && echo ------------------------------------------
pause
:done
echo [done copying questor related files to: %innerspacedotnetdirectory%]
pause
