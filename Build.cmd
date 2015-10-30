rem Build with Visual Studio's version of MSBuild that understands C# 6
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" HalClient.Net.sln /p:Configuration=Release /p:Platform="Any CPU"
cd HalClient.Net
copy ..\ReadMe.md ReadMe.txt
rem Pack without actually doing a build
..\.nuget\nuget.exe Pack HalClient.Net.csproj -Prop Configuration=Release
cd ..