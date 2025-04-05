# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Build and publish
RUN dotnet restore && \
    dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/* && \
    adduser --disabled-password --home /app --no-create-home --uid 1000 appuser && \
    chown -R appuser:appuser /app

# Copy published files
COPY --from=build --chown=appuser:appuser /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

# Switch to non-root user
USER appuser

EXPOSE 80

ENTRYPOINT ["dotnet", "E-commerce-pubg-api.dll"]