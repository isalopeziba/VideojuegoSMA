
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MadremontePreguntas : MonoBehaviour
{
    [Header("Madremonte")]
    public Text textoMadremonte;
    public Animator animator;
    public string animIdle = "";
    public string animAsentir = "";

    [Header("Globo de diálogo")]
    public GameObject canvasDialogo;

    [Header("Botones A, B, C")]
    public GameObject panelBotones;
    public Button[] botones = new Button[3];

    [Header("Pregunta")]
    [TextArea(2, 5)]
    public string enunciado = "¿Qué debemos hacer para proteger el bosque?";

    [TextArea(1, 3)]
    public string[] opciones = new string[3]
    {
        "Cuidar los animales y las plantas",
        "Talar todos los árboles",
        "Contaminar los ríos"
    };

    [Tooltip("Posición de la respuesta correcta: 0, 1 o 2")]
    [Range(0, 2)]
    public int correcta = 0;

    [TextArea(2, 4)]
    public string textoAcierto = "¡Muy bien! Has demostrado respeto por la naturaleza.";

    [TextArea(2, 4)]
    public string textoFallo = "No has acertado. Piensa bien y vuelve a intentarlo.";

    [Header("Colores")]
    public Color colorNormal = Color.white;
    public Color colorCorrecto = new Color(0.35f, 0.85f, 0.35f);
    public Color colorIncorrecto = new Color(0.9f, 0.3f, 0.3f);
    public float tiempoColorBoton = 1.2f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;
    [Range(0f, 1f)]
    public float volumenSonidos = 1f;

    [Header("Tiempos")]
    public float esperaTrasFallo = 3f;
    public float esperaTrasAcierto = 3f;

    [Header("Final")]
    public UnityEvent alTerminar;

    private int[] ordenActual = { 0, 1, 2 };
    private bool preguntaActiva = false;
    private bool enCurso = false;
    private bool completado = false;

    private void Awake()
    {
        for (int i = 0; i < botones.Length; i++)
        {
            int b = i;

            if (botones[i] != null)
                botones[i].onClick.AddListener(() => Responder(b));
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (panelBotones != null)
            panelBotones.SetActive(false);

        if (textoMadremonte != null)
            textoMadremonte.text = "";

        if (canvasDialogo != null)
            canvasDialogo.SetActive(false);
    }

    // ================== INICIO ==================

    public void IniciarPregunta()
    {
        if (enCurso || completado) return;

        enCurso = true;

        Reproducir(animIdle);

        MostrarPregunta(false);
    }

    private void MostrarPregunta(bool mezclar)
    {
        if (canvasDialogo != null)
        canvasDialogo.SetActive(true);
        if (mezclar)
            MezclarOrden();
        else
            ordenActual = new int[] { 0, 1, 2 };

        if (textoMadremonte != null)
            textoMadremonte.text = enunciado;

        for (int b = 0; b < botones.Length; b++)
        {
            if (botones[b] == null) continue;

            Text texto = botones[b].GetComponentInChildren<Text>();

            if (texto != null)
                texto.text = opciones[ordenActual[b]];

            PintarBoton(b, colorNormal);
            botones[b].interactable = true;
            botones[b].gameObject.SetActive(true);
        }

        if (panelBotones != null)
            panelBotones.SetActive(true);

        preguntaActiva = true;
    }

    // ================== MEZCLAR RESPUESTAS ==================

    private void MezclarOrden()
    {
        int[] anterior = (int[])ordenActual.Clone();
        int intentos = 0;

        do
        {
            ordenActual = new int[] { 0, 1, 2 };

            for (int i = ordenActual.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                (ordenActual[i], ordenActual[j]) =
                    (ordenActual[j], ordenActual[i]);
            }

            intentos++;
        }
        while (MismoOrden(anterior, ordenActual) && intentos < 10);
    }

    private bool MismoOrden(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }

        return true;
    }

    // ================== RESPONDER ==================

    private void Responder(int boton)
    {
        if (!preguntaActiva || !enCurso) return;

        preguntaActiva = false;

        bool acierto = ordenActual[boton] == correcta;

        // Desactivar todos los botones mientras se procesa la respuesta
        foreach (Button b in botones)
        {
            if (b != null)
                b.interactable = false;
        }

        PintarBoton(
            boton,
            acierto ? colorCorrecto : colorIncorrecto
        );

        ReproducirSonido(
            acierto ? sonidoCorrecto : sonidoIncorrecto
        );

        if (acierto)
            StartCoroutine(RutinaAcierto());
        else
            StartCoroutine(RutinaFallo());
    }

    // ================== FALLO ==================

    private IEnumerator RutinaFallo()
    {
        if (textoMadremonte != null)
            textoMadremonte.text = textoFallo;

        yield return new WaitForSeconds(tiempoColorBoton);

        if (panelBotones != null)
            panelBotones.SetActive(false);

        yield return new WaitForSeconds(
            Mathf.Max(0f, esperaTrasFallo - tiempoColorBoton)
        );

        // Volver a preguntar con respuestas mezcladas
        MostrarPregunta(true);
    }

    // ================== ACIERTO ==================

    private IEnumerator RutinaAcierto()
    {
        if (canvasDialogo != null)
            canvasDialogo.SetActive(true);

        if (textoMadremonte != null)
            textoMadremonte.text = textoAcierto;

        Reproducir(animAsentir);

        yield return new WaitForSeconds(tiempoColorBoton);

        if (panelBotones != null)
            panelBotones.SetActive(false);

        yield return new WaitForSeconds(
            Mathf.Max(0f, esperaTrasAcierto - tiempoColorBoton)
        );

        // Ocultar diálogo de la pregunta
        if (canvasDialogo != null)
            canvasDialogo.SetActive(false);

        if (textoMadremonte != null)
            textoMadremonte.text = "";

        completado = true;
        enCurso = false;

        // Iniciar conversación final
        alTerminar?.Invoke();
    }

    // ================== AYUDANTES ==================

    private void PintarBoton(int i, Color color)
    {
        if (botones[i] == null) return;

        Image img = botones[i].image;

        if (img != null)
            img.color = color;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volumenSonidos);
    }

    private void Reproducir(string estado)
    {
        if (animator == null || string.IsNullOrEmpty(estado))
            return;

        if (animator.HasState(0, Animator.StringToHash(estado)))
            animator.Play(estado, 0, 0f);
    }
}
