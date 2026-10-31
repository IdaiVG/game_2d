using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10);

    [Header("Limites del escenario")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-1000f, -1000f);
    public Vector2 maxBounds = new Vector2(1000f, 1000f);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            float halfHeight = cam != null ? cam.orthographicSize : 5f;
            float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 8f;

            float minX = minBounds.x + halfWidth;
            float maxX = maxBounds.x - halfWidth;
            float minY = minBounds.y + halfHeight;
            float maxY = maxBounds.y - halfHeight;

            desired.x = minX <= maxX ? Mathf.Clamp(desired.x, minX, maxX) : (minBounds.x + maxBounds.x) * 0.5f;
            desired.y = minY <= maxY ? Mathf.Clamp(desired.y, minY, maxY) : (minBounds.y + maxBounds.y) * 0.5f;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            Mathf.Clamp01(smoothSpeed * Time.deltaTime)
        );
    }
}
