@echo off
setlocal ENABLEEXTENSIONS EnableDelayedExpansion

IF %1.==. (
    REM - Look for the ARM GCC tools in registry (installer by default will create this entry)
    FOR /F "usebackq skip=2 tokens=1-2*" %%A IN (`REG QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\ARM\GNU Tools for ARM Embedded Processors" /v "InstallFolder" /reg:32 2^>nul`) DO (
        set ArmGccRoot=%%C
    )
    IF "!ArmGccRoot!"=="" (
        @echo Error: The path to GCC tools was not provided and was not found in the registry
        goto :EOF
    )
) else (
    set ArmGccRoot=%~s1
)

set LLVMSrcValue=%2
IF NOT %2.==. GOTO SKIPREG

REM - Look for the LLVM reg key set by BuildllvmWithVS script
FOR /F "usebackq skip=2 tokens=1-2*" %%A IN (`REG QUERY "HKEY_CURRENT_USER\SOFTWARE\LLVM\3.7.0" /v SrcRoot 2^>nul`) DO (
    set LLVMRegValue=%%C
)
REM - try per machine setting
IF "%LLVMRegValue%"=="" (
    FOR /F "usebackq skip=2 tokens=1-2*" %%A IN (`REG QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\LLVM\3.7.0" /v SrcRoot 2^>nul`) DO (
        set LLVMRegValue=%%C
    )
)

REM - If LLVM SrcRoot was not found, try to use the LLVM_SRCROOT_DIR environment variable
if defined LLVMRegValue (
    @echo Found LLVM at %LLVMRegValue% using reg key
    set LLVMSrcValue=%LLVMRegValue%
) else (
    @echo Using parameter for LLVM Src path: %2
    set LLVMSrcValue=%2
)

:SKIPREG
if not exist "%ArmGccRoot%\bin" (
    echo Error: The path entered for GCC must be to the version folder.
    echo Ex: C:\ARM_GCC\4.9\
    goto :EOF
)
if not exist "%LLVMSrcValue%" (
    echo Error: LLVM directory does not exist.
    echo Ex: C:\LLVM\3.7.0\
    goto :EOF
)

setx LLILUM_GCC %1\
echo Set LLILUM_GCC to %1\

setx LLILUM_LLVM %LLVMSrcValue%\
echo Set LLILUM_LLVM to %LLVMSrcValue%\

setx LLILUM_SDK %CD%\
echo Set LLILUM_SDK to %CD%\

@rem - It is important NOT to use parentheses for the following two lines.
@rem - This is due to how the commandline parser handles/mis-handles parentheses.
@rem - Since the environment variables and their values contain parentheses in them
@rem - The parser gets confused and treats them as part of the if statement itself
@rem - when the if statement includes any parentheses.
if exist "%ProgramFiles(x86)%" set PROG_FILE_DIR=%ProgramFiles(x86)%
if not exist "%ProgramFiles(x86)%" set PROG_FILE_DIR=%ProgramFiles%

@echo using PROG_FILE_DIR=%PROG_FILE_DIR%

if not exist "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type" (
    echo Error: MSBuild is not installed to where it should be. "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type"
    goto :EOF
)

if exist "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum" (
    echo Deleting old Llilum Application Type directory
    rmdir /s /q "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"
)

mkdir "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"
robocopy /E ".\LlilumApplicationType\Llilum" "%PROG_FILE_DIR%\MSBuild\Microsoft.Cpp\v4.0\V140\Application Type\Llilum"

start LlilumApplication.vsix

echo DONE!
echo See SDKDrop\tools\readme.txt for the PY_OCD download link
goto :EOF

:USAGE
echo Usage:
echo InstallVSTools.cmd (path to GCC) (optional)(path to LLVM)

