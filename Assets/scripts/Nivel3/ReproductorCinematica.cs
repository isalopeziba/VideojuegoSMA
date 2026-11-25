using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// Va en la escena de la CINEMÁTICA FINAL, en el objeto que tiene el VideoPlayer.
// Reproduce el video al entrar y, cuando termina (o se salta), muestra las estadísticas:
// activa un panel de esta escena o carga una escena de estadísticas.
[RequireComponent(typeof(VideoPlayer))]
public class ReproductorCinematica : MonoBehaviour
{
    [Header("Al terminar el video")]
    [Tooltip("Opción A: panel de estadísticas en esta misma escena (desactivado al inicio)")]
    public GameObject panelEstadisticas;
    [Tooltip("Opción B: nombre de una escena de estadísticas (vacío si usas el panel)")]
    public string escenaEstadisticas = "";
    [Tooltip("Para conectar otras cosas al terminar el video")]
    public UnityEvent alTerminarVideo;

    [Header("Saltar (opcional)")]
    [Tooltip("Botón para saltar la cinemática")]
    public Button botonSaltar;

    private VideoPlayer video;
    private bool terminado = false;

    private void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.playOnAwake = false;
        video.isLooping = false;
        video.loopPointReached += AlTerminarVideo;

        if (panelEstadisticas != null) panelEstadisticas.SetActive(false);
        if (botonSaltar != null) botonSaltar.onClick.AddListener(Saltar);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (video.clip == null && string.IsNullOrEmpty(video.url))
        {
            Debug.LogWarning("ReproductorCinematica: el VideoPlayer no tiene video asignado.");
            Terminar();
            return;
        }
        video.Play();
    }

    private void AlTerminarVideo(VideoPlayer vp) { Terminar(); }

    public void Saltar()
    {
        if (video != null) video.Stop();
        Terminar();
    }

    private void Terminar()
    {
        if (terminado) return;
        terminado = true;

        if (botonSaltar != null) botonSaltar.gameObject.SetActive(false);
        alTerminarVideo?.Invoke();

        if (panelEstadisticas != null)
            panelEstadisticas.SetActive(true);
        else if (!string.IsNullOrEmpty(escenaEstadisticas))
            SceneManager.LoadScene(escenaEstadisticas);
        else
            Debug.Log("ReproductorCinematica: el video terminó, pero no hay panel ni escena de estadísticas asignados.");
    }

    private void OnDestroy()
    {
        if (video != null) video.loopPointReached -= AlTerminarVideo;
    }
}
