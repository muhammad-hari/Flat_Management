#!/bin/sh
# wait-for-db-and-migrate.sh

set -e

: "${DB_HOST:?DB_HOST environment variable not set}"
: "${DB_PORT:=3306}"

echo "Menunggu database $DB_HOST:$DB_PORT siap..."

while ! nc -z $DB_HOST $DB_PORT; do
  echo "Database belum siap, menunggu 3 detik..."
  sleep 3
done

echo "Database siap! Menjalankan EF Core migrations..."

# Jalankan migrations untuk AppDbContext
dotnet ef migrations add InitialMigrate --context AppDbContext -s ../MyApp.Web || true
dotnet ef database update --context AppDbContext -s ../MyApp.Web -- --environment Development

# Jalankan migrations untuk ApplicationDbContext (Identity)
dotnet ef migrations add InitIdentity --context ApplicationDbContext -s ../MyApp.Web || true
dotnet ef database update --context ApplicationDbContext -s ../MyApp.Web -- --environment Development

echo "Migrations selesai. Menjalankan web app..."
exec dotnet YourWebApp.dll
