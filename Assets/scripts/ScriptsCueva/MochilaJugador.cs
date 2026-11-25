using System.Collections.Generic;
using UnityEngine;

public class MochilaJugador : MonoBehaviour
{
    public int capacidad = 3;

    private readonly List<ObjetoCueva> objetos = new List<ObjetoCueva>();
    public IReadOnlyList<ObjetoCueva> Objetos => objetos;

    // ---- Avisos para la UI, el Mohán y el Duende ----
    public event System.Action AlCambiar;                    // se tomó o se soltó algo
    public event System.Action<ObjetoCueva> AlTomar;         // se tomó este objeto
    public event System.Action AlIntentarLlenarDeMas;        // intentó tomar un 4.º objeto

    public int Cantidad => objetos.Count;
    public bool EstaLlena => objetos.Count >= capacidad;
    public bool TieneSemillas => objetos.Exists(o => o.tipo == TipoObjetoCueva.Semillas);
    public bool SoloSemillas => objetos.Count == 1 && TieneSemillas;

    public bool Tomar(ObjetoCueva obj)
    {
        if (obj == null || objetos.Contains(obj)) return false;

        if (EstaLlena)
        {
            AlIntentarLlenarDeMas?.Invoke();
            return false;
        }

        objetos.Add(obj);
        obj.Guardar();
        AlTomar?.Invoke(obj);
        AlCambiar?.Invoke();
        return true;
    }

    // Tocar una casilla de la mochila: suelta ese objeto en el pedestal libre más cercano
    public void Soltar(int indice)
    {
        PedestalCueva pedestal = PedestalCueva.MasCercanoLibre(transform.position);
        if (pedestal == null)
        {
            Debug.LogWarning("MochilaJugador: no hay pedestales libres para soltar el objeto.");
            return;
        }
        SoltarEn(indice, pedestal);
    }

    // E junto a un pedestal vacío: deja ahí el último objeto que se tomó
    public void SoltarUltimoEn(PedestalCueva pedestal)
    {
        SoltarEn(objetos.Count - 1, pedestal);
    }

    private void SoltarEn(int indice, PedestalCueva pedestal)
    {
        if (indice < 0 || indice >= objetos.Count || pedestal == null || !pedestal.Libre) return;

        ObjetoCueva obj = objetos[indice];
        objetos.RemoveAt(indice);
        obj.ColocarEn(pedestal);
        AlCambiar?.Invoke();
    }
}