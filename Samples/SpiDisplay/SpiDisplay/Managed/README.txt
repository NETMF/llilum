#
# Using a modified board configuration
#

The mbed Application Board has a SPI display connected to SPI0. The MISO and CS pins, however,
are not connected as SPI pins; they are mapped to _reset and _A0, respectively. 

In order to ensure that the MISO and CS pins are not reserved by the SpiDevice, we must modify
the board configuration assembly (LPC1768.dll) and create a new one (LPC1768ApplicationBoard.dll).

Below are the steps for how we accomplished that goal:
1. Copy the LPC1768 project from Zelig\Runtime\DeviceModels\CortexM\Boards to the Solution Dir.
2. Add the copied project to the solution, rename it, and rename its assembly in AssemblyInfo.cs
3. Edit the project properties so that build output goes to SDKDrop\ZeligBuild\Target\bin\Debug\
4. Resolve any reference issues, and ensure that all DLLs are in SDKDrop\ZeligBuild\Target\bin\Debug\
5. Change the LPC1768 reference in the Managed project to LPC1768ApplicationBoard
6. Ensure that the managed projects build correctly
7. In mbed_simple.FrontEndConfig, change "-Reference LPC1768" to "-Reference LPC1768ApplicationBoard"
8. Rebuild the entire solution