using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SeedController : MonoBehaviour
{
    [SerializeField] private List<Color> startingColors;
    [SerializeField] private List<Texture2D> startingPatterns;
    [Space]
    [Range(.075F, .15F)][SerializeField] private float minStartingScale;
    [Range(.1F, .3F)][SerializeField] private float maxStartingScale;
    [SerializeField] private float _moveSpeed = 1.3f;

    [SerializeField] private List<GameObject> _deactivateOnFlung;

    [Space]
    public UnityEvent<SeedController> OnSeedFlung;
    public UnityEvent<SeedController> OnSeedPopped;

    public Color MaterialColor
    {
        get
        {
            // TEJAS: Dupe code from Awake, okay for now
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (!_meshRenderer)
            {
                throw new ApplicationException($"No MeshRenderer component found on {name}");
            }

            return _meshRenderer.material.color;
        }
        private set => value = MaterialColor;
    }

  
    public Vector3 Size => _destinationScale;
    public bool IsAboutToBeAbsorbed { get; private set; }

    private const float ScaleTransitionSpeed = 4F;
    private const float ColorTransitionSpeed = 3F;

    private MeshRenderer _meshRenderer;
    private Vector3 _destinationScale;
    private Color _destinationColor;
    private float _colorTransitionTime;

    private Vector3 _target = Vector3.negativeInfinity;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (!_meshRenderer)
        {
            throw new ApplicationException($"No MeshRenderer component found on {name}");
        }

        ConfigureVariation();
        
        MaterialColor = _meshRenderer.material.color;
        _destinationScale = transform.localScale;
        _destinationColor = MaterialColor;
    }

    private void Update()
    {
        if (_destinationScale.magnitude - transform.localScale.magnitude > .01F)
            transform.localScale = Vector3.Lerp(transform.localScale, _destinationScale, ScaleTransitionSpeed);
        
        if (_colorTransitionTime < 1)
        {
            _meshRenderer.material.color = Color.Lerp(MaterialColor, _destinationColor, _colorTransitionTime);
            _colorTransitionTime += Time.deltaTime / ColorTransitionSpeed;
        }

        if (_target.IsValid())
        {
            transform.position = Vector3.Slerp(transform.position, _target, _moveSpeed * Time.deltaTime);
        }
    }
    
    private void ConfigureVariation()
    {
        // Color
        var randomColor = startingColors[Random.Range(0, startingColors.Count)];
        _meshRenderer.material.color = randomColor;
        
        // Pattern
        var randomPattern = startingPatterns[Random.Range(0, startingPatterns.Count)];
        _meshRenderer.material.mainTexture = randomPattern;
        
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

    public void ChangeScale(Vector3 newScale)
    {
        _destinationScale = newScale;
    }
    
    // TEJAS: Keep in mind changing the color will no longer make the material be GPU Instanced
    public void ChangeColor(Color newColor)
    {
        _destinationColor = newColor;
        _colorTransitionTime = 0;
    }
    
    // TEJAS: May refactor this method later to avoid race-conditions or move method to a more appropriate place
    // Used to prevent race-conditions when two blobs are trying to absorb each other in the same frame
    public void LockForAbsorption()
    {
        IsAboutToBeAbsorbed = true;
    }

    public void SeedFlung()
    {
        OnSeedFlung?.Invoke(this);
    }

    public void SetTarget(Vector3 target)
    {
        _target = target;
        bool isValid = _target.IsValid();
        foreach (GameObject go in _deactivateOnFlung)
        {
            go.SetActive(!isValid);
        }
    }

    public void ResetTarget() => SetTarget(Vector3.negativeInfinity);

    public void Pop()
    {
        Debug.Log($"({gameObject.name})[{nameof(SeedController)}] {nameof(Pop)}");

        // Popping simply deactivates the seed for now.
        gameObject.SetActive(false);
        OnSeedPopped?.Invoke(this);
    }

   
}
