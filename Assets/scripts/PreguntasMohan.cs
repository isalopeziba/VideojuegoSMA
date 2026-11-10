using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class PreguntaMohan
{
    [TextArea(2, 4)] public string enunciado;
    [TextArea(1, 3)] public string[] opciones = new string[3];   // en orden A, B, C original
    [Tooltip("0 = A, 1 = B, 2 = C (según el orden original)")]
    public int correcta;
    [TextArea(2, 4)] public string textoAcierto;
    [TextArea(2, 4)] public string textoFallo;

    [Tooltip("Opción INCORRECTA que el Duende descarta (0 = A, 1 = B, 2 = C)")]
    public int opcionQueDescarta;
    [TextArea(1, 3)] public string textoPista;
}

[System.Serializable]
public class SeccionRio
{
    public string nombre = "Sección";

    [Tooltip("Collider grande que cubre la sección. Al iniciar, las basuras que estén dentro se agregan solas a la lista.")]
    public BoxCollider zona;

    [Tooltip("Basuras de la sección. Si la dejas vacía y pones una zona, se llena sola.")]
    public List<GameObject> basuras = new List<GameObject>();

    [Tooltip("Basuras DESACTIVADAS que reaparecen en esta sección cuando se falla la pregunta")]
    public List<GameObject> basurasError = new List<GameObject>();

    [Tooltip("Pared de niebla que se disuelve al acertar y abre la siguiente sección (en la última puede quedar vacía)")]
    public GameObject paredNiebla;

    [Tooltip("Efecto opcional que se reproduce cuando empieza a disolverse la niebla")]
    public ParticleSystem efectoAlAbrir;

    [Tooltip("Para conectar otras cosas al abrir la sección (sonido, calmar el agua, etc.)")]
    public UnityEvent alAbrirse;

    [System.NonSerialized] public Bounds limites;      // se guardan al iniciar
    [System.NonSerialized] public bool tieneLimites;
}

public class PreguntasMohan : MonoBehaviour
{
    [Header("Mohán")]
    public Text textoMohan;
    public Animator animatorMohan;
    public string animMohanIdle = "idle_mohan";
    public string animMohanSonrie = "mohan_sonriendo";

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

    [Header("Secciones del río (una pregunta por sección, en orden)")]
    public List<SeccionRio> secciones = new List<SeccionRio>();
    public int basurasPorFallo = 2;

    [Header("Niebla")]
    public float duracionNiebla = 3f;

    [Header("Barra del río (puntos fijos sobre 100)")]
    public float puntosAcierto = 4f;
    public float puntosFallo = 8f;

    [Header("Tiempos (segundos)")]
    public float esperaTrasAcierto = 3f;
    public float esperaTrasFallo = 3f;

    [Header("Final")]
    [TextArea(2, 4)] public string textoFinal = "Gracias, joven. Cuida mi río y cuéntaselo a los demás.";
    public UnityEvent alTerminar;

