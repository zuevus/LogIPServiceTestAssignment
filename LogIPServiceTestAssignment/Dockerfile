FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LogIPServiceTestAssignment.csproj .
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app .

RUN dotnet tool install --global dotnet-ef --version 7.0.0

ENV PATH="$PATH:/root/.dotnet/tools"

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 50051

ENTRYPOINT ["./entrypoint.sh"]