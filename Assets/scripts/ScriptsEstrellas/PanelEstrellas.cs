using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PanelEstrellas : MonoBehaviour
{
    [Header("Textos")]
    public Text textoEstrellas;     
    public Text textoMotivos;        //  lista de por qué se ganaron

    [Header("Imágenes de estrellas (opcional)")]
    public Image[] imagenesEstrellas = new Image[3];
    public Sprite estrellaLlena;
    public Sprite estrellaVacia;

    [Header("Botón continuar")]
    public Button botonContinuar;
    [Tooltip("Escena a la que se pasa al tocar Continuar (vacío = solo cierra el panel)")]
    public string escenaSiguiente = "";

    [Header("Al mostrar el panel")]
    [Tooltip("Objetos que se ocultan mientras se ve el panel (ej: casillas de la mochila, joystick)")]
    public GameObject[] ocultarAlMostrar;
    public bool pausarJuego = true;

    
    private bool botonConectado = false;

    private void ConectarBoton()
    {
        if (botonConectado || botonContinuar == null) return;
        botonContinuar.onClick.AddListener(Continuar);
        botonConectado = true;
    }

    public void Mostrar(int estrellas, List<string> motivos)
    {
        ConectarBoton();
        gameObject.SetActive(true);

        if (textoEstrellas != null)
            textoEstrellas.text = $"Estrellas: {estrellas} / {ProgresoEstrellas.MaxPorNivel}";

        if (textoMotivos != null)
            textoMotivos.text = "• " + string.Join("\n• ", motivos);

        for (int i = 0; i < imagenesEstrellas.Length; i++)
        {
            if (imagenesEstrellas[i] == null) continue;
            bool ganada = i < estrellas;
            if (estrellaLlena != null && estrellaVacia != null)
                imagenesEstrellas[i].sprite = ganada ? estrellaLlena : estrellaVacia;
            else
                imagenesEstrellas[i].enabled = ganada;
        }

        foreach (GameObject g in ocultarAlMostrar)
            if (g != null) g.SetActive(false);

        if (pausarJuego) Time.timeScale = 0f;

        if (gameObject.activeInHierarchy)
            Debug.Log("PanelEstrellas: panel mostrado.");
        else
            Debug.LogWarning("PanelEstrellas: el panel se activó, pero NO se ve porque uno de sus padres " +
                             "(por ejemplo el canvas) está desactivado o está en la lista 'Ocultar Al Mostrar'.");
    }

    public void Continuar()
    {
        Time.timeScale = 1f;   // siempre antes de cambiar de escena

        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            SceneManager.LoadScene(escenaSiguiente);
        }
        else
        {
            Debug.Log("PanelEstrellas: no hay 'Escena Siguiente' asignada.");
            gameObject.SetActive(false);
            foreach (GameObject g in ocultarAlMostrar)
                if (g != null) g.SetActive(true);
        }
    }
}