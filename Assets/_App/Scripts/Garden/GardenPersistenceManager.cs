using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpatialAnchorCoreBuildingBlock))]
public class GardenPersistenceManager : MonoBehaviour
{
    [SerializeField] private Islands _islands;
    [SerializeField] private Plants _plants;

    public UnityEvent<IslandData, IslandController> OnNewIslandCreated;
    public UnityEvent<IslandData, IslandController> OnIslandLoaded;
    public UnityEvent<PlantData, PlantController> OnNewPlantCreated;
    public UnityEvent<PlantData, PlantController> OnPlantLoaded;

    private GardenData _garden;
    public GardenData Garden => _garden ??= GardenDataManager.LoadGarden() ?? new();

    private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

    void Awake()
    {
        _spatialAnchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();
    }

    public void CreateNewIsland(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        _spatialAnchorCore.InstantiateSpatialAnchor(prefab, position, rotation);
    }

    public void CreateNewPlant(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        _spatialAnchorCore.InstantiateSpatialAnchor(prefab, position, rotation);
    }

    public void InitGarden()
    {
        LoadIslands();
        LoadPlants();
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();
        GardenDataManager.SaveGarden(new());
    }

    public void SaveGardenState()
    {
        // Update plant's growth.
        PlantController[] plants = FindObjectsOfType<PlantController>();
        foreach (PlantController plantController in plants)
        {
            if (plantController.TryGetComponent(out OVRSpatialAnchor anchor))
            {
                Garden.PlantMap[anchor.Uuid] = new()
                {
                    Uuid = anchor.Uuid,
                    Type = plantController.Type.ToString(),
                    Growth = plantController.BasePlantGrowth
                };
            }
        }

        Garden.DateTimeOfLastVisit = DateTime.Now.ToString("s");
        GardenDataManager.SaveGarden(Garden);
    }

    public TimeSpan? TimeSinceLastGardenVisit => Garden.GetTimeSinceLastVisit();

    private void LoadIslands()
    {
        Dictionary<Islands.IslandType, List<Guid>> islandTypeMap = new();

        foreach (KeyValuePair<Guid, IslandData> island in Garden.IslandMap)
        {
            if (Enum.TryParse(island.Value.Type, out Islands.IslandType islandType))
            {
                if (islandTypeMap.TryGetValue(islandType, out List<Guid> islandsByType))
                {
                    islandsByType.Add(island.Key);
                }
                else
                {
                    islandsByType = new() { island.Key };
                }
                islandTypeMap[islandType] = islandsByType;
            }
        }

        foreach (KeyValuePair<Islands.IslandType, List<Guid>> islandsByType in islandTypeMap)
        {
            if (_islands.TryGetPrefab(islandsByType.Key, out GameObject prefab))
            {
                _spatialAnchorCore.LoadAndInstantiateAnchors(prefab, islandsByType.Value);
            }
        }
    }

    private void LoadPlants()
    {
        Dictionary<Plants.PlantType, List<Guid>> plantTypeMap = new();

        foreach (KeyValuePair<Guid, PlantData> plant in Garden.PlantMap)
        {
            if (Enum.TryParse(plant.Value.Type, out Plants.PlantType plantType))
            {
                if (plantTypeMap.TryGetValue(plantType, out List<Guid> plantsByType))
                {
                    plantsByType.Add(plant.Key);
                }
                else
                {
                    plantsByType = new() { plant.Key };
                }
                plantTypeMap[plantType] = plantsByType;
            }
        }

        foreach (KeyValuePair<Plants.PlantType, List<Guid>> plantsByType in plantTypeMap)
        {
            if (_plants.TryGetPrefab(plantsByType.Key, out GameObject prefab))
            {
                _spatialAnchorCore.LoadAndInstantiateAnchors(prefab, plantsByType.Value);
            }
        }
    }

    public void DestroyPlant(Guid uuid)
    {
        _spatialAnchorCore.EraseAnchorByUuid(uuid);
    }

    #region Spatial Anchor Core Building Block Events

    public void OnAchorCreateCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (anchor.TryGetComponent(out IslandController islandController))
        {
            IslandData data = new() { Uuid = anchor.Uuid, Type = islandController.Type.ToString() };

            Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAchorCreateCompleted)}: New Island Created: {data}");

            Garden.IslandMap[anchor.Uuid] = data;
            OnNewIslandCreated?.Invoke(data, islandController);
        }
        else if (anchor.TryGetComponent(out PlantController plantController))
        {
            PlantData data = new() { Uuid = anchor.Uuid, Type = plantController.Type.ToString() };

            Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAchorCreateCompleted)}: New Plant Created: {data}");

            Garden.PlantMap[anchor.Uuid] = data;
            OnNewPlantCreated?.Invoke(data, plantController);
        }
        else
        {
            Debug.LogWarning($"[{nameof(GardenPersistenceManager)}] {nameof(OnAchorCreateCompleted)}: New Unknown Anchor Created");
        }
    }

    public void OnAnchorsLoadCompleted(List<OVRSpatialAnchor> anchors)
    {
        foreach (OVRSpatialAnchor anchor in anchors)
        {
            if (anchor.TryGetComponent(out IslandController islandController))
            {
                if (Garden.IslandMap.TryGetValue(anchor.Uuid, out IslandData data))
                {
                    Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorsLoadCompleted)}: Island Loaded: {data}");
                    OnIslandLoaded?.Invoke(data, islandController);
                }
            }
            else if (anchor.TryGetComponent(out PlantController plantController))
            {
                if (Garden.PlantMap.TryGetValue(anchor.Uuid, out PlantData data))
                {
                    Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorsLoadCompleted)}: Plant Loaded: {data}");
                    OnPlantLoaded?.Invoke(data, plantController);
                }
            }
            else
            {
                Debug.LogWarning($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorsLoadCompleted)}: Unknown Anchor Loaded");
            }
        }
    }

    public void OnAnchorsEraseAllCompleted(OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        GardenDataManager.SaveGarden(new());
        Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorsEraseAllCompleted)}: Garden Destroyed");
    }

    public void OnAnchorEraseCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (Garden.IslandMap.Remove(anchor.Uuid))
        {
            GardenDataManager.SaveGarden(Garden);
            Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorEraseCompleted)}: Island Deleted: {nameof(anchor.Uuid)}={anchor.Uuid}");
        }
        else if (Garden.PlantMap.Remove(anchor.Uuid))
        {
            GardenDataManager.SaveGarden(Garden);
            Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(OnAnchorEraseCompleted)}: Plant Deleted: {nameof(anchor.Uuid)}={anchor.Uuid}");
        }
    }

    #endregion

#if UNITY_EDITOR

    [Sirenix.OdinInspector.Button("Save Garden State")]
    public void SaveGardenStateButton()
    {
        SaveGardenState();
    }

    [Sirenix.OdinInspector.Button("Destroy Garden")]
    public void DestroyGardenButton()
    {
        DestroyGarden();
    }

#endif
}
