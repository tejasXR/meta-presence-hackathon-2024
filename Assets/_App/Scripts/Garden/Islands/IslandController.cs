using System;
using UnityEngine;

public class IslandController : MonoBehaviour
{
    [SerializeField] private Islands.IslandType _type;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private float _maxYThreshold;
    [SerializeField] private int _maxAttempts;

    public Islands.IslandType Type => _type;

    private Action<Vector3, Vector3> _onValidSpawnPointFound;

    private Vector3 _referencePosition;
    private Vector3 _islandSurfacePosition;
    private Vector3 _islandSurfaceNormal;

    public void GetValidSpawnPoint(Vector3 referencePosition, Action<Vector3, Vector3> onValidSpawnPointFound)
    {
        _referencePosition = referencePosition;
        _onValidSpawnPointFound = onValidSpawnPointFound;
    }

    void Update()
    {
        if (_onValidSpawnPointFound == null)
        {
            return;
        }
        FindValidSpawnPoint();
    }

    private void FindValidSpawnPoint()
    {
        _referencePosition.y = _meshRenderer.bounds.center.y - _meshRenderer.bounds.extents.y;
        if (Physics.Raycast(_referencePosition, Vector3.up, out RaycastHit hit, Mathf.Infinity))
        {
            _islandSurfacePosition = hit.point;
            _islandSurfaceNormal = hit.normal;

            _onValidSpawnPointFound?.Invoke(_islandSurfacePosition, _islandSurfaceNormal);
        }
        else Debug.LogError($"[{nameof(IslandController)}] {nameof(FindValidSpawnPoint)}: FAILED TO FIND ISLAND COLLISION POINT");

        _onValidSpawnPointFound = null;
    }

    // #if UNITY_EDITOR
    //     private void OnDrawGizmos()
    //     {
    //         if (_meshRenderer == null) return;

    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawSphere(_islandSurfacePosition, 0.01f);

    //         Gizmos.color = Color.red;
    //         Gizmos.DrawSphere(_referencePosition, 0.01f);

    //         Gizmos.color = Color.green;
    //         Gizmos.DrawRay(_islandSurfacePosition, _islandSurfaceNormal);

    //         Gizmos.color = Color.green;
    //         DrawSpawnArea();

    //         Gizmos.color = Color.red;
    //         DrawHiddenArea();
    //     }

    //     private void DrawSpawnArea()
    //     {
    //         Bounds bounds = _meshRenderer.bounds;
    //         Vector3 center = bounds.center;
    //         Vector3 size = bounds.size;

    //         size.y = size.y / 2f - _maxYThreshold;
    //         center.y += size.y / 2f + _maxYThreshold;

    //         Gizmos.DrawWireCube(center, size);
    //     }

    //     private void DrawHiddenArea()
    //     {
    //         Bounds bounds = _meshRenderer.bounds;
    //         Vector3 center = bounds.center;
    //         Vector3 size = bounds.size;

    //         size.y /= 2f;
    //         center.y -= size.y / 2f;

    //         Gizmos.DrawWireCube(center, size);
    //     }
    // #endif
}