    [Header("Preguntas (la 1 va con la sección 1, la 2 con la 2...)")]
    public List<PreguntaMohan> preguntas = new List<PreguntaMohan>
    {
        new PreguntaMohan
        {
            enunciado = "Una red sale del agua con cien peces. Un hombre necesita diez. ¿Qué pasa con los otros noventa?",
            opciones = new string[]
            {
                "Se venden y así nadie pasa hambre",
                "Se pudren en la orilla y el río se queda sin quienes lo habitan",
                "El río los vuelve a crear esa misma noche"
            },
            correcta = 1,
            textoAcierto = "Lo que se toma de más no vuelve. El río da lo que se necesita, no es una bodega.",
            textoFallo = "No, joven. Un pez sacado de más no se devuelve al agua. Piensa en quién se queda sin nada.",
            opcionQueDescarta = 0,
            textoPista = "El Mohán nunca aprobaría tomar de más."
        },
        new PreguntaMohan
        {
            enunciado = "El agua que ensucias hoy es la que beberá quien venga después de ti. ¿Quién paga lo que tiras al río?",
            opciones = new string[]
            {
                "Nadie, el río es muy grande",
                "Los animales del río, las personas que lo beben y sus hijos",
                "Solo el que lo ensució"
            },
            correcta = 1,
            textoAcierto = "Así es. Quien tira algo, deja que otros paguen su descuido.",
            textoFallo = "El río corre y lo que tiras llega a muchos. Mira otra vez.",
            opcionQueDescarta = 0,
            textoPista = "Por grande que sea, el río no se traga lo que le tiran."
        },
        new PreguntaMohan
        {
            enunciado = "Hoy sacaste la basura de mi río. Mañana alguien volverá a tirarla. ¿Para qué sirvió lo que hiciste?",
            opciones = new string[]
            {
                "Para nada, se va a ensuciar igual",
                "Sirve si se lo cuentas y adviertes a los demás y les enseñas la importancia de cuidar el río",
                "Para que alguien te lo agradezca"
            },
            correcta = 1,
            textoAcierto = "Un río se cuida con manos, pero también con historias. Lo que se cuenta no se olvida.",
            textoFallo = "Si nadie más lo sabe, el río vuelve a ensuciarse. Piensa en qué hace que algo dure.",
            opcionQueDescarta = 2,
            textoPista = "Al Mohán no le importan los aplausos, le importa su río."
        }
    };

    // ---- Lo que el Duende escucha ----
    public event System.Action<int> AlIniciarPregunta;   // la primera vez que sale la pregunta de una sección
    public event System.Action AlFallar;                 // cada respuesta incorrecta

    // true mientras los botones están esperando respuesta
    public bool PreguntaActiva { get; private set; }

    // cuántas basuras normales se han recogido en todo el nivel
    public int BasurasRecogidas
    {
        get
        {
            int n = 0;
            foreach (SeccionRio s in secciones)
                foreach (GameObject g in s.basuras)
                    if (g == null || !g.activeSelf) n++;
            return n;
        }
    }

    private int seccionActual = 0;
    private bool enPregunta = false;
    private bool terminado = false;
    private int[] ordenActual = { 0, 1, 2 };
    private bool pistaUsada = false;
    private List<GameObject> basurasPendientes = new List<GameObject>();

    private int TotalSecciones => Mathf.Min(secciones.Count, preguntas.Count);

