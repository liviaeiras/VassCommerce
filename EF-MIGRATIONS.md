# EF Core database setup

The API is configured for SQL Server using `ConnectionStrings:DefaultConnection`.
After installing the EF CLI (`dotnet tool install --global dotnet-ef`), create and
apply migrations from the project directory:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Startup currently calls `EnsureCreated` so a fresh development database works
without the CLI; production deployments should use the migration commands.
