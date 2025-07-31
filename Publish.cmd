@echo off
echo Cleaning...
dotnet clean --verbosity minimal
echo Publishing native binary...

rem dotnet publish ZipDir.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
rem dotnet publish ZipDir.csproj -r win-arm64 -c Release
dotnet publish ZipDir.csproj -r win-x64 -c Release
pause
