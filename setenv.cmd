@echo off



set ZELIG_ROOT=%~dp0%

prompt $_$+---$G $P$_$+   $G$_Cmd$G

REM 
REM Set LLVM environment variables
REM 

set LLVM_BIN=%ZELIG_ROOT%external\LLVM\Debug\bin\

set LLVM_INCLUDE=%ZELIG_ROOT%external\LLVM\include\


set LLVM_LIBS=%ZELIG_ROOT%external\LLVM\



REM 
REM Add pyOCD and and make to path
REM 
set PATH=%ZELIG_ROOT%external\tools;%PATH%



cd zelig

echo Welcome to Zelig! 
