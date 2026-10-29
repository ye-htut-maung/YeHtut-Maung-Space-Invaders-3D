using UnityEngine;

public class Dart : MonoBehaviour
{
    [Header("Movement (units per second)")]
    [SerializeField, Min(0)] private float speed = 15f;
    private Camera gameCamera;
    public bool IsConsumed { get; private set; }

    public bool TryConsume()
    {
        if (IsConsumed) return false;
        IsConsumed = true;
        Destroy(gameObject);
        return true;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (IsConsumed || !GameOverController.IsPlaying)
        {
            return;
        }

        Vector3 position = transform.position;
        position.y += speed * Time.fixedDeltaTime;
        transform.position = position;
        float topEdge = gameCamera != null
            ? gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 1,
                Vector3.Dot(position - gameCamera.transform.position, gameCamera.transform.forward))).y
            : 15f;
        if (position.y > topEdge + 1f)
        {
            Destroy(gameObject);
        }
    }

}
