using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class BarraRio : MonoBehaviour
{
    public static BarraRio Instancia { get; private set; }

  
    public Image relleno;                 
    public float velocidadAnimacion = 1f; 

    
    [Range(0f, 100f)] public float valor = 0f;

   
    public UnityEvent alLlenarse;         

    private bool yaSeLleno = false;      

    public float Valor => valor;

    private void Awake()
    {
        Instancia = this;
        if (relleno != null) relleno.fillAmount = valor / 100f;
    }

    private void Update()
    {
        if (relleno == null) return;
        relleno.fillAmount = Mathf.MoveTowards(relleno.fillAmount, valor / 100f, velocidadAnimacion * Time.deltaTime);
    }

    
    public void Sumar(float puntos)
    {
        valor = Mathf.Clamp(valor + puntos, 0f, 100f);

        if (!yaSeLleno && valor >= 100f)
        {
            yaSeLleno = true;
            alLlenarse?.Invoke();
        }
    }

   
    public void Restar(float puntos)
    {
        valor = Mathf.Clamp(valor - puntos, 0f, 100f);
    }

    // Solo para probar sin la mecánica de recoger basura 
   
    [ContextMenu("Probar: llenar barra")]
    private void ProbarLlenar()
    {
        Sumar(100f);
    }
}
