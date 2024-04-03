using System;
using UnityEngine;

public class Blob : MonoBehaviour
{
    public Color MaterialColor
    {
        get
        {
            // TEJAS: Dupe code from Awake, okay for now
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (!_meshRenderer)
            {
                throw new ApplicationException($"No MeshRenderer component found on {name}");
            }
            
            return _meshRenderer.material.color;
        }
        private set => MaterialColor = value;
    }

    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (!_meshRenderer)
        {
            throw new ApplicationException($"No MeshRenderer component found on {name}");
        }

        MaterialColor = _meshRenderer.material.color;
    }

    public void ChangeScale(Vector3 newScale)
    {
        transform.localScale = newScale;
    }

    public void ChangeColor(Color newColor)
    {
        // TEJAS: Keep in mind changing the color will no longer make the material be GPU Instanced
        _meshRenderer.material.color = newColor;
    }
}
