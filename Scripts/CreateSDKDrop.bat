@echo OFF
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

if not "%1"=="" (
    set SDK_DIR=%1
) else (
    set SDK_DIR=%CD%
)
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
copy /y "%ZELIG_ROOT%\Zelig\LLVM2IR_results\mbed\simple\temporary_helper.c" %SDK_DIR%\SDKDrop\output\temporary_helper.c

echo.
echo Copying Zelig compiler...
xcopy /i /e /q /y "%ZELIG_ROOT%\Zelig\ZeligBuild" %SDK_DIR%\SDKDrop\ZeligBuild

echo.
echo Copying mbed libraries...
xcopy /i /e /q /y "%ZELIG_ROOT%\external\targets\mbed" %SDK_DIR%\SDKDrop\mbed

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
    goto End
)
if not exist %SDK_DIR%\SDKDrop\LlilumApplicationType (
    @echo Error: Visual Studio Application Type missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\LlilumProjectType (
    @echo Error: Visual Studio Project Type missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\output\temporary_helper.c (
    @echo Error: temporary_helper missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\ZeligBuild (
    @echo Error: Zelig binaries missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\mbed (
    @echo Error: Mbed libs missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\Test (
    @echo Error: Test application missing
    goto End
)
if not exist %SDK_DIR%\SDKDrop\tools\readme.txt (
    @echo Error: Tools readme missing
    goto End
)

echo Complete.

:End
