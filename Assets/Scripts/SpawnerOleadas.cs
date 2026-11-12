using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Opción E: generador temporizado de enemigos por oleadas.
/// Cada X segundos crea una oleada alrededor del jugador (a distancia), sobre el NavMesh.
/// Cada oleada trae más enemigos. Al sobrevivir la última oleada avisa al GameManager.
/// </summary>
public class SpawnerOleadas : MonoBehaviour
{
    [Header("Enemigo")]
    [Tooltip("Prefab del enemigo (con EnemySetup). Si queda vacío se crea una cápsula roja automáticamente")]
    public GameObject prefabEnemigo;

    [Header("Oleadas")]
    public float tiempoEntreOleadas = 30f;
    public float retrasoPrimeraOleada = 3f;
    public int enemigosPrimeraOleada = 3;
    public int enemigosExtraPorOleada = 2;
    public int oleadasParaGanar = 5;
    public int maxEnemigosVivos = 30;

    [Header("Zona de aparición (alrededor del jugador)")]
    public float distanciaMinima = 18f;
    public float distanciaMaxima = 28f;

    public int OleadaActual { get; private set; }
    public float TiempoParaSiguiente { get; private set; }
    public int EnemigosVivos => vivos.Count;
    public bool UltimaOleadaLanzada => OleadaActual >= oleadasParaGanar;

    public event Action<int> OnOleadaIniciada;
    public event Action OnSupervivenciaCompletada;

    readonly List<Enemigo> vivos = new List<Enemigo>();
    Transform jugador;
    bool completado;

    void OnEnable()  { Enemigo.OnEnemigoMuerto += QuitarEnemigo; }
    void OnDisable() { Enemigo.OnEnemigoMuerto -= QuitarEnemigo; }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
        else Debug.LogWarning("[SpawnerOleadas] No se encontró el jugador (tag Player)");

        TiempoParaSiguiente = retrasoPrimeraOleada;
    }

    void Update()
    {
        vivos.RemoveAll(e => e == null);

        if (completado || jugador == null) return;
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;

        TiempoParaSiguiente -= Time.deltaTime;
        if (TiempoParaSiguiente > 0f) return;

        if (UltimaOleadaLanzada)
        {
            completado = true;
            Debug.Log("[SpawnerOleadas] ¡Se sobrevivieron todas las oleadas!");
            OnSupervivenciaCompletada?.Invoke();
            return;
        }

        LanzarOleada();
        TiempoParaSiguiente = tiempoEntreOleadas;
    }

    void LanzarOleada()
    {
        OleadaActual++;
        int cantidad = enemigosPrimeraOleada + (OleadaActual - 1) * enemigosExtraPorOleada;
        int creados = 0;

        for (int i = 0; i < cantidad; i++)
        {
            if (vivos.Count >= maxEnemigosVivos) break;
            if (CrearEnemigo()) creados++;
        }

        Debug.Log($"[SpawnerOleadas] Oleada {OleadaActual}: {creados} enemigos creados");
        OnOleadaIniciada?.Invoke(OleadaActual);
    }

    bool CrearEnemigo()
    {
        for (int intento = 0; intento < 10; intento++)
        {
            float angulo = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distancia = UnityEngine.Random.Range(distanciaMinima, distanciaMaxima);
            Vector3 punto = jugador.position + new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * distancia;

            // Busca el punto del NavMesh más cercano (para que aparezca sobre el terreno caminable)
            if (!NavMesh.SamplePosition(punto, out NavMeshHit hit, 5f, NavMesh.AllAreas)) continue;

            GameObject go = prefabEnemigo != null
                ? Instantiate(prefabEnemigo, hit.position + Vector3.up * 0.1f, Quaternion.identity)
                : CrearEnemigoBasico(hit.position);

            go.name = $"Enemigo_O{OleadaActual}_{vivos.Count}";
            Enemigo e = go.GetComponent<Enemigo>();
            if (e == null)
            {
                EnemySetup setup = go.GetComponent<EnemySetup>();
                if (setup == null) setup = go.AddComponent<EnemySetup>();
                setup.Configurar();
                e = go.GetComponent<Enemigo>();
            }
            vivos.Add(e);
            return true;
        }

        Debug.LogWarning("[SpawnerOleadas] No se encontró lugar en el NavMesh para crear un enemigo. ¿Hiciste Bake?");
        return false;
    }

    GameObject CrearEnemigoBasico(Vector3 posicion)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.transform.position = posicion + Vector3.up;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = Color.red;
        go.AddComponent<EnemySetup>().Configurar();
        return go;
    }

    void QuitarEnemigo(Enemigo e)
    {
        vivos.Remove(e);
    }

    void OnDrawGizmosSelected()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(p.transform.position, distanciaMinima);
        Gizmos.DrawWireSphere(p.transform.position, distanciaMaxima);
    }
}
