using UnityEngine;
using UnityEngine.UI;

public class InventarioSemillas : MonoBehaviour
{
    // Semillas al empezar el nivel: zona N necesita N + 1 árboles ? 2 + 3 + 4 + 5 = 14")]
    public int semillasIniciales = 14;

    [Header("HUD")]
    public Text textoCantidad;
    public string formato = "x {0}";

    public int Cantidad { get; private set; }
    public event System.Action AlCambiar;

    private void Awake()
    {
        Cantidad = semillasIniciales;
    }

    private void Start()
    {
        ActualizarHUD();
    }

    public bool Usar()
    {
        if (Cantidad <= 0) return false;
        Cantidad--;
        ActualizarHUD();
        return true;
    }

    // Se usa al reiniciar una zona: vuelve a las semillas que tenía al entrar
    public void Establecer(int cantidad)
    {
        Cantidad = Mathf.Max(0, cantidad);
        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (textoCantidad != null) textoCantidad.text = string.Format(formato, Cantidad);
        AlCambiar?.Invoke();
    }
}
