using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Obstaculo : MonoBehaviour
{
    Rigidbody rb;
    bool yaGolpeo;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (yaGolpeo) return;

        if (c.gameObject.GetComponentInParent<PlayerController>() != null)
        {
            yaGolpeo = true;
            if (GameManager.Instance != null) GameManager.Instance.Perder();
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}