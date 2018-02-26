FROM microsoft/dotnet
RUN mkdir /app
WORKDIR /app
COPY . .
EXPOSE 9014
ENTRYPOINT ["dotnet","run"]

