# FurniqApi

ASP.NET Core 9 Web API + EF Core (Pomelo MySQL/MariaDB) + Scalar, a `furniq` adatbázishoz.

## Indítás
1. Importáld a `furniq_1.sql`-t phpMyAdminban (adatbázis neve: `furniq`).
2. Ha kell, írd át a kapcsolatot az `appsettings.json`-ban (`ConnectionStrings:Default`).
3. `dotnet restore` majd `dotnet run`
4. Scalar felület: `https://localhost:<port>/scalar/v1`
