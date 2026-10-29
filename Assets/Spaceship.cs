using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    [SerializeField, Min(0.01f)] private float shotCooldown = 0.35f;
    public float lastShot = 0;
    public float speed = 10f;
    private bool isDead;
    private Camera gameCamera;
    private Renderer[] shipRenderers;
    public const int StartingLives = 3;
    public int Lives { get; private set; } = StartingLives;
    [SerializeField, Min(0.1f)] private float hitProtectionDuration = 1.25f;
    private float invulnerableUntil;
    public bool IsInvulnerable => !isDead && Time.time < invulnerableUntil;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameOverController.EnsureExists();
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            // Bullet impacts must not rotate the ship or knock it out of its row.
            body.constraints |= RigidbodyConstraints.FreezeRotation
                | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            body.angularVelocity = Vector3.zero;
        }
        gameCamera = Camera.main;
        shipRenderers = GetComponentsInChildren<Renderer>();
        GameOverController.EnsureExists().UpdateLives(Lives);
        // A fresh round can fire immediately; only later shots must wait.
        lastShot = float.NegativeInfinity;
    }

    // Update is called once per frame
    void Update()
    {
        // Destroy is deferred; stop accepting input as soon as a fatal hit occurs.
        if (isDead || !GameOverController.IsPlaying || Keyboard.current == null)
        {
            return;
        }

        // Flash while protected, then restore the complete ship automatically.
        bool visible = !IsInvulnerable || Mathf.FloorToInt(Time.time * 10) % 2 == 0;
        foreach (Renderer piece in shipRenderers)
        {
            piece.enabled = visible;
        }

        Vector3 nextPosition = transform.position;
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            nextPosition.x += speed * Time.deltaTime;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            nextPosition.x -= speed * Time.deltaTime;
        }
        transform.position = ClampToView(nextPosition);
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time - lastShot >= shotCooldown)
        {
            Vector3 position = transform.position;
            GameObject bullet = Instantiate<GameObject>(dartPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
    }

    private Vector3 ClampToView(Vector3 nextPosition)
    {
        if (gameCamera == null || shipRenderers.Length == 0)
        {
            return nextPosition;
        }

        // Measure the complete ship graphic, including any offset child renderers.
        Bounds bounds = shipRenderers[0].bounds;
        for (int i = 1; i < shipRenderers.Length; i++)
        {
            bounds.Encapsulate(shipRenderers[i].bounds);
        }

        float depth = Vector3.Dot(transform.position - gameCamera.transform.position,
            gameCamera.transform.forward);
        float leftEdge = gameCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, depth)).x;
        float rightEdge = gameCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth)).x;
        const float margin = 0.1f;
        float minX = leftEdge + (transform.position.x - bounds.min.x) + margin;
        float maxX = rightEdge - (bounds.max.x - transform.position.x) - margin;

        // If the view is narrower than the ship, keep it centered.
        nextPosition.x = minX <= maxX
            ? Mathf.Clamp(nextPosition.x, minX, maxX)
            : (minX + maxX) * 0.5f;
        return nextPosition;
    }

    public void TakeHit()
    {
        if (isDead || !GameOverController.IsPlaying || IsInvulnerable)
        {
            return;
        }

        Lives--;
        GameOverController.EnsureExists().UpdateLives(Lives);
        if (Lives == 0)
        {
            Die();
            return;
        }

        invulnerableUntil = Time.time + hitProtectionDuration;
        ImpactEffect.Spawn(transform.position, ImpactEffect.Kind.ShipHit);
    }

    public void Die()
    {
        if (isDead || !GameOverController.IsPlaying)
        {
            return;
        }

        Lives = 0;
        GameOverController.EnsureExists().UpdateLives(Lives);
        isDead = true;
        enabled = false;
        ImpactEffect.Spawn(transform.position, ImpactEffect.Kind.ShipDestroyed);
        GameOverController.EnsureExists().EndGame();
        Destroy(gameObject);
    }

}


