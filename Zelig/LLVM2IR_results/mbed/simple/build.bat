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
IF NOT DEFINED LLILUM_SKIP_CLEANCLEAN SET LLILUM_SKIP_CLEANCLEAN=1

@REM set incremental build as default, and correct as needed
SET LLILUM_CLEANCLEAN=0

IF "%2"=="/bc" (
	SET LLILUM_CLEANCLEAN=1
)
IF "%LLILUM_SKIP_CLEANCLEAN%" == "0" (
	SET LLILUM_CLEANCLEAN=1
)

IF "%LLILUM_CLEANCLEAN%" == "0" (
    ECHO Incremental build for c/cpp/asm sources...
    ECHO Use 'set LLILUM_SKIP_CLEANCLEAN=' or 'set LLILUM_SKIP_CLEANCLEAN=0' to perform a full build, 
    ECHO or call the build batch file with '/bc' as last argument
)
    
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
    SET LLILUM_TARGET_ARCH=thumb
    SET SIZE_OF_HEAP=0x6000
    SET LWIP_USE=0
) ELSE (
    ECHO Detected target: %1
    SET TARGET=%1
    IF "%1"=="K64F" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m4
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=1
    ) ELSE IF "%1"=="LPC1768" (
    SET SIZE_OF_HEAP=0x6000
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32F091" (
    SET SIZE_OF_HEAP=0x6000
    SET LLILUM_TARGET_CPU=cortex-m0
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32F411" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m4
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32F401" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m4
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=0
    ) ELSE IF "%1"=="STM32L152" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=0
    ) ELSE IF "%1"=="WIN32" (
    SET SIZE_OF_HEAP=0x10000
    SET LLILUM_TARGET_CPU=x86-64
    SET LLILUM_TARGET_TRIPLE=x86_64-pc-windows-msvc
    SET LLILUM_TARGET_ARCH=x86
    SET LLILUM_TARGET_EXTRA=-function-sections -dwarf-version=3 
    SET LLILUM_SKIP_LLC=0
    SET LWIP_USE=0
    ) ELSE (    
    SET SIZE_OF_HEAP=0x6000
    SET LLILUM_TARGET_CPU=cortex-m3
    SET LLILUM_TARGET_TRIPLE=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF
    SET LLILUM_TARGET_ARCH=thumb
    SET LWIP_USE=0
    )
)

@REM - To skip optimizations for testing you can SET this ENVIRONMENT variable, otherwise the optimization will run as normal
IF "%LLILUM_SKIP_OPT%" == "0" (
    ECHO Running LLVM Optimization passes...

    @REM -"%LLVM_BIN%\opt" -O2 -adce -globaldce %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc
    @REM -"%LLVM_BIN%\opt" -O1 -globalopt -constmerge -adce -globaldce -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc

    "%LLVM_BIN%\opt" -verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_verify.bc
    "%LLVM_BIN%\opt" -march=%LLILUM_TARGET_ARCH% -mcpu=%LLILUM_TARGET_CPU% -aa-eval -indvars -gvn -globaldce -adce -dce -tailcallopt -scalarrepl -mem2reg -ipconstprop -deadargelim -sccp -dce -ipsccp -dce -constmerge -scev-aa -targetlibinfo -irce -dse -dce -argpromotion -mem2reg -adce -mem2reg -globaldce -die -dce -dse -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc
) ELSE (
    ECHO Skipping optimization passes...
    COPY %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc 1>NUL
)
@REM -"%LLVM_BIN%\llvm-bcanalyzer" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc

IF "%LLILUM_SKIP_LLC%" == "0" (
    ECHO Generating LLVM IR source file...
    "%LLVM_BIN%\llvm-dis" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc

    ECHO Compiling to %LLILUM_TARGET_ARCH% ^(optimization level %LLILUM_OPT_LEVEL%^)...
    "%LLVM_BIN%\llc" -O%LLILUM_OPT_LEVEL% %LLILUM_TARGET_EXTRA% -code-model=small -data-sections -relocation-model=pic -march=%LLILUM_TARGET_ARCH% -mcpu=%LLILUM_TARGET_CPU% -mtriple=%LLILUM_TARGET_TRIPLE% -filetype=obj -o=%TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc
    if %ERRORLEVEL% LSS 0 (
        @ECHO ERRORLEVEL=%ERRORLEVEL%
        goto :EXIT
    )
) ELSE (
    ECHO skipping LLC compilation...
)

IF "%1"=="WIN32" (
    GOTO :EXIT
)

ECHO Size Report...
"%GCC_BIN%\arm-none-eabi-size.exe" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.o

ECHO.
ECHO Compiling and linking with mbed libs...

IF "%LLILUM_CLEANCLEAN%" == "1" (
	ECHO Cleaning target '%TARGET%' for  intermediate and final artifacts...
	make cleanclean TARGET=%TARGET% 
) ELSE (
	ECHO Cleaning target '%TARGET%' for final artifacts only...
	make clean TARGET=%TARGET% 
)

make DEBUG=%LLILUM_DEBUG% TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% STACK_SIZE=%SIZE_OF_STACK% USE_LWIP=%LWIP_USE%

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 