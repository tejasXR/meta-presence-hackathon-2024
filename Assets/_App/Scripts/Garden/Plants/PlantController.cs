using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PlantController : MonoBehaviour
{
    [SerializeField] private Plants.PlantType _type;
    [SerializeField] private float _lifeSpan = 5f;
    [SerializeField] private List<MeshRenderer> _meshRenderers;
    
    [Range(0f, 1f)] [SerializeField] private float _minGrowth = 0f;
    [Range(0f, 1f)] [SerializeField] private float _maxGrowth = 1f;
    [SerializeField] private float _growth;
    [Space] 
    [Range(0f, 1f)] [SerializeField] private float _additionalEmissiveStrength = .5F;
    
    [Header("LootConfig")]
    public int MinLoot = 1;
    public Transform LootSpawnPointsRoot;

    [Space]
    public UnityEvent<PlantController> SeedSpawningTriggered;

    public bool IsFullyGrown => Mathf.Abs(_maxGrowth - _growth) < 0.001f;
    public Plants.PlantType Type => _type;

    private readonly List<Material> _materials = new();

    private const float MAX_SEED_SPAWN_CHARGE = 1F; 
    private const string EMISSIVE_STRENGTH_PROPERTY = "_Emissive_Strength";
    private const string GROWTH_PROPERTY = "_Growth";

    private Coroutine _growthCoroutine;
    private float _seedSpawnCharge;
    private Dictionary<Material, float> _originalMaterialEmission = new();

    public float Growth
    {
        get => _growth;
        set
        {
            
            // Debug.Log($"[{nameof(PlantController)}] {nameof(Growth)}: {nameof(value)}={value}");
            _growth = value;

            foreach (Material material in _materials)
            {
                material.SetFloat(GROWTH_PROPERTY, _growth);
            }
        }
    }

    void Awake()
    {
        InitializeMaterials();

        foreach (var material in _materials)
        {
            var currentEmission = material.GetFloat(EMISSIVE_STRENGTH_PROPERTY);
            _originalMaterialEmission.Add(material, currentEmission);
        }
        
        Growth = _minGrowth;
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

        Growth = Math.Min(lastRecordedGrowthValue + growthDifference, _maxGrowth);
        Debug.Log($"[{nameof(PlantController)}] {nameof(ResumeGrowing)}: {nameof(Growth)}={Growth}");

        StartGrowthCoroutine(ref _growthCoroutine);
    }

    public void ChargeUpSeedSpawn(float incrementalChargeValue)
    {
        if (!IsFullyGrown)
            return;
        
        _seedSpawnCharge += incrementalChargeValue;

        AddToOriginalEmissiveStrength(_additionalEmissiveStrength);
        
        if (_seedSpawnCharge > MAX_SEED_SPAWN_CHARGE)
        {
            TriggerSeedSpawning();
            CancelSeedSpawnCharging();
        }
    }

    public void CancelSeedSpawnCharging()
    {
        _seedSpawnCharge = 0;
        ResetEmissiveStrengthToOriginal();
    }

    private void TriggerSeedSpawning()
    {
        // TEJAS: As a placeholder visual, we reset the growth of this plant
        StartGrowing();
        SeedSpawningTriggered?.Invoke(this);
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
        while (Growth < _maxGrowth)
        {
            Growth += 1 / (_lifeSpan / GameManager.Instance.PlantsGrowthFrequency) * GameManager.Instance.PlantsGrowthSpeed;
            yield return new WaitForSeconds(GameManager.Instance.PlantsGrowthFrequency);
        }

        Growth = _maxGrowth;
    }

    private void InitializeMaterials()
    {
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            foreach (Material material in meshRenderer.materials)
            {
                if (material.HasProperty(GROWTH_PROPERTY) && material.HasProperty(EMISSIVE_STRENGTH_PROPERTY))
                {
                    _materials.Add(material);
                }
            }
        }
    }

    private void AddToOriginalEmissiveStrength(float emissionValue)
    {
        foreach (Material material in _materials)
        {
            _originalMaterialEmission.TryGetValue(material, out var originalEmissiveStrength);
            material.SetFloat(EMISSIVE_STRENGTH_PROPERTY,  originalEmissiveStrength + emissionValue);
        }
    }

    private void ResetEmissiveStrengthToOriginal()
    {
        foreach (Material material in _materials)
        {
            _originalMaterialEmission.TryGetValue(material, out var originalEmissiveStrength);
            material.SetFloat(EMISSIVE_STRENGTH_PROPERTY, originalEmissiveStrength);
        }
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
        ResumeGrowing(Growth, TimeSpan.FromSeconds(TimeInSeconds));
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
