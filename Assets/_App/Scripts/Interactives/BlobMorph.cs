using System;
using UnityEngine;

/// <summary>
/// Objects of this class will morph into other Blob objects
/// </summary>
[RequireComponent(typeof(BlobController))]
public class BlobMorph : MonoBehaviour
{
    [SerializeField] private BlobCombineBehaviorEnum blobCombineBehavior;
    [SerializeField] private BlobSizeComparisonBehaviorEnum blobSizeComparisonBehavior;

    private BlobController _blobController;
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
        _blobController = GetComponent<BlobController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactingBlob = other.GetComponent<BlobController>();
        if (interactingBlob) 
            CombineBlobs(interactingBlob);
    }

    private BlobSizeComparisonEnum CompareSize(BlobController blob)
    {
        if (_blobController.Size.magnitude > blob.Size.magnitude)
            return BlobSizeComparisonEnum.Larger;

        if (Math.Abs(_blobController.Size.magnitude - blob.Size.magnitude) < .001F)
            return BlobSizeComparisonEnum.SameSize;

        return BlobSizeComparisonEnum.Smaller;
    }

    private void CombineBlobs(BlobController interactingBlob)
    {
        var sizeOfInteractingBlob = CompareSize(interactingBlob);
        
        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.Smaller)
        {
            if (blobSizeComparisonBehavior == BlobSizeComparisonBehaviorEnum.OnlyCombineWithBlobsOfSameSize)
            {
                // Have the interacting blob 'live' inside our blob
                interactingBlob.transform.position = transform.position;
                interactingBlob.transform.SetParent(transform);
            }

            if (blobSizeComparisonBehavior == BlobSizeComparisonBehaviorEnum.OnlyCombineWithBlobsOfSameSizeOrSmaller)
            {
                AbsorbBlob(interactingBlob);
            }
        }

        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.SameSize)
        {
            AbsorbBlob(interactingBlob);
        }

        if (sizeOfInteractingBlob == BlobSizeComparisonEnum.Larger)
        {
            if (blobCombineBehavior == BlobCombineBehaviorEnum.FitSmallerBlobsInsideThisBlob)
            {
                // Have our blob 'live' inside the interacting blob
                transform.position = interactingBlob.transform.position;
                transform.SetParent(interactingBlob.transform);
            }
        }
    }

    private void AbsorbBlob(BlobController blobToAbsorb)
    {
        while (_blobController.IsAboutToBeAbsorbed == false)
        {
            Debug.Log($"{name} is absorbing {blobToAbsorb.name}");
            
            blobToAbsorb.LockForAbsorption();
        
            // CHANGE SCALE
            // TEJAS: We may want to use lossy scale in the future and then convert to local scales
            // Using local scales should be fine for now
            var combinedScale = _blobController.Size + blobToAbsorb.Size;
            _blobController.ChangeScale(combinedScale);
        
            // CHANGE COLOR
            var combinedColor = CombineColors(_blobController.MaterialColor, blobToAbsorb.MaterialColor);
            _blobController.ChangeColor(combinedColor);
        
            Destroy(blobToAbsorb.gameObject);
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
