using System;
using UnityEngine;

/// <summary>
/// Objects of this class will morph into other Blob objects
/// </summary>
[RequireComponent(typeof(BlobController))]
public class BlobMorph : MonoBehaviour
{
    private BlobController _blobController;
    
    private Material _material;
    private float _colorTransitionTime;

    private enum BlobSizeComparisonEnum
    {
        Smaller,
        SameSize,
        Larger
    }

    private void Awake()
    {
        _blobController = GetComponent<BlobController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactingBlob = other.GetComponent<BlobController>();
        if (interactingBlob) 
            MixBlobs(interactingBlob);
    }

    private BlobSizeComparisonEnum CompareSize(BlobController blob)
    {
        if (_blobController.Size.magnitude > blob.Size.magnitude)
            return BlobSizeComparisonEnum.Larger;

        if (Math.Abs(_blobController.Size.magnitude - blob.Size.magnitude) < .001F)
            return BlobSizeComparisonEnum.SameSize;

        return BlobSizeComparisonEnum.Smaller;
    }

    private void MixBlobs(BlobController interactingBlob)
    {
        var sizeOfInteractingBlob = CompareSize(interactingBlob);
        switch (sizeOfInteractingBlob)
        {
            case BlobSizeComparisonEnum.Smaller:
                interactingBlob.transform.position = transform.position;
                interactingBlob.transform.SetParent(transform);
                break;
            case BlobSizeComparisonEnum.SameSize:
                AbsorbBlobScale(interactingBlob);
                AbsorbBlobColor(interactingBlob);
                Destroy(interactingBlob.gameObject);
                break;
            case BlobSizeComparisonEnum.Larger:
                transform.position = interactingBlob.transform.position;
                transform.SetParent(interactingBlob.transform);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AbsorbBlobScale(BlobController blobToAbsorb)
    {
        // TEJAS: We may want to use lossy scale in the future and then convert to local scales
        // Using local scales should be fine for now
        var combinedScale = _blobController.Size + blobToAbsorb.Size;
        _blobController.ChangeScale(combinedScale);
    }

    private void AbsorbBlobColor(BlobController blobToAbsorb)
    {
        var combinedColor = CombineColors(_blobController.MaterialColor, blobToAbsorb.MaterialColor);
        _blobController.ChangeColor(combinedColor);
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
