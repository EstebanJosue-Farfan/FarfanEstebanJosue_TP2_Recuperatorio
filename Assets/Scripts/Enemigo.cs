using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo con NavMeshAgent:
/// - Patrulla puntos aleatorios alrededor de donde apareció.
/// - Tiene un campo de visión (cono visible en el piso). Si el jugador entra, lo persigue.
/// - Ataca al jugador cuando está en rango. Tiene vida, da puntos y puede ser aturdido (Opción A).
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

    [Header("Campo de visión")]
    [Tooltip("Distancia del cono de visión")]
    public float radioDeteccion = 15f;
    [Tooltip("Apertura del cono en grados")]
    [Range(10f, 360f)] public float anguloVision = 90f;
    [Tooltip("Si el jugador está más cerca que esto, lo detecta aunque esté detrás")]
    public float radioCercania = 3f;
    [Tooltip("Si el jugador se aleja más que esto, deja de perseguirlo")]
    public float radioPerdida = 25f;
    public float velocidadPersecucion = 5f;
    [Tooltip("Si está activo, las paredes (capas de abajo) tapan la visión")]
    public bool usarObstaculos = false;
    public LayerMask capasObstaculo;

    [Header("Visual del campo de visión")]
    public bool mostrarCampoVision = true;
    public Color colorPatrulla = new Color(1f, 0.9f, 0f, 0.25f);
    public Color colorAlerta = new Color(1f, 0f, 0f, 0.35f);
    public Color colorConoAturdido = new Color(0f, 1f, 1f, 0.25f);

    [Header("Ataque")]
    public float rangoAtaque = 1.8f;
    public int danioAtaque = 10;
    public float tiempoEntreAtaques = 1f;
    public int danioContacto = 5;

    [Header("Visual del cuerpo")]
    public Renderer cuerpo;
    public Color colorAturdido = Color.cyan;
    public Color colorGolpe = Color.white;

    /// <summary>Aviso global cuando un enemigo muere (lo usa el SpawnerOleadas).</summary>
    public static event Action<Enemigo> OnEnemigoMuerto;

    public bool EstaAturdido => estado == Estado.Aturdido;
    public int VidaActual { get; private set; }

    NavMeshAgent agente;
    NavMeshPath camino;
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
    float tiempoQuieto;
    float proximoRecalculo;
    Color colorOriginal;
    bool muerto;
    Material materialCono;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        camino = new NavMeshPath();
        VidaActual = vidaMaxima;
        if (cuerpo == null) cuerpo = GetComponentInChildren<Renderer>();
        if (cuerpo != null) colorOriginal = cuerpo.material.color;

        // Un Rigidbody con física no deja que el NavMeshAgent lo mueva
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Más cálculo de caminos por frame (el terreno es grande)
        if (NavMesh.pathfindingIterationsPerFrame < 1000)
            NavMesh.pathfindingIterationsPerFrame = 1000;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            jugador = p.transform;
            vidaJugador = p.GetComponent<PlayerHealth>();
        }
        else Debug.LogWarning("[Enemigo] No se encontró el jugador (tag Player)");

        origen = transform.position;
        if (mostrarCampoVision) CrearConoVision();
        ActualizarColorCono();
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

        // Si se salió del NavMesh, lo vuelve a poner en el punto más cercano
        if (!agente.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit cerca, 5f, NavMesh.AllAreas))
                agente.Warp(cerca.position);
            return;
        }

        if (estado == Estado.Aturdido)
        {
            if (Time.time >= finAturdimiento) TerminarAturdimiento();
            return;
        }

        // Distancia solo en el plano (la altura no importa)
        Vector3 diferencia = jugador.position - transform.position;
        diferencia.y = 0f;
        float distancia = diferencia.magnitude;

        switch (estado)
        {
            case Estado.Patrulla:
                agente.speed = velocidadPatrulla;
                if (PuedeVerJugador(diferencia, distancia))
                {
                    CambiarEstado(Estado.Persecucion);
                    break;
                }
                Patrullar();
                break;

            case Estado.Persecucion:
                agente.speed = velocidadPersecucion;
                PerseguirJugador();
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

    void LateUpdate()
    {
        // Siempre parado (nunca tumbado)
        Vector3 e = transform.eulerAngles;
        if (Mathf.Abs(e.x) > 0.01f || Mathf.Abs(e.z) > 0.01f)
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }

    // ---------------- Movimiento ----------------
    /// <summary>Calcula el camino en el momento y lo asigna al agente.</summary>
    bool IrA(Vector3 destino)
    {
        if (!agente.isOnNavMesh) return false;

        if (NavMesh.SamplePosition(destino, out NavMeshHit hit, 6f, NavMesh.AllAreas))
            destino = hit.position;

        if (agente.CalculatePath(destino, camino) && camino.status != NavMeshPathStatus.PathInvalid)
        {
            agente.isStopped = false;
            agente.SetPath(camino);
            return true;
        }
        return false;
    }

    void PerseguirJugador()
    {
        if (Time.time < proximoRecalculo) return;
        proximoRecalculo = Time.time + 0.3f;
        IrA(jugador.position);
    }

    // ---------------- Patrulla ----------------
    void Patrullar()
    {
        if (agente.pathPending) return;

        bool llego = !agente.hasPath || agente.remainingDistance <= agente.stoppingDistance + 0.3f;

        // Seguro anti-trabado: si debería moverse pero está quieto, elige otro punto
        if (!llego && agente.velocity.sqrMagnitude < 0.01f)
        {
            tiempoQuieto += Time.deltaTime;
            if (tiempoQuieto > 2f)
            {
                tiempoQuieto = 0f;
                NuevoPuntoPatrulla();
            }
            return;
        }
        tiempoQuieto = 0f;

        if (!llego) return;

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
        for (int i = 0; i < 5; i++)
        {
            Vector2 circulo = UnityEngine.Random.insideUnitCircle * radioPatrulla;
            Vector3 azar = origen + new Vector3(circulo.x, 0f, circulo.y);
            if (IrA(azar)) return;
        }
    }

    // ---------------- Campo de visión ----------------
    bool PuedeVerJugador(Vector3 diferencia, float distancia)
    {
        // Muy cerca: lo detecta aunque esté detrás
        if (distancia <= radioCercania) return true;

        // Fuera del alcance del cono
        if (distancia > radioDeteccion) return false;

        // Fuera del ángulo del cono
        Vector3 adelante = transform.forward;
        adelante.y = 0f;
        if (Vector3.Angle(adelante, diferencia) > anguloVision * 0.5f) return false;

        // Paredes en el medio (opcional)
        if (usarObstaculos)
        {
            Vector3 ojos = transform.position + Vector3.up * 0.5f;
            if (Physics.Linecast(ojos, jugador.position, capasObstaculo, QueryTriggerInteraction.Ignore))
                return false;
        }
        return true;
    }

    void CrearConoVision()
    {
        GameObject cono = new GameObject("CampoVision");
        cono.transform.SetParent(transform, false);

        // Apoyado en el piso (los pies del enemigo) y sin heredar la escala
        Vector3 escala = transform.lossyScale;
        float pies = agente.baseOffset / Mathf.Max(0.01f, escala.y);
        cono.transform.localPosition = new Vector3(0f, -pies + 0.05f, 0f);
        cono.transform.localRotation = Quaternion.identity;
        cono.transform.localScale = new Vector3(1f / escala.x, 1f / escala.y, 1f / escala.z);

        MeshFilter mf = cono.AddComponent<MeshFilter>();
        mf.mesh = CrearMallaCono(radioDeteccion, anguloVision, 30);

        MeshRenderer mr = cono.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            materialCono = new Material(shader);
            mr.material = materialCono;
        }
    }

    Mesh CrearMallaCono(float radio, float angulo, int segmentos)
    {
        Vector3[] vertices = new Vector3[segmentos + 2];
        Color[] colores = new Color[vertices.Length];
        int[] triangulos = new int[segmentos * 3];

        vertices[0] = Vector3.zero;
        float inicio = -angulo * 0.5f;
        for (int i = 0; i <= segmentos; i++)
        {
            float a = (inicio + angulo * i / segmentos) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radio;
        }
        for (int i = 0; i < segmentos; i++)
        {
            triangulos[i * 3] = 0;
            triangulos[i * 3 + 1] = i + 1;
            triangulos[i * 3 + 2] = i + 2;
        }
        for (int i = 0; i < colores.Length; i++) colores[i] = Color.white;

        Mesh mesh = new Mesh { name = "ConoVision" };
        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.colors = colores;
        mesh.RecalculateNormals();
        return mesh;
    }

    void ActualizarColorCono()
    {
        if (materialCono == null) return;
        Color c = estado switch
        {
            Estado.Patrulla => colorPatrulla,
            Estado.Aturdido => colorConoAturdido,
            _ => colorAlerta
        };
        materialCono.color = c;
    }

    void CambiarEstado(Estado nuevo)
    {
        if (estado == nuevo) return;
        estado = nuevo;
        tiempoQuieto = 0f;
        proximoRecalculo = 0f;
        if (agente.isOnNavMesh)
            agente.isStopped = (nuevo == Estado.Ataque || nuevo == Estado.Aturdido);
        ActualizarColorCono();
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

    // Dibuja el cono en la vista Scene al seleccionar el enemigo
    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        Vector3 izq = Quaternion.Euler(0f, -anguloVision * 0.5f, 0f) * transform.forward;
        Vector3 der = Quaternion.Euler(0f, anguloVision * 0.5f, 0f) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + izq * radioDeteccion);
        Gizmos.DrawLine(pos, pos + der * radioDeteccion);
        Gizmos.DrawWireSphere(pos, radioDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, rangoAtaque);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(pos, radioCercania);
    }
}