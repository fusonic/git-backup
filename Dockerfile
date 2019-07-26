FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /src
COPY . ./
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app

RUN apt-get update \
 && apt-get install -y --no-install-recommends git \
 && rm -rf /tmp/* /var/tmp/* /var/lib/apt/lists/* /usr/share/man/ || true

COPY --from=build-env /app .
ENTRYPOINT ["dotnet",  "fusonic-git-backup.dll"]