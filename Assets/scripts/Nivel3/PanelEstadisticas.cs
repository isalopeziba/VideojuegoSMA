using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class FichaResultado
{
    [Tooltip("Mohan, Patasola o Madremonte")]
    public string id;
    [Tooltip("Imagen de la ficha")]
    public Image imagen;
    [Tooltip("Texto debajo de la ficha con lo que se aprendió")]
    public Text textoAprendizaje;
    [TextArea(1, 2)] public string aprendizaje;
}

// Va en el PANEL FINAL de resultados (desactivado; lo activa ReproductorCinematica al acabar el video).
// Al mostrarse: llena las estrellas ganadas una por una, muestra las fichas y lo aprendido.
// El título y el mensaje final se ponen a mano como textos normales.
public class PanelEstadisticas : MonoBehaviour
{
    [Header("Estrellas (6 = 3 del Nivel 1 + 3 del Nivel 2, en orden)")]
    public Image[] estrellas = new Image[6];
    public Sprite estrellaLlena;
    public Sprite estrellaVacia;
    [Tooltip("Opcional: texto tipo '5 / 6'")]
    public Text textoTotalEstrellas;
    public float pausaEntreEstrellas = 0.35f;
    public AudioSource audioSource;
    public AudioClip sonidoEstrella;

    [Header("Fichas de las leyendas")]
    public FichaResultado[] fichas =
    {
        new FichaResultado { id = ColeccionFichas.Mohan,      aprendizaje = "Toma solo lo necesario" },
        new FichaResultado { id = ColeccionFichas.Patasola,   aprendizaje = "Cuida el bosque y a quienes viven en él" },
        new FichaResultado { id = ColeccionFichas.Madremonte, aprendizaje = "Repara lo que dañas" }
    };
    [Tooltip("Color de una ficha que no se ganó")]
    public Color colorFichaBloqueada = new Color(0.3f, 0.3f, 0.3f, 0.6f);
    public string textoFichaBloqueada = "???";

    [Header("Botones")]
    public Button botonJugarDeNuevo;
    [Tooltip("Escena con la que empieza el juego (menú o Nivel 1)")]
    public string escenaInicio = "";
    public Button botonSalir;

    private void Awake()
    {
        if (botonJugarDeNuevo != null) botonJugarDeNuevo.onClick.AddListener(JugarDeNuevo);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
        MostrarFichas();
        StartCoroutine(MostrarEstrellas());
    }

    // ================== ESTRELLAS ==================

    private IEnumerator MostrarEstrellas()
    {
        int nivel1 = ProgresoEstrellas.ObtenerNivel(1);
        int nivel2 = ProgresoEstrellas.ObtenerNivel(2);

        // Primero todas vacías
        foreach (Image img in estrellas)
            if (img != null) { img.sprite = estrellaVacia; img.transform.localScale = Vector3.one; }

        if (textoTotalEstrellas != null)
            textoTotalEstrellas.text = $"0 / {estrellas.Length}";

        yield return new WaitForSecondsRealtime(0.5f);

        // Luego se llenan las ganadas, una por una
        int contador = 0;
        for (int i = 0; i < estrellas.Length; i++)
        {
            bool esNivel1 = i < 3;
            int posEnNivel = esNivel1 ? i : i - 3;
            bool ganada = esNivel1 ? posEnNivel < nivel1 : posEnNivel < nivel2;
            if (!ganada || estrellas[i] == null) continue;

            contador++;
            estrellas[i].sprite = estrellaLlena;
            if (audioSource != null && sonidoEstrella != null) audioSource.PlayOneShot(sonidoEstrella);
            if (textoTotalEstrellas != null) textoTotalEstrellas.text = $"{contador} / {estrellas.Length}";

            yield return Rebote(estrellas[i].transform);
            yield return new WaitForSecondsRealtime(pausaEntreEstrellas);
        }
    }

    // Pequeño "pop" al llenarse
    private IEnumerator Rebote(Transform t)
    {
        float d = 0.2f, tiempo = 0f;
        while (tiempo < d)
        {
            tiempo += Time.unscaledDeltaTime;
            float s = 1f + Mathf.Sin(tiempo / d * Mathf.PI) * 0.3f;
            t.localScale = Vector3.one * s;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    // ================== FICHAS ==================

    private void MostrarFichas()
    {
        foreach (FichaResultado f in fichas)
        {
            bool tiene = ColeccionFichas.Tiene(f.id);

            if (f.imagen != null)
                f.imagen.color = tiene ? Color.white : colorFichaBloqueada;

            if (f.textoAprendizaje != null)
                f.textoAprendizaje.text = tiene ? f.aprendizaje : textoFichaBloqueada;
        }
    }

    // ================== BOTONES ==================

    public void JugarDeNuevo()
    {
        // Borrar el progreso para empezar de cero
        ProgresoEstrellas.Reiniciar();
        ColeccionFichas.Reiniciar();
        PlayerPrefs.DeleteKey(SalidaCueva.ClaveTieneSemillas);
        PlayerPrefs.DeleteKey(SalidaCueva.ClaveSoloSemillas);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(escenaInicio)) SceneManager.LoadScene(escenaInicio);
        else SceneManager.LoadScene(0);   // la primera escena de Build Profiles
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
