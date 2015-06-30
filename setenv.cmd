@echo off

set ZELIG_ROOT=%~dp0%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

::
:: Set LLVM environment variables.
::

set LLVM_BIN=%ZELIG_ROOT%external\LLVM\Debug\bin\
set LLVM_INCLUDE=%ZELIG_ROOT%external\LLVM\include\
set LLVM_LIBS=%ZELIG_ROOT%external\LLVM\

::
:: Add pyOCD and and make to path.
::

set PATH=%ZELIG_ROOT%external\tools;%PATH%

cd Zelig
echo Welcome to LLILUM! 
