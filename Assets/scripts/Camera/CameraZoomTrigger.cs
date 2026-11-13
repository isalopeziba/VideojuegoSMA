
using UnityEngine;

public class CameraZoomTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CameraFollow2D cameraFollow;

    [Header("Trigger Identification")]
    [SerializeField] private string triggerID = "ZoomTrigger";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        cameraFollow.ZoomIn();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        cameraFollow.ZoomOut();
    }
}
