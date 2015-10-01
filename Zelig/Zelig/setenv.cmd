@echo off

echo.
echo Welcome to Zelig!
echo.

if "%DEPOTROOT%" equ "" (

   pushd %~dp0\..
   FOR /F "usebackq delims==" %%i IN (`cd`) DO @set DEPOTROOT=%%i
   popd

)

path %ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin;%PATH%
path %DEPOTROOT%\Zelig\Scripts;%PATH%
path %DEPOTROOT%\..\bin;%DEPOTROOT%\..\..\bin;%PATH%

call "%VS100COMNTOOLS%\vsvars32.bat"

rem prompt $_$+Dir: $P$_$_Cmd$G
prompt $_$+---$G $P$_$+   $G$_Cmd$G 