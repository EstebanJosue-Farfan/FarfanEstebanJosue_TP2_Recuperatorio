using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;

public enum EstadoJuego { Inicio, Jugando, Pausa, Victoria, Derrota }

/// <summary>
/// Singleton que controla el flujo del juego: Inicio, Jugando, Pausa, Victoria y Derrota.
/// No contiene lógica del jugador ni de los enemigos: escucha eventos de PlayerHealth
/// y de SpawnerOleadas (desacoplado).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias (si quedan vacías se buscan solas)")]
    public PlayerHealth jugador;
    public SpawnerOleadas spawnerOleadas;

    [Header("UI (TextMeshPro)")]
    [Tooltip("Vida y puntos")]
    [FormerlySerializedAs("textoTiempo")] public TextMeshProUGUI textoHUD;
    [Tooltip("Mensajes grandes (inicio, pausa, victoria, avisos)")]
    public TextMeshProUGUI textoMensaje;
    [Tooltip("Información de la oleada")]
    [FormerlySerializedAs("textoBoost")] public TextMeshProUGUI textoOleada;

    public EstadoJuego Estado { get; private set; }
    public int Puntos { get; private set; }
    public bool EstaJugando => Estado == EstadoJuego.Jugando;

    public event Action<EstadoJuego> OnEstadoCambio;
    public event Action<int> OnPuntosCambio;

    string mensajeTemporal = "";
    float mensajeHasta;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.GetComponent<PlayerHealth>();
        }
        if (spawnerOleadas == null) spawnerOleadas = FindFirstObjectByType<SpawnerOleadas>();

        if (jugador != null)
        {
            jugador.OnVidaCambio += VidaCambio;
            jugador.OnMuerte += Perder;
        }
        else Debug.LogWarning("[GameManager] No se encontró el PlayerHealth del jugador");

        if (spawnerOleadas != null)
        {
            spawnerOleadas.OnOleadaIniciada += OleadaIniciada;
            spawnerOleadas.OnSupervivenciaCompletada += Victoria;
        }
        else Debug.LogWarning("[GameManager] No se encontró un SpawnerOleadas en la escena");

        CambiarEstado(EstadoJuego.Inicio);
    }

    void OnDestroy()
    {
        if (jugador != null)
        {
            jugador.OnVidaCambio -= VidaCambio;
            jugador.OnMuerte -= Perder;
        }
        if (spawnerOleadas != null)
        {
            spawnerOleadas.OnOleadaIniciada -= OleadaIniciada;
            spawnerOleadas.OnSupervivenciaCompletada -= Victoria;
        }
    }

    void Update()
    {
        switch (Estado)
        {
            case EstadoJuego.Inicio:
                if (Input.GetKeyDown(KeyCode.Return)) CambiarEstado(EstadoJuego.Jugando);
                break;

            case EstadoJuego.Jugando:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                    CambiarEstado(EstadoJuego.Pausa);
                break;

            case EstadoJuego.Pausa:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                    CambiarEstado(EstadoJuego.Jugando);
                break;

            case EstadoJuego.Victoria:
            case EstadoJuego.Derrota:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                break;
        }

        ActualizarHUD();
    }

    // ---------------- Estados ----------------
    public void CambiarEstado(EstadoJuego nuevo)
    {
        Estado = nuevo;
        Time.timeScale = (nuevo == EstadoJuego.Jugando) ? 1f : 0f; // pausa = tiempo congelado
        Debug.Log($"[GameManager] Estado -> {nuevo}");
        OnEstadoCambio?.Invoke(nuevo);
    }

    public void Perder()
    {
        if (Estado == EstadoJuego.Jugando || Estado == EstadoJuego.Pausa)
            CambiarEstado(EstadoJuego.Derrota);
    }

    public void Victoria()
    {
        if (Estado == EstadoJuego.Jugando) CambiarEstado(EstadoJuego.Victoria);
    }

    // ---------------- Puntos ----------------
    public void SumarPuntos(int cantidad)
    {
        if (!EstaJugando) return;
        Puntos += cantidad;
        Debug.Log($"[GameManager] Puntos: {Puntos}");
        OnPuntosCambio?.Invoke(Puntos);
    }

    // ---------------- Eventos recibidos ----------------
    void VidaCambio(int actual, int maxima)
    {
        Debug.Log($"[GameManager] Vida del jugador: {actual}/{maxima}");
    }

    void OleadaIniciada(int numero)
    {
        Debug.Log($"[GameManager] Comenzó la oleada {numero}");
        MostrarMensaje($"¡OLEADA {numero}!", 2.5f);
    }

    // ---------------- HUD ----------------
    /// <summary>Mensaje temporal en pantalla (funciona aunque el juego esté en pausa).</summary>
    public void MostrarMensaje(string mensaje, float duracion = 2f)
    {
        mensajeTemporal = mensaje;
        mensajeHasta = Time.unscaledTime + duracion;
    }

    void ActualizarHUD()
    {
        if (textoHUD != null && jugador != null)
            textoHUD.text = $"Vida: {jugador.VidaActual}/{jugador.vidaMaxima}   Puntos: {Puntos}";

        if (textoOleada != null && spawnerOleadas != null)
        {
            float t = Mathf.Max(0f, spawnerOleadas.TiempoParaSiguiente);
            string reloj = $"{Mathf.FloorToInt(t / 60f):0}:{Mathf.FloorToInt(t % 60f):00}";
            string cuando = spawnerOleadas.UltimaOleadaLanzada ? $"Resistí: {reloj}" : $"Próxima: {reloj}";
            textoOleada.text = $"Oleada {spawnerOleadas.OleadaActual}/{spawnerOleadas.oleadasParaGanar}   {cuando}   Enemigos: {spawnerOleadas.EnemigosVivos}";
        }

        if (textoMensaje == null) return;

        string mensajeEstado = Estado switch
        {
            EstadoJuego.Inicio   => "SUPERVIVENCIA\nPresioná ENTER para comenzar",
            EstadoJuego.Pausa    => "PAUSA\n(ESC o P para continuar)",
            EstadoJuego.Victoria => "¡SOBREVIVISTE!\nPresioná R para reiniciar",
            EstadoJuego.Derrota  => "¡PERDISTE!\nPresioná R para reiniciar",
            _ => ""
        };

        bool hayTemporal = Estado == EstadoJuego.Jugando && Time.unscaledTime < mensajeHasta;
        textoMensaje.text = hayTemporal ? mensajeTemporal : mensajeEstado;
    }
}
