using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private List<Seed> Seeds;

    [SerializeField] private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;
    [SerializeField] private SpatialAnchorLoaderBuildingBlock _spatialAnchorLoader;

    public void InitGarden()
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(InitGarden)}");

        _spatialAnchorLoader.LoadAnchorsFromDefaultLocalStorage();
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();
    }

    public void PlantSeed()
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(PlantSeed)}");

        int seedCount = 1;

        Tuple<Vector3, Quaternion>[] spawnPositions = SpawnUtil.GetSpawnPositions(
                objectBounds: Utilities.GetPrefabBounds(Seeds[0].PlantPrefab),
                positionCount: seedCount,
                spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
                labels: MRUKAnchor.SceneLabels.CEILING);

        foreach (Tuple<Vector3, Quaternion> spawnPosition in spawnPositions)
        {
            Debug.Log($"[{nameof(GardenManager)}] {nameof(PlantSeed)}: Planting new seed at position={spawnPosition.Item1}, rotation={spawnPosition.Item2}");
            _spatialAnchorCore.InstantiateSpatialAnchor(Seeds[0].PlantPrefab, spawnPosition.Item1, spawnPosition.Item2);
        }
    }
}
