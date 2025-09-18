using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    [SerializeField] private Transform mapRoot;
    [SerializeField] private float padding = 1f;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        Bounds bounds = new Bounds(mapRoot.position, Vector3.zero);
        foreach (Renderer r in mapRoot.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(r.bounds);

        float mapWidth = bounds.size.x;
        float mapHeight = bounds.size.y;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = mapWidth / mapHeight;

        if (screenRatio >= targetRatio)
        {
            _cam.orthographicSize = (mapHeight / 2f) + padding;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            _cam.orthographicSize = (mapHeight / 2f) * differenceInSize + padding;
        }
        
        _cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
    }
}
