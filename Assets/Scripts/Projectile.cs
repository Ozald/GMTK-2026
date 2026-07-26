using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 1f;
    public float lifetime = 3f;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transform.position.y > other.transform.position.y + 1f || transform.position.y < other.transform.position.y - 1f)
            return;

        if (other.CompareTag("Player"))
        {
            // Deal damage here
            Destroy(gameObject);
        }
    }
}