
using UnityEngine;
using UnityEngine.UI;

public class BotonRecoger : MonoBehaviour
{
    private Button boton;
    public PlayerMov jugador;

    private void Awake()
    {
        boton = GetComponent<Button>();
    }

    private void Update()
    {
    }

    public void PulsarBoton()
    {
        basura objetivo = basura.BasuraCercana;

        if (objetivo != null)
            objetivo.Interactuar();
    }

    public void Interact()
    {
        if (jugador != null)
        {
            jugador.Interactuar();
        }
    }
}
