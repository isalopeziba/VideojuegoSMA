
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;

    public Vector2 InputDirection { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Joystick: clic detectado");
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            float radius = background.rect.width / 2f;

            if (radius <= 0f) return;

            InputDirection = Vector2.ClampMagnitude(
                position / radius, 1f);

            handle.anchoredPosition = InputDirection * radius;

            Debug.Log("Joystick: " + InputDirection);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Joystick: soltado");
        InputDirection = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
