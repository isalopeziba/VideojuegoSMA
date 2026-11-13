using UnityEngine;
using UnityEngine.UI;

// Va en el CANVAS de la mochila. Muestra los objetos en las 3 casillas
// y al tocar una casilla suelta ese objeto.
public class MochilaUI : MonoBehaviour
{
    public MochilaJugador mochila;

    [Tooltip("Las 3 casillas (con componente Button)")]
    public Button[] casillas = new Button[3];

    [Tooltip("La Image hija de cada casilla donde se pone el icono del objeto")]
    public Image[] iconos = new Image[3];

    [Tooltip("Opcional: texto tipo '2/3'")]
    public Text textoContador;

    private void Awake()
    {
        for (int i = 0; i < casillas.Length; i++)
        {
            int indice = i;
            if (casillas[i] != null)
                casillas[i].onClick.AddListener(() => { if (mochila != null) mochila.Soltar(indice); });
        }
    }

    private void OnEnable()
    {
        if (mochila != null) mochila.AlCambiar += Actualizar;
        Actualizar();
    }

    private void OnDisable()
    {
        if (mochila != null) mochila.AlCambiar -= Actualizar;
    }

    private void Actualizar()
    {
        if (mochila == null) return;

        for (int i = 0; i < casillas.Length; i++)
        {
            bool hayObjeto = i < mochila.Cantidad;

            if (iconos.Length > i && iconos[i] != null)
            {
                iconos[i].enabled = hayObjeto;
                if (hayObjeto) iconos[i].sprite = mochila.Objetos[i].icono;
            }

            if (casillas[i] != null) casillas[i].interactable = hayObjeto;
        }

        if (textoContador != null)
            textoContador.text = $"{mochila.Cantidad}/{mochila.capacidad}";
    }
}
