using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public static class SpawnUtil
{
    /// <summary>
    /// Copy of Meta's <see cref="FindSpawnPositions.StartSpawn"/> where positions get returned instead of the objects getting instantiated or moved.
    /// 
    /// Allows for fast generation of valid (inside the room, outside furniture bounds) random positions for content spawning.
    /// Optional method to pin directly to surfaces.
    /// </summary>
    /// <param name="objectBounds">Bounds of the prefab to be placed into the scene.</param>
    /// <param name="positionCount">Number of SpawnObject(s) to place into the scene, only applies to Prefabs.</param>
    /// <param name="spawnLocation">Attach content to scene surfaces.</param>
    /// <param name="labels">When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.</param>
    /// <param name="checkOverlaps">If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves."</param>
    /// <param name="overrideBounds">Required free space for the object (Set negative to auto-detect using GetPrefabBounds)</param>
    /// <param name="layerMask">Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.</param>
    /// <param name="surfaceClearanceDistance">The clearance distance required in front of the surface in order for it to be considered a valid spawn position.</param>
    /// <param name="maxIterations">Maximum number of times to attempt spawning/moving an object before giving up.</param>
    /// <returns></returns>
    public static Tuple<Vector3, Quaternion>[] GetSpawnPositions(
        Bounds? objectBounds,
        int positionCount = 8,
        FindSpawnPositions.SpawnLocation spawnLocation = FindSpawnPositions.SpawnLocation.Floating,
        MRUKAnchor.SceneLabels labels = ~(MRUKAnchor.SceneLabels)0,
        bool checkOverlaps = true,
        float overrideBounds = -1f,
        int layerMask = ~0,
        float surfaceClearanceDistance = 0.1f,
        int maxIterations = 1000)
    {
        Tuple<Vector3, Quaternion>[] spawnPositions = new Tuple<Vector3, Quaternion>[positionCount];

        var room = MRUK.Instance.GetCurrentRoom();
        float minRadius = 0.0f;
        const float clearanceDistance = 0.01f;
        float baseOffset = -objectBounds?.min.y ?? 0.0f;
        float centerOffset = objectBounds?.center.y ?? 0.0f;
        Bounds adjustedBounds = new();

        if (objectBounds.HasValue)
        {
            minRadius = Mathf.Min(-objectBounds.Value.min.x, -objectBounds.Value.min.z, objectBounds.Value.max.x, objectBounds.Value.max.z);
            if (minRadius < 0f)
            {
                minRadius = 0f;
            }
            var min = objectBounds.Value.min;
            var max = objectBounds.Value.max;
            min.y += clearanceDistance;
            if (max.y < min.y)
            {
                max.y = min.y;
            }
            adjustedBounds.SetMinMax(min, max);
            if (overrideBounds > 0)
            {
                Vector3 center = new Vector3(0f, clearanceDistance, 0f);
                Vector3 size = new Vector3(overrideBounds * 2f, clearanceDistance * 2f, overrideBounds * 2f); // OverrideBounds represents the extents, not the size
                adjustedBounds = new Bounds(center, size);
            }
        }

        for (int i = 0; i < positionCount; ++i)
        {
            for (int j = 0; j < maxIterations; ++j)
            {
                Vector3 spawnPosition = Vector3.zero;
                Vector3 spawnNormal = Vector3.zero;
                if (spawnLocation == FindSpawnPositions.SpawnLocation.Floating)
                {
                    var randomPos = room.GenerateRandomPositionInRoom(minRadius, true);
                    if (!randomPos.HasValue)
                    {
                        break;
                    }

                    spawnPosition = randomPos.Value;
                }
                else
                {
                    MRUK.SurfaceType surfaceType = 0;
                    switch (spawnLocation)
                    {
                        case FindSpawnPositions.SpawnLocation.AnySurface:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                        case FindSpawnPositions.SpawnLocation.VerticalSurfaces:
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            break;
                        case FindSpawnPositions.SpawnLocation.OnTopOfSurfaces:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            break;
                        case FindSpawnPositions.SpawnLocation.HangingDown:
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                    }
                    if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, LabelFilter.FromEnum(labels), out var pos, out var normal))
                    {
                        spawnPosition = pos + normal * baseOffset;
                        spawnNormal = normal;
                        var center = spawnPosition + normal * centerOffset;
                        // In some cases, surfaces may protrude through walls and end up outside the room
                        // check to make sure the center of the prefab will spawn inside the room
                        if (!room.IsPositionInRoom(center))
                        {
                            continue;
                        }
                        // Ensure the center of the prefab will not spawn inside a scene volume
                        if (room.IsPositionInSceneVolume(center))
                        {
                            continue;
                        }
                        // Also make sure there is nothing close to the surface that would obstruct it
                        if (room.Raycast(new Ray(pos, normal), surfaceClearanceDistance, out _))
                        {
                            continue;
                        }
                    }
                }

                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                if (checkOverlaps && objectBounds.HasValue)
                {
                    if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, adjustedBounds.extents, spawnRotation, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }
                }

                spawnPositions[i] = new(spawnPosition, spawnRotation);
                break;
            }
        }
        return spawnPositions;
    }
}
