# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0  AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish *.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Create the UploadedFiles directory
RUN mkdir -p /app/UploadedFiles

# Expose port
EXPOSE 7259

# Set entry point
ENTRYPOINT ["dotnet", "XLead Server.dll"]