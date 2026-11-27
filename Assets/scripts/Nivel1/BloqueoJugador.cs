using UnityEngine;


public static class BloqueoJugador
{
    public static void Aplicar(PlayerMov movimiento, bool bloquear)
    {
        if (movimiento == null) return;

        movimiento.enabled = !bloquear;

        if (bloquear)
        {
            // Quieto y en idle
            Rigidbody rb = movimiento.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            if (movimiento.animator != null && !string.IsNullOrEmpty(movimiento.animIdle))
                movimiento.animator.Play(movimiento.animIdle, 0, 0f);
        }
    }
}
