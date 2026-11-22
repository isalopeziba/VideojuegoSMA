using UnityEngine;

// Va en el DUENDE (junto a DuendeNPC) en la escena del Nivel 2.
// Solo decide QUÉ dice el Duende en cada momento; para hablar usa el Decir() de DuendeNPC.
public class DuendeNivel2 : MonoBehaviour
{
    [Header("Referencias")]
    public DuendeNPC duende;
    public ControlNivel2 control;

    [Header("Frases")]
    [TextArea(1, 3)] public string fraseOyeSaltos = "¿Oyes? Ya viene. Siembra rápido.";
    [TextArea(1, 3)] public string frasePrimerArbol = "Mira cómo respira el bosque. Uno más.";
    [TextArea(1, 3)] public string fraseFaltaPoco = "¡Los saltos suenan cerca! ¡Ahora!";
    [TextArea(1, 3)] public string fraseEntraClaro = "Los parches están lejos. ¡Corre, pero con cuidado!";

    [Tooltip("Número de la zona del claro (zona D)")]
    public int zonaDelClaro = 4;

    private bool dijoSaltos = false;
    private bool dijoPrimerArbol = false;
    private bool dijoClaro = false;

    private void Awake()
    {
        if (duende == null) duende = GetComponent<DuendeNPC>();
    }

    private void OnEnable()
    {
        if (control == null)
        {
            Debug.LogWarning("DuendeNivel2: falta arrastrar ControlNivel2.");
            return;
        }
        control.AlEntrarZona += OnEntrarZona;
        control.AlSembrarArbol += OnSembrar;
        control.AlPatasolaCerca += OnPatasolaCerca;
    }

    private void OnDisable()
    {
        if (control == null) return;
        control.AlEntrarZona -= OnEntrarZona;
        control.AlSembrarArbol -= OnSembrar;
        control.AlPatasolaCerca -= OnPatasolaCerca;
    }

    private void OnEntrarZona(TreeCountingManager zona)
    {
        // Entra al claro: tiene prioridad sobre la frase de los saltos
        if (!dijoClaro && zona.numeroZona == zonaDelClaro && zona.Intentos == 0)
        {
            dijoClaro = true;
            duende.Decir(fraseEntraClaro, true);
            return;
        }

        // Primera vez que empiezan a sonar los saltos
        if (!dijoSaltos)
        {
            dijoSaltos = true;
            duende.Decir(fraseOyeSaltos, true);
        }
    }

    private void OnSembrar(TreeCountingManager zona)
    {
        if (dijoPrimerArbol) return;
        dijoPrimerArbol = true;
        duende.Decir(frasePrimerArbol, true);
    }

    // Cada vez que falta poco (en cada zona e intento)
    private void OnPatasolaCerca(TreeCountingManager zona)
    {
        duende.Decir(fraseFaltaPoco, true);
    }
}
