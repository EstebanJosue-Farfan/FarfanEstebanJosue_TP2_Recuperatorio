using UnityEngine;

public class PlataformaIntermitente : MonoBehaviour
{
    public float tiempoVisible = 5f;
    public float tiempoOculto = 3f;
    public float aviso = 1f;
    public float desfaseInicial = 0f;

    public bool EstaActiva { get; private set; } = true;

    Renderer rend;
    Collider col;
    Color colorOriginal;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        colorOriginal = rend.material.color;
    }

    void Start()
    {
        float t = tiempoVisible + desfaseInicial;
        Invoke(nameof(Advertir), Mathf.Max(0f, t - aviso));
        Invoke(nameof(Ocultar), t);
    }

    void Advertir() { rend.material.color = Color.red; }

    void Ocultar()
    {
        EstaActiva = false;
        rend.enabled = false;
        col.enabled = false;
        Invoke(nameof(Mostrar), tiempoOculto);
    }

    void Mostrar()
    {
        EstaActiva = true;
        rend.enabled = true;
        col.enabled = true;
        rend.material.color = colorOriginal;
        Invoke(nameof(Advertir), Mathf.Max(0f, tiempoVisible - aviso));
        Invoke(nameof(Ocultar), tiempoVisible);
    }
}