using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
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

    public void PlantSeed(Vector3 position)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(PlantSeed)}: Planting {nameof(Seed)} at position={position}, rotation={Quaternion.identity}");
        _spatialAnchorCore.InstantiateSpatialAnchor(Seeds[0].PlantPrefab, position, Quaternion.identity);
    }
}
