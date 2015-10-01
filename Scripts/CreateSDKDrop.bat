@echo OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

set ZELIG_ROOT=%~dp0%..
set PATH=%ZELIG_ROOT%\external\tools;%PATH%
call "%VS140COMNTOOLS%"vsvars32.bat

where /q nuget.exe || (
    echo Error: nuget.exe not found. Please add nuget.exe to PATH.
    goto Error
)

echo.
echo Building Zelig compiler...
msbuild /m "%ZELIG_ROOT%\Zelig\Zelig\Zelig.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="Mixed Platforms" /flp:verbosity=diagnostic;LogFile=ZeligCompilerBuild.log /clp:verbosity=minimal
if NOT %errorlevel%==0 (
    echo Error: Failed to build Zelig compiler.
    goto Error
)

echo.
echo Building Zelig VSIX package...
nuget.exe restore  "%ZELIG_ROOT%\VisualStudio\LlilumProjectType"
msbuild /m "%ZELIG_ROOT%\VisualStudio\LlilumProjectType\LlilumApplication.sln" /t:Rebuild /p:Configuration=Debug;Platform="Any CPU" /flp:verbosity=diagnostic;LogFile=ZeligVSIXBuild.log /clp:verbosity=minimal
if NOT %errorlevel%==0 (
    echo Error: Failed to build Zelig VSIX package.
    goto Error
)

if not "%1"=="" (
    set SDK_DIR=%1
) else (
    set SDK_DIR=%CD%
)

echo.
echo Set drop directory to %SDK_DIR%

:: Drop Creation
echo.
echo Creating Drop directory...
xcopy /i /e /q /y %ZELIG_ROOT%\SDKHelpers %SDK_DIR%\SDKDrop

echo.
echo Copying Visual Studio files...
xcopy /s /i /E /q /y "%ZELIG_ROOT%\VisualStudio\LlilumApplicationType" %SDK_DIR%\SDKDrop\LlilumApplicationType
xcopy /s /i /E /q /y "%ZELIG_ROOT%\VisualStudio\LlilumProjectType" %SDK_DIR%\SDKDrop\LlilumProjectType

echo.
echo Copying files for GCC...
::
:: !!! Relies on naming convention !!!
::
copy /y "%ZELIG_ROOT%\Zelig\Zelig\Test\mbed\SimpleSDK\Native\helpers.h" %SDK_DIR%\SDKDrop\output\helpers.h
copy /y "%ZELIG_ROOT%\Zelig\Zelig\Test\mbed\SimpleSDK\Native\InteropExports_*.cpp" %SDK_DIR%\SDKDrop\output\.

echo.
echo Copying Zelig compiler...
xcopy /i /e /q /y "%ZELIG_ROOT%\Zelig\ZeligBuild" %SDK_DIR%\SDKDrop\ZeligBuild

echo.
echo Copying board configurations
xcopy /i /e /q /y "%ZELIG_ROOT%\Zelig\Zelig\RunTime\DeviceModels\Boards" %SDK_DIR%\SDKDrop\Boards

echo.
echo Copying target libraries...
xcopy /i /e /q /y "%ZELIG_ROOT%\external\targets\*" %SDK_DIR%\SDKDrop\*

echo.
echo Copying test project...
xcopy /i /e /q /y %ZELIG_ROOT%\Zelig\Zelig\Test %SDK_DIR%\SDKDrop\Test

echo.
echo Copying installation instructions for pyOCD and make for Windows...
if not exist %SDK_DIR%\SDKDrop\tools (
    mkdir %SDK_DIR%\SDKDrop\tools
)
xcopy /q /y "%ZELIG_ROOT%\external\tools" %SDK_DIR%\SDKDrop\tools

:: Drop Verification
echo.
echo Verifying drop...

if not exist %SDK_DIR%\SDKDrop (
    @echo Error: Base directory missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\LlilumApplicationType (
    @echo Error: Visual Studio Application Type missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\LlilumProjectType (
    @echo Error: Visual Studio Project Type missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\output\helpers.h (
    @echo Error: temporary helpers header missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\output\InteropExports_*.cpp (
    @echo Error: temporary helpers sources missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\ZeligBuild (
    @echo Error: Zelig binaries missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\mbed (
    @echo Error: Mbed libs missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\mbed-rtos (
    @echo Error: Mbed Source and headers missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\mbed-rtos.lib (
    @echo Error: Mbed RTOSS lib missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\Test (
    @echo Error: Test application missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\tools\readme.txt (
    @echo Error: Tools readme missing
    goto Error
)

echo Complete.
echo.
goto :EOF


:Error
exit /B 1
