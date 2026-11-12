using UnityEngine;

/// <summary>
/// Puerta que se abre/cierra al interactuar.
/// Poné este script en un objeto vacío ubicado en la BISAGRA, con el cubo de la puerta como hijo.
/// El cubo hijo debe estar en la capa Interactable.
/// </summary>
public class Puerta : MonoBehaviour, IInteractuable
{
    public float anguloApertura = 90f;
    public float velocidad = 4f;

    bool abierta;
    Quaternion rotCerrada;
    Quaternion rotAbierta;

    void Start()
    {
        rotCerrada = transform.localRotation;
        rotAbierta = rotCerrada * Quaternion.Euler(0f, anguloApertura, 0f);
    }

    void Update()
    {
        Quaternion objetivo = abierta ? rotAbierta : rotCerrada;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, objetivo, velocidad * Time.deltaTime);
    }

    public string Mensaje() => abierta ? "Cerrar puerta" : "Abrir puerta";

    public void Interactuar()
    {
        abierta = !abierta;
        Debug.Log($"[Puerta] {name} {(abierta ? "abierta" : "cerrada")}");
    }
}
