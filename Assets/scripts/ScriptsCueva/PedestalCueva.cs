using System.Collections.Generic;
using UnityEngine;


public class PedestalCueva : MonoBehaviour, IInteractuable
{
    public static readonly List<PedestalCueva> Todos = new List<PedestalCueva>();

    [Tooltip("Punto encima del pedestal donde se pone el objeto (si está vacío, usa 1 unidad arriba)")]
    public Transform puntoObjeto;

    public ObjetoCueva Ocupante { get; set; }
    public bool Libre => Ocupante == null;

    public Vector3 PosicionObjeto =>
        puntoObjeto != null ? puntoObjeto.position : transform.position + Vector3.up;

    // Solo se puede usar con E cuando está vacío (si tiene objeto, se usa el objeto)
    public bool PuedeInteractuar => isActiveAndEnabled && Libre;

    private void OnEnable() { Todos.Add(this); }
    private void OnDisable() { Todos.Remove(this); }

    public void Interactuar(InteraccionJugador jugador)
    {
        if (jugador.Mochila != null) jugador.Mochila.SoltarUltimoEn(this);
    }

    public static PedestalCueva MasCercanoLibre(Vector3 posicion)
    {
        PedestalCueva mejor = null;
        float mejorDist = float.MaxValue;
        foreach (PedestalCueva p in Todos)
        {
            if (!p.Libre) continue;
            float d = (p.transform.position - posicion).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = p; }
        }
        return mejor;
    }

    private void OnTriggerEnter(Collider other) { InteraccionJugador.AlEntrarTrigger(this, other); }
    private void OnTriggerExit(Collider other) { InteraccionJugador.AlSalirTrigger(this, other); }
}