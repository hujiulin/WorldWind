REM
REM You MUST run this from the VS.NET 2003 Command Prompt.
REM Just pointing devenv to the VS.NET /bin directory will *not* work.
REM
CALL "%VS80COMNTOOLS%\vsvars32.bat"

devenv WorldWind.sln /rebuild Debug
devenv WorldWind.sln /rebuild Release
