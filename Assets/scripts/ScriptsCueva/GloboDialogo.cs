using UnityEngine;
using UnityEngine.UI;
public class GloboDialogo : MonoBehaviour
{
    [Tooltip("El canvas (o la imagen del globo) que se muestra y se oculta")]
    public GameObject globo;

    [Tooltip("El texto del globo")]
    public Text texto;

    private void LateUpdate()
    {
        if (globo == null || texto == null) return;

        bool hayTexto = !string.IsNullOrWhiteSpace(texto.text);
        if (globo.activeSelf != hayTexto)
            globo.SetActive(hayTexto);
    }
}
