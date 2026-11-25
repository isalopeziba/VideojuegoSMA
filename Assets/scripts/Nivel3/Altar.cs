using System.Collections;
using UnityEngine;


public class Altar : MonoBehaviour
{
    [Header("Configuración del altar")]
    public string nombreAltar;

    [Header("Reliquia")]
    public GameObject reliquia;
    public bool ofrendaColocada = false;

    [Header("Audio")]
    public AudioSource audioSource;
    [Tooltip("Sonido de poner la ofrenda (ej. la gota)")]
    public AudioClip sonidoOfrenda;
    [Tooltip("Voz del guardián como eco (Mohán en el altar de la gota, Patasola en el de la hoja)")]
    public AudioClip vozGuardian;
    [Tooltip("Segundos entre el sonido de la ofrenda y la voz")]
    public float retrasoVoz = 0.6f;
    [Range(0f, 1f)] public float volumenVoz = 1f;

    [Header("Brillo del altar")]
    [Tooltip("Luz o partículas del altar, desactivadas al inicio")]
    public GameObject brilloAltar;
    [Tooltip("Segundos que dura el brillo (0 = se queda encendido)")]
    public float duracionBrillo = 2.5f;
    public AudioClip sonidoBrillo;

    [Header("Efectos opcionales")]
    [Tooltip("La reliquia se hunde en el tronco y desaparece (para la gota)")]
    public bool hundirReliquia = false;
    public float distanciaHundir = 0.5f;
    public float duracionHundir = 1.2f;
    [Tooltip("Algo que brota al colocar la ofrenda (la rama de la Patasola), desactivado al inicio")]
    public GameObject brote;
    public float duracionBrote = 1f;

    public event System.Action AlCompletarOfrenda;

    // Cuánto dura todo el efecto de este altar (para que ControlAltares espere a que acabe la voz)
    public float DuracionEfecto =>
        retrasoVoz + Mathf.Max(duracionBrillo, vozGuardian != null ? vozGuardian.length : 0f);

    private void Start()
    {
        if (reliquia != null) reliquia.SetActive(false);
        if (brilloAltar != null) brilloAltar.SetActive(false);
        if (brote != null) brote.SetActive(false);

        if (audioSource == null)
            Debug.LogWarning($"Altar '{nombreAltar}': no tiene AudioSource asignado, no sonará nada.");
        if (vozGuardian == null)
            Debug.LogWarning($"Altar '{nombreAltar}': no tiene 'Voz Guardian' asignada.");
    }

    public void ColocarOfrenda()
    {
        if (ofrendaColocada) return;

        if (reliquia == null)
        {
            Debug.LogWarning("No hay reliquia asignada al altar: " + nombreAltar);
            return;
        }

        reliquia.SetActive(true);
        ofrendaColocada = true;

        // Sonido de la ofrenda (gota, hoja...)
        Reproducir(sonidoOfrenda, 1f);

        // Voz, brillo y efectos
        StartCoroutine(RutinaEfectos());

        AlCompletarOfrenda?.Invoke();

        Debug.Log("Ofrenda colocada en: " + nombreAltar);
    }

    private IEnumerator RutinaEfectos()
    {
        // La reliquia se hunde mientras suena la ofrenda
        if (hundirReliquia) StartCoroutine(Hundir());

        yield return new WaitForSeconds(retrasoVoz);

        // Voz del guardián
        Reproducir(vozGuardian, volumenVoz);

        // El altar brilla
        if (brilloAltar != null)
        {
            brilloAltar.SetActive(true);
            Reproducir(sonidoBrillo, 1f);
            if (duracionBrillo > 0f) StartCoroutine(ApagarBrillo());
        }

        // Brota la rama (si hay)
        if (brote != null) StartCoroutine(Brotar());
    }

    private IEnumerator ApagarBrillo()
    {
        yield return new WaitForSeconds(duracionBrillo);
        if (brilloAltar != null) brilloAltar.SetActive(false);
    }

    private IEnumerator Hundir()
    {
        Transform r = reliquia.transform;
        Vector3 inicio = r.position;
        Vector3 fin = inicio - Vector3.up * distanciaHundir;
        Vector3 escala = r.localScale;

        float t = 0f;
        while (t < duracionHundir)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duracionHundir);
            r.position = Vector3.Lerp(inicio, fin, p);
            r.localScale = Vector3.Lerp(escala, escala * 0.3f, p);
            yield return null;
        }

        reliquia.SetActive(false);
        r.position = inicio;
        r.localScale = escala;
    }

    private IEnumerator Brotar()
    {
        Vector3 escala = brote.transform.localScale;
        brote.transform.localScale = Vector3.zero;
        brote.SetActive(true);

        float t = 0f;
        while (t < duracionBrote)
        {
            t += Time.deltaTime;
            brote.transform.localScale = escala * Mathf.SmoothStep(0f, 1f, t / duracionBrote);
            yield return null;
        }
        brote.transform.localScale = escala;
    }

    private void Reproducir(AudioClip clip, float volumen)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip, volumen);
    }
}