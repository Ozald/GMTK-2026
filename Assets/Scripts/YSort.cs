using UnityEngine;
public class YSort : MonoBehaviour
{
    public Transform feet;
    public int offset = 0;
    public float precision = 100f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        spriteRenderer.sortingOrder =
            Mathf.RoundToInt(-feet.position.y * precision) + offset;
    }
}