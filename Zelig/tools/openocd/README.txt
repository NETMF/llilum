For information on how to acquire OpenOCD or build it for Windows, please go here: http://openocd.org/getting-openocd/

This folder acts as the OpenOCD root folder. Below is the structure of how it should look:
.\bin
.\bin-x64
.\drivers
.\scripts\interface
.\scripts\board
.\scripts\...

In order to use OpenOCD with the debugXXX.bat scripts, please run the setenv.bat script in the Llilum root 
directory before running debug scripts

For debugging STM32 boards, users need to download "ST-Link, ST-Link/V2, ST-Link/V2-1 USB driver signed"
which, at the time of this writing, can be obtained from here: http://www.st.com/web/en/catalog/tools/PF260219#

For deploying to STM32 boards, it is recommended that users download the "ST-Link Utility"
which, at the time of this writing, can be obtained from here: http://www.st.com/web/en/catalog/tools/PF258168#
