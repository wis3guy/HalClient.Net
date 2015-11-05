"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" HalClient.Net.sln /p:Configuration=Release /p:Platform="Any CPU"
cd HalClient.Net
..\.nuget\nuget.exe Pack HalClient.Net.csproj -Prop Configuration=Release
cd ..