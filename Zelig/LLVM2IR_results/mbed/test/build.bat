@echo off
setlocal enableDelayedExpansion

IF %1.==. (
    ECHO.
    ECHO Error no test name given.  e.g. Build.bat GpioTest LPC1768
    GOTO :EOF
)

IF NOT EXIST "Microsoft.Zelig.Test.%1.ll" (
    ECHO.
    ECHO Error invalid test name given: %1.  Microsoft.Zelig.Test.%1.ll was not found!
    GOTO :EOF    
)

@REM - set this ENVIRONMENT Variable to override default optimization level for testing
IF NOT DEFINED LLILUM_OPT_LEVEL SET LLILUM_OPT_LEVEL=2

IF /i "%LLVM_BIN%"=="" (
    ECHO LLVM_BIN is not defined. Please define LLVM_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)
IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

IF %2.==. (
    ECHO No target passed in. Defaulting to LPC1768
    set TARGET=LPC1768
    set SIZE_OF_HEAP=0x6B00
) ELSE (
    ECHO Detected target: %2
    set TARGET=%2
    IF "%2"=="K64F" (
    set SIZE_OF_HEAP=0x40000
    ) ELSE (
    set SIZE_OF_HEAP=0x6B00
    )
)

@REM - To skip optimizations for testing you can set this ENVIRONMENT variable, otherwise the optimization will run as normal
IF NOT DEFINED LLILUM_SKIP_OPT (
    ECHO Running LLVM Optimization Phases...

    @REM -"%LLVM_BIN%\opt" -O2 -adce -globaldce Microsoft.Zelig.Test.%1.bc -o Microsoft.Zelig.Test.%1_opt.bc
    @REM -"%LLVM_BIN%\opt" -O1 -globalopt -constmerge -adce -globaldce -time-passes Microsoft.Zelig.Test.%1.bc -o Microsoft.Zelig.Test.%1_opt.bc

    "%LLVM_BIN%\opt" -verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info Microsoft.Zelig.Test.%1.bc -o Microsoft.Zelig.Test.%1_verify.bc
    "%LLVM_BIN%\opt" -march=thumb -mcpu=cortex-m3 -aa-eval -indvars -gvn -globaldce -adce -dce -tailcallopt -scalarrepl -mem2reg -ipconstprop -deadargelim -sccp -dce -ipsccp -dce -constmerge -scev-aa -targetlibinfo -irce -dse -dce -argpromotion -mem2reg -adce -mem2reg -globaldce -die -dce -dse -time-passes Microsoft.Zelig.Test.%1.bc -o Microsoft.Zelig.Test.%1_opt.bc
) else (
    Echo Skipping optimization passes...
    copy Microsoft.Zelig.Test.%1.bc Microsoft.Zelig.Test.%1_opt.bc
)
@REM -"%LLVM_BIN%\llvm-bcanalyzer" Microsoft.Zelig.Test.%1_opt.bc

ECHO Cenerating LLVM IR source file...
"%LLVM_BIN%\llvm-dis" Microsoft.Zelig.Test.%1_opt.bc

ECHO Compiling to ARM...
"%LLVM_BIN%\llc" -O%LLILUM_OPT_LEVEL% -code-model=small -data-sections -relocation-model=pic -march=thumb -mcpu=cortex-m3 -filetype=obj -mtriple=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF -o=Microsoft.Zelig.Test.%1_opt.o Microsoft.Zelig.Test.%1_opt.bc
if %ERRORLEVEL% LSS 0 (
    @echo ERRORLEVEL=%ERRORLEVEL%
    goto :EXIT
)

ECHO Size Report...
"%GCC_BIN%\arm-none-eabi-size.exe" Microsoft.Zelig.Test.%1_opt.o

ECHO.
ECHO Linking with mbed libs...
make clean TARGET=%TARGET% PROJECT=%1
make DEBUG=1 TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% PROJECT=%1

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 