# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Solution faylını kopyala və restore et
COPY ["MyLibrary.sln", "./"]
COPY ["MyLibrary/", "MyLibrary/"]
RUN dotnet restore "MyLibrary/MyLibrary.csproj"

# Layihəni build et
RUN dotnet build "MyLibrary/MyLibrary.csproj" -c Release -o /app/build

# 2️⃣ Publish stage
FROM build AS publish
RUN dotnet publish "MyLibrary/MyLibrary.csproj" -c Release -o /app/publish

# 3️⃣ Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyLibrary.dll"]
