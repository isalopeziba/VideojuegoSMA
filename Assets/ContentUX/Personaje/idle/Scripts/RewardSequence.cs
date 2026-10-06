using System.Collections;
using UnityEngine;

public class RewardSequence : MonoBehaviour
{
    [System.Serializable]
    public class Reward
    {
        public string rewardName;          // solo para identificarlo en el Inspector
        public CanvasGroup canvasGroup;    // el GameObject de la imagen debe tener un CanvasGroup
    }

    [Header("Recompensas en orden de aparición")]
    public Reward[] rewards;

    [Header("Tiempos")]
    public float fadeDuration = 0.5f;      // duración del fundido de entrada/salida
    public float displayDuration = 1.2f;   // cuánto se queda visible antes de pasar a la siguiente

    [Header("Comportamiento")]
    [Tooltip("Si está activo, cada recompensa desaparece antes de que aparezca la siguiente. Si está apagado, se van acumulando en pantalla.")]
    public bool stackRewards = false;

    [Tooltip("Si está activo, la secuencia arranca sola al entrar a la escena.")]
    public bool autoStart = true;

    void Start()
    {
        // Aseguramos que todas empiecen invisibles
        foreach (var r in rewards)
        {
            if (r.canvasGroup != null)
                r.canvasGroup.alpha = 0f;
        }

        if (autoStart)
            StartCoroutine(PlaySequence());
    }

    public IEnumerator PlaySequence()
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            var reward = rewards[i];

            // Aparece
            yield return StartCoroutine(Fade(reward.canvasGroup, 0f, 1f, fadeDuration));

            // Se queda visible
            yield return new WaitForSeconds(displayDuration);

            // Si están configuradas para no acumularse, se oculta antes de pasar a la siguiente
            if (stackRewards && i < rewards.Length - 1)
            {
                yield return StartCoroutine(Fade(reward.canvasGroup, 1f, 0f, fadeDuration));
            }
        }
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    // Útil si quieres disparar la secuencia manualmente (ej. desde un evento de desbloqueo de mito)
    public void PlayManually()
    {
        StopAllCoroutines();
        StartCoroutine(PlaySequence());
    }
}