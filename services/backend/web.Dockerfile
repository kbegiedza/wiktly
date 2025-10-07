FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /workspace

COPY ["src/Wiktly.Web/Wiktly.Web.csproj", "src/Wiktly.Web/"]

RUN dotnet restore "src/Wiktly.Web/Wiktly.Web.csproj"

COPY . .

WORKDIR "/workspace/src/Wiktly.Web"

RUN dotnet publish "Wiktly.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wiktly.Web.dll"]
