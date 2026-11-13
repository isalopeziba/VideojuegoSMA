using System.Collections.Generic;
using UnityEngine;

// Va en cada PEDESTAL de la cueva. Solo sabe si está libre u ocupado
// y en qué punto se coloca el objeto encima.
public class PedestalCueva : MonoBehaviour
{
    public static readonly List<PedestalCueva> Todos = new List<PedestalCueva>();

    [Tooltip("Punto encima del pedestal donde se pone el objeto (si está vacío, usa 1 unidad arriba)")]
    public Transform puntoObjeto;

    public ObjetoCueva Ocupante { get; set; }
    public bool Libre => Ocupante == null;

    public Vector3 PosicionObjeto =>
        puntoObjeto != null ? puntoObjeto.position : transform.position + Vector3.up;

    private void OnEnable() { Todos.Add(this); }
    private void OnDisable() { Todos.Remove(this); }

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
}
