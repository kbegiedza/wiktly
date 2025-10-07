FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /workspace

COPY ["src/Wiktly.API/Wiktly.API.csproj", "src/Wiktly.API/"]

RUN dotnet restore "src/Wiktly.API/Wiktly.API.csproj"

COPY . .

WORKDIR "/workspace/src/Wiktly.API"

RUN dotnet publish "Wiktly.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wiktly.API.dll"]
