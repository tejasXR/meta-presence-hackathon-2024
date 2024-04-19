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

        foreach (KeyValuePair<Guid, PlantData> plant in _garden.Map)
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
            _spatialAnchorCore.LoadAndInstantiateAnchors(plants.GetPrefab(plantsByType.Key), plantsByType.Value);
        }
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();
        
        _garden.Map.Clear();
        _garden.DateTimeOfLastVisit = "";
        GardenDataManager.SaveGarden(_garden);
    }

    public void LoadGardenState()
    {
        _garden = GardenDataManager.LoadGarden() ?? new();

        var timePassedSinceLastVisit = _garden.GetTimeSinceLastVisit();

        if (timePassedSinceLastVisit == null)
            return;

        var timePassedValue = timePassedSinceLastVisit.Value;
        
        var timeLog = "Time since garden was last visited is ";
        timeLog += timePassedValue.Days > 0 ? $"{timePassedValue.Days} days" : "";
        timeLog += timePassedValue.Hours > 0 ? $"{timePassedValue.Hours} hours" : "";
        timeLog += timePassedValue.Minutes > 0 ? $"{timePassedValue.Minutes} minutes" : "";
        timeLog += $"{timePassedValue.Seconds} seconds";
            
        Debug.Log(timeLog);
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

        _garden.DateTimeOfLastVisit = DateTime.Now.ToString("s");
        GardenDataManager.SaveGarden(_garden);
    }

    public void RestoreGardenState(List<Guid> _)
    {
        PlantController[] plants = FindObjectsOfType<PlantController>();
        foreach (PlantController plantController in plants)
        {
            if (plantController.TryGetComponent(out OVRSpatialAnchor anchor) && _garden.Map.TryGetValue(anchor.Uuid, out PlantData plantData))
            {
                plantController.ResumeGrowing(plantData.GrowValue);
                plantController.SetCreationDate(plantData.CreatedAt);
                plantController.GrowBasedOnPassedTime(DateTime.Now);
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
            GardenDataManager.SaveGarden(_garden);
        }
    }

    public void RemoveAllPlantsFromLocalStorage(OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        _garden.Map.Clear();
        GardenDataManager.SaveGarden(_garden);
    }
}
