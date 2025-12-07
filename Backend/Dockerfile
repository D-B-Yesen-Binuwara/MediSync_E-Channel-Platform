# -----------------------------
# 1. Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY Backend/*.csproj ./Backend/

# Restore dependencies
RUN dotnet restore Backend/Backend.csproj

# Copy all source files
COPY . .

# Publish the project
WORKDIR /src/Backend
RUN dotnet publish -c Release -o /app/publish

# -----------------------------
# 2. Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app


# Set ASP.NET Core to listen on port 5001 inside the container
# When running the container, map host port 5001 to container port 5001 (docker run -p 5001:5001)
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001

# Set ASP.NET Core to listen on port 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy published files from build stage
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "Backend.dll"]
