using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Transportable : MonoBehaviour
{
    public PlataformaIntermitente[] plataformasRespawn;
    public float alturaCaida = -8f;

    public bool EstaLlevado { get; private set; }

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (!EstaLlevado && transform.position.y < alturaCaida) Reaparecer();
    }

    public void Recoger(Transform punto)
    {
        if (EstaLlevado) return;
        EstaLlevado = true;
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(punto);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Soltar()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
        EstaLlevado = false;
    }

    void Reaparecer()
    {
        foreach (PlataformaIntermitente p in plataformasRespawn)
        {
            if (p.EstaActiva)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                transform.position = p.transform.position + Vector3.up * 1.5f;
                return;
            }
        }
    }
}