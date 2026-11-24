using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class ControladorSecuenciaVideos : MonoBehaviour
{
    [Header("Lista de Videos en Orden")]
    [Tooltip("Arrastra aquí escena_0_v2 en el elemento 0 y escena_1_v2 en el elemento 1")]
    public VideoClip[] listaVideos;

    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena a cargar tras el último video")]
    public string nombreSiguienteEscena;

    [Header("Opcional: Saltar video actual")]
    public bool permitirSaltarConClic = false;

    private VideoPlayer videoPlayer;
    private int indiceActual = 0;
    private bool cambioIniciado = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        // Nos aseguramos de que no intente reproducir antes de cargar el primer clip de la lista
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
    }

    private void OnEnable()
    {
        videoPlayer.loopPointReached += AlFinalizarVideoActual;
    }

    private void OnDisable()
    {
        videoPlayer.loopPointReached -= AlFinalizarVideoActual;
    }

    private void Start()
    {
        if (listaVideos != null && listaVideos.Length > 0)
        {
            ReproducirVideo(0);
        }
        else
        {
            Debug.LogWarning("No hay videos asignados en 'Lista de Videos'. Saltando a la siguiente escena.");
            CargarSiguienteEscena();
        }
    }

    private void Update()
    {
        if (permitirSaltarConClic && !cambioIniciado && Input.GetMouseButtonDown(0))
        {
            AvanzarSiguientePaso();
        }
    }

    private void ReproducirVideo(int indice)
    {
        indiceActual = indice;
        videoPlayer.clip = listaVideos[indiceActual];
        videoPlayer.Play();
    }

    private void AlFinalizarVideoActual(VideoPlayer vp)
    {
        AvanzarSiguientePaso();
    }

    private void AvanzarSiguientePaso()
    {
        int siguienteIndice = indiceActual + 1;

        // Si todavía quedan videos en la lista, reproduce el siguiente
        if (siguienteIndice < listaVideos.Length)
        {
            ReproducirVideo(siguienteIndice);
        }
        else
        {
            // Ya terminó el último video de la lista
            CargarSiguienteEscena();
        }
    }

    private void CargarSiguienteEscena()
    {
        if (cambioIniciado) return;
        cambioIniciado = true;

        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogError("Falta asignar 'nombreSiguienteEscena' en el Inspector de ControladorSecuenciaVideos.");
        }
    }
}