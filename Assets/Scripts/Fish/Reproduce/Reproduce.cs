using UnityEngine;
using Fish.Entities;

namespace Fish.Reproduce
{
    public class Reproduce : MonoBehaviour
    {
        bool isReproducing = false;

        public FishEntity CreateFish(int id, string name, int experience, float hunger = 0f, GameObject prefab = null)
        {
            return new FishEntity(id, name, experience, hunger, prefab);
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
            // 1. Obtener Materiales de los padres
            Material mat1 = GetMaterialFromFish(parent1);
            Material mat2 = GetMaterialFromFish(parent2);

            // 2. Algoritmo de Mezcla (Biological System)
            Material newMaterial = MixMaterials(mat1, mat2);

            // 3. Determinar Prefab (Heredar de uno aleatorio)
            GameObject parentPrefab = (Random.value > 0.5f) ? parent1.prefab : parent2.prefab;

            // 4. Crear el nuevo pez
            FishEntity child = CreateFish(newId, newName, 0, 0f, parentPrefab);

            // TODO: Al instanciar este pez, aplicar 'newMaterial' a su Renderer.

            return child;
        }

        /// <summary>
        /// Mezcla dos materiales para crear uno nuevo (Genética de color).
        /// </summary>
        public Material MixMaterials(Material m1, Material m2)
        {
            Material baseMat = m1 != null ? m1 : (m2 != null ? m2 : new Material(Shader.Find("Standard")));
            Material result = new Material(baseMat);

            if (m1 != null && m2 != null && m1.HasProperty("_Color") && m2.HasProperty("_Color"))
            {
                result.color = Color.Lerp(m1.color, m2.color, Random.Range(0.2f, 0.8f));
            }

            return result;
        }

        private Material GetMaterialFromFish(FishEntity fish)
        {
            if (fish == null || fish.prefab == null) return null;
            var renderer = fish.prefab.GetComponentInChildren<Renderer>();
            return renderer != null ? renderer.sharedMaterial : null;
        }
    }
} 