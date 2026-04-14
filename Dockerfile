FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Prueba/Prueba.csproj", "Prueba/"]
RUN dotnet restore "Prueba/Prueba.csproj"

COPY . .
WORKDIR "/src/Prueba"
RUN dotnet publish "Prueba.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Prueba.dll"]
