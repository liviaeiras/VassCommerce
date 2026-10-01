# EF Core database setup

The API is configured for SQL Server using
`ConnectionStrings:DefaultConnection`. Configure the connection string outside
the repository. After installing the EF CLI
(`dotnet tool install --global dotnet-ef`), apply the existing migrations from
the project directory:

```powershell
dotnet ef database update
```

The API applies pending migrations at startup as well. Do not edit migrations
that have already been applied. The authentication changes did not alter the
persistent model and therefore do not require a new migration.
