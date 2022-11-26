#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk
#ARG Jason
RUN --mount=type=secret,id=Jason
WORKDIR /app

COPY . .
#RUN dotnet nuget add source --username dibiancoj@gmail.com --password $Jason --store-password-in-clear-text --name github "https://nuget.pkg.github.com/dibiancoj/index.json"
RUN dotnet restore IntegrationTests/LibraryCore.IntegrationTests/LibraryCore.IntegrationTests.csproj
ENTRYPOINT ["dotnet", "test", "IntegrationTests/LibraryCore.IntegrationTests/LibraryCore.IntegrationTests.csproj"]