# Sistema de Decoración - Resumen Rápido

## Archivos Creados

```
Assets/Scripts/Decoration/
├── DecorationData.cs              ✅ Estructura de datos para decoraciones
├── DecorationInventory.cs         ✅ Gestiona el inventario
├── DecorationPlacer.cs            ✅ Sistema de colocación con preview
├── DecorationObject.cs            ✅ Componente para decoraciones colocadas
├── DecorationUI.cs                ✅ Interfaz de usuario del inventario
├── DecorationManager.cs            ✅ Coordinador central
├── DecorationSystemInitializer.cs ✅ Inicialización de datos de prueba
├── DecorationAPIAdapter.cs        ✅ Integración con tu API
└── GUIA_DECORACIONES.md           ✅ Documentación completa
```

## Setup Rápido en 5 pasos

### 1. Crear estructura en la escena
```
Aquarium (main aquarium transform)
DecorationSystem (GameObject vacío con los scripts)
```

### 2. Asignar componentes a DecorationSystem
- DecorationManager
- DecorationInventory
- DecorationPlacer
- DecorationUI
- DecorationSystemInitializer

### 3. Configurar DecorationPlacer
- **aquariumParent**: Asigna el transform de Aquarium
- **placementHeight**: 0 (o la altura de tu pecera)
- **useGridSnapping**: true
- **gridSize**: 0.5

### 4. Configurar DecorationUI
- **inventoryGrid**: Asigna tu Grid Layout Group
- **inventoryItemPrefab**: Crea un prefab simple (Image + Button)
- **decorationInventory**: Asigna el componente
- **decorationPlacer**: Asigna el componente

### 5. Añadir decoraciones al inventario
- En DecorationSystemInitializer, asigna arrays de prefabs e iconos
- O llama manualmente: `inventory.AddDecoration(new DecorationData(...))`

## Flujo de ejecución

```
Usuario clickea item en UI
    ↓
OnDecorationSelected(decoration)
    ↓
decorationPlacer.StartPlacing(decoration)
    ↓
Aparece preview semi-transparente
    ↓
Usuario mueve ratón
    ↓
UpdatePreviewPosition() sigue al ratón
    ↓
Usuario clickea izquierdo
    ↓
PlaceDecoration()
    ├─ Destruye preview
    ├─ Instancia prefab real
    ├─ Agrega DecorationObject
    ├─ Lo coloca como hijo de aquarium
    └─ Consume del inventario y actualiza UI
    ↓
Usuario puede mover/rotar con ratón
```

## Controles

| Acción | Control |
|--------|---------|
| Seleccionar decoración | Click en item UI |
| Mover preview | Ratón |
| Colocar | Click izquierdo |
| Cancelar colocación | ESC |
| Mover decoración colocada | Arrastra con ratón |
| Rotar decoración | Q / E |

## Integración con tu API

Hay dos formas:

### Opción A: Datos de prueba (Rápido)
```csharp
// En DecorationSystemInitializer.Start()
decorationInventory.AddDecoration(new DecorationData(...));
```

### Opción B: Desde servidor (Recomendado)
```csharp
// Implementar en DecorationAPIAdapter
// Conectar con tu APIAdapter existente
// Cargar decoraciones cuando inicia la escena
```

## Personalización

### Cambiar tamaño del grid
En DecorationPlacer: `gridSize = 0.25` (más pequeño) o `1.0` (más grande)

### Deshabilitar snapping
En DecorationPlacer: `useGridSnapping = false`

### Cambiar altura de colocación
En DecorationPlacer: `placementHeight = 2.0` (ajusta a tu pecera)

### Añadir límite de decoraciones
En DecorationPlacer.PlaceDecoration(), añade:
```csharp
if (aquariumParent.childCount >= MAX_DECORATIONS) return;
```

## Próximos pasos

1. **Crear prefabs de decoraciones** con Mesh Renderer y Collider
2. **Crear UI de inventario** con Grid Layout Group
3. **Conectar con tu API** para cargar datos reales
4. **Implementar guardado** de estado de decoraciones
5. **Añadir efectos visuales** (partículas, sonidos)

---

¡Todo listo! Ahora integra según tus necesidades.
