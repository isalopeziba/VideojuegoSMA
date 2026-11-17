using UnityEngine;

// Va en el ARCO  (con un Collider "Is Trigger").

[RequireComponent(typeof(Collider))]
public class ArcoZona : MonoBehaviour
{
    public ControlNivel2 control;
    public TreeCountingManager zona;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || control == null || zona == null) return;
        control.JugadorCruzoArco(zona);
    }
}