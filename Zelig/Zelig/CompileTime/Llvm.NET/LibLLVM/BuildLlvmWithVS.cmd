@echo off
@REM - This bat file should be placed into the root of the LLVM source tree
@REM - It can be run from there, with no parameters to generate all the
@REM - Libs for Win32 and x64 with the Debug, RelWithDebInfo, and Release
@REM - configurations.

setlocal
set LLVM_ROOT=%~d0%~p0

@REM By default do the following:
@REM 1 - Generate the build project and solutions from CMake
@REM 2 - Build all the Platform\configuration combos (2x3)
@REM 3 - Register the output location in registry
set GENERATE=1
set BUILD=1
set REGISTER=1
set LlvmVersion=3.7.0

@REM - Allow overriding default version and disabling any of the stages via parameters
:arg_loop
if /i "%1"=="-g-" set GENERATE=0
if /i "%1"=="-b-" set BUILD=0
if /i "%1"=="-r-" set REGISTER=0
if /i "%1"=="-v" (
    set LlvmVersion=%2
    shift
)
if /i "%1"=="-?" goto :ShowUsage
if /i "%1"=="/?" goto :ShowUsage
if /i "%1"=="/h" goto :ShowUsage

@rem - move to next arg and loop back around if it isn't empty
shift 
if NOT "%1"=="" goto :arg_loop

@REM - End of args parsing
if NOT EXIST %LLVM_ROOT%CMakeLists.txt (
    @echo CMakeLists.txt is missing, the current directory does not appear to be an LLVM source directory
    goto :exit
)

if %GENERATE%==1 (
    call :CongigureLLVMBuild "Visual Studio 14 2015 Win64" x64
    if %ERRORLEVEL% GTR 0 goto :exit
    
    call :CongigureLLVMBuild "Visual Studio 14 2015" Win32
    if %ERRORLEVEL% GTR 0 goto :exit
)
if %BUILD%==1 (
    call :BuildLLVMPlatform x64
    if %ERRORLEVEL% GTR 0 goto :exit
    call :BuildLLVMPlatform Win32
    if %ERRORLEVEL% GTR 0 goto :exit
)

if %REGISTER%==1 (
    @echo registering LLVM path in registry
    reg add HKCU\Software\LLVM\%LlvmVersion% /v SrcRoot /d %LLVM_ROOT% /f
)
goto :exit

:ShowUsage
@echo.
@echo Description:
@echo     1) Generates the VS projects from LLVM source via CMake
@echo     2) Builds the sources for win32 and x64 platforms with the Debug,Release, and RelWithDebInfo configurations
@echo     3) Adds a registry entry for the location of the src and build output HKCU\SOFTWARE\LLVM\^<LlvmVersionNumber^>\SrcRoot
@echo.
@echo The registry entry is used by the LlvmApplication.props Propertysheet for VC projects to locate the various
@echo LLVM headers and libs. Alternatively, if LLVM_SRCROOT_DIR is provided either as an environment variable
@echo or as an MsBuild property for the application's build then LlvmApplication.props will use it and the registry
@echo entry is not required. This is to support automated build servers where this script may be run on a machine
@echo sharing out the results and the application build server is run from a different account and machine where
@echo HKCU settings are different.
@echo.
@echo Usage:
@echo     BuildLlvmWithVS [-g-][-b-][-r-][-v ^<LlvmVersionNumber^>]
@echo.
@echo Where:
@echo     -g- disables the cmake project generation phase
@echo     -b- disables the code build phase 
@echo     -r- disables the registry update
@echo     -v ^<LlvmVersion^> sets the LLVM version number used for the registry entry (Default is 3.6.1)
@echo.

:exit
endlocal
goto :EOF

:CongigureLLVMBuild
    @echo __--== Generating build configuration of LLVM For %2 ==--__    
    if NOT EXIST "build\%2" md build\%2
    pushd build\%2
    cmake -G"%~1" -DCMAKE_INSTALL_PREFIX=Install ..\..
    popd
    if %ERRORLEVEL% GTR 0 exit /B %ERRORLEVEL%

    goto :EOF

:BuildLLVMPlatform
    @echo __--== Building LLVM For %1 ==--__

    @echo __--== Building Release configuration For %1 ==--__
    cmake --build build\%1. --config Release 
    if %ERRORLEVEL% GTR 0 exit /B %ERRORLEVEL%

    @echo __--== Building Checked configuration For %1 ==--__
    cmake --build build\%1 --config RelWithDebInfo 
    if %ERRORLEVEL% GTR 0 exit /B %ERRORLEVEL%

    @echo __--== Building Debug configuration For %1 ==--__
    cmake --build build\%1 --config Debug 
    if %ERRORLEVEL% GTR 0 exit /B %ERRORLEVEL%

    goto :EOF
