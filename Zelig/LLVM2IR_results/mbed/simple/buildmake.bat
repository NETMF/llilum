@echo off

IF %1.==. (
	ECHO No target passed in. Defaulting to LPC1768
	set TARGET=LPC1768
    set SIZE_OF_HEAP=0x6000
) ELSE (
	ECHO Detected target: %1
	set TARGET=%1
	IF "%1"=="K64F" (
		set SIZE_OF_HEAP=0x10000
	) ELSE (
		set SIZE_OF_HEAP=0x6000
	)
)

::make clean TARGET=%TARGET%
make DEBUG=1 TARGET=%TARGET% HEAP_SIZE=%SIZE_OF_HEAP%

