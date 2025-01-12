FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DevTask.DiscountService.Client.Web/DevTask.DiscountService.Client.Web.csproj", "DevTask.DiscountService.Client.Web/"]
RUN dotnet restore "DevTask.DiscountService.Client.Web/DevTask.DiscountService.Client.Web.csproj"
COPY . .
WORKDIR "/src/DevTask.DiscountService.Client.Web"
RUN dotnet publish "DevTask.DiscountService.Client.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DevTask.DiscountService.Client.Web.dll"]
