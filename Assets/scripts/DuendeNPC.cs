using UnityEngine;
using UnityEngine.InputSystem;

public class DuendeNPC : MonoBehaviour
{
    
    private InputSystem_Actions inputActions;

   
    public Transform jugador;
    public float distanciaSeguimiento = 1.5f; 
    public float suavizado = 5f;              
    public float alturaOffset = 0f;           

    
    public SpriteRenderer spriteDuende;

   
    public GameObject panelAyuda;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
       
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
        
        panelAyuda.SetActive(!panelAyuda.activeSelf);
    }

    private void Update()
    {
        if (jugador == null) return;

        // Dirección del duende hacia el jugador 
        Vector3 haciaJugador = jugador.position - transform.position;
        haciaJugador.y = 0f;
        float distancia = haciaJugador.magnitude;

        
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
