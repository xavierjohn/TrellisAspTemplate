@echo off
dotnet build TrellisAspTemplate.slnx -c Release
IF ERRORLEVEL 1 ( EXIT /B %ERRORLEVEL% )
dotnet test TrellisAspTemplate.slnx -c Release --no-build
IF ERRORLEVEL 1 ( EXIT /B %ERRORLEVEL% )
dotnet pack templatepack.csproj -c Release -o nupkg -p:PublicRelease=true
