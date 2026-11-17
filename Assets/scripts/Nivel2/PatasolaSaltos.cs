using System.Collections;
using UnityEngine;

// Va en la PATASOLA (objeto padre). Cuando se acaba el tiempo de una zona,
// aparece y la cruza dando saltos. Nunca toca al joven: va por arriba.
// Quítale los colliders sólidos (o déjalos como trigger) para que no lo empuje.
public class PatasolaSaltos : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer sprite;
    [Tooltip("Estado del Animator mientras salta (vacío si no hay)")]
    public string animSalto = "";

    public int cantidadSaltos = 4;
    public float alturaSalto = 2f;
    public float duracion = 2.5f;

    [Header("Sonido opcional")]
    public AudioSource audioSource;
    public AudioClip sonidoSalto;

    [Tooltip("Ocultarla al terminar de cruzar")]
    public bool ocultarAlTerminar = true;

    private void Start()
    {
        if (ocultarAlTerminar) gameObject.SetActive(false);
    }

    // La llama ControlNivel2 con StartCoroutine desde su propio objeto
    public IEnumerator PasarPor(Transform desde, Transform hasta)
    {
        if (desde == null || hasta == null) yield break;

        gameObject.SetActive(true);
        transform.position = desde.position;
        if (sprite != null) sprite.flipX = hasta.position.x < desde.position.x;
        if (animator != null && !string.IsNullOrEmpty(animSalto)) animator.Play(animSalto, 0, 0f);

        int ultimoSalto = -1;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracion);

            Vector3 pos = Vector3.Lerp(desde.position, hasta.position, p);
            pos.y += Mathf.Abs(Mathf.Sin(p * Mathf.PI * cantidadSaltos)) * alturaSalto;
            transform.position = pos;

            int salto = Mathf.FloorToInt(p * cantidadSaltos);
            if (salto != ultimoSalto && audioSource != null && sonidoSalto != null)
                audioSource.PlayOneShot(sonidoSalto);
            ultimoSalto = salto;

            yield return null;
        }

        transform.position = hasta.position;
        if (ocultarAlTerminar) gameObject.SetActive(false);
    }
}
