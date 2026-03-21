# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Install dependencies for QuestPDF + ClosedXML
RUN apt-get update && apt-get install -y libgdiplus

COPY --from=build /app/publish .

# Expose port (Render uses 10000 internally)
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "SparshaERP.dll"]