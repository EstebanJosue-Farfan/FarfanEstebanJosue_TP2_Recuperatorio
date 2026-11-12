using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float multiplicadorCorrer = 1.7f;
    public KeyCode teclaCorrer = KeyCode.LeftShift;
    public float aceleracion = 40f;
    public float fuerzaSalto = 6f;
    public float alturaCaida = -10f;

    [Header("Chequeo de suelo (Raycast + LayerMask)")]
    [Tooltip("Marcar SOLO la capa Ground")]
    public LayerMask capaSuelo;
    public float distanciaSuelo = 1.1f;

    [Header("Apuntado")]
    [Tooltip("Si queda vacío usa la Main Camera")]
    public Camera camara;

    public bool EstaCorriendo { get; private set; }

    Rigidbody rb;
    PlayerHealth vida;
    Vector3 input;
    bool saltar;
    float multiplicadorZona = 1f; 
    Quaternion rotacionObjetivo;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        vida = GetComponent<PlayerHealth>();
        if (camara == null) camara = Camera.main;
        rotacionObjetivo = transform.rotation;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.EstaJugando)
        {
            input = Vector3.zero;
            return;
        }

        LeerMovimiento();

        if (Input.GetKeyDown(KeyCode.Space) && EstaEnSuelo()) saltar = true;

        ApuntarAlMouse();

        // Caer al vacío pasa por el sistema de salud
        if (transform.position.y < alturaCaida && vida != null)
            vida.MatarInstantaneo();
    }

    void FixedUpdate()
    {
        float mult = (EstaCorriendo ? multiplicadorCorrer : 1f) * multiplicadorZona;
        Vector3 objetivo = input * velocidad * mult;
        Vector3 actual = rb.linearVelocity;
        Vector3 horizontal = Vector3.MoveTowards(new Vector3(actual.x, 0, actual.z), objetivo, aceleracion * Time.fixedDeltaTime);
        float y = actual.y;
        if (saltar) { y = fuerzaSalto; saltar = false; }
        rb.linearVelocity = new Vector3(horizontal.x, y, horizontal.z);

        rb.MoveRotation(rotacionObjetivo);
    }

    void LeerMovimiento()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");


        Vector3 adelante = Vector3.forward;
        Vector3 derecha = Vector3.right;
        if (camara != null)
        {
            adelante = Vector3.ProjectOnPlane(camara.transform.forward, Vector3.up).normalized;
            derecha = Vector3.ProjectOnPlane(camara.transform.right, Vector3.up).normalized;
        }

        input = Vector3.ClampMagnitude(adelante * v + derecha * h, 1f);
        EstaCorriendo = Input.GetKey(teclaCorrer) && input != Vector3.zero;
    }

    void ApuntarAlMouse()
    {
        if (camara == null) return;

        Ray rayo = camara.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.up, transform.position);
        if (plano.Raycast(rayo, out float distancia))
        {
            Vector3 dir = rayo.GetPoint(distancia) - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) rotacionObjetivo = Quaternion.LookRotation(dir);
        }
    }

    bool EstaEnSuelo()
    {

        bool enSuelo = Physics.Raycast(transform.position, Vector3.down, distanciaSuelo, capaSuelo, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(transform.position, Vector3.down * distanciaSuelo, enSuelo ? Color.green : Color.red);
        return enSuelo;
    }


    public void SetMultiplicadorZona(float valor)
    {
        multiplicadorZona = valor;
    }
}
