# 📊 Diagrama de Flujo del Sistema de Decoración

## Flujo Principal: Colocar una Decoración

```
┌─────────────────────────────────────────────────────────────┐
│ USUARIO CLICKEA ITEM EN INVENTARIO (UI)                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationUI.OnDecorationSelected(decoration)               │
│ └─ Verifica que hay cantidad disponible                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationPlacer.StartPlacing(decoration)                   │
│ ├─ Crea preview del prefab                                  │
│ ├─ Lo hace semi-transparente (alpha = 0.5)                 │
│ └─ Activa modo de colocación (isPlacing = true)            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ USUARIO MUEVE EL RATÓN (cada frame)                         │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationPlacer.UpdatePreviewPosition()                    │
│ ├─ Raycast desde cámara al plano del suelo                 │
│ ├─ Aplica grid snapping (si está activado)                 │
│ └─ Actualiza posición del preview                           │
└──────────────────────┬──────────────────────────────────────┘
                       │
            ┌──────────┴──────────┐
            │   (Continúa loop)   │
            └──────────────────────┘
            (Mientras mueve ratón)
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ USUARIO CLICKEA MOUSE BUTTON LEFT (O Presiona ESC)         │
└──────────────────────┬──────────────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
   Click IZQ     Presiona ESC     Otra tecla
        │              │              │
        ▼              ▼              ▼
   COLOCAR      CANCELAR          IGNORAR
        │              │              │
        └──────┬───────┘──────────────┘
               ▼
┌─────────────────────────────────────────────────────────────┐
│ ¿ESC presionado?                                            │
└──────────────────────┬──────────────────────────────────────┘
       SÍ ─────────────┼────────────── NO (Click izq)
       │               │
       ▼               ▼
┌──────────────┐  ┌──────────────────────────────────────────┐
│ CANCELAR     │  │ DecorationPlacer.PlaceDecoration()       │
│ Destroy(     │  │ ├─ Hace opaca la decoración (alpha=1)  │
│   preview)   │  │ ├─ Restaura materiales originales       │
│              │  │ ├─ Asigna como hijo de aquariumParent   │
│ isPlacing    │  │ ├─ Agrega DecorationObject              │
│ = false      │  │ └─ Consume del inventario               │
└──────────────┘  └────────────┬──────────────────────────────┘
                               │
                               ▼
                  ┌────────────────────────────┐
                  │ DecorationInventory.       │
                  │ RemoveDecoration(id)       │
                  │ └─ Reduce quantity         │
                  └────────────┬───────────────┘
                               │
                               ▼
                  ┌────────────────────────────┐
                  │ DecorationUI.              │
                  │ RefreshInventoryUI()       │
                  │ └─ Actualiza cantidad      │
                  └────────────┬───────────────┘
                               │
                               ▼
                  ┌────────────────────────────┐
                  │ ✅ DECORACIÓN COLOCADA     │
                  │ Y ACTIVA EN LA ESCENA      │
                  └────────────────────────────┘
```

---

## Flujo de Interacción: Mover/Rotar Decoración

```
┌─────────────────────────────────────────────────────────────┐
│ USUARIO CLICKEA DECORACIÓN COLOCADA (OnMouseDown)           │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationObject.OnMouseDown()                              │
│ ├─ isDragging = true                                        │
│ ├─ Crea plano de arrastre                                  │
│ └─ Calcula offset entre ratón y objeto                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ USUARIO ARRASTRA EL RATÓN (mientras mantiene botón)        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationObject.OnMouseDrag() + Update()                   │
│ ├─ Raycast hacia el plano                                  │
│ ├─ Calcula nueva posición con offset                       │
│ ├─ Actualiza transform.position                            │
│ │                                                           │
│ └─ Mientras está presionado:                               │
│    ├─ Si Input.GetKey(Q) → Rotar -5 grados                 │
│    └─ Si Input.GetKey(E) → Rotar +5 grados                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ USUARIO SUELTA EL BOTÓN DEL RATÓN (OnMouseUp)              │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ DecorationObject.OnMouseUp()                                │
│ └─ isDragging = false                                       │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────┐
        │ ✅ DECORACIÓN EN NUEVA POSICIÓN │
        └──────────────────────────────┘
```

