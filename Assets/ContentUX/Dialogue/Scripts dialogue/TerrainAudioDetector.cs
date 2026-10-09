using UnityEngine;

public class TerrainAudioDetector : MonoBehaviour
{
    [Header("Audio Manager")]
    public AudioManager audioManager;

    [Header("Sonidos de Tierra")]
    public AudioClip[] tierraSFX;

    [Header("Sonidos de Hierba")]
    public AudioClip[] hierbaSFX;

    [Header("Sonidos de Piedra")]
    public AudioClip[] piedraSFX;

    [Header("Sonidos de Agua")]
    public AudioClip[] aguaSFX;

    [Header("Detección")]
    public float distanciaDeteccion = 2f;

    [Header("Pasos")]
    public float intervaloPasos = 0.45f;

    private string terrenoActual = "";
    private Rigidbody rb;
    private float tiempoSiguientePaso = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (audioManager == null)
        {
            Debug.LogError("TerrainAudioDetector: AudioManager NO está asignado.");
        }
    }

    void Update()
    {
        DetectarTerreno();

        if (EstaCaminando())
        {
            tiempoSiguientePaso -= Time.deltaTime;

            if (tiempoSiguientePaso <= 0f)
            {
                ReproducirPaso();
                tiempoSiguientePaso = intervaloPasos;
            }
        }
        else
        {
            tiempoSiguientePaso = 0f;
        }
    }

    void DetectarTerreno()
    {
        RaycastHit hit;

        Vector3 origen = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(
            origen,
            Vector3.down,
            out hit,
            distanciaDeteccion))
        {
            string nuevoTerreno = hit.collider.tag;

            if (nuevoTerreno != terrenoActual)
            {
                terrenoActual = nuevoTerreno;

                // Al cambiar de terreno, el siguiente paso
                // debe reproducirse inmediatamente.
                tiempoSiguientePaso = 0f;

                Debug.Log("Terreno detectado: " + terrenoActual);
            }
        }
    }

    bool EstaCaminando()
    {
        if (rb == null)
            return false;

        Vector3 velocidadHorizontal = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        return velocidadHorizontal.magnitude > 0.1f;
    }

    void ReproducirPaso()
    {
        if (audioManager == null)
        {
            Debug.LogError("No hay AudioManager asignado.");
            return;
        }

        AudioClip[] sonidos = ObtenerSonidosTerreno();

        if (sonidos == null || sonidos.Length == 0)
        {
            Debug.LogWarning(
                "No hay sonidos configurados para: " + terrenoActual
            );

            return;
        }

        AudioClip sonido = sonidos[
            Random.Range(0, sonidos.Length)
        ];

        if (sonido == null)
        {
            Debug.LogWarning("El AudioClip seleccionado es NULL.");
            return;
        }

        Debug.Log(
            "Intentando reproducir: " +
            sonido.name +
            " | Terreno: " +
            terrenoActual
        );

        audioManager.ReproducirSFX(sonido);
    }

    AudioClip[] ObtenerSonidosTerreno()
    {
        switch (terrenoActual)
        {
            case "Tierra":
                return tierraSFX;

            case "Hierba":
                return hierbaSFX;

            case "Piedra":
                return piedraSFX;

            case "Agua":
                return aguaSFX;

            default:
                return null;
        }
    }
}