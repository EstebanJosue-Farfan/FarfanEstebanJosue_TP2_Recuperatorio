using UnityEngine;


public class ArmaAturdidora : MonoBehaviour
{
    public float alcance = 20f;
    public float duracionAturdimiento = 3f;
    public float tiempoEntreDisparos = 3f;
    [Tooltip("Marcar Default, Ground y Enemy. NO marcar Player (evita autodaño)")]
    public LayerMask capasImpacto;
    public Color colorTrazo = Color.cyan;

    float proximoDisparo;
    LineRenderer trazo;
    float ocultarTrazoEn;

    void Start()
    {
        trazo = Trazador.Crear(transform, colorTrazo, 0.15f);
    }

    void Update()
    {
        if (trazo != null && trazo.enabled && Time.time >= ocultarTrazoEn) trazo.enabled = false;

        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;
        if (!Input.GetMouseButtonDown(1)) return;

        if (Time.time < proximoDisparo)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.MostrarMensaje($"Aturdidor recargando ({proximoDisparo - Time.time:0.0}s)", 1f);
            return;
        }
        proximoDisparo = Time.time + tiempoEntreDisparos;

        Vector3 origen = transform.position + Vector3.up * 0.3f;
        Vector3 fin = origen + transform.forward * alcance;

        if (Physics.Raycast(origen, transform.forward, out RaycastHit hit, alcance, capasImpacto, QueryTriggerInteraction.Ignore))
        {
            fin = hit.point;
            Enemigo enemigo = hit.collider.GetComponentInParent<Enemigo>();
            if (enemigo != null) enemigo.Aturdir(duracionAturdimiento);
        }

        Debug.DrawLine(origen, fin, Color.cyan, 0.5f);
        if (trazo != null)
        {
            trazo.SetPosition(0, origen);
            trazo.SetPosition(1, fin);
            trazo.enabled = true;
            ocultarTrazoEn = Time.time + 0.12f;
        }
    }
}
