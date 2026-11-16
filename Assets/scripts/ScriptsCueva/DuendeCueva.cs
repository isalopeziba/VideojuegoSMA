using UnityEngine;



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

    private bool yaRecordoConMochilaLlena = false;

    private void Awake()
    {
        if (duende == null) duende = GetComponent<DuendeNPC>();
    }

    private void OnEnable()
    {
        if (entrada != null) entrada.AlEntrarCueva += OnEntrar;
        else Debug.LogWarning("DuendeCueva: falta arrastrar la ENTRADA (objeto con EntradaCueva).");

        if (salida != null) salida.AlIntentarSalirSinSemillas += OnSalirSinSemillas;
        else Debug.LogWarning("DuendeCueva: falta arrastrar la SALIDA (objeto con SalidaCueva).");

        if (mochila != null) mochila.AlCambiar += OnMochilaCambio;
        else Debug.LogWarning("DuendeCueva: falta arrastrar el jugador al campo 'Mochila'.");
    }

    private void OnDisable()
    {
        if (entrada != null) entrada.AlEntrarCueva -= OnEntrar;
        if (salida != null) salida.AlIntentarSalirSinSemillas -= OnSalirSinSemillas;
        if (mochila != null) mochila.AlCambiar -= OnMochilaCambio;
    }

    private void OnEntrar()
    {
        duende.Decir(fraseEntrar, true);
        duende.Decir(fraseComoSalir);
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