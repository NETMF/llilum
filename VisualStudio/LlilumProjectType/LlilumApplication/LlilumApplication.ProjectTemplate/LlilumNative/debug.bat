@echo off

IF /i "%LLILUM_SDK%"=="" (
    ECHO LLILUM_SDK is not defined. Please run setenv.cmd from the SDK folder. 
    GOTO :EXIT
)

ECHO.
ECHO Trying to start pyOCD for Windows

START /I %LLILUM_SDK%tools\pyocd_win.exe

ECHO.
ECHO Trying to attach to remote target on port 3333
"%LLILUM_GCC%\bin\arm-none-eabi-gdb.exe" %*
