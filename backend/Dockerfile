FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/SaleManagement.Api/SaleManagement.Api.csproj", "SaleManagement.Api/"]
RUN dotnet restore "SaleManagement.Api/SaleManagement.Api.csproj"
COPY src/SaleManagement.Api/ SaleManagement.Api/
WORKDIR /src/SaleManagement.Api
RUN dotnet build "SaleManagement.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SaleManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SaleManagement.Api.dll"]
