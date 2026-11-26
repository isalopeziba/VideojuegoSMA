using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnSkip : MonoBehaviour
{
    public void IrAEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}