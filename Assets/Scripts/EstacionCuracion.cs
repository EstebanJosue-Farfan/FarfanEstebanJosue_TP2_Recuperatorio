using UnityEngine;

/// <summary>
/// Estación de Curación (Requisito Extra).
/// - Cura de forma progresiva (+cantidadCura cada intervaloCura segundos).
/// - Tiene una carga máxima; al agotarse entra en recarga (cooldown).
/// - Durante la recarga apaga luz y partículas, cambia de color y avisa por consola y HUD.
/// Necesita un Collider con "Is Trigger" activado.
/// </summary>
public class EstacionCuracion : MonoBehaviour
{
    [Header("Curación")]
    public int cantidadCura = 10;
    public float intervaloCura = 1.5f;
    [Tooltip("Vida total que puede dar antes de entrar en recarga")]
    public int cargaMaxima = 50;
    public float tiempoRecarga = 10f;

    [Header("Feedback visual")]
    public Light luz;
    public ParticleSystem particulas;
    public Renderer plataforma;
    public Color colorActiva = Color.green;
    public Color colorRecarga = Color.red;

    int cargaRestante;
    bool enRecarga;
    float finRecarga;
    float proximaCura;
    PlayerHealth jugadorDentro;

    void Start()
    {
        Activar(false);
    }

    void Update()
    {
        if (enRecarga)
        {
            if (Time.time >= finRecarga) Activar(true);
            return;
        }

        if (jugadorDentro == null || !jugadorDentro.EstaHerido || jugadorDentro.EstaMuerto) return;
        if (Time.time < proximaCura) return;

        int curado = jugadorDentro.Curar(Mathf.Min(cantidadCura, cargaRestante));
        cargaRestante -= curado;
        proximaCura = Time.time + intervaloCura;

        Debug.Log($"[EstacionCuracion] +{curado} HP | Carga restante: {cargaRestante}/{cargaMaxima}");
        if (GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje($"Curando +{curado} HP (carga {cargaRestante}/{cargaMaxima})", intervaloCura);

        if (cargaRestante <= 0) EmpezarRecarga();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro = other.GetComponentInParent<PlayerHealth>();
        proximaCura = Time.time + intervaloCura; // progresiva: no cura al instante

        if (enRecarga)
        {
            float faltan = finRecarga - Time.time;
            Debug.Log($"[EstacionCuracion] Estación en Recarga ({faltan:0}s)");
            if (GameManager.Instance != null)
                GameManager.Instance.MostrarMensaje($"Estación en Recarga ({faltan:0}s)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) jugadorDentro = null;
    }

    void EmpezarRecarga()
    {
        enRecarga = true;
        finRecarga = Time.time + tiempoRecarga;

        if (luz != null) luz.enabled = false;
        if (particulas != null) particulas.Stop();
        if (plataforma != null) plataforma.material.color = colorRecarga;

        Debug.Log($"[EstacionCuracion] Estación en Recarga durante {tiempoRecarga}s");
        if (GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje("Estación en Recarga", 3f);
    }

    void Activar(bool avisar)
    {
        enRecarga = false;
        cargaRestante = cargaMaxima;

        if (luz != null) luz.enabled = true;
        if (particulas != null) particulas.Play();
        if (plataforma != null) plataforma.material.color = colorActiva;

        if (avisar)
        {
            Debug.Log("[EstacionCuracion] Estación disponible nuevamente");
            if (GameManager.Instance != null)
                GameManager.Instance.MostrarMensaje("Estación de curación disponible");
        }
    }
}
