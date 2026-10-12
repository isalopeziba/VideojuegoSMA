using UnityEngine;

// Clase para que la cámara siga a un objetivo 2D con efectos de suavizado y "look ahead"
public class CameraFollow2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player; // Referencia al transform del jugador

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.15f; // Tiempo de suavizado para el seguimiento

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f; // Distancia de anticipación en la dirección del movimiento
    [SerializeField] private float lookAheadSmooth = 5f;   // Suavizado para el look ahead

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f; // Amplitud del movimiento oscilante vertical
    [SerializeField] private float swaySpeed = 4f;     // Velocidad del movimiento oscilante

    private Vector3 velocity;         // Velocidad utilizada por SmoothDamp
    private float currentLookAhead;   // Valor actual de look ahead

    // Se ejecuta después de Update, ideal para seguir al jugador
    private void LateUpdate()
    {
        // Detectar dirección horizontal del jugador usando la velocidad del Rigidbody
        float direction = Mathf.Sign(player.GetComponent<Rigidbody>().linearVelocity.x);

        // Si el jugador está casi quieto, no aplicar look ahead
        if (Mathf.Abs(player.GetComponent<Rigidbody>().linearVelocity.x) < 0.1f)
            direction = 0;

        // Calcular el look ahead objetivo según la dirección
        float targetLookAhead = direction * lookAheadDistance;

        // Suavizar el cambio de look ahead
        currentLookAhead = Mathf.Lerp(
            currentLookAhead,
            targetLookAhead,
            lookAheadSmooth * Time.deltaTime
        );

        // Calcular la posición objetivo de la cámara
        Vector3 targetPosition = new Vector3(
            player.position.x + currentLookAhead, // Anticipación horizontal
            player.position.y + 0.5f,             // Ligeramente por encima del jugador
            transform.position.z                  // Mantener la profundidad de la cámara
        );

        // Agregar un pequeño movimiento oscilante vertical para dar naturalidad
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        targetPosition.y += sway;

        // Mover la cámara suavemente hacia la posición objetivo
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
