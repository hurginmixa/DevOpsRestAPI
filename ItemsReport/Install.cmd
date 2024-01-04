set runPath=%~d0%~p0

set srcFolder=%runPath%bin\Debug\net6.0\
set targetFolder=c:\Mixa\ItemsReport\

if not exist "%targetFolder%" mkdir "%targetFolder%"

copy %srcFolder%CommonCode.dll %targetFolder%
copy %srcFolder%ItemsReport.dll %targetFolder%
copy %srcFolder%ItemsReport.exe %targetFolder%
copy %srcFolder%ItemsReport.runtimeconfig.json %targetFolder%
copy %srcFolder%CommonCode.pdb %targetFolder%
copy %srcFolder%ItemsReport.pdb %targetFolder%

copy %srcFolder%PAT.txt %targetFolder%

copy %srcFolder%ItemReportScript.js %targetFolder%

pause