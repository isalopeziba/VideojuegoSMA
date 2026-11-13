using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class SalidaCueva : MonoBehaviour, IInteractuable
{
    [Header("Referencias")]
    public MochilaJugador mochila;
    public MohanCueva mohan;

    [Header("Salida")]
    public float esperaAntesDeSalir = 3f;
    [Tooltip("Nombre de la escena del Nivel 2 (déjalo vacío mientras no exista)")]
    public string escenaSiguiente = "";
    public UnityEvent alSalir;

    public event System.Action AlIntentarSalirSinSemillas;   // lo escucha el Duende

    // Resultado del nivel, para las estrellas
    public const string ClaveTieneSemillas = "Nivel1_TieneSemillas";
    public const string ClaveSoloSemillas = "Nivel1_SoloSemillas";
    public bool SalioSoloConSemillas { get; private set; }

    private bool saliendo = false;

    public bool PuedeInteractuar => !saliendo;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void Interactuar(InteraccionJugador jugador)
    {
        if (saliendo || mochila == null) return;

        // Sin semillas: la cascada lo detiene
        if (!mochila.TieneSemillas)
        {
            AlIntentarSalirSinSemillas?.Invoke();
            return;
        }

        StartCoroutine(RutinaSalir());
    }

    private IEnumerator RutinaSalir()
    {
        saliendo = true;
        SalioSoloConSemillas = mochila.SoloSemillas;

        if (SalioSoloConSemillas) mohan?.Sonreir();   // "Sabio es quien toma lo necesario."
        else mohan?.Asentir();                         // lo deja pasar sin la estrella

        GuardarResultado();

        yield return new WaitForSeconds(esperaAntesDeSalir);

        alSalir?.Invoke();

        if (!string.IsNullOrEmpty(escenaSiguiente))
            SceneManager.LoadScene(escenaSiguiente);
        else
            Debug.Log($"SalidaCueva: el jugador salió (solo semillas: {SalioSoloConSemillas}). " +
                      "No hay 'Escena Siguiente' asignada, así que no se cambia de escena.");
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