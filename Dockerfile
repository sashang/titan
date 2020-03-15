FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
COPY /deploy /
WORKDIR /Server
EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]