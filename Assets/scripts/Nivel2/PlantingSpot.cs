
using UnityEngine;

public class PlantingSpot : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject arbolPrefab;
    public Transform puntoDeSiembra;

    [Header("TreeCount")]
    public TreeCountingManager counterManager;

    private bool yaSembrado = false;

    public bool PuedeSembrar()
    {
        return !yaSembrado &&
               arbolPrefab != null &&
               counterManager != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null)
        {
            jugador.EntrarParche(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMov jugador = other.GetComponent<PlayerMov>();

        if (jugador != null)
        {
            jugador.SalirParche(this);
        }
    }

    public void Sembrar()
    {
        if (!PuedeSembrar()) return;

        Vector3 posicion = puntoDeSiembra != null
            ? puntoDeSiembra.position
            : transform.position;

        GameObject nuevoArbol = Instantiate(
            arbolPrefab,
            posicion,
            Quaternion.identity
        );

        Animator animArbol = nuevoArbol.GetComponent<Animator>();

        if (animArbol != null)
        {
            animArbol.enabled = true;
            animArbol.Play(0, 0, 0f);
        }

        yaSembrado = true;

        // Registrar el árbol en su zona
        counterManager.RegistrarArbol();

        Debug.Log("Árbol sembrado correctamente");
    }
}
