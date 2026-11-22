using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class TreeCountingManager : MonoBehaviour
{
    [Header("Configuración de zona")]
    public int numeroZona = 1;
    [Tooltip("Segundos que tarda la Patasola en llegar a esta zona")]
    public float tiempoLimite = 60f;

    [Header("Progreso")]
    public int arbolesSembrados = 0;

    [Header("Barrera")]
    public BlockBarrier barrera;

    [Header("Cuando llega la Patasola")]
    [Tooltip("Por dónde aparece saltando y hacia dónde va")]
    public Transform patasolaDesde;
    public Transform patasolaHasta;
    public UnityEvent alLlegarPatasola;

    [Header("Al completar")]
    public UnityEvent alCompletar;

    // --- ControlNivel2 ----
    public ControlNivel2 Control { get; set; }
    public bool EnCurso { get; set; }
    public int Intentos { get; set; }
    public bool ZonaCompletada => zonaCompletada;

    // Los parches de esta zona (los que tienen esta zona en su 'counterManager')
    public List<PlantingSpot> Parches { get; } = new List<PlantingSpot>();

    private bool zonaCompletada = false;

    public int ArbolesNecesarios
    {
        get { return numeroZona + 1; }
    }

    // 
    public bool PuedePlantar => Control == null || (EnCurso && Control.HaySemillas);

    private void Awake()
    {
        foreach (PlantingSpot p in FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None))
            if (p.counterManager == this) Parches.Add(p);

        if (Parches.Count < ArbolesNecesarios)
            Debug.LogWarning($"Zona {numeroZona}: necesita {ArbolesNecesarios} árboles pero solo tiene {Parches.Count} parches.");
    }

    public void RegistrarArbol()
    {
        if (zonaCompletada) return;

        arbolesSembrados++;

        Debug.Log("Zona " + numeroZona +
                  ": " + arbolesSembrados +
                  "/" + ArbolesNecesarios + " árboles");

        if (Control != null) Control.RegistrarSiembra(this);   // semilla y barra del bosque

        if (arbolesSembrados >= ArbolesNecesarios)
        {
            CompletarZona();
        }
    }

    private void CompletarZona()
    {
        zonaCompletada = true;
        EnCurso = false;

        Debug.Log("¡Zona " + numeroZona + " completada!");

        if (barrera != null)
        {
            barrera.AbrirPaso();
        }

        alCompletar?.Invoke();
        if (Control != null) Control.ZonaCompletada(this);
    }

    // La Patasola llegó antes: los árboles de esta zona desaparecen y se empieza de cero
    public void Reiniciar()
    {
        arbolesSembrados = 0;
        zonaCompletada = false;
        foreach (PlantingSpot p in Parches)
            if (p != null) p.Reiniciar();
    }
}