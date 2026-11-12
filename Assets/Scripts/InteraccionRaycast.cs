using UnityEngine;

public class InteraccionRaycast : MonoBehaviour
{
    public float distancia = 3f;
    [Tooltip("Altura del rayo respecto al centro del jugador")]
    public float alturaRayo = 0f;
    [Tooltip("Marcar SOLO la capa Interactable")]
    public LayerMask capaInteractuable;
    public KeyCode tecla = KeyCode.E;

    IInteractuable objetivo;

    void Update()
    {
        objetivo = null;
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;

        Ray rayo = new Ray(transform.position + Vector3.up * alturaRayo, transform.forward);
        Debug.DrawRay(rayo.origin, rayo.direction * distancia, Color.cyan);

        if (Physics.Raycast(rayo, out RaycastHit hit, distancia, capaInteractuable, QueryTriggerInteraction.Ignore))
        {
            objetivo = hit.collider.GetComponentInParent<IInteractuable>();
            if (objetivo != null && Input.GetKeyDown(tecla))
                objetivo.Interactuar();
        }
    }

    void OnGUI()
    {
        if (objetivo == null) return;
        GUIStyle estilo = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height * 0.75f, Screen.width, 30), $"[{tecla}] {objetivo.Mensaje()}", estilo);
    }
}
