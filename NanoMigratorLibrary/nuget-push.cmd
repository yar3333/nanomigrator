@ECHO OFF

IF [%2]==[] GOTO :help

nuget push NanoMigratorLibrary.%1.nupkg %2 -Source https://api.nuget.org/v3/index.json
goto exit

:help
echo.
echo Usage: nuget-push ^<version^> ^<token^>
echo.

:exit