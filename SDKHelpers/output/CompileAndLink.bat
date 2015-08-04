@echo off

IF %1.==. (
	ECHO No target passed in. Defaulting to LPC1768
	set TARGET=LPC1768
) ELSE (
	ECHO Detected target: %1
	set TARGET=%1
)

ECHO.
ECHO Linking with mbed libs...
make clean TARGET=%TARGET%
make TARGET=%TARGET%

GOTO :EXIT

:EXIT
ECHO.
ECHO Completed 

