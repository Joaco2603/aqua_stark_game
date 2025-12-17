# GUÍA: Configurar Agua Realista en URP

## 📋 PASOS PARA CONFIGURAR EL AGUA

### Paso 1: Configurar URP (MUY IMPORTANTE)

1. **Abre tu URP Renderer Asset**:
   - En el Project, busca: `UniversalRenderer` o `ForwardRenderer`
   - Usualmente está en: `Assets/Settings/` o `Assets/Rendering/`

2. **Habilita estas opciones**:
   - ✅ **Opaque Texture** ← CRUCIAL para refracción
   - ✅ **Depth Texture** ← CRUCIAL para efectos de profundidad

### Paso 2: Descargar/Crear Normal Map para Olas

**Opción A - Usar un Normal Map que ya tengas:**
- Busca en tus assets un normal map de agua/olas

**Opción B - Descargar uno gratis:**
1. Ve a: https://polyhaven.com/textures/water
2. Descarga cualquier "Water Normal" texture
3. Importa a Unity en `Assets/Textures/`

**Opción C - Crear uno simple:**
1. Haz clic derecho en Project → Create → Render Texture
2. Nómbralo "WaterNormalMap"
3. Por ahora usaremos el default "bump"

### Paso 3: Crear Material de Agua

1. **Crear Material**:
   - Clic derecho en Project → Create → Material
   - Nómbralo: `WaterMaterial`

2. **Asignar Shader**:
   - Selecciona el material
   - En Inspector, busca "Shader"
   - Elige: `Custom/URP/SimpleOceanWater` (más realista)
   - O: `Custom/URP/BasicWater` (más simple)

### Paso 4: Configurar el Material

#### Para SimpleOceanWater:

**Water Color:**
- Shallow: RGB(0, 102, 179) - Azul claro
- Deep: RGB(0, 25, 77) - Azul oscuro

**Waves:**
- Wave Normal Map: Arrastra tu normal map aquí
- Wave Speed: (0.05, 0.04, -0.03, -0.06)
- Wave Scale: (1, 1, 0.5, 0.5)
- Wave Strength: 0.3 - 0.5

**Refraction:**
- Refraction Strength: 0.1

**Fresnel:**
- Fresnel Power: 3.0
- Reflection Strength: 0.8

**Foam:**
- Foam Color: Blanco
- Foam Distance: 0.2
- Foam Cutoff: 0.7

#### Para BasicWater (más simple):

- Shallow Color: RGB(83, 206, 248) Alpha 0.7
- Deep Color: RGB(22, 104, 255) Alpha 0.75
- Smoothness: 0.95
- Normal Map: Tu normal map
- Wave Speed: 0.1
- Wave Tiling: 1.0
- Depth Fade Distance: 1.0

### Paso 5: Aplicar al Agua

1. **Encuentra el objeto del agua**:
   - En Hierarchy, busca tu plano/mesh de agua
   - Debería ser el que está amarillo en tu screenshot

2. **Aplicar material**:
   - Arrastra `WaterMaterial` al objeto
   - O en Inspector → Mesh Renderer → Materials → Asigna el material

### Paso 6: Ajustar la Iluminación

1. **Luz Direccional**:
   - Asegúrate de tener una Directional Light en la escena
   - Ajusta su ángulo para buenos reflejos

2. **Reflection Probe** (Opcional pero recomendado):
   - GameObject → Light → Reflection Probe
   - Colócalo sobre el agua
   - En Inspector:
     - Type: Realtime o Baked
     - Box Size: Que cubra toda el agua
     - Haz clic en "Bake" si es Baked

### Paso 7: Ajustar Configuración del Agua

**Si el agua se ve muy transparente:**
- Aumenta el Alpha de Deep Color

**Si no hay reflejos:**
- Verifica que Opaque Texture esté habilitado
- Aumenta Reflection Strength
- Añade un Reflection Probe

**Si las olas no se mueven:**
- Asegúrate de estar en Play Mode
- Aumenta Wave Speed

**Si se ve muy plana:**
- Aumenta Wave Strength
- Aumenta Normal Strength

### Paso 8: Optimización

**Para mejor rendimiento:**
1. Reduce Wave Scale
2. Usa BasicWater en lugar de SimpleOceanWater
3. Desactiva shadows en el agua

**Para mejor calidad:**
1. Usa un normal map de alta resolución
2. Añade Reflection Probe en Realtime
3. Activa Post-Processing

## 🎨 CONFIGURACIONES RECOMENDADAS POR TIPO

### Agua de Océano:
```
Shallow: RGB(0, 120, 180)
Deep: RGB(0, 30, 90)
Wave Speed: 0.05
Wave Strength: 0.5
```

### Agua de Piscina:
```
Shallow: RGB(80, 200, 255)
Deep: RGB(30, 150, 230)
Wave Speed: 0.02
Wave Strength: 0.2
```

### Agua Turbia:
```
Shallow: RGB(100, 130, 120)
Deep: RGB(40, 60, 50)
Wave Speed: 0.03
Wave Strength: 0.3
```

## ⚠️ PROBLEMAS COMUNES

### Agua se ve negra:
✅ Verifica que tienes una luz direccional
✅ Revisa que Opaque Texture esté habilitado
✅ Asegúrate de que hay objetos bajo el agua

### Sin refracción:
✅ Habilita Opaque Texture en URP Renderer
✅ Asegúrate de que el agua está en Queue Transparent
✅ Verifica que hay objetos opacos bajo el agua

### Errores en consola:
✅ Si dice "missing Normal Map", asigna una textura
✅ Si dice "Opaque Texture not available", habilítalo en URP
✅ Reimporta los shaders (clic derecho → Reimport)

## 🔧 TESTING RÁPIDO

1. Crea un Cube debajo del agua para probar refracción
2. Mueve la cámara para ver reflejos en diferentes ángulos
3. Ajusta los valores en Play Mode para ver cambios en tiempo real
4. Presiona Ctrl+Z si algo no te gusta

---

## 📝 NOTAS FINALES

- Los shaders funcionan con **URP 10+**
- Requieren **Shader Model 3.5+**
- Los efectos se ven mejor con **Opaque Texture habilitado**
- Para acuarios, usa BasicWater con valores bajos de Wave

¡Disfruta de tu agua realista! 🌊
