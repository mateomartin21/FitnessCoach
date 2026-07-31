# Imagen de FitnessCoach. Multi-etapa: el SDK solo vive en la etapa de compilacion,
# la imagen final lleva unicamente el runtime de ASP.NET.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: si no cambian, Docker reutiliza la capa del restore
# y no vuelve a bajar los paquetes en cada build.
COPY FitnessCoach.csproj ./
COPY FitnessCoach.Domain/FitnessCoach.Domain.csproj FitnessCoach.Domain/
COPY FitnessCoach.Application/FitnessCoach.Application.csproj FitnessCoach.Application/
COPY FitnessCoach.Infrastructure/FitnessCoach.Infrastructure.csproj FitnessCoach.Infrastructure/
RUN dotnet restore FitnessCoach.csproj

COPY . .
# Los .gz y .br de los estaticos se generan en el publish, no en el build (ADR-20).
RUN dotnet publish FitnessCoach.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Sin esto el proceso corre como root dentro del contenedor.
RUN useradd --uid 1001 --create-home --shell /usr/sbin/nologin fitnesscoach
USER 1001

COPY --from=build --chown=1001:1001 /app/publish .

# Kestrel escucha en 8080: el 80 exige privilegios que el usuario no root no tiene.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# La imagen NO trae cadena de conexion ni claves de IA: se pasan por entorno.
#   ConnectionStrings__DefaultConnection, Gemini__ApiKey, Groq__ApiKey
ENTRYPOINT ["dotnet", "FitnessCoach.dll"]
