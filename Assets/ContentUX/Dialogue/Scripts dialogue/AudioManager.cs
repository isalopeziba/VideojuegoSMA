using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicaExploracion;
    public AudioSource musicaTension;
    public AudioSource sfxSource;

    private void Start()
    {
        if (musicaExploracion != null)
        {
            musicaExploracion.Play();
        }

        if (musicaTension != null)
        {
            musicaTension.Stop();
        }
    }

    public void ActivarTension()
    {
        if (musicaExploracion != null)
            musicaExploracion.Stop();

        if (musicaTension != null)
            musicaTension.Play();
    }

    public void ActivarExploracion()
    {
        if (musicaTension != null)
            musicaTension.Stop();

        if (musicaExploracion != null)
            musicaExploracion.Play();
    }

    public void ReproducirSFX(AudioClip sonido)
    {
        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: SFX Source NO está asignado.");
            return;
        }

        if (sonido == null)
        {
            Debug.LogWarning("AudioManager: El AudioClip recibido es NULL.");
            return;
        }

        // Detener cualquier SFX que esté sonando
        sfxSource.Stop();

        // Asignar el nuevo sonido
        sfxSource.clip = sonido;

        // Reproducirlo desde el inicio
        sfxSource.Play();

        Debug.Log("SFX reproducido: " + sonido.name);
    }
}