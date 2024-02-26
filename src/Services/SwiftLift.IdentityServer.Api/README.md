EF core tools

dotnet tool restore

dotnet ef migrations add InitialUsers -c ApplicationDbContext -o Data/Migrations/usersdb

dotnet ef database update -c ApplicationDbContext --no-build

dotnet ef migrations add InitialConfiguration -c ConfigurationDbContext -o Data/Migrations/configurationdb

dotnet ef database update -c ConfigurationDbContext --no-build

dotnet ef migrations add InitialOperational -c PersistedGrantDbContext -o Data/Migrations/operationaldb

dotnet ef database update -c PersistedGrantDbContext --no-build
