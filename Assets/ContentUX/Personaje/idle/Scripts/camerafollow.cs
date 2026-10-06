using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // Arrastra aquí el Transform del jugador
    public Vector3 offset = new Vector3(0f, 6f, -8f);

    [Header("Suavizado")]
    [Tooltip("Más bajo = cámara más rápida/rígida. Más alto = más suave/lenta.")]
    public float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;

    [Header("Límites del escenario")]
    public bool useBounds = true;
    public Vector2 xLimits = new Vector2(-20f, 20f);
    public Vector2 zLimits = new Vector2(-20f, 20f);

    [Header("Mirada")]
    public bool lookAtTarget = true;
    public float lookHeightOffset = 1.5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, xLimits.x, xLimits.y);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, zLimits.x, zLimits.y);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        if (lookAtTarget)
        {
            transform.LookAt(target.position + Vector3.up * lookHeightOffset);
        }
    }
}