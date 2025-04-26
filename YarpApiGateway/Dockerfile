FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5261

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YarpApiGateway.csproj", "./"]
RUN dotnet restore "YarpApiGateway.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "YarpApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YarpApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YarpApiGateway.dll"] 