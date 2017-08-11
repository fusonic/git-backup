FROM microsoft/dotnet:runtime

WORKDIR /app

COPY out .

RUN apt-get update \
 && apt-get install -y --no-install-recommends git \
 && rm -rf /tmp/* /var/tmp/* /var/lib/apt/lists/* /usr/share/man/ || true

CMD dotnet /app/fusonic-git-backup.dll
