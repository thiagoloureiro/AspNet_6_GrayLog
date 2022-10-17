FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AspNet_6_GrayLog/AspNet_6_GrayLog.csproj", "AspNet_6_GrayLog/"]
RUN dotnet restore "AspNet_6_GrayLog/AspNet_6_GrayLog.csproj"
COPY . .
WORKDIR "/src/AspNet_6_GrayLog"
RUN dotnet build "AspNet_6_GrayLog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNet_6_GrayLog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNet_6_GrayLog.dll"]
