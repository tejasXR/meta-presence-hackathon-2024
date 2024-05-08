using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class PlantController : MonoBehaviour
{
    [SerializeField] private Plants.PlantType _type;
    [SerializeField] private float _lifeSpan = 5f;
    [Range(0f, 1f)] [SerializeField] private float _minGrowth = 0f;
    [Range(0f, 1f)] [SerializeField] private float _maxGrowth = 1f;
    [Range(0f, 1f)] [SerializeField] private float _plantHarvestCooldownSeconds = 60f;
    [Space] 
 
    [SerializeField] private PlantReadyVfx _plantReadyVfx;
    [Range(0F, 2F)] [SerializeField] private float plantChargeSpeed = .1F;
    [Range(0F, 2F)] [SerializeField] private float plantCancelChargeSpeed = .5F;
    [SerializeField] private float _seedBloomEmissionTransitionSpeed = 1.5F;

    [Header("DullVfx")]
    [SerializeField] private float _dullTransitionDuration = 5f;
    [SerializeField] private float _dullEmissiveStrength = 0f;
    [SerializeField] private Color _dullColor;
    [Header("LootConfig")]
    public int MinLoot = 1;
    public Transform LootSpawnPointsRoot;

    [Space]
    public UnityEvent PlantReadyToBeHarvested;
    public UnityEvent PlantChargingUp;
    public UnityEvent PlantChargingDown;
    public UnityEvent<PlantController> SeedSpawningTriggered;
    public UnityEvent PlantGlowingBeforeSeedSpawn;

    public bool IsPlantBaseFullyGrown => Mathf.Abs(_maxGrowth - _basePlantGrowth) < 0.001f;
    public Plants.PlantType Type => _type;
    
    private const float MAX_PLANT_BLOOM_GROWTH = 1F; 
    private const float MAX_PLANT_CHARGE = 1F; 
    private const float SEED_BLOOM_EMISSIVE_ADDITION = 7F; 
    private const string EMISSIVE_STRENGTH_PROPERTY = "_Emissive_Strength";
    private const string GROWTH_PROPERTY = "_Growth";
    private const string COLOR_PROPERTY = "_Color";

    private Coroutine _growthCoroutine;
    private Coroutine _dullTransitionCoroutine;
    private Coroutine _emissiveStrengthCoroutine;
    private Coroutine _colorCoroutine;
    private float _currentPlantCharge;
    private float _basePlantGrowth;
    private float _seedBloomPlantGrowth;
    private float _basePlantBloomGrowth;
    
    private Material _basePlantMaterial;
    private Material _plantBloomMaterial;

    private bool _harvestCooldownActive;
    private bool _dullTransitionActive;

    public float BasePlantGrowth
    {
        get => _basePlantGrowth;
        set
        {
            _basePlantGrowth = Mathf.Clamp(value, _minGrowth, _maxGrowth);
            _basePlantMaterial.SetFloat(GROWTH_PROPERTY, _basePlantGrowth);
        }
    }

    public float SeedBloomPlantGrowth
    {
        get => _seedBloomPlantGrowth;
        set
        {
            _seedBloomPlantGrowth = Mathf.Clamp(value, _basePlantBloomGrowth, MAX_PLANT_BLOOM_GROWTH);
            _plantBloomMaterial.SetFloat(GROWTH_PROPERTY, _seedBloomPlantGrowth);
        }
    }
    
    public Guid SpatialAnchorUuid { get; private set; }

    private void Awake()
    {
        var meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 1)
        {
            Debug.LogError(
                $"More than one mesh renderer on plant {gameObject.name}." +
                $" Due to how our materials work, we can only have 1 mesh with two materials");
        }
        else if (meshRenderers.Length == 0)
        {
            Debug.LogError($"Didn't find any mesh renderers on the plant {gameObject.name}");
        }

        _basePlantMaterial = meshRenderers[0].materials[0];
        _plantBloomMaterial = meshRenderers[0].materials[1];
        _basePlantBloomGrowth = _plantBloomMaterial.GetFloat(GROWTH_PROPERTY);
        
        BasePlantGrowth = _minGrowth;
    }

    public void StartGrowing() => ResumeGrowing(_minGrowth, null);

    public void ResumeGrowing(float lastRecordedGrowthValue, TimeSpan? lastRecordedGrowthTimespan)
    {
        Debug.Log($"[{nameof(PlantController)}] {nameof(ResumeGrowing)}: {nameof(lastRecordedGrowthValue)}={lastRecordedGrowthValue}, {nameof(lastRecordedGrowthTimespan)}={lastRecordedGrowthTimespan?.ToString(@"d\.hh\:mm\:ss") ?? "N/A"}");

        float growthDifference = 0f;
        if (lastRecordedGrowthTimespan.HasValue)
        {
            growthDifference = 1f / (_lifeSpan / (float)lastRecordedGrowthTimespan.Value.TotalSeconds) * GameManager.Instance.AwayPlantsGrowthSpeed;

            Debug.Log($"[{nameof(PlantController)}] {nameof(ResumeGrowing)}: Plant grew {growthDifference} in the last {lastRecordedGrowthTimespan.Value:d\\.hh\\:mm\\:ss}");
        }

        BasePlantGrowth = Math.Min(lastRecordedGrowthValue + growthDifference, _maxGrowth);
        Debug.Log($"[{nameof(PlantController)}] {nameof(ResumeGrowing)}: {nameof(BasePlantGrowth)}={BasePlantGrowth}");

        if (lastRecordedGrowthTimespan.HasValue && (float)lastRecordedGrowthTimespan.Value.TotalSeconds >= GameManager.Instance.AwaySecondsToDull)
        {
            Debug.Log($"[{nameof(PlantController)}] {nameof(ResumeGrowing)}: Plant has become dull!");
            StartDullTransitionCoroutine(ref _dullTransitionCoroutine);
        }
        else
        {
            StartGrowthCoroutine(ref _growthCoroutine);
        }
    }

    public IEnumerator ChargeUpPlantForSeedSpawn()
    {
        if (!IsPlantBaseFullyGrown)
            yield break;

        if (_harvestCooldownActive || _dullTransitionActive)
            yield break;
        
        PlantChargingUp?.Invoke();
        
        while (_currentPlantCharge < MAX_PLANT_CHARGE)
        {
            _currentPlantCharge += plantChargeSpeed * Time.deltaTime;
            SeedBloomPlantGrowth = _basePlantBloomGrowth + _currentPlantCharge * (MAX_PLANT_BLOOM_GROWTH - _basePlantBloomGrowth);
            yield return new WaitForEndOfFrame();
        }
        
        StartCoroutine(TriggerSeedSpawning());
        _currentPlantCharge = MAX_PLANT_CHARGE;
    }

    public IEnumerator CancelSeedSpawnCharging()
    {
        if (Math.Abs(_currentPlantCharge - MAX_PLANT_CHARGE) < .001F)
            yield break;
        
        PlantChargingDown?.Invoke();
        
        while (_currentPlantCharge > 0)
        {
            _currentPlantCharge -= plantCancelChargeSpeed * Time.deltaTime;
            SeedBloomPlantGrowth = _basePlantBloomGrowth + _currentPlantCharge * (MAX_PLANT_BLOOM_GROWTH - _basePlantBloomGrowth);
            yield return new WaitForEndOfFrame();
        }

        _currentPlantCharge = 0;
    }

    public void SetSpatialAnchorId(Guid uuid)
    {
        SpatialAnchorUuid = uuid;
    }

    private IEnumerator TriggerSeedSpawning()
    {
        PlantGlowingBeforeSeedSpawn?.Invoke();
        
        var seedBloomMaterialEmission = _plantBloomMaterial.GetFloat(EMISSIVE_STRENGTH_PROPERTY);
        var seedBloomDestinationEmission = seedBloomMaterialEmission + SEED_BLOOM_EMISSIVE_ADDITION;
        while (seedBloomMaterialEmission < seedBloomDestinationEmission)
        {
            seedBloomMaterialEmission += _seedBloomEmissionTransitionSpeed * Time.deltaTime;
            _plantBloomMaterial.SetFloat(EMISSIVE_STRENGTH_PROPERTY, seedBloomMaterialEmission);
            yield return new WaitForEndOfFrame();
        }
        
        SeedSpawningTriggered?.Invoke(this);

        SeedBloomPlantGrowth = _basePlantBloomGrowth;

        StartCoroutine(TriggerPlantHarvestCooldown());
    }

    private IEnumerator TriggerPlantHarvestCooldown()
    {
        _harvestCooldownActive = true;
        var coolDownTimer = _plantHarvestCooldownSeconds;
        while (coolDownTimer > 0)
        {
            coolDownTimer -= Time.deltaTime;
            yield return null;
        }

        _harvestCooldownActive = false;
        StartGrowing();
    }

    private void StartGrowthCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(Grow());
    }

    private IEnumerator Grow()
    {
        while (BasePlantGrowth < _maxGrowth)
        {
            BasePlantGrowth += 1 / (_lifeSpan / GameManager.Instance.PlantsGrowthFrequency) * GameManager.Instance.PlantsGrowthSpeed;
            yield return new WaitForSeconds(GameManager.Instance.PlantsGrowthFrequency);
        }

        // Fully grown
        BasePlantGrowth = _maxGrowth;
        PlantReadyToBeHarvested?.Invoke();
    }

    private void StartDullTransitionCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(StartDullTransition());
    }

    private IEnumerator StartDullTransition()
    {
        _dullTransitionActive = true;

        float defaultEmissiveStrength = _basePlantMaterial.GetFloat(EMISSIVE_STRENGTH_PROPERTY);
        Color defaultColor = _basePlantMaterial.GetColor(COLOR_PROPERTY);

        // Set dull look.
        _basePlantMaterial.SetFloat(EMISSIVE_STRENGTH_PROPERTY, _dullEmissiveStrength);
        _basePlantMaterial.SetColor(COLOR_PROPERTY, _dullColor);

        // Start transitioning to default look. 
        StartEmissiveStrengthInterpolation(
                    ref _emissiveStrengthCoroutine, _dullEmissiveStrength, defaultEmissiveStrength, _dullTransitionDuration,
                    emissiveStrength => _basePlantMaterial.SetFloat(EMISSIVE_STRENGTH_PROPERTY, emissiveStrength));

        StartColorInterpolation(
                    ref _colorCoroutine, _dullColor, defaultColor, _dullTransitionDuration,
                    color => _basePlantMaterial.SetColor(COLOR_PROPERTY, color));

        yield return new WaitForSeconds(_dullTransitionDuration);

        _dullTransitionActive = false;

        StartGrowthCoroutine(ref _growthCoroutine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartColorInterpolation(ref Coroutine coroutine, Color startColor, Color targetColor, float duration, Action<Color> setColorAction)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(InterpolationUtils.InterpolateColor(startColor, targetColor, duration, setColorAction));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartEmissiveStrengthInterpolation(ref Coroutine coroutine, float startValue, float targetValue, float duration, Action<float> setValueAction)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(InterpolationUtils.InterpolateValue(startValue, targetValue, duration, setValueAction));
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button("Start Growing Plant")]
    public void StartGrowingButton()
    {
        StartGrowing();
    }

    [Sirenix.OdinInspector.Button("Start Plant Dull Transition")]
    public void StartDullTransitionButton()
    {
        StartDullTransitionCoroutine(ref _dullTransitionCoroutine);
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     foreach (Transform spawnPoint in LootSpawnPointsRoot)
    //     {
    //         Gizmos.DrawSphere(spawnPoint.position, 0.05f);
    //     }
    // }
#endif
}
