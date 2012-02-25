@Echo off
set innerspacedotnetdirectory="..\..\innerspace\.net programs\"

copy /y .\output\*.exe %innerspacedotnetdirectory% 
copy /y .\output\*.dll %innerspacedotnetdirectory% 
copy /y .\output\*.xml %innerspacedotnetdirectory%  
pause
