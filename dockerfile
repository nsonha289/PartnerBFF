FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PartnerBFF.API/PartnerBFF.API.csproj",                       "PartnerBFF.API/"]
COPY ["PartnerBFF.Application/PartnerBFF.Application.csproj",       "PartnerBFF.Application/"]
COPY ["PartnerBFF.Infrastructure/PartnerBFF.Infrastructure.csproj", "PartnerBFF.Infrastructure/"]

RUN dotnet restore "PartnerBFF.API/PartnerBFF.API.csproj"

COPY . .

RUN dotnet build "PartnerBFF.API/PartnerBFF.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PartnerBFF.API/PartnerBFF.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PartnerBFF.API.dll"]