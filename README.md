# 🏘️ RealEstateApp — Plataforma Inmobiliaria Integral

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512bd4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-v4-38bdf8?logo=tailwind-css)](https://tailwindcss.com/)
[![Testcontainers](https://img.shields.io/badge/Testcontainers-SQL%20Server-16a085?logo=docker)](https://testcontainers.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-6b7280)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)

**RealEstateApp** es una plataforma web full-stack que conecta a clientes, agentes inmobiliarios y administradores en un ecosistema completo de gestión de propiedades. Construida sobre **.NET 9** con **Clean Architecture**, ofrece desde la publicación de inmuebles con múltiples imágenes y mejoras, hasta un sistema de ofertas, mensajería en tiempo real y un dashboard administrativo con indicadores clave.

---

## The Dream Team

| Desarrollador | Rol | Matrícula | GitHub |
| :--- | :--- | :--- | :--- |
| **Angel Gonzalez Muñoz** | **Lead Developer** | 2025-1122 | [xNeuNoRo](https://github.com/xNeuNoRo) |
| **Isaias Jose Morillo F.** | Software Developer | 2025-1242 | [IsaiasMorillo](https://github.com/IsaiasMorillo) |
| **Engel Orlando Acosta D.** | Software Developer | 2025-0037 | [notengel](https://github.com/notengel) |

### Profesor
*   **Leonardo Enrique Tavarez Betances** — Project Master / Esclavizador

---

## 🌟 Funcionalidades Destacadas

*   **🔐 Seguridad por Roles:** RBAC estricto con 4 roles (Admin, Agent, Client, Developer), autorización por endpoint y filtros de sesión.
*   **🏠 Publicación de Propiedades:** Registro completo con título, descripción, precio, tamaño, habitaciones, baños, tipo, venta, hasta 4 imágenes y mejoras seleccionables.
*   **🔍 Búsqueda Avanzada:** Filtros simultáneos por precio (rango), tipo, habitaciones, baños y código de 6 dígitos con paginación.
*   **⭐ Favoritos & Ofertas:** Los clientes pueden marcar propiedades favoritas y enviar ofertas de compra/alquiler con estado de seguimiento.
*   **💬 Mensajería Integrada:** Sistema de conversaciones entre agentes y clientes con notificaciones y SweetAlert2.
*   **📊 Dashboard Administrativo:** Indicadores de propiedades disponibles/vendidas, agentes, clientes y desarrolladores activos/inactivos.
*   **📋 Catálogos CRUD:** Mantenimiento completo de tipos de propiedad, tipos de venta y mejoras, con conteo de propiedades asociadas.
*   **🧪 145+ Tests de Integración:** Suite completa con Testcontainers SQL Server real, cubriendo 100% de los endpoints de la API.

---

## 🧩 Módulos del Sistema

| Módulo | Descripción |
| :--- | :--- |
| **Auth & Account** | Registro de clientes, inicio de sesión, recuperación de contraseña, activación por correo |
| **Property Catalog** | Publicación, edición, eliminación y búsqueda de propiedades con filtros |
| **Agent Management** | Panel de agente, perfil, propiedades propias, ofertas recibidas |
| **Client Portal** | Dashboard personal, favoritos, ofertas enviadas, perfil |
| **Admin Dashboard** | KPIs del sistema, gestión de agentes, admins, desarrolladores |
| **Catalog CRUD** | Tipos de propiedad, tipos de venta, mejoras (con conteo de propiedades) |
| **Messaging** | Conversaciones agente-cliente con historial y notificaciones |
| **Offer System** | Envío, recepción y seguimiento de ofertas de compra/alquiler |

---

## 🏗️ Arquitectura

1.  **`RealEstateApp.Domain`**: Entidades puras (`Property`, `AppUser`, `Offer`), Enums, Value Objects y contratos de repositorio. Sin dependencias externas.
2.  **`RealEstateApp.Application`**: Casos de uso (servicios), DTOs, ViewModels, validaciones con **FluentValidation** y perfiles de **AutoMapper**.
3.  **`RealEstateApp.Infrastructure.Persistence`**: EF Core (Code First) con SQL Server, repositorios genéricos específicos y migraciones.
4.  **`RealEstateApp.Infrastructure.Identity`**: ASP.NET Core Identity para autenticación, autorización y gestión de usuarios.
5.  **`RealEstateApp.Infrastructure.Shared`**: Servicios transversales — MailKit para correos electrónicos con plantillas Razor.
6.  **`RealEstateApp.Api`**: API RESTful con 6 controladores (Account, Properties, Agents, PropertyType, SaleType, Improvement).
7.  **`RealEstateApp.WebApp`**: Capa de presentación MVC con Razor Views, componentes parciales y **Tailwind CSS v4**.

---

## 🛠️ Stack Tecnológico

*   **Lenguaje:** C# 13 / .NET 9
*   **Base de Datos:** SQL Server + Entity Framework Core 9 (Code First)
*   **Frontend:** ASP.NET Core MVC + Razor Views + Tailwind CSS v4 + Lucide Icons
*   **Autenticación:** ASP.NET Core Identity + JWT (API) + Cookies (WebApp)
*   **Validación:** FluentValidation + FluentValidation.AspNetCore
*   **Mapeo:** AutoMapper 16
*   **Notificaciones:** MailKit + SweetAlert2
*   **Testing:** xUnit + Testcontainers.MsSql + FluentAssertions

---

## ⚙️ Configuración Rápida

1.  **Base de Datos:** Configura `ConnectionStrings:RealEstateDb` en `appsettings.Development.json`.
2.  **Migraciones:**
    ```bash
    dotnet ef database update --project src/RealEstateApp.Infrastructure.Persistence --startup-project app/RealEstateApp.WebApp
    dotnet ef database update --context IdentityContext --project src/RealEstateApp.Infrastructure.Identity --startup-project app/RealEstateApp.WebApp
    ```
3.  **Ejecutar:**
    ```bash
    dotnet run --project app/RealEstateApp.WebApp
    ```
4.  **Tests (requiere Docker):**
    ```bash
    dotnet test tests/RealEstateApp.Api.Tests
    ```

### Credenciales por Defecto (Dev)

| Usuario | Rol | Password |
| :--- | :--- | :--- |
| `admin` | Admin | Admin123! |
| `developer` | Developer | Developer123! |
| `agente` | Agent | Agente123! |

---

*Instituto Tecnológico de las Américas (ITLA) — Proyecto Universitario 2026*
