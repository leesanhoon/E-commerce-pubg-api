FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy the rest of the code and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Configure environment variables for performance
ENV ASPNETCORE_URLS=http://+:80 \
    COMPlus_ThreadPool_ForceMinWorkerThreads=100 \
    COMPlus_ThreadPool_ForceMaxWorkerThreads=100

EXPOSE 80

ENTRYPOINT ["dotnet", "E-commerce-pubg-api.dll"]