using UnityEngine;
using UnityEngine.InputSystem;

public class TerrainTestPlayer : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1f;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1f;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1f;
        }

        input = Vector2.ClampMagnitude(input, 1f);

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
    }
}