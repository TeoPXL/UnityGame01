using UnityEngine;
using UnityEngine.InputSystem;

public class NozzleController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float smoothness = 8f;
    public float recoilDistance = 0.2f;
    public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;

    [Header("Spread + Autofire")]
    public float spreadAngle = 5f;
    public float fireRate = 0.1f;
    private float nextFireTime = 0f;

    [Header("Audio Settings")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    [Header("Ammo Settings")]
    public int maxAmmo = 6; // defined here
    // currentAmmo no longer needed here

    private Color defaultColor;
    private Color targetColor;
    private Vector3 defaultPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            defaultColor = spriteRenderer.color;

        targetColor = defaultColor;
        defaultPosition = transform.localPosition;
        targetPosition = defaultPosition;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (GameStateManager.Instance.IsPaused)
            return;

        HandleShootingInput();
        HandleReloadInput();
        SmoothTransition();
    }

    private void HandleShootingInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            AttemptShot(withSpread: false, infinite: false);

        if (Keyboard.current.spaceKey.isPressed && Time.time >= nextFireTime)
        {
            if (AttemptShot(withSpread: true, infinite: true))
                nextFireTime = Time.time + fireRate;
        }
    }

    private void HandleReloadInput()
    {
        if (Keyboard.current[Key.R].wasPressedThisFrame)
            Reload();
    }

    private void Reload()
    {
        if (GameStateManager.Instance.CurrentState is PlayingState playing)
            playing.Reload();
    }

    private bool AttemptShot(bool withSpread, bool infinite)
    {
        if (!GameStateManager.Instance.IsPlaying)
            return false;

        if (GameStateManager.Instance.CurrentState is PlayingState playing)
        {
            if (!playing.TryShoot(infinite))
                return false;
        }

        SpawnBullet(withSpread);
        return true;
    }

    private void SpawnBullet(bool withSpread)
    {
        if (bulletPrefab == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Quaternion rotation = spawnPoint.rotation;

        if (withSpread)
        {
            float angleOffset = Random.Range(-spreadAngle, spreadAngle);
            rotation *= Quaternion.Euler(0, 0, angleOffset);
        }

        Instantiate(bulletPrefab, spawnPoint.position, rotation);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        targetPosition = defaultPosition - Vector3.up * recoilDistance;
        targetColor = flashColor;

        SimpleCameraShake camShake = Camera.main.GetComponent<SimpleCameraShake>();
        if (camShake != null)
            StartCoroutine(camShake.Shake(0.15f, 0.1f));
    }

    private void SmoothTransition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothness * Time.deltaTime);
        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, smoothness * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)
        {
            targetPosition = defaultPosition;
            targetColor = defaultColor;
        }
    }
}
