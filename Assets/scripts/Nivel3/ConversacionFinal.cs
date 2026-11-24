
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Duración de los diálogos")]
    public float duracionMinima = 2f;
    public float segundosPorLetra = 0.06f;

    private Coroutine rutinaConversacion;
    private bool conversacionTerminada = false;

    private void Start()
    {
        OcultarGlobos();
    }

    // ================== INICIAR ==================

    public void IniciarConversacion()
    {
        if (rutinaConversacion != null || conversacionTerminada)
            return;

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

        // Finalizar conversación
        OcultarGlobos();

        conversacionTerminada = true;
        rutinaConversacion = null;
    }

    // ================== MOSTRAR DIÁLOGO ==================

    private IEnumerator MostrarDialogo(GloboDialogo globo, string frase)
    {
        OcultarGlobos();

        if (globo == null || globo.canvas == null || globo.texto == null)
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

    // ================== AYUDANTES ==================

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
