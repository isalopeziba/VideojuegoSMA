using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Preguntas de un guardián del bosque (Patasola, y sirve para el siguiente nivel).
// Se hacen seguidas al final del nivel. No hay castigo:
//   - Acierto: el guardián asiente, el bosque brilla un poco más y pasa a la siguiente.
//   - Fallo: explica el error, las luces del bosque bajan unos segundos y la pregunta
//            se repite con las opciones en otro orden.
public class PreguntasGuardian : MonoBehaviour, IFuentePista
{
    [Header("Guardián")]
    [Tooltip("Objeto PADRE del guardián")]
    public Transform guardian;
    [Tooltip("Dónde se para para hacer las preguntas (centro del claro)")]
    public Transform puntoPreguntas;
    [Tooltip("Opcional: para que llegue saltando antes de preguntar")]
    public PatasolaSaltos saltos;
    [Tooltip("Desde dónde empieza a saltar para llegar al centro del claro")]
    public Transform puntoEntrada;
    public Text textoGuardian;
    public Animator animator;
    public string animIdle = "";
    public string animAsentir = "";

    [Header("Jugador")]
    [Tooltip("Se bloquea mientras dura la ronda de preguntas")]
    public PlayerMov movimientoJugador;

    [Header("Botones A, B, C")]
    public GameObject panelBotones;
    public Button[] botones = new Button[3];

    [Header("Retroalimentación del botón")]
    public Color colorNormal = Color.white;
    public Color colorCorrecto = new Color(0.35f, 0.85f, 0.35f);
    public Color colorIncorrecto = new Color(0.9f, 0.3f, 0.3f);
    public float tiempoColorBoton = 1.2f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;
    [Range(0f, 1f)] public float volumenSonidos = 1f;

    [Header("Luces del bosque")]
    [Tooltip("Grupo de luces que se apaga unos segundos al fallar")]
    public GameObject lucesBosque;
    public float segundosLucesApagadas = 3f;
    [Tooltip("Luces que se van encendiendo con cada acierto (una por pregunta, desactivadas al inicio)")]
    public GameObject[] lucesPorAcierto;

    [Header("Tiempos")]
    public float esperaTrasAcierto = 3f;
    public float esperaTrasFallo = 3f;

    [Header("Final")]
    [TextArea(2, 4)] public string textoFinal = "";
    public UnityEvent alTerminar;

    [Header("Preguntas")]
    public List<PreguntaMohan> preguntas = new List<PreguntaMohan>
    {
        new PreguntaMohan
        {
            enunciado = "Alguien vacía un balde de aceite y químicos en la quebrada del monte porque \"es poquito\". ¿Qué le pasa a mi bosque?",
            opciones = new string[]
            {
                "Nada, la tierra lo absorbe",
                "El veneno baja por las raíces y llega a las plantas, a los insectos, a los pájaros y a quien beba esa agua",
                "Solo mueren las plantas del borde de la quebrada"
            },
            correcta = 1,
            textoAcierto = "Así es. En el monte todo está unido. Lo que se vierte en un lugar llega a todos.",
            textoFallo = "No, joven. Lo poquito también envenena. Piensa en a dónde va el agua.",
            opcionQueDescarta = 0,
            textoPista = "La tierra no se traga el veneno: lo lleva a todos los que viven en el bosque."
        },
        new PreguntaMohan
        {
            enunciado = "Una familia de monos aulladores pierde a su madre porque alguien la cazó por diversión. Los monos llevan semillas en su cuerpo y las siembran por todo el bosque. ¿Qué pierde mi bosque?",
            opciones = new string[]
            {
                "Nada, hay más monos",
                "Una sembradora: sin animales que lleven semillas, el bosque deja de crecer, además de perder un ejemplar único de fauna",
                "Solo un animal"
            },
            correcta = 1,
            textoAcierto = "Cada animal cuida el bosque a su manera. Cuando uno cae, el bosque pierde un poco de vida.",
            textoFallo = "No es solo un animal. Piensa en quién siembra mientras nadie mira.",
            opcionQueDescarta = 0,
            textoPista = "La Patasola cuida a los que viven en el bosque, no solo a los árboles."
        },
        new PreguntaMohan
        {
            enunciado = "Una colilla sobre hojas secas parece poca cosa. ¿Qué es en realidad?",
            opciones = new string[]
            {
                "Un residuo pequeño",
                "Una chispa que puede quemar en una noche lo que tardó cien años en crecer, con todos los que viven ahí",
                "Algo que la lluvia siempre apaga"
            },
            correcta = 1,
            textoAcierto = "Un descuido pequeño puede ser un incendio grande. Por eso el monte se respeta y no se recorre con descuido.",
            textoFallo = "La lluvia no siempre llega. Piensa en cuánto tarda en crecer un árbol.",
            opcionQueDescarta = 2,
            textoPista = "No siempre llueve a tiempo para apagar un fuego."
        }
    };

