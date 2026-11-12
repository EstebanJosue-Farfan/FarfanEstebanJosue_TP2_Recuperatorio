using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnerOleadas : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastrá acá al Jugador. Si queda vacío lo busca por el tag Player")]
    public Transform jugador;

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
    [Tooltip("Qué tan lejos (en metros) busca el NavMesh desde cada punto elegido")]
    public float radioBusquedaNavMesh = 8f;

    public int OleadaActual { get; private set; }
    public float TiempoParaSiguiente { get; private set; }
    public int EnemigosVivos => vivos.Count;
    public bool UltimaOleadaLanzada => OleadaActual >= oleadasParaGanar;

    public event Action<int> OnOleadaIniciada;
    public event Action OnSupervivenciaCompletada;

    readonly List<Enemigo> vivos = new List<Enemigo>();
    bool completado;

    void OnEnable()  { Enemigo.OnEnemigoMuerto += QuitarEnemigo; }
    void OnDisable() { Enemigo.OnEnemigoMuerto -= QuitarEnemigo; }

    void Start()
    {
        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }

        if (jugador != null)
            Debug.Log($"[SpawnerOleadas] Jugador: '{jugador.name}' en {jugador.position}");
        else
            Debug.LogWarning("[SpawnerOleadas] No se encontró el jugador. Arrastralo al campo Jugador");

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

        Debug.Log($"[SpawnerOleadas] Oleada {OleadaActual}: {creados} enemigos creados cerca de {jugador.position}");
        OnOleadaIniciada?.Invoke(OleadaActual);
    }

    bool CrearEnemigo()
    {
        const int intentos = 20;
        for (int intento = 0; intento < intentos; intento++)
        {
            // En los últimos intentos prueba cada vez más cerca del jugador
            float factor = 1f - (float)intento / intentos * 0.7f;
            float distancia = UnityEngine.Random.Range(distanciaMinima, distanciaMaxima) * factor;
            float angulo = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 punto = jugador.position + new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * distancia;

            // Usa la altura del terreno en ese punto (por si el jugador está en una plataforma alta)
            Terrain terreno = Terrain.activeTerrain;
            if (terreno != null)
                punto.y = terreno.SampleHeight(punto) + terreno.transform.position.y;

            if (!NavMesh.SamplePosition(punto, out NavMeshHit hit, radioBusquedaNavMesh, NavMesh.AllAreas)) continue;

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
        Transform centro = jugador;
        if (centro == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) centro = p.transform;
        }
        if (centro == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(centro.position, distanciaMinima);
        Gizmos.DrawWireSphere(centro.position, distanciaMaxima);
    }
}