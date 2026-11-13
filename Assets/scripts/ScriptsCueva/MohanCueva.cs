using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Va en el MOHÁN (el mismo del río). Se encarga de llevarlo a la cueva
// y de sus reacciones a lo que hace el joven con la mochila.
public class MohanCueva : MonoBehaviour
{
    [Header("Referencias")]
    public MochilaJugador mochila;
    public Text textoMohan;                 // el mismo texto sobre su cabeza
    public Animator animator;

    [Header("Posición en la cueva")]
    [Tooltip("Objeto vacío en la esquina de la cueva donde se sienta el Mohán")]
    public Transform puntoEnCueva;

    [Header("Animaciones (deja vacío si aún no existe)")]
    public string animIdle = "idle_mohan";
    public string animSonrie = "mohan_sonriendo";
    public string animAsentir = "";
    public string animLevantarMano = "";
    public float duracionReaccion = 2f;

    [Header("Frases")]
    [TextArea(1, 3)] public string fraseSabio = "Sabio es quien toma lo necesario.";
    [TextArea(1, 3)] public string fraseMochilaLlena = "Tu mochila ya está llena.";

    private Coroutine rutinaReaccion;

    private void OnEnable()
    {
        if (mochila == null) return;
        mochila.AlTomar += OnTomar;
        mochila.AlIntentarLlenarDeMas += LevantarMano;
    }

    private void OnDisable()
    {
        if (mochila == null) return;
        mochila.AlTomar -= OnTomar;
        mochila.AlIntentarLlenarDeMas -= LevantarMano;
    }

    // ---- Acciones ----

    public void IrALaCueva()
    {
        if (puntoEnCueva != null)
        {
            transform.position = puntoEnCueva.position;
            transform.rotation = puntoEnCueva.rotation;
        }
        Decir("");                       // al entrar solo observa
        Reproducir(animIdle);
    }

    public void Silencio()
    {
        Decir("");
    }

    public void Sonreir()
    {
        Reaccionar(animSonrie, fraseSabio);
    }

    public void Asentir()
    {
        Reaccionar(animAsentir, "");
    }

    public void LevantarMano()
    {
        Reaccionar(animLevantarMano, fraseMochilaLlena);
    }

    // ---- Interno ----

    private void OnTomar(ObjetoCueva obj)
    {
        if (obj.tipo == TipoObjetoCueva.Tesoro) Silencio();
    }

    private void Reaccionar(string animacion, string frase)
    {
        if (rutinaReaccion != null) StopCoroutine(rutinaReaccion);
        rutinaReaccion = StartCoroutine(RutinaReaccion(animacion, frase));
    }

    private IEnumerator RutinaReaccion(string animacion, string frase)
    {
        Decir(frase);
        Reproducir(animacion);
        yield return new WaitForSeconds(duracionReaccion);
        Reproducir(animIdle);
        rutinaReaccion = null;
    }

    private void Decir(string texto)
    {
        if (textoMohan != null) textoMohan.text = texto;
    }

    private void Reproducir(string estado)
    {
        if (animator == null || string.IsNullOrEmpty(estado)) return;
        if (animator.HasState(0, Animator.StringToHash(estado)))
            animator.Play(estado, 0, 0f);
        else
            Debug.LogWarning($"MohanCueva: el Animator no tiene el estado '{estado}'.");
    }
}
