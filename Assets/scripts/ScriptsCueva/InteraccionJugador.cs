using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Todo lo que el jugador puede usar con la tecla E / botón de la manito
// (objetos de los pedestales, la cascada de salida...) implementa esta interfaz.
public interface IInteractuable
{
    Transform transform { get; }
    bool PuedeInteractuar { get; }
    void Interactuar(InteraccionJugador jugador);
}

// Va en el JUGADOR. Detecta qué cosas interactuables tiene cerca y,
// al presionar E (o el botón de la manito), usa la más cercana.
public class InteraccionJugador : MonoBehaviour
{
    public MochilaJugador Mochila { get; private set; }

    private InputSystem_Actions inputActions;
    private readonly List<IInteractuable> cercanos = new List<IInteractuable>();

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        Mochila = GetComponent<MochilaJugador>();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.started += OnInteract;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.started -= OnInteract;
        inputActions.Player.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        PulsarAccion();
    }

    // Conecta aquí el OnClick del botón de la manito (celular)
    public void PulsarAccion()
    {
        IInteractuable objetivo = MasCercano();
        if (objetivo != null) objetivo.Interactuar(this);
    }

    public void Registrar(IInteractuable i)
    {
        if (!cercanos.Contains(i)) cercanos.Add(i);
    }

    public void Quitar(IInteractuable i)
    {
        cercanos.Remove(i);
    }

    private IInteractuable MasCercano()
    {
        // limpiar los que se destruyeron
        cercanos.RemoveAll(i => (i as Object) == null);

        IInteractuable mejor = null;
        float mejorDist = float.MaxValue;
        foreach (IInteractuable i in cercanos)
        {
            if (!i.PuedeInteractuar) continue;
            float d = (i.transform.position - transform.position).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = i; }
        }
        return mejor;
    }

    // Ayuda para que cada interactuable se registre con su trigger
    public static void AlEntrarTrigger(IInteractuable i, Collider other)
    {
        if (!other.CompareTag("Player")) return;
        InteraccionJugador j = other.GetComponentInParent<InteraccionJugador>();
        if (j != null) j.Registrar(i);
    }

    public static void AlSalirTrigger(IInteractuable i, Collider other)
    {
        if (!other.CompareTag("Player")) return;
        InteraccionJugador j = other.GetComponentInParent<InteraccionJugador>();
        if (j != null) j.Quitar(i);
    }
}
