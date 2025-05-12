### build
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /sln
COPY ./*.sln ./src/*/*.csproj ./
RUN mkdir src
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

RUN dotnet restore "BabloBudget.sln"
COPY . .
RUN dotnet build "BabloBudget.sln" -c Release --no-restore

## tests
FROM mcr.microsoft.com/dotnet/sdk:9.0 as tests
WORKDIR /tests
COPY --from=build /sln/src/Tests .
ENTRYPOINT ["dotnet", "test", "-c", "Release", "--no-build"]

### publish
FROM build as publish
RUN dotnet publish "./src/BabloBudget.Api/BabloBudget.Api.csproj" -c Release --output /dist/services --no-restore

### services
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS services
EXPOSE 8018
ENV ASPNETCORE_URLS=http://+:8018
WORKDIR /app
COPY --from=publish /dist/services .
ENTRYPOINT ["dotnet", "BabloBudget.dll"]