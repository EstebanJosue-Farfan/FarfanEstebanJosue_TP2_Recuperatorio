using UnityEngine;
using UnityEngine.AI;


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

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        NavMeshAgent agente = GetComponent<NavMeshAgent>();
        if (agente == null) agente = gameObject.AddComponent<NavMeshAgent>();
        agente.speed = 3f;
        agente.angularSpeed = 360f;
        agente.acceleration = 12f;
        agente.stoppingDistance = 0.5f;
        agente.baseOffset = CalcularAlturaPies();

        Enemigo enemigo = GetComponent<Enemigo>();
        if (enemigo == null) enemigo = gameObject.AddComponent<Enemigo>();
        enemigo.capasObstaculo = LayerMask.GetMask("Default");
        if (enemigo.cuerpo == null) enemigo.cuerpo = GetComponentInChildren<Renderer>();

        Debug.Log($"[EnemySetup] {name} configurado");
    }

    float CalcularAlturaPies()
    {
        float escalaY = transform.lossyScale.y;

        CapsuleCollider capsula = GetComponent<CapsuleCollider>();
        if (capsula != null)
            return (capsula.height * 0.5f - capsula.center.y) * escalaY;

        BoxCollider caja = GetComponent<BoxCollider>();
        if (caja != null)
            return (caja.size.y * 0.5f - caja.center.y) * escalaY;

        return 1f;
    }
}