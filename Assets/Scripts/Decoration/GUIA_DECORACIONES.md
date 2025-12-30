# Sistema de Decoración - Guía de Implementación

## Descripción
Este sistema permite a los jugadores colocar decoraciones dentro de la pecera (aquarium) de forma similar a Unity o Blender. Las decoraciones se pueden mover, rotar y eliminar una vez colocadas.

## Componentes

### 1. **DecorationData.cs**
Define la estructura de una decoración con:
- `id`: Identificador único
- `name`: Nombre de la decoración
- `description`: Descripción
- `icon`: Sprite del icono para la UI
- `prefab`: GameObject prefab a instanciar
- `quantity`: Cantidad en el inventario

### 2. **DecorationInventory.cs**
Gestiona el inventario de decoraciones:
- `GetDecorations()`: Retorna todas las decoraciones
- `GetDecorationById(id)`: Busca una decoración por ID
- `AddDecoration(decoration)`: Añade una decoración al inventario
- `RemoveDecoration(id)`: Consume una decoración del inventario
- `HasDecoration(id)`: Verifica disponibilidad

### 3. **DecorationPlacer.cs**
Controla el sistema de colocación:
- Muestra un preview semi-transparente mientras mueves el ratón
- Permite snapping a grid (opcional)
- Click izquierdo coloca la decoración
- Esc cancela la colocación

**Variables ajustables:**
- `aquariumParent`: Transform donde se colocan las decoraciones
- `placementHeight`: Altura de colocación
- `useGridSnapping`: Activar/desactivar snapping a grid
- `gridSize`: Tamaño del grid (0.5 unidades por defecto)

### 4. **DecorationObject.cs**
Componente añadido a cada decoración colocada que permite:
- Arrastrar con el ratón para mover
- Teclas Q/E para rotar
- Método `Delete()` para eliminar

### 5. **DecorationUI.cs**
Gestiona la UI del inventario:
- Muestra los items disponibles
- Click en item inicia la colocación
- Actualiza automáticamente después de colocar

### 6. **DecorationManager.cs**
Coordinador central del sistema.

## Configuración en Unity

### Setup recomendado:

1. **Crea un GameObject vacío llamado "DecorationSystem"**

2. **Añade estos componentes:**
   - DecorationManager (Script)
   - DecorationInventory (Script)
   - DecorationPlacer (Script)
   - DecorationUI (Script)

3. **En DecorationPlacer, asigna:**
   - `aquariumParent`: El transform de la pecera
   - `placementHeight`: La altura Y donde se colocan objetos
   - `useGridSnapping`: true/false según prefieran
   - `gridSize`: 0.5 (o el valor deseado)

4. **En DecorationUI, asigna:**
   - `inventoryGrid`: Un Grid Layout Group en tu Canvas
   - `inventoryItemPrefab`: Un prefab con Image + Button
   - `decorationInventory`: El DecorationInventory del sistema
   - `decorationPlacer`: El DecorationPlacer del sistema

5. **Crea prefabs de decoraciones:**
   - Asegúrate que tengan Mesh Renderer
   - Agrega colisionador (Collider) para interacción
   - Coloca los prefabs en `Assets/Resources/Decorations/`

6. **Asigna decoraciones al inventario:**
   ```csharp
   // En un script de inicialización
   var inventory = GetComponent<DecorationInventory>();
   inventory.AddDecoration(new DecorationData(
       id: 1,
       name: "Planta",
       description: "Una planta acuática",
       icon: plantIcon,
       prefab: plantPrefab,
       quantity: 3
   ));
   ```

## Flujo de Uso

1. **Usuario clickea en un item del inventario**
   ↓
2. **DecorationUI llama a `decorationPlacer.StartPlacing()`**
   ↓
3. **Se crea un preview semi-transparente**
   ↓
4. **Usuario mueve el ratón para posicionar**
   ↓
5. **Click izquierdo para colocar**
   ↓
6. **Se consume del inventario y se actualiza la UI**
   ↓
7. **Se puede mover/rotar con el ratón**

## Controles de Usuario

- **Mover cámara/seleccionar**: Ratón
- **Colocar decoración**: Click izquierdo
- **Cancelar colocación**: ESC
- **Mover decoración colocada**: Arrastra con ratón
- **Rotar decoración**: Q/E mientras se arrastra

## Próximas mejoras sugeridas

- [ ] Guardar/cargar estado de decoraciones
- [ ] Sistema de undo/redo
- [ ] Rotación con controles más avanzados (rueda ratón)
- [ ] Validación de colisiones
- [ ] Efectos visuales al colocar
- [ ] Sistema de presupuesto/límite de decoraciones
- [ ] Animaciones suaves de colocación
- [ ] Soporte para escalar decoraciones

## Posibles errores y soluciones

**Error: "NullReferenceException en DecorationPlacer"**
- Asegúrate de haber asignado `aquariumParent` en el inspector

**Las decoraciones no aparecen en la UI**
- Verifica que `inventoryItemPrefab` tenga un componente Button
- Comprueba que `inventoryGrid` esté asignado

**Las decoraciones se pierden al cambiar de escena**
- Necesitas implementar un sistema de persistencia
- O usar DontDestroyOnLoad en un manager

---

¡Espero que te ayude! Ajusta los valores según tus necesidades.
