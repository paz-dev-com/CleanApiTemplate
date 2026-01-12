# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy nuget.config first if it exists
COPY ["nuget.config", "./"]

# Copy project files for restore
COPY ["CleanApiTemplate.API/CleanApiTemplate.API.csproj", "CleanApiTemplate.API/"]
COPY ["CleanApiTemplate.Core/CleanApiTemplate.Core.csproj", "CleanApiTemplate.Core/"]
COPY ["CleanApiTemplate.Data/CleanApiTemplate.Data.csproj", "CleanApiTemplate.Data/"]

# Restore packages
RUN dotnet restore "CleanApiTemplate.API/CleanApiTemplate.API.csproj"

# Copy everything else
COPY . .

WORKDIR "/src/CleanApiTemplate.API"
RUN dotnet build "CleanApiTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CleanApiTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanApiTemplate.API.dll"]
