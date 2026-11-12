#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AbrirEscenaInicial
{
    const string RutaEscena = "Assets/Scenes/SampleScene.unity";
    const string Clave = "AbrirEscenaInicial_Hecho";

    static AbrirEscenaInicial()
    {
        EditorApplication.delayCall += Abrir;
    }

    static void Abrir()
    {
        // Solo una vez por sesión de Unity (no molesta mientras trabajás)
        if (SessionState.GetBool(Clave, false)) return;
        SessionState.SetBool(Clave, true);

        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SceneManager.GetActiveScene().path == RutaEscena) return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena) == null)
        {
            Debug.LogWarning($"[AbrirEscenaInicial] No se encontró la escena en {RutaEscena}");
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(RutaEscena);
    }
}
#endif