    // Para el Duende y las estrellas
    public event System.Action<int> AlIniciarPregunta;
    public event System.Action AlFallar;
    public bool PreguntaActiva { get; private set; }
    public int AciertosALaPrimera { get; private set; }

    private int indice = 0;
    private int[] ordenActual = { 0, 1, 2 };
    private bool pistaUsada = false;
    private bool falloEnEstaPregunta = false;
    private bool enCurso = false;

    private void Awake()
    {
        for (int i = 0; i < botones.Length; i++)
        {
            int b = i;
            if (botones[i] != null) botones[i].onClick.AddListener(() => Responder(b));
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoGuardian != null) textoGuardian.text = "";
        foreach (GameObject l in lucesPorAcierto) if (l != null) l.SetActive(false);
    }

    // ================== INICIO ==================

    [ContextMenu("Probar: iniciar preguntas")]
    public void IniciarPreguntas()
    {
        if (enCurso || preguntas.Count == 0) return;
        enCurso = true;
        indice = 0;
        BloqueoJugador.Aplicar(movimientoJugador, true);
        StartCoroutine(RutinaEntrada());
    }

    private IEnumerator RutinaEntrada()
    {
        if (guardian == null && saltos != null) guardian = saltos.transform;

        if (saltos != null && puntoEntrada != null && puntoPreguntas != null)
        {
            // Llega saltando al centro del claro y se queda ahí
            yield return saltos.Saltar(puntoEntrada, puntoPreguntas);
        }
        else if (guardian != null)
        {
            guardian.gameObject.SetActive(true);
            if (puntoPreguntas != null) guardian.position = puntoPreguntas.position;
        }

        if (guardian == null)
            Debug.LogWarning("PreguntasGuardian: falta asignar 'Guardian' (la Patasola).");

        Reproducir(animIdle);
        MostrarPregunta(true, nueva: true);
    }

