using UnityEngine;
using Fish.Entities;

namespace Fish.Reproduce
{
    public class Reproduce : MonoBehaviour
    {
        bool isReproducing = true;
        Vector3 spawnPosition = new Vector3(8.42f, 9.41f, -75.135f);
        [Header("Opcional: contenedor padre para peces instanciados")]
        [SerializeField] private Transform parentContainer;

        public FishEntity CreateFish(int id, string name, int experience, float hunger = 0f, GameObject prefab = null, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("No se puede crear un pez sin prefab.");
                return null;
            }

            // Instanciar el prefab en la escena en la posición de spawn y con el padre correcto
            Transform usedParent = parent != null ? parent : parentContainer;
            GameObject fishObject;
            if (usedParent != null)
            {
                // Instanciar con parent preservando la posición en el mundo
                fishObject = Instantiate(prefab, spawnPosition, Quaternion.identity, usedParent);
            }
            else
            {
                fishObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            }
            
            // Obtener o añadir el componente FishEntity
            FishEntity fishEntity = fishObject.GetComponent<FishEntity>();
            if (fishEntity == null)
            {
                fishEntity = fishObject.AddComponent<FishEntity>();
            }

            // Configurar las propiedades
            fishEntity.id = id;
            fishEntity.fishName = name;
            fishEntity.experience = experience;
            fishEntity.hunger = hunger;
            fishEntity.prefab = prefab;
            fishEntity.isReproductionEnabled = true;
            
            // Forzar posición de spawn (antes de que Start() guarde initialPosition)
            fishObject.transform.position = spawnPosition;
            Debug.Log($"Reproduce CreateFish: Posición forzada a {spawnPosition}, posición actual: {fishObject.transform.position}");

            return fishEntity;
        }

