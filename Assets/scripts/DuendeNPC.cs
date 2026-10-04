using UnityEngine;
using UnityEngine.InputSystem;

public class DuendeNPC : MonoBehaviour
{
    // Referencia al sistema de acciones (clase generada)
    private InputSystem_Actions inputActions;

    [Header("Seguimiento")]
    public Transform jugador;
    public float distanciaSeguimiento = 1.5f; // Qué tan atrás se queda
    public float suavizado = 5f;              // Qué tan rápido lo alcanza
    public float alturaOffset = 0f;           // Ajuste de altura respecto al jugador

    [Header("Visual")]
    public SpriteRenderer spriteDuende;

    [Header("Panel de ayuda")]
    public GameObject panelAyuda;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        // El panel empieza oculto
        if (panelAyuda != null) panelAyuda.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Ayuda.performed += OnAyuda;
    }

    private void OnDisable()
    {
        inputActions.Player.Ayuda.performed -= OnAyuda;
        inputActions.Player.Disable();
    }

    private void OnAyuda(InputAction.CallbackContext context)
    {
        // Abre o cierra el panel
        panelAyuda.SetActive(!panelAyuda.activeSelf);
    }

    private void Update()
    {
        if (jugador == null) return;

        // Dirección del duende hacia el jugador (ignorando la altura)
        Vector3 haciaJugador = jugador.position - transform.position;
        haciaJugador.y = 0f;
        float distancia = haciaJugador.magnitude;

        // Solo se mueve si el jugador se alejó más de la distancia de seguimiento
        if (distancia > distanciaSeguimiento)
        {
            Vector3 objetivo = jugador.position - haciaJugador.normalized * distanciaSeguimiento;
            objetivo.y = jugador.position.y + alturaOffset;

            transform.position = Vector3.Lerp(transform.position, objetivo, suavizado * Time.deltaTime);
        }

        // Voltear el sprite para que siempre mire hacia el jugador
        if (spriteDuende != null)
        {
            if (haciaJugador.x < -0.05f) spriteDuende.flipX = true;
            else if (haciaJugador.x > 0.05f) spriteDuende.flipX = false;
        }
    }
}
