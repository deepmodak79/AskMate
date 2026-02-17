# Backend Dockerfile for Render / Docker deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/DeepOverflow.API/DeepOverflow.API.csproj", "DeepOverflow.API/"]
COPY ["src/DeepOverflow.Application/DeepOverflow.Application.csproj", "DeepOverflow.Application/"]
COPY ["src/DeepOverflow.Domain/DeepOverflow.Domain.csproj", "DeepOverflow.Domain/"]
COPY ["src/DeepOverflow.Infrastructure/DeepOverflow.Infrastructure.csproj", "DeepOverflow.Infrastructure/"]

RUN dotnet restore "DeepOverflow.API/DeepOverflow.API.csproj"

COPY src/ .
WORKDIR "/src/DeepOverflow.API"
RUN dotnet build "DeepOverflow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeepOverflow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DeepOverflow.API.dll"]
