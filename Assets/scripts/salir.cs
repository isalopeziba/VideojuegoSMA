using UnityEngine;
using System.Collections;

public class salir : MonoBehaviour
{
    [Header("Sonido del botón")]
    public AudioSource audioSource;
    public AudioClip sonidoClic;
    [Range(0f, 1f)] public float volumen = 1f;

    private bool saliendo = false;

    public void Salir()
    {
        if (saliendo) return;
        saliendo = true;
        StartCoroutine(RutinaSalir());
    }

    private IEnumerator RutinaSalir()
    {
        float espera = 0f;

        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic, volumen);
            espera = sonidoClic.length;
        }

        // Tiempo real: funciona aunque el juego esté en pausa (timeScale = 0)
        yield return new WaitForSecondsRealtime(espera);

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
