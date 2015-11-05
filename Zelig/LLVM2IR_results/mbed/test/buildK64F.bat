@echo off

IF %1.==. (
    ECHO Error no test name given.  e.g. BuildK64F.bat GpioTest
    GOTO :EOF
)

"%dp~0%build.bat" %1 K64F 

EXIT /b