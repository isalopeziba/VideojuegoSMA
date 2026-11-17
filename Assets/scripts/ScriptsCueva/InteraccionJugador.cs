using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public interface IInteractuable
{
    Transform transform { get; }
    bool PuedeInteractuar { get; }
    void Interactuar(InteraccionJugador jugador);
}

// Va en el JUGADOR. Detecta qué cosas interactuables tiene cerca y,

public class InteraccionJugador : MonoBehaviour
{
    public MochilaJugador Mochila { get; private set; }

    // true mientras se mantiene E o el botón de la manito
    public bool AccionMantenida => teclaMantenida || botonUIMantenido;

    private bool teclaMantenida = false;
    private bool botonUIMantenido = false;

    private InputSystem_Actions inputActions;
    private readonly List<IInteractuable> cercanos = new List<IInteractuable>();

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        Mochila = GetComponent<MochilaJugador>();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.started += OnInteractInicio;
        inputActions.Player.Interact.canceled += OnInteractFin;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.started -= OnInteractInicio;
        inputActions.Player.Interact.canceled -= OnInteractFin;
        inputActions.Player.Disable();
        teclaMantenida = false;
        botonUIMantenido = false;
    }

    private void Update()
    {
        // Respaldo: si la acción tiene una interacción "Hold", 'canceled' puede no llegar igual
        if (teclaMantenida && Keyboard.current != null && !Keyboard.current.eKey.isPressed)
            teclaMantenida = false;
    }

    private void OnInteractInicio(InputAction.CallbackContext context)
    {
        teclaMantenida = true;
        PulsarAccion();
    }

    private void OnInteractFin(InputAction.CallbackContext context)
    {
        teclaMantenida = false;
    }

    // Botón de la manito en celular. Lo usa BotonMantenerAccion (o un OnClick para un toque simple)
    public void PulsarAccion()
    {
        IInteractuable objetivo = MasCercano();
        if (objetivo != null) objetivo.Interactuar(this);
    }

    public void AccionUI(bool presionada)
    {
        botonUIMantenido = presionada;
        if (presionada) PulsarAccion();
    }

    public void Registrar(IInteractuable i)
    {
        if (!cercanos.Contains(i)) cercanos.Add(i);
    }

    public void Quitar(IInteractuable i)
    {
        cercanos.Remove(i);
    }

    public bool EstaCercaDe(IInteractuable i) => cercanos.Contains(i);

    private IInteractuable MasCercano()
    {
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