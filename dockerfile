FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PartnerBFF.API/PartnerBFF.API.csproj", "src/PartnerBFF.API/"]
COPY ["src/PartnerBFF.Application/PartnerBFF.Application.csproj", "src/PartnerBFF.Application/"]
COPY ["src/PartnerBFF.Infrastructure/PartnerBFF.Infrastructure.csproj", "src/PartnerBFF.Infrastructure/"]
RUN dotnet restore "src/PartnerBFF.API/PartnerBFF.API.csproj"
COPY . .
RUN dotnet build "src/PartnerBFF.API/PartnerBFF.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PartnerBFF.API/PartnerBFF.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PartnerBFF.API.dll"]