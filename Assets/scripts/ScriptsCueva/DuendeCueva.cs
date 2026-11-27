using UnityEngine;

// Va en el DUENDE (junto a DuendeNPC). Solo decide QUÉ dice el Duende dentro
// de la cueva; para hablar usa el Decir() que ya tiene DuendeNPC.
public class DuendeCueva : MonoBehaviour
{
    [Header("Referencias (las 4 son necesarias)")]
    public DuendeNPC duende;
    public MochilaJugador mochila;
    public EntradaCueva entrada;
    public SalidaCueva salida;

    [Header("Frases")]
    [TextArea(1, 3)] public string fraseEntrar = "Mucho brillo, ¿verdad? Piensa: ¿qué necesitas de verdad?";
    [TextArea(1, 3)] public string fraseComoSalir = "Cuando creas que tienes lo necesario, ve a la puerta y presiona Acción.";
    [TextArea(1, 3)] public string fraseAlgoTeFalta = "Algo te falta...";
    [TextArea(1, 3)] public string fraseRecordarSemillas = "¿Y las semillas? Las vas a necesitar en el bosque.";

    [Header("Pista dentro de la cueva")]
    [Tooltip("Si presiona pista y todavía NO lleva las semillas")]
    [TextArea(1, 3)] public string frasePistaElegir = "Aquí no hay basura. Elige solo lo que de verdad necesitas y luego ve a la puerta de la cueva.";
    [Tooltip("Si presiona pista y YA lleva las semillas")]
    [TextArea(1, 3)] public string frasePistaListo = "Ya tienes lo necesario. Ve a la puerta de la cueva y presiona Acción.";

    private bool yaRecordoConMochilaLlena = false;

    private void Awake()
    {
        if (duende == null) duende = GetComponent<DuendeNPC>();
    }

    private void OnEnable()
    {
        if (entrada != null) entrada.AlEntrarCueva += OnEntrar;
        else Debug.LogWarning("DuendeCueva: falta arrastrar la ENTRADA (objeto con EntradaCueva).");

        if (salida != null)
        {
            salida.AlIntentarSalirSinSemillas += OnSalirSinSemillas;
            salida.alSalir.AddListener(OnSalio);
        }
        else Debug.LogWarning("DuendeCueva: falta arrastrar la SALIDA (objeto con SalidaCueva).");

        if (mochila != null) mochila.AlCambiar += OnMochilaCambio;
        else Debug.LogWarning("DuendeCueva: falta arrastrar el jugador al campo 'Mochila'.");
    }

    private void OnDisable()
    {
        if (entrada != null) entrada.AlEntrarCueva -= OnEntrar;
        if (salida != null)
        {
            salida.AlIntentarSalirSinSemillas -= OnSalirSinSemillas;
            salida.alSalir.RemoveListener(OnSalio);
        }
        if (mochila != null) mochila.AlCambiar -= OnMochilaCambio;
        if (duende != null) duende.ObtenerFraseSinObjetivo = null;
    }

    private void OnEntrar()
    {
        duende.Decir(fraseEntrar, true);
        duende.Decir(fraseComoSalir);

        // Mientras esté en la cueva, la pista dice esto en vez de "ya está limpio"
        duende.ObtenerFraseSinObjetivo = FrasePistaCueva;
    }

    private void OnSalio()
    {
        // Afuera la pista vuelve a funcionar normal
        duende.ObtenerFraseSinObjetivo = null;
    }

    private string FrasePistaCueva()
    {
        return (mochila != null && mochila.TieneSemillas) ? frasePistaListo : frasePistaElegir;
    }

    private void OnSalirSinSemillas()
    {
        duende.Decir(fraseAlgoTeFalta, true);
        duende.Decir(fraseRecordarSemillas);
    }

    // Si llenó la mochila sin las semillas, se lo recuerda (una vez por cada vez que se llena)
    private void OnMochilaCambio()
    {
        if (!mochila.EstaLlena)
        {
            yaRecordoConMochilaLlena = false;
            return;
        }

        if (!mochila.TieneSemillas && !yaRecordoConMochilaLlena)
        {
            yaRecordoConMochilaLlena = true;
            duende.Decir(fraseRecordarSemillas, true);
        }
    }
}