    private void Awake()
    {
        for (int i = 0; i < botones.Length; i++)
        {
            int indiceBoton = i;
            if (botones[i] != null)
                botones[i].onClick.AddListener(() => Responder(indiceBoton));
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
        if (textoMohan != null) textoMohan.text = "";

        DetectarBasurasPorZona();

        if (secciones.Count != preguntas.Count)
            Debug.LogWarning($"PreguntasMohan: hay {secciones.Count} secciones y {preguntas.Count} preguntas. Deberían ser iguales.");
    }

    private void DetectarBasurasPorZona()
    {
        basura[] todas = FindObjectsByType<basura>(FindObjectsSortMode.None); // solo las activas

        foreach (SeccionRio s in secciones)
        {
            if (s.zona == null) continue;

            s.limites = s.zona.bounds;   // leer los límites ANTES de apagar el collider
            s.tieneLimites = true;
            s.zona.enabled = false;

            if (s.basuras.Count > 0) continue;

            foreach (basura b in todas)
                if (s.limites.Contains(b.transform.position))
                    s.basuras.Add(b.gameObject);

            if (s.basuras.Count == 0)
                Debug.LogWarning($"PreguntasMohan: la {s.nombre} no tiene basuras dentro de su zona.");
            else
                Debug.Log($"PreguntasMohan: {s.nombre} tiene {s.basuras.Count} basuras.");
        }
    }

    private void Update()
    {
        if (terminado || enPregunta || seccionActual >= TotalSecciones) return;

        if (SeccionLimpia(secciones[seccionActual]))
            IniciarPregunta();
    }

    private bool SeccionLimpia(SeccionRio s)
    {
        if (s.basuras.Count == 0) return false;
        foreach (GameObject g in s.basuras)
            if (g != null && g.activeSelf) return false;
        return true;
    }

    // ================== PISTA FUERA DE LAS PREGUNTAS ==================

    // Devuelve la basura sin recoger más cercana al jugador, dentro de la sección donde está
    public GameObject BasuraParaPista(Vector3 posJugador)
    {
        SeccionRio s = SeccionDe(posJugador);
        if (s == null) return null;

        GameObject mejor = null;
        float mejorDist = float.MaxValue;

        void Revisar(List<GameObject> lista)
        {
            foreach (GameObject g in lista)
            {
                if (g == null || !g.activeSelf) continue;
                float d = (g.transform.position - posJugador).sqrMagnitude;
                if (d < mejorDist) { mejorDist = d; mejor = g; }
            }
        }

        Revisar(s.basuras);
        Revisar(s.basurasError);   // las que reaparecieron por un error también cuentan
        return mejor;
    }

    private SeccionRio SeccionDe(Vector3 pos)
    {
        foreach (SeccionRio s in secciones)
        {
            if (!s.tieneLimites) continue;
            // solo X y Z, para que la altura del jugador no importe
            if (pos.x >= s.limites.min.x && pos.x <= s.limites.max.x &&
                pos.z >= s.limites.min.z && pos.z <= s.limites.max.z)
                return s;
        }
        // si no está dentro de ninguna zona, usar la sección en curso
        return seccionActual < secciones.Count ? secciones[seccionActual] : null;
    }

    // ================== PREGUNTA ==================

    private void IniciarPregunta()
    {
        enPregunta = true;
        MostrarPregunta(false);
        AlIniciarPregunta?.Invoke(seccionActual);
    }

    private void MostrarPregunta(bool mezclar)
    {
        PreguntaMohan p = preguntas[seccionActual];

        if (mezclar) MezclarOrden();
        else ordenActual = new int[] { 0, 1, 2 };

        if (textoMohan != null) textoMohan.text = p.enunciado;

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

        PreguntaMohan p = preguntas[seccionActual];
        bool acierto = ordenActual[boton] == p.correcta;

        PintarBoton(boton, acierto ? colorCorrecto : colorIncorrecto);
        ReproducirSonido(acierto ? sonidoCorrecto : sonidoIncorrecto);

        if (acierto) StartCoroutine(RutinaAcierto(p));
        else
        {
            AlFallar?.Invoke();
            StartCoroutine(RutinaFallo(p));
        }
    }

    private IEnumerator RutinaAcierto(PreguntaMohan p)
    {
        if (textoMohan != null) textoMohan.text = p.textoAcierto;
        if (animatorMohan != null) animatorMohan.Play(animMohanSonrie, 0, 0f);
        if (BarraRio.Instancia != null) BarraRio.Instancia.Sumar(puntosAcierto);

        yield return new WaitForSeconds(tiempoColorBoton);
        if (panelBotones != null) panelBotones.SetActive(false);

        StartCoroutine(DisolverNiebla(secciones[seccionActual]));

        yield return new WaitForSeconds(Mathf.Max(0f, esperaTrasAcierto - tiempoColorBoton));

        if (animatorMohan != null) animatorMohan.Play(animMohanIdle, 0, 0f);

        seccionActual++;
        enPregunta = false;

        if (seccionActual >= TotalSecciones) Terminar();
        else if (textoMohan != null) textoMohan.text = "";
    }

    private IEnumerator RutinaFallo(PreguntaMohan p)
    {
        if (textoMohan != null) textoMohan.text = p.textoFallo;
        if (BarraRio.Instancia != null) BarraRio.Instancia.Restar(puntosFallo);

        yield return new WaitForSeconds(tiempoColorBoton);
        if (panelBotones != null) panelBotones.SetActive(false);

        yield return new WaitForSeconds(Mathf.Max(0f, esperaTrasFallo - tiempoColorBoton));

        basurasPendientes = ActivarBasuras(secciones[seccionActual]);
        if (basurasPendientes.Count > 0)
            yield return new WaitUntil(() => TodasRecogidas(basurasPendientes));
        basurasPendientes.Clear();

        MostrarPregunta(true);
    }

    // ================== NIEBLA ==================

    private IEnumerator DisolverNiebla(SeccionRio s)
    {
        s.alAbrirse?.Invoke();
        if (s.efectoAlAbrir != null) s.efectoAlAbrir.Play();

        GameObject pared = s.paredNiebla;
        if (pared == null) yield break;

        foreach (Collider c in pared.GetComponentsInChildren<Collider>())
            c.enabled = false;

        foreach (ParticleSystem ps in pared.GetComponentsInChildren<ParticleSystem>())
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        List<Material> materiales = new List<Material>();
        List<Color> coloresIniciales = new List<Color>();
        foreach (Renderer r in pared.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer) continue;
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_BaseColor") || m.HasProperty("_Color"))
                {
                    materiales.Add(m);
                    coloresIniciales.Add(m.color);
                }
            }
        }

