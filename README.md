## ⭐ NUEVO: Refactorización y Seguridad (Mayo de 2026)

La aplicación ha sido sometida a un proceso de **Hardening de Seguridad** y refactorización MVVM:
- ✅ **Seguridad de Credenciales:** Eliminación de secretos hardcodeados; ahora se utiliza un sistema de Login dinámico.
- ✅ **Cifrado de Red:** Implementación obligatoria de **HTTPS** para servidores externos y soporte seguro para `localhost`.
- ✅ **Protección de Datos:** Validación estricta de rutas de exportación para prevenir ataques de *Path Traversal*.
- ✅ **MVVM Avanzado:** Uso de `CommunityToolkit.Mvvm` e Inyección de Dependencias.
- ✅ **Unit Testing:** Cobertura de código superior al **75%**.

---

## Índice
1. [Arquitectura de la Aplicación](#1-arquitectura-de-la-aplicación)
2. [Instalación y Configuración](#2-instalación-y-configuración)
3. [Seguridad](#3-seguridad)
4. [Funcionamiento](#4-funcionamiento)
5. [Estado del Proyecto](#5-estado-del-proyecto)
6. [Autores](#6-autores)

---

## 1. Arquitectura de la Aplicación

**EZBar Escritorio** es el módulo de gestión administrativa del ecosistema EZBar, desarrollado en **WPF (Windows Presentation Foundation)** con **.NET 9**. Su función principal es permitir al personal administrativo y gerencial supervisar pedidos y pagos en tiempo real.

### Componentes Clave:
- **ViewModels:** Gestionan la lógica de presentación y el estado de la UI de forma reactiva.
- **Repositories:** Capa de abstracción para la comunicación con la API externa.
- **Services:** Servicios especializados para exportación a Excel (`ClosedXML`) y gestión de diálogos.
- **Infrastructure:** Manejo de red mediante `HttpClient` con interceptores para autenticación dinámica y compatibilidad con túneles Ngrok.

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

La configuración básica se encuentra en `appsettings.json`. Por seguridad, **las credenciales no se almacenan en este archivo**:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080"
  }
}
```

Al iniciar la aplicación, se solicitará el usuario y la contraseña mediante la ventana de Login.

---

## 3. Seguridad

Se han implementado medidas de protección de nivel industrial:
- **Zero Secrets in Code:** No hay contraseñas en el código ni en archivos de configuración.
- **HTTPS Enforcement:** La aplicación rechaza automáticamente conexiones no cifradas (HTTP) a hosts externos.
- **Validación de Archivos:** El exportador de Excel solo permite extensiones `.xlsx` y bloquea el acceso a directorios del sistema (Windows/System32).
- **Control de Visibilidad:** La interfaz permite ocultar/mostrar la contraseña y configurar parámetros de red avanzados bajo demanda.

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
