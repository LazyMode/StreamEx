@echo off
powershell -ep bypass -f redirect.ps1 StreamEx.Await StreamEx.Await.CS
pushd Release\nupkg
powershell -ep bypass -f "..\..\package.ps1" "..\..\StreamEx.Await.nuspec" "..\StreamEx"
popd
echo.
echo Done.
