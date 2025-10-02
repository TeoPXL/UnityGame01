using UnityEngine;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        private Transform target;
        private float initialScaleX;

        public void Initialize(Transform enemy)
        {
            target = enemy;
            initialScaleX = transform.localScale.x; // save original width
        }

        void Update()
        {
            if (target != null)
            {
                // Make the health bar follow the enemy
                transform.position = target.position + new Vector3(0, 1.2f, 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // healthNormalized = currentHealth / maxHealth
        public void SetHealth(float healthNormalized)
        {
            healthNormalized = Mathf.Clamp01(healthNormalized);
            transform.localScale = new Vector3(initialScaleX * healthNormalized, transform.localScale.y,
                transform.localScale.z);
        }
    }
}