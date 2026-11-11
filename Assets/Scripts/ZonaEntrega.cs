using UnityEngine;

public class ZonaEntrega : MonoBehaviour
{
    public Color colorVictoria = Color.green;
    public ParticleSystem particulas;

    bool entregado;

    void OnTriggerEnter(Collider other)
    {
        Revisar(other);
    }

    void OnTriggerStay(Collider other)
    {
        Revisar(other);
    }

    void Revisar(Collider other)
    {
        Debug.Log("Entró algo a la zona: " + other.name);

        if (entregado) return;

        Transportable t = other.GetComponent<Transportable>();
        if (t == null)
        {
            Debug.Log("No es la caja");
            return;
        }
        if (t.EstaLlevado)
        {
            Debug.Log("La caja todavía está en la mano");
            return;
        }

        entregado = true;
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = colorVictoria;
        if (particulas != null) particulas.Play();
        GameManager.Instance.Victoria();
    }
}