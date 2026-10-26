
using UnityEngine;
using UnityEngine.UI;

public class BotonRecoger : MonoBehaviour
{
    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
    }

    private void Update()
    {
        // Solo habilitar el boton si hay basura cercana
        bool hayBasura = basura.BasuraCercana != null &&
                         basura.BasuraCercana.gameObject.activeInHierarchy;

        if (boton != null)
            boton.interactable = hayBasura;
    }

    public void PulsarBoton()
    {
        basura objetivo = basura.BasuraCercana;

        if (objetivo != null)
            objetivo.Interactuar();
    }
}
