using UnityEngine;

public class GuardarFicha : MonoBehaviour
{
    public string idFicha = ColeccionFichas.Madremonte;

    public void Guardar()
    {
        ColeccionFichas.Obtener(idFicha);
    }
}
