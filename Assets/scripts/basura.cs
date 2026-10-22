using UnityEngine;
using UnityEngine.InputSystem;

public class basura : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvasMinijuego;          // Objeto Canvas/Panel que contiene el minijuego
    public NewMonoBehaviourScript minijuego;     // Referencia al script del minijuego

    [Header("Configuración")]
    public float puntosPorBasura = 10f;         // Puntos que le sumará a BarraRio

    private bool jugadorEnRango = false;

    private void Update()
    {
        // Al estar en rango y presionar la tecla 1 (Alpha1)
        if (jugadorEnRango && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)    
        {
            IniciarMinijuego();
        }
    }

    void IniciarMinijuego()
    {
        canvasMinijuego.SetActive(true);
        minijuego.IniciarJuego(this); // Pasa la referencia de este objeto al minijuego
    }

    public void FinalizarMinijuego(bool ganado)
    {
        canvasMinijuego.SetActive(false);

        if (ganado)
        {
            // Suma los puntos en BarraRio usando su Instancia
            if (BarraRio.Instancia != null)
            {
                BarraRio.Instancia.Sumar(puntosPorBasura);
            }
            else
            {
                Debug.LogWarning("No se encontró la instancia de BarraRio en la escena.");
            }

            // Elimina la basura del mundo
            gameObject.SetActive(false);    
        }
        else
        {
            Debug.Log("Minijuego no completado. La basura sigue aquí para reintentarlo.");
        }
    }

    // Detección de cercanía en 3D (Si tu proyecto es 2D, cambia por OnTriggerEnter2D y OnTriggerExit2D)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            Debug.Log("Presiona 'e' para recolectar la basura");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
        }
    }
}