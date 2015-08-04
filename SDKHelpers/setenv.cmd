@echo off

set ZELIG_ROOT=%CD%\
echo Current dir is %ZELIG_ROOT%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

::
:: Set LLVM environment variables.
::

set LLVM_BIN=%ZELIG_ROOT%LLVM\Debug\bin\
set GCC_BIN=%ZELIG_ROOT%ARM_GCC\4.9\bin\

::
:: Add pyOCD and and make to path.
::

set PATH=%ZELIG_ROOT%tools;%PATH%

echo Welcome to LLILUM! 
echo --- NOTE ---
echo See SDKDrop\tools\readme.txt for the PY_OCD download link
