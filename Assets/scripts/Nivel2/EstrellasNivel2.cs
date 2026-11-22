using System.Collections.Generic;
using UnityEngine;

// Reglas de estrellas del NIVEL 2 (La Patasola). Usa el mismo guardado (ProgresoEstrellas)
// y el mismo panel (PanelEstrellas) que el Nivel 1.
//
//   1 estrella  → completar el nivel
//  +1 estrella  → la Patasola no alcanzó al joven en ninguna zona (nunca se quedó sin tiempo)
//  +1 estrella  → acertar al menos 2 acertijos a la primera
public class EstrellasNivel2 : MonoBehaviour
{
    public int numeroNivel = 2;

    [Header("Referencias")]
    public ControlNivel2 control;
    public PreguntasGuardian preguntas;
    public PanelEstrellas panel;

    [Header("Reglas")]
    [Tooltip("Cuántas zonas se pueden perder y aun así ganar la estrella del tiempo (0 = ninguna)")]
    public int zonasFalladasPermitidas = 0;
    public int aciertosALaPrimeraNecesarios = 2;

    private bool yaTermino = false;

    // La llama CierreNivel2 al final
    public void TerminarNivel()
    {
        if (yaTermino) return;
        yaTermino = true;

        List<string> motivos = new List<string>();
        int estrellas = 0;

        // 1. Completar el nivel
        estrellas++;
        motivos.Add("Completaste el nivel");

        // 2. Sin quedarse sin tiempo
        int falladas = ZonasFalladas();
        if (falladas <= zonasFalladasPermitidas)
        {
            estrellas++;
            motivos.Add(falladas == 0
                ? "Sembraste todas las zonas antes de que llegara la Patasola"
                : $"La Patasola solo te alcanzó en {falladas} zona(s)");
        }

        // 3. Acertijos a la primera
        int aciertos = preguntas != null ? preguntas.AciertosALaPrimera : 0;
        if (aciertos >= aciertosALaPrimeraNecesarios)
        {
            estrellas++;
            motivos.Add($"Acertaste {aciertos} acertijos de la Patasola a la primera");
        }

        ProgresoEstrellas.GuardarNivel(numeroNivel, estrellas);
        Debug.Log($"EstrellasNivel2: {estrellas} estrellas (zonas falladas: {falladas}). Total del juego: {ProgresoEstrellas.Total()}");

        if (panel != null) panel.Mostrar(estrellas, motivos);
        else Debug.LogWarning("EstrellasNivel2: falta arrastrar el PanelEstrellas al campo 'Panel'.");
    }

    private int ZonasFalladas()
    {
        if (control == null) return 0;
        int n = 0;
        foreach (TreeCountingManager z in control.zonas)
            if (z != null && z.Intentos > 0) n++;
        return n;
    }
}
