using UnityEngine;
using UnityEngine.UI;

// Va en el canvas. Las 5 huellas se van llenando/encendiendo mientras la Patasola se acerca.
// Si la Image es de tipo "Filled" se rellena; si es normal, cambia de apagada a encendida.
public class HuellasUI : MonoBehaviour
{
    [Tooltip("El objeto que contiene las huellas (se muestra solo mientras corre el tiempo)")]
    public GameObject contenedor;
    [Tooltip("Las 5 huellas, en orden")]
    public Image[] huellas = new Image[5];

    public Color colorApagada = new Color(1f, 1f, 1f, 0.25f);
    public Color colorEncendida = Color.white;

    private void Start()
    {
        Ocultar();
    }

    public void Mostrar()
    {
        if (contenedor != null) contenedor.SetActive(true);
        Actualizar(0f);
    }

    public void Ocultar()
    {
        if (contenedor != null) contenedor.SetActive(false);
    }

    // progreso: 0 = recién empezó, 1 = la Patasola llegó
    public void Actualizar(float progreso)
    {
        int n = huellas.Length;
        for (int i = 0; i < n; i++)
        {
            Image img = huellas[i];
            if (img == null) continue;

            float parte = Mathf.Clamp01(progreso * n - i);   // cuánto le toca a esta huella

            if (img.type == Image.Type.Filled)
            {
                img.fillAmount = parte;
                img.color = colorEncendida;
            }
            else
            {
                img.color = Color.Lerp(colorApagada, colorEncendida, parte);
            }
        }
    }
}
