using UnityEngine;

// Guarda las estrellas de TODO el juego 
// Se usa desde cualquier escena:

public static class ProgresoEstrellas
{
    public const int MaxPorNivel = 3;
    public const int CantidadNiveles = 2;   

    private static string ClaveMejor(int nivel) => $"Estrellas_Nivel{nivel}_Mejor";
    private static string ClaveUltima(int nivel) => $"Estrellas_Nivel{nivel}_Ultima";

    
    public static void GuardarNivel(int nivel, int estrellas)
    {
        estrellas = Mathf.Clamp(estrellas, 0, MaxPorNivel);

        PlayerPrefs.SetInt(ClaveUltima(nivel), estrellas);
        if (estrellas > ObtenerNivel(nivel))
            PlayerPrefs.SetInt(ClaveMejor(nivel), estrellas);

        PlayerPrefs.Save();
    }

    // Mejores estrellas obtenidas en ese nivel (0 si no se ha jugado)
    public static int ObtenerNivel(int nivel) => PlayerPrefs.GetInt(ClaveMejor(nivel), 0);

    // Estrellas de la última vez que se jugó ese nivel
    public static int ObtenerUltimaPartida(int nivel) => PlayerPrefs.GetInt(ClaveUltima(nivel), 0);

    // Total de estrellas del juego (suma de los mejores resultados de cada nivel)
    public static int Total()
    {
        int total = 0;
        for (int n = 1; n <= CantidadNiveles; n++) total += ObtenerNivel(n);
        return total;
    }

    public static int TotalPosible => CantidadNiveles * MaxPorNivel;

    
    public static void Reiniciar()
    {
        for (int n = 1; n <= CantidadNiveles; n++)
        {
            PlayerPrefs.DeleteKey(ClaveMejor(n));
            PlayerPrefs.DeleteKey(ClaveUltima(n));
        }
        PlayerPrefs.Save();
    }
}
