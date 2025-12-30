# 🎨 Sistema de Decoración - Guía Visual

## ¿Qué es lo que tienes?

Un **sistema completo para colocar decoraciones** en tu pecera, como si fuera:
- 🎮 Un videojuego (click = colocar)
- 🏗️ Blender/Unity (mover objetos en la escena)
- 📦 Un sistema de inventario

---

## 🎯 Lo que hace:

```
                    ┌─────────────────┐
                    │   INVENTARIO    │
                    │  (En tu Canvas) │
                    └────────┬────────┘
                             │
                  ┌──────────┴──────────┐
                  │ Clickea un item     │
                  └──────────┬──────────┘
                             │
                    ┌────────▼─────────┐
                    │  PREVIEW FLOTANTE│
                    │ (Semi-transparente)
                    └────────┬─────────┘
                             │
              ┌──────────────┴──────────────┐
              │ Mueve el ratón             │
              │ (Sigue tu cursor)          │
              └──────────────┬──────────────┘
                             │
                    ┌────────▼─────────┐
                    │  CLICKEA AQUÍ    │
                    │ (Coloca objeto)  │
                    └────────┬─────────┘
                             │
        ┌────────────────────┴────────────────────┐
        │ ¡Decoración colocada en tu pecera!      │
        │ Puedes mover/rotar con el ratón         │
        │ Se consume del inventario               │
        └─────────────────────────────────────────┘
```

---

## 🎮 Controles Usuario

| Acción | Tecla |
|--------|-------|
| 🖱️ Seleccionar decoración | Click en item UI |
| 🖱️ Mover preview | Ratón |
| ✅ Colocar | Click izquierdo |
| ❌ Cancelar colocación | ESC |
| 🔄 Mover objeto colocado | Arrastra ratón |
| 🔁 Rotar | Q (izq) / E (der) |

---

## 📦 Lo que recibiste

### 10 Scripts C#
```
DecorationData              ← Estructura de datos
DecorationInventory         ← Gestor de inventario
DecorationPlacer            ← Coloca objetos con preview
DecorationObject            ← Mueve/rota objetos
DecorationUI                ← Muestra inventario
DecorationManager           ← Coordinador
DecorationSystemInitializer ← Carga datos
DecorationAPIAdapter        ← Conecta con servidor
DecorationSceneSetup        ← Setup automático
DecorationExamples          ← Ejemplos de código
```

### 5 Documentos de Guía
```
SETUP_DECORACION_RAPIDO.md      ← 5 PASOS PARA EMPEZAR
CHECKLIST_DECORACION.md         ← Lista de tareas
DIAGRAMA_FLUJO_DECORACION.md    ← Cómo funciona
REFERENCIAS_RAPIDAS_DECORACION  ← Quick reference API
INDEX_DECORACION.md             ← Índice de todo
```

---

## 🚀 Cómo Empezar (5 Pasos)

```
PASO 1: Crea un GameObject "DecorationSystem"
        └─ Clic derecho en Hierarchy > Create Empty

PASO 2: Asigna el script DecorationSceneSetup.cs
        └─ Arrastra el script al GameObject

PASO 3: Asigna en el Inspector:
        ├─ aquariumTransform  → Tu pecera
        ├─ uiCanvas          → Tu Canvas
        ├─ decorationPrefabs → Tus modelos 3D
        └─ decorationIcons   → Tus iconos PNG

PASO 4: Crea un Panel UI para el inventario
        └─ Canvas > Right click > Panel - Image

PASO 5: Presiona PLAY
        └─ ¡Debería funcionar!
```

---

## 🧠 Cómo Funciona Internamente

### Fase 1: Usuario selecciona item
```
UI → DecorationUI.OnDecorationSelected()
          ↓
   DecorationPlacer.StartPlacing()
```

### Fase 2: Modo de colocación activo
```
Update() → UpdatePreviewPosition()
  ↓
Raycast desde cámara
  ↓
Seguir ratón
  ↓
Aplicar grid snapping
```

### Fase 3: Colocación
```
Click izquierdo → PlaceDecoration()
  ↓
├─ Instanciar prefab real
├─ Agregar DecorationObject
├─ Ponerlo en la pecera
├─ Consumir del inventario
└─ Actualizar UI
```

### Fase 4: Objeto colocado
```
Ahora el objeto tiene DecorationObject
  ├─ Click+arrastre = Mover
  ├─ Q/E mientras arrastra = Rotar
  └─ Puede eliminarse
```

