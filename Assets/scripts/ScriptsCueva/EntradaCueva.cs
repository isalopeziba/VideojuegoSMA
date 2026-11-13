using UnityEngine;
using UnityEngine.Events;


// Cuando el jugador entra por primera vez, lleva al Mohán a la cueva y avisa a los demás.
[RequireComponent(typeof(Collider))]
public class EntradaCueva : MonoBehaviour
{
    public MohanCueva mohan;

    [Tooltip("Opcional: el canvas de la mochila, para mostrarlo al entrar")]
    public GameObject canvasMochila;

    public UnityEvent alEntrar;
    public event System.Action AlEntrarCueva;   // lo escucha el Duende

    private bool yaEntro = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaEntro || !other.CompareTag("Player")) return;
        yaEntro = true;

        if (mohan != null) mohan.IrALaCueva();
        if (canvasMochila != null) canvasMochila.SetActive(true);

        alEntrar?.Invoke();
        AlEntrarCueva?.Invoke();
    }
}
