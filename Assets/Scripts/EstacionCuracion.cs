using UnityEngine;

/// <summary>
/// Estación de Curación (Requisito Extra).
/// - Detecta la presencia del jugador dentro de su Collider (lo revisa en cada frame).
/// - Cura de forma progresiva (+cantidadCura cada intervaloCura segundos).
/// - Tiene una carga máxima; al agotarse entra en recarga (cooldown).
/// - Durante la recarga apaga luz y partículas, cambia de color y avisa por consola y HUD.
/// </summary>
public class EstacionCuracion : MonoBehaviour
{
    [Header("Curación")]
    public int cantidadCura = 10;
    public float intervaloCura = 1.5f;
    [Tooltip("Vida total que puede dar antes de entrar en recarga")]
    public int cargaMaxima = 50;
    public float tiempoRecarga = 10f;

    [Header("Detección")]
    [Tooltip("Collider que marca la zona de curación. Si queda vacío usa el de este objeto")]
    public Collider zona;
    [Tooltip("Si no hay Collider, usa este radio alrededor de la estación")]
    public float radioSinCollider = 2f;

    [Header("Feedback visual")]
    public Light luz;
    public ParticleSystem particulas;
    public Renderer plataforma;
    public Color colorActiva = Color.green;
    public Color colorRecarga = Color.red;

    PlayerHealth jugador;
    int cargaRestante;
    bool enRecarga;
    float finRecarga;
    float proximaCura;
    bool estabaDentro;
    bool avisoVidaLlena;

    void Start()
    {
        if (zona == null) zona = GetComponent<Collider>();
        if (zona == null) zona = GetComponentInChildren<Collider>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.GetComponent<PlayerHealth>();
        if (jugador == null) Debug.LogWarning("[EstacionCuracion] No se encontró el PlayerHealth del jugador");

        Activar(false);
    }

    void Update()
    {
        if (jugador == null) return;
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;

        bool dentro = JugadorDentro();

        // Entrar / salir
        if (dentro && !estabaDentro) AlEntrar();
        if (!dentro && estabaDentro) Debug.Log("[EstacionCuracion] Jugador salió de la estación");
        estabaDentro = dentro;

        if (enRecarga)
        {
            if (Time.time >= finRecarga) Activar(true);
            return;
        }

        if (!dentro || jugador.EstaMuerto) return;

        if (!jugador.EstaHerido)
        {
            if (!avisoVidaLlena)
            {
                avisoVidaLlena = true;
                if (GameManager.Instance != null) GameManager.Instance.MostrarMensaje("Vida llena", 1.5f);
            }
            return;
        }
        avisoVidaLlena = false;

        if (Time.time < proximaCura) return;

        int curado = jugador.Curar(Mathf.Min(cantidadCura, cargaRestante));
        cargaRestante -= curado;
        proximaCura = Time.time + intervaloCura;

        Debug.Log($"[EstacionCuracion] +{curado} HP | Carga restante: {cargaRestante}/{cargaMaxima}");
        if (GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje($"Curando +{curado} HP (carga {cargaRestante}/{cargaMaxima})", intervaloCura);

        if (cargaRestante <= 0) EmpezarRecarga();
    }

    bool JugadorDentro()
    {
        Vector3 pos = jugador.transform.position;

        if (zona != null)
        {
            // Punto de la zona más cercano al jugador: si coincide (o está muy cerca), está dentro
            Vector3 cercano = zona.ClosestPoint(pos);
            return (cercano - pos).sqrMagnitude < 0.8f * 0.8f;
        }

        Vector3 dif = pos - transform.position;
        dif.y = 0f;
        return dif.magnitude <= radioSinCollider;
    }

    void AlEntrar()
    {
        avisoVidaLlena = false;
        proximaCura = Time.time + intervaloCura; // progresiva: no cura al instante
        Debug.Log("[EstacionCuracion] Jugador entró a la estación");

        if (GameManager.Instance == null) return;
        if (enRecarga)
        {
            float faltan = finRecarga - Time.time;
            GameManager.Instance.MostrarMensaje($"Estación en Recarga ({faltan:0}s)");
        }
        else GameManager.Instance.MostrarMensaje("Estación de curación", 1.5f);
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

    void OnDrawGizmosSelected()
    {
        if (zona == null) zona = GetComponent<Collider>();
        if (zona != null) return; // el collider ya se ve en verde
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioSinCollider);
    }
}