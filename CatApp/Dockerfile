# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install necessary utilities
RUN apt-get update && apt-get install -y iputils-ping telnet netcat-openbsd

# Install Microsoft SQL Server Command-Line Tools (extra toolset just in case)
RUN apt-get update && apt-get install -y curl gnupg \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/debian/10/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev \
    && echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc

 

# Set the user to root
USER root
#USER $APP_UID

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Fix the paths of the project files
COPY ["CatApp/CatApp.csproj", "CatApp/"]
COPY ["CatApp.Services/CatApp.Services.csproj", "CatApp.Services/"]
COPY ["CatApp.Shared/CatApp.Shared.csproj", "CatApp.Shared/"]
RUN dotnet restore "./CatApp/CatApp.csproj"
#COPY . .  # Make sure the rest of the source code is copied
COPY CatApp/ CatApp/
COPY CatApp.Services/ CatApp.Services/
COPY CatApp.Shared/ CatApp.Shared/
WORKDIR "/src/CatApp"
RUN dotnet build "./CatApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./CatApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app


# Install dos2unix  <-------------- IMPORTANT FOR CHANGING (CRLF → LF) for entrypoint.sh  cause it fails silently  !!!!
RUN apt-get update && apt-get install -y dos2unix

# Copy published files
COPY --from=publish /app/publish .
# Copy source files for EF Core commands
COPY --from=build /src /src

# Install EF Core tools -- an attempt to use ef migrations during docker compose ( currently not used )
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Install netcat for SQL Server readiness check
RUN apt-get update && apt-get install -y netcat-openbsd

EXPOSE 80
#EXPOSE 8080
#EXPOSE 8081

# Copy entrypoint script
# Entrypoint of the script must be in the solution root folder
RUN echo "copying critical  start up file --> entrypoint.sh"
COPY ../entrypoint.sh /app/entrypoint.sh

# IMPORTANT - IMPORTANT - IMPORTANT (CRLF → LF) or the app not starts at all !!!
RUN echo "Ensure  entrypoint.sh  -- is LF -- NOT -- CRLF"
RUN dos2unix /app/entrypoint.sh && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]