# Authentication_Module_BE

Authentication Module with .NET CORE

migrations : dotnet ef migrations add "Init_Db" --project Authentication.Infrastructure --startup-project Authentication.API --output-dir Persistence/Migrations
dotnet ef migrations remove --project Authentication.Infrastructure --startup-project Authentication.API

 dotnet ef database update --project Authentication.Infrastructure --startup-project Authentication.API