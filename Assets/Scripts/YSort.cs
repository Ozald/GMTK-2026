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
        if (feet == null)
        {
            Debug.LogWarning("Feet transform is not assigned for YSort on " + gameObject.name);
            return;
        }

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-feet.position.y * precision) + offset;
    }
}