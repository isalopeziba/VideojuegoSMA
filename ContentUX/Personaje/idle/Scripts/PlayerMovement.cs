using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public VirtualJoystick joystick;
    public float speed = 5f;

    [Header("Volteo (flip) del sprite paper-cutout")]
    [Tooltip("Arrastra aquí el objeto hijo que contiene el modelo/sprite visual. Si lo dejas vacío, se voltea este mismo transform.")]
    public Transform visual;

    private Rigidbody rb;
    private Animator animator;
    private float facingDirection = 1f; // 1 = derecha, -1 = izquierda

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (visual == null)
            visual = transform;
    }

    void FixedUpdate()
    {
        Vector2 input = joystick.InputDirection;

        Vector3 movement = new Vector3(
            input.x,
            0f,
            input.y
        );

        rb.linearVelocity = new Vector3(
            movement.x * speed,
            rb.linearVelocity.y,
            movement.z * speed
        );

        bool isWalking = input.magnitude > 0.1f;
        animator.SetBool("isWalking", isWalking);

        // Solo volteamos según el movimiento horizontal (input.x).
        // El movimiento vertical (input.y / eje Z) mueve al personaje
        // pero NUNCA rota el sprite, así se mantiene siempre "de frente".
        if (Mathf.Abs(input.x) > 0.1f)
        {
            facingDirection = Mathf.Sign(input.x);

            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            visual.localScale = scale;
        }
    }
}