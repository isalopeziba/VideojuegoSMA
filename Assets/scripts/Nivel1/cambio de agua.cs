using UnityEngine;
using System.Collections.Generic;

public class RiverManager : MonoBehaviour
{
    [Header("Referencias del Río")]
    public Renderer riverRenderer;
    [ColorUsage(false, true)] public Color dirtyColor = new Color(0.45f, 0.3f, 0.15f); // Color café inicial
    [ColorUsage(false, true)] public Color cleanColor = new Color(0.2f, 0.5f, 0.9f);   // Color azul final

    [Header("Configuración de Basura")]
    public List<GameObject> trashItems = new List<GameObject>();
    private int totalTrashCount;
    private Material riverMaterialInstance;

    void Start()
    {
        if (riverRenderer != null)
        {
            // Creamos una instancia del material para modificarlo solo en tiempo de ejecución
            riverMaterialInstance = riverRenderer.material;
        }

        // Si no se asignan manualmente, busca todos los objetos con la etiqueta "Trash"
        if (trashItems.Count == 0)
        {
            GameObject[] foundTrash = GameObject.FindGameObjectsWithTag("Trash");
            trashItems.AddRange(foundTrash);
        }

        totalTrashCount = trashItems.Count;
        UpdateRiverColor();
    }

    void Update()
    {
        UpdateRiverColor();
    }

    void UpdateRiverColor()
    {
        if (riverMaterialInstance == null) return;

        int activeTrash = 0;

        foreach (GameObject trash in trashItems)
        {
            if (trash != null && trash.activeInHierarchy)
            {
                activeTrash++;
            }
        }

        if (totalTrashCount == 0) return;

        // Calcula el porcentaje de limpieza (de 0.0 a 1.0)
        float cleanedPercentage = 1f - ((float)activeTrash / totalTrashCount);

        // Interpola suavemente entre el color café y el azul
        Color targetColor = Color.Lerp(dirtyColor, cleanColor, cleanedPercentage);

        // Aplica el color directamente al material usando la propiedad de URP
        riverMaterialInstance.SetColor("_BaseColor", targetColor);
    }
}