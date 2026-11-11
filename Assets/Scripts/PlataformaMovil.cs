using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlataformaMovil : MonoBehaviour
{
    public Vector3 desplazamiento = new Vector3(6, 0, 0);
    public float velocidad = 2f;
    public float pausaEnExtremos = 1.5f;

    Rigidbody rb;
    Vector3 puntoA, puntoB, destino;
    bool esperando;
    Vector3 delta;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Start()
    {
        puntoA = transform.position;
        puntoB = puntoA + desplazamiento;
        destino = puntoB;
    }

    void FixedUpdate()
    {
        delta = Vector3.zero;
        if (esperando) return;

        Vector3 nueva = Vector3.MoveTowards(rb.position, destino, velocidad * Time.fixedDeltaTime);
        delta = nueva - rb.position;
        rb.MovePosition(nueva);

        if (Vector3.Distance(nueva, destino) < 0.01f)
        {
            esperando = true;
            Invoke(nameof(CambiarDireccion), pausaEnExtremos);
        }
    }

    void CambiarDireccion()
    {
        destino = (destino == puntoB) ? puntoA : puntoB;
        esperando = false;
    }

    void OnCollisionStay(Collision c)
    {
        if (c.gameObject.CompareTag("Player") && c.rigidbody != null)
            c.rigidbody.position += delta;
    }
}