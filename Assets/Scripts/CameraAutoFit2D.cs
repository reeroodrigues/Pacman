using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoFit2D : MonoBehaviour
{
    public enum FitMode
    {
        FitInside,       // caber tudo (pode sobrar borda)
        FillSafeArea,    // encostar em todas as bordas (pode cortar)
        FitWidthToSafeArea // encostar nas LATERAIS, ignorar topo/rodapé
    }

    [Header("Alvo (labirinto)")]
    [SerializeField] private Transform worldRoot;          // ex.: Grid
    [SerializeField] private float marginWorldUnits = 0.0f; // use 0~0.1 p/ laterais colarem

    [Header("Ajuste")]
    [SerializeField] private FitMode fitMode = FitMode.FitWidthToSafeArea;
    [SerializeField] private float extraOverscan = 0.0f;   // 0..0.03 se notar “linha” na borda

    private Camera cam;
    private Bounds worldBounds;
    private Vector2Int lastScreen;
    private Rect lastSafeArea;

    void Awake() => cam = GetComponent<Camera>();
    void Start() { RecalcBounds(); Fit(); }

    void Update()
    {
        if (Screen.width != lastScreen.x || Screen.height != lastScreen.y || Screen.safeArea != lastSafeArea)
            Fit();
    }

    [ContextMenu("Recalc Bounds")]
    public void RecalcBounds()
    {
        if (!worldRoot) return;
        var rs = worldRoot.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return;

        worldBounds = rs[0].bounds;
        foreach (var r in rs.Skip(1)) worldBounds.Encapsulate(r.bounds);
        worldBounds.Expand(marginWorldUnits * 2f);
    }

    public void Fit()
    {
        if (!cam || worldBounds.size == Vector3.zero) return;

        // Aspecto efetivo do device via SAFE AREA
        Rect sa = Screen.safeArea;
        float effW = sa.width  > 0 ? sa.width  : Screen.width;
        float effH = sa.height > 0 ? sa.height : Screen.height;
        float targetAspect = effW / effH;

        float worldW = worldBounds.size.x;
        float worldH = worldBounds.size.y;

        float sizeByHeight = worldH * 0.5f;                   // caber por altura
        float sizeByWidth  = (worldW / targetAspect) * 0.5f;  // caber por largura

        float size;
        switch (fitMode)
        {
            case FitMode.FitInside:        size = Mathf.Max(sizeByHeight, sizeByWidth); break;
            case FitMode.FillSafeArea:     size = Mathf.Min(sizeByHeight, sizeByWidth); break;
            case FitMode.FitWidthToSafeArea:
            default:                       size = sizeByWidth;                           break; // << encosta nas LATERAIS
        }

        size *= (1f + extraOverscan);
        cam.orthographicSize = size;

        // centraliza no mundo (se quiser “puxar” o labirinto pra cima, ajuste o Y aqui)
        var c = worldBounds.center;
        cam.transform.position = new Vector3(c.x, c.y + 4.62f, cam.transform.position.z);

        lastScreen   = new Vector2Int(Screen.width, Screen.height);
        lastSafeArea = Screen.safeArea;
    }
}
