using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Configura automáticamente un enemigo: capa Enemy, Collider, NavMeshAgent y script Enemigo.
/// Se ejecuta solo al agregar este componente, o desde el menú de los tres puntitos
/// del componente -> "Configurar Enemigo". También lo usa el SpawnerOleadas.
/// </summary>
public class EnemySetup : MonoBehaviour
{
    void Reset()
    {
        Configurar();
    }

    [ContextMenu("Configurar Enemigo")]
    public void Configurar()
    {
        int capaEnemy = LayerMask.NameToLayer("Enemy");
        if (capaEnemy >= 0) gameObject.layer = capaEnemy;
        else Debug.LogWarning("[EnemySetup] Falta crear la capa 'Enemy'");

        if (GetComponent<Collider>() == null) gameObject.AddComponent<CapsuleCollider>();

        // Un enemigo con NavMeshAgent no debe tener Rigidbody con física
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        NavMeshAgent agente = GetComponent<NavMeshAgent>();
        if (agente == null) agente = gameObject.AddComponent<NavMeshAgent>();
        agente.speed = 3f;
        agente.angularSpeed = 360f;
        agente.acceleration = 12f;
        agente.stoppingDistance = 0.5f;

        // Que los pies queden apoyados sobre el NavMesh (no enterrado a la mitad)
        Collider col = GetComponent<Collider>();
        if (col != null) agente.baseOffset = transform.position.y - col.bounds.min.y;

        Enemigo enemigo = GetComponent<Enemigo>();
        if (enemigo == null) enemigo = gameObject.AddComponent<Enemigo>();
        enemigo.capasObstaculo = LayerMask.GetMask("Default");
        if (enemigo.cuerpo == null) enemigo.cuerpo = GetComponentInChildren<Renderer>();

        Debug.Log($"[EnemySetup] {name} configurado");
    }
}
