using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class IslandController : MonoBehaviour
{
    [SerializeField] private Islands.IslandType _type;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private float _maxYThreshold;
    [SerializeField] private int _maxAttempts;

    public Islands.IslandType Type => _type;

    private Mesh SharedMesh => _meshFilter != null ? _meshFilter.sharedMesh : null;

    void Awake()
    {
        if (SharedMesh != null)
        {
            GetComponent<MeshCollider>().sharedMesh = SharedMesh;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SharedMesh == null) return;

        Gizmos.color = Color.green;
        DrawSpawnArea();

        Gizmos.color = Color.red;
        DrawHiddenArea();
    }

    private void DrawSpawnArea()
    {
        Bounds bounds = SharedMesh.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // Adjust the center Y position based on _spawnAreaYThreshold.
        center.y = _maxYThreshold + size.y / 2f;

        Gizmos.DrawWireCube(center, size);
    }

    private void DrawHiddenArea()
    {
        Bounds bounds = SharedMesh.bounds;
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // Adjust the center Y position based on the GameObject origin.
        center.y -= size.y / 2f;

        Gizmos.DrawWireCube(center, size);
    }
#endif
}
