#!/bin/sh

# Wait for the database to be ready
echo "Waiting for PostgreSQL to start..."
while ! nc -z postgres 5432; do
  sleep 1
done
echo "PostgreSQL started."

# Apply database migrations
echo "Applying database migrations..."
dotnet ef database update --project /app/WorkerService.csproj --connection "Host=postgres;Database=logsdb;Username=postgres;Password=postgres"

# Start the application
echo "Starting the application..."
dotnet LogIPServiceTestAssignment.dll