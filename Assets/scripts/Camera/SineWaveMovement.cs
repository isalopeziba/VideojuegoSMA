// Este script mueve un objeto a lo largo de una trayectoria en forma de onda senoidal (sine wave) en el eje Y mientras avanza en el eje X, útil para efectos de movimiento orgánico o patrones de enemigos en juegos 2D.

using UnityEngine;

public class SineWaveMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;      // Velocidad de movimiento horizontal
    [SerializeField] private float amplitude = 2f;  // Amplitud de la onda senoidal (altura máxima)
    [SerializeField] private float frequency = 2f;  // Frecuencia de la onda senoidal (qué tan rápido oscila)

    private float startY; // Posición Y inicial del objeto

    private void Start()
    {
        // Guardar la posición Y inicial al comenzar
        startY = transform.position.y;
    }

    private void Update()
    {
        // Calcular la nueva posición X avanzando a la velocidad definida
        float x = transform.position.x + speed * Time.deltaTime;

        // Calcular la nueva posición Y usando una función seno para crear el movimiento ondulatorio
        float y = startY + Mathf.Sin(Time.time * frequency) * amplitude;

        // Actualizar la posición del objeto
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
