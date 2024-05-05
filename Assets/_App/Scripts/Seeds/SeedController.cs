using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SeedController : MonoBehaviour
{
    [SerializeField]  [ColorUsageAttribute(true, true)] private List<Color> startingColors;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private MeshRenderer nucleus;
    [Space]
    [Range(.075F, .15F)][SerializeField] private float minStartingScale;
    [Range(.1F, .3F)][SerializeField] private float maxStartingScale;

    [SerializeField] private float _moveSpeed = 1.3f;
    [SerializeField] private float _ascendSpeed = 1.3f;
    [SerializeField] private float _spinSpeed = 1.3f;

    [SerializeField] private List<GameObject> _deactivateOnFlung;

    [Space]
    public UnityEvent<SeedController> OnSeedFlung;
    public UnityEvent<SeedController, Vector3> OnSeedPoppedOnTheCeiling;
    public UnityEvent<SeedController, Vector3, Vector3> OnSeedPoppedOnAnIsland;

    public Color SeedColor
    {
        get => _seedMaterial.GetColor(BASE_COLOR_PROPERTY);
        private set
        {
            _seedMaterial.SetColor(BASE_COLOR_PROPERTY, value);  
            _seedMaterial.SetColor(MAIN_GLOW_PROPERTY, value);
        } 
    }
    
    public Guid Uuid
    {
        get
        {
            if (_uuid == Guid.Empty)
            {
                _uuid = Guid.NewGuid();
            }
            return _uuid;
        }
    }

  
    public Vector3 Size => transform.lossyScale;
    public bool IsAboutToBeAbsorbed { get; private set; }

    private const float SCALE_TRANSITION_SPEED = 4F;
    private const float COLOR_TRANSITION_SEED = 3F;
    
    private const string BASE_COLOR_PROPERTY = "_BaseColor";
    private const string MAIN_GLOW_PROPERTY = "_MainGlow";
    private const string DISSOLVE_LEVEL_PROPERTY = "_DissolveLevel";

    private Material _seedMaterial;
    private Vector3 _destinationScale;
    private Color _destinationColor;
    private float _colorTransitionTime;

    private Vector3 _targetDestination = Vector3.negativeInfinity;
    private bool _isAscending = false;

    private Guid _uuid = Guid.Empty;
    
    private void Awake()
    {
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (!meshRenderer)
        {
            throw new ApplicationException($"No MeshRenderer component found on {name}");
        }

        _seedMaterial = meshRenderer.material;

        ConfigureVariation();
        
        _destinationScale = transform.localScale;
        _destinationColor = SeedColor;
    }

    private void Update()
    {
        if (_destinationScale.magnitude - transform.localScale.magnitude > .01F)
            transform.localScale = Vector3.Lerp(transform.localScale, _destinationScale, SCALE_TRANSITION_SPEED);
        
        if (_colorTransitionTime < 1)
        {
            // Seed material color
            SeedColor = Color.Lerp(SeedColor, _destinationColor, _colorTransitionTime);
            
            // Trail color
            var trailColor = SeedColor;
            trailColor.a = trailRenderer.material.color.a;
            trailRenderer.material.color = trailColor;
            
            // Nucleus Color
            var nucleusColor = SeedColor;
            nucleusColor.a = nucleus.material.color.a;
            nucleus.material.color = SeedColor;
            
            // Particle color
            var main = trailParticles.main;
            var particleAlpha = main.startColor.colorMax.a;
            var particleColor = SeedColor;
            
            particleColor.a = particleAlpha;
            main.startColor = particleColor;
            
            _colorTransitionTime += Time.deltaTime / COLOR_TRANSITION_SEED;
        }

        if (_targetDestination.IsValid())
        {
            transform.position = Vector3.Lerp(transform.position, _targetDestination, _moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Ceiling"))
        {
            // Raycast from the seed center upwards 
            if (Physics.Raycast(transform.position, (_targetDestination - transform.position).normalized, out RaycastHit hit, Mathf.Infinity))
            {
                OnSeedPoppedOnTheCeiling?.Invoke(this, hit.point);
                Pop();
                Reset();
            }
            else
            {
                Debug.LogError($"[{nameof(SeedController)}] {nameof(OnTriggerEnter)}: FAILED TO FIND CEILING COLLISION POINT");
            }
        }
        else if (collider.CompareTag("Island"))
        {
            // Raycast from the seed center upwards 
            if (Physics.Raycast(transform.position, (_targetDestination - transform.position).normalized, out RaycastHit hit, Mathf.Infinity))
            {
                OnSeedPoppedOnAnIsland?.Invoke(this, hit.point, hit.normal);
                Pop();
                Reset();
            }
            else
            {
                Debug.LogError($"[{nameof(SeedController)}] {nameof(OnTriggerEnter)}: FAILED TO FIND ISLAND COLLISION POINT");
            }
        }
    }

    private void ConfigureVariation()
    {
        // Color
        var seedStartingColor = startingColors[Random.Range(0, startingColors.Count)];
        SeedColor = seedStartingColor;
        
        // Rotation
        transform.rotation = Quaternion.Euler(Random.Range(0F, 360F), Random.Range(0F, 360F), Random.Range(0F, 360F));
        
        // Scale
        if (maxStartingScale < minStartingScale)
        {
            throw new ApplicationException(
                "The maximum seed starting scale can't be less than the minimum starting scale. Aborting seed creation");
        }
        transform.localScale = Vector3.one * Random.Range(minStartingScale, maxStartingScale);
    }

    public void SetScale(Vector3 newScale)
    {
        _destinationScale = newScale;
        trailRenderer.startWidth = nucleus.transform.lossyScale.magnitude;
    }
    
    // TEJAS: Keep in mind changing the color will no longer make the material be GPU Instanced
    public void SetColor(Color newColor)
    {
        _destinationColor = newColor;
        _colorTransitionTime = 0;
    }

    /*private void LerpToDestinationColor(float time)
    {
        SeedColor = Color.Lerp(SeedColor, _destinationColor, time);
    }*/
    
    // TEJAS: May refactor this method later to avoid race-conditions or move method to a more appropriate place
    // Used to prevent race-conditions when two blobs are trying to absorb each other in the same frame
    public void LockForAbsorption()
    {
        IsAboutToBeAbsorbed = true;
    }

    /// <summary>
    /// Called by Seed's game object <see cref="FlingDetector"/> component (see inspector).
    /// </summary>
    public void FlungTowardsCeiling()
    {
        // TODO(anyone): Trigger flung VFX.
        // https://github.com/tejasXR/meta-presence-hackathon-2024/issues/28
        OnSeedFlung?.Invoke(this);
    }

    public void SetTargetDestination(Vector3 targetDestination)
    {
        _targetDestination = targetDestination;

        bool isValid = _targetDestination.IsValid();
        foreach (GameObject go in _deactivateOnFlung)
        {
            go.SetActive(!isValid);
        }
    }

    private void Pop()
    {
        // Popping simply deactivates the seed for now.
        // TODO(anyone): Trigger pop VFX.
        // https://github.com/tejasXR/meta-presence-hackathon-2024/issues/28
        gameObject.SetActive(false);
    }

    private void Reset() => SetTargetDestination(Vector3.negativeInfinity);

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_targetDestination.IsValid())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _targetDestination);
        }
    }
#endif
}

/*[Serializable]
public struct SeedColorData
{
    public Color baseColor;
    public Color mainGlowColor;
    [ColorUsageAttribute(true, true)] public Color edgeGlowColor;
}*/
