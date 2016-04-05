@echo off
setlocal
set pack=%~n0
set pack=%pack:pack-=%
pushd Release\nupkg
powershell -ep bypass -f "..\..\package.ps1" "..\..\%pack%.nuspec" "..\%pack%"
popd
echo.
echo Done.
