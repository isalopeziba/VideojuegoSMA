using UnityEngine;
using UnityEngine.InputSystem;

public class DuendeNPC : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    [Header("Seguimiento")]
    public Transform jugador;
    public float distanciaSeguimiento = 1.5f;  
    public float margenArranque = 0.3f;       
    public float desplazamientoLateral = 0.8f; 
    public float velocidad = 5f;
    public LayerMask terrainLayer;     
    public float groundDist = 0.5f;

    [Header("Sprite y animaciones")]
    public SpriteRenderer spriteDuende;
    public Animator animator;
    public string animIdle = "duende_idle";
    public string animLado = "duende_camina";
    public string animAtras = "duende_detras";
    public string animFrente = "duende_frente";
    public string animSenalando = "duende_senalando";

    [Header("Ayuda")]
    public GameObject panelAyuda;

    private Vector3 direccionJugador = Vector3.right;
    private Vector3 posAnteriorJugador;
    private float lado = 1f;            // +1 derecha del niño, -1 izquierda
    private float lateralActual = 0f;   // se ajusta suave, sin saltos
    private bool siguiendo = false;
    private string estadoActual = "";

    private Vector3 velSuavizada = Vector3.zero;
    private bool estaCaminando = false;



    public PreguntasMohan preguntasMohan;


    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        if (panelAyuda != null) panelAyuda.SetActive(false);
        if (jugador != null) posAnteriorJugador = jugador.position;
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
       
        if (preguntasMohan != null && preguntasMohan.PreguntaActiva)
        {
            preguntasMohan.UsarPista();
            return;
        }

        if (panelAyuda != null) panelAyuda.SetActive(!panelAyuda.activeSelf);
    }

    private void Update()
    {
        if (jugador == null) return;


        Vector3 movJugador = jugador.position - posAnteriorJugador;
        movJugador.y = 0f;
        posAnteriorJugador = jugador.position;
        if (Time.deltaTime > 0f && movJugador.magnitude / Time.deltaTime > 0.5f)
            direccionJugador = movJugador.normalized;

      
        float dx = transform.position.x - jugador.position.x;
        if (Mathf.Abs(dx) > 0.2f) lado = Mathf.Sign(dx);

       
        float lateralDeseado = desplazamientoLateral * Mathf.Abs(direccionJugador.z);
        lateralActual = Mathf.MoveTowards(lateralActual, lateralDeseado, 1.5f * Time.deltaTime);

        Vector3 ancla = jugador.position + Vector3.right * lado * lateralActual;

       
        Vector3 haciaAncla = ancla - transform.position;
        haciaAncla.y = 0f;
        float distancia = haciaAncla.magnitude;

        
        if (!siguiendo && distancia > distanciaSeguimiento + margenArranque) siguiendo = true;
        else if (siguiendo && distancia <= distanciaSeguimiento) siguiendo = false;

        Vector3 posAntes = transform.position;

        if (siguiendo)
        {
            Vector3 objetivo = ancla - haciaAncla.normalized * distanciaSeguimiento;
            objetivo.y = transform.position.y; // se mueve solo en horizontal; la altura la pone el suelo

            float exceso = distancia - distanciaSeguimiento;
            float vel = Mathf.Max(velocidad, exceso * 3f);
            transform.position = Vector3.MoveTowards(transform.position, objetivo, vel * Time.deltaTime);
        }

        AjustarAlSuelo();

        // 5. Animación
        Vector3 movDuende = transform.position - posAntes;
        movDuende.y = 0f;
        ActualizarAnimacion(movDuende);
    }

    private void ActualizarAnimacion(Vector3 movDuende)
    {
        if (Time.deltaTime <= 0f) return;

        Vector3 velActual = movDuende / Time.deltaTime;
        velSuavizada = Vector3.Lerp(velSuavizada, velActual, 8f * Time.deltaTime);
        float rapidez = velSuavizada.magnitude;

        if (!estaCaminando && rapidez > 1f) estaCaminando = true;
        else if (estaCaminando && rapidez < 0.3f) estaCaminando = false;

        if (estaCaminando)
        {
            float absX = Mathf.Abs(velSuavizada.x);
            float absZ = Mathf.Abs(velSuavizada.z);

            bool vaVertical;
            if (estadoActual == animLado) vaVertical = absZ > absX * 1.5f;
            else if (estadoActual == animAtras || estadoActual == animFrente) vaVertical = absZ * 1.5f > absX;
            else vaVertical = absZ >= absX;

            if (vaVertical)
            {
                CambiarAnimacion(velSuavizada.z > 0 ? animAtras : animFrente);
            }
            else
            {
                CambiarAnimacion(animLado);
                Voltear(velSuavizada.x);
            }
        }
        else
        {
            
            Vector3 haciaJugador = jugador.position - transform.position;
            Voltear(haciaJugador.x);

            if (panelAyuda != null && panelAyuda.activeSelf)
                CambiarAnimacion(animSenalando);
            else
                CambiarAnimacion(animIdle);
        }
    }

    private void Voltear(float x)
    {
        if (spriteDuende == null) return;
        if (x < -0.05f) spriteDuende.flipX = true;
        else if (x > 0.05f) spriteDuende.flipX = false;
    }

    private void CambiarAnimacion(string nuevoEstado)
    {
        if (animator == null || nuevoEstado == estadoActual) return;
        animator.Play(nuevoEstado);
        estadoActual = nuevoEstado;
    }

    private void AjustarAlSuelo()
    {
   
        float alturaInicio = Mathf.Max(transform.position.y, jugador.position.y) + 3f;
        Vector3 castPos = new Vector3(transform.position.x, alturaInicio, transform.position.z);

        RaycastHit hit;
        if (Physics.Raycast(castPos, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y + groundDist;
            transform.position = p;
        }
        else
        {
            
            Vector3 p = transform.position;
            p.y = jugador.position.y;
            transform.position = p;
        }
    }
}