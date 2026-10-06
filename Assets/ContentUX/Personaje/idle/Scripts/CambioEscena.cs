using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar (debe estar agregada en File > Build Settings)")]
    public string sceneName;

    // Conecta este método al evento OnClick() del botón en el Inspector
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    // Alternativa si prefieres cargar por índice de Build Settings en vez de nombre
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}

