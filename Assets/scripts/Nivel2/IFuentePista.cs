using UnityEngine;

// Cualquier script que le pueda dar pistas al Duende implementa esto.
// Nivel 1: PreguntasMohan.  Nivel 2: ControlNivel2 (parches) y PreguntasGuardian (preguntas).
public interface IFuentePista
{
    // ¿Hay botones de pregunta esperando respuesta?
    bool PreguntaActiva { get; }

    // Descarta una opción incorrecta y devuelve la frase de pista (null si no se pudo)
    string UsarPista();

    // Qué objeto iluminar cerca del jugador (null si no hay nada)
    GameObject ObjetivoParaPista(Vector3 posJugador);
}
