cd HalClient.Net
copy ..\ReadMe.md ReadMe.txt
..\.nuget\nuget.exe Pack HalClient.Net.csproj -Prop Configuration=Release -build
cd ..