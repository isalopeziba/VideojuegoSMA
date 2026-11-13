
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRb;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private float depthOffset = 0f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmooth = 5f;

    [Header("Sway")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float swaySpeed = 4f;

    [Header("Camera Zoom")]
    [SerializeField] private float zoomHeightOffset = 0.5f;
    [SerializeField] private float zoomDepthOffset = -3f;
    [SerializeField] private float zoomSmooth = 3f;

    private Vector3 velocity;
    private float currentLookAhead;

    private float initialHeightOffset;
    private float initialDepthOffset;

    private float targetHeightOffset;
    private float targetDepthOffset;

    private Quaternion initialCameraRotation;

    private void Awake()
    {
        initialCameraRotation = transform.rotation;

        if (player != null && playerRb == null)
            playerRb = player.GetComponent<Rigidbody>();

        // Guardar los offsets originales.
        initialHeightOffset = heightOffset;
        initialDepthOffset = depthOffset;

        targetHeightOffset = initialHeightOffset;
        targetDepthOffset = initialDepthOffset;
    }

    private void LateUpdate()
    {
        if (player == null || playerRb == null)
            return;

        // Interpolar los offsets del zoom.
        heightOffset = Mathf.Lerp(
            heightOffset,
            targetHeightOffset,
            zoomSmooth * Time.deltaTime
        );

        depthOffset = Mathf.Lerp(
            depthOffset,
            targetDepthOffset,
            zoomSmooth * Time.deltaTime
        );

        // Detectar dirección horizontal.
        float horizontalVelocity = playerRb.linearVelocity.x;

        float direction = Mathf.Abs(horizontalVelocity) < 0.1f
            ? 0f
            : Mathf.Sign(horizontalVelocity);

        // Look Ahead.
        float targetLookAhead = direction * lookAheadDistance;

        currentLookAhead = Mathf.Lerp(
            currentLookAhead,
            targetLookAhead,
            lookAheadSmooth * Time.deltaTime
        );

        // Posición objetivo.
        Vector3 targetPosition = new Vector3(
            player.position.x + currentLookAhead,
            player.position.y + heightOffset,
            player.position.z + depthOffset
        );

        // Sway.
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        targetPosition.y += sway;

        // Movimiento suavizado.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        transform.rotation = initialCameraRotation;
    }

    // Activar acercamiento.
    public void ZoomIn()
    {
        targetHeightOffset = zoomHeightOffset;
        targetDepthOffset = zoomDepthOffset;
    }

    // Restaurar distancia original.
    public void ZoomOut()
    {
        targetHeightOffset = initialHeightOffset;
        targetDepthOffset = initialDepthOffset;
    }
}
