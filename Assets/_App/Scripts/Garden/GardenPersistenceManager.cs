using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpatialAnchorCoreBuildingBlock))]
public class GardenPersistenceManager : MonoBehaviour
{
    [SerializeField] private Plants plants;

    [Button("Save Garden State")]
    public void SaveGardenStateButton()
    {
        SaveGardenState();
    }

    [Button("Load Garden State")]
    public void LoadGardenStateButton()
    {
        LoadGardenState();
    }

    [Button("Destroy Garden")]
    public void DestroyGardenButton()
    {
        DestroyGarden();
    }

    private GardenData _garden;
    public GardenData Garden
    {
        get
        {
            if (_garden == null)
            {
                LoadGardenState();
            }
            return _garden;
        }
    }

    private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

    void Awake()
    {
        LoadGardenState();
        _spatialAnchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();
    }

    public void CreateNewPlant(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        _spatialAnchorCore.InstantiateSpatialAnchor(prefab, position, rotation);
    }

    public void InitGarden()
    {
        Dictionary<Plants.PlantType, List<Guid>> plantTypeMap = new();

        foreach (KeyValuePair<Guid, PlantData> plant in Garden.Map)
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
            if (plants.TryGetPrefab(plantsByType.Key, out GameObject prefab))
            {
                _spatialAnchorCore.LoadAndInstantiateAnchors(prefab, plantsByType.Value);
            }
        }
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();

        Garden.Map.Clear();
        Garden.DateTimeOfLastVisit = "";
        GardenDataManager.SaveGarden(Garden);
    }

    public void LoadGardenState()
    {
        _garden = GardenDataManager.LoadGarden() ?? new();
        Debug.Log($"[{nameof(GardenPersistenceManager)}] {nameof(LoadGardenState)}: {Garden}");
    }

    public void SaveGardenState()
    {
        PlantController[] plants = FindObjectsOfType<PlantController>();
        foreach (PlantController plantController in plants)
        {
            if (plantController.TryGetComponent(out OVRSpatialAnchor anchor))
            {
                Garden.Map[anchor.Uuid] = new()
                {
                    Uuid = anchor.Uuid,
                    Type = plantController.Type.ToString(),
                    Growth = plantController.Growth
                };
            }
        }

        Garden.DateTimeOfLastVisit = DateTime.Now.ToString("s");
        GardenDataManager.SaveGarden(Garden);
    }

    public void SavePlant(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (anchor.TryGetComponent(out PlantController plantController))
        {
            Garden.Map[anchor.Uuid] = new() { Uuid = anchor.Uuid, Type = plantController.Type.ToString() };
        }
    }

    public void DeletePlant(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (Garden.Map.Remove(anchor.Uuid))
        {
            GardenDataManager.SaveGarden(Garden);
        }
    }

    public void DeleteAllPlants(OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        Garden.Map.Clear();
        GardenDataManager.SaveGarden(Garden);
    }

    public bool TryGetPlantData(Guid plantId, out PlantData plantData) => Garden.Map.TryGetValue(plantId, out plantData);

    public TimeSpan? TimeSinceLastGardenVisit => Garden.GetTimeSinceLastVisit();
}
