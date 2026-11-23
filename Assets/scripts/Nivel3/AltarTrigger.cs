
using UnityEngine;

public class AltarTrigger : MonoBehaviour
{
    public Altar altar;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null)
        {
            jugador.altarActual = altar;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null && jugador.altarActual == altar)
        {
            jugador.altarActual = null;
        }
    }
}
