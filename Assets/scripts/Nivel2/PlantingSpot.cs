using UnityEngine;
using UnityEngine.InputSystem;


public class PlantingSpot : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject arbolPrefab;
    public Transform puntoDeSiembra;

    [Header("TreeCount")]
    public TreeCountingManager counterManager;

    [Header("Luz mientras se mantiene Acción")]
    public GameObject luzSiembra;

    private bool yaSembrado = false;
    private GameObject arbolSembrado;

    private bool jugadorDentro = false;
    private InteraccionJugador interaccion;   // para el botón de la manito en celular

    public bool YaSembrado => yaSembrado;

    private void Start()
    {
        if (luzSiembra != null) luzSiembra.SetActive(false);
    }

    public bool PuedeSembrar()
    {
        return !yaSembrado &&
               arbolPrefab != null &&
               counterManager != null &&
               counterManager.PuedePlantar;
    }

    private void Update()
    {
        if (luzSiembra == null) return;

        bool presionando =
            (Keyboard.current != null && Keyboard.current.eKey.isPressed) ||
            (interaccion != null && interaccion.AccionMantenida);

        bool encender = jugadorDentro && presionando && PuedeSembrar();
        if (luzSiembra.activeSelf != encender) luzSiembra.SetActive(encender);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null)
        {
            jugador.EntrarParche(this);
            jugadorDentro = true;
            interaccion = other.GetComponentInParent<InteraccionJugador>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null)
        {
            jugador.SalirParche(this);
            jugadorDentro = false;
        }
    }

    public void Sembrar()
    {
        if (!PuedeSembrar()) return;

        Vector3 posicion = puntoDeSiembra != null
            ? puntoDeSiembra.position
            : transform.position;

        arbolSembrado = Instantiate(
            arbolPrefab,
            posicion,
            Quaternion.identity
        );

        Animator animArbol = arbolSembrado.GetComponent<Animator>();

        if (animArbol != null)
        {
            animArbol.enabled = true;
            animArbol.Play(0, 0, 0f);
        }

        yaSembrado = true;
        if (luzSiembra != null) luzSiembra.SetActive(false);

        // Registrar el árbol en su zona
        counterManager.RegistrarArbol();

        Debug.Log("Árbol sembrado correctamente");
    }

    // La llama la zona cuando la Patasola llega antes de terminar
    public void Reiniciar()
    {
        if (arbolSembrado != null) Destroy(arbolSembrado);
        arbolSembrado = null;
        yaSembrado = false;
        if (luzSiembra != null) luzSiembra.SetActive(false);
    }
}