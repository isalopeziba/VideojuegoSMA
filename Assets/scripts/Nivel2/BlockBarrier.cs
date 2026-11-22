
using UnityEngine;

public class BlockBarrier : MonoBehaviour
{
    [Header("Elementos de la barrera")]
    public GameObject barreraVisual;
    public Collider barreraCollider;

    private bool abierta = false;

    public void AbrirPaso()
    {
        if (abierta) return;

        abierta = true;

        if (barreraVisual != null)
        {
            barreraVisual.SetActive(false);
        }

        if (barreraCollider != null)
        {
            barreraCollider.enabled = false;
        }

        Debug.Log("¡El camino está desbloqueado!");
    }
}
