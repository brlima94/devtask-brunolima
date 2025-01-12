FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DevTask.DiscountService.Server/DevTask.DiscountService.Server.csproj", "DevTask.DiscountService.Server/"]
RUN dotnet restore "DevTask.DiscountService.Server/DevTask.DiscountService.Server.csproj"
COPY . .
WORKDIR "/src/DevTask.DiscountService.Server"
RUN dotnet publish "DevTask.DiscountService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DevTask.DiscountService.Server.dll"]
