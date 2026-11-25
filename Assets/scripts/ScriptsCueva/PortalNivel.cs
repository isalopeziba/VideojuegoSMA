using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Collider))]
public class PortalNivel : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena (debe estar MARCADA en Build Profiles)")]
    public string escenaSiguiente = "";

    [Tooltip("Respaldo: número de la escena en Build Profiles (ej. 4 para NIVEL2FINAL). -1 = no usar")]
    public int indiceEscenaRespaldo = -1;

    [Header("Prueba en el celular (opcional)")]
    [Tooltip("Un Text en pantalla para ver en el celular qué está pasando. Quítalo para la entrega.")]
    public Text textoDebug;

    private bool cargando = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnEnable()
    {
        Mostrar($"Portal activo → '{escenaSiguiente}'");
    }

    private void OnTriggerEnter(Collider other)
    {
        Mostrar($"Algo entró al portal: {other.name} (tag: {other.tag})");

        if (cargando || !other.CompareTag("Player")) return;
        cargando = true;

        Time.timeScale = 1f;
        StartCoroutine(Cargar());
    }

    private IEnumerator Cargar()
    {
        AsyncOperation op = null;

        // 1. Por nombre, si existe en la APK
        if (!string.IsNullOrEmpty(escenaSiguiente) && Application.CanStreamedLevelBeLoaded(escenaSiguiente))
        {
            Mostrar($"Cargando '{escenaSiguiente}'...");
            op = SceneManager.LoadSceneAsync(escenaSiguiente);
        }
        // 2. Si el nombre no existe, por número
        else if (indiceEscenaRespaldo >= 0 && indiceEscenaRespaldo < SceneManager.sceneCountInBuildSettings)
        {
            Mostrar($"No existe '{escenaSiguiente}' en la APK. Cargando la escena #{indiceEscenaRespaldo}...");
            op = SceneManager.LoadSceneAsync(indiceEscenaRespaldo);
        }
        else
        {
            Mostrar($"ERROR: la escena '{escenaSiguiente}' no está en la APK y no hay número de respaldo.");
            cargando = false;
            yield break;
        }

        while (op != null && !op.isDone)
        {
            Mostrar($"Cargando... {Mathf.RoundToInt(op.progress * 100)} %");
            yield return null;
        }
    }

    private void Mostrar(string mensaje)
    {
        Debug.Log("PortalNivel: " + mensaje);
        if (textoDebug != null) textoDebug.text = mensaje;
    }
}