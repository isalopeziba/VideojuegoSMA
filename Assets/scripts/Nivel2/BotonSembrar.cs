using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BotonSembrar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Jugador")]
    public Transform jugador;
    [Tooltip("Opcional: el InteraccionJugador del player, para que se encienda la luz del lote")]
    public InteraccionJugador interaccion;

    [Header("Siembra")]
    [Tooltip("Pon el mismo valor que 'Tiempo Para Sembrar' de PlayerMov, para que sea igual que la E")]
    public float tiempoMantener = 2f;

    [Header("Opcional")]
    [Tooltip("Imagen 'Filled' que muestra el progreso mientras se mantiene")]
    public Image progreso;

    private bool presionado = false;
    private bool yaSembroEsteToque = false;
    private float tiempo = 0f;
    private PlantingSpot objetivo;

    private void Start()
    {
        if (jugador != null && interaccion == null)
            interaccion = jugador.GetComponent<InteraccionJugador>();
        MostrarProgreso(0f);
    }

    public void OnPointerDown(PointerEventData e)
    {
        presionado = true;
        yaSembroEsteToque = false;
        tiempo = 0f;
        objetivo = BuscarParche();
        if (interaccion != null) interaccion.AccionUI(true);   // enciende la luz del lote
    }

    public void OnPointerUp(PointerEventData e) { Soltar(); }
    public void OnPointerExit(PointerEventData e) { Soltar(); }

    private void Soltar()
    {
        presionado = false;
        tiempo = 0f;
        objetivo = null;
        if (interaccion != null) interaccion.AccionUI(false);
        MostrarProgreso(0f);
    }

    private void Update()
    {
        if (!presionado || yaSembroEsteToque) return;

        // Si no hay parche válido (o el joven se salió), buscar de nuevo y empezar de cero
        if (objetivo == null || !objetivo.PuedeSembrar() || !JugadorDentro(objetivo))
        {
            objetivo = BuscarParche();
            tiempo = 0f;
            MostrarProgreso(0f);
            if (objetivo == null) return;
        }

        tiempo += Time.deltaTime;
        MostrarProgreso(tiempo / tiempoMantener);

        if (tiempo >= tiempoMantener)
        {
            objetivo.Sembrar();
            yaSembroEsteToque = true;   // hay que soltar y volver a apretar para el siguiente
            tiempo = 0f;
            MostrarProgreso(0f);
            if (interaccion != null) interaccion.AccionUI(false);
        }
    }

    // El parche donde está parado el joven (dentro de su collider), que aún se pueda sembrar
    private PlantingSpot BuscarParche()
    {
        if (jugador == null) return null;

        PlantingSpot mejor = null;
        float mejorDist = float.MaxValue;
        foreach (PlantingSpot p in FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None))
        {
            if (!p.PuedeSembrar() || !JugadorDentro(p)) continue;
            float d = (p.transform.position - jugador.position).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = p; }
        }
        return mejor;
    }

    // Igual que con la E: el joven debe estar dentro del collider del parche (sin importar la altura)
    private bool JugadorDentro(PlantingSpot p)
    {
        Collider c = p.GetComponent<Collider>();
        if (c == null) return Vector3.Distance(p.transform.position, jugador.position) < 1.5f;

        Bounds b = c.bounds;
        Vector3 pos = jugador.position;
        return pos.x >= b.min.x && pos.x <= b.max.x && pos.z >= b.min.z && pos.z <= b.max.z;
    }

    private void MostrarProgreso(float valor)
    {
        if (progreso == null) return;
        progreso.fillAmount = Mathf.Clamp01(valor);
        progreso.enabled = valor > 0f;
    }
}
