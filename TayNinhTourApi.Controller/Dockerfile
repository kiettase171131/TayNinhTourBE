FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TayNinhTourApi.Controller/TayNinhTourApi.Controller.csproj", "TayNinhTourApi.Controller/"]
COPY ["TayNinhTourApi.BusinessLogicLayer/TayNinhTourApi.BusinessLogicLayer.csproj", "TayNinhTourApi.BusinessLogicLayer/"]
COPY ["TayNinhTourApi.DataAccessLayer/TayNinhTourApi.DataAccessLayer.csproj", "TayNinhTourApi.DataAccessLayer/"]
RUN dotnet restore "TayNinhTourApi.Controller/TayNinhTourApi.Controller.csproj"
COPY . .
WORKDIR "/src/TayNinhTourApi.Controller"
RUN dotnet build "./TayNinhTourApi.Controller.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TayNinhTourApi.Controller.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TayNinhTourApi.Controller.dll"]