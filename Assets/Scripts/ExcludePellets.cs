using UnityEngine;

public class ExcludePelletsBox : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform exclusionCenter;
    [SerializeField] private Transform pelletsRoot;

    [Header("Zona de exclusão (retângulo em torno do objeto)")]
    [SerializeField] private Vector2 boxSize = new Vector2(4f, 3f);
    [SerializeField] private bool ignorePowerPellets = true;
    [SerializeField] private string powerPelletTag = "PowerPellet";

    private void Start()
    {
        if (exclusionCenter == null || pelletsRoot == null) return;

        Vector2 center = exclusionCenter.position;
        Vector2 half = boxSize * 0.5f;

        for (int i = pelletsRoot.childCount - 1; i >= 0; i--)
        {
            Transform p = pelletsRoot.GetChild(i);
            if (ignorePowerPellets && p.CompareTag(powerPelletTag))
                continue;

            Vector2 d = (Vector2)p.position - center;
            if (Mathf.Abs(d.x) <= half.x && Mathf.Abs(d.y) <= half.y)
                Destroy(p.gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (exclusionCenter == null) return;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.25f);
        Gizmos.DrawCube(exclusionCenter.position, new Vector3(boxSize.x, boxSize.y, 0f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(exclusionCenter.position, new Vector3(boxSize.x, boxSize.y, 0f));
    }
#endif
}