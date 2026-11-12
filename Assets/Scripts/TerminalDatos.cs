using UnityEngine;

public class TerminalDatos : MonoBehaviour, IInteractuable
{
    public int puntos = 10;
    public Color colorUsada = Color.gray;
    [Tooltip("Opcional: puerta que se abre al activar la terminal")]
    public Puerta puertaVinculada;

    bool usada;

    public string Mensaje() => usada ? "Terminal sin datos" : "Descargar datos";

    public void Interactuar()
    {
        if (usada) return;
        usada = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SumarPuntos(puntos);
            GameManager.Instance.MostrarMensaje($"Datos descargados (+{puntos})");
        }
        Debug.Log($"[TerminalDatos] {name} activada: +{puntos}");

        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = colorUsada;
        if (puertaVinculada != null) puertaVinculada.Interactuar();
    }
}