    private void MostrarPregunta(bool mezclar, bool nueva)
    {
        PreguntaMohan p = preguntas[indice];

        if (mezclar) MezclarOrden();
        else ordenActual = new int[] { 0, 1, 2 };

        if (textoGuardian != null) textoGuardian.text = p.enunciado;

        for (int b = 0; b < botones.Length; b++)
        {
            if (botones[b] == null) continue;
            Text texto = botones[b].GetComponentInChildren<Text>();
            if (texto != null) texto.text = p.opciones[ordenActual[b]];
            PintarBoton(b, colorNormal);
            MostrarBoton(b, true);
            botones[b].interactable = true;
        }

        if (panelBotones != null) panelBotones.SetActive(true);
        pistaUsada = false;
        PreguntaActiva = true;

        if (nueva)
        {
            falloEnEstaPregunta = false;
            AlIniciarPregunta?.Invoke(indice);
        }
    }

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
                (ordenActual[i], ordenActual[j]) = (ordenActual[j], ordenActual[i]);
            }
            intentos++;
        }
        while (MismoOrden(anterior, ordenActual) && intentos < 10);
    }

    private bool MismoOrden(int[] a, int[] b)
    {
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    // ================== RESPONDER ==================

    private void Responder(int boton)
    {
        if (!PreguntaActiva) return;
        PreguntaActiva = false;

        PreguntaMohan p = preguntas[indice];
        bool acierto = ordenActual[boton] == p.correcta;

        PintarBoton(boton, acierto ? colorCorrecto : colorIncorrecto);
        ReproducirSonido(acierto ? sonidoCorrecto : sonidoIncorrecto);

        if (acierto)
        {
            if (!falloEnEstaPregunta) AciertosALaPrimera++;
            StartCoroutine(RutinaAcierto(p));
        }
        else
        {
            falloEnEstaPregunta = true;
            AlFallar?.Invoke();
            StartCoroutine(RutinaFallo(p));
        }
    }

    private IEnumerator RutinaAcierto(PreguntaMohan p)
    {
        if (textoGuardian != null) textoGuardian.text = p.textoAcierto;
        Reproducir(animAsentir);

        // El bosque brilla un poco más
        if (indice < lucesPorAcierto.Length && lucesPorAcierto[indice] != null)
            lucesPorAcierto[indice].SetActive(true);

        yield return new WaitForSeconds(tiempoColorBoton);
        if (panelBotones != null) panelBotones.SetActive(false);
        yield return new WaitForSeconds(Mathf.Max(0f, esperaTrasAcierto - tiempoColorBoton));

        Reproducir(animIdle);
        indice++;

        if (indice < preguntas.Count) MostrarPregunta(true, nueva: true);
        else Terminar();
    }

    private IEnumerator RutinaFallo(PreguntaMohan p)
    {
        if (textoGuardian != null) textoGuardian.text = p.textoFallo;
        if (lucesBosque != null) lucesBosque.SetActive(false);

        yield return new WaitForSeconds(tiempoColorBoton);
        if (panelBotones != null) panelBotones.SetActive(false);

        float restante = Mathf.Max(segundosLucesApagadas - tiempoColorBoton, esperaTrasFallo - tiempoColorBoton);
        yield return new WaitForSeconds(Mathf.Max(0f, restante));

        if (lucesBosque != null) lucesBosque.SetActive(true);
        MostrarPregunta(true, nueva: false);
    }

    private void Terminar()
    {
        enCurso = false;
        PreguntaActiva = false;
        BloqueoJugador.Aplicar(movimientoJugador, false);
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoGuardian != null) textoGuardian.text = textoFinal;
        alTerminar?.Invoke();
    }

    // ================== PISTA (IFuentePista) ==================

    public string UsarPista()
    {
        if (!PreguntaActiva || pistaUsada) return null;
        pistaUsada = true;

        PreguntaMohan p = preguntas[indice];
        for (int b = 0; b < botones.Length; b++)
        {
            if (ordenActual[b] == p.opcionQueDescarta)
            {
                MostrarBoton(b, false);
                break;
            }
        }
        return p.textoPista;
    }

    public GameObject ObjetivoParaPista(Vector3 posJugador) => null;

    // ================== AYUDANTES ==================

    private void PintarBoton(int i, Color color)
    {
        if (botones[i] == null) return;
        Image img = botones[i].image;
        if (img != null) img.color = color;
    }

    private void MostrarBoton(int i, bool visible)
    {
        if (botones[i] == null) return;
        CanvasGroup cg = botones[i].GetComponent<CanvasGroup>();
        if (cg == null) cg = botones[i].gameObject.AddComponent<CanvasGroup>();
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, volumenSonidos);
    }

    private void Reproducir(string estado)
    {
        if (animator == null || string.IsNullOrEmpty(estado)) return;
        if (animator.HasState(0, Animator.StringToHash(estado))) animator.Play(estado, 0, 0f);
        else Debug.LogWarning($"PreguntasGuardian: el Animator no tiene el estado '{estado}'.");
    }
}