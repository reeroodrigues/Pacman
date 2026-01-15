using UnityEngine;

public class MenuVFXController : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem floatingPellets;
    [SerializeField] private ParticleSystem logoGlow;
    [SerializeField] private ParticleSystem ambientParticles;
    
    [Header("Materials")]
    [SerializeField] private Material pelletMaterial;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Material ambientMaterial;

    private void Start()
    {
        SetupFloatingPellets();
        SetupLogoGlow();
        SetupAmbientParticles();
    }

    private void SetupFloatingPellets()
    {
        if (floatingPellets == null) return;

        var main = floatingPellets.main;
        main.startLifetime = 8f;
        main.startSpeed = 50f;
        main.startSize = 30f;
        main.startColor = new Color(1f, 0.92f, 0.016f, 1f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = floatingPellets.emission;
        emission.rateOverTime = 5f;

        var shape = floatingPellets.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(1920f, 1080f, 1f);

        var velocityOverLifetime = floatingPellets.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-20f, 20f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-20f, 20f);

        var colorOverLifetime = floatingPellets.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.2f),
                new GradientAlphaKey(1.0f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = floatingPellets.GetComponent<ParticleSystemRenderer>();
        if (renderer != null && pelletMaterial != null)
        {
            renderer.material = pelletMaterial;
            renderer.sortingLayerName = "FX";
            renderer.sortingOrder = 1;
        }
    }

    private void SetupLogoGlow()
    {
        if (logoGlow == null) return;

        var main = logoGlow.main;
        main.startLifetime = 3f;
        main.startSpeed = 30f;
        main.startSize = new ParticleSystem.MinMaxCurve(40f, 80f);
        main.startColor = new Color(1f, 0.9f, 0.3f, 0.6f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = logoGlow.emission;
        emission.rateOverTime = 20f;

        var shape = logoGlow.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 400f;

        var velocityOverLifetime = logoGlow.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(50f);

        var colorOverLifetime = logoGlow.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0.0f), 
                new GradientColorKey(new Color(1f, 0.7f, 0f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = logoGlow.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.2f);
        curve.AddKey(0.5f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = logoGlow.GetComponent<ParticleSystemRenderer>();
        if (renderer != null && glowMaterial != null)
        {
            renderer.material = glowMaterial;
            renderer.sortingLayerName = "FX";
            renderer.sortingOrder = 0;
        }
    }

    private void SetupAmbientParticles()
    {
        if (ambientParticles == null) return;

        var main = ambientParticles.main;
        main.startLifetime = 10f;
        main.startSpeed = 30f;
        main.startSize = new ParticleSystem.MinMaxCurve(10f, 25f);
        main.startColor = new Color(0.3f, 0.8f, 1f, 0.3f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ambientParticles.emission;
        emission.rateOverTime = 8f;

        var shape = ambientParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(2200f, 1200f, 1f);

        var velocityOverLifetime = ambientParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(15f, 30f);

        var colorOverLifetime = ambientParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0.0f), 
                new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.3f, 0.2f),
                new GradientAlphaKey(0.3f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = ambientParticles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null && ambientMaterial != null)
        {
            renderer.material = ambientMaterial;
            renderer.sortingLayerName = "Background";
            renderer.sortingOrder = 10;
        }
    }
}
