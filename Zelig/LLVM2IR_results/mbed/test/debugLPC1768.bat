@echo off

"%dp~0%debug.bat" LPC1768\*.elf %* -ex "target remote :3333" -ex "monitor halt reset" -ex "ni"

exit /b
