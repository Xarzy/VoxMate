# 🧠 TaskNotes – Asistente con Interfaz Natural en .NET MAUI

## 📌 Descripción del proyecto
**TaskNotes** es una aplicación multiplataforma desarrollada en **.NET MAUI (.NET 10)** que integra una **interfaz natural basada en voz**.  
La aplicación permite interactuar con un asistente conversacional capaz de reconocer comandos hablados, responder mediante síntesis de voz y realizar distintas tareas como cálculos, conversiones, generación de números aleatorios y respuestas dinámicas.

El objetivo del proyecto es aplicar los conceptos de **Interfaces Naturales** vistos en clase, combinándolos con una interfaz cuidada y navegación básica.

---

## 🛠️ Tecnologías utilizadas
- .NET 10
- .NET MAUI
- C#
- MVVM
- Speech-to-Text
- Text-to-Speech
- Regex
- NavigationPage

---

## 📱 Plataformas soportadas
- ✅ Android  
- ✅ Windows  

---

## 🧩 Funcionalidad principal

### 🔹 Navegación
- Uso de `NavigationPage`
- Al menos dos páginas:
  - Página principal del asistente
  - Página secundaria de ayuda/información

### 🔹 Controles utilizados
- Entry
- Button
- Label
- CollectionView
- ScrollView

---

## 🗣️ Interfaz natural implementada – Voz

La aplicación utiliza **voz como interfaz natural principal**, incluyendo:

### ✔ Reconocimiento de voz (Speech-to-Text)
- Transcripción de la voz del usuario a texto.
- Procesamiento del comando al finalizar la grabación.

### ✔ Síntesis de voz (Text-to-Speech)
- Respuestas habladas del asistente.
- Variación automática de diálogos para una interacción más natural.

### ✔ Comandos por voz simples
Ejemplos:
- `Hola`
- `Me llamo Carlos`
- `Dame un número aleatorio entre 1 y 50`
- `Convierte 3 km a metros`
- `¿Cuánto es el 20% de 80?`
- `Cuéntame un chiste`
- `¿Qué puedes hacer?`

---

## 🤖 Asistente conversacional
El asistente:
- Tiene nombre propio
- Recuerda el nombre del usuario durante la sesión
- Responde con frases variables
- Cuenta bromas genéricas
- Interpreta lenguaje natural básico mediante expresiones regulares

---

## ▶️ Instrucciones para ejecutar el proyecto

### Requisitos
- Visual Studio 2022 o superior
- Workload .NET MAUI instalado
- SDK .NET 10
- Emulador Android o Windows

### Pasos
1. Clonar el repositorio o descomprimir el proyecto.
2. Abrir la solución en Visual Studio.
3. Seleccionar la plataforma (Android o Windows).
4. Ejecutar el proyecto (`F5`).
5. Conceder permisos de micrófono cuando se soliciten.
6. Interactuar con el asistente mediante voz o texto.

---

## 📅 Información académica
- **Asignatura**: Interfaces Naturales  
- **Tema**: Tema 5 – Interfaces Naturales en .NET MAUI  
- **Fecha de entrega**: 08/01/2026  
- **Tipo de proyecto**: Aplicación libre con interfaz natural  

---

## ✅ Cumplimiento de requisitos
- ✔ Desarrollado en .NET MAUI
- ✔ Ejecutable en Android y Windows
- ✔ Navegación básica
- ✔ Múltiples páginas
- ✔ Uso de controles comunes
- ✔ Interfaz natural basada en voz
- ✔ README documentado
