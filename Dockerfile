FROM docker.io/markadams/chromium-xvfb

RUN apt-get update
RUN apt-get install -y software-properties-common

RUN apt-get update && apt-get install -y wget
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb

RUN apt-get install -y gpg
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list
RUN apt-get install -y apt-transport-https
RUN apt-get update
RUN apt-get install -y dotnet-sdk-3.0

COPY HouseFinderConsoleBot/bin/Release/netcoreapp3.0/publish/ app/
COPY HouseFinderConsoleBot/bin/Release/netcoreapp3.0/publish/files app/

ENTRYPOINT ["dotnet", "app/HouseFinderConsoleBot.dll"]