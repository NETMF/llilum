Welcome to LLILUM!
================== 

A development platform for IL based languages and UWP applications targeting Cortex-M processors.

## Why LLILUM 
Cortex-M processors are heavily resource contrained, and yet, because of their low power consumption and optimized interrupt dispatching, are arguably the most suitable solution for IoT applications.
Existing development platfoms and tool chains that target this space are based on C/C++, and no native support for higher level languages and RAD environment exist that can take advantage of what a Cortex-M HW can offer. 
Previous efforts in this space include [.NET Micro Framework](https://github.com/NETMF/netmf-interpreter) and other Java solutions, which are sometimes too large in code size, too slow or too difficult to extend for mainstream adoption. 
LLILUM implements a scalable alternative to C/C++ that features comparable code size and speed, as well as easy extensibility from within the Visual Studio development tool chain. 

## What is LLILUM 
LLILUM is an optimizing compilation tool chain that, just like .NET Micro Framework, is entirely based on [CLI](http://www.ecma-international.org/publications/standards/Ecma-335.htm). 
LLILUM relies on the well known development experience in Visual Studio to transform source code into MSIL, and then into an intermediate represenation (IR) that is optmized for size and speed, taking into account the target platform constraints, such as memory layout, native ISA, calling convention, and, first and foremost, the application requirements. The method and type closure is optimized based on application usage of system facilities, and the type system is morphed to shrink into the smallest possible footprint. 
Standard features of CLI environments, e.g. type system (TS) introspection, garbage collection (GC) and a full featured multi-threaded execution engine (MTEE) are provided. 

## State of the Art 
LLILUM can generate runnable images for ARMv4 and ARMv5 ISA, with partial FP support. We could extend the code generator to support ARMv7-M but we decided to leverage LLVM instead, hoping we can get a wider set of targets over time. The current incarnation of the system successfully uses LLVM to target a Cortex-M ISA with a fully functional Type System. GC and MTEE are in the works. 

# Supported Languages 
Currrent target language is C#; extensions to Python and possibly TypeScript are in the works. We are also targeting [UWP](https://msdn.microsoft.com/en-us/library/dn894631.aspx) app development, so that it will be possible to share code between a Windows 10 device app and a Cortex-M micro processor. Welcome to One Core!

# Further reading
Please see the following documents in our wiki:

1. [Detailed system description](https://github.com/NETMF/llilum-pr/wiki/system) 
  1. [Build System](https://github.com/NETMF/llilum-pr/wiki/system.build)
  2. [Front End Configuration](https://github.com/NETMF/llilum-pr/wiki/system.frontend)
2. [Repo layout](https://github.com/NETMF/llilum-pr/wiki/docs/resources/repo) 
3. [Setup and build instruction](https://github.com/NETMF/llilum-pr/wiki/setup)
4. [Build and run test demo](https://github.com/NETMF/llilum-pr/wiki/demo)
5. [Performance considerations](https://github.com/NETMF/llilum-pr/wiki/perf)
6. [Next steps](https://github.com/NETMF/llilum-pr/wiki/schedule) 
