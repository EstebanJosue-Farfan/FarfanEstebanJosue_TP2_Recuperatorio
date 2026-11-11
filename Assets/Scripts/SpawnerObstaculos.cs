using UnityEngine;

public class SpawnerObstaculos : MonoBehaviour
{
    public GameObject prefab;
    public float retrasoInicial = 3f;
    public float intervalo = 2f;
    public float radioArea = 5f;
    public float vidaObstaculo = 6f;

    void Start()
    {
        InvokeRepeating(nameof(Generar), retrasoInicial, intervalo);
    }

    void Generar()
    {
        Vector3 pos = transform.position + new Vector3(Random.Range(-radioArea, radioArea), 0, Random.Range(-radioArea, radioArea));
        GameObject o = Instantiate(prefab, pos, Quaternion.identity);
        Destroy(o, vidaObstaculo);
    }

    public void Detener()
    {
        CancelInvoke(nameof(Generar));
    }
}