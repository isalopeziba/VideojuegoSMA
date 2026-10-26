
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EstrellasManager : MonoBehaviour
{
    public static EstrellasManager Instancia { get; private set; }

    [Header("Configuracion")]
    [SerializeField, Min(1)]
    private int maxEstrellasPorNivel = 3;

    private int estrellasNivelActual = 0;
    private string idNivelActual;

    private const string PREFIJO_NIVEL = "EstrellasNivel_";
    private const string CLAVE_LISTA_NIVELES = "ListaNivelesConEstrellas";

    // Estrellas conseguidas en la escena actual durante esta partida
    public int EstrellasNivel => estrellasNivelActual;

    // Suma de las mejores puntuaciones guardadas de todos los niveles
    public int TotalEstrellas => CalcularTotalEstrellas();

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        idNivelActual = SceneManager.GetActiveScene().name;

        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void Start()
    {
        // Permite continuar una partida aunque el manager se cree
        // después de que la escena ya haya empezado a cargar.
        idNivelActual = SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        if (Instancia != this) return;

        SceneManager.sceneLoaded -= AlCargarEscena;
        Instancia = null;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        idNivelActual = escena.name;
        estrellasNivelActual = 0;
    }

    /// <summary>
    /// Suma una estrella al nivel actual, hasta un maximo de 3.
    /// Se puede llamar desde cualquier otro script.
    /// </summary>
    public void SumarEstrella()
    {
        SumarEstrellas(1);
    }

    /// <summary>
    /// Suma varias estrellas de una vez, sin superar el maximo del nivel.
    /// </summary>
    public void SumarEstrellas(int cantidad)
    {
        if (cantidad <= 0) return;

        int anterior = estrellasNivelActual;

        estrellasNivelActual = Mathf.Clamp(
            estrellasNivelActual + cantidad,
            0,
            maxEstrellasPorNivel
        );

        if (estrellasNivelActual == anterior) return;

        GuardarMejorPuntuacionNivel();

        Debug.Log(
            $"Nivel: {idNivelActual} | " +
            $"Estrellas del nivel: {estrellasNivelActual} | " +
            $"Total acumulado: {TotalEstrellas}"
        );
    }

    /// <summary>
    /// Devuelve la mejor puntuacion guardada de una escena.
    /// </summary>
    public int ObtenerEstrellasNivel(string nombreEscena)
    {
        return PlayerPrefs.GetInt(
            PREFIJO_NIVEL + nombreEscena, 0
        );
    }

    private void GuardarMejorPuntuacionNivel()
    {
        int mejorAnterior = ObtenerEstrellasNivel(idNivelActual);

        // Repetir un nivel no acumula estrellas duplicadas:
        // se conserva la mejor puntuacion conseguida.
        if (estrellasNivelActual > mejorAnterior)
        {
            PlayerPrefs.SetInt(
                PREFIJO_NIVEL + idNivelActual,
                estrellasNivelActual
            );

            RegistrarNivel(idNivelActual);
            PlayerPrefs.Save();
        }
    }

    private void RegistrarNivel(string nombreEscena)
    {
        string lista = PlayerPrefs.GetString(
            CLAVE_LISTA_NIVELES, ""
        );

        List<string> niveles = new List<string>();

        if (!string.IsNullOrEmpty(lista))
        {
            niveles.AddRange(lista.Split('|'));
        }

        if (!niveles.Contains(nombreEscena))
        {
            niveles.Add(nombreEscena);

            PlayerPrefs.SetString(
                CLAVE_LISTA_NIVELES,
                string.Join("|", niveles)
            );
        }
    }

    private int CalcularTotalEstrellas()
    {
        string lista = PlayerPrefs.GetString(
            CLAVE_LISTA_NIVELES, ""
        );

        if (string.IsNullOrEmpty(lista)) return 0;

        int total = 0;
        string[] niveles = lista.Split('|');

        foreach (string nivel in niveles)
        {
            if (string.IsNullOrEmpty(nivel)) continue;

            total += ObtenerEstrellasNivel(nivel);
        }

        return total;
    }
}
