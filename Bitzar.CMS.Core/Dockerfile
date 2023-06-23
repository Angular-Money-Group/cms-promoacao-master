#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat 
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019
EXPOSE 80
EXPOSE 443

# Install MySqlConnector
ADD https://downloads.mysql.com/archives/get/p/6/file/mysql-connector-net-8.0.13.msi .
Run msiexec.exe /i "C:\mysql-connector-net-8.0.13.msi" /qn

# Set working folder to wwwroot
WORKDIR /inetpub/wwwroot
RUN icacls 'C:\inetpub\wwwroot\' /grant 'IIS_IUSRS:(OI)(CI)F' /T

# Copy release version to workingfolder
COPY ${source:-/bin/Release/Publish} .