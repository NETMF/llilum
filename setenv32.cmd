@echo off

set LLILUM_ROOT=%~dp0%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

::
:: Set LLVM environment variables.
::

set LLVM_BIN=%LLVM_SRCROOT_DIR%\build\Win32\Release\bin\
set LLVM_INCLUDE=%LLVM_SRCROOT_DIR%\include\
set LLVM_LIBS=%LLVM_SRCROOT_DIR%\

::
:: Add pyOCD and and make to path.
::

set PATH=%LLILUM_ROOT%Zelig\tools;%LLILUM_ROOT%Zelig\tools\openocd\bin-x64;%PATH%
set LLILUM_OPENOCD_SCRIPTS=%LLILUM_ROOT%Zelig\tools\openocd\scripts\

cd Zelig
echo Welcome to LLILUM! 
