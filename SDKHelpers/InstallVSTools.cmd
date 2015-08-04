@echo off

IF %1.==. GOTO USAGE
set LLVMSrcValue=%2
IF NOT %2.==. GOTO SKIPREG

:: Look for the LLVM reg key set by BuildllvmWithVS script
setlocal ENABLEEXTENSIONS
set KEY_NAME="HKEY_CURRENT_USER\SOFTWARE\LLVM\3.6.1"
set VALUE_NAME=SrcRoot
FOR /F "usebackq skip=2 tokens=1-2*" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul`) DO (
    set LLVMRegValue=%%C
)

:: If LLVM SrcRoot was not found, try to use the LLVM_SRCROOT_DIR environment variable
if defined LLVMRegValue (
    @echo Found LLVM at %LLVMRegValue% using reg key
    set LLVMSrcValue=%LLVMRegValue%
) else (
    @echo Using parameter for LLVM Src path: %2
    set LLVMSrcValue=%2
)

:SKIPREG
if not exist "%1\bin" (
    echo Error: The path entered for GCC must be to the version folder.
    echo Ex: C:\ARM_GCC\4.9\
    goto END
)
if not exist "%LLVMSrcValue%" (
    echo Error: LLVM directory does not exist.
    echo Ex: C:\ARM_GCC\4.9\
    goto END
)

setx LLILUM_GCC %1\
echo Set LLILUM_GCC to %1\

setx LLILUM_LLVM %LLVMSrcValue%\
echo Set LLILUM_LLVM to %LLVMSrcValue%\

setx LLILUM_SDK %CD%\
echo Set LLILUM_SDK to %CD%\

if exist "%ProgramFiles(x86)%" (
    echo Running on 64 bit machine. Using %ProgramFiles(x86)%
    set PROG_FILE_DIR=%ProgramFiles(x86)%
) 

if not exist "%ProgramFiles(x86)%" (
    echo Running on 32 bit machine. Using %ProgramFiles%
    set PROG_FILE_DIR=%ProgramFiles%
)

if not exist "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type" (
    echo Error: MSBuild is not installed to where it should be. "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type"
    goto END
)

if exist "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum" (
    echo Deleting old Llilum Application Type directory
    rmdir /s /q "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"
)

mkdir "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"
robocopy /E .\LlilumApplicationType\Llilum "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"

start .\LlilumProjectType\LlilumApplication\LlilumApplication.ProjectType\bin\Debug\LlilumApplication.vsix

echo DONE!
echo See SDKDrop\tools\readme.txt for the PY_OCD download link
goto END

:USAGE
echo Usage:
echo InstallVSTools.cmd (path to GCC) (optional)(path to LLVM)

:END