using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> _meshRenderers;
    [SerializeField] private float _lifeSpan = 5f;
    [SerializeField] private float _growthRate = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float _minGrow = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float _maxGrow = 0.9f;

    private readonly List<Material> _materials = new();
    private bool _isFullyGrown;

    private const string GROW_PROPERTY = "_Grow";

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

        foreach (Material material in _materials)
        {
            StartCoroutine(Grow(material));
        }
    }

    private IEnumerator Grow(Material material)
    {
        float growValue = material.GetFloat(GROW_PROPERTY);
        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Grow)}: value={growValue}");

        while (growValue < _maxGrow)
        {
            growValue += 1 / (_lifeSpan / _growthRate);
            material.SetFloat(GROW_PROPERTY, growValue);
            Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Grow)}: value={growValue}");

            yield return new WaitForSeconds(_growthRate);
        }

        _isFullyGrown = true;
        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Grow)}: reached maximum growth.");
    }

    private IEnumerator Shrink(Material material)
    {
        _isFullyGrown = false;

        float growValue = material.GetFloat(GROW_PROPERTY);
        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: value={growValue}");

        while (growValue > _minGrow)
        {
            growValue -= 1 / (_lifeSpan / _growthRate);
            material.SetFloat(GROW_PROPERTY, growValue);
            Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: value={growValue}");

            yield return new WaitForSeconds(_growthRate);
        }

        Debug.Log($"({gameObject.name})[{nameof(PlantController)}] {nameof(Shrink)}: reached minimum growth.");
    }
}
