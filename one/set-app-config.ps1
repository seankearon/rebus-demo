# Sets the private configuration values used by the two Rebus demo projects.
# Note that the SQL Serer connection is only used by the Saga demo.
# Docs here: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2

cd $PSScriptRoot
dotnet user-secrets set "PrivateConfig:SBConnectionString" "<Your Azure Service Bus connection string here>"
dotnet user-secrets set "PrivateConfig:MSSqlConnectionString" "<Your SQL Server connection string here>"
