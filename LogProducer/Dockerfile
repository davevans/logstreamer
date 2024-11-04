FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LogProducer/LogProducer.csproj", "LogProducer/"]
RUN dotnet restore "LogProducer/LogProducer.csproj"
COPY . .
WORKDIR "/src/LogProducer"
RUN dotnet build "LogProducer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogProducer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogProducer.dll"]
