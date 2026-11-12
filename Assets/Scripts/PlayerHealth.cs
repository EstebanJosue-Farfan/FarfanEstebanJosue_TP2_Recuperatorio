using System;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    public int vidaMaxima = 100;
    [Tooltip("Segundos de inmunidad después de recibir un golpe")]
    public float tiempoInmunidad = 1f;

    public int VidaActual { get; private set; }
    public bool EstaMuerto => VidaActual <= 0;
    public bool EstaHerido => VidaActual < vidaMaxima;

    public event Action<int, int> OnVidaCambio; // (actual, máxima)
    public event Action OnMuerte;

    float ultimoGolpe = -999f;

    void Awake()
    {
        VidaActual = vidaMaxima;
    }


    public bool RecibirDanio(int cantidad, bool ignorarInmunidad = false)
    {
        if (EstaMuerto || cantidad <= 0) return false;
        if (!ignorarInmunidad && Time.time < ultimoGolpe + tiempoInmunidad) return false;

        if (!ignorarInmunidad) ultimoGolpe = Time.time;
        VidaActual = Mathf.Max(0, VidaActual - cantidad);
        Debug.Log($"[PlayerHealth] Daño -{cantidad}. Vida: {VidaActual}/{vidaMaxima}");
        OnVidaCambio?.Invoke(VidaActual, vidaMaxima);

        if (EstaMuerto) Morir();
        return true;
    }


    public int Curar(int cantidad)
    {
        if (EstaMuerto || cantidad <= 0) return 0;
        int antes = VidaActual;
        VidaActual = Mathf.Min(vidaMaxima, VidaActual + cantidad);
        int curado = VidaActual - antes;
        if (curado > 0) OnVidaCambio?.Invoke(VidaActual, vidaMaxima);
        return curado;
    }


    public void MatarInstantaneo()
    {
        if (EstaMuerto) return;
        VidaActual = 0;
        OnVidaCambio?.Invoke(VidaActual, vidaMaxima);
        Morir();
    }

    void Morir()
    {
        Debug.Log("[PlayerHealth] El jugador murió");
        OnMuerte?.Invoke();
    }
}
