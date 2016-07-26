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

@REM lwIP stack has binaries for release, checked and debug. Checked is a release build 
@REM of lwIP (-Os -DNDEBUG) with LWIP_DEBUG defined and all debug switches for modules turned on. 
@REM Flag for release binaries is LLILUM_LWIP_DEBUG=0, debug binaries is 1, and checked is 2.
IF NOT DEFINED LLILUM_LWIP_DEBUG      SET LLILUM_LWIP_DEBUG=0

@REM SET incremental build as default, and correct as needed
SET LLILUM_CLEANCLEAN=0
SET LLILUM_DUMPS=0

IF "%2"=="/clean" (
    SET LLILUM_CLEANCLEAN=1
)
IF "%2"=="/dumps" (
    SET LLILUM_DUMPS=1
)
IF "%3"=="/dumps" (
    SET LLILUM_DUMPS=1
)

IF "%LLILUM_SKIP_CLEANCLEAN%" == "0" (
    SET LLILUM_CLEANCLEAN=1
)
IF "%LLILUM_CLEANCLEAN%" == "0" (
    ECHO Incremental build for c/cpp/asm sources...
    ECHO Use 'SET LLILUM_SKIP_CLEANCLEAN=' or 'SET LLILUM_SKIP_CLEANCLEAN=0' to perform a full build, 
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
    SET LLILUM_TARGET_CPU=cortex-m3
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

SET ARCH_AND_CPU=-march=%LLILUM_TARGET_ARCH% -mcpu=%LLILUM_TARGET_CPU% 

SET OPT_STEPS__VERIFY=-verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info
SET OPT_STEPS__OPTIMIZE=-march=%LLILUM_TARGET_ARCH% -mcpu=%LLILUM_TARGET_CPU% -mem2reg -instcombine -scev-aa -indvars -gvn -globaldce -adce -dce -tailcallopt -instcombine -memcpyopt -scalarrepl -ipconstprop -deadargelim -sccp -dce -die -ipsccp -dce -die -instcombine -constmerge -scev-aa -targetlibinfo -irce -dse -dce -instcombine -argpromotion -adce -instcombine -memcpyopt -globaldce -die -dce -dse -loop-deletion -indvars -loop-simplify -licm -indvars -loop-reduce -mem2reg -time-passes

SET OPT_STEPS__BUILDS_DATA_STRUCTURES=-domfrontier -domtree -intervals -globals-aa -memdep -dce -die -domfrontier -domtree -intervals -globals-aa -memdep
SET OPT_STEPS__SIMPLIFY_GLOBALS=-strip-dead-prototypes -globalopt -globaldce -gvn -prune-eh -reassociate -constmerge -ipconstprop -reassociate -constmerge -constprop -constmerge -sccp -constmerge -deadargelim -ipsccp -irce %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__DEAD_CODE_OBVIOUS=-dse -die %OPT_STEPS__BUILDS_DATA_STRUCTURES%
REM SET OPT_STEPS__INLINE=-always-inline -inline %OPT_STEPS__BUILDS_DATA_STRUCTURES%
REM SET OPT_STEPS__INLINE=-always-inline %OPT_STEPS__BUILDS_DATA_STRUCTURES%
REM SET OPT_STEPS__MERGE_FUNCTIONS=-mergefunc -mergereturn %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__SIMPLIFY_FUNCTIONS=-simplifycfg %OPT_STEPS__BUILDS_DATA_STRUCTURES%
REM SET OPT_STEPS__SIMPLIFY_SINK=-sink %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__SIMPLIFY_TAIL_CALL=-tailcallopt %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__SIMPLIFY_INSTRUCTIONS=-instcombine -sroa -memcpyopt -irce -scalarrepl -scalarrepl-ssa %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__DEAD_CODE_AGGRESSIVE=-adce -bdce %OPT_STEPS__BUILDS_DATA_STRUCTURES%
REM SET OPT_STEPS__LOOPS=-loop-deletion -indvars -loop-simplify -licm -indvars -loop-reduce %OPT_STEPS__BUILDS_DATA_STRUCTURES%
SET OPT_STEPS__MEM2REG=-mem2reg %OPT_STEPS__BUILDS_DATA_STRUCTURES%

@REM - To skip optimizations for testing you can SET this ENVIRONMENT variable, otherwise the optimization will run as normal
IF "%LLILUM_SKIP_OPT%" == "0" (
    ECHO Running LLVM Optimization passes...
    
    "%LLVM_BIN%\llvm-dis" %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc

    @REM -"%LLVM_BIN%\opt" -O2 -adce -globaldce %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc
    @REM -"%LLVM_BIN%\opt" -O1 -globalopt -constmerge -adce -globaldce -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc

    ECHO Verify...
    "%LLVM_BIN%\opt" %OPT_STEPS__VERIFY% %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_verify.bc
    
    ECHO Optimize...
    ECHO Generating LLVM IR source file...
    
    "%LLVM_BIN%\opt" %OPT_STEPS__OPTIMIZE% %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc
    REM "%LLVM_BIN%\opt" %ARCH_AND_CPU% %OPT_STEPS__DEAD_CODE_OBVIOUS% %OPT_STEPS__SIMPLIFY_GLOBALS% %OPT_STEPS__INLINE% %OPT_STEPS__MERGE_FUNCTIONS% %OPT_STEPS__INLINE% %OPT_STEPS__SIMPLIFY_FUNCTIONS% %OPT_STEPS__SIMPLIFY_SINK% %OPT_STEPS__SIMPLIFY_TAIL_CALL% %OPT_STEPS__SIMPLIFY_INSTRUCTIONS% %OPT_STEPS__LOOPS% %OPT_STEPS__MEM2REG% -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc -o       %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_0.bc
    REM "%LLVM_BIN%\opt" %ARCH_AND_CPU% %OPT_STEPS__DEAD_CODE_OBVIOUS% %OPT_STEPS__SIMPLIFY_GLOBALS% %OPT_STEPS__INLINE% %OPT_STEPS__MERGE_FUNCTIONS% %OPT_STEPS__INLINE% %OPT_STEPS__SIMPLIFY_FUNCTIONS% %OPT_STEPS__SIMPLIFY_SINK% %OPT_STEPS__SIMPLIFY_TAIL_CALL% %OPT_STEPS__SIMPLIFY_INSTRUCTIONS% %OPT_STEPS__LOOPS% %OPT_STEPS__MEM2REG% -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_0.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_1.bc
    REM "%LLVM_BIN%\opt" %ARCH_AND_CPU% %OPT_STEPS__DEAD_CODE_OBVIOUS% %OPT_STEPS__SIMPLIFY_GLOBALS% %OPT_STEPS__INLINE% %OPT_STEPS__MERGE_FUNCTIONS% %OPT_STEPS__INLINE% %OPT_STEPS__SIMPLIFY_FUNCTIONS% %OPT_STEPS__SIMPLIFY_SINK% %OPT_STEPS__SIMPLIFY_TAIL_CALL% %OPT_STEPS__SIMPLIFY_INSTRUCTIONS% %OPT_STEPS__LOOPS% %OPT_STEPS__MEM2REG% -time-passes %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_1.bc -o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc
    
    REM "%LLVM_BIN%\llvm-dis" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_0.bc
    REM "%LLVM_BIN%\llvm-dis" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_1.bc
    "%LLVM_BIN%\llvm-dis" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc

    @echo off
    
) ELSE (
    ECHO Skipping optimization passes...
    COPY %TARGET%\Microsoft.Zelig.Test.mbed.Simple.bc %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc 1>NUL
)
@REM -"%LLVM_BIN%\llvm-bcanalyzer" %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.bc

IF "%LLILUM_SKIP_LLC%" == "0" (
    ECHO Generating LLVM IR source file...
    
    ECHO Compiling to %LLILUM_TARGET_ARCH% ^(optimization level %LLILUM_OPT_LEVEL%^)...
    "%LLVM_BIN%\llc" -O%LLILUM_OPT_LEVEL% %LLILUM_TARGET_EXTRA% -code-model=small -data-sections -relocation-model=pic -march=%LLILUM_TARGET_ARCH% -mcpu=%LLILUM_TARGET_CPU% -mtriple=%LLILUM_TARGET_TRIPLE% -filetype=obj -o=%TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt.o %TARGET%\Microsoft.Zelig.Test.mbed.Simple_opt_2.bc
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

make all TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% STACK_SIZE=%SIZE_OF_STACK% USE_LWIP=%LWIP_USE% DEBUG=%LLILUM_DEBUG% LWIP_DEBUG=%LLILUM_LWIP_DEBUG% 

ECHO.
ECHO Target '%TARGET%' build is complete.
ECHO.
    
IF "%LLILUM_DUMPS%" == "1" (
    ECHO Creating .lst and .disas files... 
    make dumps TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP% STACK_SIZE=%SIZE_OF_STACK% USE_LWIP=%LWIP_USE%
    ECHO done! 
)

GOTO :EXIT

:EXIT
ECHO.