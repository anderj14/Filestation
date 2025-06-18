# File API with JWT Authentication

API para gestión segura de archivos con autenticación JWT, rate limiting y almacenamiento en SQL Server.

## 📌 Características Principales

- ✅ **Autenticación JWT** con Identity Framework
- 📁 **Gestión de archivos** (subir/descargar/eliminar)
- ⏱ **Rate Limiting** (5 solicitudes/minuto en login)
- 🛡 **Protección CSRF** (con AntiForgery)
- 🗃 **Relación usuario-archivos** (un usuario solo accede a sus archivos)

## 🚀 Requisitos Técnicos

- .NET 7.0 SDK
- SQL Server 2019+ (o Azure SQL)
- (Opcional) Azure Blob Storage para archivos grandes

## 🔧 Configuración Inicial

1. Clona el repositorio
2. Configura la base de datos en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FileDB;User Id=sa;Password=TuContraseña;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "ClaveSecretaDe64CaracteresMinimumParaJWT1234567890$$$",
    "Issuer": "https://tudominio.com",
    "Audience": "https://tudominio.com"
  }
}
```

3. Ejecuta el docker-compose

```bash
# Iniciar todos los servicios
docker-compose up -d

# Detener los servicios
docker-compose down

# Ver logs de la API
docker-compose logs -f api

# Ejecutar migraciones (después de iniciar los contenedores)
docker-compose exec api dotnet ef database update
```

4. Ejecuta las migraciones:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Inicia la API:

```bash
dotnet run
```

# 📚 Endpoints Principales

## 🔐 Autenticación

| Método | Endpoint   | Descripción                   | Requiere Auth |
|--------|------------|-------------------------------|----------------|
| POST   | /register  | Registro de nuevo usuario     | No             |
| POST   | /login     | Inicio de sesión (JWT)        | No             |

## 📁 Gestión de Archivos

| Método | Endpoint        | Descripción                     | Requiere Auth |
|--------|------------------|---------------------------------|----------------|
| POST   | /upload          | Subir archivo                   | Sí             |
| GET    | /files           | Listar archivos del usuario     | Sí             |
| GET    | /download/{id}   | Descargar archivo por ID        | Sí             |
| DELETE | /files/{id}      | Eliminar archivo                | Sí             |

# 🛠 Ejemplos de Uso

## 📌 Registro de usuario

```http
POST /register
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "P@ssw0rd123!"
}

POST /login
Content-Type: application/json
```

```http
{
  "email": "usuario@ejemplo.com",
  "password": "P@ssw0rd123!"
}
```

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "usuario@ejemplo.com"
}
```

```http
POST /upload
Authorization: Bearer <TU_JWT>
Content-Type: multipart/form-data

-- Form Data --
file: <tu_archivo.pdf>
```
