set runPath=%~d0%~p0

set srcFolder=%runPath%bin\Debug\net5.0\
set srcScriptsFolder=%srcFolder%\Scripts\
set targetFolder=c:\Mixa\ItemsReport\
set targetScriptsFolder=%targetFolder%Scripts\

if not exist "%targetFolder%" mkdir "%targetFolder%"
if not exist "%targetScriptsFolder%" mkdir "%targetScriptsFolder%"

copy %srcFolder%CommonCode.dll %targetFolder%
copy %srcFolder%ItemsReport.dll %targetFolder%
copy %srcFolder%ItemsReport.exe %targetFolder%
copy %srcFolder%ItemsReport.runtimeconfig.json %targetFolder%
copy %srcFolder%CommonCode.pdb %targetFolder%
copy %srcFolder%ItemsReport.pdb %targetFolder%
copy %srcFolder%PAT.txt %targetFolder%

copy %srcScriptsFolder%FirstScript.js %targetScriptsFolder%
copy %srcScriptsFolder%FirstScript.js.map %targetScriptsFolder%

pause