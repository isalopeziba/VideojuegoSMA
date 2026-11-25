using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(10000)]
public class abrirCueva : MonoBehaviour
{
    [Header("Objetos de la puerta")]
    [Tooltip("Paredes, rocas, etc. que se mueven. Deben empezar en su posición CERRADA.")]
    public List<Transform> objetosAMover = new List<Transform>();
    [Tooltip("Cuánto se mueven hacia atrás en Z al abrir")]
    public float distanciaZ = 5f;
    public float duracionMovimiento = 2.5f;

    [Header("Temblor de cámara")]
    public Transform camaraTransform;
    [Tooltip("0.1 a 0.3 se nota bien")]
    public float fuerzaTemblor = 0.15f;

    [Header("Sonido opcional")]
    public AudioSource audioSource;
    public AudioClip sonidoMovimiento;

    [Header("Controles / Pruebas")]
    [Tooltip("Permite abrir o cerrar con la tecla N (PC) o tocando la pantalla (Celular)")]
    public bool permitirToqueYTecla = true;
    [Tooltip("Evita que se abra/cierre si tocas un botón de la UI o el Joystick")]
    public bool ignorarToquesEnUI = true;

    public bool EstaAbierta { get; private set; } = false;

    private readonly List<Vector3> posCerradas = new List<Vector3>();
    private Coroutine rutina;

    private bool temblando = false;
    private Vector3 offsetTemblor = Vector3.zero;

    private void Start()
    {
        if (camaraTransform == null && Camera.main != null)
            camaraTransform = Camera.main.transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        foreach (Transform obj in objetosAMover)
        {
            posCerradas.Add(obj != null ? obj.position : Vector3.zero);
            if (obj != null && obj.gameObject.isStatic)
                Debug.LogWarning($"abrirCueva: '{obj.name}' está marcado como Static y no se verá moverse.");
        }
    }

    private void Update()
    {
        // Quitar el temblor del frame anterior ANTES de que el script de la cámara la mueva
        QuitarTemblor();

        if (!permitirToqueYTecla) return;

        // 1. Entrada por teclado (PC)
        bool presionoTecla = Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame;

        // 2. Entrada táctil (Celular)
        bool tocoPantalla = false;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            bool sobreUI = ignorarToquesEnUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId);

            if (!sobreUI)
                tocoPantalla = true;
        }

        if (presionoTecla || tocoPantalla)
        {
            if (EstaAbierta) Cerrar(); else Abrir();
        }
    }

    private void LateUpdate()
    {
        // Sumar el temblor DESPUÉS de que el script de la cámara la movió
        if (temblando && camaraTransform != null)
        {
            offsetTemblor = (Vector3)(Random.insideUnitCircle * fuerzaTemblor);
            camaraTransform.position += offsetTemblor;
        }
    }

    private void QuitarTemblor()
    {
        if (camaraTransform != null && offsetTemblor != Vector3.zero)
        {
            camaraTransform.position -= offsetTemblor;
            offsetTemblor = Vector3.zero;
        }
    }

    // Se deja con este nombre porque ya está conectado al "Al Terminar" de PreguntasMohan
    public void ActivarEfectoFinal() { Abrir(); }

    public void Abrir()
    {
        if (EstaAbierta) return;
        EstaAbierta = true;
        Mover(abrir: true);
        Debug.Log("abrirCueva: abriendo");
    }

    public void Cerrar()
    {
        if (!EstaAbierta) return;
        EstaAbierta = false;
        Mover(abrir: false);
        Debug.Log("abrirCueva: cerrando");
    }

    private void Mover(bool abrir)
    {
        if (rutina != null) StopCoroutine(rutina);

        List<Vector3> destinos = new List<Vector3>();
        for (int i = 0; i < posCerradas.Count; i++)
            destinos.Add(abrir ? posCerradas[i] - new Vector3(0f, 0f, distanciaZ) : posCerradas[i]);

        rutina = StartCoroutine(RutinaMover(destinos));
    }

    private IEnumerator RutinaMover(List<Vector3> destinos)
    {
        if (audioSource != null && sonidoMovimiento != null)
            audioSource.PlayOneShot(sonidoMovimiento);

        List<Vector3> inicios = new List<Vector3>();
        foreach (Transform obj in objetosAMover)
            inicios.Add(obj != null ? obj.position : Vector3.zero);

        temblando = true;
        float tiempo = 0f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracionMovimiento));

            for (int i = 0; i < objetosAMover.Count; i++)
                if (objetosAMover[i] != null)
                    objetosAMover[i].position = Vector3.Lerp(inicios[i], destinos[i], p);

            yield return null;
        }

        for (int i = 0; i < objetosAMover.Count; i++)
            if (objetosAMover[i] != null)
                objetosAMover[i].position = destinos[i];

        temblando = false;
        rutina = null;
    }

    private void OnDisable()
    {
        temblando = false;
        QuitarTemblor();
    }
}