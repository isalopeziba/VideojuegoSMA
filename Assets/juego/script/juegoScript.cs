using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Referencias del Minijuego")]
    public RectTransform barraverde;
    public RectTransform basura;
    public Image barraCaptura;

    [Header("Botón Único (Recoger / Jugar)")]
    [Tooltip("Arrastra aquí tu mismo botón de Recoger")]
    public Button botonUnico;

    [Header("Velocidades")]
    public float velbasura = 200f;
    public float velbarra = 300f;
    public float velcaptura = 0.5f;

    [Header("Límites de Movimiento")]
    public float limiteBasura = 150f;
    public float limitebarraverde = 150f;

    [Header("Movimiento de la Basura")]
    public float intervaloDireccionMin = 0.5f;
    public float intervaloDireccionMax = 1.5f;

    private float direccionbasura = 1;
    private float tiempoCambio;
    private float capturaProgreso;
    private basura basuraActual;

    private bool botonPresionado = false;
    private bool juegoActivo = false;
    private bool ignorarPrimerSoltar = false;

    private void Awake()
    {
        if (botonUnico != null)
        {
            EventTrigger trigger = botonUnico.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = botonUnico.gameObject.AddComponent<EventTrigger>();
            }

            // Al presionar el botón -> sube (solo si el juego ya inició)
            EventTrigger.Entry entradaPresionar = new EventTrigger.Entry();
            entradaPresionar.eventID = EventTriggerType.PointerDown;
            entradaPresionar.callback.AddListener((data) => { AlPresionarBoton(); });
            trigger.triggers.Add(entradaPresionar);

            // Al soltar el botón -> baja
            EventTrigger.Entry entradaSoltar = new EventTrigger.Entry();
            entradaSoltar.eventID = EventTriggerType.PointerUp;
            entradaSoltar.callback.AddListener((data) => { AlSoltarBoton(); });
            trigger.triggers.Add(entradaSoltar);

            // Si el dedo/cursor sale del botón -> baja
            EventTrigger.Entry entradaSalir = new EventTrigger.Entry();
            entradaSalir.eventID = EventTriggerType.PointerExit;
            entradaSalir.callback.AddListener((data) => { AlSoltarBoton(); });
            trigger.triggers.Add(entradaSalir);
        }
    }

    // Método público que inicia el minijuego desde basura.cs
    public void IniciarJuego(basura interactuable)
    {
        // Si ya estamos jugando, evita que el botón vuelva a reiniciar el minijuego
        if (juegoActivo) return;

        basuraActual = interactuable;
        juegoActivo = true;
        botonPresionado = false;

        // Evita que el mismo clic con el que pulsaste "Recoger" deje la barra pegada arriba
        ignorarPrimerSoltar = true;

        capturaProgreso = 0.2f;
        barraCaptura.fillAmount = capturaProgreso;

        Vector2 posBarra = barraverde.anchoredPosition;
        posBarra.y = 0;
        barraverde.anchoredPosition = posBarra;

        Vector2 posBasura = basura.anchoredPosition;
        posBasura.y = 0;
        basura.anchoredPosition = posBasura;

        nuevoTiempoCambio();
    }

    private void Update()
    {
        if (!juegoActivo) return;

        moverPez();
        controlBarraVerde();
        actualizaCaptura();
    }

    public void AlPresionarBoton()
    {
        if (!juegoActivo) return;
        ignorarPrimerSoltar = false;
        botonPresionado = true;
    }

    public void AlSoltarBoton()
    {
        if (!juegoActivo) return;
        if (ignorarPrimerSoltar)
        {
            ignorarPrimerSoltar = false;
            return;
        }
        botonPresionado = false;
    }

    void moverPez()
    {
        Vector2 posicionbasura = basura.anchoredPosition;

        if (Time.time >= tiempoCambio)
        {
            direccionbasura = -direccionbasura;
            nuevoTiempoCambio();
        }

        posicionbasura.y += direccionbasura * velbasura * Time.deltaTime;
        posicionbasura.y = Mathf.Clamp(posicionbasura.y, -limiteBasura, limiteBasura);
        basura.anchoredPosition = posicionbasura;
    }

    void controlBarraVerde()
    {
        Vector2 posicionBarraVerde = barraverde.anchoredPosition;

        if (botonPresionado)
        {
            posicionBarraVerde.y += velbarra * Time.deltaTime;
        }
        else
        {
            posicionBarraVerde.y -= velbarra * Time.deltaTime;
        }

        posicionBarraVerde.y = Mathf.Clamp(posicionBarraVerde.y, -limitebarraverde, limitebarraverde);
        barraverde.anchoredPosition = posicionBarraVerde;
    }

    void actualizaCaptura()
    {
        float yBarra = barraverde.anchoredPosition.y;
        float yBasura = basura.anchoredPosition.y;

        float mitadAltoBarra = (barraverde.rect.height * barraverde.localScale.y) / 2f;
        float mitadAltoBasura = (basura.rect.height * basura.localScale.y) / 2f;

        float barraArriba = yBarra + mitadAltoBarra;
        float barraAbajo = yBarra - mitadAltoBarra;

        float basuraArriba = yBasura + mitadAltoBasura;
        float basuraAbajo = yBasura - mitadAltoBasura;

        bool estaDentro = (basuraAbajo <= barraArriba) && (basuraArriba >= barraAbajo);

        if (estaDentro)
        {
            capturaProgreso += velcaptura * Time.deltaTime;
        }
        else
        {
            capturaProgreso -= velcaptura * Time.deltaTime;
        }

        capturaProgreso = Mathf.Clamp01(capturaProgreso);
        barraCaptura.fillAmount = capturaProgreso;

        if (capturaProgreso >= 1f)
        {
            Finalizar(true);
        }
        else if (capturaProgreso <= 0f)
        {
            Finalizar(false);
        }
    }

    void Finalizar(bool victoria)
    {
        juegoActivo = false;
        botonPresionado = false;

        if (basuraActual != null)
        {
            basuraActual.FinalizarMinijuego(victoria);
        }
    }

    void nuevoTiempoCambio()
    {
        float intervaloCambio = Random.Range(intervaloDireccionMin, intervaloDireccionMax);
        tiempoCambio = Time.time + intervaloCambio;
    }
}