---

## 📊 Arquitectura Simplificada

```
                    ┌─────────────┐
                    │   Usuario   │
                    └──────┬──────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
    UI Items         Ratón en Escena    Controles (Q/E)
        │                  │                  │
        ▼                  ▼                  ▼
   DecorationUI      DecorationPlacer  DecorationObject
        │                  │                  │
        └──────────┬───────┴──────────────────┘
                   ▼
          DecorationInventory
                   │
                   ▼
         Escena (Aquarium + Decoraciones)
```

---

## 🎯 Flujo Típico de Uso

```
Usuario en escena de Decoración
          │
          ▼
Abre inventario (UI muestra decoraciones)
          │
          ▼
Clickea "Planta Acuática" en inventario
          │
          ▼
Aparece preview semi-transparente de la planta
          │
          ▼
Mueve el ratón para posicionar
          │
          ▼
Clickea para colocar
          │
          ▼
Planta instanciada en la pecera
Inventario actualizado (-1 planta)
          │
          ▼
Usuario puede:
├─ Hacer click+arrastrar para mover
├─ Presionar Q/E para rotar
└─ Volver al inventario para más
```

---

## 🔧 Personalización Fácil

### Cambiar tamaño del grid
```csharp
// En DecorationPlacer
gridSize = 0.5f;  // Actualmente
gridSize = 0.25f; // Más pequeño
gridSize = 1.0f;  // Más grande
```

### Cambiar altura de colocación
```csharp
// En DecorationPlacer
placementHeight = 0f;  // Actualmente
placementHeight = 1.5f; // Más alto
```

### Cambiar velocidad de rotación
```csharp
// En DecorationObject.Update()
transform.Rotate(0, -5f, 0); // Cambiar 5f a otro valor
```

---

## 🎨 Requisitos Mínimos

Para que funcione necesitas:
- ✅ GameObject "DecorationSystem" con scripts
- ✅ Transform de tu pecera (aquariumParent)
- ✅ Canvas para la UI
- ✅ Main Camera (con tag "MainCamera")
- ✅ Prefabs de decoraciones (con Renderer)
- ✅ Iconos PNG para cada decoración

---

## 💾 Guardando Decoraciones (Futuro)

El sistema incluye `DecorationAPIAdapter` para:
```
Usuario coloca decoraciones
           │
           ▼
SaveDecorationStates()
           │
           ▼
JSON → Servidor
           │
           ▼
Próxima sesión: LoadDecorationStates()
           │
           ▼
Decoraciones restauradas
```

---

## 🐛 Si algo no funciona...

| Problema | ¿Dónde revisar? |
|----------|-----------------|
| No aparece inventario | ¿uiCanvas está asignado? |
| Preview no sigue ratón | ¿Main Camera tiene tag correcto? |
| Decoración desaparece al colocar | ¿aquariumParent está asignado? |
| Controles Q/E no funcionan | Revisa DecorationObject.Update() |
| Error en consola | Abre CHECKLIST_DECORACION.md |

---

## 📈 Próximas Mejoras (Opcional)

```
1️⃣  Guardar/Cargar estado
2️⃣  Límite máximo de decoraciones
3️⃣  Validación de colisiones
4️⃣  Efectos visuales (partículas, sonidos)
5️⃣  Escalar objetos (+ / -)
6️⃣  Menú click derecho
```

---

## 📚 Documentación Completa

Si necesitas más detalle, tienes:

1. **SETUP_DECORACION_RAPIDO.md** - Empieza aquí
2. **DIAGRAMA_FLUJO_DECORACION.md** - Cómo funciona
3. **CHECKLIST_DECORACION.md** - Checklist de setup
4. **DecorationExamples.cs** - Ejemplos de código
5. **GUIA_DECORACIONES.md** - Referencia técnica

---

## ✨ Resumen

```
Tienes un sistema COMPLETO que:

✅ Muestra inventario en Canvas
✅ Permite colocar decoraciones con preview
✅ Aplica grid snapping automático
✅ Permite mover/rotar objetos colocados
✅ Consume del inventario automáticamente
✅ Está completamente documentado
✅ Tiene ejemplos listos
✅ Es fácil de personalizar

¿Necesitas algo más? Revisa los archivos .md
¿Necesitas códigos? Mira DecorationExamples.cs
¿Necesitas entender? Lee DIAGRAMA_FLUJO_DECORACION.md
```

---

**¡Ya puedes implementar el sistema en tu escena! 🚀**
