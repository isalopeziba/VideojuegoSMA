using UnityEngine;

public class debug : MonoBehaviour
{
    private string ultimo = "";

    private void OnCollisionStay(Collision c)
    {
        // ignora el suelo (contactos que vienen desde abajo)
        foreach (ContactPoint p in c.contacts)
        {
            if (p.normal.y > 0.7f) continue;
            string nombre = c.collider.name + " (padre: " + (c.transform.parent ? c.transform.parent.name : "ninguno") + ")";
            if (nombre != ultimo)
            {
                Debug.Log("CHOCANDO CON: " + nombre, c.collider.gameObject);
                ultimo = nombre;
            }
            return;
        }
    }

    private void OnCollisionExit(Collision c) { ultimo = ""; }
}
