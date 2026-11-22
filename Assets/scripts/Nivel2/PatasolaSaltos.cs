using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PatasolaSaltos : MonoBehaviour
{
    [Header("Animación")]
    public Animator animator;
    public SpriteRenderer sprite;
    [Tooltip("Estado mientras salta (vacío si no hay)")]
    public string animSalto = "";
    [Tooltip("Estado cuando se queda quieta hablando")]
    public string animIdle = "";

    [Header("Saltos")]
    public int cantidadSaltos = 4;
    public float alturaSalto = 2f;
    public float duracion = 2.5f;

    [Header("Diálogo al llegar")]
    [Tooltip("El Text de su canvas (con GloboDialogo el canvas se muestra solo)")]
    public Text texto;
    [Tooltip("Se elige una al azar cada vez que llega")]
    [TextArea(1, 3)]
    public string[] frasesAlLlegar =
    {
        "Llegué y este lugar sigue sin árboles. Inténtalo de nuevo, joven.",
        "El bosque no puede esperar. Siembra otra vez.",
        "Aquí todavía falta vida. Vuelve a empezar."
    };
    public float segundosMinimosHablando = 3f;

    [Header("Sonido opcional")]
    public AudioSource audioSource;
    public AudioClip sonidoSalto;

    [Tooltip("Ocultarla al terminar")]
    public bool ocultarAlTerminar = true;

    private void Start()
    {
        if (texto != null) texto.text = "";
        if (ocultarAlTerminar) gameObject.SetActive(false);
    }

    // La llama ControlNivel2; la zona se reinicia cuando esto termina
    public IEnumerator PasarPor(Transform desde, Transform hasta)
    {
        if (desde == null || hasta == null) yield break;

        // 1. Cruza saltando y se queda en idle
        yield return Saltar(desde, hasta);

        // 2. Habla
        if (texto != null && frasesAlLlegar.Length > 0)
        {
            string frase = frasesAlLlegar[Random.Range(0, frasesAlLlegar.Length)];
            texto.text = frase;
            yield return new WaitForSeconds(Mathf.Max(segundosMinimosHablando, frase.Length * 0.06f));
            texto.text = "";
        }

        // 3. Se va
        if (ocultarAlTerminar) gameObject.SetActive(false);
    }

    // Solo el salto de un punto a otro; al terminar queda quieta en idle y visible.
    // La usan PasarPor y PreguntasGuardian (para llegar a hacer las preguntas).
    public IEnumerator Saltar(Transform desde, Transform hasta)
    {
        if (desde == null || hasta == null) yield break;

        gameObject.SetActive(true);
        if (texto != null) texto.text = "";
        transform.position = desde.position;
        if (sprite != null) sprite.flipX = hasta.position.x < desde.position.x;
        Reproducir(animSalto);

        int ultimoSalto = -1;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracion);

            Vector3 pos = Vector3.Lerp(desde.position, hasta.position, p);
            pos.y += Mathf.Abs(Mathf.Sin(p * Mathf.PI * cantidadSaltos)) * alturaSalto;
            transform.position = pos;

            int salto = Mathf.FloorToInt(p * cantidadSaltos);
            if (salto != ultimoSalto && audioSource != null && sonidoSalto != null)
                audioSource.PlayOneShot(sonidoSalto);
            ultimoSalto = salto;

            yield return null;
        }
        transform.position = hasta.position;
        Reproducir(animIdle);
    }

    private void Reproducir(string estado)
    {
        if (animator == null || string.IsNullOrEmpty(estado)) return;
        if (animator.HasState(0, Animator.StringToHash(estado))) animator.Play(estado, 0, 0f);
        else Debug.LogWarning($"PatasolaSaltos: el Animator no tiene el estado '{estado}'.");
    }
}