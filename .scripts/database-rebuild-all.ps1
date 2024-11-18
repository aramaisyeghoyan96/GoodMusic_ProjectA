# För att göra .ps1-filen körbar, kör följande kommando i PowerShell (Behöver bara köras första gången):
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
# För att köra filen skriv:
#.\database-rebuild-all.ps1

dotnet ef database drop -f -c SqlServerDbContext -p ../DbContext -s ../DbContext

Remove-Item -Recurse -Force ../DbContext/Migrations

dotnet ef migrations add miInitial -c SqlServerDbContext -p ../DbContext -s ../DbContext -o ../DbContext/Migrations/SqlServerDbContext

dotnet ef database update -c SqlServerDbContext -p ../DbContext -s ../DbContext

