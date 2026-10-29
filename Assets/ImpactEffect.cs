using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    public enum Kind { AlienDestroyed, ShipHit, ShipDestroyed, ProjectileClash }

    private ParticleSystem particles;
    private static Material particleMaterial;
    private static Texture2D glowTexture;
    private Material instanceMaterial;

    public static void Spawn(Vector3 position, Kind kind)
    {
        if (particleMaterial == null)
            particleMaterial = Resources.Load<Material>("ImpactParticles");

        Color cyan = new Color(0.2f, 0.85f, 1f);
        Color violet = new Color(0.75f, 0.25f, 1f);
        Color color = kind == Kind.AlienDestroyed ? violet : cyan;
        Color accent = kind == Kind.ProjectileClash ? violet : Color.Lerp(color, Color.white, 0.75f);
        int count = kind == Kind.ShipDestroyed ? 40 : kind == Kind.ShipHit ? 16
            : kind == Kind.ProjectileClash ? 12 : 24;
        float scale = kind == Kind.ShipDestroyed ? 1.6f
            : kind == Kind.ProjectileClash ? 0.7f : 1f;

        GameObject effect = new GameObject("ImpactBurst_" + kind);
        effect.transform.position = position + Vector3.back;
        ParticleSystem system = effect.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = system.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2 * scale, 5 * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f * scale, 0.25f * scale);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
        main.startColor = new ParticleSystem.MinMaxGradient(color, accent);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = count + 1;
        main.stopAction = ParticleSystemStopAction.Destroy;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        ParticleSystem.ColorOverLifetimeModule fading = system.colorOverLifetime;
        fading.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0),
            new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        fading.color = gradient;
        ParticleSystem.SizeOverLifetimeModule shrinking = system.sizeOverLifetime;
        shrinking.enabled = true;
        shrinking.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0));
        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        Material material = new Material(particleMaterial);
        material.SetColor("_BaseColor", Color.white);
        material.SetTexture("_BaseMap", GetGlowTexture());
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1.5f;
        renderer.velocityScale = 0.08f;
        renderer.sortingOrder = 2;
        ImpactEffect animation = effect.AddComponent<ImpactEffect>();
        animation.particles = system;
        animation.instanceMaterial = material;
        system.Play();
        system.Emit(count);
        // One stationary white core makes the initial contact easy to read.
        ParticleSystem.EmitParams flash = new ParticleSystem.EmitParams
        {
            position = effect.transform.position,
            velocity = Vector3.zero,
            startColor = Color.Lerp(color, Color.white, 0.85f),
            startSize = 0.9f * scale,
            startLifetime = 0.16f,
            applyShapeToPosition = false
        };
        system.Emit(flash, 1);
    }

    private static Texture2D GetGlowTexture()
    {
        if (glowTexture != null) return glowTexture;

        // A shared soft circle removes square particle edges without another art asset.
        const int size = 32;
        glowTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        glowTexture.name = "ImpactGlow";
        glowTexture.wrapMode = TextureWrapMode.Clamp;
        glowTexture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float radius = new Vector2((x + 0.5f) / size * 2 - 1,
                    (y + 0.5f) / size * 2 - 1).magnitude;
                float alpha = Mathf.Pow(Mathf.Clamp01(1 - radius), 2);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        glowTexture.SetPixels(pixels);
        glowTexture.Apply(false, true);
        return glowTexture;
    }

    private void Update()
    {
        // End screens stop game time; let the final burst finish, but freeze on Pause.
        ParticleSystem.MainModule main = particles.main;
        main.useUnscaledTime = GameOverController.IsGameOver;
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null) Destroy(instanceMaterial);
    }
}
