using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMov : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private Vector2 moveInput;
    public float speed = 5f;
    public float groundDist = 0.5f;

    public LayerMask terrainLayer;

   
    public SpriteRenderer spriteJugador; 
    public Animator animator;          

   
    public string animIdle = "idle";
    public string animLado = "walk";
    public string animAtras = "caminata_detras";
    public string animFrente = "caminata_frente";
    public string animRecoger = "recogiendo";

    
    public string nombreClipRecoger = "recogiendo"; 
    private float duracionRecoger = 0.5f;           
    private float tiempoRecogiendo = 0f;
    private bool estaRecogiendo = false;

    private Rigidbody rb;
    private string estadoActual = "";

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Buscar cuánto dura el clip de recoger
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == nombreClipRecoger)
                {
                    duracionRecoger = clip.length;
                    break;
                }
            }
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Interact.started += OnRecoger;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Interact.started -= OnRecoger;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnRecoger(InputAction.CallbackContext context)
    {
        if (estaRecogiendo) return; // no reiniciar si ya está recogiendo

        estaRecogiendo = true;
        tiempoRecogiendo = 0f;
        animator.Play(animRecoger, 0, 0f);
        estadoActual = animRecoger;

        
    }

    private void Update()
    {
        
        RaycastHit hit;
        Vector3 castPos = transform.position;
        castPos.y += 1;
        if (Physics.Raycast(castPos, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 movePos = transform.position;
            movePos.y = hit.point.y + groundDist;
            transform.position = movePos;
        }

        // Mientras recoge: esperar a que termine la animación
        if (estaRecogiendo)
        {
            tiempoRecogiendo += Time.deltaTime;
            if (tiempoRecogiendo >= duracionRecoger)
            {
                estaRecogiendo = false;
                estadoActual = ""; 
            }
            return; 
        }

        // Voltear el sprite según la dirección horizontal
        if (spriteJugador != null)
        {
            if (moveInput.x < -0.1f) spriteJugador.flipX = true;
            else if (moveInput.x > 0.1f) spriteJugador.flipX = false;
        }

        
        if (animator != null)
        {
            CambiarAnimacion(ElegirAnimacion());
        }
    }

    private string ElegirAnimacion()
    {
        if (moveInput.sqrMagnitude < 0.01f)
            return animIdle;

        if (Mathf.Abs(moveInput.y) >= Mathf.Abs(moveInput.x))
        {
            if (moveInput.y > 0) return animAtras;
            else return animFrente;
        }

        return animLado;
    }

    private void CambiarAnimacion(string nuevoEstado)
    {
        if (nuevoEstado == estadoActual) return;

        animator.Play(nuevoEstado);
        estadoActual = nuevoEstado;
    }

    private void FixedUpdate()
    {
        // Quieto mientras recoge
        if (estaRecogiendo)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.linearVelocity = movement * speed;
    }
}