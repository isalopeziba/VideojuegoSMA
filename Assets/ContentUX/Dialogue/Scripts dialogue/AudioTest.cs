using UnityEngine;
using UnityEngine.InputSystem;

public class AudioTest : MonoBehaviour
{
    public AudioManager audioManager;
    public AudioClip sfxPrueba;

    private void Update()
    {
        // T = música de tensión
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            audioManager.ActivarTension();
        }

        // E = música de exploración
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            audioManager.ActivarExploracion();
        }

        // F = efecto de sonido
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            audioManager.ReproducirSFX(sfxPrueba);
        }
    }
}