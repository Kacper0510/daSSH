FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /source
COPY daSSH.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
RUN apk add --no-cache \
    bash \
    openssh
WORKDIR /app
VOLUME /app/storage
EXPOSE 8080
COPY --from=build /app ./
CMD ["dotnet", "daSSH.dll"]