        bool isReproducingEnabled(FishEntity[] parents)
        {
           if (parents == null || parents.Length < 2) 
            {
                Debug.LogError("Se necesitan al menos 2 padres para reproducir.");
                isReproducing = false;
                return false;
            }
            foreach(FishEntity fish in parents)
            {
                if (!fish.isReproductionEnabled)
                {
                    Debug.LogError("No puede reproducirse en este momento.");
                    isReproducing = false;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Reproduce usando un array de padres (debe tener al menos 2).
        /// </summary>
        public FishEntity ReproduceEntities(FishEntity[] parents, int newId, string newName)
        {
            if(!isReproducing)
            {
                Debug.LogWarning("El sistema de reproducción está deshabilitado.");
                return null;
            }
            Debug.Log("Reproduciendo peces...");
            FishEntity child = ReproduceEntities(parents[0], parents[1], newId, newName);
            return child;
        }

        /// <summary>
        /// Sistema biológico de reproducción que mezcla materiales.
        /// </summary>
        public FishEntity ReproduceEntities(FishEntity parent1, FishEntity parent2, int newId, string newName)
        {
            // 1. Obtener Materiales de los padres (del pez instanciado, no del prefab)
            Material mat1 = GetMaterialFromFishInstance(parent1);
            Material mat2 = GetMaterialFromFishInstance(parent2);

            Debug.Log($"Material padre 1: {(mat1 != null ? mat1.color.ToString() : "null")}");
            Debug.Log($"Material padre 2: {(mat2 != null ? mat2.color.ToString() : "null")}");

            // 2. Algoritmo de Mezcla Genética
            Material newMaterial = MixMaterials(mat1, mat2);

            Debug.Log($"Material hijo (mezclado): {(newMaterial != null ? newMaterial.color.ToString() : "null")}");

            // 3. Determinar Prefab (Heredar de uno aleatorio)
            GameObject parentPrefab = (Random.value > 0.5f) ? parent1.prefab : parent2.prefab;

            // 4. Crear el nuevo pez
            FishEntity child = CreateFish(newId, newName, 0, 0f, parentPrefab);

            // 5. Aplicar el material mezclado al pez hijo
            if (child != null && newMaterial != null)
            {
                ApplyMaterialToFish(child, newMaterial);
            }

            return child;
        }

        /// <summary>
        /// Aplica un material al renderer del pez.
        /// </summary>
        private void ApplyMaterialToFish(FishEntity fish, Material material)
        {
            if (fish == null || material == null) return;

            // Obtener todos los renderers del pez instanciado
            Renderer[] renderers = fish.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // Crear una instancia del material para este pez específico
                renderer.material = material;
            }

            Debug.Log($"Material aplicado al pez hijo: {material.color}");
        }

        /// <summary>
        /// Mezcla dos materiales para crear uno nuevo (Algoritmo Genético de color).
        /// Si los colores son iguales, devuelve el mismo color.
        /// Si son diferentes, mezcla los colores con variación genética.
        /// </summary>
        public Material MixMaterials(Material m1, Material m2)
        {
            Material baseMat = m1 != null ? m1 : (m2 != null ? m2 : new Material(Shader.Find("Standard")));
            Material result = new Material(baseMat);

            if (m1 != null && m2 != null)
            {
                Color color1 = GetMaterialColor(m1);
                Color color2 = GetMaterialColor(m2);

                // Verificar si los colores son diferentes
                if (!ColorsAreEqual(color1, color2))
                {
                    // Algoritmo genético: mezclar colores con variación aleatoria
                    float mixFactor = Random.Range(0.2f, 0.8f);
                    Color mixedColor = Color.Lerp(color1, color2, mixFactor);

                    // Añadir pequeña mutación genética (variación aleatoria)
                    float mutation = Random.Range(-0.1f, 0.1f);
                    mixedColor.r = Mathf.Clamp01(mixedColor.r + mutation);
                    mixedColor.g = Mathf.Clamp01(mixedColor.g + mutation);
                    mixedColor.b = Mathf.Clamp01(mixedColor.b + mutation);

                    SetMaterialColor(result, mixedColor);
                    Debug.Log($"Algoritmo genético aplicado: Color1={color1}, Color2={color2}, Resultado={mixedColor}");
                }
                else
                {
                    // Colores iguales: heredar el mismo color
                    SetMaterialColor(result, color1);
                    Debug.Log($"Colores iguales, heredando: {color1}");
                }
            }

            return result;
        }

        /// <summary>
        /// Obtiene el color de un material, soportando diferentes propiedades de shader.
        /// </summary>
        private Color GetMaterialColor(Material mat)
        {
            if (mat.HasProperty("_BaseColor"))
                return mat.GetColor("_BaseColor"); // URP/HDRP
            if (mat.HasProperty("_Color"))
                return mat.GetColor("_Color"); // Standard
            return Color.white;
        }

        /// <summary>
        /// Establece el color de un material, soportando diferentes propiedades de shader.
        /// </summary>
        private void SetMaterialColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color); // URP/HDRP
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color); // Standard
        }

        /// <summary>
        /// Compara si dos colores son iguales (con tolerancia).
        /// </summary>
        private bool ColorsAreEqual(Color c1, Color c2, float tolerance = 0.01f)
        {
            return Mathf.Abs(c1.r - c2.r) < tolerance &&
                   Mathf.Abs(c1.g - c2.g) < tolerance &&
                   Mathf.Abs(c1.b - c2.b) < tolerance;
        }

        /// <summary>
        /// Obtiene el material del pez INSTANCIADO (no del prefab).
        /// </summary>
        private Material GetMaterialFromFishInstance(FishEntity fish)
        {
            if (fish == null) return null;
            
            // Primero intentar obtener del pez instanciado en la escena
            var renderer = fish.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                return renderer.sharedMaterial;
            }

            // Fallback: obtener del prefab si no hay renderer en la instancia
            if (fish.prefab != null)
            {
                var prefabRenderer = fish.prefab.GetComponentInChildren<Renderer>();
                return prefabRenderer != null ? prefabRenderer.sharedMaterial : null;
            }

            return null;
        }

        /// <summary>
        /// OBSOLETO: Usar GetMaterialFromFishInstance en su lugar.
        /// </summary>
        private Material GetMaterialFromFish(FishEntity fish)
        {
            return GetMaterialFromFishInstance(fish);
        }
    }
} 