using System;
using UnityEngine;

public class BlobController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Following,
        Planting
    }

    public Blob Blob;

    private State _currentState = State.Idle;

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
        private set => value = MaterialColor;
    }

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        SetIdle();

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

    public void WavedAt()
    {
        switch (_currentState)
        {
            case State.Idle:
                TryToSetFollowing(true);
                TryToSetPlanting(false);
                break;

            case State.Following:
                TryToSetFollowing(false);
                TryToSetPlanting(true);
                break;

            case State.Planting:
                break;
        }
    }

    private void SetIdle()
    {
        _currentState = State.Idle;
        TryToSetFollowing(false);
        TryToSetPlanting(false);
    }

    private void TryToSetFollowing(bool isFollowing)
    {
        if (TryGetComponent(out FollowGazeBehaviour followGazeBehaviour))
        {
            followGazeBehaviour.enabled = isFollowing;
        }
        if (isFollowing)
        {
            _currentState = State.Following;
        }
    }

    private void TryToSetPlanting(bool isPlanting)
    {
        if (TryGetComponent(out PlantBehaviour plantBehaviour))
        {
            plantBehaviour.enabled = isPlanting;
        }
        if (isPlanting)
        {
            _currentState = State.Planting;
        }
    }
}
