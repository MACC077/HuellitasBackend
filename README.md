# Huellitas — Backend API

API REST para la plataforma híbrida de adopción y trazabilidad animal en Cali, Colombia.

**Proyecto de grado** — Institución Universitaria Antonio José Camacho (UNIAJC).

Este repositorio contiene el backend de **Huellitas**, una plataforma que busca facilitar los procesos de adopción animal, apoyar el trabajo de los refugios y realizar el seguimiento de los animales dentro del proceso de adopción.

La API está desarrollada con **.NET 10**, Entity Framework Core bajo el enfoque **Database First** y SQL Server. Proporciona los servicios que consumen la aplicación móvil para adoptantes y el panel web destinado a refugios y administradores.

## Tabla de contenido

- [Tecnologías](#tecnologías)
- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Reglas de arquitectura](#reglas-de-arquitectura)
- [Requisitos previos](#requisitos-previos)
- [Configuración e instalación](#configuración-e-instalación)
- [Configuración de la base de datos](#configuración-de-la-base-de-datos)
- [Autenticación y autorización](#autenticación-y-autorización)
- [Conexión con el frontend](#conexión-con-el-frontend)
- [Convenciones de desarrollo](#convenciones-de-desarrollo)
- [Pruebas](#pruebas)
- [Equipo de desarrollo](#equipo-de-desarrollo)

## Tecnologías

| Tecnología | Propósito |
|---|---|
| C# | Lenguaje de programación |
| .NET 10 | Plataforma de desarrollo |
| ASP.NET Core Web API | Construcción de la API REST |
| Entity Framework Core | Acceso y persistencia de datos |
| SQL Server | Sistema gestor de base de datos |
| FluentValidation | Validación de solicitudes y reglas de entrada |
| JWT | Autenticación mediante tokens |
| BCrypt | Hash de contraseñas |
| Brevo | Servicio externo para el envío de correos electrónicos |
| Swagger / OpenAPI | Documentación e inspección de endpoints |
| xUnit u otras herramientas configuradas | Pruebas automatizadas, según la configuración del proyecto |

## Arquitectura

El backend sigue una **arquitectura en capas basada en principios de Clean Architecture**, simplificada para las necesidades del proyecto.

Las dependencias entre las capas se organizan de la siguiente manera:

```text
Huellitas.API
     |
     v
Huellitas.Application
     ^
     |
Huellitas.Infrastructure
```

La API utiliza Application, mientras que Infrastructure implementa las operaciones de persistencia y los servicios externos requeridos por la aplicación. Los detalles concretos de las referencias entre proyectos deben mantenerse alineados con los archivos `.csproj` de la solución.

### Responsabilidades por capa

**1. Huellitas.API**

Es el punto de entrada HTTP de la aplicación.

- Expone los endpoints mediante controladores.
- Gestiona solicitudes y respuestas HTTP.
- Aplica autenticación y autorización.
- Administra middlewares y el manejo global de excepciones.
- Configura la inyección de dependencias y Swagger.
- Obtiene los datos de identidad y los claims necesarios para los endpoints protegidos.

No debe contener reglas de negocio ni lógica de persistencia.

**2. Huellitas.Application**

Contiene los casos de uso y las reglas de negocio de la aplicación.

- Implementa la lógica de los procesos de negocio.
- Define DTOs para las solicitudes y respuestas.
- Centraliza validaciones mediante FluentValidation.
- Realiza conversiones entre entidades y DTOs mediante mapeos manuales.
- Define los contratos o abstracciones que requieren los casos de uso, según la implementación existente.

Esta capa debe evitar depender de detalles concretos de SQL Server, ASP.NET Core o proveedores externos.

**3. Huellitas.Infrastructure**

Implementa los detalles técnicos necesarios para ejecutar los casos de uso.

- Contiene el acceso a datos mediante Entity Framework Core.
- Gestiona el contexto de base de datos y las entidades generadas o configuradas desde el esquema existente.
- Implementa repositorios y servicios de infraestructura.
- Integra el envío de correos mediante Brevo.
- Implementa los mecanismos de autenticación criptográfica y generación de tokens JWT que requiere la solución.
- Contiene las configuraciones de persistencia, incluidas las conversiones de tipos de datos.

La configuración concreta de los servicios debe respetar los contratos definidos por las capas superiores.

## Estructura del proyecto

La solución se organiza de la siguiente manera:

```text
Huellitas/
├── src/
│   ├── Huellitas.API/
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── Huellitas.Application/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   ├── Mappings/
│   │   └── ...
│   │
│   └── Huellitas.Infrastructure/
│       ├── Entities/
│       ├── Configurations/
│       ├── Repositories/
│       ├── Services/
│       ├── ...
│       └── ...
│
└── tests/
    ├── Huellitas.UnitTests/
    └── Huellitas.IntegrationTests/
```

> La estructura es una referencia funcional. Los directorios mostrados deben ajustarse a los nombres y ubicaciones reales de la solución si difieren de este esquema.

## Reglas de arquitectura

Para mantener una solución organizada, mantenible y fácil de extender, se establecen las siguientes reglas:

- **Separación de responsabilidades:** cada capa debe encargarse exclusivamente de las funciones que le corresponden.
- **Lógica de negocio en Application:** los controladores deben delegar la ejecución de los casos de uso.
- **Persistencia en Infrastructure:** las operaciones de acceso a datos se implementan en esta capa.
- **Autorización en la frontera HTTP:** los endpoints protegidos deben definir los requisitos de acceso correspondientes.
- **Validación centralizada:** utilizar FluentValidation para validar los datos de entrada y aplicar las reglas que correspondan.
- **Manejo uniforme de errores:** emplear `AppException` para errores controlados de negocio, como solicitudes incorrectas, recursos inexistentes o conflictos, de acuerdo con su implementación.
- **Inyección de dependencias:** registrar las implementaciones y servicios en el punto de composición de la aplicación.
- **Independencia de infraestructura:** Application no debe depender directamente de implementaciones concretas de la base de datos o de servicios externos.

## Requisitos previos

Antes de ejecutar el backend, asegúrate de contar con:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- [SQL Server](https://www.microsoft.com/sql-server) instalado o accesible desde tu entorno.
- Una herramienta para administrar SQL Server, como SQL Server Management Studio (SSMS).
- Git para clonar y gestionar el repositorio.
- Visual Studio o Visual Studio Code con las extensiones necesarias para C# y .NET.

También necesitarás acceso a la base de datos de desarrollo y las credenciales de los servicios externos que utilice la configuración actual.

## Configuración e instalación

### 1. Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd Huellitas
```

Reemplaza `<URL_DEL_REPOSITORIO>` por la dirección real del repositorio.

### 2. Restaurar dependencias

Desde la raíz de la solución, ejecuta:

```bash
dotnet restore
```

### 3. Configurar las variables de entorno

Antes de iniciar la API, configura los parámetros de conexión a SQL Server y los servicios externos.

Los valores pueden gestionarse mediante archivos de configuración locales, User Secrets de .NET o variables de entorno, según el entorno de ejecución.

No publiques contraseñas, claves JWT, credenciales de Brevo ni otros secretos en el repositorio.

### 4. Compilar la solución

```bash
dotnet build
```

Si la compilación finaliza correctamente, puedes continuar con la ejecución.

### 5. Ejecutar la API

```bash
dotnet run --project src/Huellitas.API
```

La dirección local y el puerto dependerán de los perfiles configurados en el proyecto. Revisa `Properties/launchSettings.json` y la salida de la consola para identificar la URL efectiva.

### 6. Consultar Swagger

Una vez iniciada la API, abre la URL de Swagger que aparezca en la configuración o en la salida de ejecución.

Desde Swagger podrás consultar los endpoints documentados y probar las solicitudes disponibles. Los endpoints protegidos requerirán las credenciales o el token correspondientes.

## Configuración de la base de datos

Huellitas utiliza **SQL Server con el enfoque Database First**. La estructura existente de la base de datos es la referencia principal para las entidades y las configuraciones de persistencia.

Antes de ejecutar la aplicación:

1. Verifica que SQL Server esté disponible.
2. Confirma que la base de datos de desarrollo exista y tenga el esquema requerido.
3. Configura la cadena de conexión en el mecanismo de configuración correspondiente.
4. Comprueba que el usuario de conexión tenga los permisos necesarios.
5. Ejecuta la API y verifica que las operaciones de acceso a datos funcionen correctamente.

La cadena de conexión debe mantenerse fuera de los archivos versionados cuando contenga credenciales.

### Convenciones de persistencia

- Los nombres de las tablas en SQL Server se escriben en plural, por ejemplo, `Mascotas`.
- Los nombres de las entidades C# se escriben en singular, por ejemplo, `Mascota`.
- Los enums de C# se almacenan como `TINYINT` mediante `HasConversion<byte>()` en las configuraciones de Entity Framework Core.
- Los cambios en el esquema deben coordinarse con el responsable de la base de datos y reflejarse en las entidades y configuraciones correspondientes.

Al trabajar con Database First, cualquier actualización de entidades debe realizarse de forma controlada para evitar perder configuraciones o cambios realizados manualmente.

## Autenticación y autorización

El backend contempla autenticación basada en JWT y almacenamiento seguro de contraseñas mediante BCrypt.

De forma general, el proceso de autenticación permite identificar al usuario y emitir un token que posteriormente puede enviarse en las solicitudes a los endpoints protegidos.

El encabezado HTTP utilizado para enviar el token tiene el siguiente formato:

```http
Authorization: Bearer <TOKEN_JWT>
```

La autorización debe aplicarse según los requisitos de cada endpoint, teniendo en cuenta los roles, los claims y los permisos que correspondan al usuario autenticado.

**Importante:** estar autenticado no implica tener permiso para ejecutar todas las operaciones. Cada endpoint protegido debe validar los requisitos de acceso aplicables.

La implementación concreta de los roles, las políticas y los claims debe consultarse en la configuración actual de la API.

## Conexión con el frontend

Durante el desarrollo, el frontend móvil o web puede consumir la API mediante la URL local del backend. Si ambos proyectos se ejecutan en equipos diferentes, o si la aplicación móvil se prueba desde un celular físico, es posible que la dirección `localhost` no sea accesible desde el dispositivo cliente.

### Alternativa: Visual Studio Dev Tunnels

Puedes utilizar Dev Tunnels para exponer temporalmente la API de desarrollo a otros dispositivos.

1. Abre Visual Studio.
2. Ve a `View > Other Windows > Dev Tunnels`.
3. Crea y configura un túnel según las opciones disponibles en tu entorno.
4. Actualiza `App:PublicBaseUrl` en la configuración local de la API con la URL pública del túnel, si esa propiedad se utiliza para construir las URL que necesita el frontend.
5. Comparte la URL con los integrantes del equipo.
6. Si el túnel es privado, comparte el acceso únicamente por un canal seguro y con las personas autorizadas.

Verifica también si la API necesita configurar CORS, ya que las aplicaciones web pueden requerir una política que permita solicitudes desde el origen del frontend.

**Consideraciones de seguridad:**

- No publiques tokens de acceso al túnel en GitHub ni en archivos versionados.
- No expongas una base de datos directamente a Internet.
- Utiliza datos de prueba y credenciales de desarrollo.
- Detén el túnel cuando ya no sea necesario.
- No utilices la configuración de desarrollo como configuración de producción.

## Convenciones de desarrollo

Para mantener la consistencia del código, el equipo adopta las siguientes convenciones:

### Idioma

Los nombres de clases, variables, métodos, rutas y comentarios se escriben en español, manteniendo consistencia en todo el proyecto.

### Mapeo manual

No se utiliza AutoMapper. Las conversiones entre entidades y DTOs se realizan manualmente en la carpeta `Application/Mappings`.

Los mapeos deben ser explícitos y mantener una separación clara entre los modelos internos y los datos expuestos mediante la API.

### Excepciones controladas

Utilizar `AppException` para representar errores controlados, de acuerdo con los tipos y mecanismos de manejo definidos por la solución.

Entre los casos contemplados están:

- `BadRequest`: la solicitud no cumple las condiciones requeridas.
- `NotFound`: el recurso solicitado no existe.
- `Conflict`: la operación entra en conflicto con el estado actual de los datos.

Los errores deben traducirse a respuestas HTTP consistentes mediante el mecanismo de manejo de excepciones de la API.

### Enums y SQL Server

Los enums de C# se convierten a `byte` mediante `HasConversion<byte>()` en las configuraciones de EF Core, para mantener su correspondencia con columnas `TINYINT` en SQL Server.

### Nombres de tablas y entidades

| Elemento | Convención | Ejemplo |
|---|---|---|
| Tabla de base de datos | Plural | `Mascotas` |
| Entidad C# | Singular | `Mascota` |
| DTO | Nombre descriptivo según su propósito | `MascotaRespuestaDto` |
| Mapeo | Ubicado en `Application/Mappings` | Conversión de `Mascota` a su DTO |

## Pruebas

La solución contempla dos proyectos para pruebas automatizadas:

- **Huellitas.UnitTests:** orientado a verificar unidades de comportamiento de forma aislada.
- **Huellitas.IntegrationTests:** orientado a verificar la interacción entre componentes y dependencias integradas.

Para ejecutar las pruebas desde la raíz de la solución:

```bash
dotnet test
```

Para ejecutar un proyecto de pruebas específico:

```bash
dotnet test tests/Huellitas.UnitTests
```

```bash
dotnet test tests/Huellitas.IntegrationTests
```

La ejecución de pruebas de integración puede requerir una base de datos de prueba y una configuración de servicios adecuada.

## Usuarios de prueba y roles

Este apartado debe completarse con los usuarios de prueba habilitados para el entorno de desarrollo.

Por seguridad, no se deben incluir contraseñas reales, credenciales de producción ni tokens válidos en este documento.

| Rol | Propósito | Usuario de prueba |
|---|---|---|
| Adoptante | Consultar animales y utilizar las funcionalidades disponibles para adoptantes | Pendiente de configurar |
| Refugio | Acceder a las funcionalidades habilitadas para refugios | Pendiente de configurar |
| Administrador | Acceder a las operaciones administrativas autorizadas | Pendiente de configurar |

Los nombres de los roles y los permisos definitivos deben coincidir con los definidos en la implementación actual.

## Equipo de desarrollo

Proyecto de grado de Ingeniería de Sistemas — Institución Universitaria Antonio José Camacho.

| Responsabilidad | Integrante |
|---|---|
| Base de datos | Jean Felipe García Arias |
| Backend | Mateo Alessandro Cuellar Cossio |
| Frontend móvil y web | Laura Alejandra Pacheco Caucali |

---

**Huellitas** — Tecnología para apoyar la adopción responsable, el trabajo de los refugios y la trazabilidad animal en Cali, Colombia.
