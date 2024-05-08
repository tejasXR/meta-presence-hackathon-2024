using System;
using UnityEngine;

/// <summary>
/// Objects of this class will morph into other Blob objects
/// </summary>
[RequireComponent(typeof(SeedController))]
public class SeedMorph : MonoBehaviour
{
    public event Action AbsorbedSeed;
    
    private SeedController _seedController;
    private Material _material;
    private float _colorTransitionTime;

    private enum SeedSizeComparisonEnum
    {
        Smaller,
        SameSize,
        Larger
    }

    private void Awake()
    {
        _seedController = GetComponent<SeedController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactingSeed = other.GetComponent<SeedController>();
        if (interactingSeed) 
            CombineBlobs(interactingSeed);
    }

    private SeedSizeComparisonEnum CompareSize(SeedController seed)
    {
        if (_seedController.Size.magnitude > seed.Size.magnitude)
            return SeedSizeComparisonEnum.Larger;

        if (Math.Abs(_seedController.Size.magnitude - seed.Size.magnitude) < .001F)
            return SeedSizeComparisonEnum.SameSize;

        return SeedSizeComparisonEnum.Smaller;
    }

    private void CombineBlobs(SeedController interactingSeed)
    {
        var sizeOfInteractingBlob = CompareSize(interactingSeed);
        
        if (sizeOfInteractingBlob == SeedSizeComparisonEnum.Larger)
            return;
        
        AbsorbSeed(interactingSeed);
    }

    private void AbsorbSeed(SeedController seedToAbsorb)
    {
        while (_seedController.IsAboutToBeAbsorbed == false)
        {
            Debug.Log($"{name} is absorbing {seedToAbsorb.name}");
            
            seedToAbsorb.LockForAbsorption();
        
            // CHANGE SCALE
            var combinedScale = _seedController.Size + seedToAbsorb.Size / 3;
            _seedController.SetScale(combinedScale);
        
            // CHANGE COLOR
            var combinedColor = CombineColors(_seedController.SeedColor, seedToAbsorb.SeedColor);
            _seedController.SetColor(combinedColor);

            seedToAbsorb.SeedCombined();
            AbsorbedSeed?.Invoke();
            return;
        }
    }

    private Color CombineColors(params Color[] aColors)
    {
        Color result = new Color(0,0,0,0);
        
        foreach(Color c in aColors)
        {
            result += c;
        }
        
        result /= aColors.Length;
        return result;
    }
}
