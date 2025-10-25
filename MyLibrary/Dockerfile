# Bu, proyektimizi build etmək üçün lazım olan SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proyekt fayllarını köçürürük
COPY *.csproj ./
RUN dotnet restore

# Bütün kodları köçürürük
COPY . ./
RUN dotnet publish -c Release -o out

# İndi runtime image - daha yüngül
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render-in istifadə edəcəyi PORT-u dinləyirik
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "MyLibrary.dll"]