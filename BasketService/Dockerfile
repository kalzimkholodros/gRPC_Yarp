FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5114

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasketService/BasketService.csproj", "BasketService/"]
COPY ["Protos/", "BasketService/Protos/"]
RUN dotnet restore "BasketService/BasketService.csproj"
COPY ["BasketService/", "BasketService/"]
WORKDIR "/src/BasketService"
RUN dotnet build "BasketService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/BasketService/Protos /app/Protos
ENTRYPOINT ["dotnet", "BasketService.dll"] 