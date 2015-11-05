@echo OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
set LLILUM_ROOT_DIR=%~dp0%..
set PATH=%LLILUM_ROOT_DIR%\Zelig\tools;%PATH%
call "%VS140COMNTOOLS%"vsvars32.bat

where /q nuget.exe || (
    echo Error: nuget.exe not found. Please add nuget.exe to PATH.
    goto Error
)

echo.
echo Building Board Configurations...
msbuild /m "%LLILUM_ROOT_DIR%\Zelig\BoardConfigurations\BoardConfigurations.sln" /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /flp:verbosity=diagnostic;LogFile=BoardConfigurationBuild.log /clp:verbosity=minimal
if NOT %errorlevel%==0 (
    echo.
    echo Error: Failed to build Board configurations.
    goto Error
)

:: Building the compiler will also build the native code sample target, which needs the SDK, but this is generating the SDK...
:: To resolve the "Chicken or Egg" circular dependency BuildEnv.props will set build properties to effectively make the SDK
:: directory the build output. [Technically it is one directory higher so it gets the additional target native headers etc..
:: that are copied to the drop location below]. The reason for copying things at all is to allow for a simpler zip build 
:: from a single folder that contains only the files intended for redistribution as the SDK. Once we have an MSI installer
:: then it's build project can pull files directly without needing any additional staging. 
echo.
echo Building Llilum compiler...
msbuild /m "%LLILUM_ROOT_DIR%\Zelig\Zelig\Zelig.sln" /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /flp:verbosity=diagnostic;LogFile=ZeligCompilerBuild.log /clp:verbosity=minimal
if NOT %errorlevel%==0 (
    echo.
    echo Error: Failed to build Llilum compiler.
    goto CheckSDKIssue
)

echo.
echo Building Llilum VSIX package...
nuget.exe restore  "%LLILUM_ROOT_DIR%\VisualStudio\LlilumProjectType"
msbuild /m "%LLILUM_ROOT_DIR%\VisualStudio\LlilumProjectType\LlilumApplication.sln" /t:Build /p:Configuration=Debug;Platform="Any CPU" /flp:verbosity=diagnostic;LogFile=ZeligVSIXBuild.log /clp:verbosity=minimal
if NOT %errorlevel%==0 (
    echo Error: Failed to build Llilum VSIX package.
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
xcopy /i /e /q /y %LLILUM_ROOT_DIR%\SDKHelpers %SDK_DIR%\SDKDrop

echo.
echo Copying Visual Studio files...
xcopy /s /i /E /q /y "%LLILUM_ROOT_DIR%\VisualStudio\LlilumApplicationType" "%SDK_DIR%\SDKDrop\LlilumApplicationType"

echo.
echo Copying files for GCC...
::
:: !!! Relies on naming convention !!!
::
copy /y "%LLILUM_ROOT_DIR%\Zelig\Zelig\Test\mbed\SimpleSDK\Native\helpers.h" "%SDK_DIR%\SDKDrop\output\helpers.h"
copy /y "%LLILUM_ROOT_DIR%\Zelig\Zelig\Test\mbed\SimpleSDK\Native\InteropExports_*.cpp" "%SDK_DIR%\SDKDrop\output\."

echo.
echo Copying target libraries...
xcopy /i /e /q /y "%LLILUM_ROOT_DIR%\Zelig\mbed\*" "%SDK_DIR%\SDKDrop\mbed\*"
xcopy /i /e /q /y "%LLILUM_ROOT_DIR%\Zelig\mbed-rtos\*" "%SDK_DIR%\SDKDrop\mbed-rtos\*"
xcopy /q /y "%LLILUM_ROOT_DIR%\Zelig\mbed-rtos.lib" "%SDK_DIR%\SDKDrop\*"

echo.
echo Copying installation instructions for pyOCD and make for Windows...
xcopy /i /e /q /y "%LLILUM_ROOT_DIR%\Zelig\tools\*" "%SDK_DIR%\SDKDrop\tools\*"

echo.
echo Copying Llilum compiler...
xcopy /i /e /q /y "%LLILUM_ROOT_DIR%\Zelig\ZeligBuild" "%SDK_DIR%\SDKDrop\ZeligBuild"

echo.
echo Copying board configurations
xcopy /i /e /q /y "%LLILUM_ROOT_DIR%\Zelig\Zelig\RunTime\DeviceModels\Boards" "%SDK_DIR%\SDKDrop\Boards"

echo.
echo Copying test project...
xcopy /i /e /q /y %LLILUM_ROOT_DIR%\Zelig\Zelig\Test "%SDK_DIR%\SDKDrop\Test"

echo.
echo Copying VSIX package
copy "%LLILUM_ROOT_DIR%\VisualStudio\LlilumProjectType\LlilumApplication\LlilumApplication.ProjectType\bin\Debug\LlilumApplication.vsix" "%SDK_DIR%\SDKDrop\*"

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
if not exist %SDK_DIR%\SDKDrop\output\helpers.h (
    @echo Error: temporary helpers header missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\output\InteropExports_*.cpp (
    @echo Error: temporary helpers sources missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\ZeligBuild (
    @echo Error: Llilum binaries missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\mbed (
    @echo Error: Mbed libs missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\mbed-rtos (
    @echo Error: Mbed Source and headers missing
    goto Error
)
if not exist %SDK_DIR%\SDKDrop\mbed-rtos.lib (
    @echo Error: Mbed RTOS lib missing
    goto Error
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

:CheckSDKIssue
if not "%LLILUM_SDK%"=="" (
    if not exist %LLILUM_SDK% (
        echo NOTE: You have previously installed the SDK, and may have moved or removed it.
        echo Please remove the "LLILUM_SDK" environment variable, restart Windows Explorer, open a new CMD prompt, and run this script again
    )
)

:Error
exit /B 1
