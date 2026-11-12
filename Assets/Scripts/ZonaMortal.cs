using UnityEngine;

public class ZonaMortal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth vida = other.GetComponentInParent<PlayerHealth>();
        if (vida != null)
        {
            Debug.Log("[ZonaMortal] El jugador tocó la zona mortal");
            vida.MatarInstantaneo();
        }
    }
}
