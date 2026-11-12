using UnityEngine;

/// <summary>
/// Configura automáticamente al jugador: tag, capa, Rigidbody, Collider y todos los scripts
/// con sus LayerMask. Se ejecuta solo al agregar este componente, o desde el menú de los
/// tres puntitos del componente -> "Configurar Jugador".
/// Requiere que existan las capas: Ground, Interactable, Enemy y Player.
/// </summary>
public class PlayerSetup : MonoBehaviour
{
    void Reset()
    {
        Configurar();
    }

    [ContextMenu("Configurar Jugador")]
    public void Configurar()
    {
        gameObject.tag = "Player";

        int capaPlayer = LayerMask.NameToLayer("Player");
        if (capaPlayer >= 0) gameObject.layer = capaPlayer;
        else Debug.LogWarning("[PlayerSetup] Falta crear la capa 'Player'");

        Rigidbody rb = Asegurar<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (GetComponent<Collider>() == null) gameObject.AddComponent<CapsuleCollider>();

        Asegurar<PlayerHealth>();

        LayerMask suelo = LayerMask.GetMask("Ground");
        LayerMask impacto = LayerMask.GetMask("Default", "Ground", "Enemy");
        LayerMask interactuable = LayerMask.GetMask("Interactable");

        PlayerController pc = Asegurar<PlayerController>();
        pc.capaSuelo = suelo;

        ArmaJugador arma = Asegurar<ArmaJugador>();
        arma.capasImpacto = impacto;

        ArmaAturdidora aturdidor = Asegurar<ArmaAturdidora>();
        aturdidor.capasImpacto = impacto;

        InteraccionRaycast interaccion = Asegurar<InteraccionRaycast>();
        interaccion.capaInteractuable = interactuable;

        if (suelo == 0) Debug.LogWarning("[PlayerSetup] Falta crear la capa 'Ground'");
        if (interactuable == 0) Debug.LogWarning("[PlayerSetup] Falta crear la capa 'Interactable'");
        if (LayerMask.NameToLayer("Enemy") < 0) Debug.LogWarning("[PlayerSetup] Falta crear la capa 'Enemy'");

        Debug.Log("[PlayerSetup] Jugador configurado");
    }

    T Asegurar<T>() where T : Component
    {
        T c = GetComponent<T>();
        if (c == null) c = gameObject.AddComponent<T>();
        return c;
    }
}
