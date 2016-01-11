@echo off
setlocal enableDelayedExpansion

@REM - Set this ENVIRONMENT Variable to override default optimization level for testing
IF NOT DEFINED LLILUM_OPT_LEVEL SET LLILUM_OPT_LEVEL=2
@REM with the Llilum compiler now running opt and LLC internally the default here
@REM is not to run either one unless explicitly enabled by the user. This essentially
@REM boils down to run the size app to show obj file size info and then running make
@REM to build the native code components and link the final image.

IF NOT DEFINED LLILUM_SKIP_OPT        SET LLILUM_SKIP_OPT=1
IF NOT DEFINED LLILUM_SKIP_LLC        SET LLILUM_SKIP_LLC=1
IF NOT DEFINED LLILUM_DEBUG           SET LLILUM_DEBUG=1
IF NOT DEFINED LLILUM_SKIP_CLEANCLEAN SET LLILUM_SKIP_CLEANCLEAN=0

IF /i "%LLVM_BIN%"=="" (
    ECHO LLVM_BIN is not defined. Please define LLVM_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)
IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

IF %1.==. (
    ECHO No target passed in. Defaulting to LPC1768
    SET TARGET=LPC1768
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET SIZE_OF_HEAP=0x6C00
    SET LWIP_USE=0
) ELSE (
    ECHO Detected target: %1
    SET TARGET=%1
    IF "%1"=="K64F" (
    SET SIZE_OF_HEAP=0x10000
    REM SET LLILUM_TARGET_CPU=cortex-m4 https://github.com/NETMF/llilum/issues/136
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=1
    ) ELSE IF "%1"=="LPC1768" (
    SET SIZE_OF_HEAP=0x6C00
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32F411RE" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32L152" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=0
    ) ELSE (    
    SET SIZE_OF_HEAP=0x6C00
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=0
    )
)

@REM - To skip optimizations for testing you can SET this ENVIRONMENT variable, otherwise the optimization will run as normal
IF "%LLILUM_SKIP_OPT%" == "0" (
    ECHO Running LLVM Optimization passes...

    @REM -"%LLVM_BIN%\opt" -O2 -adce -globaldce Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
    @REM -"%LLVM_BIN%\opt" -O1 -globalopt -constmerge -adce -globaldce -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc

    "%LLVM_BIN%\opt" -verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_verify.bc
    "%LLVM_BIN%\opt" -march=thumb -mcpu=%LLILUM_TARGET_CPU% -aa-eval -indvars -gvn -globaldce -adce -dce -tailcallopt -scalarrepl -mem2reg -ipconstprop -deadargelim -sccp -dce -ipsccp -dce -constmerge -scev-aa -targetlibinfo -irce -dse -dce -argpromotion -mem2reg -adce -mem2reg -globaldce -die -dce -dse -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
) ELSE (
    ECHO Skipping optimization passes...
    COPY Microsoft.Zelig.Test.mbed.Simple.bc Microsoft.Zelig.Test.mbed.Simple_opt.bc 1>NUL
)
@REM -"%LLVM_BIN%\llvm-bcanalyzer" Microsoft.Zelig.Test.mbed.Simple_opt.bc

IF "%LLILUM_SKIP_LLC%" == "0" (
    ECHO Generating LLVM IR source file...
    "%LLVM_BIN%\llvm-dis" Microsoft.Zelig.Test.mbed.Simple_opt.bc

    ECHO Compiling to ARM ^(optimization level %LLILUM_OPT_LEVEL%^)...
    "%LLVM_BIN%\llc" -O%LLILUM_OPT_LEVEL% -code-model=small -data-sections -relocation-model=pic -march=thumb -mcpu=%LLILUM_TARGET_CPU% -filetype=obj -mtriple=%LLILUM_TARGET_TRIPLE% -o=Microsoft.Zelig.Test.mbed.Simple_opt.o Microsoft.Zelig.Test.mbed.Simple_opt.bc
    if %ERRORLEVEL% LSS 0 (
        @ECHO ERRORLEVEL=%ERRORLEVEL%
        goto :EXIT
    )
) ELSE (
    ECHO skipping LLC compilation...
)

ECHO Size Report...
"%GCC_BIN%\arm-none-eabi-size.exe" Microsoft.Zelig.Test.mbed.Simple_opt.o

ECHO.
ECHO Compiling and linking with mbed libs...

IF "%LLILUM_SKIP_CLEANCLEAN%" == "0" (
	ECHO Cleaning target '%TARGET%' for  intermediate and final artifacts...
	make cleanclean TARGET=%TARGET% 
) ELSE (
	ECHO Cleaning target '%TARGET%' for final artifacts...
	make clean TARGET=%TARGET% 
)

make DEBUG=%LLILUM_DEBUG% TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% STACK_SIZE=%SIZE_OF_STACK% USE_LWIP=%LWIP_USE%

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 