        float t = 0f;
        while (t < duracionNiebla)
        {
            t += Time.deltaTime;
            float factor = 1f - Mathf.Clamp01(t / duracionNiebla);
            for (int i = 0; i < materiales.Count; i++)
            {
                Color c = coloresIniciales[i];
                c.a *= factor;
                materiales[i].color = c;
            }
            yield return null;
        }

        pared.SetActive(false);
    }

    // ================== RETROALIMENTACIÓN ==================

    private void PintarBoton(int indice, Color color)
    {
        if (botones[indice] == null) return;
        Image img = botones[indice].image;
        if (img != null) img.color = color;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volumenSonidos);
    }

    // ================== BASURA DE ERROR ==================

    private List<GameObject> ActivarBasuras(SeccionRio s)
    {
        List<GameObject> candidatas = new List<GameObject>();
        foreach (GameObject g in s.basurasError)
            if (g != null && !g.activeSelf) candidatas.Add(g);

        List<GameObject> activadas = new List<GameObject>();
        int cantidad = Mathf.Min(basurasPorFallo, candidatas.Count);
        for (int i = 0; i < cantidad; i++)
        {
            int r = Random.Range(0, candidatas.Count);
            candidatas[r].SetActive(true);
            activadas.Add(candidatas[r]);
            candidatas.RemoveAt(r);
        }

        if (cantidad < basurasPorFallo)
            Debug.LogWarning($"PreguntasMohan: no quedan suficientes basuras de error en la {s.nombre}.");

        return activadas;
    }

    private bool TodasRecogidas(List<GameObject> lista)
    {
        foreach (GameObject g in lista)
            if (g != null && g.activeSelf) return false;
        return true;
    }

    // ================== PISTA EN LAS PREGUNTAS ==================

    // La llama el Duende. Descarta una opción incorrecta y devuelve la frase de pista
    // (o null si no se pudo usar: no hay pregunta o ya se usó en esta pregunta).
    public string UsarPista()
    {
        if (!PreguntaActiva || pistaUsada) return null;
        pistaUsada = true;

        PreguntaMohan p = preguntas[seccionActual];

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

    private void MostrarBoton(int indice, bool visible)
    {
        if (botones[indice] == null) return;

        CanvasGroup cg = botones[indice].GetComponent<CanvasGroup>();
        if (cg == null) cg = botones[indice].gameObject.AddComponent<CanvasGroup>();

        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    // ================== FINAL ==================

    private void Terminar()
    {
        terminado = true;
        PreguntaActiva = false;
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoMohan != null) textoMohan.text = textoFinal;
        alTerminar?.Invoke();
    }

    // ================== PRUEBAS ==================

    [ContextMenu("Probar: limpiar sección actual")]
    private void ProbarLimpiarSeccion()
    {
        if (seccionActual >= secciones.Count) return;
        foreach (GameObject g in secciones[seccionActual].basuras)
            if (g != null) g.SetActive(false);
    }
}