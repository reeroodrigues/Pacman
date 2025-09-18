using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartGameRouter : MonoBehaviour
{
    public enum PickMode { Random, Alternate, Sequential }

    [Header("Gameplay Scenes (names)")]
    [SerializeField] private string[] gameplayScenes;

#if UNITY_EDITOR
    [Header("Editor helper (optional)")]
    [SerializeField] private SceneAsset[] sceneAssets;
    private void OnValidate()
    {
        if (sceneAssets != null && sceneAssets.Length > 0)
        {
            gameplayScenes = new string[sceneAssets.Length];
            for (int i = 0; i < sceneAssets.Length; i++)
                gameplayScenes[i] = sceneAssets[i] ? sceneAssets[i].name : null;
        }
    }

    [ContextMenu("Reset Alternate Index (PlayerPrefs)")]
    private void _CM_Reset() => ResetAlternateIndex();
#endif

    [Header("Mode")]
    [SerializeField] private PickMode mode = PickMode.Random;

    [Header("Persistence")]
    [SerializeField] private string prefsKey = "router_lastIdx";
    [SerializeField] private bool strictAlternateTwo = true;

    [Header("Safety")]
    [SerializeField] private float debounceSeconds = 0.25f;
    private static float s_blockUntil = 0f;
    
    public void StartGame()
    {
        if (!CanStartNow()) return;

        var idx = 0;
        switch (mode)
        {
            case PickMode.Random:
                idx = Random.Range(0, SafeLength());
                break;

            case PickMode.Alternate:
                idx = ComputeAlternateIndex();
                break;

            case PickMode.Sequential:
                idx = ComputeSequentialIndex();
                break;
        }

        LoadByIndex(idx, $"mode {mode}");
    }
    
    public void StartAlternate()
    {
        if (!CanStartNow()) return;

        var idx = ComputeAlternateIndex();
        LoadByIndex(idx, "StartAlternate()");
    }
    
    private bool CanStartNow()
    {
        if (Time.unscaledTime < s_blockUntil)
        {
         //   Debug.Log("[StartGameRouter] Ignorando chamada duplicada.");
            return false;
        }
        s_blockUntil = Time.unscaledTime + debounceSeconds;

        if (gameplayScenes == null || gameplayScenes.Length == 0)
        {
          //  Debug.LogError("[StartGameRouter] No gameplay scenes configured.");
            return false;
        }
        for (int i = 0; i < gameplayScenes.Length; i++)
        {
            if (string.IsNullOrEmpty(gameplayScenes[i]))
            {
              //  Debug.LogError($"[StartGameRouter] Scene name at index {i} is empty.");
                return false;
            }
        }
        return true;
    }

    private int SafeLength() => (gameplayScenes != null) ? Mathf.Max(1, gameplayScenes.Length) : 1;

    private int ComputeSequentialIndex()
    {
        var last = PlayerPrefs.GetInt(prefsKey, -1);
        var idx = (last + 1 + SafeLength()) % SafeLength();
        PlayerPrefs.SetInt(prefsKey, idx);
        PlayerPrefs.Save();
        return idx;
    }

    private int ComputeAlternateIndex()
    {
        var n = SafeLength();
        var last = PlayerPrefs.GetInt(prefsKey, -1);

        int idx;
        if (strictAlternateTwo && n == 2)
        {
            idx = (last == 0) ? 1 : 0;
        }
        else
        {
            idx = (last + 1 + n) % n;
        }

        PlayerPrefs.SetInt(prefsKey, idx);
        PlayerPrefs.Save();
        return idx;
    }

    private void LoadByIndex(int idx, string reason)
    {
        var scene = gameplayScenes[idx];

        if (!IsInBuildSettings(scene))
            Debug.LogWarning($"[StartGameRouter] Scene '{scene}' not found in Build Settings.");

        Debug.Log($"[StartGameRouter] Loading '{scene}' ({reason}, idx {idx})");
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    private static bool IsInBuildSettings(string sceneName)
    {
        var count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    public void ResetAlternateIndex()
    {
        PlayerPrefs.DeleteKey(prefsKey);
        PlayerPrefs.Save();
        Debug.Log("[StartGameRouter] Alternate index reset.");
    }
}
