using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Obstaculo : MonoBehaviour
{
    public int danio = 20;

    Rigidbody rb;
    bool yaGolpeo;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (yaGolpeo) return;

        PlayerHealth vida = c.gameObject.GetComponentInParent<PlayerHealth>();
        if (vida != null)
        {
            yaGolpeo = true;
            vida.RecibirDanio(danio);
            Destroy(gameObject);
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}