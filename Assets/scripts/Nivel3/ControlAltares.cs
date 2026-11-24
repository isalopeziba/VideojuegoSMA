
using UnityEngine;

public class ControlAltares : MonoBehaviour
{
    [Header("Altares")]
    public Altar altar1;
    public Altar altar2;

    [Header("Madremonte")]
    public MadremontePreguntas madremonte;

    private bool ritualCompletado = false;

    private void Start()
    {
        if (altar1 != null)
            altar1.AlCompletarOfrenda += RevisarAltares;

        if (altar2 != null)
            altar2.AlCompletarOfrenda += RevisarAltares;

        // Por si los altares ya estaban completados
        RevisarAltares();
    }

    private void OnDestroy()
    {
        if (altar1 != null)
            altar1.AlCompletarOfrenda -= RevisarAltares;

        if (altar2 != null)
            altar2.AlCompletarOfrenda -= RevisarAltares;
    }

    private void RevisarAltares()
    {
        if (ritualCompletado) return;

        if (altar1 == null || altar2 == null) return;

        if (altar1.ofrendaColocada && altar2.ofrendaColocada)
        {
            ritualCompletado = true;

            ActivarMadremonte();
        }
    }

    private void ActivarMadremonte()
    {
        if (madremonte == null) return;

        // Activar el objeto de la Madremonte
        madremonte.gameObject.SetActive(true);

        // Comenzar la pregunta
        madremonte.IniciarPregunta();
    }
}
