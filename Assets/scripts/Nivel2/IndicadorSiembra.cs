using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Va en cada PARCHE (el mismo objeto que tiene PlantingSpot y su collider trigger).
// Muestra encima del parche:
//   - un aviso "Mantén presionado" cuando el joven entra y aún no aprieta,
//   - un círculo que se va llenando mientras mantiene E o la manito.
public class IndicadorSiembra : MonoBehaviour
{
    [Header("Referencias")]
    public PlantingSpot parche;
    [Tooltip("Canvas World Space encima del parche (desactivado al inicio)")]
    public GameObject contenedor;
    [Tooltip("Image con Image Type = Filled y Fill Method = Radial 360")]
    public Image circuloCarga;
    [Tooltip("Opcional: texto o imagen de 'Mantén presionado'")]
    public GameObject avisoMantener;

    [Header("Tiempo")]
    [Tooltip("Igual que 'Tiempo Para Sembrar' de PlayerMov (y 'Tiempo Mantener' de BotonSembrar). " +
             "Si es 0, se toma solo del PlayerMov.")]
    public float tiempoTotal = 0f;

    private bool jugadorDentro = false;
    private PlayerMov jugadorMov;
    private InteraccionJugador interaccion;
    private float tiempo = 0f;

    private void Awake()
    {
        if (parche == null) parche = GetComponent<PlantingSpot>();
        Mostrar(false, false, 0f);
    }

    private void Update()
    {
        bool puede = jugadorDentro && parche != null && parche.PuedeSembrar();

        if (!puede)
        {
            tiempo = 0f;
            Mostrar(false, false, 0f);
            return;
        }

        bool presionando =
            (Keyboard.current != null && Keyboard.current.eKey.isPressed) ||
            (interaccion != null && interaccion.AccionMantenida);

        if (presionando)
        {
            float total = tiempoTotal > 0f ? tiempoTotal
                        : (jugadorMov != null ? jugadorMov.tiempoParaSembrar : 2f);

            tiempo += Time.deltaTime;
            Mostrar(true, false, Mathf.Clamp01(tiempo / Mathf.Max(0.01f, total)));
        }
        else
        {
            tiempo = 0f;
            Mostrar(true, true, 0f);   // solo el aviso de "mantén presionado"
        }
    }

    private void Mostrar(bool visible, bool soloAviso, float progreso)
    {
        if (contenedor != null && contenedor.activeSelf != visible) contenedor.SetActive(visible);
        if (avisoMantener != null) avisoMantener.SetActive(visible && soloAviso);
        if (circuloCarga != null)
        {
            circuloCarga.gameObject.SetActive(visible && !soloAviso);
            circuloCarga.fillAmount = progreso;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMov mov = other.GetComponent<PlayerMov>();
        if (mov == null) return;
        jugadorDentro = true;
        jugadorMov = mov;
        interaccion = other.GetComponentInParent<InteraccionJugador>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMov>() == null) return;
        jugadorDentro = false;
        tiempo = 0f;
    }
}
