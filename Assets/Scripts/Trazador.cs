using UnityEngine;

/// <summary>
/// Crea por código la línea que se ve al disparar (así no hay que configurarla a mano).
/// </summary>
public static class Trazador
{
    public static LineRenderer Crear(Transform padre, Color color, float ancho)
    {
        GameObject go = new GameObject("TrazoDisparo");
        go.transform.SetParent(padre, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = ancho;
        lr.endWidth = ancho;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            lr.material = new Material(shader);
            lr.material.color = color;
        }
        lr.startColor = color;
        lr.endColor = color;
        lr.enabled = false;
        return lr;
    }
}
