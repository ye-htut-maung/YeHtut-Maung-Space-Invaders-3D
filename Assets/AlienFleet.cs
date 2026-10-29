using UnityEngine;
using System.Collections.Generic;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject alienPrefab;

    public AlienCounter alienCounter;

    public List<GameObject> aliens = new List<GameObject> { };

    [Header("Movement (units per second)")]
    [SerializeField, Min(0)] private float startingSpeed = 4f;
    [SerializeField, Min(0)] private float maximumSpeed = 10f;
    private int initialAlienCount;
    private int direction = 1;

    public float CurrentSpeed
    {
        get
        {
            if (aliens.Count == 0)
            {
                return 0;
            }

            // The first alien destroyed starts the increase; the last survivor reaches the cap.
            float destroyedFraction = initialAlienCount > 1
                ? (float)(initialAlienCount - aliens.Count) / (initialAlienCount - 1)
                : 0;
            return Mathf.Lerp(startingSpeed, Mathf.Max(startingSpeed, maximumSpeed), destroyedFraction);
        }
    }

    private Camera gameCamera;
    private float leftBound = -16;
    private float rightBound = 16;
    private float baseFormationWidth;
    private float formationScale = 1f;
    float downTime = .1f;
    bool movingDown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameOverController.EnsureExists();
        gameCamera = Camera.main;
        float formationTop = gameCamera != null
            ? gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 1,
                Vector3.Dot(-gameCamera.transform.position, gameCamera.transform.forward))).y - 1f
            : 14f;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject alien = Instantiate<GameObject>(alienPrefab);
                alien.transform.position = new Vector3(2 * i - 12, formationTop - 2 * j, 0);
                aliens.Add(alien);
                alien.GetComponent<Alien>().fleet = this;
            }
        }
        initialAlienCount = aliens.Count;
        if (TryGetFleetBounds(out Bounds initialBounds))
        {
            baseFormationWidth = initialBounds.size.x;
        }
        FitFleetToView();
        RefreshCounter();
    }

    public void RefreshCounter()
    {
        if (alienCounter != null)
        {
            alienCounter.SetRemainingAliens(aliens.Count);
        }
    }

    void LateUpdate()
    {
        // Resize safely even while the title or pause menu is open.
        FitFleetToView();
        // Wait until spawning and this frame's collision callbacks have completed.
        if (initialAlienCount > 0 && aliens.Count == 0 && GameOverController.IsPlaying)
        {
            GameOverController.EnsureExists().WinGame();
        }
    }

    void FixedUpdate()
    {
        if (!GameOverController.IsPlaying || aliens.Count == 0)
        {
            return;
        }

        FitFleetToView();
        if (!TryGetFleetBounds(out Bounds bounds)) return;

        if (movingDown)
        {
            MoveFleet(Vector3.down * startingSpeed * Time.fixedDeltaTime);
            return;
        }

        float step = direction * CurrentSpeed * Time.fixedDeltaTime;
        // Clamp the whole visible fleet, not just the centers of its outer aliens.
        step = Mathf.Clamp(step, leftBound - bounds.min.x, rightBound - bounds.max.x);
        MoveFleet(Vector3.right * step);
        bool reachedEdge = direction > 0
            ? bounds.max.x + step >= rightBound - 0.0001f
            : bounds.min.x + step <= leftBound + 0.0001f;
        if (reachedEdge)
        {
            direction = -direction;
            movingDown = true;
            Invoke(nameof(StartSidewaysMotion), downTime);
        }
    }

    private bool TryGetFleetBounds(out Bounds bounds)
    {
        bounds = new Bounds();
        bool found = false;
        foreach (GameObject alien in aliens)
        {
            if (alien == null) continue;
            Renderer renderer = alien.GetComponentInChildren<Renderer>();
            Bounds alienBounds = renderer != null
                ? renderer.bounds : new Bounds(alien.transform.position, Vector3.zero);
            if (!found) bounds = alienBounds;
            else bounds.Encapsulate(alienBounds);
            found = true;
        }
        return found;
    }

    private void MoveFleet(Vector3 offset)
    {
        foreach (GameObject alien in aliens)
        {
            if (alien != null) alien.transform.position += offset;
        }
    }

    private void FitFleetToView()
    {
        if (gameCamera == null || !TryGetFleetBounds(out Bounds bounds)) return;
        float depth = Vector3.Dot(-gameCamera.transform.position, gameCamera.transform.forward);
        float left = gameCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, depth)).x;
        float right = gameCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, depth)).x;
        float margin = Mathf.Min(0.25f, (right - left) * 0.05f);
        leftBound = left + margin;
        rightBound = right - margin;

        // Keep 20% of a narrow playfield free for lateral movement.
        float desiredScale = baseFormationWidth > 0
            ? Mathf.Min(1, (rightBound - leftBound) * 0.8f / baseFormationWidth) : 1;
        if (!Mathf.Approximately(desiredScale, formationScale))
        {
            float ratio = desiredScale / formationScale;
            float center = bounds.center.x;
            foreach (GameObject alien in aliens)
            {
                if (alien == null) continue;
                Vector3 position = alien.transform.position;
                position.x = center + (position.x - center) * ratio;
                alien.transform.position = position;
                alien.transform.localScale *= ratio;
            }
            formationScale = desiredScale;
            TryGetFleetBounds(out bounds);
        }

        float correction = bounds.min.x < leftBound ? leftBound - bounds.min.x
            : bounds.max.x > rightBound ? rightBound - bounds.max.x : 0;
        if (correction != 0) MoveFleet(Vector3.right * correction);
    }

    private void StartSidewaysMotion()
    {
        movingDown = false;
    }
}
