using Meta.XR.MRUtilityKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Planting
{
    public SeedController Seed;
    public GameObject PlantPrefab;
    public Vector3 PlantSpawnPosition;
    public Quaternion PlantSpawnRotation;

    public readonly bool IsValid => PlantPrefab != null && PlantSpawnPosition.IsValid();
    public static Planting Invalid => new() { PlantSpawnPosition = Vector3.negativeInfinity };

    public override readonly string ToString() => $"{nameof(Planting)}[ {nameof(Seed.Uuid)}={Seed.Uuid}, {nameof(PlantPrefab)}={PlantPrefab.name}, {nameof(PlantSpawnPosition)}={PlantSpawnPosition}, {nameof(PlantSpawnRotation)}={PlantSpawnRotation} ]";
}

public class GardenManager : MonoBehaviour
{
    [SerializeField] private GardenPersistenceManager _persistenceManager;
    [SerializeField] private SeedSpawner _seedSpawner;
    [SerializeField] private Islands _islands;
    [SerializeField] private Plants _plants;

    private readonly Dictionary<Guid, Planting> _plantingMap = new();

    void OnApplicationQuit() => _persistenceManager.SaveGardenState();

    public void Initialize() => _persistenceManager.InitGarden();

    public void DestroyGarden() => _persistenceManager.DestroyGarden();

    public bool TryAndCreateNewPlanting(SeedController seed, out Vector3 seedTargetDestination)
    {
        seedTargetDestination = Vector3.negativeInfinity;
        Planting planting = Planting.Invalid;

        if (_plants.TryGetPrefab(GetPlantFrom(seed), out GameObject plantPrefab))
        {
            Tuple<Vector3, Quaternion> validPlantSpawnPoint = GetValidPlantSpawnPoint(plantPrefab);
            seedTargetDestination = validPlantSpawnPoint.Item1;

            planting = new()
            {
                Seed = seed,
                PlantPrefab = plantPrefab,
                PlantSpawnPosition = validPlantSpawnPoint.Item1,
                PlantSpawnRotation = validPlantSpawnPoint.Item2,
            };
            _plantingMap[seed.Uuid] = planting;

            Debug.Log($"[{nameof(GardenManager)}] {nameof(TryAndCreateNewPlanting)} New {nameof(Planting)} created: {planting}");
        }
        return planting.IsValid;
    }

    public void OnSeedPopped(SeedController seed)
    {
        if (_plantingMap.TryGetValue(seed.Uuid, out Planting planting))
        {
            if (planting.IsValid)
            {
                // TODO(yola): Check if island already exists at planting position.
                Quaternion randomYAxisRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                Islands.IslandType randomIslandType = (Islands.IslandType)Random.Range(1, Enum.GetValues(typeof(Islands.IslandType)).Length);
                if (_islands.TryGetPrefab(randomIslandType, out GameObject islandPrefab))
                {
                    _persistenceManager.CreateNewIsland(islandPrefab, planting.PlantSpawnPosition, randomYAxisRotation * planting.PlantSpawnRotation);
                }
            }
        }
    }

    public void OnNewIslandCreated(IslandData _, IslandController controller)
    {
        // _availablePlantSpawnPoints.AddRange(controller.PlantSpawnPoints);

        // // TODO(yola): Seed > Plant correlation
        // if (_plants.TryGetPrefab(GetPlantFrom(null), out GameObject plantPrefab))
        // {
        //     Transform plantSpawnPoint = controller.GetAvailableSpawnPoint();
        //     _persistenceManager.CreateNewPlant(plantPrefab, plantSpawnPoint.position, plantSpawnPoint.rotation);
        // }
    }

    public void OnIslandLoaded(IslandData _, IslandController controller)
    {
        // _availablePlantSpawnPoints.AddRange(controller.PlantSpawnPoints);
    }

    public void OnNewPlantCreated(PlantData _, PlantController controller)
    {
        controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        controller.StartGrowing();
    }

    public void OnPlantLoaded(PlantData data, PlantController controller)
    {
        controller.SeedSpawningTriggered.AddListener(OnPlantFullyGrown);
        controller.ResumeGrowing(data.Growth, _persistenceManager.TimeSinceLastGardenVisit);
    }

    private void OnPlantFullyGrown(PlantController plant)
    {
        _seedSpawner.SpawnFullyGrownPlantSeeds(plant.MinLoot, plant.LootSpawnPointsRoot);
    }

    private Tuple<Vector3, Quaternion> GetValidPlantSpawnPoint(GameObject prefab)
    {
        Tuple<Vector3, Quaternion>[] validPositions = SpawnUtil.GetSpawnPositions(
            objectBounds: Utilities.GetPrefabBounds(prefab),
            positionCount: 1,
            spawnLocation: FindSpawnPositions.SpawnLocation.HangingDown,
            labels: MRUKAnchor.SceneLabels.CEILING);

        Debug.Assert(validPositions.Length > 0, $"[{nameof(GardenDataManager)}] {nameof(GetValidPlantSpawnPoint)} error: invalid {nameof(validPositions)} array.");
        return validPositions[0];
    }

    private Plants.PlantType GetPlantFrom(SeedController seed)
    {
        // TODO(yola): Seed > Plant correlation
        return (Plants.PlantType)Random.Range(1, Enum.GetValues(typeof(Plants.PlantType)).Length);
    }
}
