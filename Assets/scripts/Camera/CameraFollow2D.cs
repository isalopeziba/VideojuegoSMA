using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmooth = 5f;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 4f;

    private Vector3 velocity;
    private float currentLookAhead;

    private void LateUpdate()
    {
        // Detectar dirección horizontal del jugador
        float direction = Mathf.Sign(player.GetComponent<Rigidbody>().linearVelocity.x);

        // Si está prácticamente quieto, no cambiar el look ahead
        if (Mathf.Abs(player.GetComponent<Rigidbody>().linearVelocity.x) < 0.1f)
            direction = 0;

        float targetLookAhead = direction * lookAheadDistance;

        currentLookAhead = Mathf.Lerp(
            currentLookAhead,
            targetLookAhead,
            lookAheadSmooth * Time.deltaTime
        );

        // Posición objetivo
        Vector3 targetPosition = new Vector3(
            player.position.x + currentLookAhead,
            player.position.y + 0.5f,
            transform.position.z
        );

        // Pequeño movimiento orgánico
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        targetPosition.y += sway;

        // Seguir suavemente al jugador
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
