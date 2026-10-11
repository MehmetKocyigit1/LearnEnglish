# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Domain/IngilizceProje.Domain.csproj", "Domain/"]
COPY ["Application/IngilizceProje.Application.csproj", "Application/"]
COPY ["Infrastructure/IngilizceProje.Infrastructure.csproj", "Infrastructure/"]
COPY ["IngilizceProjeAPI/IngilizceProjeAPI.csproj", "IngilizceProjeAPI/"]

RUN dotnet restore "IngilizceProjeAPI/IngilizceProjeAPI.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/IngilizceProjeAPI"
RUN dotnet publish "IngilizceProjeAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5030
EXPOSE 5030

ENTRYPOINT ["dotnet", "IngilizceProjeAPI.dll"]
