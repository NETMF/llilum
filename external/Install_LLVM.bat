:: Extract LLVM source.
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('LLVM-3.5.0.zip', 'LLVM'); }"

pushd LLVM

:: Generate solution and build it.
cmake -G"Visual Studio 12 2013 Win64"
msbuild LLVM.sln /p:Configuration=Debug;Platform=x64

popd
