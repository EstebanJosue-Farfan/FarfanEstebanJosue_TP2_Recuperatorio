using UnityEngine;

/// <summary>
/// Opción D: zona de barro. Reduce la velocidad del jugador al entrar y la restablece al salir.
/// Necesita un Collider con "Is Trigger" activado.
/// </summary>
public class ZonaBarro : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float multiplicadorVelocidad = 0.4f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        pc.SetMultiplicadorZona(multiplicadorVelocidad);
        Debug.Log("[ZonaBarro] Jugador en el barro: velocidad reducida");
        if (GameManager.Instance != null) GameManager.Instance.MostrarMensaje("¡Barro! Velocidad reducida");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        pc.SetMultiplicadorZona(1f);
        Debug.Log("[ZonaBarro] Jugador salió del barro: velocidad normal");
    }
}
