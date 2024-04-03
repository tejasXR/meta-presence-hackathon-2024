using UnityEngine;

/// <summary>
/// Objects of this class will morph into other Blob objects
/// </summary>
[RequireComponent(typeof(Blob))]
public class BlobMorph : MonoBehaviour
{
    private const float ScaleTransitionSpeed = 4F;
    private const float ColorTransitionSpeed = 1F;
    
    private Blob _blob;
    private Vector3 _destinationScale;
    private Color _destinationColor;
    private Material _material;
    private float _colorTransitionTime;

    private void Awake()
    {
        _destinationScale = transform.localScale;
        _blob = GetComponent<Blob>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var potentialBlob = other.GetComponent<Blob>();
        if (potentialBlob)
        {
            if (IsBlobSmaller(potentialBlob))
            {
                AbsorbBlobScale(potentialBlob);
                AbsorbBlobColor(potentialBlob);
                
                Destroy(potentialBlob.gameObject);
            }
        }
    }

    private void Update()
    {
        var lerpedScale = Vector3.Lerp(transform.localScale, _destinationScale, ScaleTransitionSpeed);
        _blob.ChangeScale(lerpedScale);

        if (_colorTransitionTime < 1)
        {
            _blob.ChangeColor(Color.Lerp(_blob.MaterialColor, _destinationColor, _colorTransitionTime));
            _colorTransitionTime += Time.deltaTime / ColorTransitionSpeed;
        }
    }
    
    private bool IsBlobSmaller(Blob blob)
    {
        return transform.lossyScale.magnitude > blob.transform.lossyScale.magnitude;
    }

    private void AbsorbBlobScale(Blob blobToAbsorb)
    {
        // TEJAS: We may want to use lossy scale in the future and then convert to local scales
        // Using local scales should be fine for now
       _destinationScale += blobToAbsorb.transform.localScale;
    }

    private void AbsorbBlobColor(Blob blobToAbsorb)
    {
        _destinationColor = CombineColors(_blob.MaterialColor, blobToAbsorb.MaterialColor);
        _colorTransitionTime = 0;
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
