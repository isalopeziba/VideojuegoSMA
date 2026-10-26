
using UnityEngine;
using UnityEngine.InputSystem;

public class basura : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvasMinijuego;
    public NewMonoBehaviourScript minijuego;

    [Header("Configuracion")]
    public float puntosPorBasura = 10f;

    private bool jugadorEnRango = false;
    private Transform jugador;

    public static basura BasuraCercana { get; private set; }

    private void Update()
    {
        if (jugadorEnRango && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interactuar();
        }

        // Limpiar referencia si esta basura ya no esta activa
        if (BasuraCercana == this && !gameObject.activeInHierarchy)
        {
            BasuraCercana = null;
        }
    }

    public void Interactuar()
    {
        if (!jugadorEnRango) return;

        if (canvasMinijuego == null || minijuego == null)
        {
            Debug.LogWarning(
                "Falta asignar Canvas Minijuego o Minijuego en " + name);
            return;
        }

        canvasMinijuego.SetActive(true);
        minijuego.IniciarJuego(this);
    }

    public void FinalizarMinijuego(bool ganado)
    {
        if (canvasMinijuego != null)
            canvasMinijuego.SetActive(false);

        if (ganado)
        {
            if (BarraRio.Instancia != null)
                BarraRio.Instancia.Sumar(puntosPorBasura);
            else
                Debug.LogWarning("No se encontro la instancia de BarraRio.");

            if (BasuraCercana == this)
                BasuraCercana = null;

            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Minijuego no completado. Puedes intentarlo de nuevo.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnRango = true;
        jugador = other.transform;

        ActualizarBasuraCercana();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnRango = true;
        jugador = other.transform;

        ActualizarBasuraCercana();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnRango = false;

        if (BasuraCercana == this)
            BasuraCercana = null;
    }

    private void ActualizarBasuraCercana()
    {
        if (!jugadorEnRango || jugador == null) return;

        if (BasuraCercana == null ||
            !BasuraCercana.gameObject.activeInHierarchy ||
            !BasuraCercana.jugadorEnRango ||
            BasuraCercana.jugador == null ||
            Vector3.Distance(jugador.position, transform.position) <
            Vector3.Distance(jugador.position, BasuraCercana.transform.position))
        {
            BasuraCercana = this;
        }
    }
}
