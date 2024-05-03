using System;
using UnityEngine;

/// <summary>
/// Objects of this class will morph into other Blob objects
/// </summary>
[RequireComponent(typeof(SeedController))]
public class SeedMorph : MonoBehaviour
{
    [SerializeField] private BlobCombineBehaviorEnum blobCombineBehavior;
    [SerializeField] private BlobSizeComparisonBehaviorEnum blobSizeComparisonBehavior;

    private SeedController _seedController;
    private Material _material;
    private float _colorTransitionTime;

    private enum BlobSizeComparisonEnum
    {
        Smaller,
        SameSize,
        Larger
    }

    private enum BlobSizeComparisonBehaviorEnum
    {
        OnlyCombineWithBlobsOfSameSize,
        OnlyCombineWithBlobsOfSameSizeOrSmaller
    }

    private enum BlobCombineBehaviorEnum
    {
        DirectlyCombineSizeAndColor,
        FitSmallerBlobsInsideThisBlob
    }

    private void Awake()
    {
        _seedController = GetComponent<SeedController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactingBlob = other.GetComponent<SeedController>();
        if (interactingBlob) 
            CombineBlobs(interactingBlob);
    }

    private BlobSizeComparisonEnum CompareSize(SeedController seed)
    {
        if (_seedController.Size.magnitude > seed.Size.magnitude)
            return BlobSizeComparisonEnum.Larger;

        if (Math.Abs(_seedController.Size.magnitude - seed.Size.magnitude) < .001F)
            return BlobSizeComparisonEnum.SameSize;

        return BlobSizeComparisonEnum.Smaller;
    }

    private void CombineBlobs(SeedController interactingSeed)
    {
        var sizeOfInteractingBlob = CompareSize(interactingSeed);
        
        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.Smaller)
        {
            if (blobSizeComparisonBehavior == BlobSizeComparisonBehaviorEnum.OnlyCombineWithBlobsOfSameSize)
            {
                // Have the interacting blob 'live' inside our blob
                interactingSeed.transform.position = transform.position;
                interactingSeed.transform.SetParent(transform);
            }

            if (blobSizeComparisonBehavior == BlobSizeComparisonBehaviorEnum.OnlyCombineWithBlobsOfSameSizeOrSmaller)
            {
                AbsorbBlob(interactingSeed);
            }
        }

        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.SameSize)
        {
            AbsorbBlob(interactingSeed);
        }

        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.Larger)
        {
            if (blobCombineBehavior == BlobCombineBehaviorEnum.FitSmallerBlobsInsideThisBlob)
            {
                // Have our blob 'live' inside the interacting blob
                transform.position = interactingSeed.transform.position;
                transform.SetParent(interactingSeed.transform);
            }
        }
    }

    private void AbsorbBlob(SeedController seedToAbsorb)
    {
        while (_seedController.IsAboutToBeAbsorbed == false)
        {
            Debug.Log($"{name} is absorbing {seedToAbsorb.name}");
            
            seedToAbsorb.LockForAbsorption();
        
            // CHANGE SCALE
            // TEJAS: We may want to use lossy scale in the future and then convert to local scales
            // Using local scales should be fine for now
            var combinedScale = _seedController.Size + seedToAbsorb.Size;
            _seedController.SetScale(combinedScale);
        
            // CHANGE COLOR
            var combinedColor = CombineColors(_seedController.SeedColor, seedToAbsorb.SeedColor);
            _seedController.SetColor(combinedColor);
        
            Destroy(seedToAbsorb.gameObject);
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
