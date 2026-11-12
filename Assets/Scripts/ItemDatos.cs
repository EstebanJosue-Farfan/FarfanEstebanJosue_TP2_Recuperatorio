using UnityEngine;


public class ItemDatos : MonoBehaviour
{
    public int puntos = 5;
    public float velocidadGiro = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, velocidadGiro * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || !GameManager.Instance.EstaJugando) return;

        GameManager.Instance.SumarPuntos(puntos);
        Debug.Log($"[ItemDatos] Recolectado {name}: +{puntos}");
        Destroy(gameObject);
    }
}
