@echo off

IF /i "%LLVM_BIN%"=="" (
    ECHO LLVM_BIN is not defined. Please define LLVM_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)
IF /i "%GCC_BIN%"=="" (
    ECHO GCC_BIN is not defined. Please define GCC_BIN to point to LLVM tools and binaries. 
    GOTO :EXIT
)

ECHO Running LLVM Optimization Phases...

::%LLVM_BIN%\opt -O2 -adce -globaldce Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
::%LLVM_BIN%\opt -O1 -globalopt -constmerge -adce -globaldce -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc
%LLVM_BIN%\opt -scalarrepl -targetlibinfo -verify -mem2reg -constmerge -adce -globaldce -time-passes Microsoft.Zelig.Test.mbed.Simple.bc -o Microsoft.Zelig.Test.mbed.Simple_opt.bc

::copy Microsoft.Zelig.Test.mbed.Simple.bc Microsoft.Zelig.Test.mbed.Simple_opt.bc
::%LLVM_BIN%\llvm-bcanalyzer Microsoft.Zelig.Test.mbed.Simple_opt.bc

%LLVM_BIN%\llvm-dis Microsoft.Zelig.Test.mbed.Simple_opt.bc

ECHO Compiling to ARM...
%LLVM_BIN%\llc -code-model=small -data-sections -relocation-model=pic -march=thumb -mcpu=cortex-m3 -filetype=obj -mtriple=Thumb-NoSubArch-UnknownVendor-UnknownOS-GNUEABI-ELF -o=Microsoft.Zelig.Test.mbed.Simple_opt.o Microsoft.Zelig.Test.mbed.Simple_opt.bc

ECHO Size Report...
%GCC_BIN%\arm-none-eabi-size.exe Microsoft.Zelig.Test.mbed.Simple_opt.o

ECHO.
ECHO Linking with mbed libs...
make clean
make

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 

