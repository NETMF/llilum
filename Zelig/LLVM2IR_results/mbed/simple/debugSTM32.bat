@echo off

"%dp~0%debug.bat" STM32L152\mbed_simple.elf %* -ex "target remote :3333" -ex "monitor halt reset" -ex "ni"

exit /b
