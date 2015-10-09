@echo off

set ZELIG_ROOT=%~dp0%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

::
:: Set LLVM environment variables.
::

set LLVM_BIN=%LLVM_SRCROOT_DIR%\build\x64\Release\bin\
set LLVM_INCLUDE=%LLVM_SRCROOT_DIR%\include\
set LLVM_LIBS=%LLVM_SRCROOT_DIR%\

::
:: Add pyOCD and and make to path.
::

set PATH=%ZELIG_ROOT%Zelig\tools;%PATH%

cd Zelig
echo Welcome to LLILUM! 
