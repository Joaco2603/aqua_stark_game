using UnityEngine;

public class FishMoveAnimation : MonoBehaviour
{
    [Header("Asignación de Huesos (Main2 a Main5)")]
    public Transform[] huesosCola; // Tamaño: 4

    [Header("Parámetros de Movimiento")]
    public float velocidadNado = 5.0f;
    public float amplitudBase = 12.0f;  // El ángulo base de giro
    public float desfaseOnda = 0.7f;    // Retraso entre huesos para efecto látigo

    [Range(1f, 3f)]
    public float latigoIntensidad = 1.5f; // Cuánto más se mueve la punta que la base

    void Update()
    {
        if (huesosCola == null || huesosCola.Length == 0) return;

        for (int i = 0; i < huesosCola.Length; i++)
        {
            // 1. Calculamos la onda senoidal con desfase
            float tiempo = Time.time * velocidadNado;
            float retraso = i * desfaseOnda;
            float onda = Mathf.Sin(tiempo - retraso);

            // 2. Aumentamos la amplitud conforme llegamos al final de la cola (progresivo)
            // El primer hueso (i=0) se mueve normal, el último (i=3) se mueve más.
            float amplitudProgresiva = amplitudBase * (1 + (i * latigoIntensidad / huesosCola.Length));

            float anguloFinal = onda * amplitudProgresiva;

            // 3. Aplicamos la rotación (Prueba con Y si el pez es vertical, o Z/X según el modelo)
            huesosCola[i].localRotation = Quaternion.Euler(0, anguloFinal, 0);
        }
    }
}
