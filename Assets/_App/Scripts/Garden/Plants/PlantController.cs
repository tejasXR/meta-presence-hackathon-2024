using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantController : MonoBehaviour
{
    [SerializeField] private Plants.PlantType _type;
    [SerializeField] private float _lifeSpan = 5f;
    [Range(0f, 1f)] [SerializeField] private float _minGrowth = 0f;
    [Range(0f, 1f)] [SerializeField] private float _maxGrowth = 1f;
    [Space] 
 
    [SerializeField] private ParticleSystem _plantFullyGrownParticles;
    [Range(0F, 2F)] [SerializeField] private float plantChargeSpeed = .3F;
    [SerializeField] private float _seedBloomEmissionTransitionSpeed = 1.5F;
    [Header("LootConfig")]
    public int MinLoot = 1;
    public Transform LootSpawnPointsRoot;

    [Space]
    public UnityEvent<PlantController> SeedSpawningTriggered;

    public bool IsPlantBaseFullyGrown => Mathf.Abs(_maxGrowth - _basePlantGrowth) < 0.001f;
    public Plants.PlantType Type => _type;

    private readonly List<Material> _materials = new();

    private const float MAX_PLANT_CHARGE = 1F; 
    private const float SEED_BLOOM_EMISSIVE_ADDITION = 1.5F; 
    private const string EMISSIVE_STRENGTH_PROPERTY = "_Emissive_Strength";
    private const string GROWTH_PROPERTY = "_Growth";

    private Coroutine _growthCoroutine;
    private bool _isPlantCharging;
    private float _currentPlantCharge;
    private float _basePlantGrowth;
    private float _seedBloomPlantGrowth;
    
    private Material _basePlantMaterial;
    private Material _plantBloomMaterial;
    
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
            _seedBloomPlantGrowth = Mathf.Clamp01(value);
            _plantBloomMaterial.SetFloat(GROWTH_PROPERTY, _seedBloomPlantGrowth);
        }
    }

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

        StartGrowthCoroutine(ref _growthCoroutine);
    }

    public IEnumerator ChargeUpPlantForSeedSpawn()
    {
        if (!IsPlantBaseFullyGrown)
            yield break;

        _isPlantCharging = true;

        while (_currentPlantCharge < MAX_PLANT_CHARGE)
        {
            _currentPlantCharge += plantChargeSpeed * Time.deltaTime;
            SeedBloomPlantGrowth = _currentPlantCharge;
            
            if (_currentPlantCharge > MAX_PLANT_CHARGE)
            {
                StartCoroutine(TriggerSeedSpawning());
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator CancelSeedSpawnCharging()
    {
        while (!_isPlantCharging && _currentPlantCharge != 0)
        {
            _currentPlantCharge -= plantChargeSpeed;
            yield return new WaitForEndOfFrame();
        }

        _currentPlantCharge = 0;
    }

    private IEnumerator TriggerSeedSpawning()
    {
        var seedBloomMaterialEmission = _plantBloomMaterial.GetFloat(EMISSIVE_STRENGTH_PROPERTY);
        var seedBloomDestinationEmission = seedBloomMaterialEmission + SEED_BLOOM_EMISSIVE_ADDITION;
        while (seedBloomMaterialEmission < seedBloomDestinationEmission)
        {
            seedBloomMaterialEmission += Time.deltaTime * _seedBloomEmissionTransitionSpeed;
            _plantBloomMaterial.SetFloat(EMISSIVE_STRENGTH_PROPERTY, seedBloomMaterialEmission);
            yield return new WaitForEndOfFrame();
        }
        
        SeedSpawningTriggered?.Invoke(this);
        _plantFullyGrownParticles.Stop();
        
        // TEJAS: As a placeholder function, we deactivate this object
        gameObject.SetActive(false);
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
        
        if (!_plantFullyGrownParticles.isPlaying)
            _plantFullyGrownParticles.Play();
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button("Set Random Growth For Plant")]
    public void RandomGrowthButton()
    {
        ResumeGrowing(UnityEngine.Random.Range(_minGrowth, _maxGrowth), null);
    }

    [Sirenix.OdinInspector.LabelText("Time In Seconds")]
    public int TimeInSeconds = 120;

    [Sirenix.OdinInspector.Button("Simulate Time Passed")]
    public void SimulateTimePassedButton()
    {
        ResumeGrowing(BasePlantGrowth, TimeSpan.FromSeconds(TimeInSeconds));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Transform spawnPoint in LootSpawnPointsRoot)
        {
            Gizmos.DrawSphere(spawnPoint.position, 0.05f);
        }
    }
#endif
}
