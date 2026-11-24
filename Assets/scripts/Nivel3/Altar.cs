
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Altar : MonoBehaviour
{
    [Header("Configuración del altar")]
    public string nombreAltar;

    [Header("Reliquia")]
    public GameObject reliquia;
    public bool ofrendaColocada = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoOfrenda;

    [Header("Diálogo")]
    public GameObject canvasDialogo;
    public Text textoDialogo;

    [TextArea(2, 5)]
    public string dialogoOfrenda = "La reliquia ha sido entregada.";

    public float duracionMinimaFrase = 3f;
    public float segundosPorLetra = 0.06f;

    public event System.Action AlCompletarOfrenda;

    private Coroutine rutinaDialogo;

    private void Start()
    {
        if (reliquia != null)
            reliquia.SetActive(false);

        if (canvasDialogo != null)
            canvasDialogo.SetActive(false);
    }

    public void ColocarOfrenda()
    {
        if (ofrendaColocada) return;

        if (reliquia == null)
        {
            Debug.LogWarning("No hay reliquia asignada al altar: " + nombreAltar);
            return;
        }

        // Activar reliquia
        reliquia.SetActive(true);

        // Marcar altar como completado
        ofrendaColocada = true;
        AlCompletarOfrenda?.Invoke();

        // Reproducir sonido
        if (audioSource != null && sonidoOfrenda != null)
        {
            audioSource.PlayOneShot(sonidoOfrenda);
        }

        // Mostrar diálogo
        MostrarDialogo();

        Debug.Log("Ofrenda colocada en: " + nombreAltar);
    }

    private void MostrarDialogo()
    {
        if (textoDialogo == null) return;

        if (rutinaDialogo != null)
            StopCoroutine(rutinaDialogo);

        rutinaDialogo = StartCoroutine(RutinaDialogo());
    }

    private IEnumerator RutinaDialogo()
    {
        textoDialogo.text = dialogoOfrenda;

        if (canvasDialogo != null)
            canvasDialogo.SetActive(true);

        float duracion = Mathf.Max(
            duracionMinimaFrase,
            dialogoOfrenda.Length * segundosPorLetra
        );

        yield return new WaitForSeconds(duracion);

        if (canvasDialogo != null)
            canvasDialogo.SetActive(false);

        textoDialogo.text = "";

        rutinaDialogo = null;
    }
}
