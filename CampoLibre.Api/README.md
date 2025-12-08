🚀 CampoLibre API – Backend (.NET 9 + EF Core + SQL Server + JWT)

API RESTful para el sistema de reservas deportivas CampoLibre.

Incluye gestión de usuarios, canchas, reservas, autenticación con JWT y arquitectura limpia con DTOs.

📌 Tecnologías utilizadas

.NET 9 Web API

Entity Framework Core 9

SQL Server / LocalDB

JWT Authentication

Swagger (Swashbuckle)

DTOs + Controllers + Arquitectura por capas

⚙️ Requerimientos previos

Antes de ejecutar el proyecto asegurate de tener:

✔ .NET 9 SDK

✔ SQL Server / LocalDB

CampoLibre usa por defecto:

(localdb)\MSSQLLocalDB

▶️ Cómo ejecutar el backend

1️⃣ Restaurar paquetes (solo la primera vez)

dotnet restore

2️⃣ Aplicar migraciones (crea la base de datos)

dotnet ef database update


Esto crea la BD CampoLibreDb con tablas:

Usuarios

Canchas

Reservas

3️⃣ Ejecutar la API

dotnet run


La consola mostrará algo como:

Now listening on: http://localhost:5070

4️⃣ Abrir Swagger

👉 http://localhost:5070/swagger

Desde ahí podés probar:

POST /Auth/register

POST /Auth/login

CRUD de Usuarios

CRUD de Canchas

CRUD de Reservas

🔑 Autenticación (JWT)

Para probar endpoints protegidos:

Entrá a POST /api/Auth/login

Mandá email + password

Copiá el token recibido

En Swagger tocá Authorize

Pegá:

Bearer {tu\_token}


✔ Listo, ya podés acceder a endpoints con [Authorize].

🔧 Migraciones adicionales

Si en algún momento cambiás tus entidades:

dotnet ef migrations add NombreDeTuMigracion

dotnet ef database update

👤 Roles incluidos

El sistema maneja tres roles:

Cliente

Operador

Admin

Se definen en:

Domain/Entities/UserRole.cs

🧪 Endpoints principales (resumen)

🔹 Usuarios

POST /api/Auth/register

POST /api/Auth/login

GET /api/Usuarios

PUT /api/Usuarios/{id}

DELETE /api/Usuarios/{id}

🔹 Canchas

GET /api/Canchas

POST /api/Canchas

PUT /api/Canchas/{id}

DELETE /api/Canchas/{id}

🔹 Reservas

GET /api/Reservas

POST /api/Reservas

PUT /api/Reservas/{id}

DELETE /api/Reservas/{id}

📄 Licencia / Uso

Este proyecto es parte del Bootcamp / Trabajo Práctico de Cristian Mansilla para Dicsys Academy 2024–2025.

Puede utilizarse como referencia educativa.

🙌 Autor

Cristian Mansilla

Desarrollador Backend / Fullstack

Goya, Corrientes – Argentina

