using UnityEngine;
using UnityEngine.InputSystem;

public class basura : MonoBehaviour
{
    // Propiedad estática para que el BotonRecoger sepa qué basura está al alcance del jugador
    public static basura BasuraCercana { get; private set; }

    [Header("Referencias UI")]
    public GameObject canvasMinijuego;          // Objeto Canvas/Panel que contiene el minijuego
    public NewMonoBehaviourScript minijuego;     // Referencia al script del minijuego

    [Header("Configuración")]
    public float puntosPorBasura = 10f;         // Puntos que le sumará a BarraRio

    private bool jugadorEnRango = false;
    private bool minijuegoEnCurso = false;      // Evita abrir múltiples instancias del minijuego

    private void Update()
    {
        // Permite interactuar con la tecla E si el jugador está en rango y no hay un juego activo
        if (jugadorEnRango && !minijuegoEnCurso && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interactuar();
        }
    }

    // Método público llamado por el teclado o por BotonRecoger.cs
    public void Interactuar()
    {
        if (minijuegoEnCurso) return;

        minijuegoEnCurso = true;
        canvasMinijuego.SetActive(true);
        minijuego.IniciarJuego(this);
    }

    public void FinalizarMinijuego(bool ganado)
    {
        canvasMinijuego.SetActive(false);
        minijuegoEnCurso = false;

        if (ganado)
        {
            // Si la basura destruida era la registrada como cercana, limpia la referencia estática
            if (BasuraCercana == this)
            {
                BasuraCercana = null;
            }

            if (BarraRio.Instancia != null)
            {
                BarraRio.Instancia.Sumar(puntosPorBasura);
            }
            else
            {
                Debug.LogWarning("No se encontró la instancia de BarraRio en la escena.");
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Minijuego no completado. La basura sigue aquí para reintentarlo.");
        }
    }

    // Detección de cercanía (3D)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            BasuraCercana = this; // Registra esta basura como la más cercana
            Debug.Log("Presiona 'E' o el botón en pantalla para recolectar la basura");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            if (BasuraCercana == this)
            {
                BasuraCercana = null; // Quita el registro al alejarse
            }
        }
    }

    // Detección de cercanía (2D) - Descomenta estas líneas si tu juego es en 2D:
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            BasuraCercana = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            if (BasuraCercana == this)
            {
                BasuraCercana = null;
            }
        }
    }
    */
}