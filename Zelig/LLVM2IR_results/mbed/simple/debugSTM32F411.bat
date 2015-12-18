@echo off

IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

IF /i "%LLILUM_OPENOCD_SCRIPTS%"=="" (
    ECHO Path to OpenOCD scripts is not defined. Please run setenv or set LLILUM_OPENOCD_SCRIPTS to point to the correct directory. 
    GOTO :EXIT
)

ECHO.
ECHO Trying to start OpenOCD for Windows
kill openocd.exe
START /I openocd.exe -f %LLILUM_OPENOCD_SCRIPTS%interface\stlink-v2-1.cfg -f %LLILUM_OPENOCD_SCRIPTS%board\st_nucleo_f4.cfg

PAUSE

ECHO.
ECHO Trying to attach to remote target on port 3333
"%GCC_BIN%arm-none-eabi-gdb.exe" STM32F411\mbed_simple.elf %* -ex "target remote :3333" -ex "monitor halt reset" -ex "ni"

exit /b
