#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Kafka.Consumer/Kafka.Consumer.csproj", "Kafka.Consumer/"]
RUN dotnet restore "Kafka.Consumer/Kafka.Consumer.csproj"
COPY . .
WORKDIR "/src/Kafka.Consumer"
RUN dotnet build "Kafka.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kafka.Consumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.Consumer.dll"]