using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DuendeNPC : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    [Header("Seguimiento")]
    public Transform jugador;
    public float distanciaSeguimiento = 1.5f;
    public float margenArranque = 0.3f;
    public float desplazamientoLateral = 0.8f;
    public float velocidad = 5f;
    public LayerMask terrainLayer;
    public float groundDist = 0.5f;

    [Header("Sprite y animaciones")]
    public SpriteRenderer spriteDuende;
    public Animator animator;
    public string animIdle = "duende_idle";
    public string animLado = "duende_camina";
    public string animAtras = "duende_detras";
    public string animFrente = "duende_frente";
    public string animSenalando = "duende_senalando";

    [Header("Referencias")]
    public PreguntasMohan preguntasMohan;

    [Header("Diálogo (canvas sobre la cabeza)")]
    public GameObject canvasDialogo;      // el canvas desactivado del Duende
    public Text textoDialogo;             // el texto dentro de ese canvas
    public float duracionMinimaFrase = 3f;
    public float segundosPorLetra = 0.06f; // las frases largas duran más

    [Header("Frases")]
    [TextArea(2, 5)]
    public string[] frasesInicio =
    {
        "Este río lo cuida el Mohán. Está muy sucio. Ayúdalo y quizá te escuche.",
        "Dato curioso: el Mohán viene de las tradiciones de los ríos Magdalena y Cauca. Se dice que vive en cuevas bajo el agua y que atrae a los pescadores con su música."
    };
    public float esperaAntesDeInicio = 1f;
    [TextArea(1, 3)] public string frasePrimerObjeto = "Eso no es del río. Gracias por sacarlo.";
    [TextArea(1, 3)] public string frase25 = "¿Oyes? El río empieza a respirar.";
    [TextArea(1, 3)] public string fraseMohanAparece = "No tengas miedo. Escucha bien la pregunta.";
    [TextArea(1, 3)] public string fraseError = "Casi. Piensa en qué le importa al río.";

    [Header("Pista (tecla Q o botón de pista)")]
    public int usosPista = 3;                          // por nivel
    public float duracionBrillo = 5f;
    public Color colorBrillo = new Color(0.3f, 1f, 0.3f); // verde
    public GameObject efectoBrillo;                    // prefab opcional (partículas) que se pone sobre la basura
    [TextArea(1, 3)] public string frasePistaBasura = "¡Mira! Allí brilla algo que no es del río.";
    [TextArea(1, 3)] public string fraseNoHayBasura = "Por aquí ya está limpio. Sigue adelante.";
    [TextArea(1, 3)] public string fraseSinPistas = "Ya no me quedan pistas. ¡Tú puedes!";

    // ---- estado interno ----
    private Vector3 direccionJugador = Vector3.right;
    private Vector3 posAnteriorJugador;
    private float lado = 1f;
    private float lateralActual = 0f;
    private bool siguiendo = false;
    private string estadoActual = "";
    private Vector3 velSuavizada = Vector3.zero;
    private bool estaCaminando = false;

    private Queue<string> colaFrases = new Queue<string>();
    private Coroutine rutinaDialogo;

    private bool estaSenalando = false;
    private Transform objetivoSenalado;
    private Coroutine rutinaBrillo;

    private bool dijoPrimerObjeto = false;
    private bool dijo25 = false;
    private bool dijoMohan = false;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        if (jugador != null) posAnteriorJugador = jugador.position;
        if (canvasDialogo != null) canvasDialogo.SetActive(false);

        if (preguntasMohan != null)
        {
            preguntasMohan.AlIniciarPregunta += OnPreguntaIniciada;
            preguntasMohan.AlFallar += OnFallo;
        }

        StartCoroutine(DialogoInicio());
    }

    private void OnDestroy()
    {
        if (preguntasMohan != null)
        {
            preguntasMohan.AlIniciarPregunta -= OnPreguntaIniciada;
            preguntasMohan.AlFallar -= OnFallo;
        }
    }

    private void OnEnable()
    {
        if (inputActions == null) inputActions = new InputSystem_Actions();
        inputActions.Player.Ayuda.performed += OnAyuda;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Player.Ayuda.performed -= OnAyuda;
        inputActions.Player.Disable();
    }

    private void OnAyuda(InputAction.CallbackContext context)
    {
        PulsarBotonPista();
    }

    // ================== DIÁLOGOS ==================

    private IEnumerator DialogoInicio()
    {
        yield return new WaitForSeconds(esperaAntesDeInicio);
        foreach (string f in frasesInicio) Decir(f);
    }

    // Cualquier script puede hacer hablar al Duende con esto
    public void Decir(string texto, bool interrumpir = false)
    {
        if (string.IsNullOrEmpty(texto) || textoDialogo == null) return;

        if (interrumpir)
        {
            colaFrases.Clear();
            if (rutinaDialogo != null) { StopCoroutine(rutinaDialogo); rutinaDialogo = null; }
        }

        colaFrases.Enqueue(texto);
        if (rutinaDialogo == null) rutinaDialogo = StartCoroutine(MostrarCola());
    }

    private IEnumerator MostrarCola()
    {
        while (colaFrases.Count > 0)
        {
            string frase = colaFrases.Dequeue();
            textoDialogo.text = frase;
            if (canvasDialogo != null) canvasDialogo.SetActive(true);

            float duracion = Mathf.Max(duracionMinimaFrase, frase.Length * segundosPorLetra);
            yield return new WaitForSeconds(duracion);
        }

        if (canvasDialogo != null) canvasDialogo.SetActive(false);
        textoDialogo.text = "";
        rutinaDialogo = null;
    }

    private void RevisarMomentos()
    {
        if (!dijoPrimerObjeto && preguntasMohan != null && preguntasMohan.BasurasRecogidas >= 1)
        {
            dijoPrimerObjeto = true;
            Decir(frasePrimerObjeto);
        }

        if (!dijo25 && BarraRio.Instancia != null && BarraRio.Instancia.Valor >= 25f)
        {
            dijo25 = true;
            Decir(frase25);
        }
    }

    private void OnPreguntaIniciada(int seccion)
    {
        if (dijoMohan) return;      // solo la primera vez que aparece el Mohán
        dijoMohan = true;
        Decir(fraseMohanAparece, true);
    }

    private void OnFallo()
    {
        Decir(fraseError, true);
    }

    // ================== PISTA ==================

    public void PulsarBotonPista()
    {
        // 1. Durante una pregunta: descartar una opción
        if (preguntasMohan != null && preguntasMohan.PreguntaActiva)
        {
            if (usosPista <= 0) { Decir(fraseSinPistas, true); return; }

            string pista = preguntasMohan.UsarPista();
            if (pista != null)
            {
                usosPista--;
                Decir(pista, true);
            }
            return;
        }

        // 2. Recolectando: iluminar una basura de la zona del jugador
        if (rutinaBrillo != null) return;   // ya hay una iluminada
        if (usosPista <= 0) { Decir(fraseSinPistas, true); return; }

        GameObject objetivo = (preguntasMohan != null && jugador != null)
            ? preguntasMohan.BasuraParaPista(jugador.position)
            : null;

        if (objetivo == null)
        {
            Decir(fraseNoHayBasura, true);   // no se gasta la pista
            return;
        }

        usosPista--;
        Decir(frasePistaBasura, true);
        rutinaBrillo = StartCoroutine(Iluminar(objetivo));
    }

    private IEnumerator Iluminar(GameObject objetivo)
    {
        estaSenalando = true;
        objetivoSenalado = objetivo.transform;

        // Guardar los colores originales
        List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        List<Color> coloresSprite = new List<Color>();
        List<Material> materiales = new List<Material>();
        List<Color> coloresMat = new List<Color>();

        foreach (Renderer r in objetivo.GetComponentsInChildren<Renderer>())
        {
            if (r is SpriteRenderer sr)
            {
                sprites.Add(sr);
                coloresSprite.Add(sr.color);
            }
            else if (!(r is ParticleSystemRenderer))
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor") || m.HasProperty("_Color"))
                    {
                        materiales.Add(m);
                        coloresMat.Add(m.color);
                    }
                }
            }
        }

        GameObject efecto = null;
        if (efectoBrillo != null)
            efecto = Instantiate(efectoBrillo, objetivo.transform.position, Quaternion.identity, objetivo.transform);

        // Parpadeo verde durante unos segundos (o hasta que la recojan)
        float t = 0f;
        while (t < duracionBrillo && objetivo != null && objetivo.activeInHierarchy)
        {
            t += Time.deltaTime;
            float f = Mathf.PingPong(t * 2f, 1f);
            for (int i = 0; i < sprites.Count; i++)
                if (sprites[i] != null) sprites[i].color = Color.Lerp(coloresSprite[i], colorBrillo, f);
            for (int i = 0; i < materiales.Count; i++)
                materiales[i].color = Color.Lerp(coloresMat[i], colorBrillo, f);
            yield return null;
        }

        // Volver a los colores originales
        for (int i = 0; i < sprites.Count; i++)
            if (sprites[i] != null) sprites[i].color = coloresSprite[i];
        for (int i = 0; i < materiales.Count; i++)
            if (materiales[i] != null) materiales[i].color = coloresMat[i];

        if (efecto != null) Destroy(efecto);

        estaSenalando = false;
        objetivoSenalado = null;
        rutinaBrillo = null;
    }

    // ================== MOVIMIENTO ==================

    private void Update()
    {
        if (jugador == null) return;

        RevisarMomentos();

        Vector3 movJugador = jugador.position - posAnteriorJugador;
        movJugador.y = 0f;
        posAnteriorJugador = jugador.position;
        if (Time.deltaTime > 0f && movJugador.magnitude / Time.deltaTime > 0.5f)
            direccionJugador = movJugador.normalized;

        float dx = transform.position.x - jugador.position.x;
        if (Mathf.Abs(dx) > 0.2f) lado = Mathf.Sign(dx);

        float lateralDeseado = desplazamientoLateral * Mathf.Abs(direccionJugador.z);
        lateralActual = Mathf.MoveTowards(lateralActual, lateralDeseado, 1.5f * Time.deltaTime);

        Vector3 ancla = jugador.position + Vector3.right * lado * lateralActual;

        Vector3 haciaAncla = ancla - transform.position;
        haciaAncla.y = 0f;
        float distancia = haciaAncla.magnitude;

        if (!siguiendo && distancia > distanciaSeguimiento + margenArranque) siguiendo = true;
        else if (siguiendo && distancia <= distanciaSeguimiento) siguiendo = false;

        Vector3 posAntes = transform.position;

        if (siguiendo)
        {
            Vector3 objetivo = ancla - haciaAncla.normalized * distanciaSeguimiento;
            objetivo.y = transform.position.y;

            float exceso = distancia - distanciaSeguimiento;
            float vel = Mathf.Max(velocidad, exceso * 3f);
            transform.position = Vector3.MoveTowards(transform.position, objetivo, vel * Time.deltaTime);
        }

        AjustarAlSuelo();

        Vector3 movDuende = transform.position - posAntes;
        movDuende.y = 0f;
        ActualizarAnimacion(movDuende);
    }

    private void ActualizarAnimacion(Vector3 movDuende)
    {
        if (Time.deltaTime <= 0f) return;

        Vector3 velActual = movDuende / Time.deltaTime;
        velSuavizada = Vector3.Lerp(velSuavizada, velActual, 8f * Time.deltaTime);
        float rapidez = velSuavizada.magnitude;

        if (!estaCaminando && rapidez > 1f) estaCaminando = true;
        else if (estaCaminando && rapidez < 0.3f) estaCaminando = false;

        if (estaCaminando)
        {
            float absX = Mathf.Abs(velSuavizada.x);
            float absZ = Mathf.Abs(velSuavizada.z);

            bool vaVertical;
            if (estadoActual == animLado) vaVertical = absZ > absX * 1.5f;
            else if (estadoActual == animAtras || estadoActual == animFrente) vaVertical = absZ * 1.5f > absX;
            else vaVertical = absZ >= absX;

            if (vaVertical)
            {
                CambiarAnimacion(velSuavizada.z > 0 ? animAtras : animFrente);
            }
            else
            {
                CambiarAnimacion(animLado);
                Voltear(velSuavizada.x);
            }
        }
        else if (estaSenalando && objetivoSenalado != null)
        {
            // Quieto y señalando la basura iluminada
            Voltear(objetivoSenalado.position.x - transform.position.x);
            CambiarAnimacion(animSenalando);
        }
        else
        {
            Voltear(jugador.position.x - transform.position.x);
            CambiarAnimacion(animIdle);
        }
    }

    private void Voltear(float x)
    {
        if (spriteDuende == null) return;
        if (x < -0.05f) spriteDuende.flipX = true;
        else if (x > 0.05f) spriteDuende.flipX = false;
    }

    private void CambiarAnimacion(string nuevoEstado)
    {
        if (animator == null || nuevoEstado == estadoActual) return;
        animator.Play(nuevoEstado);
        estadoActual = nuevoEstado;
    }

    private void AjustarAlSuelo()
    {
        float alturaInicio = Mathf.Max(transform.position.y, jugador.position.y) + 3f;
        Vector3 castPos = new Vector3(transform.position.x, alturaInicio, transform.position.z);

        RaycastHit hit;
        if (Physics.Raycast(castPos, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y + groundDist;
            transform.position = p;
        }
        else
        {
            Vector3 p = transform.position;
            p.y = jugador.position.y;
            transform.position = p;
        }
    }
}