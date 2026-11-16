using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class cierreNivel1 : MonoBehaviour
{
    [Header("Jugador")]
    public Transform jugador;
    [Tooltip("Se desactiva durante la ceremonia para que el joven no se mueva")]
    public PlayerMov movimientoJugador;

    [Header("Mohán")]
    
    public Transform mohan;
    public Animator animatorMohan;
    public Text textoMohan;

    [Tooltip("Punto en el río donde aparece el Mohán")]
    public Transform puntoMohanRio;
    public string animReverencia = "mohan_reverencia";
    public string animIdle = "idle_mohan";
    public float duracionReverencia = 2f;

    [Tooltip("Cuánto baja al salir/hundirse en el agua")]
    public float profundidadAgua = 2f;
    public float duracionSalirHundirse = 2f;

    [Header("Gota")]
    [Tooltip("Objeto de la gota (sprite + animación), desactivado en la escena")]
    public Transform gota;

    [Tooltip("Opcional: desde dónde sale la gota (la mano del Mohán)")]
    public Transform origenGota;

    public float duracionViajeGota = 1.5f;
    public float alturaArco = 1.5f;

    [Tooltip("Icono de la gota en el HUD, desactivado; se activa al recibirla")]
    public GameObject gotaEnHUD;

    [Header("Duende")]
    public DuendeNPC duende;

    [Header("Frases")]
    [TextArea(2, 4)] public string fraseMohan = "El río da lo que se necesita. Quien toma de más, un día se queda sin nada. Llévate esta gota: es mi palabra.";
    [TextArea(2, 4)] public string fraseDuende = "Tomar solo lo necesario también es una forma de cuidar. ¡Vamos al bosque!";
    [TextArea(2, 4)] public string datoCurioso = "Dato curioso: las leyendas del Mohán han servido para recordar que no hay que pescar de más ni contaminar los ríos.";

    [Header("Final")]
    public string idFicha = ColeccionFichas.Mohan;
    [Tooltip("Puerta de niebla verde, desactivada en la escena")]
    public GameObject portal;
    public EstrellasNivel1 estrellas;
    [Tooltip("Para conectar el río sereno, música, etc.")]
    public UnityEvent alRioSereno;

    private bool preparada = false;
    private bool iniciada = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Conectar al "Al Salir" de SalidaCueva
    public void PrepararCeremonia()
    {
        preparada = true;
        Debug.Log("CierreNivel1: ceremonia lista, esperando a que el jugador llegue al río.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!preparada || iniciada || !other.CompareTag("Player")) return;
        iniciada = true;
        StartCoroutine(Ceremonia());
    }

    private IEnumerator Ceremonia()
    {
        BloquearJugador(true);

        // 1. El Mohán sale del río
        Vector3 arriba = puntoMohanRio != null ? puntoMohanRio.position : mohan.position;
        Vector3 abajo = arriba - Vector3.up * profundidadAgua;
        if (textoMohan != null) textoMohan.text = "";
        mohan.gameObject.SetActive(true);
        Reproducir(animIdle);
        yield return MoverMohan(abajo, arriba, desvanecer: false);

        // 2. Reverencia
        Reproducir(animReverencia);
        yield return new WaitForSeconds(duracionReverencia);
        Reproducir(animIdle);

        // 3. Entrega de la gota
        yield return EntregarGota();

        // 4. Frase del Mohán
        if (textoMohan != null) textoMohan.text = fraseMohan;
        yield return new WaitForSeconds(TiempoLectura(fraseMohan));
        if (textoMohan != null) textoMohan.text = "";

        // 5. Se hunde en el río
        yield return MoverMohan(arriba, abajo, desvanecer: true);
        mohan.gameObject.SetActive(false);
        alRioSereno?.Invoke();

        // 6. Duende y dato curioso
        if (duende != null)
        {
            duende.Decir(fraseDuende, true);
            duende.Decir(datoCurioso);
            yield return new WaitUntil(() => !duende.EstaHablando);
        }

        // 7. Ficha, portal y estrellas
        ColeccionFichas.Obtener(idFicha);
        if (portal != null) portal.SetActive(true);

        BloquearJugador(false);
        if (estrellas != null) estrellas.TerminarNivel();
    }

    // ---------------- Partes de la ceremonia ----------------

    private IEnumerator EntregarGota()
    {
        if (gota == null || jugador == null) yield break;

        Vector3 inicio = origenGota != null ? origenGota.position : mohan.position + Vector3.up;
        Vector3 escalaOriginal = gota.localScale;
        gota.position = inicio;
        gota.gameObject.SetActive(true);

        // Viaje en arco hasta el joven
        float t = 0f;
        while (t < duracionViajeGota)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionViajeGota);
            Vector3 fin = jugador.position + Vector3.up;
            Vector3 pos = Vector3.Lerp(inicio, fin, Mathf.SmoothStep(0f, 1f, p));
            pos.y += Mathf.Sin(p * Mathf.PI) * alturaArco;
            gota.position = pos;
            yield return null;
        }

        // Se "absorbe" en el joven
        t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            gota.localScale = Vector3.Lerp(escalaOriginal, Vector3.zero, t / 0.4f);
            yield return null;
        }

        gota.gameObject.SetActive(false);
        gota.localScale = escalaOriginal;
        if (gotaEnHUD != null) gotaEnHUD.SetActive(true);
    }

    private IEnumerator MoverMohan(Vector3 desde, Vector3 hasta, bool desvanecer)
    {
        List<SpriteRenderer> sprites = new List<SpriteRenderer>(mohan.GetComponentsInChildren<SpriteRenderer>());
        List<Color> colores = new List<Color>();
        foreach (SpriteRenderer sr in sprites) colores.Add(sr.color);

        float t = 0f;
        while (t < duracionSalirHundirse)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracionSalirHundirse));
            mohan.position = Vector3.Lerp(desde, hasta, p);

            for (int i = 0; i < sprites.Count; i++)
            {
                Color c = colores[i];
                c.a = desvanecer ? colores[i].a * (1f - p) : colores[i].a * p;
                sprites[i].color = c;
            }
            yield return null;
        }

        // dejar los colores bien al terminar (por si se reutiliza)
        if (!desvanecer)
            for (int i = 0; i < sprites.Count; i++) sprites[i].color = colores[i];
    }

    private void BloquearJugador(bool bloquear)
    {
        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = !bloquear;

            // Quieto en idle mientras dura la ceremonia
            if (bloquear && movimientoJugador.animator != null)
                movimientoJugador.animator.Play(movimientoJugador.animIdle, 0, 0f);
        }

        if (bloquear && jugador != null)
        {
            Rigidbody rb = jugador.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    private void Reproducir(string estado)
    {
        if (animatorMohan == null || string.IsNullOrEmpty(estado)) return;
        if (animatorMohan.HasState(0, Animator.StringToHash(estado)))
            animatorMohan.Play(estado, 0, 0f);
        else
            Debug.LogWarning($"CierreNivel1: el Animator del Mohán no tiene el estado '{estado}'.");
    }

    private float TiempoLectura(string texto) => Mathf.Max(3f, texto.Length * 0.06f);
}
