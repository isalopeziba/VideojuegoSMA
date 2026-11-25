using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PortalCinematica : MonoBehaviour
{
    [Tooltip("Objeto con VideoPlayer + ReproductorCinematica, DESACTIVADO en la escena")]
    public GameObject cinematica;

    [Tooltip("Lo que debe desaparecer mientras se ve el video (HUD, joystick, botones, jugador...)")]
    public GameObject[] ocultarAlReproducir;

    [Tooltip("Movimiento del jugador, para que no camine mientras se ve el video")]
    public PlayerMov movimientoJugador;

    private bool usado = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (usado || !other.CompareTag("Player")) return;
        usado = true;

        if (movimientoJugador != null) movimientoJugador.enabled = false;

        foreach (GameObject g in ocultarAlReproducir)
            if (g != null) g.SetActive(false);

        if (cinematica != null) cinematica.SetActive(true);
        else Debug.LogWarning("PortalCinematica: falta asignar el objeto 'Cinematica'.");
    }
}