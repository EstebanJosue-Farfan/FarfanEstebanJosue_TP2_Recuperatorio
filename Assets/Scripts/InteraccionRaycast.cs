using UnityEngine;

/// <summary>
/// Interacción con puertas y terminales (tecla E):
/// 1) Raycast frontal desde el jugador, filtrado por la capa Interactable (a distancia).
/// 2) Si el rayo no encuentra nada, busca objetos interactuables pegados al jugador
///    (sirve aunque esté de espaldas).
/// </summary>
public class InteraccionRaycast : MonoBehaviour
{
    [Header("Raycast frontal")]
    public float distancia = 3f;
    [Tooltip("Altura del rayo respecto al centro del jugador")]
    public float alturaRayo = 0f;

    [Header("Cercanía (aunque esté de espaldas)")]
    public bool permitirCercania = true;
    public float radioCercania = 1.5f;

    [Header("General")]
    [Tooltip("Marcar SOLO la capa Interactable")]
    public LayerMask capaInteractuable;
    public KeyCode tecla = KeyCode.E;

    IInteractuable objetivo;

    void Update()
    {
        objetivo = null;
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando) return;

        objetivo = BuscarConRaycast();
        if (objetivo == null && permitirCercania) objetivo = BuscarCerca();

        if (objetivo != null && Input.GetKeyDown(tecla))
            objetivo.Interactuar();
    }

    IInteractuable BuscarConRaycast()
    {
        Ray rayo = new Ray(transform.position + Vector3.up * alturaRayo, transform.forward);
        Debug.DrawRay(rayo.origin, rayo.direction * distancia, Color.cyan);

        if (Physics.Raycast(rayo, out RaycastHit hit, distancia, capaInteractuable, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<IInteractuable>();
        return null;
    }

    IInteractuable BuscarCerca()
    {
        Collider[] cercanos = Physics.OverlapSphere(transform.position, radioCercania, capaInteractuable, QueryTriggerInteraction.Ignore);

        IInteractuable masCercano = null;
        float mejorDistancia = float.MaxValue;

        foreach (Collider c in cercanos)
        {
            IInteractuable i = c.GetComponentInParent<IInteractuable>();
            if (i == null) continue;

            float d = (c.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            if (d < mejorDistancia)
            {
                mejorDistancia = d;
                masCercano = i;
            }
        }
        return masCercano;
    }

    void OnGUI()
    {
        if (objetivo == null) return;
        GUIStyle estilo = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, Screen.height * 0.75f, Screen.width, 30), $"[{tecla}] {objetivo.Mensaje()}", estilo);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioCercania);
    }
}