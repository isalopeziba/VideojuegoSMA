using UnityEngine;

public class SineWaveMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 2f;

    private float startY;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        float x = transform.position.x + speed * Time.deltaTime;

        float y = startY + Mathf.Sin(Time.time * frequency) * amplitude;

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
