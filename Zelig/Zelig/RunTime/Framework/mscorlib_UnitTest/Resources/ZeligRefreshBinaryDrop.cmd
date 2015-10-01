@echo off
setlocal

cd/d %SDXROOT%\Zelig

if /I "%1" == "full" (set msbuild_opt=/t:Rebuild)

msbuild Zelig.sln %msbuild_opt% /p:Configuration=Debug
msbuild Zelig.sln %msbuild_opt% /p:Configuration=Release

cd %SDXROOT%\Zelig\BinaryDrop
sd edit ...

rd/s/q %SDXROOT%\Zelig\BinaryDrop
md 	   %SDXROOT%\Zelig\BinaryDrop\Host
md 	   %SDXROOT%\Zelig\BinaryDrop\Target

xcopy/s/e %SDXROOT%\ZeligBuild\Host\bin   %SDXROOT%\Zelig\BinaryDrop\Host
xcopy/s/e %SDXROOT%\ZeligBuild\Target\bin %SDXROOT%\Zelig\BinaryDrop\Target
del/s *.vshost.exe*

sd add       ...
sd online    ...
sd revert -a ...

