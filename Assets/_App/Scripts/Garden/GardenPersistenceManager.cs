using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;

[RequireComponent(typeof(SpatialAnchorCoreBuildingBlock))]
public class GardenPersistenceManager : MonoBehaviour
{
    [SerializeField] private Plants _plants;

    private GardenData _garden;

    private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

    void Awake()
    {
        _garden = SaveDataManager.LoadGarden() ?? new();
        _spatialAnchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();
    }

    public void CreateNewPlant(GameObject plantPrefab, Tuple<Vector3, Quaternion> plantPosition)
    {
        _spatialAnchorCore.InstantiateSpatialAnchor(plantPrefab, plantPosition.Item1, plantPosition.Item2);
    }

    public void InitGarden()
    {
        Dictionary<Plants.PlantType, List<Guid>> plantTypeMap = new();

        foreach (KeyValuePair<Guid, GardenData.PlantData> plant in _garden.Map)
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
            _spatialAnchorCore.LoadAndInstantiateAnchors(_plants.GetPrefab(plantsByType.Key), plantsByType.Value);
        }
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();
    }

    public void SaveGardenState()
    {
        PlantController[] plants = FindObjectsOfType<PlantController>();
        foreach (PlantController plantController in plants)
        {
            if (plantController.TryGetComponent(out OVRSpatialAnchor anchor))
            {
                _garden.Map[anchor.Uuid] = new() { Uuid = anchor.Uuid, Type = plantController.Type.ToString(), GrowValue = plantController.GrowValue };
            }
        }

        _garden.TimeSinceLastVisit = DateTime.Now;
        SaveDataManager.SaveGarden(_garden);
    }

    public void RestoreGardenState(List<Guid> _)
    {
        PlantController[] plants = FindObjectsOfType<PlantController>();
        foreach (PlantController plantController in plants)
        {
            if (plantController.TryGetComponent(out OVRSpatialAnchor anchor) && _garden.Map.TryGetValue(anchor.Uuid, out GardenData.PlantData plantData))
            {
                plantController.ResumeGrowing(plantData.GrowValue);
            }
        }
    }

    public void SaveAndStartGrowingPlant(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (anchor.TryGetComponent(out PlantController plantController))
        {
            _garden.Map[anchor.Uuid] = new() { Uuid = anchor.Uuid, Type = plantController.Type.ToString(), CreatedAt = DateTime.Now };

            plantController.StartGrowing();
        }
    }

    public void RemovePlantFromLocalStorage(Guid uuid, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (_garden.Map.Remove(uuid))
        {
            SaveDataManager.SaveGarden(_garden);
        }
    }

    public void RemoveAllPlantsFromLocalStorage(OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        _garden.Map.Clear();
        SaveDataManager.SaveGarden(_garden);
    }
}
