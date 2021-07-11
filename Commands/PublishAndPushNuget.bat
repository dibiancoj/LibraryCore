set /p pat="Enter GitHub Personal Access Token: "
cd..

REM The github action = "UpdateNugetPackages" will also run this in an automated fashion

REM if you run into any issues saying that the repo url or type is not filled out - make sure you don't have any .nupkg hanging out in the debug output for each project
dotnet pack --configuration Release
dotnet nuget push "**/*.nupkg" --source "https://nuget.pkg.github.com/dibiancoj/index.json" --api-key %pat% --skip-duplicate

echo '%pat%'