---

## Estructura de Clases

```
DecorationData
├── id: int
├── name: string
├── description: string
├── icon: Sprite
├── prefab: GameObject
└── quantity: int

DecorationInventory
├── decorations: List<DecorationData>
├── AddDecoration()
├── RemoveDecoration()
├── GetDecorationById()
├── HasDecoration()
└── GetDecorations()

DecorationPlacer
├── aquariumParent: Transform
├── placementHeight: float
├── useGridSnapping: bool
├── gridSize: float
├── currentPreview: GameObject
├── isPlacing: bool
├── selectedDecorationId: int
├── StartPlacing()
├── UpdatePreviewPosition()
├── PlaceDecoration()
└── CancelPlacement()

DecorationObject
├── offset: Vector3
├── isDragging: bool
├── dragPlane: Plane
├── OnMouseDown()
├── OnMouseDrag()
├── OnMouseUp()
├── Update() [Rotación Q/E]
└── Delete()

DecorationUI
├── inventoryGrid: Transform
├── inventoryItemPrefab: GameObject
├── decorationInventory: DecorationInventory
├── decorationPlacer: DecorationPlacer
├── RefreshInventoryUI()
├── CreateInventoryItem()
└── OnDecorationSelected()

DecorationManager
├── decorationInventory: DecorationInventory
├── decorationPlacer: DecorationPlacer
├── decorationUI: DecorationUI
└── InitializeWithTestData()
```

---

## Flujo de Datos

```
API/Servidor
    ▲
    │
    └─── DecorationAPIAdapter ───┐
                                  │
                                  ▼
                        DecorationInventory
                         (Datos del juego)
                                  │
                    ┌─────────────┴──────────────┐
                    │                            │
                    ▼                            ▼
            DecorationPlacer             DecorationUI
           (Coloca objetos)           (Muestra inventario)
                    │                            │
                    └──────────┬─────────────────┘
                               │
                               ▼
                        DecorationObject
                    (Interacción en escena)
```

---

## Ciclo de Vida de una Decoración

```
1. CREACIÓN
   └─ AddDecoration() → Entra al inventario

2. VISUALIZACIÓN
   └─ DecorationUI crea item en Canvas

3. SELECCIÓN
   └─ Usuario clickea item → OnDecorationSelected()

4. COLOCACIÓN
   ├─ StartPlacing() → Crea preview
   ├─ UpdatePreviewPosition() → Sigue ratón
   └─ PlaceDecoration() → Instancia objeto real

5. INTERACCIÓN
   ├─ OnMouseDown() → Comienza arrastre
   ├─ OnMouseDrag() → Actualiza posición
   └─ OnMouseUp() → Termina arrastre

6. ELIMINACIÓN (Opcional)
   └─ Delete() → Destroy(gameObject)
```

---

## Requisitos por Componente

```
DecorationPlacer requiere:
├─ Main Camera (con tag "MainCamera")
└─ aquariumParent (Transform asignado)

DecorationUI requiere:
├─ Canvas
├─ inventoryGrid (con GridLayoutGroup)
└─ inventoryItemPrefab (con Button)

DecorationObject requiere:
└─ Collider (para detectar clicks)

Prefabs de Decoración requieren:
├─ Renderer (Mesh o Sprite)
└─ Material asignado
```

---

## Estados y Transiciones

```
┌─────────────────┐
│  NO_PLACING     │ (Estado inicial)
└────────┬────────┘
         │ StartPlacing()
         ▼
┌─────────────────┐
│  PLACING_PREVIEW│ (Mostrando preview)
└────────┬────────┘
         │
    ┌────┴────┐
    │          │
ESC │          │ Click Izq
    │          │
    ▼          ▼
 CANCELADO   PLACED
    │          │
    └────┬─────┘
         │ vuelve a NO_PLACING
         ▼
┌─────────────────┐
│  NO_PLACING     │
└─────────────────┘
```
