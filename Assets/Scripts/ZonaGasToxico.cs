using UnityEngine;

/// <summary>
/// Opción F: zona de gas tóxico. Resta vida de a poco mientras el jugador esté dentro.
/// Necesita un Collider con "Is Trigger" activado.
/// </summary>
public class ZonaGasToxico : MonoBehaviour
{
    public int danioPorTick = 5;
    public float intervalo = 1f;

    PlayerHealth jugadorDentro;
    float proximoTick;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro = other.GetComponentInParent<PlayerHealth>();
        proximoTick = Time.time + intervalo;
        Debug.Log("[ZonaGasToxico] Jugador entró al gas");
        if (GameManager.Instance != null) GameManager.Instance.MostrarMensaje("¡Gas tóxico!");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro = null;
        Debug.Log("[ZonaGasToxico] Jugador salió del gas");
    }

    void Update()
    {
        if (jugadorDentro == null || Time.time < proximoTick) return;
        proximoTick = Time.time + intervalo;

        // El gas tiene su propio ritmo, por eso ignora la inmunidad de golpes
        jugadorDentro.RecibirDanio(danioPorTick, true);
    }
}
