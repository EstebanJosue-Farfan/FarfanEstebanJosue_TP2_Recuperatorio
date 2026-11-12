using UnityEngine;


public class CamaraSeguimiento : MonoBehaviour
{
    [Tooltip("Si queda vacío busca el objeto con tag Player")]
    public Transform objetivo;
    public Vector3 offset = new Vector3(0, 12, -8);
    public float suavizado = 5f;
    [Tooltip("Cuánto se adelanta la cámara hacia donde apunta el mouse")]
    public float anticipacionMouse = 4f;
    [Tooltip("Al empezar, orienta la cámara para mirar al jugador según el offset")]
    public bool mirarAlObjetivo = true;

    void Start()
    {
        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
        }
        if (mirarAlObjetivo && offset != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(-offset);
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 extra = Vector3.zero;
        bool jugando = GameManager.Instance == null || GameManager.Instance.EstaJugando;

        if (jugando && anticipacionMouse > 0f)
        {
            Vector2 m = new Vector2(Input.mousePosition.x / Screen.width - 0.5f,
                                    Input.mousePosition.y / Screen.height - 0.5f) * 2f;
            m = Vector2.ClampMagnitude(m, 1f);

            Vector3 derecha = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 adelante = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            extra = (derecha * m.x + adelante * m.y) * anticipacionMouse;
        }

        Vector3 destino = objetivo.position + offset + extra;
        transform.position = Vector3.Lerp(transform.position, destino, suavizado * Time.deltaTime);
    }
}
