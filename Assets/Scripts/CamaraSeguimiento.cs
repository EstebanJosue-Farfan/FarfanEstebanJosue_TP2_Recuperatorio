using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    public Transform objetivo;
    public Vector3 offset = new Vector3(0, 9, -7);
    public float suavizado = 5f;

    void LateUpdate()
    {
        if (objetivo == null) return;
        transform.position = Vector3.Lerp(transform.position, objetivo.position + offset, suavizado * Time.deltaTime);
    }
}
