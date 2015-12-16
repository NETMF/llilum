@echo off
setlocal enableDelayedExpansion

@REM - set this ENVIRONMENT Variable to override default optimization level for testing
IF NOT DEFINED LLILUM_OPT_LEVEL SET LLILUM_OPT_LEVEL=2
IF NOT DEFINED LLILUM_SKIP_OPT  SET LLILUM_SKIP_OPT=0
IF NOT DEFINED LLILUM_DEBUG     SET LLILUM_DEBUG=1

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
    set TARGET=LPC1768
    set SIZE_OF_HEAP=0x6000
) ELSE (
    ECHO Detected target: %1
    set TARGET=%1
    IF "%1"=="K64F" (
    set SIZE_OF_HEAP=0x10000
    ) ELSE (
    set SIZE_OF_HEAP=0x6000
    )
)

@REM - To skip optimizations for testing you can set this ENVIRONMENT variable, otherwise the optimization will run as normal
IF "%LLILUM_SKIP_OPT%" == "0" (
    ECHO Running LLVM Optimization Phases...

    @REM -"%LLVM_BIN%\opt" -O2 -adce -globaldce Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
    @REM -"%LLVM_BIN%\opt" -O1 -globalopt -constmerge -adce -globaldce -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc

    "%LLVM_BIN%\opt" -verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_verify.bc
    "%LLVM_BIN%\opt" -march=thumb -mcpu=cortex-m3 -aa-eval -indvars -gvn -globaldce -adce -dce -tailcallopt -scalarrepl -mem2reg -ipconstprop -deadargelim -sccp -dce -ipsccp -dce -constmerge -scev-aa -targetlibinfo -irce -dse -dce -argpromotion -mem2reg -adce -mem2reg -globaldce -die -dce -dse -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
) else (
    Echo Skipping optimization passes...
    copy Microsoft.Zelig.Test.mbed.Simple.bc Microsoft.Zelig.Test.mbed.Simple_opt.bc
)
@REM -"%LLVM_BIN%\llvm-bcanalyzer" Microsoft.Zelig.Test.mbed.Simple_opt.bc

ECHO Cenerating LLVM IR source file...
"%LLVM_BIN%\llvm-dis" Microsoft.Zelig.Test.mbed.Simple_opt.bc

ECHO Compiling to ARM (optimization level %LLILUM_OPT_LEVEL%)...
"%LLVM_BIN%\llc" -O%LLILUM_OPT_LEVEL% -code-model=small -data-sections -relocation-model=pic -march=thumb -mcpu=cortex-m3 -filetype=obj -mtriple=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF -o=Microsoft.Zelig.Test.mbed.Simple_opt.o Microsoft.Zelig.Test.mbed.Simple_opt.bc
if %ERRORLEVEL% LSS 0 (
    @echo ERRORLEVEL=%ERRORLEVEL%
    goto :EXIT
)

ECHO Size Report...
"%GCC_BIN%\arm-none-eabi-size.exe" Microsoft.Zelig.Test.mbed.Simple_opt.o

ECHO.
ECHO Linking with mbed libs...
make clean TARGET=%TARGET% 
make DEBUG=%LLILUM_DEBUG% TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% STACK_SIZE=%SIZE_OF_STACK%

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 