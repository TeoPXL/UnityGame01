using state;
using UI;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public EnemySpawnController spawnController; // assigned on spawn

    public enum Type
    {
        Tank,
        Speedy,
        Sporadic
    }

    public Type enemyType = Type.Tank;
    public Transform targetPoint; // Point enemies are biased towards

    // Audio clips
    public AudioClip hitPlanetSound;
    public AudioClip hitByBulletSound;
    public AudioClip powerupSound;

    private Vector3 currentDirection;
    private Vector3 targetDirection;

    // Movement parameters
    private float speed;
    private float biasStrength;
    private float randomnessStrength;
    private float smoothing = 3f; // how quickly direction changes

    private SpriteRenderer sr; // For coloring

    // For rainbow effect
    private float rainbowHue;

    // HEALTH SYSTEM
    private int maxHealth;
    private int currentHealth;

    private HealthBar healthBar;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (!sr)
        {
            Debug.LogError("Enemy prefab has no child with a SpriteRenderer!");
        }

        float difficulty = GameStateManager.Instance.settings.Difficulty;

        // Set movement stats and color
        switch (enemyType)
        {
            case Type.Tank:
                speed = 2f * difficulty;
                biasStrength = 0.6f * difficulty;
                randomnessStrength = 0.5f * difficulty;
                transform.localScale = Vector3.one * 1.1f;
                sr.color = Color.red;
                break;
            case Type.Speedy:
                speed = 5f * difficulty;
                biasStrength = 1.2f * difficulty;
                randomnessStrength = 0.6f * difficulty;
                transform.localScale = Vector3.one * 0.9f;
                sr.color = Color.white;
                break;
            case Type.Sporadic:
                speed = 7f * difficulty;
                biasStrength = 0.01f * difficulty;
                randomnessStrength = 3.5f * difficulty;
                smoothing = 500f;
                transform.localScale = Vector3.one;
                break;
        }

        // Initialize health based on difficulty and type
        maxHealth = CalculateHealth(enemyType, difficulty);
        currentHealth = maxHealth;

        // Start with a random direction
        currentDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
        targetDirection = currentDirection;
    }

    private int CalculateHealth(Type type, float difficulty)
    {
        switch (type)
        {
            case Type.Tank:
                if (difficulty < 1f) return 2;
                if (difficulty >= 1f && difficulty < 1.5f) return 4;
                if (difficulty >= 1.5f) return 5;
                return 3; // default
            case Type.Speedy:
                if (difficulty > 1f && difficulty < 1.5f) return 2;
                break;
            case Type.Sporadic:
                if (difficulty > 1.5f) return 2;
                break;
        }

        return 1;
    }

    void Update()
    {
        if (GameStateManager.Instance.IsMenu)
        {
            Destroy(gameObject);
        }

        if (GameStateManager.Instance.IsPaused)
            return;

        // Random nudge
        Vector3 randomNudge = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized *
                              (randomnessStrength * Time.deltaTime);

        // Bias toward target with sin wave modulation
        Vector3 biasDir = targetPoint.position - transform.position;
        biasDir.z = 0f;
        biasDir.Normalize();

        float sinWave = Mathf.Sin(Time.time * 2f);
        float biasMultiplier = sinWave > -0.4f ? 1f : -0.5f;
        Vector3 modulatedBias = biasDir * (biasStrength * biasMultiplier);

        if (enemyType == Type.Sporadic)
        {
            targetDirection = (modulatedBias + currentDirection + randomNudge * 2f).normalized;
            currentDirection = Vector3.Lerp(currentDirection, targetDirection, Time.deltaTime * smoothing).normalized;
        }
        else
        {
            targetDirection = (modulatedBias + currentDirection + randomNudge).normalized;
            currentDirection = Vector3.Lerp(currentDirection, targetDirection, Time.deltaTime * smoothing).normalized;
        }

        // Move enemy
        Vector3 newPos = transform.position + currentDirection * (speed * Time.deltaTime);
        newPos.z = 0f;
        transform.position = newPos;

        // Rotate smoothly in 2D
        if (currentDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, angle - 90f),
                Time.deltaTime * 5f);
        }

        // Rainbow for sporadic
        if (enemyType == Type.Sporadic)
        {
            rainbowHue += Time.deltaTime * 0.5f;
            if (rainbowHue > 1f) rainbowHue -= 1f;
            sr.color = Color.HSVToRGB(rainbowHue, 1f, 1f);
        }

        if (spawnController != null && !spawnController.IsInsideKillZone(transform.position))
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "PlanetSprite")
        {
            if (hitPlanetSound != null)
            {
                PlaySound2D(hitPlanetSound);
            }

            Destroy(gameObject);
        }
        else if (collision.gameObject.name.Contains("Bullet"))
        {
            TakeDamage(1);
        }
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (enemyType == Type.Sporadic && powerupSound != null)
        {
            PlaySound2D(powerupSound);
        }
        else if (hitByBulletSound != null)
        {
            PlaySound2D(hitByBulletSound);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        GameStateManager.Instance.Score++;
    }

    void PlaySound2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempGo = new GameObject("TempAudio");
        tempGo.transform.position = Vector3.zero; // position irrelevant for 2D
        AudioSource aSource = tempGo.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.spatialBlend = 0f; // 0 = 2D sound
        aSource.Play();
        Destroy(tempGo, clip.length);
    }
}