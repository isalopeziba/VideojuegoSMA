
using UnityEngine;

// Clase para que la cámara siga a un objetivo 2.5D con suavizado,
// "look ahead" horizontal y movimiento en profundidad sin alterar su rotación.
public class CameraFollow2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player; // Referencia al transform del jugador
    [SerializeField] private Rigidbody playerRb; // Rigidbody del jugador para detectar su movimiento

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.15f; // Tiempo de suavizado para el seguimiento
    [SerializeField] private float heightOffset = 1f; // Altura adicional respecto al jugador
    [SerializeField] private float depthOffset = 0f; // Desplazamiento fijo en profundidad respecto al jugador

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f; // Distancia de anticipación horizontal
    [SerializeField] private float lookAheadSmooth = 5f; // Suavizado del look ahead

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f; // Amplitud del movimiento oscilante vertical
    [SerializeField] private float swaySpeed = 4f; // Velocidad del movimiento oscilante

    private Vector3 velocity; // Velocidad utilizada por SmoothDamp
    private float currentLookAhead; // Valor actual del look ahead
    private float initialCameraZ; // Profundidad original de la cámara
    private Quaternion initialCameraRotation; // Rotación original de la cámara

    // Se ejecuta al inicializar el componente.
    private void Awake()
    {
        // Guardar la profundidad y la rotación originales de la cámara.
        initialCameraZ = transform.position.z;
        initialCameraRotation = transform.rotation;

        // Obtener el Rigidbody del jugador si no se asignó manualmente.
        if (player != null && playerRb == null)
            playerRb = player.GetComponent<Rigidbody>();
    }

    // Se ejecuta después de Update, ideal para seguir al jugador.
    private void LateUpdate()
    {
        // Evitar errores si faltan las referencias necesarias.
        if (player == null || playerRb == null)
            return;

        // Detectar la dirección horizontal del movimiento del jugador.
        float horizontalVelocity = playerRb.linearVelocity.x;
        float direction = Mathf.Abs(horizontalVelocity) < 0.1f
            ? 0f
            : Mathf.Sign(horizontalVelocity);

        // Calcular y suavizar la anticipación horizontal.
        float targetLookAhead = direction * lookAheadDistance;

        currentLookAhead = Mathf.Lerp(
            currentLookAhead,
            targetLookAhead,
            lookAheadSmooth * Time.deltaTime
        );

        // Calcular la posición objetivo en los tres ejes.
        Vector3 targetPosition = new Vector3(
            player.position.x + currentLookAhead, // Anticipación horizontal
            player.position.y + heightOffset, // Altura respecto al jugador
            player.position.z + depthOffset // Seguimiento en profundidad
        );

        // Agregar un pequeño movimiento oscilante vertical.
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        targetPosition.y += sway;

        // Suavizar el movimiento de la cámara hacia la posición objetivo.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Restaurar la rotación original para conservar el look 2.5D.
        transform.rotation = initialCameraRotation;
    }
}
