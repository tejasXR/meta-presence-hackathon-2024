using System.Linq;
using UnityEngine;

public class PlantCreator : MonoBehaviour
{
    [SerializeField] private GameObject centerSeed;

    private const float SeedRotationSpeed = 75F;
    
    private SeedSnapDelegate[] _snapDelegates;
    private bool _allSectionsSnapped;
    
    private void Awake()
    {
        _snapDelegates = GetComponentsInChildren<SeedSnapDelegate>();

        foreach (var snapDelegate in _snapDelegates)
        {
            snapDelegate.Snapped += OnSectionSnapped;
            snapDelegate.Unsnapped += OnSectionUnsnapped;
        }
    }

    private void OnDestroy()
    {
        foreach (var snapDelegate in _snapDelegates)
        {
            snapDelegate.Snapped -= OnSectionSnapped;
            snapDelegate.Unsnapped -= OnSectionUnsnapped;
        }
    }

    private void Update()
    {
        if (_allSectionsSnapped)
        {
            centerSeed.transform.Rotate(Vector3.up, Time.deltaTime * SeedRotationSpeed);
            centerSeed.transform.Rotate(Vector3.right, Time.deltaTime * SeedRotationSpeed);
        }
    }

    private void OnSectionSnapped()
    {
        CheckIfAllSectionsAreSnapped();
    }

    private void OnSectionUnsnapped()
    {
        CheckIfAllSectionsAreSnapped();
    }

    private void CheckIfAllSectionsAreSnapped()
    {
        _allSectionsSnapped = _snapDelegates.All(sD => sD.IsSnapped);
    }
}
