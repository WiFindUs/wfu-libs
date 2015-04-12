SET INPUT_DIR=%1
SET OUTPUT_DIR=%2

IF %INPUT_DIR:~-1%==\ SET INPUT_DIR=%INPUT_DIR:~0,-1%
IF %OUTPUT_DIR:~-1%==\ SET OUTPUT_DIR=%OUTPUT_DIR:~0,-1%

ECHO input: %INPUT_DIR%
ECHO output: %OUTPUT_DIR%

xcopy /C /R /Y /D %INPUT_DIR%\*.dll %OUTPUT_DIR%
xcopy /C /R /Y /D %INPUT_DIR%\*.exe %OUTPUT_DIR%
xcopy /C /R /Y /D %INPUT_DIR%\*.conf %OUTPUT_DIR%
xcopy /C /R /Y /D /S /I %INPUT_DIR%\images %OUTPUT_DIR%\images
xcopy /C /R /Y /D /S /I %INPUT_DIR%\maps %OUTPUT_DIR%\maps