### Llilum SDK Installer
This folder contains the Solution for building the Llilum SDK MSI installer. 

##### Prerequisites 
The installer is built using the [Windows Installer XML (Wix) v3.10.2](https://wix.codeplex.com/downloads/get/1540240)

#### Building the Binaries that go into the SDK
The SDK will incorporate the binareis built by other projects in the build. While it might make sense to define
dependencies in the solution on those projects so that a single project builds everything - that doesn't really work
for automated builds with digital signatures for a final production release. The binaries in the MSI must all get a
proper digital signature and or strong name signed before they are bound into the MSI. After that the MSI is generated
a signature is generated for the MSI itself. Thus keeping the MSI build isolated helps in managing the multi-stage
process of creating a fully signed release build.

The Following solutions and projects must be built to provide the input binaries for the SDK MSI:  
1. Zelig.sln solution  
2. BoardConfigurations.sln solution  

###### Note:  
In the future the output of the board configurations solution will not go into the SDK itself, rather it will convert to a sample project
that **uses** the SDK to create Board Support packages. This would include bundling the output of the build to a NUGET package to make
adding a BSP to a project a simple act of adding a NUGET pacakge. This requires re-working the native project system to move the board
selection and related properties out of the built in build infrastructure. We haven't quite made it that far so, for the short term the
SDK includes the board configurations built in the Llilum core team.  

#### Building the MSI
You need to install the Wix Toolset before you can build this solution. Once you have installed the Wix Toolset follow these simple steps:  
1. Open the LlilumSDK.sln solution file  
2. Select **Build | Build Solution**  
3. The MSI will be gnerated into the ZeligBuild\Host\bin\$(Configuration) folder

#### Using the SDK
In order to use the SDK you must install the following:  
1. The MSI  
2. The VSIX package for the Llilum project system.  
3. The ARM GCC compiler (Assuming you are targeting Cortex-M class devices)  
4. Appropriate Debugger components (pyOCD, OpenOCD, mBed Serial Drivers, etc...)  

Once you have installed these you can open Visual Studio and create a new LlilumApplication and code away...

###### NOTE:
The SDK and VSIX package are not bundled together to allow for faster and simpler updates as well as to keep the SDK from becoming
bound to a specific version of Visual Studio. The release cadence of VS has increased dramatically and maintaining multiple variants
of the SDK for each VS version is logistically challenging and distracting.  
At best we could add a bootstrapper executable that installed the MSI, then detected VS installations and downloaded the appropriate
VSIX pacakges. That is a significant amount of work as the bootstrapper would require a fairly high degree of future proofing to ensure
it could detect future Visual Studio installations, AND find an appropriate VSIX package to go with it. Ideally that would *not* require
creating a new web service to provide information to the bootstrapper. (Although leveraging the public readable nature of Github over
HTTPS, it could use a data file pushed on GitHub to pull down and use to extract information) None of that is particularly deeply thought
out or considered for active development yet.


