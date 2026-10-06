using UnityEngine;

public class SurfaceType : MonoBehaviour
{
    public enum TipoTerreno
    {
        Tierra,
        Hierba,
        Piedra,
        Agua
    }

    public TipoTerreno tipoTerreno;
}