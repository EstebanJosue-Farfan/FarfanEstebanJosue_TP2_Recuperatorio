using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float tiempoSupervivencia = 120f;
    public GameObject plataformaMeta;
    public SpawnerObstaculos spawner;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoMensaje;
    public TextMeshProUGUI textoBoost;

    float restante;
    bool terminado, metaAbierta;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        restante = tiempoSupervivencia;
        plataformaMeta.SetActive(false);
        textoMensaje.text = "";
        textoBoost.text = "";
    }

    void Update()
    {
        if (terminado)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (!metaAbierta)
        {
            restante -= Time.deltaTime;
            if (restante <= 0f) { restante = 0f; AbrirMeta(); }
            int m = Mathf.FloorToInt(restante / 60f);
            int s = Mathf.FloorToInt(restante % 60f);
            textoTiempo.text = $"Sobreviví: {m:00}:{s:00}";
        }
    }

    void AbrirMeta()
    {
        metaAbierta = true;
        plataformaMeta.SetActive(true);
        textoTiempo.text = "META ABIERTA";
        textoMensaje.text = "¡Sobreviviste!\nLlevá la caja a la meta";
        Invoke(nameof(LimpiarMensaje), 4f);
    }

    void LimpiarMensaje()
    {
        if (!terminado) textoMensaje.text = "";
    }

    public void MostrarBoost(bool activo)
    {
        textoBoost.text = activo ? "¡BOOST DE VELOCIDAD!" : "";
    }

    public void Perder()
    {
        if (terminado) return;
        terminado = true;
        spawner.Detener();
        textoMensaje.text = "¡PERDISTE!\nPresioná R para reiniciar";
        Time.timeScale = 0f;
    }

    public void Victoria()
    {
    if (terminado) return;
    terminado = true;
    spawner.Detener();
    textoMensaje.text = "¡VICTORIA!\nPresioná R para reiniciar";
    Time.timeScale = 0f;
    }
}