using System.Collections;
using UnityEngine;

// Cuando las dos ofrendas están en el altar: espera a que terminen las voces,
// todo el claro brilla (luz + sonido) y luego aparece la Madremonte con su pregunta.
public class ControlAltares : MonoBehaviour
{
    [Header("Altares")]
    public Altar altar1;
    public Altar altar2;

    [Header("Madremonte")]
    public MadremontePreguntas madremonte;

    [Header("El claro brilla")]
    [Tooltip("Luces/partículas de todo el claro, desactivadas al inicio")]
    public GameObject brilloClaro;
    public AudioSource audioSource;
    public AudioClip sonidoClaro;
    [Tooltip("Segundos que se ve el claro brillando antes de que aparezca la Madremonte")]
    public float esperaAntesDeMadremonte = 2.5f;

    private bool ritualCompletado = false;

    private void Start()
    {
        if (brilloClaro != null) brilloClaro.SetActive(false);

        if (altar1 != null)
            altar1.AlCompletarOfrenda += RevisarAltares;

        if (altar2 != null)
            altar2.AlCompletarOfrenda += RevisarAltares;

        // Por si los altares ya estaban completados
        RevisarAltares();
    }

    private void OnDestroy()
    {
        if (altar1 != null)
            altar1.AlCompletarOfrenda -= RevisarAltares;

        if (altar2 != null)
            altar2.AlCompletarOfrenda -= RevisarAltares;
    }

    private void RevisarAltares()
    {
        if (ritualCompletado) return;

        if (altar1 == null || altar2 == null) return;

        if (altar1.ofrendaColocada && altar2.ofrendaColocada)
        {
            ritualCompletado = true;
            StartCoroutine(RutinaRitual());
        }
    }

    private IEnumerator RutinaRitual()
    {
        // 1. Esperar a que termine la voz del último altar (Mohán o Patasola)
        float espera = Mathf.Max(altar1.DuracionEfecto, altar2.DuracionEfecto);
        yield return new WaitForSeconds(espera);

        // 2. Todo el claro brilla
        if (brilloClaro != null) brilloClaro.SetActive(true);
        if (audioSource != null && sonidoClaro != null) audioSource.PlayOneShot(sonidoClaro);
        Debug.Log("ControlAltares: las dos ofrendas están en el altar, el claro brilla.");

        yield return new WaitForSeconds(esperaAntesDeMadremonte);

        // 3. Aparece la Madremonte con su pregunta
        ActivarMadremonte();
    }

    private void ActivarMadremonte()
    {
        if (madremonte == null) return;

        // Activar el objeto de la Madremonte
        madremonte.gameObject.SetActive(true);

        // Comenzar la pregunta
        madremonte.IniciarPregunta();
    }
}