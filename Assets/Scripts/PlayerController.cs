using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float aceleracion = 40f;
    public float fuerzaSalto = 6f;
    public float alturaCaida = -10f;

    [Header("Transporte")]
    public Transform puntoTransporte;
    public float radioRecoger = 2f;
    public KeyCode teclaRecoger = KeyCode.E;
    public KeyCode teclaSoltar = KeyCode.G;

    [Header("Power-Up")]
    public float duracionBoost = 4f;
    public float multiplicadorBoost = 1.8f;
    public Color colorBoost = Color.yellow;

    public bool BoostActivo { get; private set; }

    Rigidbody rb;
    Renderer rend;
    Color colorOriginal;
    Vector3 input;
    bool saltar;
    float multiplicador = 1f;
    Transportable objetoLlevado;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rend = GetComponent<Renderer>();
        colorOriginal = rend.material.color;
    }

    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && EstaEnSuelo()) saltar = true;

        if (Input.GetKeyDown(teclaRecoger) && objetoLlevado == null) Recoger();
        if (Input.GetKeyDown(teclaSoltar) && objetoLlevado != null) Soltar();

        if (transform.position.y < alturaCaida)
            GameManager.Instance.Perder();
    }

    void FixedUpdate()
    {
        Vector3 objetivo = input * velocidad * multiplicador;
        Vector3 actual = rb.linearVelocity;
        Vector3 horizontal = Vector3.MoveTowards(new Vector3(actual.x, 0, actual.z), objetivo, aceleracion * Time.fixedDeltaTime);
        float y = actual.y;
        if (saltar) { y = fuerzaSalto; saltar = false; }
        rb.linearVelocity = new Vector3(horizontal.x, y, horizontal.z);

        if (input != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(input);
    }

    bool EstaEnSuelo()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, ~0, QueryTriggerInteraction.Ignore);
    }

    void Recoger()
    {
        Collider[] cercanos = Physics.OverlapSphere(transform.position, radioRecoger);
        foreach (Collider c in cercanos)
        {
            Transportable t = c.GetComponent<Transportable>();
            if (t != null && !t.EstaLlevado)
            {
                t.Recoger(puntoTransporte);
                objetoLlevado = t;
                return;
            }
        }
    }

    void Soltar()
    {
        objetoLlevado.Soltar();
        objetoLlevado = null;
    }

    public void ActivarBoost()
    {
        if (!BoostActivo) StartCoroutine(BoostCoroutine());
    }

    IEnumerator BoostCoroutine()
    {
        BoostActivo = true;
        multiplicador = multiplicadorBoost;
        rend.material.color = colorBoost;
        if (GameManager.Instance != null) GameManager.Instance.MostrarBoost(true);

        yield return new WaitForSeconds(duracionBoost);

        multiplicador = 1f;
        rend.material.color = colorOriginal;
        BoostActivo = false;
        if (GameManager.Instance != null) GameManager.Instance.MostrarBoost(false);
    }
}
