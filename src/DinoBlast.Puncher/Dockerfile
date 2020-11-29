# Buildtime
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "DinoBlast.Puncher.dll"]
