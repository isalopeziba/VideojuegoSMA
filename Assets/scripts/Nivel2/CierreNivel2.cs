using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Cierre del Nivel 2, en orden:
//   la Patasola entrega la Hoja del bosque (viaja hasta el joven y queda en el HUD) →
//   dice su frase → se aleja saltando → aparece la puerta de luz →
//   frase del Duende y dato curioso → se guarda la ficha → panel de estrellas.
//
// Va en un objeto vacío. Se conecta a PreguntasGuardian → Al Terminar → CierreNivel2.Iniciar
public class CierreNivel2 : MonoBehaviour
{
    [Header("Jugador")]
    public Transform jugador;
    [Tooltip("Se desactiva durante el cierre para que el joven no se mueva")]
    public PlayerMov movimientoJugador;

    [Header("Patasola")]
    public PatasolaSaltos patasola;
    public Text textoPatasola;
    [Tooltip("Hacia dónde se va saltando al final (entre los árboles)")]
    public Transform puntoSalida;

    [Header("Ofrenda: Hoja del bosque")]
    [Tooltip("Objeto de la hoja (sprite), desactivado en la escena")]
    public Transform ofrenda;
    [Tooltip("Desde dónde sale la hoja (un punto en la mano de la Patasola)")]
    public Transform origenOfrenda;
    public float duracionViaje = 1.5f;
    public float alturaArco = 1.5f;
    [Tooltip("Ícono de la hoja en el HUD, desactivado; se activa al recibirla")]
    public GameObject ofrendaEnHUD;

    [Header("Duende")]
    public DuendeNPC duende;

    [Header("Frases")]
    [TextArea(2, 4)] public string frasePatasola = "No te adentres solo en el monte ni con descuido. Cuídalo, y él te cuidará. Lleva esta hoja a la Madremonte: es mi palabra.";
    [TextArea(2, 4)] public string fraseDuende = "Aprendiste que el bosque se cuida con las manos y con la memoria. ¡Falta la última guardiana!";
    [TextArea(2, 4)] public string datoCurioso = "Dato curioso: muchas historias de la Patasola sirven de advertencia: no te adentres solo en el monte, y mucho menos con descuido.";

    [Header("Final")]
    public string idFicha = ColeccionFichas.Patasola;
    [Tooltip("Puerta de luz hacia la Madremonte, desactivada (con PortalNivel)")]
    public GameObject portal;
    public EstrellasNivel2 estrellas;
    public UnityEvent alTerminar;

    private bool iniciado = false;

    // Conectar a PreguntasGuardian → Al Terminar
    public void Iniciar()
    {
        if (iniciado) return;
        iniciado = true;
        StartCoroutine(Cierre());
    }

    private IEnumerator Cierre()
    {
        BloquearJugador(true);
        if (textoPatasola != null) textoPatasola.text = "";

        // 1. Entrega de la Hoja del bosque
        yield return EntregarOfrenda();

        // 2. Frase de la Patasola
        if (textoPatasola != null) textoPatasola.text = frasePatasola;
        yield return new WaitForSeconds(TiempoLectura(frasePatasola));
        if (textoPatasola != null) textoPatasola.text = "";

        // 3. Se aleja saltando entre los árboles
        if (patasola != null && puntoSalida != null)
        {
            yield return patasola.Saltar(patasola.transform, puntoSalida);
            patasola.gameObject.SetActive(false);
        }

        // 4. Aparece la puerta de luz
        if (portal != null) portal.SetActive(true);

        // 5. Duende y dato curioso
        if (duende != null)
        {
            duende.Decir(fraseDuende, true);
            duende.Decir(datoCurioso);
            yield return new WaitUntil(() => !duende.EstaHablando);
        }

        // 6. Ficha y estrellas
        ColeccionFichas.Obtener(idFicha);
        BloquearJugador(false);
        alTerminar?.Invoke();
        if (estrellas != null) estrellas.TerminarNivel();
    }

    private IEnumerator EntregarOfrenda()
    {
        if (ofrenda == null || jugador == null) yield break;

        Vector3 inicio = origenOfrenda != null ? origenOfrenda.position
                       : (patasola != null ? patasola.transform.position + Vector3.up : jugador.position);
        Vector3 escalaOriginal = ofrenda.localScale;
        ofrenda.position = inicio;
        ofrenda.gameObject.SetActive(true);

        // Viaje en arco hasta el joven
        float t = 0f;
        while (t < duracionViaje)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionViaje);
            Vector3 fin = jugador.position + Vector3.up;
            Vector3 pos = Vector3.Lerp(inicio, fin, Mathf.SmoothStep(0f, 1f, p));
            pos.y += Mathf.Sin(p * Mathf.PI) * alturaArco;
            ofrenda.position = pos;
            yield return null;
        }

        // Se "absorbe" en el joven
        t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            ofrenda.localScale = Vector3.Lerp(escalaOriginal, Vector3.zero, t / 0.4f);
            yield return null;
        }

        ofrenda.gameObject.SetActive(false);
        ofrenda.localScale = escalaOriginal;
        if (ofrendaEnHUD != null) ofrendaEnHUD.SetActive(true);
    }

    private void BloquearJugador(bool bloquear)
    {
        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = !bloquear;
            if (bloquear && movimientoJugador.animator != null)
                movimientoJugador.animator.Play(movimientoJugador.animIdle, 0, 0f);
        }

        if (bloquear && jugador != null)
        {
            Rigidbody rb = jugador.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    private float TiempoLectura(string texto) => Mathf.Max(3f, texto.Length * 0.06f);
}
