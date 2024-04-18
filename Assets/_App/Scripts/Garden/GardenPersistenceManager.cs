using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpatialAnchorCoreBuildingBlock))]
public class GardenPersistenceManager : MonoBehaviour
{
    [SerializeField] private PlantData plantData;

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
        Dictionary<PlantData.PlantType, List<Guid>> plantTypeMap = new();

        foreach (KeyValuePair<Guid, GardenData.PlantData> plant in _garden.Map)
        {
            if (Enum.TryParse(plant.Value.Type, out PlantData.PlantType plantType))
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

        foreach (KeyValuePair<PlantData.PlantType, List<Guid>> plantsByType in plantTypeMap)
        {
            _spatialAnchorCore.LoadAndInstantiateAnchors(plantData.GetPrefab(plantsByType.Key), plantsByType.Value);
        }
    }

    public void DestroyGarden()
    {
        _spatialAnchorCore.EraseAllAnchors();
    }

    public void LoadGardenState()
    {
        _garden = GardenDataManager.LoadGarden() ?? new();
        
        if (_garden != default)
        {
            var timePassedSinceLastVisit = _garden.TimeSinceLastVisit;
            
            var timeLog = "Time since garden was last visited is ";
            timeLog += timePassedSinceLastVisit.Days > 0 ? $"{timePassedSinceLastVisit.Days} days" : "";
            timeLog += timePassedSinceLastVisit.Hours > 0 ? $"{timePassedSinceLastVisit.Hours} hours" : "";
            timeLog += timePassedSinceLastVisit.Minutes > 0 ? $"{timePassedSinceLastVisit.Minutes} minutes" : "";
            timeLog += $"{timePassedSinceLastVisit.Seconds} seconds";
            
            Debug.Log(timeLog);
        }
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
