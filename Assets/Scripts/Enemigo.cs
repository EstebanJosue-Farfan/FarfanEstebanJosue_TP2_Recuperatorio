using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo con NavMeshAgent:
/// - Patrulla puntos aleatorios alrededor de donde apareció.
/// - Si ve al jugador (o recibe un disparo) lo persigue y ataca al estar en rango.
/// - Tiene vida, da puntos al morir y puede ser aturdido (Opción A).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Enemigo : MonoBehaviour
{
    enum Estado { Patrulla, Persecucion, Ataque, Aturdido }

    [Header("Vida")]
    public int vidaMaxima = 30;
    public int puntosAlMorir = 10;

    [Header("Patrulla")]
    public float radioPatrulla = 8f;
    public float velocidadPatrulla = 2.5f;
    public float esperaEnPunto = 1.5f;

    [Header("Detección")]
    public float radioDeteccion = 12f;
    public float radioPerdida = 22f;
    public float velocidadPersecucion = 5f;
    [Tooltip("Capas que bloquean la visión (Default). NO marcar Player ni Enemy")]
    public LayerMask capasObstaculo;

    [Header("Ataque")]
    public float rangoAtaque = 1.8f;
    public int danioAtaque = 10;
    public float tiempoEntreAtaques = 1f;
    public int danioContacto = 5;

    [Header("Visual")]
    public Renderer cuerpo;
    public Color colorAturdido = Color.cyan;
    public Color colorGolpe = Color.white;

    /// <summary>Aviso global cuando un enemigo muere (lo usa el SpawnerOleadas).</summary>
    public static event Action<Enemigo> OnEnemigoMuerto;

    public bool EstaAturdido => estado == Estado.Aturdido;
    public int VidaActual { get; private set; }

    NavMeshAgent agente;
    Transform jugador;
    PlayerHealth vidaJugador;
    Estado estado = Estado.Patrulla;
    Vector3 origen;
    bool esperando;
    float proximoPunto;
    float ultimoAtaque = -999f;
    float finAturdimiento;
    float alertadoHasta;
    float finFlash;
    Color colorOriginal;
    bool muerto;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        VidaActual = vidaMaxima;
        if (cuerpo == null) cuerpo = GetComponentInChildren<Renderer>();
        if (cuerpo != null) colorOriginal = cuerpo.material.color;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            jugador = p.transform;
            vidaJugador = p.GetComponent<PlayerHealth>();
        }
        origen = transform.position;
        NuevoPuntoPatrulla();
    }

    void Update()
    {
        if (muerto) return;

        if (finFlash > 0f && Time.time >= finFlash)
        {
            finFlash = 0f;
            if (cuerpo != null && !EstaAturdido) cuerpo.material.color = colorOriginal;
        }

        if (jugador == null || vidaJugador == null || vidaJugador.EstaMuerto) return;
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;
        if (!agente.isOnNavMesh) return;

        if (estado == Estado.Aturdido)
        {
            if (Time.time >= finAturdimiento) TerminarAturdimiento();
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        switch (estado)
        {
            case Estado.Patrulla:
                agente.speed = velocidadPatrulla;
                Patrullar();
                if (PuedeVerJugador(distancia)) CambiarEstado(Estado.Persecucion);
                break;

            case Estado.Persecucion:
                agente.speed = velocidadPersecucion;
                agente.SetDestination(jugador.position);
                if (distancia <= rangoAtaque) CambiarEstado(Estado.Ataque);
                else if (distancia > radioPerdida && Time.time > alertadoHasta)
                {
                    CambiarEstado(Estado.Patrulla);
                    origen = transform.position;
                    NuevoPuntoPatrulla();
                }
                break;

            case Estado.Ataque:
                MirarJugador();
                if (Time.time >= ultimoAtaque + tiempoEntreAtaques)
                {
                    ultimoAtaque = Time.time;
                    if (vidaJugador.RecibirDanio(danioAtaque))
                        Debug.Log($"[Enemigo] {name} atacó al jugador (-{danioAtaque})");
                }
                if (distancia > rangoAtaque + 0.5f) CambiarEstado(Estado.Persecucion);
                break;
        }
    }

    // ---------------- Patrulla ----------------
    void Patrullar()
    {
        if (agente.pathPending) return;
        if (agente.remainingDistance > agente.stoppingDistance + 0.3f) return;

        if (!esperando)
        {
            esperando = true;
            proximoPunto = Time.time + esperaEnPunto;
        }
        else if (Time.time >= proximoPunto)
        {
            esperando = false;
            NuevoPuntoPatrulla();
        }
    }

    void NuevoPuntoPatrulla()
    {
        if (!agente.isOnNavMesh) return;
        Vector3 azar = origen + UnityEngine.Random.insideUnitSphere * radioPatrulla;
        if (NavMesh.SamplePosition(azar, out NavMeshHit hit, radioPatrulla, NavMesh.AllAreas))
        {
            agente.isStopped = false;
            agente.SetDestination(hit.position);
        }
    }

    // ---------------- Detección ----------------
    bool PuedeVerJugador(float distancia)
    {
        if (distancia > radioDeteccion) return false;
        Vector3 ojos = transform.position + Vector3.up * 0.5f;
        return !Physics.Linecast(ojos, jugador.position, capasObstaculo, QueryTriggerInteraction.Ignore);
    }

    void CambiarEstado(Estado nuevo)
    {
        if (estado == nuevo) return;
        estado = nuevo;
        if (agente.isOnNavMesh)
            agente.isStopped = (nuevo == Estado.Ataque || nuevo == Estado.Aturdido);
        Debug.Log($"[Enemigo] {name} -> {nuevo}");
    }

    void MirarJugador()
    {
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    // ---------------- Daño ----------------
    public void RecibirDanio(int cantidad)
    {
        if (muerto) return;

        VidaActual -= cantidad;
        alertadoHasta = Time.time + 5f;

        if (cuerpo != null && !EstaAturdido)
        {
            cuerpo.material.color = colorGolpe;
            finFlash = Time.time + 0.08f;
        }

        // Si le disparan mientras patrulla, va a buscar al jugador
        if (estado == Estado.Patrulla) CambiarEstado(Estado.Persecucion);

        if (VidaActual <= 0) Morir();
    }

    void Morir()
    {
        muerto = true;
        Debug.Log($"[Enemigo] {name} eliminado (+{puntosAlMorir} puntos)");
        if (GameManager.Instance != null) GameManager.Instance.SumarPuntos(puntosAlMorir);
        OnEnemigoMuerto?.Invoke(this);
        Destroy(gameObject);
    }

    // Daño por contacto (la inmunidad de PlayerHealth evita perder toda la vida en un frame)
    void OnCollisionStay(Collision c)
    {
        if (muerto || EstaAturdido) return;
        PlayerHealth ph = c.gameObject.GetComponentInParent<PlayerHealth>();
        if (ph != null) ph.RecibirDanio(danioContacto);
    }

    // ---------------- Opción A: aturdimiento ----------------
    public void Aturdir(float duracion)
    {
        if (muerto) return;
        finAturdimiento = Time.time + duracion;
        CambiarEstado(Estado.Aturdido);
        if (agente.isOnNavMesh) agente.velocity = Vector3.zero;
        if (cuerpo != null) cuerpo.material.color = colorAturdido;
        Debug.Log($"[Enemigo] {name} aturdido por {duracion}s");
    }

    void TerminarAturdimiento()
    {
        if (cuerpo != null) cuerpo.material.color = colorOriginal;
        CambiarEstado(Estado.Persecucion);
        if (agente.isOnNavMesh) agente.isStopped = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(Application.isPlaying ? origen : transform.position, radioPatrulla);
    }
}
