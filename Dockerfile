# .NET Version
ARG VERSION=7.0-alpine

#Build Step
FROM mcr.microsoft.com/dotnet/sdk:$VERSION AS build-env
WORKDIR /src

COPY ["scripts/nuget.config", "./nuget.config"]

COPY . .
#COPY ["Utilities/Swaggers/", "Utilities/Swaggers/"]
#COPY ["Utilities/Utilities/", "Utilities/Utilities/"]

RUN dotnet publish "queueapi.csproj" \
	-c Release \
	-o /app/publish \
	-r alpine-x64 \
	--self-contained true \
	/p:PublishReadyToRun=true \
	/p:PublishReadyToRunShowWarnings=true \
	/p:PublishSingleFile=true \
	/p:TargetLatestRuntimePatch=true

RUN chmod u+x,o+x /app/publish

#Excute image
FROM mcr.microsoft.com/dotnet/aspnet:$VERSION AS final

ENV APP_USER=app \
	DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
	DOTNET_RUNNING_IN_CONTAINER=true \
	ASPNETCORE_ENVIRONMENT=Production \
	ASPNETCORE_URLS=http://+:8080 \
	TZ=Asia/Bangkok

RUN apk add libgdiplus --repository https://dl-3.alpinelinux.org/alpine/edge/testing/
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib tzdata libgdiplus --upgrade bash
# RUN addgroup -S $APP_USER && \
#  	adduser -S $APP_USER -G $APP_USER
RUN addgroup -g 1000 $APP_USER && adduser -u 1000 -G $APP_USER -s /bin/ash -h /home/$APP_USER -D $APP_USER
# COPY ["certs/ServerPersonalAccounting.pfx","/https/servercert.pfx"]
# COPY ["certs/CaPersonalAccounting.crt","/usr/local/share/ca-certificates/cacert.crt"]

#COPY ["https.pfx","/https/aspnetapp.pfx"]
#RUN dotnet dev-certs https --trust

#RUN update-ca-certificates

WORKDIR $APP_USER

COPY ["scripts/hardening/harden.sh", "./harden.sh"]
RUN chmod +x harden.sh
CMD	"/harden.sh"
RUN	rm harden.sh

COPY --from=build-env --chown=$APP_USER:$APP_USER /app/publish .

#COPY --from=publish /app/publish .

COPY ["scripts/hardening/post-install.sh", "./post-install.sh"]
RUN chmod +x post-install.sh
CMD	"/post-install.sh queueapi"
RUN	rm post-install.sh

USER $APP_USER
EXPOSE 8080

ENTRYPOINT ["./queueapi"]