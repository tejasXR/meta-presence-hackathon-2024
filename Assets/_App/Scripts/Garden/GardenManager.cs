using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> Plants;

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

    public void OnSeedPopped(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        GameObject plantPrefab = Plants[0];

        Plant(plantPrefab, GetValidPositionForPlanting(plantPrefab));
    }

    public void Plant(GameObject plantPrefab, Tuple<Vector3, Quaternion> plantPosition)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(Plant)}: Planting {plantPrefab.name} at position={plantPosition.Item1}, rotation={plantPosition.Item2}");

        _spatialAnchorCore.InstantiateSpatialAnchor(plantPrefab, plantPosition.Item1, plantPosition.Item2);
    }

    public Tuple<Vector3, Quaternion> GetValidPositionForPlanting(GameObject plantPrefab)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(GetValidPositionForPlanting)}: plant={plantPrefab.name}");

        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
                objectBounds: Utilities.GetPrefabBounds(plantPrefab),
                positionCount: 1,
                spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
                labels: MRUKAnchor.SceneLabels.CEILING);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenManager)}] {nameof(GetValidPositionForPlanting)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }
}
