#!/bin/sh
docker run -it -v $(pwd):/app microsoft/dotnet:sdk /bin/bash -c "cd app; dotnet publish -c Release -o out"