cd..

set /p test="Enter the test name to run: "
dotnet build --configuration release
dotnet run --configuration release --project PerformanceTests\LibraryCore.Performance.Tests %test%