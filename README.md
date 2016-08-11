Welcome to LLILUM!
================== 

A development platform for IL based languages and UWP applications targeting Cortex-M processors.

## Why LLILUM 
Cortex-M processors are heavily resource constrained, and yet, because of their low power consumption and optimized interrupt dispatching, are arguably the most suitable solution for IoT applications.
Existing development platfoms and tool chains that target this space are based on C/C++, and no native support for higher level languages and RAD environment exist that can take advantage of what a Cortex-M HW can offer. 
Previous efforts in this space include [.NET Micro Framework](https://github.com/NETMF/netmf-interpreter) and other Java solutions, which are sometimes too large in code size, too slow or too difficult to extend for mainstream adoption. 
LLILUM implements a scalable alternative to C/C++ that features comparable code size and speed, as well as easy extensibility from within the Visual Studio development tool chain. 

## What is LLILUM 
LLILUM is an optimizing compiler tool chain based on the latest  [CLI](http://www.ecma-international.org/publications/standards/Ecma-335.htm) standard and [LLVM](http://llvm.org/) version [3.8.1](http://llvm.org/releases/download.html#3.8.1). 
LLILUM relies on the compelling development experience of Visual Studio to transform source code into MSIL, and then into an intermediate represenation (IR) that is optmized for size and speed, taking into account the target platform constraints, such as memory layout, native ISA, calling convention, and, first and foremost, the application requirements. The method and type closure is optimized based on application usage of system facilities, and the type system is morphed to shrink into the smallest possible footprint. Debugging is carried out through Visual Studio and GDB.
Standard features of CLI environments, e.g. type system (TS) introspection, garbage collection (GC) and a full featured multi-threaded execution engine (MTEE) are provided. 


# State of the Art 
LLILUM can generate runnable images for ARMv4 and ARMv5 ISA, with partial FP support. We could have extended the code generator to support ARMv7-M but we decided to leverage LLVM instead, hoping we can get a wider set of targets over time. 
The current incarnation of the system successfully uses LLVM to target: 
* Cortex-M0/3/4 ISA with 
* A fully functional Managed Type System and FP support 
* A reference counting collector
* A conservative tracing collector (Bohem GC) 
* A fully multi-threaded execution environment compatible with Cortex-M architecture for `NVIC` and `SysTick` standard controllers 
* [lwIP](http://savannah.nongnu.org/projects/lwip/) TCP/IP stack
* [CMSIS-RTOS](http://www.keil.com/pack/doc/cmsis/RTOS/html/index.html) porting layer for easy network stack porting 
* Visual Studio SDK with GDB debugging 

# Supported Hardware
LLILUM supports a significant number of development boards from the [mBed](https://www.mbed.com/en/) ecosystem, and specifically: 
* [Freescale K64F-FRDM](https://developer.mbed.org/platforms/FRDM-K64F/) Cortex-M4 development board
* [NXP LPC1768](https://developer.mbed.org/platforms/mbed-LPC1768/) Cortex-M3 development board
* [STMicroelectronics STM32F091RC](https://developer.mbed.org/platforms/ST-Nucleo-F091RC/) Cortex-M0 development board
* [STMicroelectronics STM32F401RE](https://developer.mbed.org/platforms/ST-Nucleo-F401RE/) Cortex-M3 development board
* [STMicroelectronics STM32F411RE](https://developer.mbed.org/platforms/ST-Nucleo-F411RE/) Cortex-M4 development board
* [STMicroelectronics STM32L152RE](https://developer.mbed.org/platforms/ST-Nucleo-L152RE/) Cortex-M3 development board

# Supported Languages 
Currrent target language is C#; extensions to Python and possibly TypeScript are in the works. We are also targeting [UWP](https://msdn.microsoft.com/en-us/library/dn894631.aspx) app development, so that it will be possible to share code between a Windows 10 device app and a Cortex-M micro processor. Welcome to OneCore!

# Further reading
Please see the following documents in our wiki:
Welcome to the _llilum_ wiki! 

1. [Detailed system description](https://github.com/NETMF/llilum-pr/wiki/System) 
  1. [Build System](https://github.com/NETMF/llilum-pr/wiki/Building)
  2. [Front End Configuration](https://github.com/NETMF/llilum-pr/wiki/LLILUM-Compiler-Frontend)
2. [Setup and build instruction](https://github.com/NETMF/llilum-pr/wiki/Setup)
3. [Build and run test demo](https://github.com/NETMF/llilum-pr/wiki/Demo)
4. [Performance considerations](https://github.com/NETMF/llilum-pr/wiki/Performance-Considerations)
5. [Next steps](https://github.com/NETMF/llilum-pr/wiki/Prototype-Roadmap) 


# Acknowledgments
A big thanks to Miguel Perez Martinez for helping bringing up this proof of concepts with exceptional dedication, perseverance and competency. An even bigger thanks to D.M., who created the vast majority of this codebase single-handedly. 
