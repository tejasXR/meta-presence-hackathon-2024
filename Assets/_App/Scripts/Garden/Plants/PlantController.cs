using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    [SerializeField] private Plants.PlantType _type;
    [SerializeField] private float _lifeSpan = 5f;
    [SerializeField] private float _growthRate = 0.05f;
    [SerializeField] private List<MeshRenderer> _meshRenderers;

    [Range(0f, 1f)]
    [SerializeField] private float _minGrow = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float _maxGrow = 0.9f;

    public Plants.PlantType Type => _type;
    public float GrowValue => _materials.Count > 0 ? _materials[0].GetFloat(GROW_PROPERTY) : _minGrow;

    private readonly List<Material> _materials = new();
    private bool _isFullyGrown;
    private DateTime? _creationDate; // TEJAS: We may consolidate this property elsewhere as we refactor/clean the project!
    private List<Coroutine> _growMaterialRoutines;

    private const string GROW_PROPERTY = "_Grow";
    private const int MAX_LIFE_SPAN_DAYS = 2;

    [Button("Set Random Growth For Plant")]
    public void RandomGrowthButton()
    {
        ResumeGrowing(Random.Range(_minGrow, _maxGrow));
    }

    void Start()
    {
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            foreach (Material material in meshRenderer.materials)
            {
                if (material.HasProperty(GROW_PROPERTY))
                {
                    material.SetFloat(GROW_PROPERTY, _minGrow);
                    _materials.Add(material);
                }
            }
        }
    }

    public void StartGrowing() => ResumeGrowing(_minGrow);

    public void ResumeGrowing(float growthValue)
    {
        foreach (Material material in _materials)
        {
            var growthRoutine = StartCoroutine(SetMaterialGrowth(material, growthValue));
            _growMaterialRoutines.Add(growthRoutine);
        }
    }

    private IEnumerator SetMaterialGrowth(Material material, float newGrowthValue)
    {
        var currentGrowthValue = material.GetFloat(GROW_PROPERTY);

        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(SetMaterialGrowth)}: value = {newGrowthValue}");

        int growthDirection = currentGrowthValue < newGrowthValue ? 1 : -1;
        var growthIncrement = 1 / (_lifeSpan / _growthRate) * growthDirection;

        while (currentGrowthValue < newGrowthValue)
        {
            if (currentGrowthValue > _maxGrow)
                yield break; 
                    
            currentGrowthValue = Mathf.Clamp(currentGrowthValue + growthIncrement, _minGrow, _maxGrow);
            material.SetFloat(GROW_PROPERTY, currentGrowthValue);

            yield return new WaitForSeconds(_growthRate);
        }

        _isFullyGrown = Math.Abs(currentGrowthValue - _maxGrow) < .001F;
        
        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(SetMaterialGrowth)}: reached maximum growth.");
    }

    // TEJAS: Because we can now set a specific growth value, a dedicated Shrink method isn't needed
    /*private IEnumerator Shrink(Material material)
    {
        _isFullyGrown = false;

        float growValue = material.GetFloat(GROW_PROPERTY);
        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: value={growValue}");

        while (growValue > _minGrow)
        {
            growValue -= 1 / (_lifeSpan / _growthRate);
            material.SetFloat(GROW_PROPERTY, growValue);
            // Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: value={growValue}");

            yield return new WaitForSeconds(_growthRate);
        }

        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: reached minimum growth.");
    }*/

    // TEJAS: We may consolidate this method elsewhere more relevant! 
    public void SetCreationDate(DateTime creationDate)
    {
        _creationDate = creationDate;
    }

    public void GrowBasedOnPassedTime()
    {
        StopGrowthCoroutines();
        
        if (!_creationDate.HasValue)
        {
            Debug.LogError("Our plant doesn't have a creation date, so we can't grow based on real time");
            return;
        }
        
        var timeSinceCreation = DateTime.Now - _creationDate;
        var growthTarget = (_maxGrow - _minGrow) * (float)(timeSinceCreation / TimeSpan.FromDays(MAX_LIFE_SPAN_DAYS));
        var clampedTarget = Mathf.Clamp(growthTarget, _minGrow, _maxGrow);
        ResumeGrowing(clampedTarget); 
    }

    private void StopGrowthCoroutines()
    {
        // Helpful when we need to force-set a growth value on start
        _growMaterialRoutines.ForEach(StopCoroutine);
        _growMaterialRoutines.Clear();
    }
}
