using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 15f; // Units per second
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move up relative to the bullet's local orientation
        transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.Self);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BulletController>() != null)
        {
            return;
        }

        // Check if the object we hit has an EnemyController
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            if (enemy.enemyType == EnemyController.Type.Sporadic)
            {
                // Find nearest other enemy to ricochet toward
                EnemyController nearest = FindNearestEnemy(enemy);
                if (nearest != null)
                {
                    // Rotate bullet toward nearest enemy
                    Vector3 dir = (nearest.transform.position - transform.position).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
                else
                {
                    // No other enemy found, destroy bullet
                    Destroy(gameObject);
                }
            }
            else
            {
                // Normal enemy, destroy bullet
                Destroy(gameObject);
            }
        }
        else
        {
            // Hit something else (e.g., planet), destroy bullet
            Destroy(gameObject);
        }
    }

    EnemyController FindNearestEnemy(EnemyController ignore)
    {
        // Find all EnemyController instances in the scene (unsorted, faster)
        EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        EnemyController nearest = null;
        float minDist = float.MaxValue;

        foreach (EnemyController e in enemies)
        {
            if (e == ignore) continue; // Skip the one we just hit
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }
}