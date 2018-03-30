FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY SignalRSample ./SignalRSample
COPY NLog.SignalR.Core ./NLog.SignalR.Core
COPY lib ./lib

WORKDIR /src/SignalRSample
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
EXPOSE 5000
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SignalRSample.dll"]
