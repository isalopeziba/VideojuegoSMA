using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class PortalNivel : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena del siguiente nivel (debe estar en Build Profiles)")]
    public string escenaSiguiente = "";

    private bool cargando = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cargando || !other.CompareTag("Player")) return;

        if (string.IsNullOrEmpty(escenaSiguiente))
        {
            Debug.Log("PortalNivel: el jugador entró al portal, pero no hay 'Escena Siguiente' asignada.");
            return;
        }

        cargando = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaSiguiente);
    }
}
