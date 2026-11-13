using UnityEngine;

public enum TipoObjetoCueva
{
    Semillas,   // bolsa de semillas: lo único necesario
    Tesoro,     // copa, collar, máscara
    SinValor    // concha, piedra lisa
}

// Va en cada OBJETO de la cueva. Necesita un Collider marcado como "Is Trigger"
// un poco más grande que el objeto, para que el jugador pueda tomarlo al acercarse.
[RequireComponent(typeof(Collider))]
public class ObjetoCueva : MonoBehaviour, IInteractuable
{
    public string nombre = "Objeto";
    public TipoObjetoCueva tipo = TipoObjetoCueva.SinValor;
    [Tooltip("Imagen que se muestra en la casilla de la mochila")]
    public Sprite icono;
    [Tooltip("Si se deja vacío, se pone en el pedestal libre más cercano al iniciar")]
    public PedestalCueva pedestalInicial;

    public PedestalCueva PedestalActual { get; private set; }

    public bool PuedeInteractuar => isActiveAndEnabled && PedestalActual != null;

    private void Start()
    {
        PedestalCueva p = pedestalInicial != null ? pedestalInicial : PedestalCueva.MasCercanoLibre(transform.position);
        if (p != null) ColocarEn(p);
        else Debug.LogWarning($"ObjetoCueva: '{nombre}' no encontró pedestal libre.");
    }

    public void Interactuar(InteraccionJugador jugador)
    {
        if (jugador.Mochila != null) jugador.Mochila.Tomar(this);
    }

    // Lo llama la mochila al tomarlo
    public void Guardar()
    {
        if (PedestalActual != null) PedestalActual.Ocupante = null;
        PedestalActual = null;
        gameObject.SetActive(false);
    }

    // Lo llama la mochila al soltarlo (o Start al iniciar)
    public void ColocarEn(PedestalCueva pedestal)
    {
        PedestalActual = pedestal;
        pedestal.Ocupante = this;
        transform.position = pedestal.PosicionObjeto;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other) { InteraccionJugador.AlEntrarTrigger(this, other); }
    private void OnTriggerExit(Collider other) { InteraccionJugador.AlSalirTrigger(this, other); }
}
