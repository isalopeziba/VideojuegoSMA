using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public RectTransform barraverde;
    public RectTransform basura;
    public Image barraCaptura;
    public float velbasura = 200f;
    public float velbarra = 300f;
    public float velcaptura = 0.5f;

    public float limiteBasura = 150f;
    public float limitebarraverde = 150f;

    float direccionbasura = 1;
    float tiempoCambio;
    public float intervaloDireccionMin = 0.5f;
    public float intervaloDireccionMax = 1.5f;

    float capturaProgreso;
    private basura basuraActual;

    // Método público que inicia el minijuego desde el script basura.cs
    public void IniciarJuego(basura interactuable)
    {
        basuraActual = interactuable;

        // Reinicio de barra y posiciones al comenzar
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
        moverPez();
        controlBarraVerde();
        actualizaCaptura();
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

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
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

        // Victoria
        if (capturaProgreso >= 1f)
        {
            if (basuraActual != null)
            {
                basuraActual.FinalizarMinijuego(true);
            }
        }
        // Derrota
        else if (capturaProgreso <= 0f)
        {
            if (basuraActual != null)
            {
                basuraActual.FinalizarMinijuego(false);
            }
        }
    }

    void nuevoTiempoCambio()
    {
        float intervaloCambio = Random.Range(intervaloDireccionMin, intervaloDireccionMax);
        tiempoCambio = Time.time + intervaloCambio;
    }
}