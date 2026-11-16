using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Va en la ENTRADA de la cueva (mismo objeto y collider "Is Trigger" que EntradaCueva).
// Al presionar Acción, revisa qué lleva el joven y decide si puede salir.
[RequireComponent(typeof(Collider))]
public class SalidaCueva : MonoBehaviour, IInteractuable
{
    [Header("Referencias")]
    public MochilaJugador mochila;
    public MohanCueva mohan;

    [Header("Salida")]
    [Tooltip("Normalmente vacío: el cambio de escena lo hace el portal")]
    public string escenaSiguiente = "";
    [Tooltip("Abrir la puerta, preparar la ceremonia...")]
    public UnityEvent alSalir;

    public event System.Action AlIntentarSalirSinSemillas;   // lo escucha el Duende

    // Resultado del nivel, para las estrellas
    public const string ClaveTieneSemillas = "Nivel1_TieneSemillas";
    public const string ClaveSoloSemillas = "Nivel1_SoloSemillas";
    public bool SalioSoloConSemillas { get; private set; }

    private bool salio = false;

    public bool PuedeInteractuar => !salio;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (mochila == null)
            Debug.LogWarning("SalidaCueva: falta arrastrar el jugador al campo 'Mochila'. Sin eso no se puede salir.");
    }

    public void Interactuar(InteraccionJugador jugador)
    {
        if (salio) return;
        if (mochila == null) { Debug.LogWarning("SalidaCueva: falta la referencia 'Mochila'."); return; }

        // Sin semillas: no puede salir
        if (!mochila.TieneSemillas)
        {
            Debug.Log("SalidaCueva: intentó salir SIN semillas.");
            AlIntentarSalirSinSemillas?.Invoke();
            return;
        }

        salio = true;
        SalioSoloConSemillas = mochila.SoloSemillas;
        Debug.Log($"SalidaCueva: sale con semillas (solo semillas: {SalioSoloConSemillas}).");

        if (SalioSoloConSemillas) mohan?.Sonreir();   // "Sabio es quien toma lo necesario."
        else mohan?.Asentir();

        GuardarResultado();

        // Se abre la puerta enseguida, para que el joven no quede trabado esperando
        alSalir?.Invoke();

        if (!string.IsNullOrEmpty(escenaSiguiente))
            SceneManager.LoadScene(escenaSiguiente);
    }

    private void GuardarResultado()
    {
        PlayerPrefs.SetInt(ClaveTieneSemillas, 1);
        PlayerPrefs.SetInt(ClaveSoloSemillas, SalioSoloConSemillas ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnTriggerEnter(Collider other) { InteraccionJugador.AlEntrarTrigger(this, other); }
    private void OnTriggerExit(Collider other) { InteraccionJugador.AlSalirTrigger(this, other); }
}