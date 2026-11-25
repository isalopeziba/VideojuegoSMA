using UnityEngine;

public class salir : MonoBehaviour
{
    public void Salir()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   
#else
        Application.Quit();                                 // en el celular o PC cierra el juego
#endif
    }
}
