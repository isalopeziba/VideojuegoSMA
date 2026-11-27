using System.Collections;
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

    [Header("Tiempos")]
    [Tooltip("Segundos después de empezar una zona antes de que el Duende hable")]
    public float esperaAlEntrarZona = 4f;
    [Tooltip("Segundos después de sembrar el primer árbol antes de que el Duende hable")]
    public float esperaPrimerArbol = 1f;

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
            StartCoroutine(DecirConEspera(fraseEntraClaro, esperaAlEntrarZona, zona));
            return;
        }

        // Primera vez que empiezan a sonar los saltos
        if (!dijoSaltos)
        {
            dijoSaltos = true;
            StartCoroutine(DecirConEspera(fraseOyeSaltos, esperaAlEntrarZona, zona));
        }
    }

    private void OnSembrar(TreeCountingManager zona)
    {
        if (dijoPrimerArbol) return;
        dijoPrimerArbol = true;
        StartCoroutine(DecirConEspera(frasePrimerArbol, esperaPrimerArbol, null));
    }

    // Cada vez que falta poco (en cada zona e intento): este sí sale enseguida
    private void OnPatasolaCerca(TreeCountingManager zona)
    {
        duende.Decir(fraseFaltaPoco, true);
    }

    private IEnumerator DecirConEspera(string frase, float espera, TreeCountingManager zona)
    {
        yield return new WaitForSeconds(espera);

        // Si mientras esperaba la zona ya terminó, no tiene sentido decirlo
        if (zona != null && !zona.EnCurso) yield break;

        duende.Decir(frase, true);
    }
}