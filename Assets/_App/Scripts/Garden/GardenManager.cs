using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private SeedSpawner _seedSpawner;
    [SerializeField] private Plants _plants;

    void OnApplicationQuit() => _persistenceManager.SaveGardenState();

    public void Initialize() => _persistenceManager.InitGarden();

    public void DestroyGarden() => _persistenceManager.DestroyGarden();

    public Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, Enum.GetValues(typeof(Plants.PlantType)).Length);
    }

    public bool TryGetPlantPrefab(Plants.PlantType plant, out GameObject prefab) => _plants.TryGetPrefab(plant, out prefab);

    public void OnSeedPopped(SeedController seed)
    {
        Debug.Log($"[{nameof(GardenManager)}] {nameof(OnSeedPopped)}: {nameof(seed)}={seed.gameObject.name}, {nameof(seed.Plant)}={seed.Plant}, {nameof(seed.PlantTargetPosition)}={seed.PlantTargetPosition}");

        if (_plants.TryGetPrefab(seed.Plant, out GameObject prefab))
        {
            _persistenceManager.CreateNewPlant(prefab, seed.PlantTargetPosition, seed.PlantTargetRotation);
        }
    }

    public void OnNewPlantCreated(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            return;
        }

        if (anchor.TryGetComponent(out PlantController plantController))
        {
            plantController.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
            plantController.StartGrowing();
        }
    }

    public void OnPlantsRestored(List<OVRSpatialAnchor> anchors)
    {
        foreach (OVRSpatialAnchor anchor in anchors)
        {
            if (anchor.TryGetComponent(out PlantController plantController) && _persistenceManager.TryGetPlantData(anchor.Uuid, out PlantData plantData))
            {
                plantController.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
                plantController.ResumeGrowing(plantData.Growth, _persistenceManager.TimeSinceLastGardenVisit);
            }
        }
    }

    private void OnPlantFullyGrown(PlantController plant)
    {
        _seedSpawner.SpawnFullyGrownPlantSeeds(plant.MinLoot, plant.LootSpawnPointsRoot);
    }
}
