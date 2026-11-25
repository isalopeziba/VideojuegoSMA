using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConversacionFinal : MonoBehaviour
{
    [System.Serializable]
    public class GloboDialogo
    {
        public GameObject canvas;
        public Text texto;
    }

    [Header("Globos de diálogo")]
    public GloboDialogo globoJugador;
    public GloboDialogo globoMadremonte;
    public GloboDialogo globoDuende;

    [Header("Diálogos del jugador")]
    [TextArea(2, 5)]
    public string dialogoJugador1;

    [TextArea(2, 5)]
    public string dialogoJugador2;

    [Header("Diálogos de la Madremonte")]
    [TextArea(2, 5)]
    public string dialogoMadremonte1;

    [TextArea(2, 5)]
    public string dialogoMadremonte2;

    [Header("Diálogos del Duende")]
    [TextArea(2, 5)]
    public string dialogoDuende1;

    [TextArea(2, 5)]
    public string dialogoDuende2;

    [TextArea(2, 5)]
    public string dialogoDuende3;

    [Header("Duración de los diálogos")]
    public float duracionMinima = 2f;
    public float segundosPorLetra = 0.06f;

    [Header("Al terminar")]
    [Tooltip("Portal hacia la cinemática final. Empieza oculto y aparece SOLO al terminar la conversación.")]
    public GameObject portal;
    [Tooltip("Para conectar otras cosas al final (guardar ficha, estrellas, etc.)")]
    public UnityEvent alTerminar;

    private Coroutine rutinaConversacion;
    private bool conversacionTerminada = false;

    private void Start()
    {
        OcultarGlobos();
        if (portal != null) portal.SetActive(false);   // el portal empieza oculto
    }

    // ================== INICIAR ==================

    public void IniciarConversacion()
    {
        if (rutinaConversacion != null || conversacionTerminada)
            return;

        if (portal != null) portal.SetActive(false);

        rutinaConversacion = StartCoroutine(RutinaConversacion());
    }

    // ================== CONVERSACIÓN ==================

    private IEnumerator RutinaConversacion()
    {
        // 1. Jugador
        yield return MostrarDialogo(globoJugador, dialogoJugador1);

        // 2. Madremonte
        yield return MostrarDialogo(globoMadremonte, dialogoMadremonte1);

        // 3. Madremonte
        yield return MostrarDialogo(globoMadremonte, dialogoMadremonte2);

        // 4. Duende
        yield return MostrarDialogo(globoDuende, dialogoDuende1);

        // 5. Jugador
        yield return MostrarDialogo(globoJugador, dialogoJugador2);

        // 6. Duende
        yield return MostrarDialogo(globoDuende, dialogoDuende2);

        // 7. Duende
        yield return MostrarDialogo(globoDuende, dialogoDuende3);

        // Finalizar conversación
        OcultarGlobos();

        conversacionTerminada = true;
        rutinaConversacion = null;

        
        if (portal != null) portal.SetActive(true);
        

        alTerminar?.Invoke();
    }

    // ================== MOSTRAR DIÁLOGO ==================

    private IEnumerator MostrarDialogo(GloboDialogo globo, string frase)
    {
        OcultarGlobos();

        if (globo == null || globo.canvas == null || globo.texto == null || string.IsNullOrEmpty(frase))
            yield break;

        globo.texto.text = frase;
        globo.canvas.SetActive(true);

        float duracion = Mathf.Max(
            duracionMinima,
            frase.Length * segundosPorLetra
        );

        yield return new WaitForSeconds(duracion);

        globo.canvas.SetActive(false);
        globo.texto.text = "";
    }

   
    private void OcultarGlobos()
    {
        Ocultar(globoJugador);
        Ocultar(globoMadremonte);
        Ocultar(globoDuende);
    }

    private void Ocultar(GloboDialogo globo)
    {
        if (globo == null) return;

        if (globo.canvas != null)
            globo.canvas.SetActive(false);

        if (globo.texto != null)
            globo.texto.text = "";
    }
}