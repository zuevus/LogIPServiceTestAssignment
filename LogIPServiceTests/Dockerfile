FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install Java (required for Allure)
RUN apt-get update && \
    apt-get install -y openjdk-17-jre && \
    rm -rf /var/lib/apt/lists/*

RUN curl -o allure-2.25.0.tgz -Ls https://repo.maven.apache.org/maven2/io/qameta/allure/allure-commandline/2.25.0/allure-commandline-2.25.0.tgz && \
    tar -zxvf allure-2.25.0.tgz -C /opt/ && \
    ln -s /opt/allure-2.25.0/bin/allure /usr/bin/allure && \
    rm allure-2.25.0.tgz

WORKDIR /src

COPY ["LogIPServiceTestAssignment/LogIPServiceTestAssignment.csproj", "./"]
COPY ["LogIPServiceTests.csproj", "./"]
RUN dotnet restore "LogIPServiceTests.csproj"

COPY . .

RUN dotnet test "LogIPServiceTests.csproj" --logger:"trx;LogFileName=testresults.trx" --results-directory /allure-results
RUN allure generate /allure-results -o /allure-report --clean

FROM nginx:alpine
COPY --from=build /allure-report /usr/share/nginx/html
EXPOSE 80
