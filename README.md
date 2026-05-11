# EZBar Escritorio

## ⭐ NUEVO: Refactorización MVVM y Cobertura de Pruebas (Mayo de 2026)

La aplicación de escritorio ha sido refactorizada para seguir **patrones MVVM avanzados**, mejorando la testabilidad y la separación de responsabilidades:
- ✅ **Inyección de Dependencias:** Uso de `Microsoft.Extensions.DependencyInjection`.
- ✅ **MVVM Toolkit:** Implementación con `CommunityToolkit.Mvvm`.
- ✅ **Abstracción de UI:** Interfaz `IDialogService` para desacoplar la lógica de negocio de la interfaz gráfica.
- ✅ **Unit Testing:** Cobertura de código superior al **75%** en la capa de lógica y repositorios.

---

## Índice
1. [Arquitectura de la Aplicación](#1-arquitectura-de-la-aplicación)
2. [Instalación y Configuración](#2-instalación-y-configuración)
3. [Funcionamiento](#3-funcionamiento)
4. [Estado del Proyecto](#4-estado-del-proyecto)
5. [Autores](#5-autores)

---

## 1. Arquitectura de la Aplicación

**EZBar Escritorio** es el módulo de gestión administrativa del ecosistema EZBar, desarrollado en **WPF (Windows Presentation Foundation)** con **.NET 9**. Su función principal es permitir al personal administrativo y gerencial supervisar pedidos y pagos en tiempo real.

### Componentes Clave:
- **ViewModels:** Gestionan la lógica de presentación y el estado de la UI de forma reactiva.
- **Repositories:** Capa de abstracción para la comunicación con la API externa.
- **Services:** Servicios especializados para exportación a Excel (`ClosedXML`) y gestión de diálogos.
- **Infrastructure:** Manejo de red mediante `HttpClient` con interceptores para autenticación y compatibilidad con túneles Ngrok.

---

## 2. Instalación y Configuración

### Requisitos Previos

Para compilar y ejecutar la aplicación de escritorio, necesitas:
- **Visual Studio 2022** (versión 17.8 o superior)
- **.NET 9 SDK**
- Conexión activa a la API de EZBar (local o mediante Ngrok)

### Clonar el Repositorio

```bash
git clone https://github.com/Monti1751/EZBar-escritorio.git
cd EZBar-escritorio
```

### Configuración

La aplicación busca la configuración en `appsettings.json`. Asegúrate de configurar la URL base de la API:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://tu-url-ngrok.ngrok-free.app",
    "Username": "admin",
    "Password": "password"
  }
}
```

### Ejecución de Pruebas

Para verificar la integridad del código y ver el reporte de cobertura:

```bash
dotnet test /p:CollectCoverage=true
```

---

## 3. Funcionamiento

La aplicación actúa como un panel de control centralizado que se sincroniza automáticamente con el backend.

### Funcionalidades Principales:
- **Monitorización en Tiempo Real:** Seguimiento de pedidos activos y su estado (Pendiente, Listo, etc.).
- **Gestión de Pagos:** Procesamiento de cobros, cálculo de cambios y registro de métodos de pago.
- **Filtrado Avanzado:** Búsqueda por mesa, zona o fecha específica de pago.
- **Exportación de Datos:** Generación de informes en formato Excel (.xlsx) para auditoría y contabilidad.
- **Gestión de Errores:** Sistema de reintento de conexión automática y notificaciones visuales de estado de red.

---

## 4. Estado del Proyecto

EZBar Escritorio se encuentra actualmente en **fase Beta**. 

### Logros Actuales:
- ✅ Interfaz de usuario intuitiva y moderna (Dark Mode por defecto).
- ✅ Sincronización bidireccional estable con el Backend Java/Node.js.
- ✅ Lógica de negocio protegida por una suite completa de pruebas unitarias.
- ✅ Exportación de informes totalmente funcional.

### Próximos Pasos:
- Implementación de gráficas estadísticas de ventas mensuales.
- Gestión avanzada de usuarios y roles desde el escritorio.
- Sistema de impresión de tickets físicos.

---

## 5. Autores

Este módulo forma parte del proyecto completo **EZBar**, desarrollado por:

- **Francisco Montesinos**   
  - GitHub: [Monti1751](https://github.com/Monti1751)
- **Miguel Tomás**    
  - GitHub: [ismigue23](https://github.com/ismigue23)
- **Miguel Jiménez**  
  - GitHub: [MiguelJimenezSerrano](https://github.com/MiguelJimenezSerrano)
- **Miguel Duque**  
  - GitHub: [El-Mig](https://github.com/El-Mig)

---
> Para más información sobre el ecosistema completo (App móvil y Backend), consulta los repositorios hermanos en la organización.
