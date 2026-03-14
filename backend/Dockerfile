FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY SideReport.sln ./
COPY src/SideReport.Domain/SideReport.Domain.csproj src/SideReport.Domain/
COPY src/SideReport.Application/SideReport.Application.csproj src/SideReport.Application/
COPY src/SideReport.Infrastructure/SideReport.Infrastructure.csproj src/SideReport.Infrastructure/
COPY src/SideReport.Api/SideReport.Api.csproj src/SideReport.Api/
COPY tests/SideReport.Tests/SideReport.Tests.csproj tests/SideReport.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish src/SideReport.Api/SideReport.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "SideReport.Api.dll"]
