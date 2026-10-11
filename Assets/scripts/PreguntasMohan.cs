using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class PreguntaMohan
{
    [TextArea(2, 4)] public string enunciado;
    [TextArea(1, 3)] public string[] opciones = new string[3];   // en orden A, B, C original
    [Tooltip("0 = A, 1 = B, 2 = C (según el orden original)")]
    public int correcta;
    [TextArea(2, 4)] public string textoAcierto;
    [TextArea(2, 4)] public string textoFallo;

    [Header("Pista del Duende")]
    [Tooltip("Opción INCORRECTA que el Duende descarta (0 = A, 1 = B, 2 = C)")]
    public int opcionQueDescarta;
    [TextArea(1, 3)] public string textoPista;
}

public class PreguntasMohan : MonoBehaviour
{
    [Header("Mohán")]
    public Text textoMohan;                 // texto del canvas sobre la cabeza del Mohán
    public Animator animatorMohan;
    public string animMohanIdle = "idle_mohan";
    public string animMohanSonrie = "mohan_sonriendo";

    [Header("Botones A, B, C")]
    public GameObject panelBotones;             
    public Button[] botones = new Button[3];    

    [Header("Duende (pista con la Q)")]
    public Text textoDuende;                // texto del canvas sobre la cabeza del Duende
    public float duracionTextoDuende = 4f;

    [Header("Basura que reaparece al fallar")]
    public List<GameObject> basurasError = new List<GameObject>(); 
    public int basurasPorFallo = 2;

    [Header("Barra del río (puntos fijos sobre 100)")]
    public float puntosAcierto = 4f;
    public float puntosFallo = 8f;

    [Header("Tiempos (segundos)")]
    public float esperaTrasAcierto = 3f;
    public float esperaTrasFallo = 3f;

    [Header("Final")]
    [TextArea(2, 4)] public string textoFinal = "Gracias, joven. Cuida mi río y cuéntaselo a los demás.";
    public UnityEvent alTerminar;               

    [Header("Preguntas")]
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

    // true mientras los botones están esperando respuesta (el Duende lo usa para la pista)
    public bool PreguntaActiva { get; private set; }

    private static readonly string[] letras = { "A", "B", "C" };
    private int indicePregunta = 0;
    private int[] ordenActual = { 0, 1, 2 };   
    private bool pistaUsada = false;
    private bool enCurso = false;
    private Coroutine rutinaDuende;

    private void Awake()
    {
       
        for (int i = 0; i < botones.Length; i++)
        {
            int indiceBoton = i;
            if (botones[i] != null)
                botones[i].onClick.AddListener(() => Responder(indiceBoton));
        }
    }

    private void Start()
    {
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoMohan != null) textoMohan.text = "";
        if (textoDuende != null) textoDuende.text = "";
    }

   

    // Se llama cuando la barra llega a 100 % (evento de BarraRio) o desde otro script
    
    public void IniciarPreguntas()
    {
        if (enCurso || preguntas.Count == 0) return;
        enCurso = true;
        indicePregunta = 0;
        MostrarPregunta(false);
    }

    // ================== MOSTRAR PREGUNTA ==================

    private void MostrarPregunta(bool mezclar)
    {
        PreguntaMohan p = preguntas[indicePregunta];

        if (mezclar) MezclarOrden();
        else ordenActual = new int[] { 0, 1, 2 };

        if (textoMohan != null) textoMohan.text = p.enunciado;

        for (int b = 0; b < botones.Length; b++)
        {
            if (botones[b] == null) continue;

            int opcion = ordenActual[b];
            Text texto = botones[b].GetComponentInChildren<Text>();
            if (texto != null) texto.text = p.opciones[opcion];

            MostrarBoton(b, true);
            botones[b].interactable = true;
        }

        if (panelBotones != null) panelBotones.SetActive(true);
        pistaUsada = false;
        PreguntaActiva = true;
    }

    // Cambia el orden de las opciones, asegurando que quede distinto al anterior
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

    

    private void Responder(int boton)
    {
        if (!PreguntaActiva) return;
        PreguntaActiva = false;

        foreach (Button b in botones) if (b != null) b.interactable = false;

        PreguntaMohan p = preguntas[indicePregunta];
        int opcionElegida = ordenActual[boton];

        if (opcionElegida == p.correcta) StartCoroutine(RutinaAcierto(p));
        else StartCoroutine(RutinaFallo(p));
    }

    private IEnumerator RutinaAcierto(PreguntaMohan p)
    {
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoMohan != null) textoMohan.text = p.textoAcierto;
        if (animatorMohan != null) animatorMohan.Play(animMohanSonrie, 0, 0f);
        if (BarraRio.Instancia != null) BarraRio.Instancia.Sumar(puntosAcierto);

        yield return new WaitForSeconds(esperaTrasAcierto);

        if (animatorMohan != null) animatorMohan.Play(animMohanIdle, 0, 0f);

        indicePregunta++;
        if (indicePregunta < preguntas.Count) MostrarPregunta(false);
        else Terminar();
    }

    private IEnumerator RutinaFallo(PreguntaMohan p)
    {
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoMohan != null) textoMohan.text = p.textoFallo;
        if (BarraRio.Instancia != null) BarraRio.Instancia.Restar(puntosFallo);

        yield return new WaitForSeconds(esperaTrasFallo);

        // Reaparecen basuras y se espera a que el jugador las recoja (se desactivan de nuevo)
        List<GameObject> activadas = ActivarBasuras();
        if (activadas.Count > 0)
        {
            yield return new WaitUntil(() => TodasRecogidas(activadas));
        }

        // Repetir la misma pregunta con las opciones en otro orden
        MostrarPregunta(true);
    }

    

    private List<GameObject> ActivarBasuras()
    {
        // Candidatas: las que están desactivadas
        List<GameObject> candidatas = new List<GameObject>();
        foreach (GameObject g in basurasError)
            if (g != null && !g.activeSelf) candidatas.Add(g);

        // Elegir al azar
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
            Debug.LogWarning("PreguntasMohan: no hay suficientes basuras desactivadas en la lista 'basurasError'.");

        return activadas;
    }

    private bool TodasRecogidas(List<GameObject> lista)
    {
        foreach (GameObject g in lista)
            if (g != null && g.activeSelf) return false;   // si fue destruida (null) también cuenta como recogida
        return true;
    }

    

    // La llama el Duende cuando se presiona Q durante una pregunta
    public void UsarPista()
    {
        if (!PreguntaActiva || pistaUsada) return;
        pistaUsada = true;

        PreguntaMohan p = preguntas[indicePregunta];

        // Buscar en qué botón quedó la opción a descartar y ocultarlo
        for (int b = 0; b < botones.Length; b++)
        {
            if (ordenActual[b] == p.opcionQueDescarta)
            {
                MostrarBoton(b, false);
                break;
            }
        }

        if (textoDuende != null)
        {
            if (rutinaDuende != null) StopCoroutine(rutinaDuende);
            rutinaDuende = StartCoroutine(RutinaTextoDuende(p.textoPista));
        }
    }

    private IEnumerator RutinaTextoDuende(string texto)
    {
        textoDuende.text = texto;
        yield return new WaitForSeconds(duracionTextoDuende);
        textoDuende.text = "";
    }

    // Oculta/muestra un botón sin desactivarlo (así no se mueve el layout)
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
        PreguntaActiva = false;
        enCurso = false;
        if (panelBotones != null) panelBotones.SetActive(false);
        if (textoMohan != null) textoMohan.text = textoFinal;
        alTerminar?.Invoke();
    }
}
