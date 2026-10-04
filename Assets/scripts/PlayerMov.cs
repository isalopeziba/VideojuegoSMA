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
    private Rigidbody rb;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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

        // Voltear el sprite según la dirección, falta poner la aniamcon correcta la de correr, que puse la de idle solo para probar
        if (moveInput.x < 0) spriteJugador.flipX = true;
        else if (moveInput.x > 0) spriteJugador.flipX = false;
    }

    private void FixedUpdate()
    {
        
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.linearVelocity = movement * speed;
    }
}