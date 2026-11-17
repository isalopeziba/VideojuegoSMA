using UnityEngine;

public class TreeCountingManager : MonoBehaviour
{
    [Header("Configuración de zona")]
    public int numeroZona = 1;

    [Header("Progreso")]
    public int arbolesSembrados = 0;

    [Header("Barrera")]
    public BlockBarrier barrera;

    private bool zonaCompletada = false;

    public int ArbolesNecesarios
    {
        get { return numeroZona + 1; }
    }

    public void RegistrarArbol()
    {
        if (zonaCompletada) return;

        arbolesSembrados++;

        Debug.Log("Zona " + numeroZona +
                  ": " + arbolesSembrados +
                  "/" + ArbolesNecesarios + " árboles");

        if (arbolesSembrados >= ArbolesNecesarios)
        {
            CompletarZona();
        }
    }

    private void CompletarZona()
    {
        zonaCompletada = true;

        Debug.Log("¡Zona " + numeroZona + " completada!");

        if (barrera != null)
        {
            barrera.AbrirPaso();
        }
    }
}