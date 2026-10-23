# MiauShop — imagem para o Render (ou qualquer hospedagem com Docker).
# Etapa 1 compila com o SDK; etapa 2 roda só com o runtime (imagem bem menor).

# ---------- Etapa 1: compilar e publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura os pacotes primeiro (camada reaproveitada enquanto o .csproj não mudar).
COPY MiauShop/MiauShop.csproj MiauShop/
RUN dotnet restore MiauShop/MiauShop.csproj

COPY MiauShop/ MiauShop/
RUN dotnet publish MiauShop/MiauShop.csproj -c Release -o /app --no-restore

# ---------- Etapa 2: rodar ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production

# Usuário sem privilégios de administrador (já existe na imagem oficial da Microsoft).
USER $APP_UID

# O Render informa a porta na variável PORT (padrão 10000).
EXPOSE 10000
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-10000} exec dotnet MiauShop.dll"]
