@echo off

IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

ECHO.
ECHO Trying to start pyOCD for Windows

START /I pyocd_win.exe

PAUSE

ECHO.
ECHO Trying to attach to remote target on port 3333
"%GCC_BIN%arm-none-eabi-gdb.exe" %*

exit /b