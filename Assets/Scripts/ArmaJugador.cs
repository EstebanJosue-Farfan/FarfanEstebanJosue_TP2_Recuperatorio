using UnityEngine;

public class ArmaJugador : MonoBehaviour
{
    public int danio = 10;
    [Tooltip("Segundos entre disparos")]
    public float cadencia = 0.15f;
    public float alcance = 30f;
    [Tooltip("Marcar Default, Ground y Enemy. NO marcar Player (evita autodaño)")]
    public LayerMask capasImpacto;
    [Tooltip("Opcional: punto desde donde sale el disparo")]
    public Transform puntoDisparo;
    public Color colorTrazo = Color.yellow;

    float proximoDisparo;
    LineRenderer trazo;
    float ocultarTrazoEn;

    void Start()
    {
        trazo = Trazador.Crear(transform, colorTrazo, 0.06f);
    }

    void Update()
    {
        if (trazo != null && trazo.enabled && Time.time >= ocultarTrazoEn) trazo.enabled = false;

        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;
        if (!Input.GetMouseButton(0) || Time.time < proximoDisparo) return;

        proximoDisparo = Time.time + cadencia;
        Disparar();
    }

    void Disparar()
    {
        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position + Vector3.up * 0.3f;
        Vector3 direccion = transform.forward;
        Vector3 fin = origen + direccion * alcance;

        if (Physics.Raycast(origen, direccion, out RaycastHit hit, alcance, capasImpacto, QueryTriggerInteraction.Ignore))
        {
            fin = hit.point;
            Enemigo enemigo = hit.collider.GetComponentInParent<Enemigo>();
            if (enemigo != null) enemigo.RecibirDanio(danio);
        }

        Debug.DrawLine(origen, fin, Color.yellow, 0.1f);
        if (trazo != null)
        {
            trazo.SetPosition(0, origen);
            trazo.SetPosition(1, fin);
            trazo.enabled = true;
            ocultarTrazoEn = Time.time + 0.05f;
        }
    }
}
