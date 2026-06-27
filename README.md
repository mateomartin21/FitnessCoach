# FitnessCoach

Aplicacion web desarrollada con ASP.NET Core MVC (.NET 10) como proyecto academico para el curso de Arquitectura de Software — Tecnologico de Software.

---

## Descripcion

FitnessCoach es un entrenador personal inteligente que genera rutinas de entrenamiento personalizadas segun el objetivo fitness del usuario. Calcula calorias diarias recomendadas y aplica calentamiento y enfriamiento automatico a cada plan generado.

---

## Arquitectura


El proyecto implementa **Arquitectura Hexagonal (Ports and Adapters)** en estructura multiproyecto:FitnessCoach.Domain          # Modelos, puertos e interfaces, patrones GOF

FitnessCoach.Application     # Servicios de aplicacion y casos de uso

FitnessCoach.Infrastructure  # Adaptadores (repositorios, persistencia)

FitnessCoach.Web             # Controladores MVC, API REST, vistas Razor---

## Patrones de Diseno GOF Implementados

### Strategy (Comportamiento)
Ubicacion: `FitnessCoach.Domain/Patterns/Strategy/`

Cada objetivo fitness tiene su propia estrategia de generacion de rutina, intercambiable sin modificar el servicio principal.

- `IEstrategiaRutina` — interfaz del patron
- `EstrategiaPerderPeso` — rutina Full Body 3 dias con cardio
- `EstrategiaGanarMusculo` — rutina Hipertrofia 5 dias
- `EstrategiaRecomposicion` — rutina Torso/Pierna 4 dias

### Decorator (Estructural)
Ubicacion: `FitnessCoach.Domain/Patterns/Decorator/`

Agrega calentamiento y enfriamiento automatico a cualquier rutina sin modificar las estrategias base.

- `RutinaDecorator` — decorador abstracto base
- `RutinaConCalentamiento` — inyecta calentamiento al inicio de cada dia
- `RutinaConEnfriamiento` — agrega estiramientos al final de cada dia

---

## Ramas del Repositorio

| Rama | Descripcion |
|------|-------------|
| `mvc-inicial` | Estado inicial: MVC puro con modelos, vistas y controladores basicos |
| `api-layer` | Incorporacion de capa API REST con Scalar UI y endpoints documentados |
| `master` | Version actual: arquitectura hexagonal multiproyecto + patrones GOF |

---

## Tecnologias

- ASP.NET Core MVC (.NET 10)
- Razor Views + Bootstrap 5
- Microsoft.AspNetCore.OpenApi + Scalar UI
- Arquitectura Hexagonal multiproyecto
- Patrones GOF: Strategy + Decorator

---

## Funcionalidades

- Configuracion de perfil de usuario (nombre, edad, peso, estatura, objetivo)
- Calculo de calorias diarias con formula Mifflin-St Jeor
- Generacion de rutinas personalizadas segun objetivo
- Calentamiento y enfriamiento automatico en cada sesion
- API REST documentada con Scalar UI
- Interfaz oscura con mascota Lobo Coach

---

## Clausula de Uso de IA

Este proyecto fue desarrollado con asistencia de herramientas de inteligencia artificial (Claude - Anthropic) durante el proceso de aprendizaje. El uso de IA se limito a apoyo en la resolucion de errores de compilacion, generacion de codigo base para patrones de diseno, y orientacion sobre decisiones arquitectonicas. Todo el codigo fue revisado, comprendido e integrado manualmente por el autor. El diseno arquitectonico, las decisiones documentadas en los ADRs y la estructura del proyecto son responsabilidad del estudiante.

---

*Tecnologico de Software — Desarrollo de Software y Negocios Digitales*
*Arquitectura de Software — Dr. Pedrozo — 2026*
