using UnityEngine;


public static class ColeccionFichas
{
    public const string Mohan = "Mohan";
    public const string Patasola = "Patasola";
    public const string Madremonte = "Madremonte";
    public static readonly string[] Todas = { Mohan, Patasola, Madremonte };

    // Avisa al HUD cuando se gana una ficha
    public static event System.Action AlCambiar;

    private static string Clave(string id) => "Ficha_" + id;

    public static void Obtener(string id)
    {
        if (Tiene(id)) return;
        PlayerPrefs.SetInt(Clave(id), 1);
        PlayerPrefs.Save();
        Debug.Log($"ColeccionFichas: ficha '{id}' guardada.");
        AlCambiar?.Invoke();
    }

    public static bool Tiene(string id) => PlayerPrefs.GetInt(Clave(id), 0) == 1;

    public static int Cantidad
    {
        get
        {
            int n = 0;
            foreach (string id in Todas) if (Tiene(id)) n++;
            return n;
        }
    }

    // Para "Nueva partida"
    public static void Reiniciar()
    {
        foreach (string id in Todas) PlayerPrefs.DeleteKey(Clave(id));
        PlayerPrefs.Save();
        AlCambiar?.Invoke();
    }
}
