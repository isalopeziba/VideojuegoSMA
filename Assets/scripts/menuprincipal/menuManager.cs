using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class menuManager : MonoBehaviour
{
    [Header("Sonido de los botones")]
    public AudioSource audioSource;
    public AudioClip sonidoClic;
    [Range(0f, 1f)] public float volumen = 1f;

    private bool cambiando = false;

    // Función pública para que el botón pueda detectarla
    public void IrAEscena(string nombreEscena)
    {
        if (cambiando) return;          // evita que se presione dos veces
        cambiando = true;
        StartCoroutine(RutinaIrAEscena(nombreEscena));
    }

    private IEnumerator RutinaIrAEscena(string nombreEscena)
    {
        float espera = 0f;

        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic, volumen);
            espera = sonidoClic.length;
        }

        // Espera a que termine el sonido antes de cambiar de escena
        yield return new WaitForSecondsRealtime(espera);

        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
}
