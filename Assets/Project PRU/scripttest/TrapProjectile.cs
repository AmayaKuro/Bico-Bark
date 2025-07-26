using UnityEngine;

public class TrapProjectile : MonoBehaviour
{
    private float damage = 10f;
    private float speed = 10f;
    private Vector3 direction;
    private Rigidbody2D rb;

    public void Setup(float dmg, float spd, Vector3 dir)
    {
        damage = dmg;
        speed = spd;
        direction = dir.normalized;

        // Setup movement
        
        rb = GetComponent<Rigidbody2D>(); 
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.linearVelocity = direction * speed;

        // destroy after 5 seconds to prevent memory leaks
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

}
