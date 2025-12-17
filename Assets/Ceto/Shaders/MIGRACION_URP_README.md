# Ceto Ocean System - Migración a URP

## Resumen de la Migración

Se han migrado los shaders principales de Ceto de Built-in Render Pipeline a **Universal Render Pipeline (URP)**.

## Archivos Creados

### Archivos HLSL (Includes compartidos)
1. **OceanShaderHeader.hlsl** - Variables globales y definiciones para URP
2. **OceanDisplacement.hlsl** - Funciones de desplazamiento de olas
3. **OceanUnderWater.hlsl** - Efectos submarinos y refracción (usa `_CameraOpaqueTexture` en lugar de GrabPass)
4. **OceanBRDF.hlsl** - Modelo de iluminación adaptado para URP

### Shaders Principales URP
1. **OceanTopSide_Opaque_URP.shader** - Superficie superior del océano (opaco)
2. **OceanTopSide_Transparent_URP.shader** - Superficie superior del océano (transparente)
3. **OceanUnderSide_Opaque_URP.shader** - Superficie inferior del océano (opaco)
4. **OceanUnderSide_Transparent_URP.shader** - Superficie inferior del océano (transparente)
5. **BlurEffectConeTaps_URP.shader** - Efecto de desenfoque

## Cambios Importantes

### 1. GrabPass → Camera Opaque Texture
- **Antes (Built-in):** `GrabPass { "Ceto_RefractionGrab" }`
- **Ahora (URP):** `_CameraOpaqueTexture`

Para que esto funcione, debes **habilitar "Opaque Texture" en tu URP Asset**:
1. Selecciona tu URP Renderer Asset
2. En el Inspector, busca "Opaque Texture"
3. Activa la casilla

### 2. Surface Shaders → Forward Rendering
Los shaders ahora usan passes explícitos:
- **ForwardLit** - Renderizado principal
- **ShadowCaster** - Proyección de sombras
- **DepthOnly** - Para profundidad

### 3. Sintaxis de Texturas
- **Antes:** `sampler2D Ceto_Reflections0;`
- **Ahora:** `TEXTURE2D(Ceto_Reflections0); SAMPLER(sampler_Ceto_Reflections0);`
- **Sampling:** `SAMPLE_TEXTURE2D(texture, sampler, uv)`

### 4. Sistema de Iluminación
- Ahora usa `GetMainLight()` de URP
- Lighting personalizado a través de `LightingOceanBRDF()`
- Soporta sombras y luces adicionales de URP

## Configuración del Proyecto

### Paso 1: Configurar URP Renderer
1. Abre tu **URP Renderer Asset** (usualmente en `Assets/Settings/`)
2. Activa **"Opaque Texture"** - CRUCIAL para refracción
3. Configura **"Depth Texture"** si usas efectos submarinos

### Paso 2: Actualizar Materiales
1. Encuentra todos los materiales de Ceto en tu proyecto
2. Cambia los shaders a las versiones URP:
   - `Ceto/OceanTopSide_Opaque` → `Ceto/URP/OceanTopSide_Opaque`
   - `Ceto/OceanTopSide_Transparent` → `Ceto/URP/OceanTopSide_Transparent`
   - `Ceto/OceanUnderSide_Opaque` → `Ceto/URP/OceanUnderSide_Opaque`
   - `Ceto/OceanUnderSide_Transparent` → `Ceto/URP/OceanUnderSide_Transparent`

### Paso 3: Verificar Scripts de Ceto
Es posible que algunos scripts de C# de Ceto necesiten ajustes para trabajar con URP:
- Búsca referencias a `RenderSettings`
- Verifica que las cámaras de reflexión funcionen correctamente
- Comprueba que los comandos de renderizado sean compatibles con URP

## Keywords de Shader

Los shaders URP mantienen los mismos keywords de Ceto:
- `CETO_REFLECTION_ON` - Activa reflexiones
- `CETO_UNDERWATER_ON` - Activa efectos submarinos
- `CETO_USE_OCEAN_DEPTHS_BUFFER` - Usa buffer de profundidad del océano
- `CETO_USE_4_SPECTRUM_GRIDS` - Usa 4 grids de espectro

## Limitaciones Conocidas

1. **Compute Shaders** - Los archivos `.compute` no fueron modificados, deberían funcionar igual
2. **Post-Processing** - Los efectos de post-procesamiento necesitarán migración si usan GrabPass
3. **Máscaras de Océano** - Verifica que los replacement shaders funcionen con URP

## Solución de Problemas

### Refracción no funciona
- Verifica que **"Opaque Texture"** esté habilitado en el URP Renderer
- Asegúrate de que el océano renderice en el queue correcto

### Océano aparece negro
- Verifica que haya una luz direccional en la escena
- Comprueba que las texturas de reflexión se estén generando correctamente

### Errores de compilación
- Asegúrate de que todos los archivos `.hlsl` estén en la carpeta de Shaders
- Verifica que las rutas de `#include` sean correctas

### Performance
- Los shaders URP deberían tener performance similar o mejor
- Si hay problemas, desactiva features como `CETO_USE_4_SPECTRUM_GRIDS`

## Archivos Originales

Los archivos originales de Built-in RP se mantienen intactos:
- `OceanTopSide_Opaque.shader`
- `OceanTopSide_Transparent.shader`
- `OceanUnderSide_Opaque.shader`
- `OceanUnderSide_Transparent.shader`
- Archivos `.cginc`

Puedes volver a Built-in RP en cualquier momento cambiando los materiales de vuelta a los shaders originales.

## Próximos Pasos Recomendados

1. **Probar en una escena de prueba** antes de aplicar en producción
2. **Revisar los parámetros de cada material** tras la conversión
3. **Ajustar settings de URP** para optimizar performance
4. **Verificar que las reflexiones funcionen** correctamente
5. **Probar en diferentes plataformas** (PC, móvil, consolas)

## Notas de Desarrollo

- Versión de Unity recomendada: 2021.3 o superior
- URP versión: 12.0 o superior
- Los shaders requieren Shader Model 3.5 o superior

---

**Migración completada el:** 16 de diciembre de 2025
**Shaders migrados:** 5 principales + utilities
**Archivos HLSL creados:** 4 archivos de include

Para soporte o preguntas adicionales, consulta la documentación de URP de Unity.
