using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Movement (units per second)")]

    [SerializeField, Min(0)] private float speed = 5f;
    private bool hasCollided;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (hasCollided || !GameOverController.IsPlaying)
        {
            return;
        }

        Vector3 position = transform.position;
        position.y -= speed * Time.fixedDeltaTime;
        transform.position = position;
        if(position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (hasCollided || !GameOverController.IsPlaying)
        {
            return;
        }

        // The ship's colliders are on children, but its controller is on the root.
        Spaceship spaceship = coll.collider.GetComponentInParent<Spaceship>();
        if (spaceship != null)
        {
            hasCollided = true;
            Destroy(gameObject);
            spaceship.TakeHit();
        }
        else
        {
            Dart dart = coll.collider.GetComponentInParent<Dart>();
            if (dart == null || !dart.TryConsume()) return;
            hasCollided = true;
            Vector3 impact = coll.contactCount > 0 ? coll.GetContact(0).point : transform.position;
            ImpactEffect.Spawn(impact, ImpactEffect.Kind.ProjectileClash);
            Destroy(gameObject);
        }
    }
}
