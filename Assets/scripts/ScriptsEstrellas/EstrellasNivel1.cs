using System.Collections.Generic;
using UnityEngine;


public class EstrellasNivel1 : MonoBehaviour
{
    public int numeroNivel = 1;

    [Header("Referencias")]
    public PreguntasMohan preguntasMohan;
    public SalidaCueva salidaCueva;
    public PanelEstrellas panel;

    [Header("Reglas")]
    public int aciertosALaPrimeraNecesarios = 2;

    private bool yaTermino = false;

    
    public void TerminarNivel()
    {
        if (yaTermino) return;
        yaTermino = true;

        List<string> motivos = new List<string>();
        int estrellas = 0;

        // 1. Completar el nivel
        estrellas++;
        motivos.Add("Completaste el nivel");

        // 2. Acertijos a la primera
        int aciertos = preguntasMohan != null ? preguntasMohan.AciertosALaPrimera : 0;
        if (aciertos >= aciertosALaPrimeraNecesarios)
        {
            estrellas++;
            motivos.Add($"Acertaste {aciertos} acertijos del Mohán a la primera");
        }

        // 3. Solo la bolsa de semillas en la cueva
        if (salidaCueva != null && salidaCueva.SalioSoloConSemillas)
        {
            estrellas++;
            motivos.Add("En la cueva solo tomaste lo necesario: la bolsa de semillas");
        }

        ProgresoEstrellas.GuardarNivel(numeroNivel, estrellas);
        Debug.Log($"EstrellasNivel1: {estrellas} estrellas. Total del juego: {ProgresoEstrellas.Total()}");

        if (panel != null) panel.Mostrar(estrellas, motivos);
        else Debug.LogWarning("EstrellasNivel1: falta arrastrar el PanelEstrellas al campo 'Panel'.");
    }
}