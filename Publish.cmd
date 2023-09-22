@echo off
echo Cleaning...
dotnet clean --verbosity minimal
echo Publishing native binary...

rem dotnet publish ZipDir.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
dotnet publish ZipDir.csproj -r win-x64 -c Release
pause
