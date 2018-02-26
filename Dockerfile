FROM microsoft/dotnet
RUN mkdir /app
WORKDIR /app
COPY ./ScanerSimulator .
EXPOSE 9014
ENTRYPOINT ["dotnet","run"]

