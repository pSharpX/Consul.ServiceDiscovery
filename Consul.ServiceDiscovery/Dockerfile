#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Consul.ServiceDiscovery/Consul.ServiceDiscovery.csproj", "Consul.ServiceDiscovery/"]
RUN dotnet restore "Consul.ServiceDiscovery/Consul.ServiceDiscovery.csproj"
COPY . .
WORKDIR "/src/Consul.ServiceDiscovery"
RUN dotnet build "Consul.ServiceDiscovery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Consul.ServiceDiscovery.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Consul.ServiceDiscovery.dll"]