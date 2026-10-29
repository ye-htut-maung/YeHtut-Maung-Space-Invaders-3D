using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]

    public GameObject bulletPrefab;
    public AlienFleet fleet;
    
    static float bulletSpawnRate = .002f;
    public float lastShot = 0;
    public static float shotCooldown = .1f;
    private bool isDying;

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
        if (!GameOverController.IsPlaying)
        {
            return;
        }

        Vector3 position = transform.position;
        
        if(UnityEngine.Random.value < bulletSpawnRate && Time.time - lastShot > shotCooldown){
            GameObject bullet = Instantiate<GameObject>(bulletPrefab);
            bullet.transform.position = position;
            lastShot = Time.time;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!GameOverController.IsPlaying || isDying)
        {
            return;
        }

        Dart dart = coll.collider.GetComponentInParent<Dart>();
        if (dart == null || !dart.TryConsume()) return;

        isDying = true;
        ImpactEffect.Spawn(transform.position, ImpactEffect.Kind.AlienDestroyed);
        if (fleet.aliens.Remove(gameObject))
        {
            fleet.RefreshCounter();
        }
        Destroy(gameObject);
    }
}
