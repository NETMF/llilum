@ECHO OFF
setlocal enabledelayedexpansion

IF %1.==. (
        ECHO No target passed in. Defaulting to LPC1768
        set TARGET=LPC1768
) ELSE (
        ECHO Detected target: %1
        set TARGET=%1
)


SET DEPLOY_DRIVE=e:

IF "%2" NEQ "" (
   set DEPLOY_DRIVE=%2
) ELSE IF EXIST "d:\MBED.HTM" (
   set DEPLOY_DRIVE=d:
) ELSE IF EXIST "e:\MBED.HTM" (
   set DEPLOY_DRIVE=e:
) ELSE IF EXIST "f:\MBED.HTM" (
   set DEPLOY_DRIVE=f:
) ELSE IF EXIST "g:\MBED.HTM" (
   set DEPLOY_DRIVE=g:
) ELSE IF EXIST "h:\MBED.HTM" (
   set DEPLOY_DRIVE=h:
)

IF NOT EXIST %DEPLOY_DRIVE%\ (
  @ECHO.
  @ECHO Error: device drive not found!
  @ECHO.
  GOTO :USAGE
)

@ECHO Deleting old image (%DEPLOY_DRIVE%\*.bin) ...
del %DEPLOY_DRIVE%\*.bin

@ECHO Installing new image (%TARGET%\mbed_simple.bin) to %DEPLOY_DRIVE%\...
copy %TARGET%\mbed_simple.bin %DEPLOY_DRIVE%\.

@ECHO Complete!

GOTO :EOF

:USAGE
@ECHO Usage: deploy.bat [DeployDeviceDrive]
@ECHO    e.g. 'deploy.bat e:'
@ECHO.
