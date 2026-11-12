using System.Collections;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int cantidadCura = 25;
    public float recarga = 15f;
    public float velocidadGiro = 90f;

    Renderer rend;
    Collider col;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, velocidadGiro * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other) { Intentar(other); }
    void OnTriggerStay(Collider other) { Intentar(other); }

    void Intentar(Collider other)
    {
        if (col == null || !col.enabled) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth vida = other.GetComponentInParent<PlayerHealth>();
        if (vida == null || !vida.EstaHerido) return; // con vida llena no lo gasta

        int curado = vida.Curar(cantidadCura);
        Debug.Log($"[PowerUp] +{curado} de vida");
        if (GameManager.Instance != null) GameManager.Instance.MostrarMensaje($"+{curado} VIDA", 1.5f);

        StartCoroutine(Recargar());
    }

    IEnumerator Recargar()
    {
        if (rend != null) rend.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(recarga);
        if (rend != null) rend.enabled = true;
        col.enabled = true;
    }
}
