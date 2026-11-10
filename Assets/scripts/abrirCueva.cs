using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para la lectura de teclas en el nuevo Input System

public class abrirCueva : MonoBehaviour
{
    [Header("Referencias de Objetos (Pared / Escenario)")]
    [Tooltip("Lista de GameObjects que se moverán en el eje -Z (ej: Paredes, rocas, etc.)")]
    public List<Transform> objetosAMover = new List<Transform>();

    [Tooltip("Distancia que se desplazarán los objetos hacia atrás en el eje Z (valor positivo)")]
    public float distanciaZ = 5f;

    [Tooltip("Tiempo en segundos que tardará en completarse el movimiento")]
    public float duracionMovimiento = 2.5f;

    [Header("Configuración de Temblor de Cámara")]
    [Tooltip("Cámara principal que temblará (Si se deja vacía, usará Camera.main)")]
    public Transform camaraTransform;

    [Tooltip("Intensidad o fuerza del temblor")]
    public float fuerzaTemblor = 0.003f;

    [Header("Sonido Opcional")]
    public AudioSource audioSource;
    public AudioClip sonidoDerrumbeOMovimiento;

    private Vector3 posicionOriginalCamara;
    private bool enMovimiento = false;

    private void Start()
    {
        // Si no asignaste una cámara en el Inspector, busca la MainCamera automáticamente
        if (camaraTransform == null && Camera.main != null)
        {
            camaraTransform = Camera.main.transform;
        }

        if (camaraTransform != null)
        {
            posicionOriginalCamara = camaraTransform.localPosition;
        }
    }

    private void Update()
    {
        // Tecla N para pruebas rápidas en el editor
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            Debug.Log("Prueba manual activada con tecla N");
            ActivarEfectoFinal();
        }
    }

    // Este método PÚBLICO se conecta al evento alTerminar del Mohán
    public void ActivarEfectoFinal()
    {
        if (!enMovimiento)
        {
            StartCoroutine(RutinaMovimientoYTemblor());
        }
    }

    private IEnumerator RutinaMovimientoYTemblor()
    {
        enMovimiento = true;

        // Reproduce sonido de derrumbe/vibración si está asignado
        if (audioSource != null && sonidoDerrumbeOMovimiento != null)
        {
            audioSource.PlayOneShot(sonidoDerrumbeOMovimiento);
        }

        // Guardar posiciones iniciales y calcular destino final en -Z de cada objeto
        List<Vector3> posIniciales = new List<Vector3>();
        List<Vector3> posFinales = new List<Vector3>();

        foreach (Transform obj in objetosAMover)
        {
            if (obj != null)
            {
                posIniciales.Add(obj.position);
                // Calcula el movimiento al restar distanciaZ sobre el eje Z global
                posFinales.Add(obj.position - new Vector3(0, 0, distanciaZ));
            }
        }

        float tiempo = 0f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;
            float porcentaje = Mathf.Clamp01(tiempo / duracionMovimiento);

            // 1. Mover suavemente los objetos en -Z
            for (int i = 0; i < objetosAMover.Count; i++)
            {
                if (objetosAMover[i] != null)
                {
                    objetosAMover[i].position = Vector3.Lerp(posIniciales[i], posFinales[i], porcentaje);
                }
            }

            // 2. Aplicar el temblor (Shake) aleatorio en la cámara
            if (camaraTransform != null)
            {
                Vector3 puntoAleatorio = posicionOriginalCamara + (Vector3)Random.insideUnitCircle * fuerzaTemblor;
                camaraTransform.localPosition = puntoAleatorio;
            }

            yield return null;
        }

        // Asegurar que queden en su posición final exacta al terminar
        for (int i = 0; i < objetosAMover.Count; i++)
        {
            if (objetosAMover[i] != null)
            {
                objetosAMover[i].position = posFinales[i];
            }
        }

        // Regresar la cámara a su posición original
        if (camaraTransform != null)
        {
            camaraTransform.localPosition = posicionOriginalCamara;
        }

        enMovimiento = false;
    }
}