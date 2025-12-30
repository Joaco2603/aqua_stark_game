# Checklist de Implementación - Sistema de Decoración

## ✅ Código creado
- [x] DecorationData.cs - Estructura de datos
- [x] DecorationInventory.cs - Gestor de inventario
- [x] DecorationPlacer.cs - Sistema de colocación
- [x] DecorationObject.cs - Interacción con decoraciones colocadas
- [x] DecorationUI.cs - Interfaz del inventario
- [x] DecorationManager.cs - Coordinador principal
- [x] DecorationSystemInitializer.cs - Inicialización de datos
- [x] DecorationAPIAdapter.cs - Integración con API
- [x] DecorationSceneSetup.cs - Setup completo

## 📋 Configuración en Unity (Haz esto después)

### Paso 1: Preparar la escena
- [ ] Abre tu escena `Assets/Scenes/Decoration.unity`
- [ ] Identifica el GameObject del acuario (donde irán las decoraciones)
- [ ] Asegúrate de que tienes un Canvas para la UI

### Paso 2: Crear GameObject del sistema
- [ ] Crea un GameObject vacío llamado "DecorationSystem"
- [ ] Colócalo en la raíz de la jerarquía de la escena

### Paso 3: Asignar componentes
- [ ] Añade el script `DecorationSceneSetup.cs` al GameObject
- [ ] En el inspector, completa las referencias:
  - [ ] aquariumTransform: Selecciona el GameObject del acuario
  - [ ] uiCanvas: Selecciona tu Canvas
  - [ ] decorationPrefabs: Crea un array con tus prefabs
  - [ ] decorationIcons: Crea un array con los iconos
  - [ ] decorationNames: (Opcional) Nombres de las decoraciones

### Paso 4: Preparar prefabs de decoraciones
Para cada decoración necesitas:
- [ ] Un prefab con Mesh Renderer
- [ ] Un Collider (Box, Mesh, etc.)
- [ ] Un icono en formato PNG o Sprite
- [ ] Colocar el prefab en `Assets/Resources/Decorations/` (recomendado)

Ejemplo de estructura de prefab:
```
MiDecoracion (GameObject)
├── Mesh Renderer (componente)
├── Box Collider (componente)
└── Material (asignado al renderer)
```

### Paso 5: Crear UI del inventario (si no la tienes)
- [ ] En el Canvas, crea un Panel para la UI
- [ ] Dentro del Panel, crea:
  - [ ] GridLayoutGroup (para organizar items)
  - [ ] ScrollRect (si tienes muchas decoraciones)
  - [ ] Crear un prefab de item:
    - Image (para mostrar icono)
    - Button (para clickear)
    - Text (para cantidad)

### Paso 6: Configuración avanzada (opcional)
- [ ] En DecorationPlacer, ajusta:
  - [ ] placementHeight: altura correcta para tu escena
  - [ ] gridSize: tamaño del grid (0.5 recomendado)
  - [ ] useGridSnapping: activar/desactivar

## 🧪 Pruebas

- [ ] Ejecuta la escena
- [ ] Verifica que aparece el inventario en la UI
- [ ] Clickea un item del inventario
- [ ] Mueve el ratón y debería ver un preview
- [ ] Clickea para colocar la decoración
- [ ] Intenta mover/rotar la decoración colocada
- [ ] Verifica que se consume del inventario

## 🐛 Si algo no funciona

**No aparece la UI del inventario**
- Verifica que uiCanvas esté asignado
- Comprueba que inventoryGrid tenga Layout Group
- Revisa la consola para errores

**Las decoraciones no se ven**
- Asegúrate de que decorationPrefabs están asignados
- Verifica que los prefabs tengan Renderer
- Comprueba que la Material está asignada correctamente

**El preview no sigue al ratón**
- Verifica que aquariumTransform está asignado
- Comprueba que la Main Camera tiene tag "MainCamera"
- Revisa que placementHeight sea correcto

**Las decoraciones desaparecen al colocar**
- Verifica que aquariumTransform es un Transform válido
- Comprueba que no hay scripts que destruyan objetos automáticamente

## 📊 Estructura final esperada

```
Decoracion.unity
├── Main Camera
├── Aquarium (Transform donde van las decoraciones)
│   └── (Las decoraciones colocadas aparecerán aquí)
├── Canvas
│   ├── DecorationUIPanel
│   │   └── InventoryGrid
│   │       └── (Items del inventario)
│   └── (Otros elementos UI)
└── DecorationSystem
    ├── DecorationSceneSetup.cs
    ├── DecorationManager.cs
    ├── DecorationInventory.cs
    ├── DecorationPlacer.cs
    └── DecorationUI.cs
```

## 🎯 Próximas características (opcional)

Después de que todo funcione, puedes añadir:

1. **Persistencia**: Guardar/cargar decoraciones
   - Implementar en DecorationAPIAdapter
   - SaveDecorationStates() / LoadDecorationStates()

2. **Validación de colisiones**: No permitir colocar sobre objetos
   - En DecorationPlacer.PlaceDecoration()

3. **Límite de decoraciones**: Máximo N decoraciones
   - En DecorationPlacer.PlaceDecoration()

4. **Efectos visuales**: Partículas al colocar
   - En DecorationPlacer.PlaceDecoration()

5. **Escalar decoraciones**: Cambiar tamaño
   - En DecorationObject.cs con teclas numéricas

6. **Menú de contexto**: Click derecho para opciones
   - Click derecho → Rotar, Eliminar, Copiar, etc.

---

**¿Necesitas ayuda con algo específico?** Revisa GUIA_DECORACIONES.md o SETUP_DECORACION_RAPIDO.md
