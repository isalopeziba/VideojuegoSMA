using UnityEngine;

[System.Serializable]
public class FichaEnHUD
{
    [Tooltip("Mohan, Patasola o Madremonte (igual que en ColeccionFichas)")]
    public string id;
    [Tooltip("La imagen de la ficha en el HUD")]
    public GameObject icono;
}

// Va en el HUD de CADA nivel. Al iniciar muestra las fichas que ya se ganaron
// y se actualiza sola cuando se gana una nueva.
public class FichasHUD : MonoBehaviour
{
    public FichaEnHUD[] fichas =
    {
        new FichaEnHUD { id = ColeccionFichas.Mohan },
        new FichaEnHUD { id = ColeccionFichas.Patasola },
        new FichaEnHUD { id = ColeccionFichas.Madremonte }
    };

    private void OnEnable()
    {
        ColeccionFichas.AlCambiar += Actualizar;
        Actualizar();
    }

    private void OnDisable()
    {
        ColeccionFichas.AlCambiar -= Actualizar;
    }

    private void Actualizar()
    {
        foreach (FichaEnHUD f in fichas)
            if (f.icono != null) f.icono.SetActive(ColeccionFichas.Tiene(f.id));
    }
}
