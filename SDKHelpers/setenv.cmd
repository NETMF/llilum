@echo off

set LLILUM_ROOT=%CD%\
echo Current dir is %LLILUM_ROOT%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

::
:: Set LLVM environment variables.
::

set LLVM_BIN=%LLILUM_ROOT%LLVM\Debug\bin\
set GCC_BIN=%LLILUM_ROOT%ARM_GCC\4.9\bin\

::
:: Add pyOCD and and make to path.
::

set PATH=%LLILUM_ROOT%tools;%PATH%

echo Welcome to LLILUM! 
echo --- NOTE ---
echo See SDKDrop\tools\readme.txt for the PY_OCD download link
