#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3500
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrganizerServer/OrganizerServer.csproj", "OrganizerServer/"]
RUN dotnet restore "OrganizerServer/OrganizerServer.csproj"
COPY . .
WORKDIR "/src/OrganizerServer"
RUN dotnet build "OrganizerServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrganizerServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrganizerServer.dll"]